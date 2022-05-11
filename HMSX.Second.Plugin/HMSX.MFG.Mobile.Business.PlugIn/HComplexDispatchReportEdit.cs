
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Utils;
using Kingdee.BOS;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Core.Metadata;
using System.ComponentModel;
using Kingdee.BOS.Mobile.PlugIn;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.ConvertElement;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.K3.MFG.Mobile.ServiceHelper;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Resource;

namespace HMSX.MFG.Mobile.Business.PlugIn
{
    [Description("批量报工编辑-表单插件")]

    public  class HComplexDispatchReportEdit: AbstractMobilePlugin
    {
        protected long materialId;
        protected long deptId;
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
         this.View.GetControl("FLable_User").SetValue(this.Context.UserName);
        }
       
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.View.GetControl("FMobileListViewEntity").SetCustomPropertyValue("listEditable", true);
            List<Dictionary<string, object>> entrys = this.View.OpenParameter.GetCustomParameter("DataPacket") as List<Dictionary<string, object>>;
            int i = 0;
            Decimal allqty = 0;
            foreach (Dictionary<string, object> row in entrys)
            {
                
                this.View.Model.CreateNewEntryRow("FMobileListViewEntity");
                this.View.Model.SetValue("FSeq", i + 1, i);
                this.View.Model.SetValue("FMoNumber", row["FMONumber"].ToString(), i);
                this.View.Model.SetValue("FOperPlanNo", row["FOperPlanNo"].ToString(), i);
                this.View.Model.SetValue("FProductId", row["FProductId"].ToString(), i);
                this.View.Model.SetValue("FLot", row["F_RUJP_Lot"].ToString(), i);
                this.View.Model.SetValue("FQty", Convert.ToDecimal(row["FWorkQty"]), i);
                DynamicObject dispatchDetail = this.GetDeDispatchDetailEntry(Convert.ToInt64(row["EntryPkId"]));
                if (dispatchDetail != null)
                {
                    this.View.Model.SetValue("FOperator", Convert.ToInt64(dispatchDetail["FEMPID"]), i);
                    this.View.Model.SetValue("FMouldId", Convert.ToInt64(dispatchDetail["FMouldId"]), i);
                    this.View.Model.SetValue("FPgBarCode", dispatchDetail["FBARCODE"].ToString(), i);
                    materialId = Convert.ToInt64(dispatchDetail["FMATERIALID"]);
                   
                }
                deptId = this.GetDeptId(Convert.ToInt64(row["FOptPlanId"]));
                this.View.Model.SetValue("FOptPlanId", Convert.ToDecimal(row["FOptPlanId"]), i);
                this.View.Model.SetValue("FOptPlanOptId", Convert.ToDecimal(row["FOptPlanOptId"]), i);
                this.View.Model.SetValue("FOptPlanOptSeq", Convert.ToDecimal(row["FOptPlanOptSeq"]), i);
                this.View.Model.SetValue("FDispatchEntryId", Convert.ToDecimal(row["EntryPkId"]), i);
                allqty = allqty + Convert.ToDecimal(row["FWorkQty"]);
                
                this.View.UpdateView("FMobileListViewEntity");
                i++;
            }
            this.View.Model.SetValue("FAllQty", allqty);
            this.View.UpdateView("FAllQty");
        }

        private DynamicObject GetDeDispatchDetailEntry(long entryId)
        {
            string strSql = string.Format(@"select t2.FMATERIALID,t.FMouldId,t.FBARCODE,t1.FEMPID from T_SFC_DISPATCHDETAILENTRY t inner join T_SFC_DISPATCHDETAILENTRYEMPS t1 on t.FENTRYID=t1.FENTRYID inner join T_SFC_DISPATCHDETAIL t2 on t.FID=t2.FID where t.FentryId={0}", entryId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            DynamicObject dispatchDetail = null;
            if (rs.Count ==1)
            {
                 dispatchDetail=rs[0] ;
            }
            return dispatchDetail;
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            string a = e.Field.Key;
            //客户标签
            if (a == "FHMSXKHBQYD")
            {
                int rowCount = this.View.Model.GetEntryRowCount("FMobileListViewEntity");
                for (int i = 0; i < rowCount; i++)
                {
                    this.View.Model.SetValue("FKHBQ", Convert.ToInt64(e.NewValue),i);
                }
            }
            // 设备
            if (a == "FEquipmentId")
            {
                int rowCount = this.View.Model.GetEntryRowCount("FMobileListViewEntity");
                for (int i = 0; i < rowCount; i++)
                {
                    this.View.Model.SetValue("F_EquipmentId", Convert.ToInt64(e.NewValue), i);
                }
            }
            //操作工
            if (a == "FOperatorId")
            {
              
                int rowCount = this.View.Model.GetEntryRowCount("FMobileListViewEntity");
                //string[] pkValues =new string[] { e.NewValue.ToString() };
                string[] pkValues =  e.NewValue as string[];
                for (int i = 0; i < rowCount; i++)
                {
                    //  if (pkValues.Length == 1)
                    //   {
                    //     this.View.Model.SetValue("FOperator",Convert.ToInt64( pkValues[0]), i);
                    //  }
                    this.View.Model.SetValue("FOperator", pkValues, i);
                }
            }

            if (e.Field.Key == "FQty")
            {
                int rowCount = this.View.Model.GetEntryRowCount("FMobileListViewEntity");
                Decimal allqty = 0;
                for (int i = 0; i < rowCount; i++)
                {
                    allqty = allqty + Convert.ToDecimal(this.View.Model.GetValue("FQty", i));
                }
                this.View.Model.SetValue("FAllQty", allqty);
                this.View.UpdateView("FAllQty");
            }
        }

        private DynamicObject GetOptplan(long operId)
        {
            SqlParam sqlParam = new SqlParam("@OperId", KDDbType.Int64, operId);
            long num = DBServiceHelper.ExecuteScalar<long>(base.Context, "SELECT S.FID FROM T_SFC_OPERPLANNINGDETAIL d INNER JOIN T_SFC_OPERPLANNINGSEQ s ON s.FENTRYID=d.FENTRYID WHERE d.FDETAILID=@OperId", 0L, new SqlParam[]
            {
              sqlParam
            });
            DynamicObjectType dynamicObjectType = ((FormMetadata)MetaDataServiceHelper.Load(base.Context, "SFC_OperationPlanning", true)).BusinessInfo.GetDynamicObjectType();
            DynamicObject optPlan = BusinessDataServiceHelper.LoadSingle(base.Context, num, dynamicObjectType, null);
            return optPlan;
        }

        private long GetDeptId(long OptPlanId)
        {
            long DeptId = 0;
            string strSql = string.Format(@"select FProDepartmentId from T_SFC_OPERPLANNING where FID={0}", OptPlanId);
            DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
            if (rs.Count == 1)
            {
                DeptId =Convert.ToInt64( rs[0]["FProDepartmentId"]);
            }
            return DeptId;
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);
            string a = e.Key.ToUpper();
            switch (a)
            {
                case "FBUTTON_RETURN":
                    this.View.Close();
                    return;
                case "FBUTTON_LOGOUT":
                    LoginUtils.LogOut(base.Context, base.View);
                    base.View.Logoff("indexforpad.aspx");
                    return;

                case "FBUTTON_CONFIRM":
                    if (ErrorInfoList().Count > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        //stringBuilder.AppendLine(ResManager.LoadKDString("报工失败！存在未领料批号：", "015747000015462", SubSystemType.MFG, new object[0]));
                        stringBuilder.AppendLine("报工失败！存在未领料批号：");
                        stringBuilder.AppendLine();
                        foreach (ErrorInfo error in ErrorInfoList())
                        {
                            //stringBuilder.AppendLine(error.seq);
                            stringBuilder.AppendLine(error.lot);
                            stringBuilder.AppendLine(" ");
                        }
                        
                        this.View.ShowMessage(stringBuilder.ToString(),MessageBoxOptions.OK, delegate (MessageBoxResult r)
                        {
                            if (r == MessageBoxResult.OK)
                            {
                                return;
                            }
                        },"",MessageBoxType.Notice);
                        
                    }
                    else
                    {
                        this.CreateOperationReport();
                    }
                    return;
            }
        }

        private  List<ErrorInfo> ErrorInfoList()
        {
            List<ErrorInfo> list = new List<ErrorInfo>();
            Entity entity = this.Model.BusinessInfo.GetEntity("FMobileListViewEntity");
            DynamicObjectCollection rows = this.View.Model.GetEntityDataObject(entity);
            foreach (DynamicObject row in rows)
            {
                string strSql = string.Format(@"SELECT count(FPgEntryId) as Fcount,sum(FAvailableQty) as Fqty, sum(FMustQty) as FMustQty FROM t_PgBomInfo WHERE FPgEntryId={0} ",Convert.ToInt64( row["FDispatchEntryId"]));
                //AND FMaterialId NOT IN (SELECT FMATERIALID FROM T_BD_MATERIAL WHERE FNUMBER like '260.07%'  )
                DynamicObjectCollection rs = DBServiceHelper.ExecuteDynamicObject(this.Context, strSql);
                if (rs.Count > 0)
                {
                    if (Convert.ToInt16(rs[0]["Fcount"]) > 0)
                    {
                        if (Convert.ToDecimal(rs[0]["Fqty"]) < Convert.ToDecimal(rs[0]["FMustQty"]) * Convert.ToDecimal(0.98))
                        {
                            ErrorInfo error = new ErrorInfo();
                            error.seq = row["FSeq"].ToString();
                            error.lot = row["FLot"].ToString();
                            list.Add(error);
                        }
                    }
                    else
                    {
                        ErrorInfo error = new ErrorInfo();
                        error.seq = row["FSeq"].ToString();
                        error.lot = row["FLot"].ToString();
                        list.Add(error);
                    }
                }
            }
            return list;
        }


        private void CreateOperationReport()
        {
            List<ListSelectedRow> list = new List<ListSelectedRow>();
            Entity entity = this.Model.BusinessInfo.GetEntity("FMobileListViewEntity");
            DynamicObjectCollection rows = this.View.Model.GetEntityDataObject(entity);
            //foreach (DynamicObject row in rows)
           // {
                ListSelectedRow listSelectedRow = new ListSelectedRow(Convert.ToString(rows[0]["FOptPlanId"]), Convert.ToString(rows[0]["FOptPlanOptId"]), Convert.ToInt32(rows[0]["FOptPlanOptSeq"]), "SFC_OperationPlanning")
                {
                    EntryEntityKey = "FSubEntity"
                };
            // list.Add(listSelectedRow);
            // }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["FSubEntity"] = Convert.ToString(rows[0]["FOptPlanOptId"]);
            listSelectedRow.FieldValues = dictionary;
            ConvertRuleElement rule = ConvertServiceHelper.GetConvertRules(base.Context, "SFC_OperationPlanning", "SFC_OperationReport").Find((ConvertRuleElement f) => f.IsDefault);
            PushArgs serviceArgs = new PushArgs(rule, new ListSelectedRow[]
        {
            listSelectedRow
        });
            OperateOption operateOption = OperateOption.Create();
            operateOption.SetVariableValue("IsMobileInvoke", true);
            operateOption.SetVariableValue("MobileBizType", "Dispatch");
            operateOption.SetVariableValue("DispatchEntryId", Convert.ToInt64(rows[0]["FDispatchEntryId"])); 
            ConvertOperationResult convertOperationResult = MobileCommonServiceHelper.Push(base.Context, serviceArgs, operateOption, false);
            if (convertOperationResult.IsSuccess)
            {
                DynamicObject dataEntity = convertOperationResult.TargetDataEntities.FirstOrDefault<ExtendedDataEntity>().DataEntity;
                dataEntity["BillGenType"] = "C";
                DynamicObjectCollection dynamicObjectCollection = dataEntity["OptRptEntry"] as DynamicObjectCollection;
                for (int i = 0; i < rows.Count-1; i++)
                {
                    DynamicObject obj = dynamicObjectCollection.Last<DynamicObject>();
                    DynamicObject newRow = (DynamicObject)obj.Clone(false, true);
                    dynamicObjectCollection.Add(newRow);
                }

                int j = 0;
                foreach (DynamicObject obj in dynamicObjectCollection)
                {
                    obj["F_SBID_BARCODE"] = rows[j]["FPgBarCode"];
                    obj["BaseFinishQty"] = rows[j]["FQty"];
                    obj["PrdFinishQty"] = rows[j]["FQty"];
                    obj["WaitCheckQty"] = rows[j]["FQty"];
                    obj["FinishQty"] = rows[j]["FQty"];
                    obj["MLot_Text"] = rows[j]["FLOT"];
                    obj["Lot_Text"] = rows[j]["FLOT"];
                    obj["FHMSXKHBQYD_Id"] = rows[j]["FKHBQ_Id"];
                    obj["EquipmentId_Id"] = rows[j]["F_EquipmentId_Id"];
                    obj["EquipmentId"] = rows[j]["F_EquipmentId"];
                    DynamicObjectCollection objopers = obj["OperatorId"] as DynamicObjectCollection;
                    DynamicObjectCollection rowopers= rows[j]["FOperator"] as DynamicObjectCollection;
                    foreach (DynamicObject item in rowopers)
                    {
                        objopers.Add(item);
                    }
                  //  obj["OperatorId"] = rows[j]["FOperator"] as DynamicObjectCollection;
                    obj["DispatchDetailEntryId"] = rows[j]["FDispatchEntryId"];
                    j++;
                }
                FormMetadata cachedFormMetaData = FormMetaDataCache.GetCachedFormMetaData(base.Context, "SFC_OperationReport");
                OperateOption option = OperateOption.Create();
                option.SetVariableValue("MobileBizType", "Dispatch");
                option.SetVariableValue("IsMobileInvoke", true);
                option.SetVariableValue("AutoAudit", true);
                IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, cachedFormMetaData.BusinessInfo, dataEntity, option, "");
                if (operationResult.IsSuccess)
                {
                    base.View.ShowStatusBarInfo(ResManager.LoadKDString("报工成功！", "015747000015470", SubSystemType.MFG, new object[0]));
                    this.View.Close();
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    if (operationResult.ValidationErrors.Count > 0)
                    {
                        stringBuilder.AppendLine();
                        foreach (ValidationErrorInfo validationErrorInfo in operationResult.ValidationErrors)
                        {
                            stringBuilder.AppendLine(validationErrorInfo.Message);
                        }
                    }
                    base.View.ShowMessage(stringBuilder.ToString(), MessageBoxType.Notice);
                }
            }
        }

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            string a;
            if ((a = e.FieldKey.ToUpper()) != null)
            {
                //客户标签
                if (a == "FHMSXKHBQYD")
                {
                    this.SetKHBQFilter(e);
                    return;
                }
                if (a == "FKHBQ")
                {
                    this.SetKHBQFilter(e);
                    return;
                }
                if (a == "FOPERATORID")
                {
                    this.SetOperatorFilter(e);
                        return;
                }
                if (a == "FOPERATOR")
                {
                    this.SetOperatorFilter(e);
                    return;
                }
                ListShowParameter listShowParameter = e.DynamicFormShowParameter as ListShowParameter;
                //listShowParameter.UseOrgId = this.userOrgId;
                listShowParameter.ListFilterParameter.Filter = e.ListFilterParameter.Filter;
            }
        }
        private void SetKHBQFilter(BeforeF7SelectEventArgs e)
        {
            string text = string.Format(" F_HMD_BASEWLBM ={0} ", this.materialId);
            e.ListFilterParameter.Filter = text;
        }

        /// <summary>
        /// 操作工
        /// </summary>
        /// <param name="e"></param>
        private void SetOperatorFilter(BeforeF7SelectEventArgs e)
        {
            string text = string.Format(@" EXISTS(SELECT 1 FROM (SELECT DISTINCT SE.FID FROM T_BD_STAFFTEMP SE
                                                                      JOIN T_BD_STAFF ST ON ST.FEMPINFOID = SE.FID AND ST.FDOCUMENTSTATUS = 'C' 
                                                                      AND ST.FFORBIDSTATUS = 'A' AND SE.FDeptID = {0}) AS A WHERE A.FID=FID)",this.deptId);
            e.ListFilterParameter.Filter = text;
        }

        private long GetPrdOrgId(long operId)
        {
            string strSql = "SELECT FPROCESSORGID FROM T_SFC_OPERPLANNINGDETAIL WHERE FDETAILID = @FDetailID";
            SqlParam sqlParam = new SqlParam("@FDetailID", KDDbType.Int64, operId);
            return DBServiceHelper.ExecuteScalar<long>(base.Context, strSql, 0L, new SqlParam[]
            {
                sqlParam
            });
        }

        internal class ErrorInfo
        {
            private string _seq;
            private string _lot;
            /// <summary>
            ///  行号
            /// </summary>
            public string seq
            {
                get
                {
                    return _seq;
                }
                set
                {
                    _seq = value;
                }
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
        }
    }
}
