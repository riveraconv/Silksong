
using Android.Content;
using Android.OS;
using Android.Provider;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Silksong.Services
{
    public static partial class YouTubeDownloader
    {
        public static partial Task<string> DownloadAudioAsync(string videoId, IProgress<double>? progressHandler = null);

        public static readonly string[] UserAgents =
        [

           "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.6533.120 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:128.0) Gecko/20100101 Firefox/128.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edg/128.0.2739.67 Safari/537.36",
            "Mozilla/5.0 (Linux; Android 14; Pixel 8 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.6613.114 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 14; SM-G998B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.6613.114 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 13; OnePlus 9 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.6613.114 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 13; Redmi Note 12) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.6613.114 Mobile Safari/537.36"
        ];
        public static HttpClient CreateHttpClient(string? forcedUserAgent = null)
        {
            var userAgent = forcedUserAgent ?? UserAgents[new Random().Next(UserAgents.Length)];
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            
            return httpClient;
        }

        public static YoutubeClient CreateYoutubeClient(string? forcedUserAgent = null)
        {
            return new YoutubeClient(CreateHttpClient(forcedUserAgent));
        }
        public static async partial Task<string> DownloadAudioAsync(string videoId, IProgress<double>? progressHandler = null)
        {
            var youtube = CreateYoutubeClient();

            try
            {

                // 🔹 Obtener información del video
                var video = await youtube.Videos.GetAsync(videoId);
                var manifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var stream = manifest.GetAudioOnlyStreams()
                            .GetWithHighestBitrate()
                            ?? throw new InvalidOperationException("No audio stream available.");

                string fileName = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars())) + ".m4a";
                string relativePath = "Download/SilksongDownloads/";

                var resolver = Android.App.Application.Context.ContentResolver;

                var projection = new[] { MediaStore.MediaColumns.Id, MediaStore.MediaColumns.DisplayName };
                var selection = $"{MediaStore.MediaColumns.RelativePath} = ? AND {MediaStore.MediaColumns.DisplayName} = ?";
                var selectionArgs = new[] { relativePath, fileName };

                using var cursor = resolver.Query(MediaStore.Downloads.ExternalContentUri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    throw new InvalidOperationException("You already have this audio file.");
                }

                var values = new ContentValues();
                values.Put(MediaStore.MediaColumns.DisplayName, fileName);
                values.Put(MediaStore.MediaColumns.MimeType, "audio/m4a");
                values.Put(MediaStore.MediaColumns.RelativePath, relativePath);

                var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri, values)
                          ?? throw new InvalidOperationException("Failed to create MediaStore entry.");

                using (var output = resolver.OpenOutputStream(uri))
                using (var input = await youtube.Videos.Streams.GetAsync(stream))
                {
                    byte[] buffer = new byte[8192];
                    long totalRead = 0;
                    int read;
                    long totalBytes = stream.Size.Bytes;

                    while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                    {
                        await output.WriteAsync(buffer.AsMemory(0, read));
                        totalRead += read;

                        double progress = (double)totalRead / totalBytes;
                        progressHandler?.Report(progress);
                    }
                }

                Android.Media.MediaScannerConnection.ScanFile(
                    Android.App.Application.Context,
                    new[] { uri.ToString() },
                    new[] { "audio/m4a" },
                    null
                );

                return Path.Combine("/storage/emulated/0/Download/SilksongDownloads", fileName);
            }
            catch(YoutubeExplode.Exceptions.YoutubeExplodeException)
            {
                throw new InvalidOperationException("Youtube blocked this download temporaly, try again later.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Couldn't download the audio: {ex.Message}", ex);
            }
        }

        public static async Task<(string Title, TimeSpan Duration, double EstimatedSizeMB)> GetVideoInfoAsync(string videoId) //to estimate paramameters before the download
        {
            var youtube = CreateYoutubeClient();
            var video = await youtube.Videos.GetAsync(new VideoId(videoId));

            //audio AAC 128 kbps (~16 KB/s)
            var durationSeconds = video.Duration?.TotalSeconds ?? 0;
            double estimatedSizeMB = (durationSeconds * 16) / 1024; //mb aprox

            return (video.Title, video.Duration ?? TimeSpan.Zero, estimatedSizeMB);
        }
    }
}

