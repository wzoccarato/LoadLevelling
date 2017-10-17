using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadL.Infrastructure
{
    public class TraceException : Exception
    {
        public TraceException(string firingfunc, string message = "")
        {
            TraceMessage = "[" + firingfunc + "] " + message;
            Errocode = 0;
        }
        public string TraceMessage { get; protected set; }
        public int Errocode { get; set; }
    }
}
