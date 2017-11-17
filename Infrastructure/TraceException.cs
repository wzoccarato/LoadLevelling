using System;

namespace CalcExtendedLogics.Infrastructure
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
