using System.Threading.Tasks;

namespace Lager.Tests
{
    public class RenameIntegerMigration : SettingsMigration
    {
        private readonly string newKey;
        private readonly string previousKey;

        public RenameIntegerMigration(int revision, string previousKey, string newKey)
            : base(revision)
        {
            this.previousKey = previousKey;
            this.newKey = newKey;
        }

        public override async Task MigrateAsync()
        {
            await this.RenameAsync<int>(this.previousKey, this.newKey);
        }
    }
}