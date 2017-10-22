using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;

namespace LoadL.Infrastructure.Abstract
{
    // deriva dalla interfaccia ILoadLevelling, 
    // aggiunge i metodi necessari per l'accesso al database 
    // tramite Entity Framework
    public interface IDbQuery:ILoadLevelling
    {
        IQueryable<LoadLevelling> LoadLevellingTable { get; }                                               // get full LoadLevelling table from database
        IQueryable<Schema> SchemaTable { get; }                                                             // get the full Schema table
        Database LlDatabase { get; }                                                                        // ritorna il database rappresentato dal DbContext
        void Save();
    }
}
