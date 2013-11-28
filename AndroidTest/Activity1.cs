using Akavache;
using Android.App;
using Android.OS;
using Android.Widget;

namespace AndroidTest
{
    [Activity(Label = "AndroidTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += (sender, args) => this.StartActivity(typeof(SettingsActivity));
        }

        protected override void OnDestroy()
        {
            BlobCache.Shutdown().Wait();
            base.OnDestroy();
        }
    }
}