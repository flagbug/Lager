using Akavache;
using Lager.Portable;

namespace AndroidTest
{
    public enum ListEnum
    {
        Item1,
        Item2,
        Item3
    }

    public class TestSettings : SettingsStorage
    {
        public TestSettings(IBlobCache blobCache)
            : base("#Settings#", blobCache)
        { }

        public bool Boolean
        {
            get { return this.GetOrCreate(true); }
            set { this.SetOrCreate(value); }
        }

        public ListEnum ListItem
        {
            get { return this.GetOrCreate(ListEnum.Item2); }
            set { this.SetOrCreate(value); }
        }

        public int Number
        {
            get { return this.GetOrCreate(42); }
            set { this.SetOrCreate(value); }
        }

        public string Text
        {
            get { return this.GetOrCreate("Default text"); }
            set { this.SetOrCreate(value); }
        }
    }
}