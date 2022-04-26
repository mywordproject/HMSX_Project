using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin.生产制造
{
    [Description("领料单单--校验领料数量，反写数量;领退补--带出供应商")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class LLDServerPlugin : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            String[] propertys = { "FStockOrgId", "FActualQty", "F_RUJP_PGENTRYID", "FMaterialId" , "FLot" };
            foreach (String property in propertys)
            {
                e.FieldKeys.Add(property);
            }
        }
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            if (FormOperation.Operation.Equals("Audit", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ExtendedDataEntity extended in e.SelectedRows)
                {
                    DynamicObject dy = extended.DataEntity;
                    
                    if (dy["StockOrgId_Id"].ToString() == "100026")
                    {
                        DynamicObjectCollection docPriceEntity = dy["Entity"] as DynamicObjectCollection;
                        foreach (var entry in docPriceEntity)
                        {
                            string cxsql = $@"select FMustQty-FAvailableQty as QTY from t_PgBomInfo where FPgEntryId='{entry["F_RUJP_PGENTRYID"].ToString()}' and FMaterialId='{entry["MaterialId_Id"].ToString()}'";
                            var cx = DBUtils.ExecuteDynamicObject(Context, cxsql);
                            if (cx.Count > 0)
                            {
                                if (Convert.ToDouble(entry["ActualQty"].ToString()) > Convert.ToDouble(cx[0]["QTY"].ToString()))
                                {
                                    throw new KDBusinessException("", "超额领料！");
                                }
                            }
                            
                        }
                    }
                }
            }
        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            if (FormOperation.Operation.Equals("Audit", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var dates in e.DataEntitys)
                {
                    if (dates["StockOrgId_Id"].ToString() == "100026")
                    {
                        var entrys = dates["Entity"] as DynamicObjectCollection;
                        foreach (var entry in entrys)
                        {
                            //string gyssql = $@"select FSUPPLYID from T_BD_LOTMASTER where FLOTID='{entry["Lot_Id"].ToString()}' and FMATERIALID='{entry["MaterialId_Id"].ToString()}'";
                            //var gys = DBUtils.ExecuteDynamicObject(Context, gyssql);
                            //if (gys.Count > 0)
                            //{
                            //    string upsql = $@"update T_PRD_PICKMTRLDATA set F_260_GYS='{gys[0]["FSUPPLYID"].ToString()}' where FENTRYID='{entry["Id"].ToString()}'";
                            //    DBUtils.Execute(Context, upsql);
                            //}
                            
                            string cxsql = $@"update T_SFC_DISPATCHDETAILENTRY set F_260_LLSL=F_260_LLSL+{Convert.ToDouble(entry["ActualQty"].ToString())} where FENTRYID='{entry["F_RUJP_PGENTRYID"].ToString()}'";
                            DBUtils.Execute(Context, cxsql);
                        }
                    }
                }
            }
            else if(FormOperation.Operation.Equals("UnAudit", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var dates in e.DataEntitys)
                {
                    if (dates["StockOrgId_Id"].ToString() == "100026")
                    {
                        var entrys = dates["Entity"] as DynamicObjectCollection;
                        foreach (var entry in entrys)
                        {
                            string cxsql = $@"update T_SFC_DISPATCHDETAILENTRY set F_260_LLSL=F_260_LLSL-{Convert.ToDouble(entry["ActualQty"].ToString())} where FENTRYID='{entry["F_RUJP_PGENTRYID"].ToString()}'";
                            DBUtils.Execute(Context, cxsql);
                        }
                    }
                }
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (FormOperation.Operation.Equals("Save", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var dates in e.DataEntitys)
                {
                    if (dates["StockOrgId_Id"].ToString() == "100026")
                    {
                        var entrys = dates["Entity"] as DynamicObjectCollection;
                        foreach (var entry in entrys)
                        {
                            string gyssql = $@"select FSUPPLYID from T_BD_LOTMASTER where FLOTID='{entry["Lot_Id"].ToString()}' and FMATERIALID='{entry["MaterialId_Id"].ToString()}'";
                            var gys = DBUtils.ExecuteDynamicObject(Context, gyssql);
                            if (gys.Count > 0)
                            {
                                string upsql = $@"update T_PRD_PICKMTRLDATA set F_260_GYS1='{gys[0]["FSUPPLYID"].ToString()}' where FENTRYID='{entry["Id"].ToString()}'";
                                DBUtils.Execute(Context, upsql);
                            }
                        }
                    }
                }
            }
        }

    }
}
