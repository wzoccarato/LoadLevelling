using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;

namespace LoadL.Infrastructure.AccessLayer
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
