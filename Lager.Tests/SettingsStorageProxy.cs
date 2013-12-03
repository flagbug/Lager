using Akavache;

namespace Lager.Tests
{
    public class SettingsStorageProxy : SettingsStorage
    {
        public SettingsStorageProxy(IBlobCache blobCache = null)
            : base("#Storage#", blobCache ?? new TestBlobCache())
        { }

        public T GetOrCreateProxy<T>(T defaultValue, string key)
        {
            return this.GetOrCreate(defaultValue, key);
        }

        public void SetOrCreateProxy<T>(T value, string key)
        {
            this.SetOrCreate(value, key);
        }
    }
}