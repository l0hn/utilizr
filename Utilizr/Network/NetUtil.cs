using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Utilizr.Network
{
    // TODO: Refactor NetUtil away from WebRequest.CreateRequest()
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

                    ip = nslookupOutput.Output!.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries)
                        .Reverse().ToArray()[0]
                        .Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries)[1];

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

        public static bool UrlReachable(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                using var response = request.GetResponse();
                var httpResponse = (HttpWebResponse)response;
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"failed to make head request to {url}");
                }
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }


        public delegate void DownloadProgressDelegate(double percent, long read, long totalSize);

        public static void DownloadFile(
            string url,
            string destination,
            DownloadProgressDelegate? progressCallback = null,
            bool autoResume = false,
            int requestTimeout = -1,
            string? userAgent = null,
            Action<PipelineActionArgs>? pipelineAction = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = requestTimeout;//infinite
            request.ReadWriteTimeout = 15 * 1000;//15 seconds

            if (!string.IsNullOrEmpty(userAgent))
                request.UserAgent = userAgent;

            long resumeFromByte = 0;
            if (autoResume)
            {
                //check file has this many bytes
                var exists = File.Exists(destination);
                if (exists)
                {
                    resumeFromByte = new FileInfo(destination).Length;

                    //Set the value with reflection so we can use long int values
                    try
                    {
                        var method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
                        string rangeHeaderValue = $"bytes={resumeFromByte}-";
                        method?.Invoke(request.Headers, new object[] { HttpRequestHeader.Range.ToString(), rangeHeaderValue });
                    }
                    catch (Exception)
                    {
                        resumeFromByte = 0;
                    }
                }
            }
            using var response = request.GetResponse();
            using var responseStream = response.GetResponseStream();
            using var fileStream = File.Open(destination, resumeFromByte > 0 ? FileMode.Open : FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            if (resumeFromByte > 0)
            {
                fileStream.Seek(resumeFromByte, 0);
            }
            long totalSize = response.ContentLength + resumeFromByte;
            long totalRead = resumeFromByte;
            int bufferLen = 8 * 1024;
            byte[] buffer = new byte[bufferLen];
            int bytesRead = 0;

            var pipelineArgs = new PipelineActionArgs(buffer);
            while ((bytesRead = responseStream.Read(buffer, 0, bufferLen)) != 0)
            {
                if (bytesRead > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    pipelineArgs.Length = bytesRead;
                    pipelineAction?.Invoke(pipelineArgs);
                    progressCallback?.Invoke(((double)totalRead / totalSize), totalRead, totalSize);
                }
            }
        }

        public static Task BeginDownloadFileAsync(
            string url,
            string destination,
            DownloadProgressDelegate? progressCallback = null,
            int requestTimeout = -1,
            string? userAgent = null)
        {
            return Task.Run(() => DownloadFile(url, destination, progressCallback, requestTimeout:requestTimeout, userAgent:userAgent));
        }

        public static long GetDownloadSize(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";

            using var response = (HttpWebResponse)(request.GetResponse());
            return response.ContentLength;
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
