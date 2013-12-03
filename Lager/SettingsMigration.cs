using Akavache;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lager
{
    public abstract class SettingsMigration
    {
        private IBlobCache blobCache;
        private string keyPrefix;

        protected SettingsMigration(int revision)
        {
            if (revision < 0)
                throw new ArgumentOutOfRangeException("revision", "Revision has to be greater or equal 0");

            this.Revision = revision;
        }

        internal int Revision { get; private set; }

        public abstract Task MigrateAsync();

        internal void Initialize(string keyPrefix, IBlobCache blobCache)
        {
            this.keyPrefix = keyPrefix;
            this.blobCache = blobCache;
        }

        protected async Task RemoveAsync<T>(string key)
        {
            await this.blobCache.InvalidateObject<T>(this.CreateKey(key));
        }

        private string CreateKey(string key)
        {
            return string.Format("{0}:{1}", this.keyPrefix, key);
        }
    }
}