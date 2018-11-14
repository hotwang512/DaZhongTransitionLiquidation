using DaZhongTransitionLiquidation.Infrastructure.DbEntity;
using SqlSugar;
using System.Collections.Generic;

namespace DaZhongTransitionLiquidation.Areas.PaymentManagement.Controllers.NextDayData
{
    public class NextDayDataPack
    {
        public bool ExistReconciliation(SqlSugarClient db, Business_Revenuepayment_Information revenuepaymentInformationDatas, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<Business_Revenuepayment_Information>().Any(i => i.TransactionID == revenuepaymentInformationDatas.TransactionID && i.VGUID != revenuepaymentInformationDatas.VGUID);
            }
            return db.Queryable<Business_Revenuepayment_Information>().Any(i => i.TransactionID == revenuepaymentInformationDatas.TransactionID);
        }
        /// <summary>
        /// 流水号是否存在
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="t1Data"></param>
        /// <param name="isEdit">是否编辑</param>
        /// <returns></returns>
        public bool IsExistTranscationId(SqlSugarClient db, Business_T1Data_Information t1Data, bool isEdit)
        {
            if (isEdit)//编辑
            {
                return db.Queryable<Business_T1Data_Information>().Any(i => i.serialnumber == t1Data.serialnumber && i.Vguid != t1Data.Vguid);
            }
            return db.Queryable<Business_T1Data_Information>().Any(i => i.WechatNo == t1Data.WechatNo);
        }

        /// <summary>
        /// 导出T+1数据
        /// </summary>
        /// <param name="db"></param>
        /// <param name="t1Datas"></param>
        /// <returns></returns>
        public int ImportData(SqlSugarClient db, List<Business_T1Data_Information> t1Datas)
        {
            return db.Insertable<Business_T1Data_Information>(t1Datas).ExecuteCommand();
        }
        /// <summary>
        /// 导出营收表
        /// </summary>
        /// <param name="db"></param>
        /// <param name="revenuepaymentInformationDatas"></param>
        /// <returns></returns>
        public int ImportData_Reconciliation(SqlSugarClient db, List<Business_Revenuepayment_Information> revenuepaymentInformationDatas)
        {
            return db.Insertable(revenuepaymentInformationDatas).ExecuteCommand();
        }



    }
}