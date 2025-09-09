using System.Text.RegularExpressions;

namespace Silksong;

public partial class DownloadPage : ContentPage
{
	public DownloadPage()
	{
		InitializeComponent();
	}

	private async void OnDownloadClicked(object sender, EventArgs e)
	{
		string? url = UrlEntry.Text;

		if (!IsValidYoutubeUrl(url))
		{
            await DisplayAlert("Error", "Please, enter a YOUTUBE valid url!", "OK");
			return;
        }
		await DisplayAlert("Url was correct", $"Processing: ({url})", "OK");
    }
    private static bool IsValidYoutubeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        var pattern = @"^(https?\:\/\/)?(www\.)?(m\.)?(youtube\.com|youtu\.be)\/.+$";
        return Regex.IsMatch(url, pattern);
    }

}