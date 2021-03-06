﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using CalcExtendedLogics.DataLayer.DbTables;
using CalcExtendedLogics.Infrastructure.Abstract;

namespace CalcExtendedLogics.Infrastructure.AccessLayer
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

        // TODO ripristinare questa, che è stata modificata a scopo di test
        public List<LoadLevellingWork> ListByWeekAndPriority(string planbu, string flaghr, string prodcat) =>
            (from rec in LoadLevellingWorkTable
             where rec.PLAN_BU == planbu && rec.FLAG_HR == flaghr && rec.PRODUCTION_CATEGORY == prodcat
             //&& (rec.Week == "201707" || rec.Week == "201708" || rec.Week == "201709")
             orderby rec.Week, rec.Priority
             select rec ).ToList();

        public IQueryable<Schema> SchemaTable => _context.Schema;
        public Database LlDatabase => _context.Database;
        public void Save()
        {
            _context.SaveChanges();
        }

        public void MassiveSaveData()
        {
            try
            {
                _context.BulkSaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        /// <summary>
        /// Aggiorna gli elementi della tabella LoadLevellings, 
        /// contenuti nella lista passta in argomento
        /// Attenzione che la funzione non esegue il salvataggio del dbcontext,
        /// che deve essere eseguito esternamente
        /// sei il record non esiste nella tabella, viene sollevata una eccezione
        /// </summary>
        /// <param name="list"></param>
        public void UpdateData(List<LoadLevelling> list)
        {
            try
            {
                foreach (var el in list)
                {
                    _context.LoadLevellings.Attach(el);
                    _context.Entry(el).State = EntityState.Modified;
                    //_context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }


        /// <summary>
        /// Aggiorna gli elementi della tabella LoadLevellings, 
        /// contenuti nella lista passta in argomento
        /// Attenzione che la funzione non esegue il salvataggio del dbcontext,
        /// che deve essere eseguito esternamente
        /// Utilizza BulkUpdate di Z.EntityFramework.Extensions 
        /// </summary>
        /// <param name="list"></param>
        public void MassiveUpdateData(List<LoadLevelling> list)
        {
            try
            {
                _context.BulkUpdate(list);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// inserisce tutti gli elementi della lista nella tabella LoadLevellings
        /// Attenzione che la funzione non esegue il salvataggio del dbcontext,
        /// che deve essere eseguito esternamente
        /// </summary>
        /// <param name="list"></param>
        public void AddData(List<LoadLevelling> list)
        {
            try
            {
                foreach (var el in list)
                {
                    _context.LoadLevellings.Add(el);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// inserisce tutti gli elementi della lista nella tabella LoadLevellings
        /// Attenzione che la funzione non esegue il salvataggio del dbcontext,
        /// che deve essere eseguito esternamente
        /// Utilizza BulkInsert di Z.EntityFramework.Extensions 
        /// Attenzione che se il record esiste gia', viene sollevata una eccezione
        /// </summary>
        /// <param name="list"></param>
        public void MassiveAddData(List<LoadLevelling> list)
        {
            try
            {
                _context.BulkInsert(list);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~EfLoadL()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }


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
