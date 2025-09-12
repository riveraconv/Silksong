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

        //1.URL Validation 
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
            bool confirm = await DisplayAlert("VIDEO INFO",
                                              $"Title: {info.Title}\nDuration: {info.Duration:mm\\:ss}\nEstimated Size: {info.EstimatedSizeMB:F2} MB\n\nDo you want to download it?",
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

            //calling download path, using Task.Run to liberate UI thread and don't getting NetworkOnMainException
            try
            {
                string filePath = await Task.Run(() =>
                    YouTubeDownloader.DownloadAudioAsync(videoId, progressHandler)
                );

                StatusLabel.Text = $"✅ Download complete: {filePath}";
                await DisplayAlert("Done", $"Audio downloaded in:\n{filePath}", "OK");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
            {
                StatusLabel.Text = "❌ " + ex.Message;
                await DisplayAlert("Info", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "❌ Download failed";
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
        catch(Exception exOuter)
        {
            StatusLabel.Text = "❌ Unexpected error";
            await DisplayAlert("Error", exOuter.Message, "OK");
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