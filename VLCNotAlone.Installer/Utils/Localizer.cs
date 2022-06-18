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
using VLCNotAlone.Installer.Controllers;

namespace VLCNotAlone.Installer.Utils
{
    internal static class Localizer
    {
        const string DEFAULT_LANGUAGE = "en_US";
        const string LocalizationFolder = "./Resources/Languagepacks";
        private static Dictionary<string, Dictionary<string, string>> LocTables = new Dictionary<string, Dictionary<string, string>>();
        public static string CurrentLanguage => ConfigController.Language;
        public static string PrevLanguage = CurrentLanguage;
        private static string[] AvailableLanguages => LocTables.Select(x => x.Key).ToArray();

        public static void Init()
        {
            var loadedTables = ReadLanguagepacksFromResources().Select(x => new KeyValuePair<string, Dictionary<string, string>>(x.Key, JsonConvert.DeserializeObject<Dictionary<string, string>>(x.Value)!));
            foreach (var table in loadedTables)
            {
                if (!LocTables.ContainsKey(table.Key))
                    LocTables.Add(table.Key, new Dictionary<string, string>());

                foreach (var locRow in table.Value)
                    LocTables[table.Key].Add(locRow.Key, locRow.Value);
            }
            FillLanguageMenuitem();

            OnLanguageChanged();
        }

        public static IEnumerable<KeyValuePair<string, string>> ReadLanguagepacksFromResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames().Where(x => x.Contains("Languagepacks"));
            foreach (var name in names)
            {
                using (Stream stream = assembly.GetManifestResourceStream(name))
                using (StreamReader reader = new StreamReader(stream))
                {
                    yield return new KeyValuePair<string, string>(name.Split('.').Reverse().ElementAt(1), reader.ReadToEnd());
                }
            }
        }

        private static void FillLanguageMenuitem()
        {
            MainWindow.Instance.LanguageSelector.ItemsSource = AvailableLanguages;
            MainWindow.Instance.LanguageSelector.Items.Refresh();
            MainWindow.Instance.LanguageSelector.SelectedItem = CurrentLanguage;
        }

        private static readonly Type[] TranslatablesTypes = new Type[] { typeof(Label), typeof(TabItem), typeof(Button), typeof(CheckBox), typeof(TextBox)};
        public static void OnLanguageChanged()
        {
            var findedLogicalNodes = new Queue<object>();
            var logicalNodesToTranslate = new Queue<DependencyObject>();
            LogicalTreeHelper.GetChildren(MainWindow.Instance).Cast<object>().ToList().ForEach(x => findedLogicalNodes.Enqueue(x));
            while (findedLogicalNodes.TryDequeue(out var node))
            {
                if (node is DependencyObject nodeDepObject)
                {
                    LogicalTreeHelper.GetChildren(nodeDepObject).Cast<object>().ToList().ForEach(x => findedLogicalNodes.Enqueue(x));
                    if (TranslatablesTypes.Any(x => node.GetType() == x))
                        logicalNodesToTranslate.Enqueue(nodeDepObject);
                }
            }

            while (logicalNodesToTranslate.TryDequeue(out var element))
            {
                if (element is Label label && label.Content is string)
                    label.Content = GetTranslateFrom(PrevLanguage, (string)label.Content);
                else if (element is TabItem tabItem && tabItem.Header is string)
                    tabItem.Header = GetTranslateFrom(PrevLanguage, (string)tabItem.Header);
                else if (element is Button button && button.Content is string)
                    button.Content = GetTranslateFrom(PrevLanguage, (string)button.Content);
                else if (element is CheckBox checkBox && checkBox.Content is string)
                    checkBox.Content = GetTranslateFrom(PrevLanguage, (string)checkBox.Content);
                else if (element is TextBox textBox)
                    textBox.Text = GetTranslateFrom(PrevLanguage, (string)textBox.Text);
            }
            PrevLanguage = CurrentLanguage;
        }

        public static string GetTranslateFrom(string fromLang, string str)
        {
            var engText = LocTables.ContainsKey(fromLang) && LocTables[fromLang].ContainsValue(str) ? LocTables[fromLang].First(x => x.Value == str).Key : str;
            return DoStr(engText);
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
