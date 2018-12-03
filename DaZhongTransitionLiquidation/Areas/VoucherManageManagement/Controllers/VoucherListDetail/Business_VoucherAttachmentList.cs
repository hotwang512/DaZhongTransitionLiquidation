using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.VoucherManageManagement.Controllers.VoucherListDetail
{
    public class Business_VoucherAttachmentList
    {
        public Guid VGUID { get; set; }
        public Guid VoucherVGUID { get; set; }
        public string Attachment { get; set; }
        public string AttachmentType { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreatePerson { get; set; }
    }
}