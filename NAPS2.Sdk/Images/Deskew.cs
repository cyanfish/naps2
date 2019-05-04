using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public static class Deskew
    {
        private const double ANGLE_MIN = -20;
        private const double ANGLE_MAX = 20;
        private const int ANGLE_STEPS = 201; // 0.2 degree step size
        private const int BEST_COUNT = 20;

        public static double GetSkewAngle(IImage image)
        {
            var bitArrays = UnsafeImageOps.ConvertToBitArrays(image);

            int h = image.Height;
            int w = image.Width;

            var sinCos = PrecalculateSinCos();

            int dCount = 2 * (w + h);
            int[,] scores = new int[dCount, ANGLE_STEPS];

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
            // TODO: Outlier detection
            return angles.Sum() / angles.Length;
        }

        private static double[] GetAnglesOfBestLines(int[,] scores, int dCount)
        {
            var best = new (int angleIndex, int count)[BEST_COUNT];
            for (int i = 0; i < dCount; i++)
            {
                for (int angleIndex = 0; angleIndex < ANGLE_STEPS; angleIndex++)
                {
                    int count = scores[i, angleIndex];
                    if (count > best[BEST_COUNT - 1].count)
                    {
                        best[BEST_COUNT - 1] = (angleIndex, count);
                        for (int j = BEST_COUNT - 2; j >= 0; j--)
                        {
                            if (count > best[j].count)
                            {
                                (best[j], best[j + 1]) = (best[j + 1], best[j]);
                            }
                        }
                    }
                }
            }
            double angleStepSize = (ANGLE_MAX - ANGLE_MIN) / (ANGLE_STEPS - 1);
            return best.Select(x => ANGLE_MIN + angleStepSize * x.angleIndex).ToArray();
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
