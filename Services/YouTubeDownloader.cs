
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Silksong.Services
{
    public static class YouTubeDownloader
    {
        private static YoutubeClient CreateYoutubeClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                                                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                                "Chrome/120.0.0.0 Safari/537.36");

            return new YoutubeClient(httpClient);
        }
        public static async Task <string> DownloadAudioAsync(string videoId, IProgress<double>? progressHandler = null)
        {
            var youtube = CreateYoutubeClient(); //new object from YoutubeExplode class library

            try
            {
                //obtains video info
                var video = await youtube.Videos.GetAsync(new VideoId(videoId));

                //obtains available audio stream
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

                var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                                                    .GetWithHighestBitrate()
                                                    ?? throw new InvalidOperationException("No audio stream available.");
                
                //standar download path route
                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                //creates the filename
                string safeTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(downloadsPath, safeTitle + ".m4a");

                //downloads the stream and returns the filepath to the user

                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, filePath, progressHandler);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Couldn't download the audio: {ex.Message}");
            }
        }
        public static async Task<(string Title, TimeSpan Duration, double EstimatedSizeMB)> GetVideoInfoAsync(string videoId) //to estimate paramameters before the download
        {
            var youtube = CreateYoutubeClient();
            var video = await youtube.Videos.GetAsync(new VideoId(videoId));

            //suponemos audio AAC 128 kbps (~16 KB/s)
            var durationSeconds = video.Duration?.TotalSeconds ?? 0;
            double estimatedSizeMB = (durationSeconds * 16) / 1024; //mb aprox que ocupara la descarga

            return (video.Title, video.Duration ?? TimeSpan.Zero, estimatedSizeMB);
        }
    }
}
