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

        protected async Task RenameAsync<T>(string previousKey, string newKey)
        {
            T value = await this.blobCache.GetObjectAsync<T>(this.CreateKey(previousKey));

            await this.blobCache.InvalidateObject<T>(this.CreateKey(previousKey));

            await this.blobCache.InsertObject(this.CreateKey(newKey), value);
        }

        protected async Task TransformAsync<TBefore, TAfter>(string key, Func<TBefore, TAfter> transformation)
        {
            key = this.CreateKey(key);

            TBefore before = await this.blobCache.GetObjectAsync<TBefore>(key);

            TAfter after = transformation(before);

            await this.RemoveAsync<TBefore>(key);

            await this.blobCache.InsertObject(key, after);
        }

        private string CreateKey(string key)
        {
            return string.Format("{0}:{1}", this.keyPrefix, key);
        }
    }
}