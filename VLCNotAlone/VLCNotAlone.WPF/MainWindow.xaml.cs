using LibVLCSharp.Forms.Platforms.WPF;
using LibVLCSharp.Forms.Shared;
using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace VLCNotAlone.WPF
{
    public partial class MainWindow : FormsApplicationPage
    {
        public MainWindow()
        {
            InitializeComponent();
            InitDependencies();
            Forms.Init();
            LoadApplication(new VLCNotAlone.App());
        }

        void InitDependencies()
        {
            var init = new List<Assembly>
            {
                typeof(VideoView).Assembly,
                typeof(VideoViewRenderer).Assembly
            };
        }
    }
}
