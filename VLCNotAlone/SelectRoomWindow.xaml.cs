using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VLCNotAlone
{
    /// <summary>
    /// Логика взаимодействия для SelectRoomWindow.xaml
    /// </summary>
    public partial class SelectRoomWindow : Window
    {
        public SelectRoomWindow()
        {
            InitializeComponent();
            MainWindow.Instance.clientApi.RequestRoomsList();
            MainWindow.Instance.clientApi.OnRoomsList += (rooms) => this.Dispatcher.Invoke(() => RoomsList.ItemsSource = rooms);
        }

        private void OnClickConnectButton(object sender, RoutedEventArgs e)
        {
            var room = (sender as Button).DataContext as VLCNotAloneShared.POCO.Room;
            if (room.HasPassword)
            {
                var passwordRequestWindow = new PasswordRequestWindow();
                if (passwordRequestWindow.ShowDialog() == true)
                    MainWindow.Instance.clientApi.ConnectToRoom(room.Name, passwordRequestWindow.Password);
            } else
            {
                MainWindow.Instance.clientApi.ConnectToRoom(room.Name, "");
            }
        }
    }
}
