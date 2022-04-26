using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Mobile;
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
    [Description("派工工序报工")]
    public class PGGXBGModilPlugin:AbstractMobileBillPlugin
    {
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            if (e.Key.Equals("FBUTTON_REWORK"))
            {
              // string id = this.Model.DataObject["Id"].ToString();
              // var paramDy = new MobileShowParameter();
              // paramDy.FormId = "SFC_MobileComplexDispatchReportList";    //移动单据唯一标识
              // paramDy.PKey = id;  //单据内码
              // paramDy.CustomParams.Add("FLable8", "FData_FEmpId"); //往子窗口传递参数
              // paramDy.CustomParams.Add("F_PAEZ_Lable", "FData_F_RUJP_Lot");
                // this.View.ShowForm(paramDy);

            }
        }
    }
}
