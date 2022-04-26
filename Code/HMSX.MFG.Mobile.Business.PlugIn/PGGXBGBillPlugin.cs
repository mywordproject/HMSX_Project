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
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Utils;

namespace HMSX.MFG.Mobile.Business.PlugIn
{
    [Description("派工工序报工")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class PGGXBGBillPlugin:ComplexDispatchReportList
    {

        protected virtual void PrepareDispDetailBindFields(Dictionary<string, string> dicFieldLabelKeys)
        {
           // base.PrepareDispDetailBindFields(dicFieldLabelKeys);
         //   DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "F_RUJP_Lot");

        }
    }
}
