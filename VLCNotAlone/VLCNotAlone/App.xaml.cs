using VLCNotAlone.Pages;
using Xamarin.Forms;

namespace VLCNotAlone
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

#if DEBUG
            MainPage = new NavigationPage(new MainMenuPage());
#else
            MainPage = new NavigationPage(new LogoScreenPage());
#endif
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
