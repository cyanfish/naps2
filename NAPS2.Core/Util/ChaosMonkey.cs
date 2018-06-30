﻿using System;
using System.Diagnostics;
using System.Threading;

namespace NAPS2.Util
{
    internal static class ChaosMonkey
    {
        private static readonly Lazy<Random> random = new Lazy<Random>();

        [Conditional("DEBUG")]
        public static void MaybeError(double chance, Exception exception = null)
        {
            if (random.Value.NextDouble() < chance)
            {
                if (exception != null)
                {
                    throw exception;
                }
                else
                {
                    throw new Exception("Randomly generated exception for testing");
                }
            }
        }

        [Conditional("DEBUG")]
        public static void MaybeDelay(double chance, double durationInSeconds, double variationInSeconds = 0)
        {
            if (random.Value.NextDouble() < chance)
            {
                double duration = durationInSeconds + (variationInSeconds * ((random.Value.NextDouble() * 2) - 1));
                Thread.Sleep(TimeSpan.FromSeconds(duration));
            }
        }
    }
}