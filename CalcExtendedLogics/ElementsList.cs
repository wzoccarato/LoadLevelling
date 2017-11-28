using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalcExtendedLogics.DataLayer.DbTables;
using CalcExtendedLogics.Infrastructure;
using static CalcExtendedLogics.Helper;

namespace CalcExtendedLogics
{
    public class ElementsList:IDisposable
    {
        private bool _isDisposed;

        private readonly List<LoadLevellingWork> _list;
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

            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                // Test di consistenza
                var invalid = (from rec in _list
                               where rec.Id != null && rec.Id == newelement.Id
                               select rec).Any();
                if (invalid)
                    throw new TraceException(fname, "Rilevata chiave multipla. Oggetto non valido");
                _list.Add(newelement);
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

        public bool RemoveElement(LoadLevellingWork toremove) => _list.Remove(toremove);

        public void AddRange(IList<LoadLevellingWork> elements)
        {
            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                foreach (var el in elements)
                {
                    _list.Add(el);
                }
                // Test di consistenza un gli id == null li permetto, perche' sono gli elementi nuovi 
                // inseriti in lista in append
                var invalid = (from rec in _list
                               where rec.Id != null
                               group rec by rec.Id into g
                               orderby g.Key, g.Count()
                               select new { id = g.Key, count = g.Count() }).ToList().Any(t => t.count>1);
                if (invalid)
                    throw new TraceException(fname, "Rilevata chiave multipla. Oggetto non valido");

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

        // merge delle due liste, rimuovento gli elementi doppi
        public void MergeElements(ElementsList el)
        {

            var fname = MethodBase.GetCurrentMethod().DeclaringType?.Name + "." + MethodBase.GetCurrentMethod().Name;
            try
            {
                foreach (var rec in el._list)
                {
                    _list.Add(rec);
                }
                // Test di consistenza
                var invalid = (from rec in _list
                               where rec != null
                               group rec by rec.Id into g
                               orderby g.Key, g.Count()
                               select new { id = g.Key, count = g.Count() }).ToList().Any(t => t.count > 1);
                if (invalid)
                    throw new TraceException(fname, "Rilevata chiave multipla. Oggetto non valido");
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
                    var wklist = (from rec in _list where rec.Week == weekplan select rec).ToList();
                    return wklist;
                }
                else
                    throw new TraceException(fname, $"Formato errato Week: {weekplan}");
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
                    _list.RemoveAll(r => r.Week == weekplan);
                }
                else
                    throw new TraceException(fname, $"Formato errato Week: {weekplan}");
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

        // per una descrizione di questa tecnica di utilizzo di Dispose,
        // vedere c# 4 e .NET 4 *Nagel e altri, pag.309

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                // qui eseguire le eventuali pulizie degli gestiti
            }
            _isDisposed = true;
        }

        // TODO: Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //~ElementsList()
        //{
        //    Dispose(false);
        //}

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
    }
}
