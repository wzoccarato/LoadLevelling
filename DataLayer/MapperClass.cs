using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoadL.DataLayer.DbTables;

namespace LoadL.DataLayer
{
    public class MapperClass
    {
        #region ctor
        public MapperClass()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<LoadLevelling, LoadLevellingWork>());
            var config = new MapperConfiguration(cfg => cfg.CreateMap<LoadLevelling, LoadLevellingWork>());

        }
        #endregion
    }
}
