using Silksong.Services;

namespace Silksong
{
    public partial class App : Application
    {
        private static NavigationPage? _mainNav;

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            ProcessPendingLink();
            return new Window(_mainNav!);
        }

        public void ProcessPendingLink()
        {
            var sharedLinkService = IPlatformApplication.Current?.Services.GetService<ISharedLinkService>();
            var pendingLink = sharedLinkService?.ConsumePendingLink();

            if (!string.IsNullOrWhiteSpace(pendingLink))
            {
                if (_mainNav == null)
                    _mainNav = new NavigationPage(new DownloadPage(sharedLinkService!, pendingLink));
                else
                    _mainNav.Navigation.PushAsync(new DownloadPage(sharedLinkService!, pendingLink));
            }
            else if (_mainNav == null)
            {
                _mainNav = new NavigationPage(new MainPage());
            }
        }
    }

}

