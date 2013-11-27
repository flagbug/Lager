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

            IDisposable disp2 = storage.WhenAnyValue(setting, x => x.ToString())
                .Subscribe(x =>
                {
                    var manager = PreferenceManager.GetDefaultSharedPreferences(preference.Context);
                    manager.Edit().PutString(preference.Key, x).Apply();
                });
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
                .Do(x => x.EventArgs.Handled = validator == null || validator(x.Value))
                .Select(x => x.Value);
        }
    }
}