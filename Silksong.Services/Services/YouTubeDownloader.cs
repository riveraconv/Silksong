using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Silksong.Services
{

    public static class YouTubeDownloader
    {
        public static readonly string[] UserAgents =
        [

            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:143.0) Gecko/20100101 Firefox/143.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Safari/537.36 Edg/140.0.3485.66",
            "Mozilla/5.0 (Linux; Android 13; Pixel 7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 13; SM-G991B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 10; IN2011) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.7339.124 Mobile Safari/537.36"
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


#if __ANDROID__
                string musicBase = Android.OS.Environment
                     .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic)?
                     .AbsolutePath 
                     ?? throw new InvalidOperationException("Couldn't find Android Music Directory.");
#else
                string musicBase = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
#endif
                string musicFolder = Path.Combine(musicBase, "SilksongDownloads");

                //creates the folder if doesn't exists yet
                if (!Directory.Exists(musicFolder))
                    Directory.CreateDirectory(musicFolder);
                
                //creates the filename, if exist, tells it to the user
                string safeTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));
                string filePath = Path.Combine(musicFolder, safeTitle + ".m4a");

                long estimatedSize = audioStreamInfo.Size.Bytes;

                if (File.Exists(filePath))
                {
                    var existingFileInfo = new FileInfo(filePath);
                    if (Math.Abs(existingFileInfo.Length - estimatedSize) < 1024)
                    {
                        throw new InvalidOperationException("You already have this audio file.");
                    }
                }


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

            //audio AAC 128 kbps (~16 KB/s)
            var durationSeconds = video.Duration?.TotalSeconds ?? 0;
            double estimatedSizeMB = (durationSeconds * 16) / 1024; //mb aprox

            return (video.Title, video.Duration ?? TimeSpan.Zero, estimatedSizeMB);
        }
    }
}
