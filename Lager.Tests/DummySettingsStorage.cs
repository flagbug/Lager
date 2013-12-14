using Akavache;

namespace Lager.Tests
{
    public class DummySettingsStorage : SettingsStorage
    {
        public DummySettingsStorage(IBlobCache cache)
            : base("__DUMMYSTORAGE__", cache)
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