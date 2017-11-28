using System.Collections.Generic;
using System.Linq;
using CalcExtendedLogics.DataLayer.DbTables;
using CalcExtendedLogics.Infrastructure.Abstract;

namespace CalcExtendedLogics.CalcExtendedLogics
{
    public class LlWrapperClass : ILoadLevelling
    {
        #region ctor

        public LlWrapperClass(List<LoadLevellingWork> loadLevelling)
        {
            LoadLevellingWorkTable = loadLevelling;
        }

        #endregion

        #region implementazione interfaccia ILoadLevelling

        public List<LoadLevellingWork> LoadLevellingWorkTable { get; set; }

        public IEnumerable<string> GetDistinctFlagHr(string planbu) => 
            (from rec in LoadLevellingWorkTable
             where rec.PLAN_BU == planbu
             select rec.FLAG_HR).Distinct().ToList();

        public IEnumerable<string> GetDistinctPlanBu() =>
            (from rec in LoadLevellingWorkTable
             select rec.PLAN_BU).Distinct().ToList();


        public IEnumerable<string> GetDistinctProductionCategory(string planbu, string flaghr) =>
            (from rec in LoadLevellingWorkTable
             where rec.PLAN_BU == planbu && rec.FLAG_HR == flaghr
             select rec.PRODUCTION_CATEGORY).Distinct().ToList();

        public List<LoadLevellingWork> ListByWeekAndPriority(string planbu, string flaghr, string prodcat) =>
            (from rec in LoadLevellingWorkTable
             where rec.PLAN_BU == planbu && rec.FLAG_HR == flaghr && rec.PRODUCTION_CATEGORY == prodcat
             //&& (rec.Week == "201707" || rec.Week == "201708" || rec.Week == "201709")
             orderby rec.Week, rec.Priority
             select rec).ToList();

        #endregion
    }
}
