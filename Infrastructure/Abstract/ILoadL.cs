using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;

namespace LoadL.Infrastructure.Abstract
{
    public interface ILoadL
    {
        IQueryable<LoadLevelling> LoadLevellingTable { get; }           // get full LoadLevelling table
        IQueryable<Schema> SchemaTable { get; }                   // get full Schema table 
        Database LLDatabase { get; }                              // ritorna il database rappresentato dal DbContext
    }
}
