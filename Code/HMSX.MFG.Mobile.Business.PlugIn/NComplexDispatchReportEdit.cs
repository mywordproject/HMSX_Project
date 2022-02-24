using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Complex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System.ComponentModel;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Core.DynamicForm;

namespace HMSX.MFG.Mobile.Business.PlugIn
{
    [Description("派工工序汇报编辑-表单插件")]
    public  class NComplexDispatchReportEdit: ComplexDispatchReportEdit
    {
        private long dispEntryId = 0L;
        private long MaterialId = 0L;
        public override void AfterBindData(EventArgs e)
        {
           base.AfterBindData(e);
           DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.BillModel.DataObject["OptRptEntry"];
             dispEntryId = Convert.ToInt64(dynamicObjectCollection[0]["DispatchDetailEntryId"]);
             MaterialId = Convert.ToInt64(dynamicObjectCollection[0]["MaterialId_Id"]);
            string strSql = string.Format("select F_LOT,F_LOT_Text,F_RUJP_Lot,FBARCODE from T_SFC_DISPATCHDETAILENTRY where FENTRYID={0}", dispEntryId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            if (rs.Count > 0)
            {
                // this.View.BillModel.SetValue("FLot", rs[0]["F_LOT_Text"].ToString());
                this.View.BillModel.SetValue("FLot", rs[0]["F_RUJP_Lot"].ToString());
                this.View.BillModel.SetValue("F_SBID_BARCODE", rs[0]["FBARCODE"].ToString()); 
                this.View.UpdateView("F_HMD_MobileProxyField5");
                this.View.UpdateView("FMobileProxyField_PgBarcode");
            }
        }
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            string a;
            if ((a = e.Key.ToUpper()) != null)
            {
                if (a == "FBUTTON_SUBMIT")
                {
                    DynamicObjectCollection dynamicObjectCollection = (DynamicObjectCollection)base.View.BillModel.DataObject["OptRptEntry"];
                   DynamicObject Material =dynamicObjectCollection[0]["MaterialId"] as DynamicObject;
                    string materialNum = Material["Number"].ToString();
                    string strSql = string.Format(@"SELECT count(FPgEntryId) as Fcount,sum(FPickQty) as Fqty, sum(FMustQty) as FMustQty FROM t_PgBomInfo WHERE FPgEntryId={0} AND FMaterialId NOT IN (SELECT FMATERIALID FROM T_BD_MATERIAL WHERE FNUMBER like '260.07%'  )", dispEntryId);
                    DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
                    if(materialNum.Substring(0, 6) != "260.07")
                    {
                        if (rs.Count > 0  )
                        {
                            if (Convert.ToInt16(rs[0]["Fcount"]) > 0)
                            {
                                if (Convert.ToDecimal(rs[0]["Fqty"]) < Convert.ToDecimal(rs[0]["FMustQty"]) * Convert.ToDecimal(0.98))
                                {
                                    base.View.ShowMessage(ResManager.LoadKDString("当前派工明细领料未完成，不允许报工！", "015747000026506", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                                    e.Cancel = true;
                                    return;
                                }
                            }
                            else
                            {
                                base.View.ShowMessage(ResManager.LoadKDString("当前派工明细领料未完成，不允许报工！", "015747000026506", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }
            }
            base.ButtonClick(e);
        }
    }
}
