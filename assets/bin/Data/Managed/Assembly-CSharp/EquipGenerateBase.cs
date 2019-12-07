using Network;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipGenerateBase : EquipMaterialBase
{
	protected new enum UI
	{
		BTN_DECISION,
		BTN_INACTIVE,
		LBL_NEXT_BTN,
		LBL_TO_SELECT,
		BTN_TO_SELECT,
		BTN_TO_SELECT_CENTER,
		OBJ_ADD_ABILITY,
		LBL_ADD_ABILITY,
		TEX_MODEL,
		TEX_DETAIL_BASE_MODEL,
		OBJ_DETAIL_ROOT,
		OBJ_DETAIL_BASE_ROOT,
		OBJ_ITEM_INFO_ROOT,
		OBJ_AIM_GROW,
		BTN_AIM_L,
		BTN_AIM_R,
		BTN_AIM_L_INACTIVE,
		BTN_AIM_R_INACTIVE,
		SPR_AIM_L,
		SPR_AIM_R,
		LBL_AIM_LV,
		OBJ_EVOLVE_ROOT,
		LBL_EVO_INDEX,
		LBL_EVO_INDEX_MAX,
		BTN_EVO_L,
		BTN_EVO_R,
		BTN_EVO_L_INACTIVE,
		BTN_EVO_R_INACTIVE,
		SPR_EVO_L,
		SPR_EVO_R,
		BTN_EVO_R2,
		BTN_EVO_L2,
		BTN_EVO_L2_INACTIVE,
		BTN_EVO_R2_INACTIVE,
		SPR_EVO_R2,
		SPR_EVO_L2,
		OBJ_ORDER_L2,
		OBJ_ORDER_R2,
		OBJ_ORDER_NORMAL_CENTER,
		OBJ_ORDER_ATTRIBUTE_CENTER,
		SPR_ORDER_ELEM_CENTER,
		OBJ_ORDER_NORMAL_R,
		OBJ_ORDER_ATTRIBUTE_R,
		SPR_ORDER_ELEM_R,
		OBJ_ORDER_NORMAL_L,
		OBJ_ORDER_ATTRIBUTE_L,
		SPR_ORDER_ELEM_L,
		OBJ_ORDER_CENTER_ANIM_ROOT,
		OBJ_ORDER_L_ANIM_ROOT,
		OBJ_ORDER_R_ANIM_ROOT,
		STR_INACTIVE,
		STR_INACTIVE_REFLECT,
		STR_DECISION,
		STR_DECISION_REFLECT,
		STR_TITLE_MATERIAL,
		STR_TITLE_MONEY,
		STR_TITLE_ATK,
		STR_TITLE_ELEM,
		STR_TITLE_DEF,
		STR_TITLE_ELEM_DEF,
		STR_TITLE_HP,
		LBL_NAME,
		LBL_LV_NOW,
		LBL_LV_MAX,
		LBL_ATK,
		LBL_DEF,
		LBL_HP,
		LBL_ELEM,
		LBL_ELEM_DEF,
		SPR_ELEM,
		SPR_ELEM_DEF,
		LBL_SELL,
		OBJ_SKILL_BUTTON_ROOT,
		BTN_SELL,
		BTN_GROW,
		OBJ_FAVORITE_ROOT,
		SPR_FAVORITE,
		SPR_UNFAVORITE,
		SPR_IS_EVOLVE,
		TWN_FAVORITE,
		TWN_UNFAVORITE,
		OBJ_ATK_ROOT,
		OBJ_DEF_ROOT,
		OBJ_ELEM_ROOT,
		SPR_TYPE_ICON,
		SPR_TYPE_ICON_BG,
		SPR_TYPE_ICON_RARITY,
		STR_TITLE_ITEM_INFO,
		STR_TITLE_STATUS,
		STR_TITLE_SKILL_SLOT,
		STR_TITLE_ABILITY,
		STR_TITLE_SELL,
		STR_TITLE_ELEMENT,
		TBL_ABILITY,
		STR_NON_ABILITY,
		LBL_ABILITY,
		LBL_ABILITY_NUM,
		BTN_EXCEED,
		SPR_COUNT_0_ON,
		SPR_COUNT_1_ON,
		SPR_COUNT_2_ON,
		SPR_COUNT_3_ON,
		STR_ONLY_EXCEED,
		LBL_AFTER_ATK,
		LBL_AFTER_DEF,
		LBL_AFTER_HP,
		LBL_AFTER_ELEM,
		LBL_AFTER_ELEM_DEF,
		GRD_NEED_MATERIAL,
		LBL_GOLD,
		LBL_CAPTION,
		BTN_GRAPH,
		BTN_LIST,
		SPR_SP_ATTACK_TYPE,
		SPR_ORDER_ACTIONTYPE_CENTER,
		SPR_ORDER_ACTIONTYPE_LEFT,
		SPR_ORDER_ACTIONTYPE_RIGHT,
		BTN_SHADOW_EVOLVE,
		OBJ_ABILITY,
		OBJ_FIXEDABILITY,
		LBL_FIXEDABILITY,
		LBL_FIXEDABILITY_NUM,
		OBJ_ABILITY_ITEM,
		LBL_ABILITY_ITEM,
		OBJ_WEAPON_ROOT,
		OBJ_ARMOR_ROOT,
		LinePartsR01
	}

	protected SkillItemTable.SkillItemData[] skillDataTable;

	protected AbilityDetailPopUp abilityDetailPopUp;

	protected List<Transform> touchAndReleaseButtons = new List<Transform>();

	public override void Initialize()
	{
		base.Initialize();
	}

	protected override string GetEquipItemName()
	{
		return GetEquipTableData().name;
	}

	protected override void EquipTableParam()
	{
		int exceed = 0;
		EquipItemInfo equipData = GetEquipData();
		if (equipData != null)
		{
			exceed = equipData.exceed;
		}
		EquipItemTable.EquipItemData table_data = GetEquipTableData();
		if (table_data == null)
		{
			return;
		}
		EquipItemExceedParamTable.EquipItemExceedParamAll equipItemExceedParamAll = table_data.GetExceedParam((uint)exceed);
		if (equipItemExceedParamAll == null)
		{
			equipItemExceedParamAll = new EquipItemExceedParamTable.EquipItemExceedParamAll();
		}
		SetLabelText(UI.LBL_NAME, table_data.name);
		SetLabelText(UI.LBL_LV_NOW, "1");
		SetLabelText(UI.LBL_LV_MAX, table_data.maxLv.ToString());
		int num = (int)table_data.baseAtk + (int)equipItemExceedParamAll.atk;
		int elemAtk = equipItemExceedParamAll.GetElemAtk(table_data.atkElement);
		SetElementSprite(UI.SPR_ELEM, equipItemExceedParamAll.GetElemAtkType(table_data.atkElement));
		SetLabelText(UI.LBL_ATK, num.ToString());
		SetLabelText(UI.LBL_ELEM, elemAtk.ToString());
		SetLabelText(text: ((int)table_data.baseDef + (int)equipItemExceedParamAll.def).ToString(), label_enum: UI.LBL_DEF);
		int elemDef = equipItemExceedParamAll.GetElemDef(table_data.defElement);
		SetDefElementSprite(UI.SPR_ELEM_DEF, equipItemExceedParamAll.GetElemDefType(table_data.defElement));
		SetLabelText(UI.LBL_ELEM_DEF, elemDef.ToString());
		SetLabelText(text: ((int)table_data.baseHp + (int)equipItemExceedParamAll.hp).ToString(), label_enum: UI.LBL_HP);
		SetActive(UI.SPR_IS_EVOLVE, table_data.IsEvolve());
		SetEquipmentTypeIcon(UI.SPR_TYPE_ICON, UI.SPR_TYPE_ICON_BG, UI.SPR_TYPE_ICON_RARITY, table_data);
		SetLabelText(UI.LBL_SELL, table_data.sale.ToString());
		if (smithType != SmithType.EVOLVE)
		{
			SetSkillIconButton(UI.OBJ_SKILL_BUTTON_ROOT, "SkillIconButton", table_data, GetSkillSlotData(table_data, 0), null);
			if (table_data.fixedAbility.Length != 0)
			{
				string allAbilityName = "";
				string allAp = "";
				string allAbilityDesc = "";
				SetTable(UI.TBL_ABILITY, "ItemDetailEquipAbilityItem", table_data.fixedAbility.Length, reset: false, delegate(int i, Transform t, bool is_recycle)
				{
					EquipItemAbility equipItemAbility = new EquipItemAbility((uint)table_data.fixedAbility[i].id, table_data.fixedAbility[i].pt);
					SetActive(t, is_visible: true);
					SetActive(t, UI.OBJ_FIXEDABILITY, is_visible: true);
					SetActive(t, UI.OBJ_ABILITY, is_visible: false);
					SetLabelText(t, UI.LBL_FIXEDABILITY, equipItemAbility.GetName());
					SetLabelText(t, UI.LBL_FIXEDABILITY_NUM, equipItemAbility.GetAP());
					SetAbilityItemEvent(t, i, touchAndReleaseButtons);
					allAbilityName += equipItemAbility.GetName();
					allAp += equipItemAbility.GetAP();
					allAbilityDesc += equipItemAbility.GetDescription();
				});
				SetActive(UI.STR_NON_ABILITY, is_visible: false);
				PreCacheAbilityDetail(allAbilityName, allAp, allAbilityDesc);
			}
			else
			{
				SetActive(UI.STR_NON_ABILITY, is_visible: true);
			}
		}
	}

	protected override void OnQuery_SKILL_ICON_BUTTON()
	{
		GameSection.SetEventData(new object[2]
		{
			ItemDetailEquip.CURRENT_SECTION.SMITH_CREATE,
			GetEquipTableData()
		});
	}

	protected override void OnQuery_START()
	{
		SmithManager.ERR_SMITH_SEND eRR_SMITH_SEND = MonoBehaviourSingleton<SmithManager>.I.CheckCreateEquipItem(GetCreateEquiptableID());
		if (eRR_SMITH_SEND != 0)
		{
			GameSection.ChangeEvent(eRR_SMITH_SEND.ToString());
			return;
		}
		isDialogEventYES = false;
		GameSection.SetEventData(new object[1]
		{
			GetEquipItemName()
		});
	}

	protected override void Send()
	{
		SmithManager.ResultData result_data = new SmithManager.ResultData();
		GameSection.SetEventData(result_data);
		GameSection.StayEvent();
		MonoBehaviourSingleton<SmithManager>.I.SendCreateEquipItem(GetCreateEquiptableID(), delegate(Error err, EquipItemInfo create_item)
		{
			switch (err)
			{
			case Error.None:
				result_data.itemData = create_item;
				MonoBehaviourSingleton<UIAnnounceBand>.I.isWait = true;
				GameSection.ResumeEvent(is_resume: true);
				break;
			case Error.WRN_SMITH_OVER_EQUIP_ITEM_NUM:
				GameSection.ChangeStayEvent("CREATE_OVER_EQUIP");
				GameSection.ResumeEvent(is_resume: true);
				break;
			default:
				GameSection.ResumeEvent(is_resume: false);
				break;
			}
		});
	}

	public void OnQuery_SmithCreateOverEquipItem_GO_ITEM_STORAGE()
	{
		string name = "TAB_" + 2;
		EventData[] autoEvents = new EventData[3]
		{
			new EventData("SECTION_BACK", null),
			new EventData("SELL", null),
			new EventData(name, null)
		};
		GameSection.StopEvent();
		MonoBehaviourSingleton<GameSceneManager>.I.SetAutoEvents(autoEvents);
	}

	public void OnQuery_SmithCreateOverEquipItem_EXPAND_STORAGE()
	{
		DispatchEvent("EXPAND_STORAGE");
	}

	protected virtual void OnQuery_ABILITY_DATA_POPUP()
	{
		object[] obj = GameSection.GetEventData() as object[];
		int num = (int)obj[0];
		EquipItem.Ability ability = GetEquipTableData().fixedAbility[num];
		Transform targetTrans = obj[1] as Transform;
		EquipItemAbility abilityDetailText = new EquipItemAbility((uint)ability.id, ability.pt);
		if (abilityDetailPopUp == null)
		{
			abilityDetailPopUp = CreateAndGetAbilityDetail(UI.OBJ_DETAIL_ROOT);
		}
		abilityDetailPopUp.ShowAbilityDetail(targetTrans);
		abilityDetailPopUp.SetAbilityDetailText(abilityDetailText);
		GameSection.StopEvent();
	}

	protected void OnQuery_RELEASE_ABILITY()
	{
		if (!(abilityDetailPopUp == null))
		{
			abilityDetailPopUp.Hide();
			GameSection.StopEvent();
		}
	}

	public override void OnNotify(NOTIFY_FLAG flags)
	{
		base.OnNotify(flags);
		if ((flags & NOTIFY_FLAG.PRETREAT_SCENE) != (NOTIFY_FLAG)0L)
		{
			NoEventReleaseTouchAndReleases(touchAndReleaseButtons);
			OnQuery_RELEASE_ABILITY();
		}
	}

	private void PreCacheAbilityDetail(string name, string ap, string desc)
	{
		if (abilityDetailPopUp == null)
		{
			abilityDetailPopUp = CreateAndGetAbilityDetail(UI.OBJ_DETAIL_ROOT);
		}
		abilityDetailPopUp.PreCacheAbilityDetail(name, ap, desc);
	}

	protected virtual uint GetCreateEquiptableID()
	{
		return 0u;
	}
}
