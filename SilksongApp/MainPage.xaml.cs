using Silksong.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace Silksong
{
    
    public partial class MainPage : ContentPage
    {
        public string AppVersion { get; set; }
        public MainPage()
        {
            InitializeComponent();

            AppVersion = $"V.{AppInfo.Current.VersionString} (Build {AppInfo.Current.BuildString})";
            BindingContext = this;
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

        private async void OnDisclaimerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DisclaimerPage());
        }
        private async void OnInstructionsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InstructionsPage());
        }
    }
}
