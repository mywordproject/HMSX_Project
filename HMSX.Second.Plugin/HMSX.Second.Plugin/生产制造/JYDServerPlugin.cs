using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DependencyRules;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin.生产制造
{
    [Description("检验单--反写检验结果、决策到汇报单")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class JYDServerPlugin : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            String[] propertys = { "FMemo1", "FUsePolicy", "F_260_HBFLNM", "F_260_BHGMS" };
            foreach (String property in propertys)
            {
                e.FieldKeys.Add(property);
            }
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);         
            foreach (var dates in e.DataEntitys)
            {
                if (dates["InspectOrgId_Id"].ToString() == "100026")
                {
                    var fentrys = dates["Entity"] as DynamicObjectCollection;
                    foreach (var fentry in fentrys)
                    {
                        string syjc = "";
                        string jyjg = "";
                        string bhgms = "";
                        var zfentrys = fentry["PolicyDetail"] as DynamicObjectCollection;
                        bhgms = fentry["F_260_BHGMS"].ToString();
                        foreach (var zfentry in zfentrys)
                        {
                            Field field = this.BusinessInfo.GetField("FUsePolicy");//转为ComboField
                            ComboField comboField = field as ComboField;//获取下拉列表字段绑定的枚举类型
                           var enumObj = (EnumObject)comboField.EnumObject;//根据枚举值获取枚举项，然后拿枚举项的枚举名称
                            var enumItemName = enumObj.Items.FirstOrDefault(p => p.Value.Equals(zfentry["UsePolicy"].ToString())).Caption.ToString();                           
                            syjc += enumItemName+";";
                            if (zfentry["Memo1"].ToString() != "")
                            {
                                jyjg += zfentry["Memo1"].ToString() + ";";
                            }
                             
                        }
                        string upsql = $@"update T_SFC_OPTRPTENTRY set F_260_JYSYJC='{syjc.Trim(';')}',F_260_JYJGMS='{jyjg.Trim(';')}',F_260_BHGMS='{bhgms}' where FENTRYID='{fentry["F_260_HBFLNM"].ToString()}'";
                        DBUtils.Execute(Context, upsql);
                    }
                }
            }
        }
    }
}
