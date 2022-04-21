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
    /// Логика взаимодействия для PasswordRequestWindow.xaml
    /// </summary>
    public partial class PasswordRequestWindow : Window
    {
        public string Password => PasswordTextBox.Text;

        public PasswordRequestWindow()
        {
            InitializeComponent();
        }

        private void OnClickCancelButton(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = false;
            Window.GetWindow(this).Close();
        }

        private void OnClickOkButton(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }
    }
}
