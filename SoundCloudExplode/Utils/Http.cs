using SoundCloudExplode.Helpers;
using SoundCloudExplode.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCloudExplode.Utils
{
    internal class Http
    {
        private static readonly Lazy<HttpClient> HttpClientLazy = new(() =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false
            };

            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            return new HttpClient(handler, true);
        });

        public static HttpClient Client => HttpClientLazy.Value;

        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

        public static int NumberOfRetries = 1;
        public static int DelayOnRetry = 500;

        public static string GetHtml(
            string url,
            WebHeaderCollection? headers = null,
            CancellationToken cancellationToken = default)
        {
            return AsyncHelper.RunSync(() => GetHtmlAsync(url, headers, null, cancellationToken));

            //var task = GetHtmlAsync(url, headers);
            //var task = Task.Run(() => GetHtmlAsync(url, headers));
            //task.Wait();
            //return task.Result;
        }

        public async static Task<string> GetHtmlAsync(
            string url,
            WebHeaderCollection? headers = null,
            IEnumerable<Cookie>? cookies = null,
            CancellationToken cancellationToken = default)
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

        public static async Task<long> GetFileSizeAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            //await Client.GetStreamAsync(url, cancellationToken);

            var request = WebRequest.Create(url);
            request.Method = "HEAD";

            using var webResponse = await request.GetResponseAsync()
                .WithCancellation(cancellationToken, request.Abort, true);
            return webResponse.ContentLength;
        }
    }
}