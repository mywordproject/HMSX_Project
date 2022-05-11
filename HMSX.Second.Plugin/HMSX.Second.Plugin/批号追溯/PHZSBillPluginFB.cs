using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Permission;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using HMSX.Second.Plugin.Tool;
using Kingdee.BOS.Core.DynamicForm.PlugIn;

namespace HMSX.Second.Plugin.批号追溯
{
    [Description("批号追溯11——单击过滤弹出对话框")]
    //热启动,不用重启IIS
    [Kingdee.BOS.Util.HotUpdate]
    public class PHZSBillPluginFB : AbstractDynamicFormPlugIn
    {
        //int X = 0;//判断向上追溯2还是向下1
        int hs = 1;
        int CJ = 2;
        /// <summary>
        /// </summary>
        /// <param name="e"></param>
        /**
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            DynamicFormShowParameter parameter = new DynamicFormShowParameter();
            parameter.OpenStyle.ShowType = ShowType.Floating;
            parameter.FormId = "RUJP_260_PHZSGL";
            parameter.MultiSelect = false;
            this.View.GetMainBarItem("RUJP_XS").Visible = false;
            this.View.GetMainBarItem("RUJP_XX").Visible = false;
            //获取返回的值
            this.View.ShowForm(parameter, delegate (FormResult result)
            {
                string[] date = (string[])result.ReturnData;
                if (date != null)
                {
                    this.Model.SetValue("F_260_WL", date[0]);
                    this.Model.SetValue("F_RUJP_PH", date[1]);
                    this.Model.SetValue("F_260_KSRQ", date[2]);
                    this.Model.SetValue("F_260_JSRQ", date[3]);
                    if (Convert.ToInt32(date[4]) == 1)
                    {
                        this.View.GetMainBarItem("RUJP_XX").Visible = true;
                    }
                    else
                    {
                        this.View.GetMainBarItem("RUJP_XS").Visible = true;
                    }
                    hs = 1;
                    this.Model.DeleteEntryData("F_RUJP_Entity");
                    this.Model.DeleteEntryData("F_RUJP_Entity1");
                    MLTC(date[0], date[1],0,1);
                    FZ(date[0], date[1], Convert.ToInt32(date[4]));
                    this.View.UpdateView("F_RUJP_Entity");
                    this.View.UpdateView("F_RUJP_Entity1");
                }
            });
        }
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);

            if (e.BarItemKey.Equals("RUJP_GL"))
            {
                DynamicFormShowParameter parameter = new DynamicFormShowParameter();
                parameter.OpenStyle.ShowType = ShowType.Floating;
                parameter.FormId = "RUJP_260_PHZSGL";
                parameter.MultiSelect = false;

                //获取返回的值
                this.View.ShowForm(parameter, delegate (FormResult result)
                {
                    string[] date = (string[])result.ReturnData;
                    if (date != null)
                    {
                        this.Model.SetValue("F_260_WL", date[0]);
                        this.Model.SetValue("F_RUJP_PH", date[1]);
                        this.Model.SetValue("F_260_KSRQ", date[2]);
                        this.Model.SetValue("F_260_JSRQ", date[3]);
                        if (Convert.ToInt32(date[4]) == 1)
                        {
                            this.View.GetMainBarItem("RUJP_XX").Visible = true;
                            this.View.GetMainBarItem("RUJP_XS").Visible = false;
                        }
                        else
                        {
                            this.View.GetMainBarItem("RUJP_XS").Visible = true;
                            this.View.GetMainBarItem("RUJP_XX").Visible = false;
                        }
                        hs = 1;
                        this.Model.DeleteEntryData("F_RUJP_Entity");
                        this.Model.DeleteEntryData("F_RUJP_Entity1");
                        MLTC(date[0], date[1], 0,1);
                        FZ(date[0], date[1], Convert.ToInt32(date[4]));
                        this.View.UpdateView("F_RUJP_Entity");
                        this.View.UpdateView("F_RUJP_Entity1");
                    }
                });
            }
            else if (e.BarItemKey.Equals("RUJP_XX"))
            {
                X = 2;
                hs = 1;
                string wl = this.Model.GetValue("F_260_WL") == null ? null : ((DynamicObject)this.Model.GetValue("F_260_WL"))["Id"].ToString();
                string ph = this.Model.GetValue("F_RUJP_PH") == null ? null : ((DynamicObject)this.Model.GetValue("F_RUJP_PH"))["Id"].ToString();
                string ksrq = this.Model.GetValue("F_260_KSRQ").ToString();
                string jsrq = this.Model.GetValue("F_260_JSRQ").ToString();
                if (wl != null && ph != null)
                {
                    this.Model.DeleteEntryData("F_RUJP_Entity");
                    this.Model.DeleteEntryData("F_RUJP_Entity1");
                    MLTC(wl, ph, 0,1);
                    FZ(wl, ph, X);
                    this.View.UpdateView("F_RUJP_Entity");
                    this.View.UpdateView("F_RUJP_Entity1");
                }
            }
            else if (e.BarItemKey.Equals("RUJP_XS"))
            {
                X = 1;
                hs = 1;
                string wl = this.Model.GetValue("F_260_WL") == null ? null : ((DynamicObject)this.Model.GetValue("F_260_WL"))["Id"].ToString();
                string ph = this.Model.GetValue("F_RUJP_PH") == null ? null : ((DynamicObject)this.Model.GetValue("F_RUJP_PH"))["Id"].ToString();
                string ksrq = this.Model.GetValue("F_260_KSRQ").ToString();
                string jsrq = this.Model.GetValue("F_260_JSRQ").ToString();
                if (wl != null && ph != null)
                {
                    hs = 1;
                    this.Model.DeleteEntryData("F_RUJP_Entity");
                    this.Model.DeleteEntryData("F_RUJP_Entity1");
                    MLTC(wl, ph, 0,1);
                    FZ(wl, ph, X);
                    this.View.UpdateView("F_RUJP_Entity");
                    this.View.UpdateView("F_RUJP_Entity1");
                }
            }
        }
        **/
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            DateTime dt = DateTime.Now;
            dt = dt.AddMonths(-3);
            this.Model.SetValue("F_260_KSRQ", dt);
            this.View.UpdateView("F_260_KSRQ");
        }
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            string wl = this.Model.GetValue("F_260_WL")==null?null:((DynamicObject)this.Model.GetValue("F_260_WL"))["Id"].ToString();
            string ph = this.Model.GetValue("F_RUJP_PH")==null?null: ((DynamicObject)this.Model.GetValue("F_RUJP_PH"))["Id"].ToString();
            string ksrq = this.Model.GetValue("F_260_KSRQ").ToString();
            string jsrq = this.Model.GetValue("F_260_JSRQ").ToString();
            string rksl = "";
            string rkslsql = $@"select FREALQTY from (
                               select FREALQTY from T_PRD_INSTOCKENTRY where FMATERIALID='{wl}' and FLOT='{ph}'
                               union all 
                               select b.FREALQTY from t_STK_InStock a--委外采购入库
                               inner join T_STK_INSTOCKENTRY b on a.FID=b.FID
                               inner join T_BAS_BILLTYPE c on c.FBILLTYPEID=a.FBILLTYPEID where FMATERIALID='{wl}' and FLOT='{ph}' and  
                               c.FNUMBER='RKD03_SYS'
                               and a.FDOCUMENTSTATUS='C' and a.FPURCHASEORGID=100026 )aa where FREALQTY>0 ";
            var rksls = DBUtils.ExecuteDynamicObject(Context, rkslsql);
            if (rksls.Count > 0)
            {
                rksl = rksls[0]["FREALQTY"].ToString();
            }
            if (e.Key.Equals("F_260_XS") && wl != null && ph != null && ksrq != null && jsrq != null)
            {
                this.Model.DeleteEntryData("F_RUJP_Entity");
                this.Model.DeleteEntryData("F_RUJP_Entity1");
                 hs = 1;
                MLTC(wl, ph, 0, 1, rksl);                
                FZ(wl, ph, 1);
                this.View.UpdateView("F_RUJP_Entity");
                this.View.UpdateView("F_RUJP_Entity1");
            }
            else if (e.Key.Equals("F_260_XX") && wl != null && ph != null && ksrq != null && jsrq != null)
            {
                this.Model.DeleteEntryData("F_RUJP_Entity");
                this.Model.DeleteEntryData("F_RUJP_Entity1");
                hs = 1;
                MLTC(wl, ph, 0, 1, rksl);
                FZ(wl, ph, 2);
                this.View.UpdateView("F_RUJP_Entity");
                this.View.UpdateView("F_RUJP_Entity1");
            }

        }
        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            if (e.FieldKey.EqualsIgnoreCase("F_RUJP_PH"))
            {
                string a = this.Model.GetValue("F_260_WL") == null ? null : ((DynamicObject)this.Model.GetValue("F_260_WL"))["Id"].ToString();
                string FMA = "FMATERIALID" + "=" + Convert.ToInt32(a);
                e.ListFilterParameter.Filter = e.ListFilterParameter.Filter.JoinFilterString(FMA);
                return;
            }
        }

        public override void EntryButtonCellClick(EntryButtonCellClickEventArgs e)
        {
            base.EntryButtonCellClick(e);
            if (!e.FieldKey.EqualsIgnoreCase("F_260_DJBH"))
            {
                return;
            }
            if (e.Row < 0)
            {
                return;
            }
            string number = ((DynamicObject)this.Model.GetValue("F_260_DJ", e.Row))["Number"].ToString();
            string formidsql = $@"SELECT FBILLFORMID,FNUMBER,FNAME FROM T_BAS_BILLTYPE A
            INNER JOIN T_BAS_BILLTYPE_L B ON A.FBILLTYPEID=B.FBILLTYPEID WHERE FNUMBER='{number}'";
            var formid = DBUtils.ExecuteDynamicObject(Context, formidsql);

            var formId = formid[0]["FBILLFORMID"].ToString();
            var requisitionMetadata = (FormMetadata)MetaDataServiceHelper.Load(this.Context, formId);
            var billNo = this.Model.GetValue(e.FieldKey, e.Row);
            var objs = BusinessDataServiceHelper.Load(this.Context, requisitionMetadata.BusinessInfo,
                new List<SelectorItemInfo>(new[] { new SelectorItemInfo("FID") }), OQLFilter.CreateHeadEntityFilter("FBillNo='" + billNo + "'"));
            if (objs == null || objs.Length == 0) { return; }
            var pkId = objs[0]["Id"].ToString(); var showParameter = new BillShowParameter
            {
                FormId = formId, // 业务对象标识              
                PKey = pkId, // 单据内码                
                Status = OperationStatus.VIEW // 查看模式打开                
                                              // Status = OperationStatus.EDIT// 编辑模式打开            
            };
            this.View.ShowForm(showParameter);
        }
        public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
        {
            base.EntityRowDoubleClick(e);
            if (e.Key.Equals("F_RUJP_ENTITY"))
            {
                var wl = this.Model.GetValue("F_RUJP_WL", e.Row) == null ? null : (DynamicObject)this.Model.GetValue("F_RUJP_WL", e.Row);
                var ph = this.Model.GetValue("F_RUJP_LOT", e.Row) == null ? null : (DynamicObject)this.Model.GetValue("F_RUJP_LOT", e.Row);
                string kssj = this.Model.GetValue("F_260_KSRQ") == null ? null : this.Model.GetValue("F_260_KSRQ").ToString();
                string jssj = this.Model.GetValue("F_260_JSRQ") == null ? null : this.Model.GetValue("F_260_JSRQ").ToString();
                if (wl != null && ph != null && kssj != null && jssj != null)
                {
                    int i = 0;//判断是否带批号
                    if (ph["Number"].ToString().IndexOf('-') > 0)
                    {
                        i = 1;
                        SJSJY(wl["Id"].ToString(), ph["Id"].ToString(), kssj, jssj, i);
                    }
                    else
                    {
                        i = 2;
                        SJSJY(wl["Id"].ToString(), ph["Id"].ToString(), kssj, jssj, i);
                    }

                }
            }
        }
        /// <summary>
        ///双击数据源
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void SJSJY(string wl, string lot, string kssj, string jssj, int i)
        {
            int xh = 0;
            this.Model.DeleteEntryData("F_RUJP_Entity1");
            string phnumber = (Tool.Utils.LoadBDData(Context, "BD_BatchMainFile", Convert.ToInt32(lot)))["Number"].ToString();
            string ybsql = $@"exec HMSX_260_PHZS '{wl}','{phnumber}','{kssj}','{jssj}',{i}";
            var DATES = DBUtils.ExecuteDynamicObject(Context, ybsql);
            foreach (var date in DATES)
            {
                this.Model.CreateNewEntryRow("F_RUJP_Entity1");
                this.View.Model.SetValue("F_260_DJMC", date["DJMC"], xh);
                this.View.Model.SetValue("F_260_DJ", date["DJLX"], xh);
                this.View.Model.SetItemValueByID("F_260_ZZ", Convert.ToInt32(date["ZZ"]), xh);
                this.View.Model.SetValue("F_260_DJBH", date["DJBH"].ToString(), xh);
                this.View.Model.SetValue("F_260_HH", date["HH"].ToString(), xh);
                this.View.Model.SetValue("F_260_RQ", Convert.ToDateTime(date["RQ"]), xh);
                this.View.Model.SetItemValueByID("F_260_DW", Convert.ToInt32(date["DW"]), xh);
                this.View.Model.SetValue("F_260_SL", Convert.ToDouble(date["SL"]), xh);
                this.View.Model.SetItemValueByID("F_260_CK", Convert.ToInt32(date["CK"]), xh);
                this.View.Model.SetItemValueByID("F_260_KCZT", Convert.ToInt32(date["KCZT"]), xh);
                this.View.Model.SetItemValueByID("F_260_SCCJ", Convert.ToInt32(date["SCCJ"]), xh);
                this.View.Model.SetValue("F_260_KHBQ", date["KHBQ"]==null? "":date["KHBQ"].ToString(), xh);
                this.View.Model.SetItemValueByID("F_260_DCCK", date["DCCK"].ToString() == "" ? 0 : Convert.ToInt32(date["DCCK"]), xh);
                xh++;
            }
            this.View.UpdateView("F_RUJP_Entity1");
        }
        /// <summary>
        ///目录填充
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
       
        public void FZ(string mater, string lot, int X)
        {        
            int p = hs - 1;
            
            var wl = Tool.Utils.LoadBDData(Context, "BD_MATERIAL", Convert.ToInt32(mater));
            //if (wl["Number"].ToString().Contains("260.01") == false)
            //{
                var piha = Tool.Utils.LoadBDData(Context, "BD_BatchMainFile", Convert.ToInt32(lot));
                string zjsql = $@"EXEC HMSX_260_PHZSML '{mater}','{piha["Number"].ToString()}',{X}";
                var boms = DBUtils.ExecuteDynamicObject(Context, zjsql);
                if (boms.Count > 0)
                {
                    CJ = 2;
                    foreach (var bom in boms)
                    {
                   
                        MLTC(bom["FMATERIALID"].ToString(), bom["FLOT"].ToString(), p,CJ,bom["FREALQTY"].ToString());
                        var wl1 = Tool.Utils.LoadBDData(Context, "BD_MATERIAL", Convert.ToInt32(bom["FMATERIALID"].ToString()));
                        if (wl1["Number"].ToString().Contains("260.01") == false)
                        {
                             ++CJ;
                            FZ(bom["FMATERIALID"].ToString(), bom["FLOT"].ToString(), X);
                        }
                    }
                }
         //  }

        }
        /// <summary>
        ///目录填充
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void MLTC(string wl, string lot, int id ,int CJ,string sl)
        {
            this.Model.CreateNewEntryRow("F_RUJP_Entity");
            this.View.Model.SetValue("FROWID",hs , hs - 1);
            this.View.Model.SetValue("F_260_CJ", CJ, hs - 1);
            this.View.Model.SetValue("FPARENTROWID", id, hs - 1);
            this.View.Model.SetValue("FROWEXPANDTYPE", 16, hs - 1);
            this.View.Model.SetItemValueByID("F_RUJP_WL",Convert.ToInt32(wl), hs - 1);
            this.View.Model.SetItemValueByID("F_RUJP_LOT", Convert.ToInt32(lot), hs - 1);
            this.View.Model.SetValue("F_260_RKSL", sl, hs - 1);
            hs++;
        }
    }
}

