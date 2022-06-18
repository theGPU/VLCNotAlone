using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VLCNotAlone.Controllers;

namespace VLCNotAlone.Utils
{
    internal static class Localizer
    {
        const string DEFAULT_LANGUAGE = "en_US";
        const string LocalizationFolder = "./Resources/Languagepacks";
        private static Dictionary<string, Dictionary<string, string>> LocTables = new Dictionary<string, Dictionary<string, string>>();
        public static string CurrentLanguage => ConfigController.Language;
        private static string[] AvailableLanguages => LocTables.Select(x => x.Key).ToArray();

        public static Action OnLanguageChanged;

        private static List<MenuItem> LanguagesOptionsItems;

        public static void Init()
        {
            var loadedTables = Directory.EnumerateFiles(LocalizationFolder, "*_*.json", SearchOption.AllDirectories).Select(x => new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(x), File.ReadAllText(x))).Select(x => new KeyValuePair<string, Dictionary<string, string>>(x.Key, JsonConvert.DeserializeObject<Dictionary<string, string>>(x.Value)!));
            foreach (var table in loadedTables)
            {
                if (!LocTables.ContainsKey(table.Key))
                    LocTables.Add(table.Key, new Dictionary<string, string>());

                foreach (var locRow in table.Value)
                    LocTables[table.Key].Add(locRow.Key, locRow.Value);
            }
            FillLanguageMenuitem();

            OnLanguageChanged += OnLanguageChangedDefault;
            OnLanguageChanged?.Invoke();
        }

        private static void FillLanguageMenuitem()
        {
            LanguagesOptionsItems = AvailableLanguages.Select(x => new MenuItem() { Header = $"_{x}", Tag = x, IsChecked = CurrentLanguage == x }).ToList();
            LanguagesOptionsItems.ForEach(x => x.Click += OnChangeLanguage);

            MainWindow.Instance.LanguageMenuitem.ItemsSource = LanguagesOptionsItems;
            MainWindow.Instance.LanguageMenuitem.Items.Refresh();
        }

        private static void OnChangeLanguage(object sender, RoutedEventArgs e)
        {
            ConfigController.Language = (string)((MenuItem)sender).Tag;
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private static void OnLanguageChangedDefault()
        {
            var menus = GetMenuItems(MainWindow.Instance.TopControlMenu);

            foreach (var menu in menus.Where(x => x.Header is string))
                menu.Header = DoStr((string)menu.Header);
        }

        private static List<MenuItem> GetMenuItems(Menu menu)
        {
            var items = new List<MenuItem>();
            foreach (var i in menu.Items.OfType<MenuItem>())
                GetMenuItemItems(i, items);

            return items;
        }

        private static void GetMenuItemItems(MenuItem item, List<MenuItem> items)
        {
            items.Add(item);
            foreach (MenuItem i in item.Items.OfType<MenuItem>())
                GetMenuItemItems(i, items);
        }

        public static FormattableString Do(FormattableString format)
        {
            var str = format.Format;
            var localizedString = LocTables.ContainsKey(CurrentLanguage) && LocTables[CurrentLanguage].ContainsKey(str) ? LocTables[CurrentLanguage][str] : LocTables.ContainsKey(DEFAULT_LANGUAGE) && LocTables[DEFAULT_LANGUAGE].ContainsKey(str) ? LocTables[DEFAULT_LANGUAGE][str] : str;
            var formLocalizedString = FormattableStringFactory.Create(localizedString, format.GetArguments());
            return formLocalizedString;
        }

        public static string DoStr(string str) => LocTables.ContainsKey(CurrentLanguage) && LocTables[CurrentLanguage].ContainsKey(str) ? LocTables[CurrentLanguage][str] : LocTables.ContainsKey(DEFAULT_LANGUAGE) && LocTables[DEFAULT_LANGUAGE].ContainsKey(str) ? LocTables[DEFAULT_LANGUAGE][str] : str;
    }
}
