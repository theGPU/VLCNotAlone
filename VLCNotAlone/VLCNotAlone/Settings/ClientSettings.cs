using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VLCNotAlone.Settings
{
    internal static class ClientSettings
    {
        public static string Nickname
        { 
            get { if (Application.Current.Properties.TryGetValue(nameof(Nickname), out var nickname)) return (string)nickname; else return "New user"; }
            set { if (Nickname == value) return; Application.Current.Properties[nameof(Nickname)] = value; Save(); }
        }

        public static string AdminToken
        {
            get { if (Application.Current.Properties.TryGetValue(nameof(AdminToken), out var adminToken)) return (string)adminToken; else return ""; }
            set { if (AdminToken == value) return; Application.Current.Properties[nameof(AdminToken)] = value; Save(); }
        }

        public static bool UseHardwareAcceleration
        {
            get { if (Application.Current.Properties.TryGetValue(nameof(UseHardwareAcceleration), out var useHardwareAcceleration)) return (bool)useHardwareAcceleration; else return false; }
            set { if (UseHardwareAcceleration == value) return; Application.Current.Properties[nameof(UseHardwareAcceleration)] = value; Save(); }
        }

        public static void Save()
        {
            Application.Current.SavePropertiesAsync(); //always run itself in ui thread, threadsafe
        }
    }
}
