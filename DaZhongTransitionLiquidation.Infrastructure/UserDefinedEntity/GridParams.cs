using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    public class GridParams
    {
        public int pagenum { get; set; }
        public int pagesize { get; set; }
        public string sortdatafield { get; set; }
        public string sortorder { get; set; }
    }
}
