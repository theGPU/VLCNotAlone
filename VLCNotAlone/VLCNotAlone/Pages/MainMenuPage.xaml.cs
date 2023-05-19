using System;
using VLCNotAlone.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
            versionLabel.Text = $"VLCNotAlone {Constants.AppVersion}";
            //Navigation.PushModalAsync(new MainPage());
        }

        private void OpenSettingsMenu(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MainMenuSettingsPage());
        }

        private void OpenDirectConnectionPage(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new DirectConnectPage());
        }

        private void OpenServersDiscoverMenu(object sender, EventArgs e)
        {
            var serverBrowserPage = new ServerBrowserPage(ServerBrowserPageStartupType.Discover);
            Navigation.PushModalAsync(serverBrowserPage);
        }

        private void OpenFavoriteServersMenu(object sender, EventArgs e)
        {
            var serverBrowserPage = new ServerBrowserPage(ServerBrowserPageStartupType.Favorite);
            Navigation.PushModalAsync(serverBrowserPage);
        }

        private void OpenOfficialServersMenu(object sender, EventArgs e)
        {
            var serverBrowserPage = new ServerBrowserPage(ServerBrowserPageStartupType.Official);
            Navigation.PushModalAsync(serverBrowserPage);
        }
    }
}