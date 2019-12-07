using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmithGrow : EquipMaterialBase
{
	private int aimLv;

	private int terminateAimLv;

	private bool terminating;

	private int baseLv;

	private NeedMaterial[][] needMmaterialDB;

	private int[] needMoneyDB;

	private uint modelID;

	protected override NOTIFY_FLAG GetUpdateUINotifyFlags()
	{
		return base.GetUpdateUINotifyFlags() | NOTIFY_FLAG.UPDATE_EQUIP_FAVORITE;
	}

	public override void Initialize()
	{
		smithType = SmithType.GROW;
		GameSection.SetEventData(MonoBehaviourSingleton<SmithManager>.I.GetSmithData<SmithManager.SmithGrowData>().selectEquipData);
		base.Initialize();
		EquipItemInfo equipData = GetEquipData();
		if (equipData != null)
		{
			string caption = (!equipData.tableData.IsWeapon()) ? base.sectionData.GetText("CAPTION_DEFENCE") : base.sectionData.GetText("CAPTION_WEAPON");
			MonoBehaviourSingleton<UIManager>.I.common.AttachCaption(this, base.sectionData.backButtonIndex, caption);
		}
		aimLv = GetEquipData().level + 1;
		if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_ITEM))
		{
			StartCoroutine(UPGRADE_TOTURIAL_EVENT());
		}
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
		SetActive(UI.STR_ONLY_EXCEED, is_visible: false);
		Color color = Color.red;
		if (num == GetEquipData().level)
		{
			SetActive(UI.STR_ONLY_EXCEED, is_visible: true);
			color = Color.gray;
		}
		else if (IsHavingMaterialAndMoney() && num > GetEquipData().level)
		{
			color = Color.white;
		}
		SetColor(UI.LBL_AIM_LV, color);
		bool flag = aimLv > GetEquipData().level + 1;
		bool flag2 = aimLv < GetEquipData().tableData.maxLv;
		SetColor(UI.SPR_AIM_L, flag ? Color.white : Color.clear);
		SetColor(UI.SPR_AIM_R, flag2 ? Color.white : Color.clear);
		SetButtonEnabled(UI.BTN_AIM_L, flag);
		SetButtonEnabled(UI.BTN_AIM_R, flag2);
		SetActive(UI.BTN_AIM_L_INACTIVE, !flag);
		SetActive(UI.BTN_AIM_R_INACTIVE, !flag2);
		SetRepeatButton(UI.BTN_AIM_L, "AIM_L");
		SetRepeatButton(UI.BTN_AIM_R, "AIM_R");
	}

	protected override void InitNeedMaterialData()
	{
		if (GetEquipData() != null)
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
		if (equipData == null || tableData == null)
		{
			return;
		}
		SetLabelText(detailBase, UI.LBL_NAME, tableData.name);
		SetLabelText(detailBase, UI.LBL_LV_MAX, tableData.maxLv.ToString());
		SetLabelCompareParam(detailBase, UI.LBL_LV_NOW, num, equipData.level);
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
			SetStatusBuffText(detailBase, UI.LBL_AFTER_ATK, num2 - equipData.atk, expression_include: false);
			SetLabelText(detailBase, UI.LBL_ELEM, equipData.elemAtk.ToString());
			SetActive(detailBase, UI.LBL_AFTER_ELEM, num4 > equipData.elemAtk);
			SetStatusBuffText(detailBase, UI.LBL_AFTER_ELEM, num4 - equipData.elemAtk, expression_include: false);
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
			SetStatusBuffText(detailBase, UI.LBL_AFTER_DEF, num5 - equipData.def, expression_include: false);
			int num8 = equipData.elemDef;
			if (equipData.tableData.isFormer)
			{
				num8 = Mathf.FloorToInt((float)num8 * 0.1f);
			}
			SetLabelText(detailBase, UI.LBL_ELEM_DEF, num8.ToString());
			SetActive(detailBase, UI.LBL_AFTER_ELEM_DEF, num7 > num8);
			SetStatusBuffText(detailBase, UI.LBL_AFTER_ELEM_DEF, num7 - num8, expression_include: false);
			int num9 = growEquipItemData.GetGrowParamHp(equipData.tableData.baseHp) + (int)equipItemExceedParamAll.hp;
			SetLabelText(detailBase, UI.LBL_HP, equipData.hp.ToString());
			SetActive(detailBase, UI.LBL_AFTER_HP, num9 > equipData.hp);
			SetStatusBuffText(detailBase, UI.LBL_AFTER_HP, num9 - equipData.hp, expression_include: false);
		}
		else
		{
			int atk = equipData.atk;
			int elemAtk = equipData.elemAtk;
			SetElementSprite(detailBase, UI.SPR_ELEM, equipData.GetElemAtkType());
			SetLabelText(detailBase, UI.LBL_ATK, atk.ToString());
			SetLabelText(detailBase, UI.LBL_ELEM, elemAtk.ToString());
			SetActive(detailBase, UI.LBL_AFTER_ATK, is_visible: false);
			SetActive(detailBase, UI.LBL_AFTER_ELEM, is_visible: false);
			int def = equipData.def;
			int elemDef = equipData.elemDef;
			SetDefElementSprite(detailBase, UI.SPR_ELEM_DEF, equipData.GetElemDefType());
			SetLabelText(detailBase, UI.LBL_DEF, def.ToString());
			SetLabelText(detailBase, UI.LBL_ELEM_DEF, elemDef.ToString());
			SetActive(detailBase, UI.LBL_AFTER_DEF, is_visible: false);
			SetActive(detailBase, UI.LBL_AFTER_ELEM_DEF, is_visible: false);
			SetLabelText(text: equipData.hp.ToString(), root: detailBase, label_enum: UI.LBL_HP);
			SetActive(detailBase, UI.LBL_AFTER_HP, is_visible: false);
		}
	}

	protected override void EquipImg()
	{
		uint id = GetEquipTableData().id;
		if (modelID != id)
		{
			modelID = id;
			SetRenderEquipModel(UI.TEX_MODEL, id);
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
		if (MonoBehaviourSingleton<UserInfoManager>.I.userStatus.IsTutorialBitReady && !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_ITEM))
		{
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL2) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.SHADOW_QUEST_WIN))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL2, delegate
				{
				});
			}
			else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL3) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL2))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL3, delegate
				{
				});
			}
			else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL4) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL3))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL4, delegate
				{
				});
			}
			else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL5) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL4))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL5, delegate
				{
				});
			}
			else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL6) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL5))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL6, delegate
				{
				});
			}
			else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL7) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL6))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL7, delegate
				{
				});
			}
			else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL8) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL7))
			{
				TutorialMessageTable.SendTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_LEVEL8, delegate
				{
				});
			}
		}
		EquipItemInfo equipData = GetEquipData();
		if (aimLv == equipData.tableData.maxLv)
		{
			return;
		}
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

	private IEnumerator UPGRADE_TOTURIAL_EVENT()
	{
		while (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_ITEM))
		{
			DispatchEvent("UPGRADE_TOTURIAL");
			yield return null;
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
			return;
		}
		EquipItemInfo selectEquipData = smithData.selectEquipData;
		if (selectEquipData == null)
		{
			GameSection.StopEvent();
			return;
		}
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
				GameSection.ResumeEvent(is_resume: true);
			}
			else
			{
				isNotifySelfUpdate = false;
				GameSection.ResumeEvent(is_resume: false);
			}
		});
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
		if (!MonoBehaviourSingleton<GameSceneManager>.I.ExistHistory("SmithGrowItemSelect"))
		{
			GameSection.StopEvent();
			TO_UNIQUE_OR_MAIN_STATUS();
		}
	}
}
