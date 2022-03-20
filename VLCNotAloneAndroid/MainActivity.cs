using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using LibVLCSharp.Shared;
using Android.Views;
using Android.Widget;
using System;
using VideoView = LibVLCSharp.Platforms.Android.VideoView;
using Android.Content.PM;

namespace VLCNotAloneAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat.NoActionBar", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]
    public class MainActivity : AppCompatActivity
    {
        //VideoView _videoView;
        //LibVLC _libVLC;
        //MediaPlayer _mediaPlayer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.RequestFeature(WindowFeatures.NoTitle);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        /*
        protected override void OnResume()
        {
            base.OnResume();
            Core.Initialize();
            _libVLC = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true
            };
            _videoView = new VideoView(this) { MediaPlayer = _mediaPlayer };
            AddContentView(_videoView, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
            using var media = new Media(_libVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
            _videoView.MediaPlayer.Play(media);
        }
        protected override void OnPause()
        {
            base.OnPause();
            _videoView.Dispose();
            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }
        */
    }
}