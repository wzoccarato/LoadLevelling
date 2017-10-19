using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.XpressionMapper.Extensions;
using LoadL.DataLayer.DbTables;
using LoadL.Infrastructure.Abstract;

namespace LoadL.Infrastructure.AccessLayer
{
    public class EFLoadL : ILoadL, IDisposable
    {
        private bool _isDisposed = false;

        private readonly EFDbContext _context = new EFDbContext();

        private MapperConfiguration _config;
        private static Mapper _mapper;


        #region ctor

        public EFLoadL()
        {
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

        public IList<LoadLevellingWork> LoadLevellingWorkTable
        {
            get
            {
                var src = LoadLevellingTable.ToList();
                var dst = new List<LoadLevellingWork>();

                var llt = LoadLevellingTable;

                foreach (var el in llt)
                {
                    var llw = Mapper.Map<LoadLevelling, LoadLevellingWork>(el);
                    dst.Add(llw);
                }
                return dst;
            }
        }

        // TODO questa e' da rifare
        

        public IEnumerable<string> PlanBu()
        {
            var llwt = LoadLevellingWorkTable;
            var list = (from rec in LoadLevellingWorkTable select rec.PLAN_BU).Distinct().ToList();
            return list;
        }

        public IQueryable<Schema> SchemaTable => _context.Schema;
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
