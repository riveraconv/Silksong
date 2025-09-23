using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Silksong.Services;

namespace Silksong
{
    [Activity(Theme = "@style/Maui.SplashTheme",
              MainLauncher = true,
              LaunchMode = LaunchMode.SingleTask,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density
    )]
    [IntentFilter(
        [Intent.ActionSend],
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = "text/plain"
    )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

                if (savedInstanceState == null && Intent?.Action == Intent.ActionSend)
                {
                    RunOnUiThread(() => HandleShareIntent(Intent));
                }      
        }
        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            if (intent?.Action == Intent.ActionSend)
            {
                RunOnUiThread(() =>
                { 
                HandleShareIntent(intent);
                (Microsoft.Maui.Controls.Application.Current as App)?.ProcessPendingLink();
                });
            }
        }
        private static void HandleShareIntent(Intent? intent)
        {
            if (intent?.Action == Intent.ActionSend && intent.Type == "text/plain")
            {
                string? sharedText = intent.GetStringExtra(Intent.ExtraText);
                
                if (!string.IsNullOrWhiteSpace(sharedText))
                {
                    var service = IPlatformApplication.Current?.Services.GetService<ISharedLinkService>();
                    service?.ReceiveSharedLink(sharedText);
                    Android.Util.Log.Debug("Silksong", $"Shared text received: {sharedText}");
                }
            }     
        }
    }
}
