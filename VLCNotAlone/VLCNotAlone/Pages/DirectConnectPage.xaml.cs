using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectConnectPage : ContentPage
    {
        public DirectConnectPage()
        {
            InitializeComponent();
#if DEBUG
            endPointEntry.Text = "https://localhost:7133/server";
#endif
        }

        private void OnBackClicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void OnConnectClicked(object sender, System.EventArgs e)
        {
            Navigation.PushModalAsync(new ConnectingProcessPage(endPointEntry.Text));
        }
    }
}