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
    public interface ILoadL
    {
        IQueryable<LoadLevelling> LoadLevellingTable { get; }                                               // get full LoadLevelling table from database
        IList<LoadLevellingWork> LoadLevellingWorkTable { get; }                                            // esegue un cast di LoadLevellingTable LoadLevellingWorkTable
        IEnumerable<string> GetDistinctPlanBu();                                                            // ritorna una lista distinct dei plan_bu (identificativo della produzione)
        IEnumerable<string> GetDistinctFlagHr(string planbu);                                               // dato un plan_bu ritorna una lista distinct dei flag_hr (flag high rotation)
        IEnumerable<string> GetDistinctProductionCategory(string planbu, string flaghr);                    // dato plan_bu e flag_hr ritorna tutti i record raggruppati per production_category
        IList<LoadLevellingWork> ListByProductionCategory(string planbu, string flaghr, string prodcat);    // tutti i record, dati plan_bu, flag_hr, production_category. sortati per week_plan e poi per priority
        IQueryable<Schema> SchemaTable { get; }                                                             // get the full Schema table
        Database LLDatabase { get; }                                                                        // ritorna il database rappresentato dal DbContext

    }
}
