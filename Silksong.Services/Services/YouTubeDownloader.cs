using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Silksong.Services
{

    public static partial class YouTubeDownloader
    {
        public static readonly string[] UserAgents =
        [

            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:143.0) Gecko/20100101 Firefox/143.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Safari/537.36 Edg/140.0.3485.66",
            "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 13; SM-G991B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 10; IN2011) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 14; Pixel 8 Pro) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36",
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

        public static async Task<string> DownloadAudioAsync(string videoId, IProgress<double>? progressHandler = null)
        {
            var youtube = CreateYoutubeClient(); // Instancia de YoutubeExplode

            try
            {
                // 🔹 Obtener información del video
                var video = await youtube.Videos.GetAsync(new VideoId(videoId));
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams()
                                                    .GetWithHighestBitrate()
                                                    ?? throw new InvalidOperationException("No audio stream available.");

                string fileName = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars())) + ".m4a";

                // Windows / macOS / Linux
                string musicBase = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                string folderPath = Path.Combine(musicBase, "SilksongDownloads");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);

                // Evitar sobrescribir si ya existe
                if (File.Exists(filePath))
                {
                    var existingFileInfo = new FileInfo(filePath);
                    long estimatedSize = audioStreamInfo.Size.Bytes;
                    if (Math.Abs(existingFileInfo.Length - estimatedSize) < 1024)
                        throw new InvalidOperationException("You already have this audio file.");
                }

                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, filePath, progressHandler);
                return filePath;

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
