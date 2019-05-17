using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{
    public class Business_AssetAttachmentList
    {
        public Guid VGUID { get; set; }
        public Guid AssetOrderVGUID { get; set; }
        public string Attachment { get; set; }
        public string AttachmentType { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreatePerson { get; set; }
    }
}