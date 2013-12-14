using Akavache;

namespace Lager.Tests
{
    public class DummySettingsStorage : SettingsStorage
    {
        public DummySettingsStorage(string keyPrefix, IBlobCache cache)
            : base(keyPrefix, cache)
        { }

        public int DummyNumber
        {
            get { return this.GetOrCreate(42); }
            set { this.SetOrCreate(value); }
        }

        public string DummyText
        {
            get { return this.GetOrCreate("Yaddayadda"); }
            set { this.SetOrCreate(value); }
        }
    }
}