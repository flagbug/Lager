using Akavache;
using Android.App;
using Android.OS;
using Android.Preferences;
using Lager.Android;

namespace AndroidTest
{
    [Activity(Label = "My Activity")]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.AddPreferencesFromResource(Resource.Layout.Settings);

            BlobCache.ApplicationName = "Settings Test App";

            var storage = new TestSettings();

            var portPreference = this.FindPreference("pref_port");
            portPreference.BindSetting(storage, x => x.Port, x => int.Parse(x.ToString()));
        }
    }
}