using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PasswordEnterPage : ContentPage
    {
        public string Password { get; private set; } = "";

        public PasswordEnterPage()
        {
            InitializeComponent();
        }

        private void OnOkButtonClicked(object sender, EventArgs e)
        {
            Password = PasswordEntry.Text;
            Navigation.PopModalAsync();
        }

        private void OnCancelButtonCLicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}