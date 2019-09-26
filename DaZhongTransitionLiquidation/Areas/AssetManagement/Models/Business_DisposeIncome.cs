using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaZhongTransitionLiquidation.Areas.AssetManagement.Models
{

    ///<summary>
    ///处置收入
    ///</summary>
    [SugarTable("Business_DisposeIncome")]
    public partial class Business_DisposeIncome
    {
        public Business_DisposeIncome()
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
        /// Desc:报废提交中获取	   
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string AssetID { get; set; }

        /// <summary>
        /// Desc:报废提交中获取
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DepartmentVehiclePlateNumber { get; set; }

        /// <summary>
        /// Desc:弃用
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string OraclePlateNumber { get; set; }

        /// <summary>
        /// Desc:导入
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ImportPlateNumber { get; set; }

        /// <summary>
        /// Desc:主表带
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string VehicleModel { get; set; }

        /// <summary>
        /// Desc:主表带
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BelongToCompany { get; set; }

        /// <summary>
        /// Desc:主表带
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ManageCompany { get; set; }

        /// <summary>
        /// Desc:主表带
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CommissioningDate { get; set; }

        /// <summary>
        /// Desc:退车日期
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? BackCarDate { get; set; }

        /// <summary>
        /// Desc:计算
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int? BackCarAge { get; set; }

        /// <summary>
        /// Desc:导入	   导入
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string SaleMonth { get; set; }

        /// <summary>
        /// Desc:根据导入的模板类型填充	   根据导入的模板类型填充	   根据导入的模板类型填充-拍卖、出售、报废
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string SaleType { get; set; }

        /// <summary>
        /// Desc:报废提交中获取
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BusinessModel { get; set; }

        /// <summary>
        /// Desc:导入	   拍卖-成交总额	   出售-实售	   报废-残值金额	   
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? DisposeIncomeValue { get; set; }

        /// <summary>
        /// Desc:根据数据字典计算
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? AddedValueTax { get; set; }

        /// <summary>
        /// Desc:根据数据字典计算
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ConstructionTax { get; set; }
        public decimal? ConsignFee { get; set; }

        /// <summary>
        /// Desc:根据数据字典计算
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? AdditionalEducationTax { get; set; }

        /// <summary>
        /// Desc:根据数据字典计算
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? LocalAdditionalEducationTax { get; set; }

        /// <summary>
        /// Desc:模式主类-租赁模式且模式子类为长租车时计算、其它默认为0	   	   	   	   	   	   
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ReturnToPilot { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? NetIncomeValue { get; set; }

        /// <summary>
        /// Desc:导入	   报废-服务费金额	   拍卖、出售默认为0	   
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? ServiceFee { get; set; }

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

        public int SubmitStatus { get; set; }
    }
}