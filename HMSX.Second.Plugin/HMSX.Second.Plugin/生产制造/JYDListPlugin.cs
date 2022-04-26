using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSX.Second.Plugin.生产制造
{
    [Description("检验单--批量修改")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public  class JYDListPlugin: AbstractListPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            if (e.BarItemKey.Equals("SLSB_PLXG"))
            {
                //选择的行,获取所有信息,放在listcoll里面
                ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;
                //定义一个字符串数组,接收分录FID的值
                string[] listKey = listcoll.GetEntryPrimaryKeyValues();
                if(listKey.Length==0)
                {
                    throw new KDBusinessException("", "请选择需要批量修改的行！");
                }

                DynamicObjectCollection dycoll = this.ListModel.GetData(listcoll);
                for (int i = 0; i < dycoll.Count; i++)
                {
                    if (dycoll[i]["FINSPECTORGID"].ToString()!="100026")
                    {
                        return;
                    }
                    if (dycoll[i]["FDOCUMENTSTATUS"].ToString()=="C" )
                    {
                        throw new KDBusinessException("", "单据状态不能已审核！");
                    }
                   
                }
                DynamicFormShowParameter parameter = new DynamicFormShowParameter();
                parameter.OpenStyle.ShowType = ShowType.Floating;
                parameter.FormId = "SLSB_PLXG";
                parameter.MultiSelect = false;
                //获取返回的值
                this.View.ShowForm(parameter, delegate (FormResult result)
                {
                string[] date = (string[])result.ReturnData;
                    if (date != null && date[0]=="1")
                    {
                        foreach (string key in listKey)
                        {
                            string upsql = $@"update T_QM_INSPECTBILLENTRY set F_260_BHGMS='{date[2]}' where FENTRYID={key}";
                            DBUtils.ExecuteDynamicObject(Context, upsql);
                        }
                        this.View.ShowMessage("批量修改成功！");
                    }
                    else if (date != null && date[0] == "2")
                    {
                        foreach (string key in listKey)
                        {
                            string cxsql = $@"select * from T_QM_IBPOLICYDETAIL_L WHERE FDetailID IN (SELECT FDetailID FROM T_QM_IBPOLICYDETAIL WHERE FENTRYID='{key}')";
                            var cx = DBUtils.ExecuteDynamicObject(Context, cxsql);
                            if (cx.Count > 0)
                            {
                                string upsql = $@"update T_QM_IBPOLICYDETAIL_L set FMEMO='{date[3]}' where FDetailID IN (SELECT FDetailID FROM T_QM_IBPOLICYDETAIL WHERE FENTRYID={key})";
                                DBUtils.ExecuteDynamicObject(Context, upsql);
                            }
                            else
                            {
                                string pkidsql = $@"select top 1 * from T_QM_IBPOLICYDETAIL_L order by fpkid desc";
                                var pkid = DBUtils.ExecuteDynamicObject(Context, pkidsql);
                                string fdidsql = $@"SELECT FDetailID FROM T_QM_IBPOLICYDETAIL WHERE FENTRYID='{key}'";
                                var fdid = DBUtils.ExecuteDynamicObject(Context, fdidsql);

                                string insertsql = $@"insert into T_QM_IBPOLICYDETAIL_L values({Convert.ToInt32(pkid[0]["FPKID"].ToString())+1},{Convert.ToInt32(fdid[0]["FDETAILID"].ToString())},2052,'{date[3]}')";
                                DBUtils.Execute(Context, insertsql);
                            }
                        }
                        this.View.ShowMessage("批量修改成功！");
                    }
                    else if (date != null && date[0] == "3")
                    {
                        foreach (string key in listKey)
                        {
                            string upsql = $@"update T_QM_IBPOLICYDETAIL set FUSEPOLICY='{date[1]}' where FENTRYID={key}";
                            DBUtils.ExecuteDynamicObject(Context, upsql);
                        }
                        this.View.ShowMessage("批量修改成功！");
                    }                  
                });
            }
        }

    }
}
