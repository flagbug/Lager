using Akavache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lager
{
    /// <summary>
    /// The base class for every settings storage. Provides methods for saving/retrieving settings.
    /// </summary>
    public abstract class SettingsStorage : INotifyPropertyChanged
    {
        private readonly IBlobCache blobCache;
        private readonly Dictionary<string, object> cache;
        private readonly ReaderWriterLockSlim cacheLock;
        private readonly string keyPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStorage" /> class.
        /// </summary>
        /// <param name="keyPrefix">
        /// This value will be used as prefix for all settings keys. It should be reasonably unique,
        /// so that it doesn't collide with other keys in the same <see cref="IBlobCache" />.
        /// </param>
        /// <param name="cache">
        /// An <see cref="IBlobCache" /> implementation where you want your settings to be stored.
        /// </param>
        protected SettingsStorage(string keyPrefix, IBlobCache cache)
        {
            if (String.IsNullOrWhiteSpace(keyPrefix))
                throw new ArgumentException("Invalid key prefix", "keyPrefix");

            if (cache == null)
                throw new ArgumentNullException("cache");

            this.keyPrefix = keyPrefix;
            this.blobCache = cache;

            this.cache = new Dictionary<string, object>();
            this.cacheLock = new ReaderWriterLockSlim();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Loads every setting in this storage into the internal cache, or, if the value doesn't
        /// exist in the storage, initializes it with its default value. You dont HAVE to call this
        /// method, but it's handy for applications with a high number of settings where you want to
        /// load all settings on startup at once into the internal cache and not one-by-one at each request.
        /// </summary>
        public Task InitializeAsync()
        {
            return Task.Run(() =>
            {
                foreach (var property in this.GetType().GetRuntimeProperties())
                {
                    property.GetValue(this);
                }
            });
        }

        /// <summary>
        /// Gets the value for the specified key, or, if the value doesn't exist, saves the
        /// <paramref name="defaultValue" /> and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the value to get or create.</typeparam>
        /// <param name="defaultValue">The default value, if no value is saved yet.</param>
        /// <param name="key">
        /// The key of the setting. Automatically set through the <see
        /// cref="CallerMemberNameAttribute" />.
        /// </param>
        protected T GetOrCreate<T>(T defaultValue, [CallerMemberName] string key = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            this.cacheLock.EnterReadLock();

            try
            {
                object value;

                if (this.cache.TryGetValue(key, out value))
                {
                    return (T)value;
                }
            }

            finally
            {
                this.cacheLock.ExitReadLock();
            }

            T returnValue = this.blobCache.GetOrCreateObject(string.Format("{0}:{1}", this.keyPrefix, key), () => defaultValue)
                .Do(x => this.AddToInternalCache(key, x)).Wait();

            return returnValue;
        }

        /// <summary>
        /// Overwrites the existing value or creates a new settings entry. The value is serialized
        /// via the Json.Net serializer.
        /// </summary>
        /// <typeparam name="T">The type of the value to set or create.</typeparam>
        /// <param name="value">The value to be set or created.</param>
        /// <param name="key">
        /// The key of the setting. Automatically set through the <see
        /// cref="CallerMemberNameAttribute" />.
        /// </param>
        protected void SetOrCreate<T>(T value, [CallerMemberName] string key = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            this.AddToInternalCache(key, value);

            // Fire and forget, we retrieve the value from the in-memory cache from now on
            this.blobCache.InsertObject(string.Format("{0}:{1}", this.keyPrefix, key), value).Subscribe();

            this.OnPropertyChanged(key);
        }

        private void AddToInternalCache(string key, object value)
        {
            this.cacheLock.EnterWriteLock();

            this.cache[key] = value;

            this.cacheLock.ExitWriteLock();
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}