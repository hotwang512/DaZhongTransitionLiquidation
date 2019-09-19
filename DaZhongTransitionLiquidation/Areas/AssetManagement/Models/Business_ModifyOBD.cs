using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_ModifyOBD
    {
        public Guid VGUID { get; set; }
        public string PlateNumber { get; set; }
        public string EquipmentNumber { get; set; }
        public Boolean ISVerify { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ChangeDate { get; set; }
        public string CreateUser { get; set; }
        public string ChangeUser { get; set; }
    }
}