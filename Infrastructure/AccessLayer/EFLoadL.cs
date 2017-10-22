using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.XpressionMapper.Extensions;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure.Abstract;

namespace LoadL.Infrastructure.AccessLayer
{
    public class EfLoadL : IDbQuery, IDisposable
    {
        private bool _isDisposed = false;

        private readonly EFDbContext _context = new EFDbContext();

        private List<LoadLevellingWork> _iLoadLevellingWorks;

        #region ctor

        public EfLoadL()
        {
            _iLoadLevellingWorks = null;

            Mapper.Initialize(cfg => cfg.CreateMap<LoadLevelling, LoadLevellingWork>()
                .ForMember(dest => dest.F1, opt => opt.MapFrom(src => src.a))
                .ForMember(dest => dest.F2, opt => opt.MapFrom(src => src.b))
                .ForMember(dest => dest.F3, opt => opt.MapFrom(src => src.c))
                .ForMember(dest => dest.Ahead, opt => opt.MapFrom(src => src.d))
                .ForMember(dest => dest.Late, opt => opt.MapFrom(src => src.e))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.f))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.g))
                .ForMember(dest => dest.Required, opt => opt.MapFrom(src => src.h))
                .ForMember(dest => dest.PLAN_BU, opt => opt.MapFrom(src => src.i))
                .ForMember(dest => dest.FLAG_HR, opt => opt.MapFrom(src => src.j))
                .ForMember(dest => dest.Allocated, opt => opt.MapFrom(src => src.k)));
        }

        #endregion

        #region LoadLevelling table

        public IQueryable<LoadLevelling> LoadLevellingTable => _context.LoadLevellings;

        public List<LoadLevellingWork> LoadLevellingWorkTable
        {
            get
            {
                if (_iLoadLevellingWorks == null)
                {
                    _iLoadLevellingWorks = new List<LoadLevellingWork>();
                    foreach (var el in LoadLevellingTable)
                    {
                        _iLoadLevellingWorks.Add(Mapper.Map<LoadLevelling, LoadLevellingWork>(el));
                    }
                }
                return _iLoadLevellingWorks;
            }
            set { }
        }

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
             where rec.PLAN_BU == planbu && rec.FLAG_HR == flaghr && rec.PRODUCTION_CATEGORY == prodcat orderby rec.WEEK_PLAN, rec.Priority
             select rec ).ToList();

        public IQueryable<Schema> SchemaTable => _context.Schema;
        public Database LlDatabase => _context.Database;
        public void Save()
        {
            _context.SaveChanges();
        }

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

        ~EfLoadL()
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
