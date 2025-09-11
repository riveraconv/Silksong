using System.Text.RegularExpressions;
using Silksong.Services;

namespace Silksong;

public partial class DownloadPage : ContentPage
{
	public DownloadPage()
	{
		InitializeComponent();
	}

	private async void OnDownloadClicked(object sender, EventArgs e)
	{
		string? url = UrlEntry.Text?.Trim();

        //1.Validation
		if (!IsValidYoutubeUrl(url))
		{
            await DisplayAlert("Error", "Please, enter a YOUTUBE valid url!", "OK");
			return;
        }

        try
        {
            //2.Obtains video ID
            string? videoId = GetYoutubeVideoId(url);
            if (string.IsNullOrEmpty(videoId))
            {
                await DisplayAlert("Error", "Couldn't extract video ID from URL.", "OK");
                return;
            }

            var info = await Services.YouTubeDownloader.GetVideoInfoAsync(videoId);

            //3.Ask to the user for download
            bool confirm = await DisplayAlert("Video Info",
                                              $"Title: {info.Title}\n Duration: {info.Duration:mm\\:ss}\n Estimated Size: {info.EstimatedSizeMB:F2} MB\n\n, Do you want to download it?",
                                              "Yes", "No"
                                              );
            if (!confirm)
            {
                StatusLabel.Text = "Download cancelled";
                return;
            }

            //4.Update status meanwhile download is active
            StatusLabel.Text = "Downloading...";
            DownloadProgressBar.Progress = 0;
            ProgressLabel.Text = "0%";

            var progressHandler = new Progress<double>(value =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DownloadProgressBar.Progress = value;
                    ProgressLabel.Text = $"{value:P0}";
                });
            });

            //calling download path
            string filePath = await YouTubeDownloader.DownloadAudioAsync(videoId, progressHandler);


            await DisplayAlert("Done", $"Audio downloaded in:\n{filePath}", "OK");
            StatusLabel.Text = "✅ Download complete";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            StatusLabel.Text = "❌ Download failed";
        }
    }
    private static bool IsValidYoutubeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        var pattern = @"^(https?\:\/\/)?(www\.)?(m\.)?(youtube\.com|youtu\.be)\/.+$";
        return Regex.IsMatch(url, pattern);
    }

    private static string? GetYoutubeVideoId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        try
        {
            var uri = new Uri(url);

            // youtube.com/watch?v=...
            if (uri.Host.Contains("youtube.com"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"];
            }

            // youtu.be/...
            if (uri.Host.Contains("youtu.be"))
            {
                return uri.AbsolutePath.Trim('/');
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}