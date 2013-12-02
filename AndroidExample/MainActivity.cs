using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;

namespace AndroidExample
{
    [Activity(Label = "AndroidExample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            var button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += (sender, args) => this.StartActivity(typeof(SettingsActivity));
        }

        protected override void OnDestroy()
        {
            BlobCache.Shutdown().Wait();
            base.OnDestroy();
        }
    }
}