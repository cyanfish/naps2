using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NAPS2.Scan.Images.Transforms;

namespace NAPS2.Scan.Images
{
    public class HoughLineDeskewer
    {
        // Conceptually, Hough Line deskewing works like this:
        //
        // 1. Convert the image to black and white.
        // 2. Create an array with an int for every possible line in the image
        // within 20 degrees of horizontal. Each line is represented by the distance
        // from the origin and the angle (in math terms, a parametric line).
        // 3. Find all black pixels with white pixels below (i.e. bottom edges).
        // 4. For each pixel, do some trigonometry to find every one of those lines
        // it intersects with, and increment the value in the array for each line.
        // 5. By looking for the largest array entries you find the lines that are
        // most aligned with the bottom edges of the letters/shapes in the image.
        // 6. Among the best lines, look for a cluster of similar angles.
        // 7. If it's a big enough cluster, return the average of those angles.
        // 8. Otherwise, a consistent skew angle couldn't be found, so return 0.

        private const double ANGLE_MIN = -20;
        private const double ANGLE_MAX = 20;
        private const int ANGLE_STEPS = 201; // 0.2 degree step size
        private const int BEST_MAX_COUNT = 100;
        private const int BEST_THRESHOLD_INDEX = 9;
        private const double BEST_THRESOLD_FACTOR = 0.5;
        private const double CLUSTER_TARGET_SPREAD = 2.01;
        private const double IGNORE_EDGE_FRACTION = 0.01;

        public double GetSkewAngle(Bitmap image)
        {
            var bitArrays = UnsafeImageOps.ConvertToBitArrays(image);

            int h = image.Height;
            int w = image.Width;
            // Ignore a bit of the top/bottom so that artifacts from the edge
            // of the scan area aren't counted. (Left/right don't matter since
            // we only look at near-horizontal lines.)
            int yOffset = (int)Math.Round(h * IGNORE_EDGE_FRACTION);

            var sinCos = PrecalculateSinCos();

            // TODO: Consider reducing the precision of distance or angle,
            // TODO: possibly with a second pass to restore precision.
            int dCount = 2 * (w + h);
            int[,] scores = new int[dCount, ANGLE_STEPS];

            // TODO: This should be a good candidate for OpenCL optimization.
            // TODO: If you parallelize over the angle, you're operating over
            // TODO: the same input data with the same branches.
            for (int y = 1 + yOffset; y <= h - 2 - yOffset; y++)
            {
                for (int x = 1; x <= w - 2; x++)
                {
                    if (bitArrays[y][x] && !bitArrays[y + 1][x])
                    {
                        for (int i = 0; i < ANGLE_STEPS; i++)
                        {
                            var sc = sinCos[i];
                            int d = (int)(y * sc.cos - x * sc.sin + w);
                            scores[d, i]++;
                        }
                    }
                }
            }

            var angles = GetAnglesOfBestLines(scores, dCount);
            var cluster = ClusterAngles(angles);
            if (cluster.Length < angles.Length / 2)
            {
                // Could not find a consistent skew angle
                return 0;
            }
            return cluster.Sum() / cluster.Length;
        }

        private double[] ClusterAngles(double[] angles)
        {
            angles = angles.OrderBy(x => x).ToArray();
            int n = angles.Length;
            int largestCluster = 0;
            int largestClusterIndex = 0;
            for (int i = 0; i < n; i++)
            {
                int clusterSize = angles
                    .Skip(i)
                    .TakeWhile(x => x < angles[i] + CLUSTER_TARGET_SPREAD)
                    .Count();
                if (clusterSize > largestCluster)
                {
                    largestCluster = clusterSize;
                    largestClusterIndex = i;
                }
            }
            return angles
                .Skip(largestClusterIndex)
                .TakeWhile(x => x < angles[largestClusterIndex] + CLUSTER_TARGET_SPREAD)
                .ToArray();
        }

        private static double[] GetAnglesOfBestLines(int[,] scores, int dCount)
        {
            var best = new (int angleIndex, int count)[BEST_MAX_COUNT];
            for (int i = 0; i < dCount; i++)
            {
                for (int angleIndex = 0; angleIndex < ANGLE_STEPS; angleIndex++)
                {
                    int count = scores[i, angleIndex];
                    if (count > best[BEST_MAX_COUNT - 1].count)
                    {
                        best[BEST_MAX_COUNT - 1] = (angleIndex, count);
                        for (int j = BEST_MAX_COUNT - 2; j >= 0; j--)
                        {
                            if (count > best[j].count)
                            {
                                (best[j], best[j + 1]) = (best[j + 1], best[j]);
                            }
                        }
                    }
                }
            }

            // Skip "insignificant" lines whose counts aren't close enough to the top 10
            int threshold = (int)(best[BEST_THRESHOLD_INDEX].count * BEST_THRESOLD_FACTOR);
            var bestWithinThreshold = best.Where(x => x.count >= threshold);

            double angleStepSize = (ANGLE_MAX - ANGLE_MIN) / (ANGLE_STEPS - 1);
            var angles = bestWithinThreshold.Select(x => ANGLE_MIN + angleStepSize * x.angleIndex);
            return angles.ToArray();
        }

        private static SinCos[] PrecalculateSinCos()
        {
            var sinCos = new SinCos[ANGLE_STEPS];
            double angleStepSize = (ANGLE_MAX - ANGLE_MIN) / (ANGLE_STEPS - 1);
            for (int i = 0; i < ANGLE_STEPS; i++)
            {
                double angle = (ANGLE_MIN + angleStepSize * i) * Math.PI / 180.0;
                sinCos[i].sin = (float)Math.Sin(angle);
                sinCos[i].cos = (float)Math.Cos(angle);
            }
            return sinCos;
        }

        private struct SinCos
        {
            public float sin;
            public float cos;
        }
    }
}
