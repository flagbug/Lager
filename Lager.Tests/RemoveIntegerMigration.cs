using System.Threading.Tasks;

namespace Lager.Tests
{
    public class RemoveIntegerMigration : SettingsMigration
    {
        private readonly string key;

        public RemoveIntegerMigration(string key)
            : base(1)
        {
            this.key = key;
        }

        public override async Task MigrateAsync()
        {
            await this.RemoveAsync<int>(key);
        }
    }
}