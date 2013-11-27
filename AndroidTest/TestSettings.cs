using Akavache;
using Lager.Portable;

namespace AndroidTest
{
    public class TestSettings : SettingsStorage
    {
        public TestSettings()
            : base("#Settings#", BlobCache.UserAccount)
        { }

        public int Port
        {
            get { return this.GetOrCreate(42); }
            set { this.SetOrCreate(value); }
        }
    }
}