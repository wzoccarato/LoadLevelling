using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure.Abstract;

namespace LoadL.TestLL.AccessLayer
{
    class EFLoadL : ILoadL, IDisposable
    {
        private bool _isDisposed = false;

        private readonly EFDbContext _context = new EFDbContext();


        #region LoadLevelling table

        public IQueryable<LoadLevelling> LoadLevellingTable => _context.LoadLevellings;

        public Database LLDatabase => _context.Database;

        #endregion


        // per una descrizione di questa tecnica di utilizzo di Dispose,
        // vedere c# 4 e .NET 4 *Nagel e altri, pag.309

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                // qui deve ripulire gli oggetti non gestiti
            }
            _isDisposed = true;
        }

        ~EFLoadL()
        {
            Dispose(false);
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AppConfiguration() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

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
