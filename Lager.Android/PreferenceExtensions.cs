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
        public static IDisposable BindSetting<TPreference, TStorage, TSProp, TPProp>(
            this TPreference preference,
            TStorage storage,
            Expression<Func<TStorage, TSProp>> setting,
            Expression<Func<TPreference, TPProp>> prefProperty,
            Func<Java.Lang.Object, TSProp> preferencePropertyToSettingConverter,
            Func<TSProp, TPProp> settingToPreferencePropertyConverter,
            Func<TSProp, bool> validator = null)
            where TPreference : Preference
            where TStorage : SettingsStorage
        {
            preference.Persistent = false;

            var disposable = new CompositeDisposable();

            string settingName = Reflection.SimpleExpressionToPropertyName(setting);
            Action<object, object> setter = Reflection.GetValueSetterForProperty(storage.GetType(), settingName);

            IDisposable disp1 = preference.PreferenceChanged(preferencePropertyToSettingConverter)
                .Subscribe(x => setter(storage, x));
            disposable.Add(disp1);

            string preferencePropertyName = Reflection.SimpleExpressionToPropertyName(prefProperty);
            Action<object, object> preferenceSetter = Reflection.GetValueSetterForProperty(preference.GetType(), preferencePropertyName);

            IDisposable disp2 = storage.WhenAnyValue(setting)
                .Subscribe(x => preferenceSetter(preference, x));
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
                .Select(x => new { x.EventArgs, x.Value, IsValid = validator == null || validator(x.Value) })
                .Do(x => x.EventArgs.Handled = x.IsValid)
                .Where(x => x.IsValid)
                .Select(x => x.Value);
        }
    }
}