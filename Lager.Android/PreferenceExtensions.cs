using Android.Preferences;
using Lager.Portable;
using ReactiveUI;
using System;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Lager.Android
{
    public static class PreferenceExtensions
    {
        public static IDisposable BindSetting<TStorage, TSetting>(this Preference preference, TStorage storage,
            Expression<Func<TStorage, TSetting>> setting, Func<Java.Lang.Object, TSetting> parser, Func<TSetting, bool> validator = null) where TStorage : SettingsStorage
        {
            preference.Persistent = false;

            var disposable = new CompositeDisposable();

            string settingName = Reflection.SimpleExpressionToPropertyName(setting);
            Action<object, object> setter = Reflection.GetValueSetterForProperty(storage.GetType(), settingName);

            IDisposable disp1 = preference.PreferenceChanged(parser)
                .Subscribe(x => setter(storage, x));
            disposable.Add(disp1);

            IDisposable disp2 = storage.WhenAnyValue(setting)
                .Subscribe(x => SetAppropriatePreferenceValue(preference, x));
            disposable.Add(disp2);

            return disposable;
        }

        public static IObservable<T> PreferenceChanged<T>(this Preference preference, Func<Java.Lang.Object, T> converter, Func<T, bool> validator = null)
        {
            return Observable.FromEventPattern<Preference.PreferenceChangeEventArgs>(
                    h => preference.PreferenceChange += h,
                    h => preference.PreferenceChange -= h)
                .Select(x => x.EventArgs)
                .Select(x => new { EventArgs = x, Value = converter(x.NewValue) })
                .Select(x => new { EventArgs = x.EventArgs, Value = x.Value, IsValid = validator == null || validator(x.Value) })
                .Do(x => x.EventArgs.Handled = x.IsValid)
                .Where(x => x.IsValid)
                .Select(x => x.Value);
        }

        private static void SetAppropriatePreferenceValue(Preference preference, object value)
        {
            var checkBoxPreference = preference as CheckBoxPreference;
            if (checkBoxPreference != null)
            {
                checkBoxPreference.Checked = (bool)value;
            }

            var editTextPreference = preference as EditTextPreference;
            if (editTextPreference != null)
            {
                editTextPreference.Text = value.ToString();
            }

            var listPreference = preference as ListPreference;
            if (listPreference != null)
            {
                listPreference.Value = value.ToString();
            }
        }
    }
}