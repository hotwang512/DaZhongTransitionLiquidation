using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSugar;

namespace DaZhongTransitionLiquidation.Areas.AssetPurchase.Models
{

    ///<summary>
    ///车辆税费,采购数量车辆配置关联表 
    ///</summary>
    [SugarTable("Business_PurchaseOrderNum")]
    public class Business_PurchaseOrderNum
    {
        public Business_PurchaseOrderNum()
        {

        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public Guid VGUID { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PayItemCode { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string PayItem { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? OrderQuantity { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public Guid? FixedAssetOrderVguid { get; set; }

        public Guid FaxOrderVguid { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ChangeDate { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUser { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ChangeUser { get; set; }

    }
    /// <summary>
    /// 采购数量下拉数据
    /// </summary>
    public class PurchaseOrderSelectNum
    {
        public Guid FixedAssetsOrderVguid { get; set; }
        public string OrderNumber { get; set; }
        public int OrderQuantity { get; set; }
        public string PayItemCode { get; set; }
        public Boolean IsChecked { get; set; }

        public string OrderDesc
        {
            get { return "订单号" + OrderNumber + "数量"+ OrderQuantity; }
        }
    }
}