using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Infrastructure.ViewEntity
{
    public class v_Business_BusinessTypeSet
    {
        public v_Business_BusinessTypeSet()
        {
        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BusinessSubItem1 { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BusinessProject { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? VGUID { get; set; }
    }
}
