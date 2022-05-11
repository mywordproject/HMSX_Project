using Kingdee.BOS.App.Data;
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
    [Description("工序汇报--保存时截取批号日期字符")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class GXHBServerPlugin: AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            String[] propertys = { "FPrdOrgId", "FLot", "FMoNumber" };
            foreach (String property in propertys)
            {
                e.FieldKeys.Add(property);
            }
        }
        public override void AfterExecuteOperationTransaction(AfterExecuteOperationTransaction e)
        {
            base.AfterExecuteOperationTransaction(e);
            if (FormOperation.Operation.Equals("Save", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var dates in e.DataEntitys)
                {
                    if (dates["PrdOrgId_Id"].ToString() == "100026")
                    {
                        var entrys = dates["OptRptEntry"] as DynamicObjectCollection;
                        foreach(var entry in entrys)
                        {
                            if (entry["MoNumber"].ToString().Substring(0, 2) == "MO")
                            {
                                string rq = ((DynamicObject)entry["lot"])["Number"].ToString().Substring(0, 8);
                                string upsql = $@"update T_SFC_OPTRPTENTRY set F_260_PHRQ='{rq}' where FENTRYID='{entry["Id"].ToString()}'";
                                DBUtils.Execute(Context, upsql);
                            }                          
                        }
                    }
                }
            }
        }
    }
}
