using System;
using System.Collections.Generic;
using System.Linq;
using Kingdee.K3.Core.MFG.SFC;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Complex;
using Kingdee.BOS.Orm.DataEntity;

using Kingdee.K3.MFG.ServiceHelper.SFS;
using System.Reflection;
using Kingdee.BOS.Resource;
using Kingdee.K3.MFG.ServiceHelper.SFC;
using Kingdee.BOS.Core;
using Kingdee.K3.MFG.SFC.Common.Core.EnumConst.Mobile;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.DataModel;
using Kingdee.BOS;
using System.Data;
using Kingdee.BOS.ServiceHelper;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCUtils;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Utils;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Mobile.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.K3.Core.SCM.STK;
using Kingdee.K3.SCM.ServiceHelper;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Mobile;
using Kingdee.BOS.Mobile.PlugIn;
using System.ComponentModel;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;

namespace HMSX.MFG.Mobile.Business.PlugIn
{
    [Description("工序任务超市-表单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class MobileComplexTaskPoolListEdit : ComplexTaskPoolList
    {

        protected long optPlanOptId;
        protected long  materialId ;
        protected long  userOrgId;
        protected long  masterId;


        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            string key;
            switch (key = e.Key.ToUpper())
            {
                case "FBUTTON_PREVIOUS":
                    this.Model.DataObject["FLot_Text"] = null;
                    this.View.Model.SetValue("FLot", "");

                   this.Model.DataObject["FMouldId_Id"] = null;
                    this.View.Model.SetValue("FMouldId", 0);
                    return;
                case "FBUTTON_NEXT":
                    this.Model.DataObject["FLot_Text"] = null;
                    this.View.Model.SetValue("FLot", "");

                    this.Model.DataObject["FMouldId_Id"] = null;
                    this.View.Model.SetValue("FMouldId", 0);
                    return;
                case "FBUTTON_CONFIRM":
                    if (this.Model.GetValue("FLot").ToString() == "" || this.Model.GetValue("FLot").ToString() == null)
                    {
                        base.View.ShowMessage(ResManager.LoadKDString("批号不允许为空！", "015747000026506", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        //获取选择行信息
                        Dictionary<string, object> currentRowData = this.GetCurrentRowData();
                        string malnumber = currentRowData["FProductId"].ToString().Substring(0, currentRowData["FProductId"].ToString().IndexOf("/"));
                        //查询派工明细是否存在相同批号
                        string cxsql = $@" 
                    select * from T_SFC_DISPATCHDETAIL a
                    inner join T_SFC_DISPATCHDETAILENTRY b on a.FID=b.FID
                    inner join T_BD_Material c on a.fmaterialid=c.fmaterialid
                    where c.fnumber= '{malnumber}' and F_RUJP_Lot='{this.Model.GetValue("FLot").ToString()}'";
                        var cx = DBUtils.ExecuteDynamicObject(Context, cxsql);
                        if (cx.Count > 0)
                        {
                            base.View.ShowMessage(ResManager.LoadKDString("该批号在派工明细已存在！", "015747000026506", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                            e.Cancel = true;
                            return;
                        }
                    }
                    return;
            }
        }

     
          
        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            if (!this.IsClaim)
            {
                this.SetLotinfo();
                this.SetMouldIdInfo();
                this.setMinPackCount();
                return;
            }
          
        }
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            if (!this.IsClaim)
            {
                if (this.DicTableData.Keys.Count<int>() == 1)
                {
                    this.SetLotinfo();
                    this.SetMouldIdInfo();
                    this.setMinPackCount();
                    return;
                }
            }
        }

        private void SetLotinfo()
        {
           // int rowIndex = this.Model.GetEntryCurrentRowIndex("FMobileListViewEntity");
            Dictionary<string, object> currentRowData = this.GetCurrentRowData();
           string today = DateTime.Today.ToString("yyyy-MM-dd");
            //工序ID
            optPlanOptId = Convert.ToInt64(currentRowData["OperId"]);
            userOrgId = GetOrgId(optPlanOptId);
            DynamicObject obj = GetOptplan(optPlanOptId);
            materialId = Convert.ToInt64(obj["ProductId_Id"]);
            string strSql = string.Format(@"/*dialect*/select top 1  t1.FentryId,t1.F_LOT_Text from T_SFC_DISPATCHDETAIL t inner join T_SFC_DISPATCHDETAILENTRY t1 on t.FID=t1.FID and  DATEDIFF ( YEAR , cast('{0}' as datetime) , t1.FDISPATCHTIME )=0  and FMATERIALID={1} order by t1.FentryId desc", today, materialId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            if (rs.Count > 0)
            {
                // lot :作业号+物料编码+当前日期+3位流水号
                //20211102001
                string strLot;
                if (rs[0]["F_LOT_Text"] != null)
                {
                    strLot = rs[0]["F_LOT_Text"].ToString();
                }
                else { strLot = "0"; }
                this.View.Model.SetValue("FLot", this.CreateLot(strLot));
                this.View.UpdateView("FLot");
            }
            else
            {
                this.View.Model.SetValue("FLot", this.CreateLot("0"));
                this.View.UpdateView("FLot");
            }
        }

        /// <summary>
        /// 设置模具字段
        /// </summary>
        private void SetMouldIdInfo()
        {
          //  int rowIndex = this.Model.GetEntryCurrentRowIndex("FMobileListViewEntity");
            Dictionary<string, object> currentRowData = this.GetCurrentRowData();
            optPlanOptId = Convert.ToInt64(currentRowData["OperId"]);
            userOrgId = GetOrgId(optPlanOptId);
            DynamicObject obj = GetOptplan(optPlanOptId);
            materialId = Convert.ToInt64(obj["ProductId_Id"]);
           string strSql = string.Format("select FID from T_ENG_MOULD where FUSEORGID={0} and FMouldModelId={1} AND FDOCUMENTSTATUS='C' AND FFORBIDSTATUS='A'", userOrgId, materialId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            if (rs.Count ==1)
            {
                this.View.Model.SetValue("FMouldId", Convert.ToInt64( rs[0]["FID"]));
                this.View.UpdateView("FMouldId");
            }
            else
            {
                this.View.Model.SetValue("FMouldId", 0);
                this.View.UpdateView("FMouldId");
            }
        }

        private void setMinPackCount()
        {
            Dictionary<string, object> currentRowData = this.GetCurrentRowData();
            optPlanOptId = Convert.ToInt64(currentRowData["OperId"]);
            DynamicObject obj = GetOptplan(optPlanOptId);
            materialId = Convert.ToInt64(obj["ProductId_Id"]);
            string strSql = string.Format(@"select FMinPackCount from T_BD_MATERIALPURCHASE where FMATERIALID={0}", materialId);
           DynamicObjectCollection rs= DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            decimal minPackcount = Convert.ToDecimal(rs[0]["FMinPackCount"]);
            this.View.Model.SetValue("F_RUJP_Qty", minPackcount);
            this.View.UpdateView("F_RUJP_Qty");
        }
        public string CreateLot(string lot)
        {
            var today = DateTime.Today.ToString("yyyyMMdd");
            if (lot.Length>1)
            {
                var lotdate = lot.Substring(0, 8);
                if (today == lotdate)
                {
                    var no = Convert.ToInt32(lot.Substring(8));
                    return $"{today}{++no:000}";
                }
            }
            return $"{today}001";
        }
        /// <summary>
        /// 获取组织Id
        /// </summary>
        /// <param name="operId"></param>
        /// <returns></returns>
        private long GetOrgId(long operId)
        {
            long userOrgId=0;
            string strSQL = "select FProcessOrgId,FWorkCenterId,FDepartmentId,fdetailId from T_SFC_OPERPLANNINGDETAIL where fdetailId=@operId ";
                  List<SqlParam> paramList = new List<SqlParam>
                 {
                      new SqlParam("@operId", KDDbType.Int64, operId)
                 };
            using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, strSQL, paramList))
            {
                while (dataReader.Read())
                {
                    userOrgId = Convert.ToInt64(dataReader["FProcessOrgId"]);
                }
            }
            return userOrgId;
        }

        protected override DynamicObject CreateDispatchDetailData()
        {
            DynamicObject obj = base.CreateDispatchDetailData();
            int rowIndex = this.Model.GetEntryCurrentRowIndex("FMobileListViewEntity");
            Dictionary<string, object> currentRowData = this.GetCurrentRowData(rowIndex);
            optPlanOptId = Convert.ToInt64(currentRowData["OperId"]);
            obj["F_SBID_OrgId_Id"] = GetOrgId(optPlanOptId);
            return obj;
        }

       
        /// <summary>
        ///  
        /// </summary>
        /// <param name="dispatchObj"></param>
        /// <param name="entryEntity"></param>
        /// <returns></returns>
        protected override DynamicObject CreateDispatchEntryData(DynamicObject dispatchObj, EntryEntity entryEntity)
        {
            DynamicObject obj = base.CreateDispatchEntryData(dispatchObj, entryEntity);
            DynamicObjectCollection entrys = dispatchObj["DispatchDetailEntry"] as DynamicObjectCollection;
            if (entrys.Count > 0)
            {
                var lastEntry = entrys.Last();
                //批号
                lastEntry["F_Lot_Text"] = this.Model.DataObject["FLot_Text"];
                lastEntry["F_Lot_Id"] = this.Model.DataObject["FLot_Id"];
                lastEntry["F_Lot"] = this.Model.DataObject["FLot"];
                lastEntry["F_RUJP_Lot"]= this.Model.DataObject["FLot_Text"];
                //模具
                lastEntry["MouldId_Id"] = this.Model.DataObject["FMouldId_Id"];
            }
            return obj;
        }
        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            string a;
            if ((a = e.FieldKey.ToUpper()) != null)
            {
                if (a == "FLOT")
                {
                    int rowIndex = this.Model.GetEntryCurrentRowIndex("FMobileListViewEntity");
                    Dictionary<string, object> currentRowData = this.GetCurrentRowData(rowIndex);
                    optPlanOptId = Convert.ToInt64(currentRowData["OperId"]);
                    DynamicObject obj = GetOptplan(optPlanOptId);
                    materialId = Convert.ToInt64(obj["ProductId_Id"]);
                    masterId = GetMasterId(materialId);
                    userOrgId = GetOrgId(optPlanOptId);
                    this.SetLotFilter(e);
                    return;
                }
                if (a == "FMOULDID")
                {
                    int rowIndex = this.Model.GetEntryCurrentRowIndex("FMobileListViewEntity");
                    Dictionary<string, object> currentRowData = this.GetCurrentRowData(rowIndex);
                    optPlanOptId = Convert.ToInt64(currentRowData["OperId"]);
                    DynamicObject obj = GetOptplan(optPlanOptId);
                    materialId = Convert.ToInt64(obj["ProductId_Id"]);
                    masterId = GetMasterId(materialId);
                    userOrgId = GetOrgId(optPlanOptId);
                    this.SetMouldFilter(e);
                    return;
                }
                ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
                listShowParameter.UseOrgId = this.userOrgId;
                listShowParameter.ListFilterParameter.Filter = e.ListFilterParameter.Filter;
            }
        }

        private void SetLotFilter(BeforeF7SelectEventArgs e)
        {
            string text = string.Format(" FMATERIALID ={0} AND FUseOrgId = {1} AND FLotStatus = '1' AND FBizType = '2' AND FCancelStatus = 'A' ", this.masterId, this.userOrgId);
            e.ListFilterParameter.Filter = text;
        }
        private void SetMouldFilter(BeforeF7SelectEventArgs e)
        {
            string text = string.Format(" FMouldModelId ={0} AND FUseOrgId = {1}  ", this.masterId, this.userOrgId);
            e.ListFilterParameter.Filter = text;
        }

        /// <summary>
        /// 获取工序计划物料信息
        /// </summary>
        /// <param name="operId"></param>
        /// <returns></returns>
        private DynamicObject GetOptplan(long operId)
        {
            SqlParam sqlParam = new SqlParam("@OperId", KDDbType.Int64, optPlanOptId);
            long num = DBServiceHelper.ExecuteScalar<long>(base.Context, "SELECT S.FID FROM T_SFC_OPERPLANNINGDETAIL d INNER JOIN T_SFC_OPERPLANNINGSEQ s ON s.FENTRYID=d.FENTRYID WHERE d.FDETAILID=@OperId", 0L, new SqlParam[]
            {
              sqlParam
            });
            DynamicObjectType dynamicObjectType = ((FormMetadata)MetaDataServiceHelper.Load(base.Context, "SFC_OperationPlanning", true)).BusinessInfo.GetDynamicObjectType();
            DynamicObject optPlan = BusinessDataServiceHelper.LoadSingle(base.Context, num, dynamicObjectType, null);
            return optPlan;
        }

        /// <summary>
        /// 根据物料ID获取MasterId
        /// </summary>
        /// <param name="materialId"></param>
        /// <returns></returns>
        private long GetMasterId(long materialId)
        {
            SqlParam sqlParam = new SqlParam("@materialId", KDDbType.Int64, materialId);
            long masterId = DBServiceHelper.ExecuteScalar<long>(base.Context, "select FMASTERID from T_BD_MATERIAL where FMATERIALID=@materialId", 0L, new SqlParam[]
            {
              sqlParam
            });
            return masterId;
        }
        /// <summary>
        /// 强制解析，获取数据源
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        protected override void PrepareDispDetailBindFields(Dictionary<string, string> dicFieldLabelKeys)
        {
            base.PrepareDispDetailBindFields(dicFieldLabelKeys);
            DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "F_RUJP_Lot");

        }
    }
}
