﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public LoadLevellingWork GetLast() => _list.Count > 0 ? _list.Last() : null;

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

        public List<LoadLevellingWork> GetByWeek(string week)
        {
            List<LoadLevellingWork> wklist = new List<LoadLevellingWork>();
            if (ValidateWeekFormat(week))
            {
                wklist = (from rec in _list where rec.TCH_WEEK == week select rec).ToList();
            }
            return wklist;
        }


    }
}
