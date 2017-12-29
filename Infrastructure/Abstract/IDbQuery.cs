using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CalcExtendedLogics.DataLayer.DbTables;

namespace CalcExtendedLogics.Infrastructure.Abstract
{
    // deriva dalla interfaccia ILoadLevelling, 
    // aggiunge i metodi necessari per l'accesso al database 
    // tramite Entity Framework
    public interface IDbQuery:ILoadLevelling
    {
        IQueryable<LoadLevelling> LoadLevellingTable { get; }       // get full LoadLevelling table from database
        IQueryable<Schema> SchemaTable { get; }                     // get the full Schema table
        Database LlDatabase { get; }                                // ritorna il database rappresentato dal DbContext
        void Save();                                                // usa SaveChanges di Entity Framework                                
        void MassiveSaveData();                                     // usa BulkSaveChanges di Z.EntityFramework.Extensions
        void UpdateData(List<LoadLevelling> list);
        void MassiveUpdateData(List<LoadLevelling> list);           // utilizza BulkUpdate di Z.EntityFramework.Extensions 
        void AddData(List<LoadLevelling> list);
        void MassiveAddData(List<LoadLevelling> list);              // utilizza BulkInserData di Z.EntityFramework.Extensions

    }
}
