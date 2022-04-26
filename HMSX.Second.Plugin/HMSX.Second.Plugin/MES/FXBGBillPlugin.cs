using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Mobile.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin.MES
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("返修报工--带出批号、派工明细")]
    public class FXBGBillPlugin: AbstractMobileBillPlugin
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
        }
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
            //var x = e.Paramter.GetCustomParameter("F_PAEZ_Lable");
            //var y = e.Paramter.GetCustomParameter("F_RUJP_Lot");
            //var z = e.Paramter.GetCustomParameter("FData_F_RUJP_Lot");
        }   
    }
}
