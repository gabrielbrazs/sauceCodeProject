using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildSmithGrow : EquipMaterialBase
{
	private int aimLv;

	private int terminateAimLv;

	private bool terminating;

	private int baseLv;

	private NeedMaterial[][] needMmaterialDB;

	private int[] needMoneyDB;

	private uint modelID;

	private int chooseIndex = -1;

	private bool backSection;

	protected override NOTIFY_FLAG GetUpdateUINotifyFlags()
	{
		return base.GetUpdateUINotifyFlags() | NOTIFY_FLAG.UPDATE_EQUIP_FAVORITE;
	}

	public override void Initialize()
	{
		smithType = SmithType.GROW;
		SmithManager.SmithGrowData smithData = MonoBehaviourSingleton<SmithManager>.I.GetSmithData<SmithManager.SmithGrowData>();
		GameSection.SetEventData(smithData.selectEquipData);
		base.Initialize();
		EquipItemInfo equipData = GetEquipData();
		if (equipData != null)
		{
			string text = base.sectionData.GetText("CAPTION_GUILD_REQUEST");
			MonoBehaviourSingleton<UIManager>.I.common.AttachCaption(this, base.sectionData.backButtonIndex, text);
		}
		aimLv = GetEquipData().level + 1;
	}

	protected override void OnOpen()
	{
		if (aimLv == 0)
		{
			aimLv = GetEquipData().level + 1;
		}
		NewNeedDB();
		terminateAimLv = -1;
		terminating = false;
		if (aimLv <= GetEquipData().tableData.maxLv)
		{
			EquipItemInfo equipData = GetEquipData();
			if (equipData != null && (!MonoBehaviourSingleton<InventoryManager>.I.IsHaveingMaterial(equipData.nextNeedTableData.needMaterial) || MonoBehaviourSingleton<UserInfoManager>.I.userStatus.money < equipData.nextNeedTableData.needMoney))
			{
				terminateAimLv = aimLv;
				terminating = true;
			}
		}
		base.OnOpen();
	}

	public override void UpdateUI()
	{
		base.UpdateUI();
		int num = Mathf.Min(aimLv, GetEquipData().tableData.maxLv);
		SetLabelText(UI.LBL_AIM_LV, num.ToString());
		SetActive(UI.STR_ONLY_EXCEED, false);
		Color color = Color.red;
		if (num == GetEquipData().level)
		{
			SetActive(UI.STR_ONLY_EXCEED, true);
			color = Color.gray;
		}
		else if (IsHavingMaterialAndMoney() && num > GetEquipData().level)
		{
			color = Color.white;
		}
		SetColor(UI.LBL_AIM_LV, color);
		bool flag = aimLv > GetEquipData().level + 1;
		bool flag2 = aimLv < GetEquipData().tableData.maxLv;
		SetColor(UI.SPR_AIM_L, (!flag) ? Color.clear : Color.white);
		SetColor(UI.SPR_AIM_R, (!flag2) ? Color.clear : Color.white);
		SetButtonEnabled(UI.BTN_AIM_L, flag);
		SetButtonEnabled(UI.BTN_AIM_R, flag2);
		SetActive(UI.BTN_AIM_L_INACTIVE, !flag);
		SetActive(UI.BTN_AIM_R_INACTIVE, !flag2);
		SetRepeatButton(UI.BTN_AIM_L, "AIM_L", null);
		SetRepeatButton(UI.BTN_AIM_R, "AIM_R", null);
		SetActive(UI.BTN_EXCEED, false);
		SetActive(UI.BTN_DECISION, false);
		SetActive(UI.BTN_INACTIVE, false);
		SetActive(UI.LBL_GOLD, false);
		SetActive(UI.LinePartsR01, false);
	}

	protected override void InitNeedMaterialData()
	{
		EquipItemInfo equipData = GetEquipData();
		if (equipData != null)
		{
			if (aimLv <= GetEquipData().tableData.maxLv)
			{
				needMaterial = MaterialSort(GetMaterialDB(aimLv));
				needMoney = GetMoneyDB(aimLv);
			}
			else
			{
				needMaterial = null;
				needMoney = 0;
			}
			CheckNeedMaterialNumFromInventory();
		}
	}

	protected override string CreateItemDetailPrefabName()
	{
		return "SmithGrowItem";
	}

	protected override void EquipParam()
	{
		int num = Mathf.Min(aimLv, GetEquipData().tableData.maxLv);
		EquipItemInfo equipData = GetEquipData();
		EquipItemTable.EquipItemData tableData = equipData.tableData;
		GrowEquipItemTable.GrowEquipItemData growEquipItemData = Singleton<GrowEquipItemTable>.I.GetGrowEquipItemData(tableData.growID, (uint)num);
		if (equipData != null && tableData != null)
		{
			SetLabelText(detailBase, UI.LBL_NAME, tableData.name);
			SetLabelText(detailBase, UI.LBL_LV_MAX, tableData.maxLv.ToString());
			SetLabelCompareParam(detailBase, UI.LBL_LV_NOW, num, equipData.level, -1);
			SetEquipmentTypeIcon(detailBase, UI.SPR_TYPE_ICON, UI.SPR_TYPE_ICON_BG, UI.SPR_TYPE_ICON_RARITY, equipData.tableData);
			if (growEquipItemData != null)
			{
				EquipItemExceedParamTable.EquipItemExceedParamAll equipItemExceedParamAll = equipData.tableData.GetExceedParam((uint)equipData.exceed);
				if (equipItemExceedParamAll == null)
				{
					equipItemExceedParamAll = new EquipItemExceedParamTable.EquipItemExceedParamAll();
				}
				int num2 = growEquipItemData.GetGrowParamAtk(equipData.tableData.baseAtk) + (int)equipItemExceedParamAll.atk;
				int[] growParamElemAtk = growEquipItemData.GetGrowParamElemAtk(equipData.tableData.atkElement);
				int i = 0;
				for (int num3 = growParamElemAtk.Length; i < num3; i++)
				{
					growParamElemAtk[i] += equipItemExceedParamAll.atkElement[i];
				}
				int num4 = Mathf.Max(growParamElemAtk);
				SetElementSprite(detailBase, UI.SPR_ELEM, equipData.GetElemAtkType());
				SetLabelText(detailBase, UI.LBL_ATK, equipData.atk.ToString());
				SetActive(detailBase, UI.LBL_AFTER_ATK, num2 > equipData.atk);
				SetStatusBuffText(detailBase, UI.LBL_AFTER_ATK, num2 - equipData.atk, false);
				SetLabelText(detailBase, UI.LBL_ELEM, equipData.elemAtk.ToString());
				SetActive(detailBase, UI.LBL_AFTER_ELEM, num4 > equipData.elemAtk);
				SetStatusBuffText(detailBase, UI.LBL_AFTER_ELEM, num4 - equipData.elemAtk, false);
				int num5 = growEquipItemData.GetGrowParamDef(equipData.tableData.baseDef) + (int)equipItemExceedParamAll.def;
				int[] growParamElemDef = growEquipItemData.GetGrowParamElemDef(equipData.tableData.defElement);
				int j = 0;
				for (int num6 = growParamElemDef.Length; j < num6; j++)
				{
					growParamElemDef[j] += equipItemExceedParamAll.defElement[j];
				}
				int num7 = Mathf.Max(growParamElemDef);
				SetDefElementSprite(detailBase, UI.SPR_ELEM_DEF, equipData.GetElemDefType());
				SetLabelText(detailBase, UI.LBL_DEF, equipData.def.ToString());
				SetActive(detailBase, UI.LBL_AFTER_DEF, num5 > equipData.def);
				SetStatusBuffText(detailBase, UI.LBL_AFTER_DEF, num5 - equipData.def, false);
				int num8 = equipData.elemDef;
				if (equipData.tableData.isFormer)
				{
					num8 = Mathf.FloorToInt((float)num8 * 0.1f);
				}
				SetLabelText(detailBase, UI.LBL_ELEM_DEF, num8.ToString());
				SetActive(detailBase, UI.LBL_AFTER_ELEM_DEF, num7 > num8);
				SetStatusBuffText(detailBase, UI.LBL_AFTER_ELEM_DEF, num7 - num8, false);
				int num9 = growEquipItemData.GetGrowParamHp(equipData.tableData.baseHp) + (int)equipItemExceedParamAll.hp;
				SetLabelText(detailBase, UI.LBL_HP, equipData.hp.ToString());
				SetActive(detailBase, UI.LBL_AFTER_HP, num9 > equipData.hp);
				SetStatusBuffText(detailBase, UI.LBL_AFTER_HP, num9 - equipData.hp, false);
			}
			else
			{
				int atk = equipData.atk;
				int elemAtk = equipData.elemAtk;
				SetElementSprite(detailBase, UI.SPR_ELEM, equipData.GetElemAtkType());
				SetLabelText(detailBase, UI.LBL_ATK, atk.ToString());
				SetLabelText(detailBase, UI.LBL_ELEM, elemAtk.ToString());
				SetActive(detailBase, UI.LBL_AFTER_ATK, false);
				SetActive(detailBase, UI.LBL_AFTER_ELEM, false);
				int def = equipData.def;
				int elemDef = equipData.elemDef;
				SetDefElementSprite(detailBase, UI.SPR_ELEM_DEF, equipData.GetElemDefType());
				SetLabelText(detailBase, UI.LBL_DEF, def.ToString());
				SetLabelText(detailBase, UI.LBL_ELEM_DEF, elemDef.ToString());
				SetActive(detailBase, UI.LBL_AFTER_DEF, false);
				SetActive(detailBase, UI.LBL_AFTER_ELEM_DEF, false);
				int hp = equipData.hp;
				SetLabelText(detailBase, UI.LBL_HP, hp.ToString());
				SetActive(detailBase, UI.LBL_AFTER_HP, false);
			}
		}
	}

	protected override void EquipImg()
	{
		uint id = GetEquipTableData().id;
		if (modelID != id)
		{
			modelID = id;
			SetRenderEquipModel(UI.TEX_MODEL, id, -1, -1, 1f);
		}
	}

	private void OnQuery_AIM_L()
	{
		EquipItemInfo equipData = GetEquipData();
		if (aimLv - 1 != equipData.level)
		{
			aimLv--;
			if (aimLv < terminateAimLv)
			{
				terminateAimLv = -1;
				terminating = false;
			}
			SetDirty(UI.GRD_NEED_MATERIAL);
			RefreshUI();
		}
	}

	private void OnQuery_AIM_R()
	{
		EquipItemInfo equipData = GetEquipData();
		if (aimLv != equipData.tableData.maxLv)
		{
			aimLv++;
			SetDirty(UI.GRD_NEED_MATERIAL);
			RefreshUI();
			if (!IsHavingMaterialAndMoney())
			{
				if (terminateAimLv < 0)
				{
					terminateAimLv = aimLv;
				}
				if (terminateAimLv == aimLv && !terminating)
				{
					terminating = true;
					TerminateRepeatButton(UI.BTN_AIM_R);
				}
			}
		}
	}

	private void OnQuery_CLEARLEVEL()
	{
		baseLv = GetEquipData().level;
		aimLv = baseLv + 1;
		SetDirty(UI.GRD_NEED_MATERIAL);
		RefreshUI();
	}

	private void OnQuery_SmithConfirmGrow_YES()
	{
		OnQueryConfirmYES();
	}

	protected override void Send()
	{
		SmithManager.SmithGrowData smithData = MonoBehaviourSingleton<SmithManager>.I.GetSmithData<SmithManager.SmithGrowData>();
		if (smithData == null)
		{
			GameSection.StopEvent();
		}
		else
		{
			EquipItemInfo selectEquipData = smithData.selectEquipData;
			if (selectEquipData == null)
			{
				GameSection.StopEvent();
			}
			else
			{
				SmithManager.ResultData result_data = new SmithManager.ResultData();
				result_data.beforeRarity = (int)selectEquipData.tableData.rarity;
				result_data.beforeLevel = selectEquipData.level;
				result_data.beforeMaxLevel = selectEquipData.tableData.maxLv;
				result_data.beforeExceedCnt = selectEquipData.exceed;
				result_data.beforeAtk = selectEquipData.atk;
				result_data.beforeDef = selectEquipData.def;
				result_data.beforeHp = selectEquipData.hp;
				result_data.beforeElemAtk = selectEquipData.elemAtk;
				result_data.beforeElemDef = selectEquipData.elemDef;
				GameSection.SetEventData(result_data);
				isNotifySelfUpdate = true;
				GameSection.StayEvent();
				MonoBehaviourSingleton<SmithManager>.I.SendGrowEquipItem(selectEquipData.uniqueID, aimLv, delegate(Error err, EquipItemInfo grow_item)
				{
					if (err == Error.None)
					{
						aimLv = grow_item.level + 1;
						result_data.itemData = grow_item;
						MonoBehaviourSingleton<UIAnnounceBand>.I.isWait = true;
						GameSection.ResumeEvent(true, null);
					}
					else
					{
						isNotifySelfUpdate = false;
						GameSection.ResumeEvent(false, null);
					}
				});
			}
		}
	}

	private void NewNeedDB()
	{
		baseLv = GetEquipData().level;
		int num = GetEquipData().tableData.maxLv - baseLv;
		needMmaterialDB = new NeedMaterial[num][];
		needMoneyDB = new int[num];
	}

	private NeedMaterial[] GetMaterialDB(int aim_lv)
	{
		int num = aim_lv - baseLv - 1;
		if (needMmaterialDB[num] == null)
		{
			int num2 = num;
			if (num > 0)
			{
				int num3 = num;
				while (num3 >= 0 && needMmaterialDB[num3] == null)
				{
					if (needMmaterialDB[num3] == null)
					{
						num2 = num3;
					}
					num3--;
				}
			}
			for (int i = num2; i <= num; i++)
			{
				int lv = baseLv + 1 + i;
				GrowEquipItemTable.GrowEquipItemNeedItemData growEquipItemNeedItemData = Singleton<GrowEquipItemTable>.I.GetGrowEquipItemNeedUniqueItemData(GetEquipData().tableData.needUniqueId, (uint)lv);
				if (growEquipItemNeedItemData == null)
				{
					growEquipItemNeedItemData = Singleton<GrowEquipItemTable>.I.GetGrowEquipItemNeedItemData(GetEquipData().tableData.needId, (uint)lv);
				}
				NeedMaterial[] needMaterial = growEquipItemNeedItemData.needMaterial;
				int needMoney = growEquipItemNeedItemData.needMoney;
				if (i > 0)
				{
					List<NeedMaterial> before_material = new List<NeedMaterial>();
					Array.ForEach(needMmaterialDB[i - 1], delegate(NeedMaterial _mat)
					{
						before_material.Add(new NeedMaterial(_mat.itemID, _mat.num));
					});
					Array.ForEach(needMaterial, delegate(NeedMaterial _material)
					{
						NeedMaterial needMaterial2 = before_material.Find((NeedMaterial _data) => _data.itemID == _material.itemID);
						if (needMaterial2 != null)
						{
							needMaterial2.num += _material.num;
						}
						else
						{
							before_material.Add(new NeedMaterial(_material.itemID, _material.num));
						}
					});
					needMoney += needMoneyDB[i - 1];
					needMmaterialDB[i] = before_material.ToArray();
					needMoneyDB[i] = needMoney;
				}
				else
				{
					needMmaterialDB[i] = growEquipItemNeedItemData.needMaterial;
					needMoneyDB[i] = growEquipItemNeedItemData.needMoney;
				}
			}
		}
		return needMmaterialDB[num];
	}

	private int GetMoneyDB(int aim_lv)
	{
		int num = aim_lv - baseLv - 1;
		return needMoneyDB[num];
	}

	private void OnQuery_SECTION_BACK()
	{
		if (!MonoBehaviourSingleton<GameSceneManager>.I.ExistHistory("GuildSmithGrowItemSelect"))
		{
			GameSection.StopEvent();
			OnQuery_MAIN_MENU_STATUS();
		}
	}

	protected new void OnQuery_MATERIAL()
	{
		chooseIndex = (int)GameSection.GetEventData();
	}

	private void OnCloseDialog_GuildDonateSendDialog()
	{
		string s = GameSection.GetEventData() as string;
		int itemID = (int)needMaterial[chooseIndex].itemID;
		string name = Singleton<ItemTable>.I.GetItemData(needMaterial[chooseIndex].itemID).name;
		try
		{
			int num = int.Parse(s);
			if (num > 0 && chooseIndex >= 0)
			{
				StartCoroutine(CRSendDonateRequest(itemID, name, string.Empty, num));
				chooseIndex = -1;
			}
		}
		catch
		{
		}
		chooseIndex = -1;
	}

	private IEnumerator CRSendDonateRequest(int itemID, string itemName, string request, int numRequest)
	{
		yield return (object)new WaitUntil(() => !MonoBehaviourSingleton<GameSceneManager>.I.isChangeing && MonoBehaviourSingleton<GameSceneManager>.I.IsEventExecutionPossible());
		GameSection.StayEvent();
		MonoBehaviourSingleton<GuildManager>.I.SendDonateRequest(itemID, itemName, request, numRequest, delegate(bool success)
		{
			GameSection.ResumeEvent(success, null);
			if (success)
			{
				((_003CCRSendDonateRequest_003Ec__Iterator66)/*Error near IL_0077: stateMachine*/)._003C_003Ef__this.backSection = true;
			}
		});
	}

	private void Update()
	{
		if (backSection && MonoBehaviourSingleton<GameSceneManager>.I.IsEventExecutionPossible() && !MonoBehaviourSingleton<GameSceneManager>.I.isChangeing)
		{
			backSection = false;
			if (LoungeMatchingManager.IsValidInLounge())
			{
				MonoBehaviourSingleton<GameSceneManager>.I.ChangeScene("Lounge", "GuildDonateMaterialSelectDialog", UITransition.TYPE.CLOSE, UITransition.TYPE.OPEN, false);
			}
			else
			{
				MonoBehaviourSingleton<GameSceneManager>.I.ChangeScene("Home", "GuildDonateMaterialSelectDialog", UITransition.TYPE.CLOSE, UITransition.TYPE.OPEN, false);
			}
		}
	}
}
