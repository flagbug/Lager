using System.Threading.Tasks;
using Xunit;

namespace Lager.Tests
{
    public class MigrationTest
    {
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
        public async Task TransformationSmokeTest()
        {
            var storage = new SettingsStorageProxy();
            storage.SetOrCreateProxy(42, "Setting");

            await storage.MigrateAsync(new[] { new TransformationMigration<int, string>(1, "Setting", x => x.ToString()) });

            string value = storage.GetOrCreateProxy("Bla", "Setting");

            Assert.Equal("42", value);
        }
    }
}