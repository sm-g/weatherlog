using System;
using System.Diagnostics;
using System.Net;

namespace Weatherlog.Models
{
    public static class Loader
    {
        public static string LoadString(string uri)
        {
            string response = "";

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    response = webClient.DownloadString(new Uri(uri));
                }
                catch (Exception e)
                {
                    Trace.TraceError(String.Format("Downloading {0}: {1}", uri, e.Message));
                }
            }

            return response;
        }

        public static void LoadStringAsync(string uri, Action<object, DownloadStringCompletedEventArgs> onCompleted, Action<object, DownloadProgressChangedEventArgs> onProgressChanged = null)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onCompleted);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(onProgressChanged);
                try
                {
                    webClient.DownloadStringAsync(new Uri(uri));
                }
                catch (Exception e)
                {
                    Trace.TraceError(String.Format("Downloading {0}: {1}", uri, e.Message));
                }
            }
        }
    }
}
