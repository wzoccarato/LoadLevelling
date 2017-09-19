using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcExtendedLogics
{
    public class CalcExLogicClass
    {
        public static bool Execute(DataSet dataset, string requiredoperation, string targetdatatablename)
        {
            try
            {
                switch (requiredoperation)
                {
                    case "goz":
                        var targetdt = dataset.Tables[targetdatatablename];
                        break;
                    default:
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
