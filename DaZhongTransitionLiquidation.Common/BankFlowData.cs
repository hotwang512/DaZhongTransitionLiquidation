using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Common
{
    public class BankFlowResult
    {
        public bool success { get; set; }
        public string errmsg { get; set; }
        public BankFlowData data { get; set; }
    }
    public class BankFlowData
    {
        /// <summary>
        /// Desc:账户号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string ACNO { get; set; }
        /// <summary>
        /// Desc:账号币种
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BIZH { get; set; }
        /// <summary>
        /// Desc:账户名称
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string HUMI { get; set; }
        /// <summary>
        /// Desc:贷方总金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DFJE { get; set; }
        /// <summary>
        /// Desc:贷方总笔数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DFBS { get; set; }
        /// <summary>
        /// Desc:借方总金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JFJE { get; set; }
        /// <summary>
        /// Desc:借方总笔数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string JFBS { get; set; }
        /// <summary>
        /// Desc:合计总笔数
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string SUMU { get; set; }
        public List<BankFlowDataDetail> Detail { get; set; }
    }
    public class BankFlowDataDetail
    {
        /// <summary>
        /// Desc:借贷方标记1:借方金额数（账户付款）;2:贷方金额数（账户收款）
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string CDFG { get; set; }
        /// <summary>
        /// Desc:摘要标志：转账、跨行转账
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string PATI { get; set; }
        /// <summary>
        /// Desc:交易金额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string FSJE { get; set; }
        /// <summary>
        /// Desc:交易时间
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string FSSJ { get; set; }
        /// <summary>
        /// Desc:对方账号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DFZH { get; set; }
        /// <summary>
        /// Desc:对方账号币种
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DFBZ { get; set; }
        /// <summary>
        /// Desc:对方户名
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string DFHM { get; set; }
        /// <summary>
        /// Desc:凭证种类
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VCTP { get; set; }
        /// <summary>
        /// Desc:凭证编号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string VCNO { get; set; }
        /// <summary>
        /// Desc:历史余额
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string YUER { get; set; }
        /// <summary>
        /// Desc:用途
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string YOTU { get; set; }
        /// <summary>
        /// Desc:备注
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string BEZH { get; set; }
        /// <summary>
        /// Desc:T24流水号
        /// Default:
        /// Nullable:True
        /// </summary> 
        public string T24F { get; set; }
        
    }

}
