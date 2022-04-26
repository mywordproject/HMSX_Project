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
    [Description("用料清单审核时---插入报工中间表")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class AuditInsertServerPlugin : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            String[] propertys = { "FStockID", "FIssueType", "FPrdOrgId", "FNumerator" };
            foreach (String property in propertys)
            {
                e.FieldKeys.Add(property);
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (var date in e.DataEntitys)
            {
                if (date["PrdOrgId_Id"].ToString() == "100026")
                {
                    DynamicObjectCollection entrydates = (DynamicObjectCollection)date["PPBomEntry"];
                    foreach (var entrydate in entrydates)
                    {
                        if (entrydate["IssueType"].ToString() == "3" && (DynamicObject)entrydate["StockID"] == null ? false : ((DynamicObject)entrydate["StockID"])["Name"].ToString().Contains("线边仓"))
                        {
                            string cxsql = $@"select * from t_PgBomInfo where FPPBomEntryId='{entrydate["Id"]}'";
                            var cxs = DBUtils.ExecuteDynamicObject(Context, cxsql);
                            if (cxs.Count > 0)
                            {
                                foreach (var cx in cxs)
                                {
                                    if (cx["FPickQty"].ToString() == "0")
                                    {
                                        string delsql = $@"delete t_PgBomInfo where FPgENTRYID='{cx["FPgENTRYID"]}'";
                                        DBUtils.Execute(Context, delsql);
                                    }
                                    else
                                    {
                                        string upsql = $@"update t_PgBomInfo set fmustqty=fpgqty*{double.Parse(entrydate["Numerator"].ToString())} where FPgENTRYID='{cx["FPgENTRYID"]}'";
                                        DBUtils.Execute(Context, upsql);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
