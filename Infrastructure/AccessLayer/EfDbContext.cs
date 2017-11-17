using System.Data.Entity;
using CalcExtendedLogics.DataLayer.DbTables;

namespace CalcExtendedLogics.Infrastructure.AccessLayer
{
    class EFDbContext : DbContext
    {
        // quelle che segue disabilita la pluralizzazione di entities e tabelle che viene 
        // effettuata per default da EntityFramework
        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingEntitySetNameConvention>();
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.PluralizingTableNameConvention>();
        }

        public DbSet<LoadLevelling> LoadLevellings { get; set; }
        public DbSet<Schema> Schema { get; set; }
    }
}
