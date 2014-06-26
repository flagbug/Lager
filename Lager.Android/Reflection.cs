using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Splat;

namespace Lager.Android
{
    // Taken from https://github.com/reactiveui/ReactiveUI
    internal static class Reflection
    {
        private static readonly MemoizingMRUCache<Tuple<Type, string>, Func<object, object>> propReaderCache =
            new MemoizingMRUCache<Tuple<Type, string>, Func<object, object>>((x, _) =>
            {
                var fi = (x.Item1).GetField(x.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (fi != null)
                {
                    return (fi.GetValue);
                }

                var pi = GetSafeProperty(x.Item1, x.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (pi != null)
                {
                    return (y => pi.GetValue(y, null));
                }

                return null;
            }, 20);

        private static readonly MemoizingMRUCache<Tuple<Type, string>, Action<object, object>> propWriterCache =
            new MemoizingMRUCache<Tuple<Type, string>, Action<object, object>>((x, _) =>
            {
                var fi = (x.Item1).GetField(x.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (fi != null)
                {
                    return (fi.SetValue);
                }

                var pi = GetSafeProperty(x.Item1, x.Item2, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (pi != null)
                {
                    return ((y, v) => pi.SetValue(y, v, null));
                }

                return null;
            }, 20);

        public static Func<TObj, object> GetValueFetcherForProperty<TObj>(string propName)
        {
            var ret = GetValueFetcherForProperty(typeof(TObj), propName);
            return x => (TObj)ret(x);
        }

        public static Func<object, object> GetValueFetcherForProperty(Type type, string propName)
        {
            Contract.Requires(type != null);
            Contract.Requires(propName != null);

            lock (propReaderCache)
            {
                return propReaderCache.Get(Tuple.Create(type, propName));
            }
        }

        public static Action<object, object> GetValueSetterForProperty(Type type, string propName)
        {
            Contract.Requires(type != null);
            Contract.Requires(propName != null);

            lock (propReaderCache)
            {
                return propWriterCache.Get(Tuple.Create(type, propName));
            }
        }

        public static string SimpleExpressionToPropertyName<TObj, TRet>(Expression<Func<TObj, TRet>> property)
        {
            Contract.Requires(property != null);

            string propName = null;

            try
            {
                var propExpr = property.Body as MemberExpression;
                if (propExpr.Expression.NodeType != ExpressionType.Parameter)
                {
                    throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");
                }

                propName = propExpr.Member.Name;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");
            }

            return propName;
        }

        internal static PropertyInfo GetSafeProperty(Type type, string propertyName, BindingFlags flags)
        {
            try
            {
                return type.GetProperty(propertyName, flags);
            }
            catch (AmbiguousMatchException)
            {
                return type.GetProperties(flags).First(pi => pi.Name == propertyName);
            }
        }
    }
}