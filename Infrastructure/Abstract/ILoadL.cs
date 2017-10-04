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
        IQueryable<LoadLevelling> LoadLevellingTable { get; }   // get the full LoadLevelling table
        Database LLDatabase { get; }                              // ritorna il database rappresentato dal DbContext
    }
}
