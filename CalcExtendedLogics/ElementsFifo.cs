using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CalcExtendedLogics.DataLayer.DbTables;

namespace CalcExtendedLogics
{
    public class ElementsFifo
    {
        private Queue<LoadLevellingWork> _fifo;
        public ElementsFifo()
        {
            _fifo = new Queue<LoadLevellingWork>();
        }

        public void Push(LoadLevellingWork newelement)
        {
            if (!_fifo.Contains(newelement))
            {
                _fifo.Enqueue(newelement);
            }
        }

        public void PushElements(IList<LoadLevellingWork> elements)
        {
            foreach (var el in elements)
            {
                if (!_fifo.Contains(el))
                {
                    _fifo.Enqueue(el);
                }
            }
        }

        public LoadLevellingWork Pop() => _fifo.Count > 0 ? _fifo.Dequeue() : null;

        public LoadLevellingWork PeekFirst() => _fifo.Count > 0 ? _fifo.Peek() : null;

        public void Purge()
        {
            _fifo.Clear();
        }
    }
}
