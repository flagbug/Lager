using Akavache;
using Android.App;
using Android.OS;
using Android.Preferences;
using Lager.Android;
using System;
using System.Linq;

namespace AndroidTest
{
    [Activity(Label = "My Activity")]
    public class SettingsActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.AddPreferencesFromResource(Resource.Layout.Settings);

            var storage = new TestSettings(BlobCache.UserAccount);

            var textPreference = (EditTextPreference)this.FindPreference("pref_text");
            textPreference.BindToSetting(storage, x => x.Text, x => x.Text, x => x.ToString());

            var boolPreference = (CheckBoxPreference)this.FindPreference("pref_bool");
            boolPreference.BindToSetting(storage, x => x.Boolean, x => x.Checked, x => bool.Parse((string)x));

            var listPreference = (ListPreference)this.FindPreference("pref_list");
            listPreference.SetEntryValues(Enum.GetNames(typeof(ListEnum)));
            listPreference.BindToSetting(storage, x => x.ListItem, x => x.Value, x => Enum.Parse(typeof(ListEnum), (string)x), x => x.ToString());

            var validationPreference = (EditTextPreference)this.FindPreference("pref_validation");
            validationPreference.EditText.TextChanged += (sender, args) =>
            {
                int value = int.Parse(new string(args.Text.ToArray()));

                if (!IsValid(value))
                {
                    validationPreference.EditText.Error = "Value must be between 100 and 200!";
                }
            };
            validationPreference.BindToSetting(storage, x => x.Number, x => x.Text, x => int.Parse(x.ToString()), x => x.ToString(), x => IsValid(x));
        }

        private static bool IsValid(int value)
        {
            return value > 100 && value < 200;
        }
    }
}