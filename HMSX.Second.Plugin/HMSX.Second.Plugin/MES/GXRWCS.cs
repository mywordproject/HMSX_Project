using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Mobile.PlugIn.ControlModel;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Complex;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Mobile;
using Kingdee.BOS.Mobile.PlugIn;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.SFC;
using Kingdee.K3.Core.Mobile.Utils;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCDymObjManager.SFC.Bill;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCUtils;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.DataModel;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Utils;
using Kingdee.K3.MFG.Mobile.ServiceHelper;
using Kingdee.K3.MFG.Mobile.ServiceHelper.SFC;
using Kingdee.K3.MFG.ServiceHelper.SFC;
using Kingdee.K3.MFG.ServiceHelper.SFS;
using Kingdee.K3.MFG.SFC.Common.Core.EnumConst.Mobile;
using System.Data;
using System.Reflection;


namespace HMSX.Second.Plugin.MES
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("工序任务超市--领料数大于零，不允许关闭")]
    public class GXRWCS: ComplexTaskPoolList
    {
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            if (e.Key.ToUpper() == "FBTN_CLOSE1")
            {
                //int rowIndex = this.Model.GetEntryCurrentRowIndex("FMobileListViewEntity_Detail");
                Dictionary<string, object> currentRowData = this.GetCurrentRowData();
                if (currentRowData != null)
                {   
                    if (Convert.ToDouble(currentRowData["F_260_LLSL"].ToString()) > 0)
                    {
                        throw new KDBusinessException("", "领料数量大于0不允许关闭！");
                    }
                    else
                    {
                        this.CloseRow(false);
                        return;
                    }
                }
            }
                   
        }	
	}
}
