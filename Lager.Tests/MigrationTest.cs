using Akavache;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lager.Tests
{
    public class MigrationTest
    {
        public async static Task<T> ThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                Assert.Throws<T>(() => { }); // Use xUnit's default behavior.
            }

            catch (T exception)
            {
                return exception;
            }

            return null;
        }

        [Fact]
        public async Task MigrateAsyncThrowsOnEmptyEnumerable()
        {
            var storage = new SettingsStorageProxy();

            await ThrowsAsync<ArgumentException>(() => storage.MigrateAsync(Enumerable.Empty<SettingsMigration>()));
        }

        [Fact]
        public async Task RemoveAsyncSmokeTest()
        {
            var storage = new SettingsStorageProxy();
            storage.SetOrCreateProxy(1, "Setting1");

            await storage.MigrateAsync(new[] { new RemoveIntegerMigration("Setting1") });

            int value = storage.GetOrCreateProxy(42, "Setting1");

            Assert.Equal(42, value);
        }

        [Fact]
        public async Task RenameSmokeTest()
        {
            var storage = new SettingsStorageProxy();
            storage.SetOrCreateProxy(42, "Setting1");

            await storage.MigrateAsync(new[] { new RenameIntegerMigration(1, "Setting1", "Setting2") });

            int value = storage.GetOrCreateProxy(1, "Setting2");

            Assert.Equal(42, value);
        }

        [Fact]
        public async Task TransformationRemovesOldObject()
        {
            var blobCache = new TestBlobCache();
            var storage = new SettingsStorageProxy(blobCache);
            storage.SetOrCreateProxy(42, "Setting");

            await storage.MigrateAsync(new[] { new TransformationMigration<int, string>(1, "Setting", x => x.ToString()) });

            Assert.Throws<KeyNotFoundException>(() => blobCache.GetObjectAsync<int>("Setting").Wait());
        }

        [Fact]
        public async Task TransformationSmokeTest()
        {
            var storage = new SettingsStorageProxy();
            storage.SetOrCreateProxy(42, "Setting");

            await storage.MigrateAsync(new[] { new TransformationMigration<int, string>(1, "Setting", x => x.ToString()) });

            string value = storage.GetOrCreateProxy("Bla", "Setting");

            Assert.Equal("42", value);
        }

        [Fact]
        public async Task TransformationsShouldHaveDistinctRevisions()
        {
            var storage = new SettingsStorageProxy();

            var migration1 = new Mock<SettingsMigration>(1);
            var migration2 = new Mock<SettingsMigration>(1);

            var migrations = new[] { migration1.Object, migration2.Object };

            await ThrowsAsync<ArgumentException>(() => storage.MigrateAsync(migrations));
        }
    }
}