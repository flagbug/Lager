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
        /// <summary>
        /// Two-way binds a <see cref="Preference"/> to a setting in a <see cref="SettingsStorage"/> container.
        /// </summary>
        /// <typeparam name="TPreference">The type of the <see cref="Preference"/>.</typeparam>
        /// <typeparam name="TStorage">The type of the <see cref="SettingsStorage"/>.</typeparam>
        /// <typeparam name="TSProp">The type of the setting in the <see cref="SettingsStorage"/>.</typeparam>
        /// <typeparam name="TPProp">The type of the property of the <see cref="Preference"/>.</typeparam>
        /// <param name="preference">The instance of the <see cref="Preference"/> to bind.</param>
        /// <param name="storage">The instance of the <see cref="SettingsStorage"/> to bind.</param>
        /// <param name="settingProperty">An expression indication the setting that is bound on the <see cref="SettingsStorage"/>.</param>
        /// <param name="prefProperty">An expression indicating the property that is bound on the <see cref="Preference"/>.</param>
        /// <param name="preferencePropertyToSettingConverter">
        /// A function to convert the <see cref="Preference"/> property to the setting.
        /// Convert a <see cref="Java.Lang.Object"/> back to your setting type with this function.
        /// </param>
        /// <param name="settingToPreferencePropertyConverter">
        /// An optional function to convert the setting to the <see cref="Preference"/> property.
        /// Useful when you bind an enum to a <see cref="ListPreference"/>, where you want the enum to be converted to a string.
        /// </param>
        /// <param name="validator">
        /// An optional function to determine whether the value of the setting is valid or not.
        /// Return true if you want the value to be saved in your <see cref="SettingsStorage"/>,
        /// return false if you want to discard the value.
        /// </param>
        /// <returns>An <see cref="IDisposable"/> that disconnects the binding when disposed.</returns>
        public static IDisposable BindToSetting<TPreference, TStorage, TSProp, TPProp>(
            this TPreference preference,
            TStorage storage,
            Expression<Func<TStorage, TSProp>> settingProperty,
            Expression<Func<TPreference, TPProp>> prefProperty,
            Func<Java.Lang.Object, TSProp> preferencePropertyToSettingConverter,
            Func<TSProp, TPProp> settingToPreferencePropertyConverter = null,
            Func<TSProp, bool> validator = null)
            where TPreference : Preference
            where TStorage : SettingsStorage
        {
            preference.Persistent = false;

            var disposable = new CompositeDisposable();

            string settingName = GetPropertyNameSafe(settingProperty);

            Action<object, object> setter = Reflection.GetValueSetterForProperty(storage.GetType(), settingName);

            IDisposable disp1 = preference.PreferenceChanged(preferencePropertyToSettingConverter, validator)
                .Subscribe(x => setter(storage, x));
            disposable.Add(disp1);

            string preferencePropertyName = Reflection.SimpleExpressionToPropertyName(prefProperty);
            Action<object, object> preferenceSetter = Reflection.GetValueSetterForProperty(preference.GetType(), preferencePropertyName);

            IDisposable disp2 = storage.WhenAnyValue(settingProperty)
                .Select(x => settingToPreferencePropertyConverter == null ? (object)x : settingToPreferencePropertyConverter(x))
                .Subscribe(x => preferenceSetter(preference, x));
            disposable.Add(disp2);

            return disposable;
        }

        private static string GetPropertyNameSafe<TObj, TRet>(Expression<Func<TObj, TRet>> expression)
        {
            // Enums have a different expression tree
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                var unaryExpr = (UnaryExpression)expression.Body;
                var expr = (MemberExpression)unaryExpr.Operand;

                return expr.Member.Name;
            }

            return Reflection.SimpleExpressionToPropertyName(expression);
        }

        private static IObservable<T> PreferenceChanged<T>(this Preference preference, Func<Java.Lang.Object, T> converter, Func<T, bool> validator = null)
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