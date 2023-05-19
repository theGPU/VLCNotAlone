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
    public partial class ServerOverviewPage : ContentPage
    {
        public ServerOverviewPage()
        {
            InitializeComponent();
        }

        private void OnRoomSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }
    }
}