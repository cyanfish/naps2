using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NAPS2.Util
{
    public class SmoothProgress : IDisposable
    {
        private const int INTERVAL = 16;
        private const int VELOCITY_SAMPLE_SIZE = 5;

        private double inputPos;
        private double outputPos;

        private double inputVelocity;
        private double timeToCompletion;
        private double outputVelocity;

        private Stopwatch stopwatch;
        private Timer timer;

        private LinkedList<double> previousInputPos;
        private LinkedList<long> previousInputTimes;

        public SmoothProgress()
        {
            Reset();
        }

        public void Reset()
        {
            lock (this)
            {
                inputPos = 0;
                outputPos = 0;
                InvokeOutputProgressChanged();

                timer?.Dispose();
                timer = null;

                stopwatch = Stopwatch.StartNew();

                previousInputPos = new LinkedList<double>();
                previousInputPos.AddLast(0);
                previousInputTimes = new LinkedList<long>();
                previousInputTimes.AddLast(0);
            }
        }

        public void InputProgressChanged(double value)
        {
            lock (this)
            {
                if (inputPos < value)
                {
                    inputPos = value;
                    previousInputPos.AddLast(inputPos);
                    previousInputTimes.AddLast(stopwatch.ElapsedMilliseconds);

                    var deltaPos = previousInputPos.Last.Value - SampleStart(previousInputPos);
                    var deltaTime = previousInputTimes.Last.Value - SampleStart(previousInputTimes);

                    if (deltaTime > 0 && inputPos < 1)
                    {
                        inputVelocity = deltaPos / deltaTime;
                        timeToCompletion = (1 - inputPos) / inputVelocity;
                        outputVelocity = (1 - outputPos) / timeToCompletion;
                    }

                    if (inputPos >= 1)
                    {
                        inputVelocity = 0;
                        timeToCompletion = 0;
                        outputVelocity = 1;
                    }

                    if (timer == null)
                    {
                        timer = new Timer(TimerTick, null, 0, INTERVAL);
                    }
                }
            }
        }

        private T SampleStart<T>(LinkedList<T> list)
        {
            var node = list.Last;
            for (int i = 0; i < VELOCITY_SAMPLE_SIZE; i++)
            {
                if (node.Previous == null)
                {
                    break;
                }

                node = node.Previous;
            }
            if (node.Previous != null)
            {
                list.RemoveFirst();
            }
            return node.Value;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        private void TimerTick(object state)
        {
            lock (this)
            {
                outputPos = Math.Min(inputPos, outputPos + outputVelocity * INTERVAL);
            }
            InvokeOutputProgressChanged();
        }

        private void InvokeOutputProgressChanged()
        {
            OutputProgressChanged?.Invoke(this, new ProgressChangeEventArgs(outputPos));
        }

        public event ProgressChangeEventHandle OutputProgressChanged;

        public delegate void ProgressChangeEventHandle(object sender, ProgressChangeEventArgs args);

        public class ProgressChangeEventArgs : EventArgs
        {
            public ProgressChangeEventArgs(double value)
            {
                Value = value;
            }

            public double Value { get; set; }
        }
    }
}
