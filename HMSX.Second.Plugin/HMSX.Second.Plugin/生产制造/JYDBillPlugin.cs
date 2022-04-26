﻿using Kingdee.BOS.Core.Bill.PlugIn;
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
    [Description("检验单--状态同步")]
    //热启动,不用重启IIS
    //python
    [Kingdee.BOS.Util.HotUpdate]
    public class JYDBillPlugin: AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key == "FPolicyStatus" && ((DynamicObject)this.Model.GetValue("FINSPECTORGID"))["Id"].ToString() == "100026")
            {

                int xh =Convert.ToInt32(this.View.Model.GetEntryCurrentRowIndex("FEntity").ToString());
                this.Model.SetValue("FINSPECTRESULT", e.NewValue,xh);
                this.View.UpdateView("FInspectResult");
                this.View.InvokeFieldUpdateService("FInspectResult", xh);
            }
        }
    }
}
