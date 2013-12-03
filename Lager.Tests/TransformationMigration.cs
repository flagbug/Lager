using System;
using System.Threading.Tasks;

namespace Lager.Tests
{
    public class TransformationMigration<TBefore, TAfter> : SettingsMigration
    {
        private readonly string key;
        private readonly Func<TBefore, TAfter> transformation;

        public TransformationMigration(int revision, string key, Func<TBefore, TAfter> transformation)
            : base(revision)
        {
            this.key = key;
            this.transformation = transformation;
        }

        public override async Task MigrateAsync()
        {
            await this.TransformAsync(key, transformation);
        }
    }
}