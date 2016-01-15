using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;

namespace bládketvindóz
{
    class NetSpeedCounter
    {
        private Stopwatch watch;
        private long NumCounts = 0;
        private int PrevBytes = 0;
        private double[] DataPoints;

        private long CurrentBytesReceived = 0;

        public double Speed { get; private set; }
        public double ReceivedBytes { get; private set; }

        private System.Timers.Timer ticker = new System.Timers.Timer(100);

        public NetSpeedCounter(WebClient webClient, int maxPoints = 5)
        {
            ReceivedBytes = 1;
            watch = new System.Diagnostics.Stopwatch();

            DataPoints = new double[maxPoints];

            webClient.DownloadProgressChanged += (sender, e) =>
            {
                int curBytes = (int)(e.BytesReceived - PrevBytes);
                if (curBytes < 0)
                    curBytes = (int)e.BytesReceived;

                CurrentBytesReceived += curBytes;
                ReceivedBytes += curBytes;

                PrevBytes = (int)e.BytesReceived;
            };

            ticker.Elapsed += (sender, e) =>
            {
                double dataPoint = (double)CurrentBytesReceived / watch.ElapsedMilliseconds;
                DataPoints[NumCounts++ % maxPoints] = dataPoint;
                Speed = DataPoints.Average();
                CurrentBytesReceived = 0;
                watch.Restart();
            };
        }

        public void Stop(object sender, AsyncCompletedEventArgs e)
        {
            watch.Stop();
            ticker.Stop();
        }

        public void Start()
        {
            watch.Start();
            ticker.Start();
        }

        public void Reset()
        {
            CurrentBytesReceived = 0;
            PrevBytes = 0;
            watch.Restart();
            ticker.Start();
        }
    }
}
