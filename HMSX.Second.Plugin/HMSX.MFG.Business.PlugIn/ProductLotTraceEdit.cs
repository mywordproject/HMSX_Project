using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata.EntityElement;

namespace HMSX.MFG.Business.PlugIn
{
    [Description("生产批号追溯")]
    public  class ProductLotTraceEdit: AbstractDynamicFormPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.Equals("F_RUJP_Lot"))
            {
                DynamicObject lot = this.View.Model.GetValue("F_RUJP_Lot") as DynamicObject;
                long lot_Id = Convert.ToInt64(lot["F_RUJP_Lot_Id"]);
                string lot_Text = lot["F_RUJP_Lot_Text"].ToString();

                string strSql = string.Format(@"select  FBILLFORMID,FBILLNO,FBILLID,FBILLDATE,FBILLENTRYID,FBILLSEQ  from T_BD_LOTMASTERBILLTRACE where FLOTID={0}", lot_Id);
                DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
                if (rs.Count > 0)
                {
                    Entity entity = this.View.BillBusinessInfo.GetEntity("F_RUJP_Entity");
                    DynamicObjectCollection entityRows = (DynamicObjectCollection)this.View.Model.GetEntityDataObject(entity);
                    



                }
            }
        }

     
    }
}
