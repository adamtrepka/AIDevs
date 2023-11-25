using Azure.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace AIDevs.Tests.Unit.Infrastructure
{
    public class VideoDownloaderTests
    {
        [Fact]
        public async Task Should_Download_Video()
        {
            var youtube = new CustomYouTube();
            var allVideos = await youtube.GetAllVideosAsync("https://www.youtube.com/watch?v=QwoQ9aXd6bk");
            var audio = allVideos.Where(x => x.AdaptiveKind == AdaptiveKind.Audio).OrderBy(x => x.ContentLength).First();

            var url = await audio.GetUriAsync();
            await youtube.CreateDownloadAsync(new Uri(url), "test.weba",
                new Progress<Tuple<long, long>>((Tuple<long, long> v) =>
                {
                    var percent = (int)((v.Item1 * 100) / v.Item2);
                    Debug.Write(string.Format("Downloading.. ( % {0} ) {1} / {2} MB\r", percent, (v.Item1 / (double)(1024 * 1024)).ToString("N"), (v.Item2 / (double)(1024 * 1024)).ToString("N")));
                }));

        }

    }

    class CustomHandler
    {
        public HttpMessageHandler GetHandler()
        {
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("CONSENT", "YES+cb", "/", "youtube.com"));
            return new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

        }
    }
    class CustomYouTube : YouTube
    {
        private long chunkSize = 10_485_760;
        private long _fileSize = 0L;
        private HttpClient _client = new HttpClient();
        protected override HttpClient MakeClient(HttpMessageHandler handler)
        {
            return base.MakeClient(handler);
        }
        protected override HttpMessageHandler MakeHandler()
        {
            return new CustomHandler().GetHandler();
        }
        public async Task CreateDownloadAsync(Uri uri, string filePath, IProgress<Tuple<long, long>> progress = null)
        {
            var totalBytesCopied = 0L;
            _fileSize = await GetContentLengthAsync(uri.AbsoluteUri) ?? 0;
            if (_fileSize == 0)
            {
                throw new Exception("File has no any content !");
            }
            //using (Stream output = File.OpenWrite(filePath))
            using (Stream output = new MemoryStream())
            {
                var segmentCount = (int)Math.Ceiling(1.0 * _fileSize / chunkSize);
                for (var i = 0; i < segmentCount; i++)
                {
                    var from = i * chunkSize;
                    var to = (i + 1) * chunkSize - 1;
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    request.Headers.Range = new RangeHeaderValue(from, to);
                    using (request)
                    {
                        // Download Stream
                        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        if (response.IsSuccessStatusCode)
                            response.EnsureSuccessStatusCode();
                        var stream = await response.Content.ReadAsStreamAsync();
                        //File Steam
                        var buffer = new byte[32_768];
                        int bytesCopied;
                        do
                        {
                            bytesCopied = await stream.ReadAtLeastAsync(buffer, buffer.Length);
                            await output.WriteAsync(buffer, 0, bytesCopied);
                            totalBytesCopied += bytesCopied;
                            progress?.Report(new Tuple<long, long>(totalBytesCopied, _fileSize));
                        } while (bytesCopied > 0);
                    }
                }
            }
        }
        private async Task<long?> GetContentLengthAsync(string requestUri, bool ensureSuccess = true)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, requestUri))
            {
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (ensureSuccess)
                    response.EnsureSuccessStatusCode();
                return response.Content.Headers.ContentLength;
            }
        }
    }
}
