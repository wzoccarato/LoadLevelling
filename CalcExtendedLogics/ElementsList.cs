using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure;
using static LoadL.CalcExtendedLogics.Helper;

namespace LoadL.CalcExtendedLogics
{
    public class ElementsList
    {
        private List<LoadLevellingWork> _list;
        public ElementsList()
        {
            _list = new List<LoadLevellingWork>();
        }

        public List<LoadLevellingWork> GetList()
        {
            return _list;
        }

        public int Count => _list.Count;

        public void AddElement(LoadLevellingWork newelement)
        {
            if (!_list.Contains(newelement))
            {
                _list.Add(newelement);
            }
        }

        public void AddRange(IList<LoadLevellingWork> elements)
        {
            foreach (var el in elements)
            {
                if (!_list.Contains(el))
                {
                    _list.Add(el);
                }
            }
        }

        // merge delle due liste, rimuovento gli elementi doppi
        public void MergeElements(ElementsList el)
        {
            // TODO: e' piu' efficiente cosi'?
            //var ll = _list.Concat(_list).GroupBy(r => r.Id).Distinct().ToList();
            
            foreach (var rec in el._list)
            {
                if (!_list.Contains(rec))
                {
                    _list.Add(rec);
                }
            }
        }

        public LoadLevellingWork GetLast() => _list.Count > 0 ? _list.Last() : null;

        public LoadLevellingWork GetFirst() => _list.Count > 0 ? _list.First() : null;

        public LoadLevellingWork RemoveLast()
        {
            var count = _list.Count;
            LoadLevellingWork retval=null;
            if (count > 0)
            {
                retval = _list.Last();
                _list.RemoveAt(count - 1);
            }
            return retval;
        }

        public void Purge()
        {
            _list.Clear();
        }

        public List<LoadLevellingWork> GetByWeek(string weekplan)
        {

            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                if (ValidateWeekFormat(weekplan))
                {
                    List<LoadLevellingWork> wklist = new List<LoadLevellingWork>();
                    wklist = (from rec in _list where rec.WEEK_PLAN == weekplan select rec).ToList();
                    return wklist;
                }
                else
                    throw new TraceException(fname, $"Formato errato WEEK_PLAN: {weekplan}");
            }
            catch (TraceException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new TraceException(fname, e.Message);
            }
        }

        public void RemoveByWeekPlan(string weekplan)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                if (ValidateWeekFormat(weekplan))
                {
                    _list.RemoveAll(r => r.WEEK_PLAN == weekplan);
                }
                else
                    throw new TraceException(fname, $"Formato errato WEEK_PLAN: {weekplan}");
            }
            catch (TraceException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new TraceException(fname, e.Message);
            }
        }

        /// <summary>
        /// Validazione della lista interna sulla base del parametro Capacity, che deve
        /// avere lo stesso valore per tutti i record
        /// TODO: togliere una volta consolidato
        /// </summary>
        /// <returns></returns>
        public bool ValidateList()
        {
            return _list.Select(r => r.Capacity).Distinct().ToList().Count == 1;
        }
    }
}
