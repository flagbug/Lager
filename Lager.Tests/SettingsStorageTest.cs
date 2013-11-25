using Akavache;
using Moq;
using System;
using System.Linq;
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
        public void SetOrCreateInsertsValueIntoBlobCache()
        {
            var cache = new TestBlobCache();
            var settings = new SettingsStorageProxy(cache);

            settings.SetOrCreateProxy(42, "TestNumber");

            Assert.Equal(1, cache.GetAllKeys().Count());
        }
    }
}