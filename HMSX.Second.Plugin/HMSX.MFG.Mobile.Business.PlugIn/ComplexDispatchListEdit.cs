using Kingdee.BOS.Mobile.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Resource;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS;
using System.Data;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Utils;
using Kingdee.BOS.Core.Metadata;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCEntity;
using Kingdee.K3.MFG.Mobile.ServiceHelper;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using System.ComponentModel;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.K3.MFG.ServiceHelper.SFS;
using Kingdee.K3.Core.MFG.SFS.ParamOption;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.ComplexCacheJson.Utils;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.ComplexCacheJson.Model;

namespace HMSX.MFG.Mobile.Business.PlugIn
{
    [Description("领料扫码-表单插件")]
    public class ComplexDispatchListEdit : AbstractMobilePlugin
    {
        string FEntryId;//派工明细EntryId
        string MoBillNo = "";//生产订单号
        string MoBillEntrySeq = "";//生产订明细行号
        List<pickinfo> pickinfoList = new List<pickinfo>();
        protected FormCacheModel cacheModel4Save = new FormCacheModel();
        protected bool HasCached;

        protected int TotalPageNumber;//总页数
        /// <summary>
        /// 列表默认行数
        /// </summary>
        protected virtual int RowCountPerPage
        {
            get
            {
                return 7;
            }
        }
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
            FEntryId = e.Paramter.GetCustomParameter("FPgEntryId").ToString();

            //获取生产订单编号，生产订单行号
            string strSql = string.Format(@"SELECT FMOBILLNO,FMOSEQ FROM T_SFC_DISPATCHDETAIL WHERE FID=(SELECT TOP 1 FID FROM T_SFC_DISPATCHDETAILENTRY WHERE FENTRYID IN ({0}))", FEntryId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            if (rs.Count > 0)
            {
                MoBillNo = rs[0]["FMOBILLNO"].ToString();
                MoBillEntrySeq = rs[0]["FMOSEQ"].ToString();
            }

            this.View.GetControl("FText_MaterialNumberScan").SetCustomPropertyValue("showKeyboard", true);
            this.View.GetControl("FLable_User").SetValue(this.Context.UserName);
            this.InitFocus();
        }
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.View.GetControl("F_SBID_MobileListViewEntity").SetCustomPropertyValue("listEditable", true);
            FillData();
            this.InitFocus();
        }

        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            base.BeforeUpdateValue(e);
            this.ScanCodeChanged(e);
        }
        private void ScanCodeChanged(BeforeUpdateValueEventArgs e)
        {
            // base.ClearDicFilterValues();
            try
            {
                string key;
                if ((key = e.Key) != null)
                {
                    if (key == "FText_MaterialNumberScan")
                    {
                        string text = Convert.ToString(e.Value);
                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                        {
                            updateEntry(text);
                            e.Value = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                e.Value = string.Empty;
                //this.CurrOptPlanScanCode = string.Empty;
                this.View.ShowStatusBarInfo(ex.Message);
            }
            this.View.GetControl(e.Key).SetFocus();
        }


        protected virtual void InitFocus()
        {
            if (this.View.BusinessInfo.ContainsKey("FText_MaterialNumberScan"))
            {
                this.View.GetControl("FText_MaterialNumberScan").SetFocus();
            }
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            string key;
            switch (key = e.Key.ToUpper())
            {
                case "FBUTTON_MATERIALNUMBERSCAN":
                    string scanText = this.View.Model.GetValue("FText_MaterialNumberScan").ToString();
                    updateEntry(scanText);
                    this.View.Model.SetValue("FText_MaterialNumberScan", " ");
                    this.View.UpdateView("FText_MaterialNumberScan");
                    this.InitFocus();
                    return;
                case "FBUTTON_RETURN":
                    this.View.Close();
                    return;
                case "FBUTTON_LOGOUT":
                    JsonCacheUtils.DeleteCache(base.Context, this.cacheModel4Save.DeviceCode, this.HasCached);
                    LoginUtils.LogOut(base.Context, base.View);
                    base.View.Logoff("indexforpad.aspx");
                    return;
                case "FSUBMIT":
                    this.Confirm();
                    return;
            }
        }

        public void FillData()
        {
            List<DynamicObject> PPBomInfo = this.GetPPBomInfo(MoBillNo, MoBillEntrySeq);
            var ppBominfosum = (from p in PPBomInfo select new { materialid = Convert.ToInt64(p["FMATERIALID"]), stockId = Convert.ToInt64(p["FSTOCKID"]) }).Distinct().ToList();
            foreach (var pp in ppBominfosum)
            {

                string strSql = string.Format(@"SELECT  t.FStockOrgId,t.FStockId,t.FMaterialId,t.FLot,t1.FNUMBER,t.FBASEUNITID,t.FBASEQTY FROM T_STK_INVENTORY t 
                                                                  LEFT JOIN T_BD_LOTMASTER T1 ON t.FLOT=t1.FLOTID  AND t.FMaterialId=T1.FmaterialId
                                                                  WHERE t.FSTOCKSTATUSID=10000  AND  t.FStockId={0} AND  t.FMaterialId={1}  AND t.FBASEQTY>0 
                                                                  ORDER BY FNUMBER ASC", pp.stockId, pp.materialid);
                DynamicObjectCollection stockrs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
                var PPBomInfotmp = (from p in PPBomInfo where Convert.ToInt64(p["FMATERIALID"]) == pp.materialid select p);
                DynamicObjectCollection tmp = stockrs;
                foreach (DynamicObject obj in PPBomInfotmp)
                {
                    Decimal mustQty = Convert.ToDecimal(obj["FMustQty"]) - Convert.ToDecimal(obj["FPickQty"]);
                    DynamicObjectCollection rs = tmp;
                    for (int i = 0; i < rs.Count; i++)
                    {
                        if (Convert.ToDecimal(rs[i]["FBASEQTY"]) <= 0)
                        {
                            continue;
                        }
                        pickinfo pInfo = new pickinfo();
                        pInfo.MONumber = obj["FMOBillNO"].ToString();
                        pInfo.MaterialNumber = obj["FNUMBER"].ToString();
                        pInfo.MaterialName = obj["FNAME"].ToString();
                        pInfo.Model = obj["FSPECIFICATION"].ToString();
                        pInfo.MustQty = mustQty;

                        if (rs[i]["FNUMBER"] != null)
                        {
                            pInfo.lot = rs[i]["FNUMBER"].ToString();
                        }

                        if (rs[i]["FBASEQTY"] != null)
                        {
                            pInfo.stockQty = Convert.ToDecimal(rs[i]["FBASEQTY"]);
                            pInfo.baseUnitId = Convert.ToInt64(rs[i]["FBASEUNITID"]);
                        }
                        else { }
                        if (Convert.ToDecimal(rs[i]["FBASEQTY"]) >= mustQty)
                        {
                            pInfo.Qty = mustQty;
                            tmp[i]["FBASEQTY"] = Convert.ToDecimal(rs[i]["FBASEQTY"]) - mustQty;
                            mustQty = 0;
                        }
                        else
                        {
                            pInfo.Qty = Convert.ToDecimal(rs[i]["FBASEQTY"]);
                            mustQty = mustQty - Convert.ToDecimal(rs[i]["FBASEQTY"]);
                            tmp[i]["FBASEQTY"] = 0;
                            //tmp.RemoveAt(i);
                        }
                        pInfo.pbomEntryId = Convert.ToInt64(obj["FENTRYID"]);
                        pInfo.pgEntryId = Convert.ToInt64(obj["FPgEntryId"]);

                        pickinfoList.Add(pInfo);
                        if (mustQty == 0)
                        {
                            break;
                        }
                    }
                   
                }
            }

            pickinfoList = pickinfoList.OrderBy(p => p.MaterialNumber).ThenBy(p => p.lot).ToList();
            if (pickinfoList.Count > 0)
            {
                string number = pickinfoList[0].MaterialNumber.ToString();
                Decimal allqty = 0;
                for (int i = 0; i < pickinfoList.Count; i++)
                {
                    this.View.Model.CreateNewEntryRow("F_SBID_MobileListViewEntity");
                    int rowCount = this.View.Model.GetEntryRowCount("F_SBID_MobileListViewEntity");
                    int Seq = i + 1;
                    this.View.Model.SetValue("FSeq", Seq, i);
                    this.View.Model.SetValue("FMONumber", pickinfoList[i].MONumber.ToString(), i);
                    this.View.Model.SetValue("FMaterialNumber", pickinfoList[i].MaterialNumber.ToString(), i);
                    this.View.Model.SetValue("FMaterialName", pickinfoList[i].MaterialName.ToString(), i);
                    this.View.Model.SetValue("FModel", pickinfoList[i].Model.ToString(), i);
                    this.View.Model.SetValue("FLot", pickinfoList[i].lot, i);
                    this.View.Model.SetValue("FBaseUnitID", pickinfoList[i].baseUnitId, i);
                    this.View.Model.SetValue("FMustQty", pickinfoList[i].MustQty, i);
                    this.View.Model.SetValue("FQty", pickinfoList[i].Qty, i);
                    allqty = allqty + Convert.ToDecimal(pickinfoList[i].Qty);
                    this.View.Model.SetValue("FStockQty", pickinfoList[i].stockQty, i);
                    this.View.Model.SetValue("FPBomEntryId", pickinfoList[i].pbomEntryId, i);
                    this.View.Model.SetValue("FPgEntryId", pickinfoList[i].pgEntryId, i);

                    if (i == 0)
                    {
                        this.View.Model.SetValue("FIsParent", "1", i);
                    }
                    if (i > 0 && number == pickinfoList[i].MaterialNumber.ToString())
                    {
                        this.View.Model.SetValue("FIsParent", "0", i);
                    }
                    if (i > 0 && number != pickinfoList[i].MaterialNumber.ToString())
                    {
                        this.View.Model.SetValue("FIsParent", "1", i);
                        number = pickinfoList[i].MaterialNumber.ToString();
                    }
                    this.View.UpdateView("F_SBID_MobileListViewEntity");
                }
                this.View.Model.SetValue("FAllQty", allqty);
                this.View.UpdateView("FAllQty");
            }
        }


        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key == "FQty")
            {
                decimal mustQty = 0;
                decimal stockQty = 0;
                mustQty = Convert.ToDecimal(this.View.Model.GetValue("FMustQty", e.Row));
                stockQty = Convert.ToDecimal(this.View.Model.GetValue("FStockQty", e.Row));
                if (stockQty > mustQty)
                {
                    if (!(Convert.ToDecimal(e.NewValue) >= mustQty * Convert.ToDecimal(0.98) && Convert.ToDecimal(e.NewValue) <= mustQty * Convert.ToDecimal(1.02)))
                    {
                        this.View.ShowMessage("录入的领料数量不在范围内！");
                        this.View.Model.SetValue("FIsScan", "", e.Row);
                    }
                }
            }
        }

        public void updateEntry(string ScanText)
        {
            if (ScanText != "")
            {
                string sql = "";
                if (ScanText.Substring(0, 2) == "PG")
                {
                    sql = string.Format(@"SELECT T.FMATERIALID,T1.FNUMBER,T.FLOT,T.FLOT_TEXT FROM T_SFC_OPTRPTENTRY T INNER JOIN T_BD_MATERIAL T1 ON T.FMATERIALID=T1.FMATERIALID  WHERE F_SBID_BARCODE='{0}'", ScanText);
                }
                else
                {
                    sql = string.Format("SELECT T.FMATERIALID,T1.FNUMBER,T.FLOT,T.FLOT_TEXT FROM T_BD_BARCODEMAIN T INNER JOIN T_BD_MATERIAL  T1 ON T.FMATERIALID=T1.FMATERIALID  WHERE FBARCODE='{0}'", ScanText);
                }
                DynamicObjectCollection rsCodes = DBServiceHelper.ExecuteDynamicObject(this.Context, sql);
                if (rsCodes.Count > 0)
                {
                    if (pickinfoList.Count > 0)
                    {
                        int tmp = 0;
                        for (int i = 0; i < pickinfoList.Count; i++)
                        {
                            string lot1 = "";
                            string lot2 = "";
                            if (Convert.ToInt64(rsCodes[0]["FLot"]) == 0 && rsCodes[0]["FLOT_TEXT"] != null)
                            {
                                lot2 = rsCodes[0]["FLOT_TEXT"].ToString();
                            }
                            if (Convert.ToInt64(rsCodes[0]["FLot"]) > 0)
                            {
                                string strSql1 = string.Format(@"SELECT FNUMBER FROM T_BD_LOTMASTER where FLOTID={0} AND FMATERIALID={1}", Convert.ToInt64(rsCodes[0]["FLot"]), Convert.ToInt64(rsCodes[0]["FMATERIALID"]));
                                DynamicObjectCollection rsLots = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql1);
                                lot2 = rsLots[0]["FNUMBER"].ToString();
                            }
                            if (pickinfoList[i].lot != null)
                            {
                                lot1 = pickinfoList[i].lot;
                            }
                            else { lot1 = " "; }
                            string number1 = "";
                            string number2 = "";
                            if (pickinfoList[i].MaterialNumber != null)
                            {
                                number1 = pickinfoList[i].MaterialNumber.ToString();
                            }
                            if (rsCodes[0]["FNUMBER"] != null)
                            {
                                number2 = rsCodes[0]["FNUMBER"].ToString();
                            }
                            if (number1 == number2 && lot1 == lot2)
                            {
                                tmp = tmp + 1;
                                this.View.Model.SetValue("FIsScan", "Y", i);
                                this.View.UpdateView("F_SBID_MobileListViewEntity");
                            }

                        }
                        if (tmp == 0)
                        {
                            base.View.ShowMessage(ResManager.LoadKDString("匹配不成功！", "015747000028226", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                            return;
                        }

                    }

                }
                else
                {
                    base.View.ShowMessage(ResManager.LoadKDString("条码不存在！", "015747000028226", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                }
            }
        }

        /// <summary>
        ///  获取用料清单信息
        /// </summary>
        /// <param name="MoBillNo"></param>
        /// <param name="MoBillEntrySeq"></param>
        /// <returns></returns>
        private List<DynamicObject> GetPPBomInfo(string MoBillNo, string MoBillEntrySeq)
        {
            string strSql = string.Format(@"SELECT T.FPRDORGID,T.FMOBillNO,T.FMOENTRYSEQ,T1.FSEQ,T1.FID,T1.FENTRYID,T1.FMATERIALID,T1.FMaterialType,T1.FREPLACEGROUP,T3.FMASTERID,T3.FNUMBER,T4.FNAME,T4.FSPECIFICATION,T2.FPICKEDQTY,T5.FSTOCKID,T1.FNUMERATOR,T1.FDENOMINATOR,T1.FSCRAPRATE,T6.FMustQty,T6.FAvailableQty as FPickQty ,T6.FPgEntryId  FROM T_PRD_PPBOM T 
                                                             INNER JOIN T_PRD_PPBOMENTRY T1 ON T.FID=T1.FID 
                                                             INNER JOIN T_PRD_PPBOMENTRY_Q T2 ON T1.FID=T2.FID AND T1.FENTRYID=T2.FENTRYID  AND T1.FMUSTQTY>T2.FPICKEDQTY
                                                             INNER JOIN T_PRD_PPBOMENTRY_C T5 ON T1.FID=T5.FID AND T1.FENTRYID=T5.FENTRYID
                                                             INNER JOIN T_BD_MATERIAL T3 ON T1.FMATERIALID=T3.FMATERIALID  AND T3.FMATERIALID NOT IN (SELECT FMATERIALID FROM T_BD_MATERIALBASE WHERE FErpClsID=5 )
                                                             INNER JOIN T_BD_MATERIAL_L T4 ON T1.FMATERIALID=T4.FMATERIALID AND T4.FLOCALEID=2052
                                                             INNER JOIN t_PgBomInfo T6 ON T1.FENTRYID=T6.FPPBomEntryId AND T6.FPgEntryId IN ({0})   AND T6.FMustQty-T6.FAvailableQty>0
                                                             WHERE T.FMOBillNO='{1}' AND T.FMOENTRYSEQ={2} AND T5.FISSUETYPE IN ('1','3') 
                                                             ORDER BY T1.FMATERIALID ASC ", FEntryId, MoBillNo, MoBillEntrySeq);
            DynamicObjectCollection source = DBServiceHelper.ExecuteDynamicObject(base.Context, strSql);
            return source.ToList<DynamicObject>();
        }

        private List<DynamicObject> GetPPBomInfo(IEnumerable<long> ppBomEntryIds)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(" SELECT ");
            stringBuilder.AppendLine(" P.FID,P.FQTY,P.FBILLNO,PE.FSEQ,PE.FENTRYID,PE.FMUSTQTY,PEQ.FSELPICKEDQTY,PEQ.FGOODRETURNQTY,PEQ.FINCDEFECTRETURNQTY,P.FMOTYPE");
            stringBuilder.AppendLine(" FROM T_PRD_PPBOM P");
            stringBuilder.AppendLine(" INNER JOIN T_PRD_PPBOMENTRY PE  ON P.FID=PE.FID");
            stringBuilder.AppendLine(" INNER JOIN T_PRD_PPBOMENTRY_Q PEQ ON PE.FENTRYID=PEQ.FENTRYID");
            stringBuilder.AppendLine(" INNER JOIN T_PRD_PPBOMENTRY_C PEC ON PEQ.FENTRYID=PEC.FENTRYID");
            stringBuilder.AppendLine(" INNER JOIN (select /*+ cardinality(b " + ppBomEntryIds.Distinct<long>().ToArray<long>().Count<long>() + ")*/ FID from table(fn_StrSplit(@EntryId,',',1)) b) EntryId ON EntryId.FID=PE.FENTRYID");
            stringBuilder.AppendLine(" WHERE PEC.FISSUETYPE IN ('1','3')");
            List<SqlParam> list = new List<SqlParam>
    {
        new SqlParam("@EntryId", KDDbType.udt_inttable, ppBomEntryIds.Distinct<long>().ToArray<long>())
    };
            DynamicObjectCollection source = DBServiceHelper.ExecuteDynamicObject(base.Context, stringBuilder.ToString(), null, null, CommandType.Text, list.ToArray());
            return source.ToList<DynamicObject>();
        }
        /// <summary>
        /// 获取派工数量
        /// </summary>
        /// <param name="entryId"></param>
        /// <returns></returns>
        private decimal GetpgQty(string entryId)
        {
            Decimal sumQty = 0;
            string strSql = string.Format(@"SELECT SUM(FWORKQTY) AS FWORKQTY  FROM T_SFC_DISPATCHDETAILENTRY WHERE FENTRYID IN ({0}) ", entryId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            if (rs.Count > 0)
            {
                sumQty = Convert.ToDecimal(rs[0]["FWORKQTY"]);
            }
            return sumQty;
        }
        protected virtual void Confirm()
        {
            //获取已扫描为Y的数据
            List<long> entryIds = new List<long>();
            Entity entity = this.Model.BusinessInfo.GetEntity("F_SBID_MobileListViewEntity");
            DynamicObjectCollection rows = this.View.Model.GetEntityDataObject(entity);
            if (rows.Count > 0)
            {
                foreach (DynamicObject row in rows)
                {
                    if (row["FIsScan"] != null)
                    {
                        entryIds.Add(Convert.ToInt64(row["FPBomEntryId"]));
                    }
                }
            }
            if (entryIds.Count == 0)
            {
                base.View.ShowMessage(ResManager.LoadKDString("没有需要领料的分录！", "015747000026506", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);

            }
            else
            {
                List<DynamicObject> PPBomInfo = this.GetPPBomInfo(entryIds);
                IOperationResult result = this.CreatePickMtrl(PPBomInfo);
                this.PickSetResult(result);
            }
        }

        protected virtual IOperationResult CreatePickMtrl(List<DynamicObject> ppBomInfos)
        {
            Dictionary<long, decimal> dictionary = new Dictionary<long, decimal>();
            List<ListSelectedRow> list = new List<ListSelectedRow>();
            foreach (DynamicObject dynamicObject in ppBomInfos)
            {
                ListSelectedRow item = new ListSelectedRow(Convert.ToString(dynamicObject["FID"]), Convert.ToString(dynamicObject["FENTRYID"]), Convert.ToInt32(dynamicObject["FSEQ"]) - 1, "PRD_PPBOM")
                {
                    EntryEntityKey = "FEntity"

                };
                list.Add(item);

            }
            if (list.Count == 0)
            {
                base.View.ShowMessage(ResManager.LoadKDString("没有需要领料的分录！", "015747000026506", SubSystemType.MFG, new object[0]), MessageBoxType.Notice);
                return null;
            }
            ConvertOperationResult convertOperationResult;
            string convertRuleId = "PRD_PPBOM2PICKMTRL_NORMAL"; //
            var ruleMeta = ConvertServiceHelper.GetConvertRule(this.Context, convertRuleId);
            var rule = ruleMeta.Rule;
            PushArgs args = new PushArgs(rule, list.ToArray())
            {
                TargetBillTypeId = "f4f46eb78a7149b1b7e4de98586acb67",//普通领料
              
            };

            OperateOption operateOption = OperateOption.Create();
            convertOperationResult = MobileCommonServiceHelper.Push(this.Context, args, operateOption, false);
            DynamicObject[] array = (from p in convertOperationResult.TargetDataEntities
                                     select p.DataEntity).ToArray<DynamicObject>();
            Entity entity = this.Model.BusinessInfo.GetEntity("F_SBID_MobileListViewEntity");
            DynamicObjectCollection rows = this.View.Model.GetEntityDataObject(entity);
            foreach (DynamicObject obj in array)
            {
                DynamicObjectCollection dynamicObjectCollection = obj["Entity"] as DynamicObjectCollection;
                int rowcount = dynamicObjectCollection.Count;

                for (int i = 0; i < rowcount; i++)
                {
                    DynamicObject obj1 = dynamicObjectCollection[i];
                    Decimal num = 0L;
                    string lot;
                    foreach (DynamicObject rowObj in rows)
                    {
                        if (rowObj["FIsScan"] != null && rowObj["FIsScan"].ToString() == "Y")
                        {
                            if (rowObj["FIsParent"].ToString() == "1" && Convert.ToInt64(rowObj["FPBomEntryId"]) == Convert.ToInt64((obj1["PPBomEntryId"])))
                            {

                                num = Convert.ToDecimal(rowObj["FQty"]);
                                string strSql = string.Format(@"select FLOTID from T_BD_LOTMASTER where FNUMBER='{0}'", rowObj["FLot"].ToString());
                                DynamicObjectCollection rslot = DBServiceHelper.ExecuteDynamicObject(base.Context, strSql);
                                obj1["AppQty"] = num;
                                obj1["StockAppQty"] = num;
                                obj1["StockActualQty"] = num;
                                obj1["ActualQty"] = num;
                                obj1["BaseAppQty"] = num;
                                obj1["BaseActualQty"] = num;
                                obj1["BaseStockActualQty"] = num;
                                obj1["Lot_Id"] = Convert.ToInt64(rslot[0]["FLOTID"]);
                                obj1["Lot_Text"] = rowObj["FLot"].ToString();
                                obj1["F_RUJP_PgEntryId"] = rowObj["FPgEntryId"].ToString();
                            }
                            if (rowObj["FIsParent"].ToString() == "0" && Convert.ToInt64(rowObj["FPBomEntryId"]) == Convert.ToInt64((obj1["PPBomEntryId"])))
                            {
                                DynamicObject newRow = (DynamicObject)obj1.Clone(false, true);
                                num = Convert.ToDecimal(rowObj["FQty"]);
                                string strSql = string.Format(@"select FLOTID from T_BD_LOTMASTER where FNUMBER='{0}'", rowObj["FLot"].ToString());
                                DynamicObjectCollection rslot = DBServiceHelper.ExecuteDynamicObject(base.Context, strSql);
                                newRow["AppQty"] = num;
                                newRow["StockAppQty"] = num;
                                newRow["StockActualQty"] = num;
                                newRow["ActualQty"] = num;
                                newRow["BaseAppQty"] = num;
                                newRow["BaseActualQty"] = num;
                                newRow["BaseStockActualQty"] = num;
                                newRow["Lot_Id"] = Convert.ToInt64(rslot[0]["FLOTID"]);
                                newRow["Lot_Text"] = rowObj["FLot"].ToString();
                                newRow["F_RUJP_PgEntryId"] = rowObj["FPgEntryId"].ToString();
                                dynamicObjectCollection.Add(newRow);
                            }
                        }
                    }

                }
            }
            FormMetadata cachedFormMetaData = FormMetaDataCache.GetCachedFormMetaData(base.Context, "PRD_PickMtrl");
            OperateOption option = OperateOption.Create();
            option.AddInteractionFlag("Kingdee.K3.SCM.App.Core.AppBusinessService.UpdateStockService,Kingdee.K3.SCM.App.Core");
            option.SetIgnoreInteractionFlag(true);
            IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, cachedFormMetaData.BusinessInfo, array, option, "");
            if (operationResult.IsSuccess)
            {
                operationResult = BusinessDataServiceHelper.Submit(base.Context, cachedFormMetaData.BusinessInfo, (from o in array
                                                                                                                   select o["Id"]).ToArray<object>(), "Submit", null);
                operationResult = BusinessDataServiceHelper.Audit(base.Context, cachedFormMetaData.BusinessInfo, (from o in array
                                                                                                                  select o["Id"]).ToArray<object>(), option);
            }
            return operationResult;
        }


        protected virtual void PickSetResult(IOperationResult result)
        {
            if (result == null)
            {
                return;
            }
            if (result.IsSuccess)
            {
                this.View.ShowStatusBarInfo(ResManager.LoadKDString("领料成功！", "015747000026623", SubSystemType.MFG, new object[0]));
                this.View.Close();
                return;
            }
            else
            {
                string text = string.Join(";", from o in result.OperateResult
                                               select o.Message);
                string text2 = string.Join(";", from o in result.ValidationErrors
                                                select o.Message);
                if (!text.IsNullOrEmptyOrWhiteSpace())
                {
                    if (!text2.IsNullOrEmptyOrWhiteSpace())
                    {
                        text = text + ";" + text2;
                    }
                    this.View.ShowMessage(ResManager.LoadKDString("操作失败！", "015747000026527", SubSystemType.MFG, new object[0]) + text, MessageBoxType.Notice);
                    return;
                }
                this.View.ShowMessage(ResManager.LoadKDString("操作失败！", "015747000026527", SubSystemType.MFG, new object[0]) + text2, MessageBoxType.Notice);
                return;
            }
        }


        internal class pickinfo
        {
            private static string _moNumber;
            private string _MaterialNumber;
            private string _MaterialName;
            private string _Model;
            private string _lot;
            private decimal _MustQty;
            private decimal _Qty;
            private decimal _stockQty;
            private long _pbomEntryId;
            private long _PgEntryId;
            private long _baseUnitId;
            private string _IsParent;

            /// <summary>
            /// 生产订单号
            /// </summary>
            public string MONumber
            {
                get
                {
                    return _moNumber;
                }
                set
                {
                    _moNumber = value;
                }
            }
            /// <summary>
            /// 物料编码
            /// </summary>
            public string MaterialNumber
            {
                get
                {
                    return _MaterialNumber;
                }
                set
                {
                    _MaterialNumber = value;
                }
            }

            /// <summary>
            /// 物料名称
            /// </summary>
            public string MaterialName
            {
                get
                {
                    return _MaterialName;
                }
                set
                {
                    _MaterialName = value;
                }
            }

            /// <summary>
            /// 规格型号
            /// </summary>
            public string Model
            {
                get { return _Model; }
                set { _Model = value; }
            }

            /// <summary> 
            /// 批号
            /// </summary>
            public string lot
            {
                get
                {
                    return _lot;
                }
                set
                {
                    _lot = value;
                }
            }

            /// <summary>
            /// 应领数量
            /// </summary>
            public decimal MustQty
            {
                get
                {
                    return _MustQty;
                }
                set
                {
                    _MustQty = value;
                }
            }
            /// <summary>
            /// 领料数量
            /// </summary>
            public decimal Qty
            {
                get
                {
                    return _Qty;
                }
                set
                {
                    _Qty = value;
                }
            }
            /// <summary>
            /// 库存数量
            /// </summary>
            public decimal stockQty
            {
                get { return _stockQty; }
                set { _stockQty = value; }
            }
            /// <summary>
            /// 用料清单分录Id
            /// </summary>
            public long pbomEntryId
            {
                get
                {
                    return _pbomEntryId;
                }
                set
                {
                    _pbomEntryId = value;
                }
            }
            /// <summary>
            /// 派工明细分录Id
            /// </summary>
            public long pgEntryId
            {
                get { return _PgEntryId; }
                set { _PgEntryId = value; }
            }
            /// <summary>
            /// 基本计量单位
            /// </summary>
            public long baseUnitId
            {
                get { return _baseUnitId; }
                set { _baseUnitId = value; }
            }

            /// <summary>
            /// 是否父分录
            /// </summary>
            public string IsParent
            {
                get { return _IsParent; }
                set { _IsParent = value; }
            }

        }
    }
}

    
