using Akavache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lager
{
    /// <summary>
    /// The base class for every settings storage.
    /// Provides methods for saving/retrieving settings.
    /// </summary>
    public abstract class SettingsStorage : INotifyPropertyChanged
    {
        private readonly IBlobCache blobCache;
        private readonly Dictionary<string, object> cache;
        private readonly ReaderWriterLockSlim cacheLock;
        private readonly string keyPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStorage"/> class.
        /// </summary>
        /// <param name="keyPrefix">This value will be used as prefix for all settings keys.
        /// It should be reasonably unique, so that it doesn't collide with other keys in the same <see cref="IBlobCache"/>.</param>
        /// <param name="cache">An <see cref="IBlobCache"/> implementation where you want your settings to be stored.</param>
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
        /// Gets the value for the specified key, or, if the value doesn't exist, saves the <paramref name="defaultValue"/> and returns it.
        /// </summary>
        /// <typeparam name="T">The type of the value to get or create.</typeparam>
        /// <param name="defaultValue">The default value, if no value is saved yet.</param>
        /// <param name="key">The key of the setting. Automatically set through the <see cref="CallerMemberNameAttribute"/>.</param>
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

            return this.blobCache.GetOrCreateObject(string.Format("{0}:{1}", this.keyPrefix, key), () => defaultValue).Wait();
        }

        /// <summary>
        /// Overwrites the existing value or creates a new settings entry.
        /// The value is serialized via the Json.Net serializer.
        /// </summary>
        /// <typeparam name="T">The type of the value to set or create.</typeparam>
        /// <param name="value">The value to be set or created.</param>
        /// <param name="key">The key of the setting. Automatically set through the <see cref="CallerMemberNameAttribute"/>.</param>
        protected void SetOrCreate<T>(T value, [CallerMemberName] string key = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            this.cacheLock.EnterWriteLock();

            this.cache.Remove(key);
            this.cache.Add(key, value);

            this.cacheLock.ExitWriteLock();

            this.blobCache.InsertObject(string.Format("{0}:{1}", this.keyPrefix, key), value);

            this.OnPropertyChanged(key);
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}