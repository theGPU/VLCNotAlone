using VLCNotAlone.Settings;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenuSettingsPage : ContentPage
    {
        public MainMenuSettingsPage()
        {
            InitializeComponent();

            nicknameEntry.Text = ClientSettings.Nickname;
            adminTokenEntry.Text = ClientSettings.AdminToken;
            useHardwareAccelerationCheckBox.IsChecked = ClientSettings.UseHardwareAcceleration;
        }

        private void OnNicknameChanged(object sender, TextChangedEventArgs e)
        {
            ClientSettings.Nickname = e.NewTextValue;
        }

        private void OnAdminTokenChanged(object sender, TextChangedEventArgs e)
        {
            ClientSettings.AdminToken = e.NewTextValue;
        }

        private void OnUseHardwareAccelerationChanged(object sender, CheckedChangedEventArgs e)
        {
            ClientSettings.UseHardwareAcceleration = e.Value;
        }
    }
}