using Silksong.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Silksong
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            var service = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<ISharedLinkService>();
            if (service is null)
            {
                await DisplayAlert("Error", "There was a problem with the download, try it again or restart the application.", "OK");
                return;
            }

            await Navigation.PushAsync(new DownloadPage(service));
        }
    }
}
