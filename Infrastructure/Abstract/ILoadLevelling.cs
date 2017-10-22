using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;

namespace LoadL.Infrastructure.Abstract
{
    // interfaccia per la implementazione delle query di base
    public interface ILoadLevelling
    {
        List<LoadLevellingWork> LoadLevellingWorkTable { get; set; }                                        // LoadLevellingWorkTable è l'oggetto utilizzato internamente dal programma
        IEnumerable<string> GetDistinctPlanBu();                                                            // ritorna una lista distinct dei plan_bu (identificativo della produzione)
        IEnumerable<string> GetDistinctFlagHr(string planbu);                                               // dato un plan_bu ritorna una lista distinct dei flag_hr (flag high rotation)
        IEnumerable<string> GetDistinctProductionCategory(string planbu, string flaghr);                    // dato plan_bu e flag_hr ritorna tutti i record raggruppati per production_category
        List<LoadLevellingWork> ListByWeekAndPriority(string planbu, string flaghr, string prodcat);    // tutti i record, dati plan_bu, flag_hr, production_category. sortati per week_plan e poi per priority

    }
}
