using Akavache;
using Lager.Portable;

namespace AndroidTest
{
    public class TestSettings : SettingsStorage
    {
        public TestSettings()
            : base("#Settings#", BlobCache.UserAccount)
        { }

        public bool Boolean
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public string Text
        {
            get { return this.GetOrCreate("Default text"); }
            set { this.SetOrCreate(value); }
        }
    }
}