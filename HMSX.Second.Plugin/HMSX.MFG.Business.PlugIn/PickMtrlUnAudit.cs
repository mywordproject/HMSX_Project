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

namespace HMSX.MFG.Business.PlugIn
{
    [Description("领料单反审核-反写派工BOM表")]
    public class PickMtrlUnAudit : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_260_PGMXID");//派工明细Id
            e.FieldKeys.Add("FActualQty");//实发数量
            e.FieldKeys.Add("FMaterialId");//物料
            e.FieldKeys.Add("F_RUJP_PgEntryId");
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            int pgEntryId = 0;
            decimal pickQty = 0;
            long materialId = 0;
            int _pgEntryIdPAD = 0;
            foreach (DynamicObject entity in e.DataEntitys)
            {
                if (entity != null)
                {
                    DynamicObjectCollection entrys = (DynamicObjectCollection)entity["Entity"];
                    foreach (DynamicObject obj in entrys)
                    {
                        pgEntryId =Convert.ToInt32(obj["F_260_PGMXID"]);
                        pickQty = Convert.ToDecimal(obj["ActualQty"]);
                        materialId = Convert.ToInt64(obj["MaterialId_Id"]);
                        _pgEntryIdPAD = Convert.ToInt32(obj["F_RUJP_PgEntryId"]);
                        if (pgEntryId>0)
                        {
                            string strSql = string.Format(@"update t_PgBomInfo set FPickQtyPc=FPickQtyPc-{0},FAllPickQty=FAllPickQty-{0},FAvailableQty=FAllPickQty-{0}+FAllFeedQty-FAllReturnQty where FPgEntryId={1} and FMaterialId={2}", pickQty, pgEntryId, materialId);
                            DBUtils.Execute(this.Context, strSql);
                        }
                        if (_pgEntryIdPAD > 0)
                        {
                            string strSql = string.Format(@"update t_PgBomInfo set FPickQty=FPickQty-{0},FAllPickQty=FAllPickQty-{0},FAvailableQty=FAllPickQty-{0}+FAllFeedQty-FAllReturnQty where FPgEntryId={1} and FMaterialId={2}", pickQty, _pgEntryIdPAD, materialId);
                            DBUtils.Execute(this.Context, strSql);
                        }
                        }
                }
            }
        }
    }
}
