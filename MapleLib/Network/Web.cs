﻿// Project: MapleSeed
// File: Web.cs
// Updated By: Jared
// 

#region usings

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MapleLib.Structs;

#endregion

namespace MapleLib.Network
{
    public static class Web
    {
        private const string WII_USER_AGENT = "wii libnup/1.0";

        public static event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChangedEvent;

        public static async Task DownloadContent(this Title title, string contentType, string version)
        {
            if (string.IsNullOrEmpty(title.ID))
                throw new Exception("Can't download content without a valid Title ID.");

            if (string.IsNullOrEmpty(title.FolderLocation))
                title.FolderLocation = Path.GetFullPath(Path.Combine(Settings.LibraryDirectory, $"{title}"));

            await Database.DownloadTitle(title.ID, title.FolderLocation, contentType, version);
        }

        public static async Task DownloadFileAsync(string url, string saveTo)
        {
            var wc = new WebClient {Headers = {[HttpRequestHeader.UserAgent] = WII_USER_AGENT}};
            wc.DownloadProgressChanged += DownloadProgressChanged;

            await wc.DownloadFileTaskAsync(new Uri(url), saveTo);
            while (wc.IsBusy) await Task.Delay(100);
            wc.Dispose();
        }

        public static string DownloadString(string url)
        {
            using (var wc = new WebClient()) {
                wc.Headers[HttpRequestHeader.UserAgent] = WII_USER_AGENT;
                wc.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                wc.DownloadProgressChanged += DownloadProgressChanged;
                return wc.DownloadString(url);
            }
        }

        public static byte[] DownloadData(string url)
        {
            using (var wc = new WebClient()) {
                wc.Headers[HttpRequestHeader.UserAgent] = WII_USER_AGENT;
                wc.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                wc.DownloadProgressChanged += DownloadProgressChanged;
                var task = wc.DownloadDataTaskAsync(new Uri(url));
                return task.Result;
            }
        }

        public static async Task<byte[]> DownloadDataAsync(string url)
        {
            using (var wc = new WebClient()) {
                wc.Headers[HttpRequestHeader.UserAgent] = WII_USER_AGENT;
                wc.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                wc.DownloadProgressChanged += DownloadProgressChanged;
                return await wc.DownloadDataTaskAsync(new Uri(url));
            }
        }

        public static void ResetDownloadProgressChanged()
        {
            DownloadProgressChangedEvent?.Invoke(null, null);
        }

        private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChangedEvent?.Invoke(sender, e);
        }

        public static bool UrlExists(string url)
        {
            var webRequest = WebRequest.Create(url);
            webRequest.Timeout = 1200;
            webRequest.Method = "HEAD";

            HttpWebResponse response = null;

            try {
                using (response = (HttpWebResponse) webRequest.GetResponse()) {
                    if (response.StatusCode == HttpStatusCode.OK)
                        return true;
                }
            }
            catch {
                // ignored
            }
            finally {
                response?.Close();
            }

            return false;
        }
    }
}