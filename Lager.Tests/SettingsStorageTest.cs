using Akavache;
using Moq;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lager.Tests
{
    public class SettingsStorageTest
    {
        [Fact]
        public void GetOrCreateHitsInternalCacheFirst()
        {
            var cache = new Mock<IBlobCache>();
            var settings = new SettingsStorageProxy(cache.Object);

            settings.SetOrCreateProxy(42, "TestNumber");

            settings.GetOrCreateProxy(20, "TestNumber");

            cache.Verify(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>()), Times.Once);
        }

        [Fact]
        public void GetOrCreateInsertsDefaultValueIntoBlobCache()
        {
            var cache = new TestBlobCache();
            var settings = new SettingsStorageProxy(cache);

            settings.GetOrCreateProxy(42, "TestNumber");

            Assert.Equal(1, cache.GetAllKeys().Count());
        }

        [Fact]
        public void GetOrCreateSetsDefaultValueIfNoneExists()
        {
            var settings = new SettingsStorageProxy();
            settings.GetOrCreateProxy(42, "TestNumber");

            Assert.Equal(42, settings.GetOrCreateProxy(20, "TestNumber"));
        }

        [Fact]
        public void GetOrCreateSmokeTest()
        {
            var settings = new SettingsStorageProxy();
            settings.SetOrCreateProxy(42, "TestNumber");

            Assert.Equal(42, settings.GetOrCreateProxy(20, "TestNumber"));
        }

        [Fact]
        public void GetOrCreateWithNullKeyThrowsArgumentNullException()
        {
            var settings = new SettingsStorageProxy();

            Assert.Throws<ArgumentNullException>(() => settings.GetOrCreateProxy(42, null));
        }

        [Fact]
        public async Task InitializeAsyncLoadsValuesIntoCache()
        {
            var testCache = new TestBlobCache();
            testCache.InsertObject("DummyNumber", 16);
            testCache.InsertObject("DummyText", "Random");

            var cache = new Mock<IBlobCache>();
            cache.Setup(x => x.GetAsync(It.IsAny<string>())).Returns<string>(testCache.GetAsync);
            var settings = new DummySettingsStorage(cache.Object);

            await settings.InitializeAsync();

            int number = settings.DummyNumber;
            string text = settings.DummyText;

            Assert.Equal(16, number);
            Assert.Equal("Random", text);
            cache.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public void SetOrCreateInsertsValueIntoBlobCache()
        {
            var cache = new TestBlobCache();
            var settings = new SettingsStorageProxy(cache);

            settings.SetOrCreateProxy(42, "TestNumber");

            Assert.Equal(1, cache.GetAllKeys().Count());
        }

        [Fact]
        public void SetOrCreateWithNullKeyThrowsArgumentNullException()
        {
            var settings = new SettingsStorageProxy();

            Assert.Throws<ArgumentNullException>(() => settings.SetOrCreateProxy(42, null));
        }
    }
}