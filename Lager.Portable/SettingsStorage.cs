using Akavache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lager.Portable
{
    public abstract class SettingsStorage : INotifyPropertyChanged
    {
        private readonly IBlobCache blobCache;
        private readonly Dictionary<string, object> cache;
        private readonly ReaderWriterLockSlim cacheLock;
        private readonly string keyPrefix;

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

        protected T GetOrCreate<T>(T defaultValue, [CallerMemberName] string key = null)
        {
            if (key == null)
                throw new InvalidOperationException("Key is null!");

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