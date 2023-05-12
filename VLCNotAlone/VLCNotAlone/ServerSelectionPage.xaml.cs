using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAlone.Networking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerSelectionPage : ContentPage
    {
        public ServerSelectionPage()
        {
            InitializeComponent();
            //Navigation.PushModalAsync(new MainPage());
        }


    }
}