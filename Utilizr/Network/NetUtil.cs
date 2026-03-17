using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Utilizr.Network
{
    public static class NetUtil
    {
        /// <summary>
        /// Get an available local port between the specified range
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="endPort"></param>
        /// <returns></returns>
        public static int GetAvailablePort(int startPort, int endPort)
        {
            var localhostAddress = Dns.GetHostEntry("localhost").AddressList?.Where(i => i.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();

            if (localhostAddress == null)
                throw new Exception("IPv4 localhost not found");

            TcpListener tcpListener;
            for (int i = startPort; i <= endPort; i++)
            {
                try
                {
                    tcpListener = new TcpListener(localhostAddress, i);
                    tcpListener.Start();
                    tcpListener.Stop();
                    return i;
                }
                catch (SocketException)
                {
                    //port unavailable
                }
            }
            throw new Exception("No ports available within the requested range");
        }


        public static int GetRandomAvailablePort(int startPort, int endPort)
        {
            var localhostAddress = Dns.GetHostEntry("localhost").AddressList.Where(i => i.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();

            if (localhostAddress == null)
                throw new Exception("IPv4 localhost not found");

            TcpListener tcpListener;
            var random = new Random(DateTime.UtcNow.Millisecond);
            for (int i = startPort; i <= endPort; i++)
            {
                try
                {
                    var port = random.Next(startPort, endPort);
                    tcpListener = new TcpListener(localhostAddress, port);
                    tcpListener.Start();
                    tcpListener.Stop();
                    return port;
                }
                catch (SocketException)
                {
                    //port unavailable
                }
            }
            throw new Exception($"No random port available within the requested range after {endPort - startPort} attempts");
        }


        public static string? GetPublicIPFromOpenDNS()
        {
            for (int i = 1; i < 4; i++)
            {
                try
                {
                    string ip = "";
                    var nslookupOutput = Shell.Exec("nslookup", "myip.opendns.com", $"resolver{i}.opendns.com");
                    if (nslookupOutput.ErrorOutput?.Contains("***") == true)
                    {
                        throw new Exception($"IP lookup failed: {nslookupOutput.ErrorOutput}");
                    }

                    ip = nslookupOutput.Output!.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .AsEnumerable()
                        .Reverse()
                        .ToArray()[0]
                        .Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1];

                    if (ip.Split('.').Length == 4)
                    {
                        return ip.Trim();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error getting public IP", ex);
                }
            }
            return null;
        }

        public static async Task<bool> UrlReachableAsync(string url)
        {
            try
            {
                using var http = new HttpClient();

                using var response = await http.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, url),
                    HttpCompletionOption.ResponseHeadersRead
                );

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<long> GetDownloadSizeAsync(string url)
        {
            using var http = new HttpClient();
            using var response = await http.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, url),
                HttpCompletionOption.ResponseHeadersRead
            );

            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentLength ?? -1;
        }


        public delegate void DownloadProgressDelegate(double percent, long read, long totalSize);

        public static async Task DownloadFileAsync(
            string url,
            MemoryStream destination,
            DownloadProgressDelegate? progressCallback = null,
            int requestTimeout = -1,
            string? userAgent = null)
        {
            using var httpClient = new HttpClient()
            {
                Timeout = requestTimeout > 0
                    ? TimeSpan.FromMilliseconds(requestTimeout)
                    : Timeout.InfiniteTimeSpan,
            };

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(userAgent))
                request.Headers.UserAgent.ParseAdd(userAgent);

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead
            );

            response.EnsureSuccessStatusCode();

            var totalSize = response.Content.Headers.ContentLength ?? 0L;
            var totalRead = 0L;

            using var responseStream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[8 * 1024];
            int bytesRead = 0;

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
                progressCallback?.Invoke(
                    (double)totalRead / totalSize,
                    totalRead,
                    totalSize
                );
            }
        }

        public static async Task DownloadFileAsync(
            string url,
            string destination,
            DownloadProgressDelegate? progressCallback = null,
            bool autoResume = false,
            int requestTimeout = -1,
            string? userAgent = null,
            Action<PipelineActionArgs>? pipelineAction = null)
        {
            using var httpClient = new HttpClient()
            {
                Timeout = requestTimeout > 0
                    ? TimeSpan.FromMilliseconds(requestTimeout)
                    : Timeout.InfiniteTimeSpan,
            };

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(userAgent))
                request.Headers.UserAgent.ParseAdd(userAgent);

            var resumeFrom = 0L;
            if (autoResume && File.Exists(destination))
            {
                resumeFrom = new FileInfo(destination).Length;

                if (resumeFrom > 0)
                    request.Headers.Range = new RangeHeaderValue(resumeFrom, null);
            }

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead
            );

            response.EnsureSuccessStatusCode();

            long totalSize = (response.Content.Headers.ContentLength ?? 0) + resumeFrom;
            long totalRead = resumeFrom;

            using var fileStream = new FileStream(
                destination,
                resumeFrom > 0
                    ? FileMode.Open
                    : FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            if (resumeFrom > 0)
                fileStream.Seek(resumeFrom, SeekOrigin.Begin);

            using var responseStream = await response.Content.ReadAsStreamAsync();

            var buffer = new byte[8 * 1024];
            int bytesRead;

            var pipelineArgs = new PipelineActionArgs(buffer);

            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);

                totalRead += bytesRead;

                pipelineArgs.Length = bytesRead;
                pipelineAction?.Invoke(pipelineArgs);

                progressCallback?.Invoke(
                    (double)totalRead / totalSize,
                    totalRead,
                    totalSize
                );
            }
        }

        public class PipelineActionArgs
        {
            public byte[] Buffer { get; set; }
            public int Length { get; set; }

            public PipelineActionArgs(byte[] buffer)
            {
                Buffer = buffer;
            }
        }
    }
}
