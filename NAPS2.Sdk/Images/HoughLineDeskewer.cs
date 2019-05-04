using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public class HoughLineDeskewer : Deskewer
    {
        private const double ANGLE_MIN = -20;
        private const double ANGLE_MAX = 20;
        private const int ANGLE_STEPS = 201; // 0.2 degree step size
        private const int BEST_MAX_COUNT = 100;
        private const int BEST_THRESHOLD_INDEX = 9;
        private const double BEST_THRESOLD_FACTOR = 0.5;
        private const double CLUSTER_TARGET_SPREAD = 2.0;

        public override double GetSkewAngle(IImage image)
        {
            var bitArrays = UnsafeImageOps.ConvertToBitArrays(image);

            int h = image.Height;
            int w = image.Width;

            var sinCos = PrecalculateSinCos();

            int dCount = 2 * (w + h);
            int[,] scores = new int[dCount, ANGLE_STEPS];

            // TODO: This should be a good candidate for OpenCL optimization.
            // TODO: If you parallelize over the angle, you're operating over
            // TODO: the same input data with the same branches.
            for (int y = 1; y <= h - 2; y++)
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
