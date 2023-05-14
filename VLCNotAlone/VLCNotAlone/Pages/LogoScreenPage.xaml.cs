using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogoScreenPage : ContentPage
    {
        public LogoScreenPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void OnLayoutSizeChanged(object sender, EventArgs e)
        {
            BlueLogo.TranslationY = -(sender as AbsoluteLayout).Height;
            Task.Run(RunLogoAnimationAsync);
        }

        private async Task RunLogoAnimationAsync()
        {
            BlueLogo.IsVisible = true;
            await BlueLogo.TranslateTo(0, 0, 1000).ContinueWith(task => Device.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PushAsync(new MainMenuPage());
                Navigation.RemovePage(this);
            }));
        }
    }
}