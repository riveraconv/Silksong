
using Silksong.Services;
using YoutubeExplode.Videos;

namespace Silksong.Tests.Services
{
    public class YouTubeDownloaderTests
    {
        [Fact]
        public void CreateHttpClient_ShouldSetUserAgent()
        {
            var expectedAgent = YouTubeDownloader.UserAgents[0];

            var httpClient = YouTubeDownloader.CreateHttpClient(expectedAgent);

            Assert.Contains(expectedAgent, httpClient.DefaultRequestHeaders.UserAgent.ToString());
        }

        [Fact]
        public void CreateYoutubeClient_ShouldReturnYoutubeClient()
        {
            var client = YouTubeDownloader.CreateYoutubeClient();

            Assert.NotNull(client);
            Assert.IsType<YoutubeExplode.YoutubeClient>(client);
        }
    }

    //integration tests

    public class YoutubeDownloaderIntegrationTests
    {
        [Fact]
        public async Task DownloadAudioAsync_RealVideo_FileDownloaded_Repeatable()
        {
            // Replace with a real test video ID
            var videoId = "dQw4w9WgXcQ";

            // Get the real video title
            var video = await YouTubeDownloader.CreateYoutubeClient()
                                              .Videos.GetAsync(videoId);
            string safeTitle = string.Concat(video.Title.Split(Path.GetInvalidFileNameChars()));

            // Downloads folder and file path
            string musicFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "SilksongDownloads");
            string filePath = Path.Combine(musicFolder, safeTitle + ".m4a");

            // Delete the file if it exists from previous tests
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Download progress
            var progress = new Progress<double>(p => Console.WriteLine($"Progress: {p:P0}"));

            // Executes the download and checks if the folder exists o has to be created

            string downloadedFile = await YouTubeDownloader.DownloadAudioAsync(videoId, progress);

            // Verify that the folder and the file were created
            Assert.True(Directory.Exists(musicFolder), "The download folder should exist after the download");
            Console.WriteLine($"Folder exists: {Directory.Exists(musicFolder)}");
            Assert.True(File.Exists(downloadedFile));

            Console.WriteLine($"Downloaded file at: {downloadedFile}");
        }
        [Fact]
        public async Task GetVideoInfoAsync_RealVideo_ReturnsInfo()
        {
            var videoId = "dQw4w9WgXcQ";
            var result = await YouTubeDownloader.GetVideoInfoAsync(videoId);

            Assert.False(string.IsNullOrEmpty(result.Title));
            Assert.True(result.Duration > TimeSpan.Zero);
            Assert.True(result.EstimatedSizeMB > 0);

            Console.WriteLine($"Title: {result.Title}, Duration: {result.Duration}, Estimated size: {result.EstimatedSizeMB}) MB.");
        }
    }

}

