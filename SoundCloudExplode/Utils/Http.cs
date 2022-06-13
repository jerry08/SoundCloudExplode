using SoundCloudExplode.Helpers;
using SoundCloudExplode.Utils.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCloudExplode.Utils
{
    internal class Http
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

        public static int NumberOfRetries = 1;
        public static int DelayOnRetry = 500;

        static Http()
        {
            // Increase maximum concurrent connections
            ServicePointManager.DefaultConnectionLimit = 20;
        }

        public static string GetHtml(
            string url,
            CancellationToken cancellationToken = default,
            WebHeaderCollection? headers = null)
        {
            return AsyncHelper.RunSync(() => GetHtmlAsync(url, cancellationToken, headers));

            //var task = GetHtmlAsync(url, headers);
            //var task = Task.Run(() => GetHtmlAsync(url, headers));
            //task.Wait();
            //return task.Result;
        }

        public async static Task<string> GetHtmlAsync(
            string url,
            CancellationToken cancellationToken = default,
            WebHeaderCollection? headers = null,
            IEnumerable<Cookie>? cookies = null)
        {
            url = url.Replace(" ", "%20");

            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    if (headers is not null)
                    {
                        for (int j = 0; j < headers.Count; j++)
                        {
                            request.SetRawHeader(headers.Keys[j]!, headers[j]!);
                        }
                    }

                    if (cookies != null)
                    {
                        request.CookieContainer = new CookieContainer();

                        foreach (var cookie in cookies)
                        {
                            request.CookieContainer.Add(cookie);
                        }
                    }

                    var response = (HttpWebResponse)await request.GetResponseAsync()
                        .WithCancellation(cancellationToken, request.Abort, true);
                    var receiveStream = response.GetResponseStream();
                    StreamReader? streamReader = null;

                    if (string.IsNullOrEmpty(response.CharacterSet))
                    {
                        streamReader = new StreamReader(receiveStream);
                    }
                    else
                    {
                        streamReader = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                    }

                    string html = await streamReader.ReadToEndAsync()
                        .WithCancellation(cancellationToken, streamReader.Close, true);

                    streamReader?.Close();
                    response?.Close();

                    return html;
                }
                catch
                {
                    await Task.Delay(DelayOnRetry);
                }
            }

            return "";
        }
    }
}