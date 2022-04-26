using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin
{
    [Description("入库单---入库数量反写到检验单")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public  class RKSLBillPlugin: AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            String[] propertys = { "FMaterialId", "FLot", "FSrcBillNo" , "FRealQty" , "FSrcEntrySeq" };
            foreach (String property in propertys)
            {
                e.FieldKeys.Add(property);
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (FormOperation.Operation.Equals("Audit", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var date in e.DataEntitys)
                {
                    if(date["StockOrgId_Id"].ToString()== "100026")
                    {
                        var entrys = date["Entity"] as DynamicObjectCollection;
                        foreach (var entry in entrys)
                        {
                            string upsql = $@"update T_QM_INSPECTBILLENTRY
                        set  F_260_RKSL={Convert.ToDouble(entry["RealQty"].ToString())}
                        where FENTRYID =(select a.FENTRYID from T_QM_INSPECTBILLENTRY_A a
                        inner join T_QM_INSPECTBILLENTRY b on a.fentryid=b.fentryid 
                        inner join T_BD_LOTMASTER c on b.FLOT=c.FLOTID
                        where FSRCBILLNO='{entry["SrcBillNo"]}'
                        and a.FMATERIALID='{ entry["MaterialId_Id"]}' and c.FNUMBER='{entry["Lot_text"]}' and FSRCENTRYSEQ={entry["SrcEntrySeq"]})";
                            DBUtils.Execute(Context, upsql);
                        }
                    }                 
                }
            }
            else if (FormOperation.Operation.Equals("UnAudit", StringComparison.OrdinalIgnoreCase))
            {               
                foreach (var date in e.DataEntitys)
                {
                    if (date["StockOrgId_Id"].ToString() == "100026")
                    {
                        var entrys = date["Entity"] as DynamicObjectCollection;
                        foreach (var entry in entrys)
                        {
                            string upsql = $@"update T_QM_INSPECTBILLENTRY
                        set  F_260_RKSL=0
                        where FENTRYID =(select a.FENTRYID from T_QM_INSPECTBILLENTRY_A a
                        inner join T_QM_INSPECTBILLENTRY b on a.fentryid=b.fentryid 
                        inner join T_BD_LOTMASTER c on b.FLOT=c.FLOTID
                        where FSRCBILLNO='{entry["SrcBillNo"]}'
                        and a.FMATERIALID='{ entry["MaterialId_Id"]}' and c.FNUMBER='{entry["Lot_text"]}' and FSRCENTRYSEQ={entry["SrcEntrySeq"]})";
                            DBUtils.Execute(Context, upsql);
                        }
                    }
                }
            }

        }
       
    }
}
