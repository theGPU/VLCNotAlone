using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace VLCNotAlone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Action OnApplicationExit;

        private void Application_Exit(object sender, SessionEndingCancelEventArgs e)
        {
            OnApplicationExit?.Invoke();

            base.OnSessionEnding(e);
        }
    }
}
