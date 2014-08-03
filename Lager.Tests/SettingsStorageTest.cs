using Akavache;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Lager.Tests
{
    public class SettingsStorageTest
    {
        [Fact]
        public void GetOrCreateHitsInternalCacheFirst()
        {
            var cache = Substitute.For<IBlobCache>();
            var settings = new SettingsStorageProxy(cache);

            settings.SetOrCreateProxy(42, "TestNumber");

            settings.GetOrCreateProxy(20, "TestNumber");

            cache.ReceivedWithAnyArgs(1).Insert(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DateTimeOffset?>());
        }

        [Fact]
        public async Task GetOrCreateInsertsDefaultValueIntoBlobCache()
        {
            var cache = new InMemoryBlobCache();
            var settings = new SettingsStorageProxy(cache);

            settings.GetOrCreateProxy(42, "TestNumber");

            Assert.Equal(1, await cache.GetAllKeys().Count());
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
            var testCache = new InMemoryBlobCache();
            await testCache.InsertObject("Storage:DummyNumber", 16);
            await testCache.InsertObject("Storage:DummyText", "Random");

            var cache = Substitute.For<IBlobCache>();
            cache.Get(Arg.Any<string>()).Returns(x => testCache.Get(x.Arg<string>()));
            var settings = new DummySettingsStorage("Storage", cache);

            await settings.InitializeAsync();

            int number = settings.DummyNumber;
            string text = settings.DummyText;

            Assert.Equal(16, number);
            Assert.Equal("Random", text);
            cache.ReceivedWithAnyArgs(2).Get(Arg.Any<string>());
        }

        [Fact]
        public async Task SetOrCreateInsertsValueIntoBlobCache()
        {
            var cache = new InMemoryBlobCache();
            var settings = new SettingsStorageProxy(cache);

            settings.SetOrCreateProxy(42, "TestNumber");

            Assert.Equal(1, await cache.GetAllKeys().Count());
        }

        [Fact]
        public void SetOrCreateWithNullKeyThrowsArgumentNullException()
        {
            var settings = new SettingsStorageProxy();

            Assert.Throws<ArgumentNullException>(() => settings.SetOrCreateProxy(42, null));
        }
    }
}