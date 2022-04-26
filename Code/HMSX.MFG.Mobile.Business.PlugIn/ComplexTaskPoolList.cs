using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.NetworkCtrl;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Mobile;
using Kingdee.BOS.Mobile.PlugIn;
using Kingdee.BOS.Mobile.PlugIn.ControlModel;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.BD.ServiceHelper;
using Kingdee.K3.Core.BD;
using Kingdee.K3.Core.BD.ServiceArgs;
using Kingdee.K3.Core.MFG.EntityHelper;
using Kingdee.K3.Core.MFG.SFC;
using Kingdee.K3.MFG.Common.BusinessEntity.SFC.SFCUtils;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.DataModel;
using Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Utils;
using Kingdee.K3.MFG.Mobile.ServiceHelper;
using Kingdee.K3.MFG.Mobile.ServiceHelper.SFC;
using Kingdee.K3.MFG.ServiceHelper.SFC;
using Kingdee.K3.MFG.ServiceHelper.SFS;
using Kingdee.K3.MFG.SFC.Common.Core.EnumConst.Mobile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
namespace Kingdee.K3.MFG.Mobile.Business.PlugIn.SFC.Complex
{
	[Description("复杂工序(工序任务超市列表)-表单插件")]
	[Kingdee.BOS.Util.HotUpdate]
	public class ComplexTaskPoolList1 : ComplexListBaseEdit
	{
		private const string HighLightColor = "255,234,199";
		private MobileEnums.DataFilterType _currDataFilterType = MobileEnums.DataFilterType.None;
		private ExtendedDataEntity[] _extDatas;
		private System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> dispatchDetails = new System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
		private System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> detailTableData = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>>();
		protected System.Collections.Generic.Dictionary<int, int> DicDetailRowIndexRelation = new System.Collections.Generic.Dictionary<int, int>();
		private int detailCurrPageIndex;
		private MobileListFormaterManager detailFormatterManager;
		protected int detailTotalPageNumber;
		protected bool IsClaim;
		protected System.Collections.Generic.Dictionary<object, long> empAndUserId = new System.Collections.Generic.Dictionary<object, long>();
		protected System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> empListObject = new System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
		protected long userId;
		protected long empId;
		protected string empName = "";
		protected string deviceNumber = "";
		protected string deviceDesc = "";
		private bool isNeedStartNetWorkCtrl;
		protected Kingdee.BOS.Orm.DataEntity.DynamicObject optPlan;
		protected Kingdee.BOS.Orm.DataEntity.DynamicObject optPlanOper;
		private UnitConvert curUnitConvert;
		private System.Collections.Generic.Dictionary<long, short> OperUnitPrecision = new System.Collections.Generic.Dictionary<long, short>();
		private System.Collections.Generic.Dictionary<long, short> DisOperUnitPrecision = new System.Collections.Generic.Dictionary<long, short>();
		protected override string SrcBillModelFormId
		{
			get
			{
				return "SFC_OperationPlanning";
			}
		}
		protected System.Collections.Generic.List<string> DispSelKeys
		{
			get
			{
				return new System.Collections.Generic.List<string>
				{
					"FId",
					"FDispatchedQty",
					"FOperId"
				};
			}
		}
		private MobileEnums.DataFilterType CurrDataFilterType
		{
			get
			{
				return this._currDataFilterType;
			}
			set
			{
				switch (value)
				{
					case MobileEnums.DataFilterType.UnDispatch:
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.Forecolor.ToString(), "54,92,143");
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.Backcolor.ToString(), "255,255,255");
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.ImageKey.ToString(), "sfc_selectbox_sel.png");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.Forecolor.ToString(), "255,255,255");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.Backcolor.ToString(), "54,92,143");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.ImageKey.ToString(), "sfc_selectbox_unsel.png");
						base.View.GetControl("FFlowLayout_Claimed").Visible = false;
						base.View.GetControl("FFlowLayout_Claim").Visible = true;
						break;
					case MobileEnums.DataFilterType.UnFinishedDetail:
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.Forecolor.ToString(), "255,255,255");
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.Backcolor.ToString(), "54,92,143");
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.ImageKey.ToString(), "sfc_selectbox_unsel.png");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.Forecolor.ToString(), "54,92,143");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.Backcolor.ToString(), "255,255,255");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.ImageKey.ToString(), "sfc_selectbox_sel.png");
						base.View.GetControl("FFlowLayout_Claim").Visible = false;
						base.View.GetControl("FFlowLayout_Claimed").Visible = true;
						this.BindDispatchDetailList("");
						break;
					default:
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.Forecolor.ToString(), "54,92,143");
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.Backcolor.ToString(), "255,255,255");
						base.View.GetControl("FButton_Claim").SetCustomPropertyValue(ControlProperty.ImageKey.ToString(), "sfc_selectbox_sel.png");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.Forecolor.ToString(), "255,255,255");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.Backcolor.ToString(), "54,92,143");
						base.View.GetControl("FButton_Claimed").SetCustomPropertyValue(ControlProperty.ImageKey.ToString(), "sfc_selectbox_unsel.png");
						base.View.GetControl("FFlowLayout_Claimed").Visible = false;
						base.View.GetControl("FFlowLayout_Claim").Visible = true;
						break;
				}
				base.View.UpdateView("FButton_Claim");
				base.View.UpdateView("FButton_Claimed");
				this._currDataFilterType = value;
				this.ReloadListData(null, false);
			}
		}
		protected override bool IsNeedHighLight
		{
			get
			{
				return true;
			}
		}
		protected override System.Collections.Generic.IEnumerable<Kingdee.BOS.Orm.DataEntity.DynamicObject> GetListData(System.Collections.Generic.List<string> fieldLabelKeys)
		{
			ExportLogInfo exportLogInfo = new ExportLogInfo
			{
				Code = this.runId,
				StartTime = System.DateTime.Now
			};
			System.Collections.Generic.IEnumerable<Kingdee.BOS.Orm.DataEntity.DynamicObject> result = new System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
			MobileEnums.DataFilterType currDataFilterType = this.CurrDataFilterType;
			if (currDataFilterType != MobileEnums.DataFilterType.ScanCode)
			{
				if (currDataFilterType == MobileEnums.DataFilterType.UnDispatch)
				{
					this._extDatas = SFSDiscreteServiceHelper.GetUnDispatchOperPlanningDatas(base.Context, this.dicFieldEntityPaths.Keys.ToList<string>(), this.DispSelKeys, this.userId);
					if (this._extDatas != null)
					{
						result =
							from s in this._extDatas
							select s.DataEntity;
					}
				}
			}
			else
			{
				this._extDatas = SFSDiscreteServiceHelper.GetOperPlanningDataByBarCode(base.Context, this.CurrScanCode, true, false, null, this.userId, this.dicFieldEntityPaths.Keys.ToList<string>(), true, this.DispSelKeys, "UnDispatchOperPlanning");
				if (this._extDatas != null)
				{
					result =
						from s in this._extDatas
						select s.DataEntity;
				}
			}
			if (base.IsWriteLog)
			{
				exportLogInfo.UserId = base.Context.UserId.ToString();
				exportLogInfo.MethodName = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName + '.' + System.Reflection.MethodBase.GetCurrentMethod().Name;
				exportLogInfo.Detail = Kingdee.BOS.Resource.ResManager.LoadKDString("子类GetListData事件", "015747000021952", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]);
				exportLogInfo.EndTime = System.DateTime.Now;
				ExportLogServiceHelper.WriteLog(base.Context, exportLogInfo);
			}
			return result;
		}
		protected override void ReloadListData(System.Collections.Generic.IEnumerable<Kingdee.BOS.Orm.DataEntity.DynamicObject> datas = null, bool isReturnCurrPage = false)
		{
			base.ReloadListData(datas, isReturnCurrPage);
			if (this.DicTableData.Keys.Count<int>() == 1)
			{
				this.SetDispatchQty();
				return;
			}
			this.SetClaim();
		}
		protected override void SetHighLight()
		{
			if (this.IsNeedHighLight)
			{
				if (!this.IsClaim)
				{
					this.SetSelecRow("FMobileListViewEntity", "FFlowLayout_Row");
					return;
				}
				this.SetSelecRow("FMobileListViewEntity_Detail", "FFlowLayout_Detail");
			}
		}
		private void SetSelecRow(string str, string flowLayout)
		{
			int entryRowCount = this.Model.GetEntryRowCount(str);
			int[] selectedRows = base.View.GetControl<MobileListViewControl>(str).GetSelectedRows();
			for (int i = 1; i <= entryRowCount; i++)
			{
				if (selectedRows != null && selectedRows.Contains(i))
				{
					this.ListFormaterManager.SetControlProperty(flowLayout, i - 1, "255,234,199", MobileFormatConditionPropertyEnums.BackColor);
				}
				else
				{
					this.ListFormaterManager.SetControlProperty(flowLayout, i - 1, "255,255,255", MobileFormatConditionPropertyEnums.BackColor);
				}
			}
			base.View.GetControl<MobileListViewControl>(str).SetFormat(this.ListFormaterManager);
			base.View.UpdateView(str);
		}
		protected override void InitControlProperty()
		{
			base.InitControlProperty();
			//EmpInfo emplyeeInfoByUserId = DataUtils.GetEmplyeeInfoByUserId(base.Context, base.Context.UserId);
			//base.View.GetControl("FLable_ClaimEmp").SetValue(emplyeeInfoByUserId.Name);
			//this.empId = System.Convert.ToInt64(emplyeeInfoByUserId.Id);
			//this.empName = System.Convert.ToString(emplyeeInfoByUserId.Name);
			//base.View.UpdateView("FLable_ClaimEmp");
			//this.empListObject = SFSDiscreteServiceHelper.GetAgentEmpList(base.Context, ref this.empAndUserId).ToList<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
			//this.SetClaimedBtn();
			//base.View.GetClientCache("DeviceCode", null);
		}
		public override void AfterGetClientCache(MobileClientCacheLoadedEventArgs args)
		{
			base.AfterGetClientCache(args);
			if (args == null)
			{
				return;
			}
			string key;
			if ((key = args.Key) != null)
			{
				if (!(key == "DeviceCode"))
				{
					return;
				}
				if (args.Value != null)
				{
					Kingdee.BOS.JSON.JSONObject jSONObject = args.Value as Kingdee.BOS.JSON.JSONObject;
					if (jSONObject == null)
					{
						return;
					}
					jSONObject.Get("DeviceCode");
					object obj = jSONObject.Get("TerminalNumber");
					if (!string.IsNullOrWhiteSpace(System.Convert.ToString((obj == null) ? "" : obj)))
					{
						this.deviceNumber = System.Convert.ToString(jSONObject.Get("TerminalNumber"));
						this.deviceDesc = System.Convert.ToString(jSONObject.Get("TerminalDesc"));
					}
				}
			}
		}
		public override void ButtonClick(ButtonClickEventArgs e)
		{
			if (!e.Key.ToUpper().Equals("FBUTTON_FILTER") && !e.Key.ToUpper().Equals("FBUTTON_FILTER1"))
			{
				base.ButtonClick(e);
			}
			string key;
			switch (key = e.Key.ToUpper())
			{
				case "FBUTTON_KEYBOARD":
					base.View.GetControl("FText_ScanCode").SetFocus();
					base.View.GetControl("FText_ScanCode").SetCustomPropertyValue("showKeyboard", true);
					return;
				case "FBUTTON_CLAIM":
					this.userId = 0L;
					this.IsClaim = false;
					this.CurrDataFilterType = MobileEnums.DataFilterType.UnDispatch;
					return;
				case "FBUTTON_CLAIMED":
					this.userId = 0L;
					this.IsClaim = true;
					this.CurrDataFilterType = MobileEnums.DataFilterType.UnFinishedDetail;
					return;
				case "FBUTTON_CONFIRM":
					this.Dispatch();
					return;
				case "FBUTTON_PREVIOUS1":
					this.SetClaimed();
					this.detailCurrPageIndex--;
					this.FillReturnToOperList();
					this.AfterTurnPage();
					return;
				case "FBUTTON_NEXT1":
					this.SetClaimed();
					this.detailCurrPageIndex++;
					this.FillReturnToOperList();
					this.AfterTurnPage();
					return;
				case "FBUTTON_FILTER":
				case "FBUTTON_FILTER1":
					this.FilterListData(e.Key);
					return;
				case "FBUTTON_OPERKEYBOARD":
					base.View.GetControl("FButton_OperKeyBoard").SetFocus();
					base.View.GetControl("FButton_OperKeyBoard").SetCustomPropertyValue("showKeyboard", true);
					return;
				case "FBUTTON_OPERKEYBOARD1":
					base.View.GetControl("FButton_OperKeyBoard1").SetFocus();
					base.View.GetControl("FButton_OperKeyBoard1").SetCustomPropertyValue("showKeyboard", true);
					return;
				case "FBTN_CLOSE":
					this.CloseRow(false);
					return;
				case "FBTN_DELETE":
					base.View.ShowMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("确认是否删除？", "015747000028218", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), MessageBoxOptions.YesNo, delegate (MessageBoxResult result)
					{
						if (result == MessageBoxResult.Yes)
						{
							this.CloseRow(true);
						}
					}, "", MessageBoxType.Notice);
					return;
				case "FBTN_CLAIMMODIFY":
					this.ClaimedModity();
					return;
				case "FBTN_PRINTLABLE":
					this.printLable();
					return;
				case "FBUTTON_PREVIOUS":
					this.SetClaim();
					return;
				case "FBUTTON_NEXT":
					this.SetClaim();
					return;
				case "FBUTTON_REFRESH1":
					this.BindDispatchDetailList("");
					this.ReloadListData(null, false);
					return;
				case "FBUTTON_REFRESH2":
					this.ReloadListData(null, false);
					break;

					return;
			}
		}
		protected void FilterListData(string btnFilter)
		{
			this.dicFilterFields.Clear();
			string attachFilter = "FData_";
			if (btnFilter.Equals("FButton_Filter1", System.StringComparison.InvariantCultureIgnoreCase))
			{
				attachFilter = "FDLbl_";
			}
			this.dicFilterFields = (
				from w in base.View.LayoutInfo.Appearances
				where w.ElementType == 1001 && w.Key.Contains(attachFilter) && w.Key.Contains(this.FilterFlag)
				select w).ToDictionary((Appearance k) => k.Key.Split(new char[]
			{
				'_'
			})[1], (Appearance v) => v.Caption.ToString());
			MobileShowParameter mobileShowParameter = new MobileShowParameter
			{
				FormId = "SFC_MobileComplexCommonFilter",
				OpenStyle =
				{
					ShowType = ShowType.Floating
				},
				TitleVisible = false,
				Height = 480,
				Width = 400
			};
			mobileShowParameter.CustomComplexParams.Add("dicFilterFields", this.dicFilterFields);
			mobileShowParameter.ParentPageId = base.View.PageId;
			base.View.ShowForm(mobileShowParameter, delegate (FormResult result)
			{
				if (result.ReturnData == null)
				{
					return;
				}
				this.dicFilterValues = (System.Collections.Generic.Dictionary<string, string>)result.ReturnData;
				this.isReturnCurrPageNumber = false;
				if (btnFilter.Equals("FButton_Filter1", System.StringComparison.InvariantCultureIgnoreCase))
				{
					this.PrepareDispatchDetailTableData(this.dicFilterValues);
				}
				else
				{
					this.FillListViewEntityByFilter(this.dicFilterValues);
				}
				this.AfterReLoadListData4Filter();
			});
		}
		protected void CloseRow(bool flag)
		{
			int[] selectedRows = base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail").GetSelectedRows();
			int num = selectedRows.FirstOrDefault<int>() + this.RowCountPerPage * (this.detailCurrPageIndex - 1);
			if (!selectedRows.Any<int>())
			{
				base.View.ShowStatusBarInfo(Kingdee.BOS.Resource.ResManager.LoadKDString("未选择分录！", "015747000028217", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
				return;
			}
			System.Collections.Generic.Dictionary<string, object> dictionary = this.detailTableData[num - 1];
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", true) as FormMetadata;
			System.Collections.Generic.List<string> lstDisPatchIds = new System.Collections.Generic.List<string>
			{
				dictionary["PkId"].ToString()
			};
			System.Collections.Generic.List<NetworkCtrlResult> netCtrlDispatchIds = this.GetNetCtrlDispatchIds(lstDisPatchIds);
			if (netCtrlDispatchIds.Count > 0)
			{
				NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, netCtrlDispatchIds);
				System.Collections.Generic.List<string> list = (
					from o in netCtrlDispatchIds
					select o.InterID).ToList<string>();
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = BusinessDataServiceHelper.Load(base.Context, list.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				System.Convert.ToString(dictionary["PkId"]);
				object entryId = dictionary["EntryPkId"];
				Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectItemValue = dynamicObject["DispatchDetailEntry"] as DynamicObjectCollection;
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject2 = (
					from o in dynamicObjectItemValue
					where entryId.Equals(o["Id"])
					select o).FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				if (dynamicObject2 != null)
				{
					if (System.Convert.ToDecimal(dynamicObject2["FinishSelQty"]) == 0m)
					{
						dynamicObjectItemValue.Remove(dynamicObject2);
					}
					else
					{
						dynamicObject2["BaseWorkQty"] = dynamicObject2["BaseFinishSelQty"];
						dynamicObject2["WorkQty"] = dynamicObject2["FinishSelQty"];
						dynamicObject2["WorkHeadQty"] = dynamicObject2["FinishSelHeadQty"];
						dynamicObject2["Status"] = "D";
					}
				}
				Kingdee.BOS.Orm.OperateOption operateOption = Kingdee.BOS.Orm.OperateOption.Create();
				operateOption.SetVariableValue("IsMobileInvoke", true);
				IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, dynamicObject, operateOption, "");
				if (operationResult.IsSuccess)
				{
					this.BindDispatchDetailList("");
					if (!flag)
					{
						base.View.ShowStatusBarInfo(Kingdee.BOS.Resource.ResManager.LoadKDString("关闭成功！", "015747000026594", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
						return;
					}
					base.View.ShowStatusBarInfo(Kingdee.BOS.Resource.ResManager.LoadKDString("删除成功！", "015747000026594", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
				}
			}
		}
		protected virtual void printLable()
		{
			int[] selectedRows = base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail").GetSelectedRows();
			int num = selectedRows.FirstOrDefault<int>() + this.RowCountPerPage * (this.detailCurrPageIndex - 1);
			if (!selectedRows.Any<int>())
			{
				base.View.ShowStatusBarInfo(Kingdee.BOS.Resource.ResManager.LoadKDString("未选择分录！", "015747000028217", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
				return;
			}
			System.Collections.Generic.Dictionary<string, object> dictionary = this.detailTableData[num - 1];
			string billBarCode = dictionary["FBarCode"].ToString();
			this.Print(billBarCode, false);
		}
		private void Print(string billBarCode, bool flag)
		{
			IOperationResult dataByDispatchBillNo = SFCMobilePrintServiceHelper.GetDataByDispatchBillNo(base.Context, billBarCode, "04", this.deviceNumber, this.deviceDesc, false);
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
			if (!dataByDispatchBillNo.IsSuccess)
			{
				int num;
				if (flag)
				{
					string value = Kingdee.BOS.Resource.ResManager.LoadKDString("1、认领成功！\n", "0151515153512030033966", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]);
					stringBuilder.Append(value);
					num = 2;
				}
				else
				{
					num = 1;
				}
				using (System.Collections.Generic.List<string>.Enumerator enumerator = (dataByDispatchBillNo.FuncResult as System.Collections.Generic.List<string>).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						stringBuilder.Append(string.Format("{0}、{1}\n", num, current));
						num++;
					}
					goto IL_FD;
				}
			}
			if (flag)
			{
				stringBuilder.Append(Kingdee.BOS.Resource.ResManager.LoadKDString("成功！", "015747000028224", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
			}
			else
			{
				stringBuilder.Append(Kingdee.BOS.Resource.ResManager.LoadKDString("打印产品成功！", "015747000029461", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
			}
			list = (dataByDispatchBillNo.FuncResult as System.Collections.Generic.List<string>);
		IL_FD:
			base.View.ShowMessage(stringBuilder.ToString(), MessageBoxType.Notice);
			if (list != null && list.Count > 0)
			{
				PrintUtils.Print(base.View, list, 1);
			}
		}
		protected void ClaimedModity()
		{
			int[] selectedRows = base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail").GetSelectedRows();
			int num = selectedRows.FirstOrDefault<int>() + this.RowCountPerPage * (this.detailCurrPageIndex - 1);
			if (!selectedRows.Any<int>())
			{
				base.View.ShowStatusBarInfo(Kingdee.BOS.Resource.ResManager.LoadKDString("未选择分录！", "015747000028217", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
				return;
			}
			System.Collections.Generic.Dictionary<string, object> dictionary = this.detailTableData[num - 1];
			decimal dispatchQtyByPK = this.GetDispatchQtyByPK(System.Convert.ToInt64(dictionary["EntryPkId"]));
			if (System.Convert.ToDecimal(this.Model.DataObject["ClaimedQty"]) > dispatchQtyByPK || System.Convert.ToDecimal(this.Model.DataObject["ClaimedQty"]) < 0m)
			{
				base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("数量必须大于0且不能超过选中行的“可认领”数量！", "0151515153512030033830", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
				return;
			}
			if (System.Convert.ToDateTime(this.Model.DataObject["PlanBeginedTime"]) < System.Convert.ToDateTime(this.Model.DataObject["PlanBeginedTime"]))
			{
				base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("计划结束时间不能早于计划开始时间！", "0151515153512030033831", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
			}
			System.Collections.Generic.List<string> lstDisPatchIds = new System.Collections.Generic.List<string>
			{
				dictionary["PkId"].ToString()
			};
			System.Collections.Generic.List<NetworkCtrlResult> netCtrlDispatchIds = this.GetNetCtrlDispatchIds(lstDisPatchIds);
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", true);
			if (netCtrlDispatchIds.Count > 0)
			{
				NetworkCtrlServiceHelper.BatchCommitNetCtrl(base.Context, netCtrlDispatchIds);
				System.Collections.Generic.List<string> list = (
					from o in netCtrlDispatchIds
					select o.InterID).ToList<string>();
				System.Convert.ToString(dictionary["PkId"]);
				object entryId = dictionary["EntryPkId"];
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = BusinessDataServiceHelper.Load(base.Context, list.ToArray(), formMetadata.BusinessInfo.GetDynamicObjectType()).FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectItemValue = dynamicObject["DispatchDetailEntry"] as  DynamicObjectCollection;
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject2 = (
					from o in dynamicObjectItemValue
					where entryId.Equals(o["Id"])
					select o).FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				if (dynamicObject2 != null)
				{
					if (this.curUnitConvert == null)
					{
						this.curUnitConvert = this.GetUnitConvert(System.Convert.ToInt64(dynamicObject["MaterialId_Id"]), System.Convert.ToInt64(dynamicObject["FUnitID_Id"]), System.Convert.ToInt64(dynamicObject["BaseUnitID_Id"]));
					}
					decimal num2 = System.Convert.ToDecimal(this.Model.DataObject["ClaimedQty"]) * System.Convert.ToDecimal(dynamicObject["UnitTransHeadQty"]) / System.Convert.ToDecimal(dynamicObject["UnitTransOperQty"]);
					dynamicObject2["BaseWorkQty"] = this.curUnitConvert.ConvertQty(num2, "");
					dynamicObject2["WorkQty"] = this.Model.DataObject["ClaimedQty"];
					dynamicObject2["WorkHeadQty"] = num2;
					dynamicObject2["PlanBeginTime"] = this.Model.DataObject["PlanBeginedTime"];
					dynamicObject2["PlanEndTime"] = this.Model.DataObject["PlanEndedTime"];
				}
				Kingdee.BOS.Orm.OperateOption operateOption = Kingdee.BOS.Orm.OperateOption.Create();
				operateOption.SetVariableValue("IsMobileInvoke", true);
				IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, dynamicObject, operateOption, "");
				if (operationResult.IsSuccess)
				{
					base.View.ShowStatusBarInfo(Kingdee.BOS.Resource.ResManager.LoadKDString("成功！", "015747000028224", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
					this.Model.DataObject["ClaimedQty"] = 0;
					base.View.SetControlProperty("FClaimedQty", 0);
					base.View.UpdateView("FClaimedQty");
					this.Model.DataObject["PlanBeginedTime"] = null;
					this.Model.DataObject["PlanEndedTime"] = null;
					base.View.UpdateView("FPlanBeginedTime");
					base.View.UpdateView("FPlanEndedTime");
					base.View.GetControl("FBtn_ClaimModify").Enabled = false;
					base.View.UpdateView("FBtn_ClaimModify");
					this.IsClaim = true;
					this.CurrDataFilterType = MobileEnums.DataFilterType.UnFinishedDetail;
				}
			}
		}
		private void SetClaimedBtn()
		{
			base.View.GetControl("FBtn_ClaimModify").Enabled = false;
			base.View.GetControl("FBtn_Delete").Enabled = false;
			base.View.GetControl("FBtn_Close").Enabled = false;
			base.View.GetControl("FButton_Confirm").Enabled = false;
			base.View.UpdateView("FBtn_ClaimModify");
			base.View.UpdateView("FBtn_Delete");
			base.View.UpdateView("FBtn_Close");
			base.View.UpdateView("FButton_Confirm");
		}
		private System.Collections.Generic.List<NetworkCtrlResult> GetNetCtrlDispatchIds(System.Collections.Generic.List<string> lstDisPatchIds)
		{
			string filter = " FMetaObjectID = 'SFC_DispatchDetail' and FNumber = 'Edit' and FType = 6 and FStart = '1' ";
			NetworkCtrlObject networkCtrlObj = NetworkCtrlServiceHelper.GetNetCtrlList(base.Context, filter).FirstOrDefault<NetworkCtrlObject>();
			string value = Kingdee.BOS.Resource.ResManager.LoadKDString("立即派工", "015747000029451", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]);
			System.Collections.Generic.List<NetWorkRunTimeParam> list = new System.Collections.Generic.List<NetWorkRunTimeParam>();
			foreach (string current in lstDisPatchIds)
			{
				list.Add(new NetWorkRunTimeParam
				{
					OperationName = new Kingdee.BOS.LocaleValue(value),
					FuncDeatilID = "Edit",
					InterID = current
				});
			}
			return NetworkCtrlServiceHelper.BatchBeginNetCtrl(base.Context, networkCtrlObj, list, false);
		}
		public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
		{
			base.BeforeUpdateValue(e);
			string a;
			if ((a = e.Key.ToUpper()) != null)
			{
				if (!(a == "FTEXT_SCANCODE"))
				{
					if (!(a == "FTEXT_SCANOPERATOR"))
					{
						if (!(a == "FTEXT_SCANCODE1"))
						{
							return;
						}
						string scanCode = System.Convert.ToString(e.Value);
						this.BindDispatchDetailList(scanCode);
						return;
					}
				}
				else
				{
					this.ScanCodeChanged(e);
					try
					{
						this.CurrDataFilterType = MobileEnums.DataFilterType.ScanCode;
						return;
					}
					catch (System.Exception ex)
					{
						e.Value = string.Empty;
						base.View.ShowStatusBarInfo(ex.Message);
						return;
					}
				}
				System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> list = this.SetEmp(e);
				if (list != null)
				{
					if (list.Count > 0)
					{
						base.View.GetControl("FLable_ClaimEmp").SetValue(list.FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>()["Name"].ToString());
						base.View.UpdateView("FLable_ClaimEmp");
					}
					else
					{
						base.View.GetControl("FLable_ClaimEmp").SetValue("");
						base.View.UpdateView("FLable_ClaimEmp");
						base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("该员工不在您的代理范围之内，请检查！", "0151515153512030033832", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
					}
					this.ReloadListData(null, false);
					return;
				}
			}
		}
		public override void AfterBindData(System.EventArgs e)
		{
			base.AfterBindData(e);
			this.CurrDataFilterType = MobileEnums.DataFilterType.UnDispatch;
			base.View.GetControl("FLable_BillTitle").Enabled = false;
			base.View.GetControl("FLable_BillTitle").Visible = false;
			base.View.UpdateView("FLable_BillTitle");
			this.detailFormatterManager = new MobileListFormaterManager();
			if (this.DicTableData.Keys.Count<int>() == 1)
			{
				this.SetDispatchQty();
				return;
			}
			this.SetClaim();
		}
		protected override void PrepareBindFields(System.Collections.Generic.Dictionary<string, string> dicFieldLabelKeys)
		{
			base.PrepareBindFields(dicFieldLabelKeys);
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FMOEntrySeq", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FSeqNumber", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FProductName", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FBillNo", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FOperNumber", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FDepartmentId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FWorkCenterId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FProcessOrgId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FSeqType", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FOperUnitId", "");
		}
		protected override void PrepareDicTableData(System.Collections.Generic.IEnumerable<Kingdee.BOS.Orm.DataEntity.DynamicObject> datas, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> dicTableData)
		{
			base.PrepareDicTableData(datas, dicTableData);
			if (!datas.Any<Kingdee.BOS.Orm.DataEntity.DynamicObject>())
			{
				return;
			}
			System.Collections.Generic.Dictionary<object, Kingdee.BOS.Orm.DataEntity.DynamicObject> dictionary = datas.SelectMany((Kingdee.BOS.Orm.DataEntity.DynamicObject s) => (Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection)s["Entity"]).SelectMany((Kingdee.BOS.Orm.DataEntity.DynamicObject s) => (Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection)s["SubEntity"]).ToDictionary((Kingdee.BOS.Orm.DataEntity.DynamicObject k) => k["Id"]);
			System.Collections.Generic.Dictionary<object, Kingdee.BOS.Orm.DataEntity.DynamicObject> dictionary2 = datas.SelectMany((Kingdee.BOS.Orm.DataEntity.DynamicObject s) => (Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection)s["Entity"]).SelectMany((Kingdee.BOS.Orm.DataEntity.DynamicObject s) => (Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection)s["SubEntity"]).ToDictionary((Kingdee.BOS.Orm.DataEntity.DynamicObject k) => k["Id"]);
			//(
			//	from s in this._extDatas
			//	select s["DispatchDatas"] as System.Collections.Generic.Dictionary<long, Kingdee.BOS.Orm.DataEntity.DynamicObject>).SelectMany((System.Collections.Generic.Dictionary<long, Kingdee.BOS.Orm.DataEntity.DynamicObject> dynamicObjects) => dynamicObjects).ToDictionary((System.Collections.Generic.KeyValuePair<long, Kingdee.BOS.Orm.DataEntity.DynamicObject> keyValuePair) => keyValuePair.Key, (System.Collections.Generic.KeyValuePair<long, Kingdee.BOS.Orm.DataEntity.DynamicObject> keyValuePair) => keyValuePair.Value);
			System.Collections.Generic.List<long> source = dictionary2.Keys.ToList<object>().OfType<long>().ToList<long>();
			this.GetUnitPrecision(source.Distinct<long>().ToList<long>());
			foreach (System.Collections.Generic.Dictionary<string, object> current in dicTableData.Values)
			{
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = dictionary[current["EntryPkId"]];
				current["OperId"] = dynamicObject["Id"];
				current["FMONumber"] = string.Format("{0}-{1}", current["FMONumber"], current["FMOEntrySeq"]);
				current["FOperPlanNo"] = string.Format("{0}-{1}-{2} ({3})", new object[]
				{
					current["FBillNo"],
					current["FSeqNumber"],
					current["FOperNumber"],
					SFCCommonUtils.Instance.ReturnOPlanSeqType(System.Convert.ToString(current["FSeqType"]))
				});
				current["FProductId"] = string.Format("{0}/{1}", current["FProductId"], current["FProductName"]);
				if (dictionary2.ContainsKey(current["OperId"]) && dictionary2[current["OperId"]] != null)
				{
					//current["FDispatchQty"] = dictionary2[current["OperId"]]["DispatchedQty"].ConvertToDecimalTrimEndZero();
					//current["DispatchDetailId"] = dictionary2[current["OperId"]]["Id"];
				}
				else
				{
					current["FDispatchQty"] = "0";
				}
				//current["FDispatchQty"] = System.Convert.ToDecimal(current["FOperQty"]) - System.Convert.ToDecimal(current["FDispatchQty"]);
			}
			System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> list = (
				from t in dicTableData.Values
				orderby t["FMONumber"], t["FMOEntrySeq"], t["FSeqNumber"], t["FOperNumber"], t["FOperPlanStartTime"]
				select t).ToList<System.Collections.Generic.Dictionary<string, object>>();
			dicTableData.Clear();
			int num = 0;
			foreach (System.Collections.Generic.Dictionary<string, object> current2 in list)
			{
				dicTableData[num++] = current2;
			}
		}
		public override void EntityRowDoubleClick(EntityRowClickEventArgs e)
		{
			base.EntityRowDoubleClick(e);
			if (!this.IsClaim)
			{
				this.SetDispatchQty();
				return;
			}
			this.SetDispatchDetailQty();
		}
		protected void Dispatch()
		{
			if (string.IsNullOrEmpty(this.empName))
			{
				base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("用户和员工无绑定关系，请检查！", "0151515153512030034010", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
				return;
			}
			System.Collections.Generic.Dictionary<string, object> currentRowData = base.GetCurrentRowData();
			long optPlanOptId = System.Convert.ToInt64(currentRowData["OperId"]);
			decimal d = System.Convert.ToDecimal(currentRowData["FDispatchQty"]);
			if (System.Convert.ToDecimal(this.Model.DataObject["ClaimQty"]) > d || System.Convert.ToDecimal(this.Model.DataObject["ClaimQty"]) < 0m)
			{
				base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("数量必须大于0且不能超过选中行的“可认领”数量！", "0151515153512030033830", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
				return;
			}
			if (System.Convert.ToDateTime(this.Model.DataObject["PlanEndTime"]) < System.Convert.ToDateTime(this.Model.DataObject["PlanBeginTime"]))
			{
				base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("计划结束时间不能早于计划开始时间！", "0151515153512030033831", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
			}
			this.BuildDispatchDetailData(optPlanOptId, true);
		}
		protected void BuildDispatchDetailData(long optPlanOptId, bool isCreateNewEntry = true)
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", true) as FormMetadata;
			this.GetOptPlanOper(optPlanOptId);
			OQLFilter ofilter = OQLFilter.CreateHeadEntityFilter(string.Format("FOperId={0}", System.Convert.ToInt64(this.optPlanOper["Id"])));
			Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = BusinessDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", null, ofilter).FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
			if (dynamicObject == null)
			{
				dynamicObject = this.CreateDispatchDetailData();
				this.isNeedStartNetWorkCtrl = true;
			}
			else
			{
				if (isCreateNewEntry)
				{
					dynamicObject = this.CreateDispatchEntryData(dynamicObject, null);
					DBServiceHelper.LoadReferenceObject(base.Context, new Kingdee.BOS.Orm.DataEntity.DynamicObject[]
					{
						dynamicObject
					}, formMetadata.BusinessInfo.GetDynamicObjectType(), false);
				}
			}
			IOperationResult operationResult = BusinessDataServiceHelper.Save(base.Context, formMetadata.BusinessInfo, dynamicObject, null, "");
			if (operationResult.IsSuccess)
			{
				this.Model.DataObject["ClaimQty"] = 0;
				base.View.SetControlProperty("FClaimQty", 0);
				base.View.UpdateView("FClaimQty");
				this.Model.DataObject["PlanBeginTime"] = null;
				this.Model.DataObject["PlanEndTime"] = null;
				base.View.UpdateView("FPlanBeginTime");
				base.View.UpdateView("FPlanEndTime");
				base.View.GetControl("FButton_Confirm").Enabled = false;
				base.View.UpdateView("FButton_Confirm");
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject2 = operationResult.SuccessDataEnity.FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection source = dynamicObject2["DispatchDetailEntry"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection;
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject3 = source.LastOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object, object>> list = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<object, object>>();
				string hMIAutoStartOper = this.GetHMIAutoStartOper(System.Convert.ToInt64(dynamicObject2["WorkCenterId_Id"]));
				if (hMIAutoStartOper == "1" && dynamicObject3 != null)
				{
					list.Add(new System.Collections.Generic.KeyValuePair<object, object>(dynamicObject2["Id"], dynamicObject3["Id"]));
					IOperationResult operationResult2 = DispatchDetailServiceHelper.SetStatus(base.Context, list, "ToStart", null);
					if (operationResult2.IsSuccess)
					{
						dynamicObject3["Status"] = "B";
					}
				}
				if (dynamicObject3 != null)
				{
					string billBarCode = dynamicObject3["BarCode"].ToString();
					this.Print(billBarCode, true);
				}
				this.ReloadListData(null, false);
				return;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			if (operationResult.ValidationErrors.Count > 0)
			{
				stringBuilder.AppendLine();
				foreach (ValidationErrorInfo current in operationResult.ValidationErrors)
				{
					stringBuilder.AppendLine(current.Message);
				}
			}
			base.View.ShowMessage(stringBuilder.ToString(), MessageBoxType.Notice);
		}
		protected virtual Kingdee.BOS.Orm.DataEntity.DynamicObject CreateDispatchDetailData()
		{
			FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", true) as FormMetadata;
			EntryEntity entryEntity = formMetadata.BusinessInfo.GetEntity("FEntity") as EntryEntity;
			Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = formMetadata.BusinessInfo.GetDynamicObjectType().CreateInstance() as Kingdee.BOS.Orm.DataEntity.DynamicObject;
			dynamicObject["OperId"] = this.optPlanOper["Id"];
			dynamicObject["FFormId"] = "SFC_DispatchDetail";
			dynamicObject["MoBillNo"] = this.optPlan["MONumber"];
			dynamicObject["MoSeq"] = this.optPlan["MOEntrySeq"];
			dynamicObject["OptPlanNo"] = this.optPlan["BillNo"];
			dynamicObject["SeqNumber"] = ((Kingdee.BOS.Orm.DataEntity.DynamicObject)this.optPlanOper.Parent)["SeqNumber"];
			dynamicObject["OperNumber"] = this.optPlanOper["OperNumber"];
			dynamicObject["PlanningQty"] = this.optPlanOper["OperQty"];
			dynamicObject["ProcessId_Id"] = this.optPlanOper["ProcessId_Id"];
			dynamicObject["MaterialId_Id"] = this.optPlan["ProductId_Id"];
			dynamicObject["WorkCenterId_Id"] = this.optPlanOper["WorkCenterId_Id"];
			dynamicObject["MOEntryId"] = this.optPlan["MOEntryId"];
			dynamicObject["FUnitID_Id"] = this.optPlan["MOUnitId_Id"];
			dynamicObject["BaseUnitID_Id"] = this.optPlan["BaseUnitId_Id"];
			dynamicObject["OperUnitID_Id"] = this.optPlanOper["OperUnitId_Id"];
			dynamicObject["UnitTransHeadQty"] = this.optPlanOper["UnitTransHeadQty"];
			dynamicObject["UnitTransOperQty"] = this.optPlanOper["UnitTransOperQty"];
			dynamicObject = this.CreateDispatchEntryData(dynamicObject, entryEntity);
			DBServiceHelper.LoadReferenceObject(base.Context, new Kingdee.BOS.Orm.DataEntity.DynamicObject[]
			{
				dynamicObject
			}, formMetadata.BusinessInfo.GetDynamicObjectType(), false);
			return dynamicObject;
		}
		protected virtual Kingdee.BOS.Orm.DataEntity.DynamicObject CreateDispatchEntryData(Kingdee.BOS.Orm.DataEntity.DynamicObject dispatchObj, EntryEntity entryEntity)
		{
			if (entryEntity == null)
			{
				FormMetadata formMetadata = MetaDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", true) as FormMetadata;
				entryEntity = (formMetadata.BusinessInfo.GetEntity("FEntity") as EntryEntity);
			}
			Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectCollection = dispatchObj["DispatchDetailEntry"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection;
			Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = entryEntity.DynamicObjectType.CreateInstance() as Kingdee.BOS.Orm.DataEntity.DynamicObject;
			if (dynamicObjectCollection.Count > 0)
			{
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject2 = dynamicObjectCollection.Last<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
				dynamicObject["ResourceId_Id"] = dynamicObject2["ResourceId_Id"];
				dynamicObject["ShiftSliceId_Id"] = dynamicObject2["ShiftSliceId_Id"];
				dynamicObject["ShiftGroupId_Id"] = dynamicObject2["ShiftGroupId_Id"];
				dynamicObject["EquipmentId_Id"] = dynamicObject2["EquipmentId_Id"];
				dynamicObject["PlanBeginTime"] = dynamicObject2["PlanBeginTime"];
				dynamicObject["PlanEndTime"] = dynamicObject2["PlanEndTime"];
				dynamicObject["Status"] = "A";
				dynamicObject["WorkQty"] = this.Model.DataObject["ClaimQty"];
				if (this.curUnitConvert == null)
				{
					this.curUnitConvert = this.GetUnitConvert(System.Convert.ToInt64(dispatchObj["MaterialId_Id"]), System.Convert.ToInt64(dispatchObj["FUnitID_Id"]), System.Convert.ToInt64(dispatchObj["BaseUnitID_Id"]));
				}
				decimal num = System.Convert.ToDecimal(dynamicObject["WorkQty"]) * System.Convert.ToDecimal(dispatchObj["UnitTransHeadQty"]) / System.Convert.ToDecimal(dispatchObj["UnitTransOperQty"]);
				dynamicObject["WorkHeadQty"] = num;
				dynamicObject["BaseWorkQty"] = this.curUnitConvert.ConvertQty(num, "");
				dynamicObject["DispatchTime"] = CommonServiceHelper.GetCurrentTime(base.Context);
			}
			else
			{
				dynamicObject["ResourceId_Id"] = this.optPlanOper["ResourceId_Id"];
				dynamicObject["ShiftSliceId_Id"] = this.optPlanOper["ShiftSliceId_Id"];
				dynamicObject["ShiftGroupId_Id"] = this.optPlanOper["ShiftGroupId_Id"];
				dynamicObject["EquipmentId_Id"] = this.optPlanOper["EquipmentId_Id"];
				dynamicObject["PlanBeginTime"] = this.optPlanOper["OperPlanStartTime"];
				dynamicObject["PlanEndTime"] = this.optPlanOper["OperPlanFinishTime"];
				dynamicObject["Status"] = "A";
				dynamicObject["DispatchTime"] = CommonServiceHelper.GetCurrentTime(base.Context);
				Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectCollection2 = this.optPlanOper["EmpId"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection;
				Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectCollection3 = dynamicObject["EmpId"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection;
				foreach (Kingdee.BOS.Orm.DataEntity.DynamicObject current in dynamicObjectCollection2)
				{
					Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject3 = new Kingdee.BOS.Orm.DataEntity.DynamicObject(dynamicObjectCollection2.DynamicCollectionItemPropertyType);
					dynamicObject3["PkId"] = 0;
					dynamicObject3["EmpId_Id"] = current["EmpId_Id"];
					dynamicObject3["EmpId"] = current["EmpId"];
					dynamicObjectCollection3.Add(dynamicObject3);
				}
				if (System.Convert.ToInt64(dynamicObject["ResourceId_Id"]) == 0L)
				{
					Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject4 = this.optPlanOper["WorkCenterId"] as Kingdee.BOS.Orm.DataEntity.DynamicObject;
					Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection source = dynamicObject4["WorkCenterCapacity"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection;
					Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject5 = source.FirstOrDefault((Kingdee.BOS.Orm.DataEntity.DynamicObject f) => System.Convert.ToBoolean(f["JoinScheduling"]));
					if (dynamicObject5 != null)
					{
						dynamicObject["ResourceId_Id"] = dynamicObject5["RESOURCEID_Id"];
					}
				}
				decimal num2 = System.Convert.ToDecimal(this.Model.DataObject["ClaimQty"]);
				dynamicObject["WorkQty"] = ((num2 < 0m) ? 0m : num2);
				if (this.curUnitConvert == null)
				{
					this.curUnitConvert = this.GetUnitConvert(System.Convert.ToInt64(dispatchObj["MaterialId_Id"]), System.Convert.ToInt64(dispatchObj["FUnitID_Id"]), System.Convert.ToInt64(dispatchObj["BaseUnitID_Id"]));
				}
				decimal num3 = System.Convert.ToDecimal(dynamicObject["WorkQty"]) * System.Convert.ToDecimal(dispatchObj["UnitTransHeadQty"]) / System.Convert.ToDecimal(dispatchObj["UnitTransOperQty"]);
				dynamicObject["WorkHeadQty"] = num3;
				dynamicObject["BaseWorkQty"] = this.curUnitConvert.ConvertQty(num3, "");
			}
			Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectCollection4 = dynamicObject["EmpId"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection;
			Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject6 = new Kingdee.BOS.Orm.DataEntity.DynamicObject(dynamicObjectCollection4.DynamicCollectionItemPropertyType);
			dynamicObject6["EmpId_Id"] = this.empId;
			dynamicObjectCollection4.Add(dynamicObject6);
			dynamicObject["EmpText"] = this.empName;
			dynamicObject["CreateMode"] = 'A';
			long wcId = System.Convert.ToInt64(dispatchObj["WorkCenterId_Id"]);
			System.Collections.Generic.List<long> mould = DataUtils.GetMould(base.Context, wcId);
			if (mould != null && mould.Count == 1)
			{
				dynamicObject["MouldId_Id"] = mould.ElementAt(0);
			}
			dynamicObject["PlanBeginTime"] = this.Model.DataObject["PlanBeginTime"];
			dynamicObject["PlanEndTime"] = this.Model.DataObject["PlanEndTime"];
			dynamicObjectCollection.Add(dynamicObject);
			return dispatchObj;
		}
		protected UnitConvert GetUnitConvert(long materialId, long srcUnitId, long descUnitId)
		{
			return UnitConvertServiceHelper.GetUnitConvertRate(base.Context, new GetUnitConvertRateArgs
			{
				MaterialId = materialId,
				SourceUnitId = srcUnitId,
				DestUnitId = descUnitId
			});
		}
		protected void GetOptPlanOper(long optPlanOptId)
		{
			Kingdee.BOS.SqlParam sqlParam = new Kingdee.BOS.SqlParam("@OperId", Kingdee.BOS.KDDbType.Int64, optPlanOptId);
			long num = DBServiceHelper.ExecuteScalar<long>(base.Context, "SELECT S.FID FROM T_SFC_OPERPLANNINGDETAIL d INNER JOIN T_SFC_OPERPLANNINGSEQ s ON s.FENTRYID=d.FENTRYID WHERE d.FDETAILID=@OperId", 0L, new Kingdee.BOS.SqlParam[]
			{
				sqlParam
			});
			Kingdee.BOS.Orm.Metadata.DataEntity.DynamicObjectType dynamicObjectType = ((FormMetadata)MetaDataServiceHelper.Load(base.Context, "SFC_OperationPlanning", true)).BusinessInfo.GetDynamicObjectType();
			this.optPlan = BusinessDataServiceHelper.LoadSingle(base.Context, num, dynamicObjectType, null);
			this.optPlanOper = ((Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection)this.optPlan["Entity"]).SelectMany((Kingdee.BOS.Orm.DataEntity.DynamicObject s) => s["SubEntity"] as Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection).FirstOrDefault((Kingdee.BOS.Orm.DataEntity.DynamicObject f) => System.Convert.ToInt64(f["Id"]) == optPlanOptId);
		}
		protected System.Collections.Generic.Dictionary<long, Tuple<long, long, long>> GetPWDId(long operId)
		{
			System.Collections.Generic.Dictionary<long, Tuple<long, long, long>> dictionary = new System.Collections.Generic.Dictionary<long, Tuple<long, long, long>>();
			string strSQL = "select FProcessOrgId,FWorkCenterId,FDepartmentId,fdetailId from T_SFC_OPERPLANNINGDETAIL where fdetailId=@operId ";
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> paramList = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>
			{
				new Kingdee.BOS.SqlParam("@operId", Kingdee.BOS.KDDbType.Int64, operId)
			};
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, strSQL, paramList))
			{
				while (dataReader.Read())
				{
					long num = System.Convert.ToInt64(dataReader["fdetailId"]);
					long item = System.Convert.ToInt64(dataReader["FProcessOrgId"]);
					long item2 = System.Convert.ToInt64(dataReader["FWorkCenterId"]);
					long item3 = System.Convert.ToInt64(dataReader["FDepartmentId"]);
					if (!dictionary.Keys.Contains(num))
					{
						dictionary.Add(num, new Tuple<long, long, long>(item, item2, item3));
					}
				}
			}
			return dictionary;
		}
		private void GetUnitPrecision(System.Collections.Generic.List<long> detaiIds)
		{
			string text = string.Format(" select D.FDETAILID,B.FPrecision  from T_SFC_OPERPLANNINGDETAIL D \r\n                                                             inner join T_BD_UNIT B on D.FOperUnitId=B.FUNITID \r\n                                                             inner join (SELECT /*+ cardinality(be {0})*/ FID FROM table(fn_StrSplit(@DetailId, ',', 1)) be) e on e.FID = D.FdetailId", detaiIds.Count<long>());
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> list = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>();
			list.Add(new Kingdee.BOS.SqlParam("@DetailId", Kingdee.BOS.KDDbType.udt_inttable, detaiIds.ToArray()));
			new System.Collections.Generic.List<long>();
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, text.ToString(), list))
			{
				while (dataReader.Read())
				{
					if (!this.OperUnitPrecision.Keys.Contains(System.Convert.ToInt64(dataReader["FDETAILID"])))
					{
						this.OperUnitPrecision.Add(System.Convert.ToInt64(dataReader["FDETAILID"]), System.Convert.ToInt16(dataReader["FPrecision"]));
					}
				}
			}
		}
		private decimal GetDispatchQty(long operId)
		{
			string strSql = "SELECT FPLANNINGQTY-FDISPATCHEDQTY FROM T_SFC_DISPATCHDETAIL where FOPERID=@operId ";
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> list = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>
			{
				new Kingdee.BOS.SqlParam("@operId", Kingdee.BOS.KDDbType.Int64, operId)
			};
			return DBServiceHelper.ExecuteScalar<int>(base.Context, strSql, 0, list.ToArray());
		}
		private decimal GetDispatchQtyByPK(long entryId)
		{
			string strSql = " select (A.FPlanningQty-A.FDispatchedQty+B.FWorkQty) as Qty  from T_SFC_DISPATCHDETAIL A \r\n                                                                                    inner join T_SFC_DISPATCHDETAILENTRY B on A.fid=B.fid\r\n                                                                                    where B.fentryid=@EntryId";
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> list = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>
			{
				new Kingdee.BOS.SqlParam("@EntryId", Kingdee.BOS.KDDbType.Int64, entryId)
			};
			return DBServiceHelper.ExecuteScalar<int>(base.Context, strSql, 0, list.ToArray());
		}
		private string GetHMIAutoStartOper(long workCenterId)
		{
			string strSql = "select FHMIDISPAUTOSTART from  T_ENG_WORKCENTER where fid=@workCenterId";
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> list = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>();
			Kingdee.BOS.SqlParam item = new Kingdee.BOS.SqlParam("@workCenterId", Kingdee.BOS.KDDbType.Int64, workCenterId);
			list.Add(item);
			return DBServiceHelper.ExecuteScalar<string>(base.Context, strSql, string.Empty, list.ToArray());
		}
		private System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> SetEmp(BeforeUpdateValueEventArgs e)
		{
			this.userId = -1L;
			System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> list = (
				from a in this.empListObject
				where a["Number"].ToString().Contains(e.Value.ToString())
				select a).ToList<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
			if (list != null && list.Count > 0)
			{
				this.empId = System.Convert.ToInt64(list.FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>()["Id"]);
				this.empName = System.Convert.ToString(list.FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>()["Name"]);
				if (this.empAndUserId.Keys.Contains(this.empId))
				{
					this.userId = this.empAndUserId[this.empId];
				}
			}
			return list;
		}
		private void SetDispatchQty()
		{
			System.Collections.Generic.Dictionary<string, object> currentRowData = base.GetCurrentRowData();
			long num = System.Convert.ToInt64(currentRowData["OperId"]);
			base.View.GetControl("FButton_Confirm").Enabled = true;
			base.View.UpdateView("FButton_Confirm");
			decimal num2 = System.Convert.ToDecimal(currentRowData["FDispatchQty"]);
			this.Model.DataObject["ClaimQty"] = num2;
			base.View.SetControlProperty("FClaimQty", num2);
			if (this.OperUnitPrecision.Keys.Contains(num))
			{
				base.View.GetControl<DecimalFieldEditor>("FClaimQty").Scale = this.OperUnitPrecision[num];
			}
			base.View.UpdateView("FClaimQty");
			this.SetStartTimeAndEndTime(num, "PlanBeginTime", "PlanEndTime");
			base.View.UpdateView("FPlanBeginTime");
			base.View.UpdateView("FPlanEndTime");
		}
		private void SetDispatchDetailQty()
		{
			int[] selectedRows = base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail").GetSelectedRows();
			int num = selectedRows.FirstOrDefault<int>() + this.RowCountPerPage * (this.detailCurrPageIndex - 1);
			if (num == 0)
			{
				num++;
			}
			if (this.dispatchDetails.Count > 0)
			{
				System.Collections.Generic.Dictionary<string, object> dictionary = this.detailTableData[num - 1];
				if (dictionary["FStatus"].ToString() == "开工")
				{
					base.View.GetControl("FBtn_ClaimModify").Enabled = false;
					base.View.GetControl("FBtn_Delete").Enabled = false;
					base.View.GetControl("FBtn_Close").Enabled = true;
					base.View.GetControl("FClaimedQty").Enabled = false;
					base.View.GetControl("FPlanBeginedTime").Enabled = false;
					base.View.GetControl("FPlanEndedTime").Enabled = false;
					this.SetClaimed();
				}
				else
				{
					if (dictionary["FStatus"].ToString() == "未开工")
					{
						long num2 = System.Convert.ToInt64(dictionary["PkId"]);
						base.View.GetControl("FBtn_ClaimModify").Enabled = true;
						base.View.GetControl("FBtn_Delete").Enabled = true;
						base.View.GetControl("FBtn_Close").Enabled = false;
						decimal num3 = System.Convert.ToDecimal(dictionary["FWorkQty"]);
						this.Model.DataObject["ClaimedQty"] = num3;
						base.View.SetControlProperty("FClaimedQty", num3);
						base.View.GetControl("FClaimedQty").Enabled = true;
						base.View.GetControl("FPlanBeginedTime").Enabled = true;
						base.View.GetControl("FPlanEndedTime").Enabled = true;
						if (this.DisOperUnitPrecision.Keys.Contains(num2))
						{
							base.View.GetControl<DecimalFieldEditor>("FClaimedQty").Scale = this.DisOperUnitPrecision[num2];
						}
						base.View.UpdateView("FClaimedQty");
						System.Convert.ToInt64(dictionary["FOperId"]);
						this.Model.DataObject["PlanBeginedTime"] = dictionary["FPlanBeginTime"];
						this.Model.DataObject["PlanEndedTime"] = dictionary["FPlanEndTime"];
						base.View.UpdateView("FPlanBeginedTime");
						base.View.UpdateView("FPlanEndedTime");
					}
				}
				base.View.UpdateView("FBtn_Delete");
				base.View.UpdateView("FBtn_ClaimModify");
				base.View.UpdateView("FBtn_Close");
			}
		}
		private void SetClaimed()
		{
			this.Model.DataObject["ClaimedQty"] = 0;
			base.View.SetControlProperty("FClaimedQty", 0);
			base.View.UpdateView("FClaimedQty");
			this.Model.DataObject["PlanBeginedTime"] = null;
			this.Model.DataObject["PlanEndedTime"] = null;
			base.View.UpdateView("FPlanBeginedTime");
			base.View.UpdateView("FPlanEndedTime");
		}
		private void SetClaim()
		{
			this.Model.DataObject["ClaimQty"] = 0;
			base.View.SetControlProperty("FClaimQty", 0);
			base.View.UpdateView("FClaimQty");
			this.Model.DataObject["PlanBeginTime"] = null;
			this.Model.DataObject["PlanEndTime"] = null;
			base.View.UpdateView("FPlanBeginTime");
			base.View.UpdateView("FPlanEndTime");
		}
		private void SetStartTimeAndEndTime(long operId, string startTime, string endTime)
		{
			System.Collections.Generic.Dictionary<long, Tuple<long, long, long>> pWDId = this.GetPWDId(operId);
			System.Collections.Generic.Dictionary<long, Tuple<System.DateTime, System.DateTime>> workCenterCalendar = SFSDiscreteServiceHelper.GetWorkCenterCalendar(base.Context, pWDId[operId].Item2);
			this.Model.DataObject[startTime] = workCenterCalendar[pWDId[operId].Item2].Item1;
			this.Model.DataObject[endTime] = workCenterCalendar[pWDId[operId].Item2].Item2;
		}
		private void BindDispatchDetailList(string scanCode)
		{
			this.dispatchDetails.Clear();
			System.Collections.Generic.List<long> permissionPrdLineIds = this.GetPermissionPrdLineIds(this.empAndUserId.Values.ToList<long>());
			System.Collections.Generic.List<long> list = new System.Collections.Generic.List<long>();
			System.Collections.Generic.List<long> list2 = (
				from a in this.empListObject
				select System.Convert.ToInt64(a["Id"])).ToList<long>();
			foreach (System.Collections.Generic.KeyValuePair<object, long> current in this.empAndUserId)
			{
				System.Collections.Generic.List<long> userOrg = SFSDiscreteServiceHelper.GetUserOrg(base.Context, current.Value);
				list.AddRange(userOrg);
			}
			if (permissionPrdLineIds.IsEmpty<long>())
			{
				this.PrepareDispatchDetailTableData(null);
				return;
			}
			System.Collections.Generic.List<long> list3 = new System.Collections.Generic.List<long>();
			OQLFilter ofilter;
			if (scanCode.IsNullOrEmptyOrWhiteSpace())
			{
				list3 = this.GetDispatchIdByScanCode(scanCode, permissionPrdLineIds, list, list2);
				ofilter = OQLFilter.CreateHeadEntityFilter(string.Format(" FENTRYID in ({0})   AND FStatus in ('A', 'B')  ", string.Join<long>(",", list3)));
			}
			else
			{
				list3 = this.GetDispatchIdByScanCode(scanCode, permissionPrdLineIds, list, list2);
				if (list3.Count == 1 && list3[0] == 0L)
				{
					permissionPrdLineIds.Clear();
					list.Clear();
					list2.Clear();
					System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> list4 = (
						from a in this.empListObject
						where a["Number"].ToString().Contains(scanCode)
						select a).ToList<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
					if (list4 != null && list4.Count > 0)
					{
						this.empId = System.Convert.ToInt64(list4.FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>()["Id"]);
						this.empName = System.Convert.ToString(list4.FirstOrDefault<Kingdee.BOS.Orm.DataEntity.DynamicObject>()["Name"]);
						if (this.empAndUserId.Keys.Contains(this.empId))
						{
							this.userId = this.empAndUserId[this.empId];
						}
					}
					if (list4.Count > 0)
					{
						list2 = (
							from a in this.empAndUserId
							where a.Value == this.userId
							select a into b
							select System.Convert.ToInt64(b.Key)).ToList<long>();
						permissionPrdLineIds = this.GetPermissionPrdLineIds(new System.Collections.Generic.List<long>
						{
							this.userId
						});
						list = SFSDiscreteServiceHelper.GetUserOrg(base.Context, this.userId);
						list3 = this.GetDispatchIdByScanCode("", permissionPrdLineIds, list, list2);
						base.View.GetControl("FLable_ClaimedEmp").SetValue(this.empName);
						base.View.UpdateView("FLable_ClaimedEmp");
					}
					else
					{
						base.View.GetControl("FLable_ClaimedEmp").SetValue("");
						base.View.UpdateView("FLable_ClaimedEmp");
						base.View.ShowErrMessage(Kingdee.BOS.Resource.ResManager.LoadKDString("该员工不在您的代理范围之内，请检查！", "0151515153512030033832", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]), "", MessageBoxType.Notice);
					}
				}
				ofilter = OQLFilter.CreateHeadEntityFilter(string.Format(" FENTRYID in ({0})  AND FStatus in ('A', 'B') ", string.Join<long>(",", list3)));
			}
			Kingdee.BOS.Orm.DataEntity.DynamicObject[] array = BusinessDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", null, ofilter);
			if (list3.Count > 0)
			{
				Kingdee.BOS.Orm.DataEntity.DynamicObject[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = array2[i];
//******//					//Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectItemValue = dynamicObject.GetDynamicObjectItemValue("DispatchDetailEntry", null);
					//System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject> list5 = new System.Collections.Generic.List<Kingdee.BOS.Orm.DataEntity.DynamicObject>();
					//foreach (Kingdee.BOS.Orm.DataEntity.DynamicObject current2 in dynamicObjectItemValue)
					//{
					//	if (!list3.Contains(System.Convert.ToInt64(current2["Id"])))
					//	{
					//		list5.Add(current2);
					//	}
					//}
					//foreach (Kingdee.BOS.Orm.DataEntity.DynamicObject current3 in list5)
					//{
					//	dynamicObjectItemValue.Remove(current3);
					//}
				}
			}
			this.dispatchDetails.AddRange(array);
			Kingdee.BOS.Orm.DataEntity.DynamicObject[] array3 = array;
			for (int j = 0; j < array3.Length; j++)
			{
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject2 = array3[j];
				long num = System.Convert.ToInt64(dynamicObject2["Id"]);
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject3 = dynamicObject2["OperUnitID"] as Kingdee.BOS.Orm.DataEntity.DynamicObject;
				short value = System.Convert.ToInt16(dynamicObject3["Precision"]);
				if (!this.DisOperUnitPrecision.Keys.Contains(num))
				{
					this.DisOperUnitPrecision.Add(num, value);
				}
			}
			this.PrepareDispatchDetailTableData(null);
		}
		private void PrepareDispatchDetailTableData(System.Collections.Generic.Dictionary<string, string> dicKeyFilter)
		{
			System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
			System.Collections.Generic.Dictionary<string, string> dictionary = new System.Collections.Generic.Dictionary<string, string>();
			this.PrepareDispDetailBindFields(dictionary);
			list.AddRange(dictionary.Keys);
			System.Collections.Generic.Dictionary<string, string> dictionary2 = (
				from w in base.View.LayoutInfo.Appearances
				where w.ElementType == 1001 && w.Key.Contains("FDLbl_")
				select w).ToDictionary((Appearance k) => k.Key.Substring(k.Key.IndexOf("FDLbl_", System.StringComparison.Ordinal) + 6), (Appearance v) => v.Key);
			list.AddRange(dictionary2.Keys);
			FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, "SFC_DispatchDetail", true);
			this.detailTableData = DataUtils.BuildTwoDimTable(base.Context, this.dispatchDetails, formMetadata.BusinessInfo, list);
			this.PrepareDetailDicTableData(this.dispatchDetails, this.detailTableData);
			if (dicKeyFilter != null && dicKeyFilter.Count > 0)
			{
				this.detailTableData = this.GetDispatchedTableDataByFilter(dicKeyFilter);
			}
			this.detailCurrPageIndex = 1;
			this.FillReturnToOperList();
		}
		protected virtual void PrepareDispDetailBindFields(System.Collections.Generic.Dictionary<string, string> dicFieldLabelKeys)
		{
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FMoBillNo", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FMoSeq", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FOptPlanNo", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FSeqNumber", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FOperNumber", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FProcessId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FMaterialId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FMaterialNumber", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FEmpId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FEquipmentId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FShiftGroupId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FPlanBeginTime", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FPlanEndTime", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FWorkQty", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FFinishSelQty", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FStatus", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FOperId", "");
			DataUtils.AddDicFieldLabel(dicFieldLabelKeys, "FBarCode", "");
		}
		public System.Collections.Generic.List<long> GetPermissionPrdLineIds(System.Collections.Generic.List<long> userId)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.AppendLine("SELECT DISTINCT DSP1.FWCID FROM T_SFC_DSPRPTPERMENTRY DSP1 ");
			stringBuilder.AppendLine("INNER JOIN T_SFC_DSPRPTPERM DSP0 ON DSP1.FID = DSP0.FID ");
			stringBuilder.AppendLine(string.Format("INNER JOIN (SELECT /*+ cardinality(be {0})*/ FID FROM table(fn_StrSplit(@UserId, ',', 1)) be) e on e.FID = DSP0.FUSERID ", userId.Distinct<long>().Count<long>()));
			stringBuilder.AppendLine("WHERE DSP1.FISCHECKED = '1'");
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> list = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>();
			list.Add(new Kingdee.BOS.SqlParam("@UserId", Kingdee.BOS.KDDbType.udt_inttable, userId.Distinct<long>().ToArray<long>()));
			System.Collections.Generic.List<long> list2 = new System.Collections.Generic.List<long>();
			if (userId.Count == 0)
			{
				return list2;
			}
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, stringBuilder.ToString(), list))
			{
				while (dataReader.Read())
				{
					long item = System.Convert.ToInt64(dataReader["FWCID"]);
					list2.Add(item);
				}
			}
			return list2;
		}
		private System.Collections.Generic.List<long> GetDispatchIdByScanCode(string scanCode, System.Collections.Generic.List<long> authPrdLines, System.Collections.Generic.List<long> OrgIds, System.Collections.Generic.List<long> EmpIds)
		{
			System.Collections.Generic.List<long> list = new System.Collections.Generic.List<long>();
			string text = "";
			if (!string.IsNullOrEmpty(scanCode))
			{
				text = "   AND  DSPE.FBARCODE LIKE '%" + scanCode + "%' ";
			}
			list.Add(0L);
			string strSQL = string.Format("SELECT DSPE.FENTRYID,DSP.FID FROM T_SFC_DISPATCHDETAIL DSP\r\n                           INNER JOIN T_SFC_DISPATCHDETAILENTRY DSPE ON DSPE.FID = DSP.FID\r\n                           INNER JOIN T_SFC_DISPATCHDETAILENTRYEMPS EMP ON EMP.FENTRYID=DSPE.FENTRYID\r\n                           INNER JOIN T_SFC_OPERPLANNINGDETAIL OPD ON OPD.FDETAILID = DSP.FOPERID\r\n                           INNER JOIN  (SELECT /*+ cardinality(be {0})*/ FID FROM table(fn_StrSplit(@OrgIds, ',', 1)) be) B on B.FID = OPD.FProcessOrgId\r\n                           INNER JOIN (SELECT /*+ cardinality(be {1})*/ FID FROM table(fn_StrSplit(@Ids, ',', 1)) be) E on E.FID = DSP.FWORKCENTERID\r\n                           INNER JOIN (SELECT /*+ cardinality(be {2})*/ FID FROM table(fn_StrSplit(@EmpIds, ',', 1)) be) W on W.FID = EMP.FEMPID\r\n                           WHERE   DSPE.FSTATUS in ('A', 'B') {3} ", new object[]
			{
				OrgIds.Distinct<long>().Count<long>(),
				authPrdLines.Distinct<long>().Count<long>(),
				EmpIds.Distinct<long>().Count<long>(),
				text
			});
			System.Collections.Generic.List<Kingdee.BOS.SqlParam> list2 = new System.Collections.Generic.List<Kingdee.BOS.SqlParam>();
			list2.Add(new Kingdee.BOS.SqlParam("@OrgIds", Kingdee.BOS.KDDbType.udt_inttable, OrgIds.Distinct<long>().ToArray<long>()));
			list2.Add(new Kingdee.BOS.SqlParam("@Ids", Kingdee.BOS.KDDbType.udt_inttable, authPrdLines.Distinct<long>().ToArray<long>()));
			list2.Add(new Kingdee.BOS.SqlParam("@EmpIds", Kingdee.BOS.KDDbType.udt_inttable, EmpIds.Distinct<long>().ToArray<long>()));
			using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.Context, strSQL, list2))
			{
				while (dataReader.Read())
				{
					long num = System.Convert.ToInt64(dataReader["FENTRYID"]);
					if (!list.Contains(num) && num != 0L)
					{
						list.Add(num);
					}
				}
			}
			return list;
		}
		protected virtual void PrepareDetailDicTableData(System.Collections.Generic.IEnumerable<Kingdee.BOS.Orm.DataEntity.DynamicObject> datas, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> dicTableData)
		{
			if (!datas.Any<Kingdee.BOS.Orm.DataEntity.DynamicObject>())
			{
				return;
			}
			System.Collections.Generic.Dictionary<long, Kingdee.BOS.Orm.DataEntity.DynamicObject> dictionary = datas.SelectMany((Kingdee.BOS.Orm.DataEntity.DynamicObject s) => (Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection)s["DispatchDetailEntry"]).ToDictionary((Kingdee.BOS.Orm.DataEntity.DynamicObject k) => System.Convert.ToInt64(k["Id"]));
			System.Collections.Generic.Dictionary<long, string> seqTypeByDispatchEntryId = SFCCommonUtils.Instance.GetSeqTypeByDispatchEntryId(base.Context, dictionary.Keys.ToList<long>());
			foreach (System.Collections.Generic.Dictionary<string, object> current in dicTableData.Values)
			{
				Kingdee.BOS.Orm.DataEntity.DynamicObject dynamicObject = dictionary[System.Convert.ToInt64(current["EntryPkId"])];
				object arg_A2_0 = dynamicObject.Parent;
				string type;
				seqTypeByDispatchEntryId.TryGetValue(System.Convert.ToInt64(current["EntryPkId"]), out type);
				current["FDispDetailId"] = dynamicObject["Id"];
				current["FMONumber"] = string.Format("{0}-{1}", current["FMoBillNo"], current["FMoSeq"]);
				current["FOperPlanNo"] = string.Format("{0}-{1}-{2} ({3})", new object[]
				{
					current["FOptPlanNo"],
					current["FSeqNumber"],
					current["FOperNumber"],
					SFCCommonUtils.Instance.ReturnOPlanSeqType(type)
				});
				current["FProductId"] = string.Format("{0}/{1}", current["FMaterialNumber"], current["FMaterialId"]);
				current["FOperator"] = current["FEmpId"];
				current["FEquipmentId"] = string.Format("{0}/{1}", current["FShiftGroupId"], current["FEquipmentId"]);
				current["FPlanDate"] = string.Format("{0}~{1}", current["FPlanBeginTime"], current["FPlanEndTime"]);
				current["FDispatchQty"] = string.Format("{0}/{1}", current["FWorkQty"], current["FFinishSelQty"]);
				current["FStatus"] = ("A".Equals(current["FStatus"]) ? Kingdee.BOS.Resource.ResManager.LoadKDString("未开工", "015747000015457", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]) : Kingdee.BOS.Resource.ResManager.LoadKDString("开工", "015747000013796", Kingdee.BOS.Resource.SubSystemType.MFG, new object[0]));
			}
			System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> list = (
				from t in dicTableData.Values
				orderby t["FOptPlanNo"], t["FSeqNumber"], t["FOperNumber"], t["FPlanBeginTime"]
				select t).ToList<System.Collections.Generic.Dictionary<string, object>>();
			dicTableData.Clear();
			int num = 0;
			foreach (System.Collections.Generic.Dictionary<string, object> current2 in list)
			{
				this.DicDetailRowIndexRelation[num] = num;
				dicTableData[num++] = current2;
			}
		}
		protected System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetDispatchedTableDataByFilter(System.Collections.Generic.Dictionary<string, string> dicKeyFilter)
		{
			int num = 0;
			this.DicDetailRowIndexRelation = new System.Collections.Generic.Dictionary<int, int>();
			System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> dictionary = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>>();
			foreach (System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>> current in this.detailTableData)
			{
				bool flag = true;
				foreach (System.Collections.Generic.KeyValuePair<string, string> current2 in dicKeyFilter)
				{
					if (System.Convert.ToString(current.Value[current2.Key]).IndexOf(current2.Value, System.StringComparison.OrdinalIgnoreCase) < 0)
					{
						flag = false;
					}
				}
				if (flag)
				{
					dictionary.Add(num, current.Value);
					this.DicDetailRowIndexRelation.Add(num, num);
					num++;
				}
			}
			return dictionary;
		}
		private void FillReturnToOperList()
		{
			this.detailFormatterManager.Clear();
			System.Collections.Generic.Dictionary<string, string> first = (
				from w in base.View.LayoutInfo.Appearances
				where w.ElementType == 1001 && w.Key.Contains("FDLbl_")
				select w).ToDictionary((Appearance k) => k.Key.Substring(k.Key.IndexOf("FDLbl_", System.StringComparison.Ordinal) + 6), (Appearance v) => v.Key);
			System.Collections.Generic.Dictionary<string, string> second = (
				from w in base.View.LayoutInfo.Appearances
				where w.ElementType == 1001 && w.Key.Contains("FDLbl_") && w.Key.Contains("_Filter")
				select w).ToDictionary((Appearance k) => k.Key.Substring(k.Key.IndexOf("FDLbl_", System.StringComparison.Ordinal) + 6, k.Key.IndexOf("_Filter", System.StringComparison.Ordinal) - (k.Key.IndexOf("FDLbl_", System.StringComparison.Ordinal) + 6)), (Appearance v) => v.Key);
			System.Collections.Generic.Dictionary<string, string> dictionary = first.Concat(second).DistinctBy((System.Collections.Generic.KeyValuePair<string, string> o) => o.Key).ToDictionary((System.Collections.Generic.KeyValuePair<string, string> k) => k.Key, (System.Collections.Generic.KeyValuePair<string, string> v) => v.Value);
			this.Model.DeleteEntryData("FMobileListViewEntity_Detail");
			base.View.GetEntryState("FMobileListViewEntity_Detail").CurrentPageIndex = this.detailCurrPageIndex - 1;
			this.detailTotalPageNumber = System.Convert.ToInt32(System.Math.Ceiling(System.Convert.ToDecimal(this.detailTableData.Count) / this.RowCountPerPage));
			System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>>> enumerable =
				from w in this.detailTableData
				where w.Key >= this.RowCountPerPage * (this.detailCurrPageIndex - 1) && w.Key <= this.RowCountPerPage * this.detailCurrPageIndex - 1
				select w;
			int num = 0;
			foreach (System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>> current in enumerable)
			{
				int rowIndex = current.Key - this.RowCountPerPage * (this.detailCurrPageIndex - 1);
				this.Model.CreateNewEntryRow("FMobileListViewEntity_Detail");
				foreach (System.Collections.Generic.KeyValuePair<string, object> current2 in current.Value)
				{
					if (dictionary.ContainsKey(current2.Key))
					{
						this.detailFormatterManager.SetControlProperty(dictionary[current2.Key], rowIndex, current2.Value, MobileFormatConditionPropertyEnums.Value);
					}
				}
				this.detailFormatterManager.SetControlProperty("FFlowLayout_Detail", num, "255,255,255", MobileFormatConditionPropertyEnums.BackColor);
				this.PrepareListFormatter(current, num);
				num++;
			}
			if (enumerable.Count<System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>>>() == 1 && this.IsNeedHighLight)
			{
				MobileListViewControl arg_2DF_0 = base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail");
				int[] selectRows = new int[1];
				arg_2DF_0.SetSelectRows(selectRows);
				this.detailFormatterManager.SetControlProperty("FFlowLayout_Detail", 0, "255,234,199", MobileFormatConditionPropertyEnums.BackColor);
			}
			else
			{
				base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail").SetSelectRows(new int[0]);
			}
			base.View.GetControl<MobileListViewControl>("FMobileListViewEntity_Detail").setFormat(this.detailFormatterManager);
			base.View.UpdateView("FMobileListViewEntity_Detail");
			base.View.SetControlProperty("FLable_CurPageNo1", string.Format("{0}/{1}", this.detailCurrPageIndex, this.detailTotalPageNumber));
			this.SetReturnToPageTurnStyle();
			if (enumerable.Count<System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>>>() == 1)
			{
				this.SetDispatchDetailQty();
				return;
			}
			this.SetClaimed();
		}
		protected virtual void SetReturnToPageTurnStyle()
		{
			base.View.GetControl("FButton_Previous1").Enabled = true;
			base.View.GetControl("FButton_Next1").Enabled = true;
			if (this.detailCurrPageIndex == 1)
			{
				base.View.GetControl("FButton_Previous1").Enabled = false;
				base.View.UpdateView("FButton_Previous1");
			}
			if (this.detailCurrPageIndex == this.detailTotalPageNumber)
			{
				base.View.GetControl("FButton_Next1").Enabled = false;
				base.View.UpdateView("FButton_Next1");
			}
			if (this.detailTableData.IsEmpty<System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>>>())
			{
				base.View.GetControl("FButton_Previous1").Enabled = false;
				base.View.UpdateView("FButton_Previous1");
				base.View.GetControl("FButton_Next1").Enabled = false;
				base.View.UpdateView("FButton_Next1");
			}
			base.View.SetControlProperty("FLable_CurPageNo1", string.Format("{0}/{1}", this.detailTableData.IsEmpty<System.Collections.Generic.KeyValuePair<int, System.Collections.Generic.Dictionary<string, object>>>() ? 0 : this.detailCurrPageIndex, this.detailTotalPageNumber));
		}
	}
}
