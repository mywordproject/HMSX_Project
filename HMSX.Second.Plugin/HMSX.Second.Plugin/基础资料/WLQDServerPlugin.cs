﻿using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin
{
    [Description("物料清单--审核时，如果02，更新物料上的日期")]
    //热启动,不用重启IIS
    //python
    [Kingdee.BOS.Util.HotUpdate]
    public class WLQDServerPlugin : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            String[] propertys = { "FCreateOrgId", "FMATERIALID" , "FCreateDate" };
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
                if (date["CreateOrgId_Id"].ToString() == "100026" && ((DynamicObject)date["MATERIALID"])["Number"].ToString().Contains("260.02"))
                {
                    string countsql = $@"select top 1 FCREATEDATE from T_ENG_BOM where FMATERIALID = '{date["MATERIALID_Id"].ToString()}' order by FCREATEDATE asc";                   
                    var cont = DBUtils.ExecuteDynamicObject(Context, countsql);
                    if (cont.Count >0)
                    {
                        DateTime createtime = Convert.ToDateTime(cont[0]["FCREATEDATE"].ToString());
                       string upsql = $@"update T_BD_MATERIAL set F_260_LCKSDATE='{createtime.AddDays(5)}'  where FMATERIALID='{date["MATERIALID_Id"].ToString()}'";
                        DBUtils.Execute(Context, upsql);
                    }                   
                }
            }
        }

    }
}
