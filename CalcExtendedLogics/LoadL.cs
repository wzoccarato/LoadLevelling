using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure.Abstract;

namespace LoadL.CalcExtendedLogics
{
    public class LoadL:ILoadLevelling
    {
        #region ctor

        public LoadL(List<LoadLevellingWork> loadLevelling)
        {
            LoadLevellingWorkTable = loadLevelling;
        }
        #endregion

        #region interfaccia ILoadLevelling

        public List<LoadLevellingWork> LoadLevellingWorkTable { get; set; }

        public IEnumerable<string> GetDistinctPlanBu() =>
        (from rec in LoadLevellingWorkTable
            select rec.PLAN_BU).Distinct().ToList();
        public IEnumerable<string> GetDistinctFlagHr(string planbu) =>
        (from rec in LoadLevellingWorkTable
            where rec.PLAN_BU == planbu
            select rec.FLAG_HR).Distinct().ToList();
        public IEnumerable<string> GetDistinctProductionCategory(string planbu, string flaghr) =>
        (from rec in LoadLevellingWorkTable
            where rec.PLAN_BU == planbu && rec.FLAG_HR == flaghr
            select rec.PRODUCTION_CATEGORY).Distinct().ToList();

        public List<LoadLevellingWork> ListByWeekAndPriority(string planbu, string flaghr, string prodcat) =>
        (from rec in LoadLevellingWorkTable
            where rec.PLAN_BU == planbu && rec.FLAG_HR == flaghr && rec.PRODUCTION_CATEGORY == prodcat
            orderby rec.WEEK_PLAN, rec.Priority
            select rec).ToList();

        #endregion
    }
}
