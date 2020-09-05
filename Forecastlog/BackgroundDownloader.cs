using System.Threading;
using Weatherlog.Models;

namespace Weatherlog
{
    class BackgroundDownloader
    {
        readonly object stopLock = new object();
        bool stopping = false;

        bool Stopping
        {
            get
            {
                lock (stopLock)
                {
                    return stopping;
                }
            }
        }

        public void Run()
        {
            var thread = new Thread(DoWork);
            thread.Name = "Background regular fetcher";
            thread.IsBackground = true;
            thread.Start();
        }

        public void Stop()
        {
            lock (stopLock)
            {
                stopping = true;
            }
        }

        private void DoWork()
        {
            Thread.Sleep(15000); // wait internet connection
            while (!Stopping)
            {
#if !DEBUG
                Fetcher.Instance.MakeRegularFetching(StationsDirector.Instance.Stations);
#endif
                Thread.Sleep(Fetcher.Instance.SpanToNextRegularFetch);
            }

        }
    }
}
