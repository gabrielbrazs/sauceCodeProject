using System;
using System.Collections.Generic;
using UnityEngine;

public class SmithGrowSkill : ItemDetailSkill
{
	protected new enum UI
	{
		OBJ_DETAIL_ROOT,
		TEX_MODEL,
		TEX_INNER_MODEL,
		LBL_NAME,
		LBL_LV_NOW,
		LBL_LV_MAX,
		OBJ_LV_EX,
		LBL_LV_EX,
		LBL_ATK,
		LBL_DEF,
		LBL_HP,
		LBL_SELL,
		LBL_DESCRIPTION,
		OBJ_FAVORITE_ROOT,
		TWN_FAVORITE,
		TWN_UNFAVORITE,
		OBJ_SUB_STATUS,
		SPR_SKILL_TYPE_ICON,
		SPR_SKILL_TYPE_ICON_BG,
		SPR_SKILL_TYPE_ICON_RARITY,
		STR_TITLE_ITEM_INFO,
		STR_TITLE_DESCRIPTION,
		STR_TITLE_STATUS,
		STR_TITLE_SELL,
		PRG_EXP_BAR,
		OBJ_NEXT_EXP_ROOT,
		STR_NON_MATERIAL,
		LBL_EQUIP_ITEM_NAME,
		SCR_INVENTORY,
		GRD_INVENTORY,
		GRD_INVENTORY_SMALL,
		TGL_CHANGE_INVENTORY,
		LBL_SORT,
		BTN_BACK,
		LBL_GOLD,
		LBL_SELECT_NUM,
		STR_SELL,
		STR_TITLE_MATERIAL,
		STR_TITLE_MONEY
	}

	public const int MATERIAL_SELECT_MAX = 10;

	private SkillItemInfo skillItem;

	protected ItemStorageTop.SkillItemInventory inventory;

	private List<SkillItemInfo> materialSkillItem;

	private int needGold;

	private Color goldColor = Color.white;

	protected UI inventoryUI;

	protected UI[] switchInventoryAry = new UI[2]
	{
		UI.GRD_INVENTORY,
		UI.GRD_INVENTORY_SMALL
	};

	protected int inventoryUIIndex;

	private bool isNoticeSendGrow;

	private int toggleIndex = -1;

	protected bool IsShowMainStatus => inventoryUIIndex == 0;

	public override void Initialize()
	{
		skillItem = (GameSection.GetEventData() as SkillItemInfo);
		GameSection.SetEventData(new object[2]
		{
			ItemDetailEquip.CURRENT_SECTION.UI_PARTS,
			skillItem
		});
		materialSkillItem = new List<SkillItemInfo>();
		UILabel component = GetComponent<UILabel>(UI.LBL_GOLD);
		if (component != null)
		{
			goldColor = component.color;
		}
		base.Initialize();
	}

	protected override void OnOpen()
	{
		isNoticeSendGrow = false;
		base.OnOpen();
	}

	public override void UpdateUI()
	{
		SetFontStyle(UI.STR_TITLE_MATERIAL, FontStyle.Italic);
		SetFontStyle(UI.STR_TITLE_MONEY, FontStyle.Italic);
		if (detailBase != null)
		{
			SetActive(detailBase, UI.OBJ_FAVORITE_ROOT, is_visible: false);
		}
		if (inventory == null)
		{
			InitInventory();
		}
		bool is_visible = inventory == null || inventory.datas == null || inventory.datas.Length <= 1;
		SetActive(UI.STR_NON_MATERIAL, is_visible);
		UpdateMaterial();
		SetupEnableInventoryUI();
		int base_item_index = Array.FindIndex(inventory.datas, (SortCompareData data) => data.GetUniqID() == skillItem.uniqueID);
		SetDynamicList(inventoryUI, null, inventory.datas.Length, reset: false, (int i) => (i != base_item_index) ? true : false, null, delegate(int i, Transform t, bool is_recycle)
		{
			SkillItemSortData item = inventory.datas[i] as SkillItemSortData;
			int num = materialSkillItem.FindIndex((SkillItemInfo material) => material.uniqueID == item.GetUniqID());
			if (num > -1)
			{
				num++;
			}
			ITEM_ICON_TYPE iconType = item.GetIconType();
			bool is_new = MonoBehaviourSingleton<InventoryManager>.I.IsNewItem(iconType, item.GetUniqID());
			ItemIcon itemIcon = CreateItemIconDetail(iconType, item.skillData.tableData.iconID, item.skillData.tableData.rarity, item, IsShowMainStatus, t, "MATERIAL", i, is_new, 0, num, item.IsEquipping());
			itemIcon.toggleSelectFrame.onChange.Clear();
			itemIcon.toggleSelectFrame.onChange.Add(new EventDelegate(this, "IconToggleChange"));
			SetLongTouch(itemIcon.transform, "DETAIL", i);
		});
	}

	private void InitInventory()
	{
		inventory = new ItemStorageTop.SkillItemInventory(SortSettings.SETTINGS_TYPE.GROW_SKILL_ITEM);
		inventory.sortSettings = SortSettings.CreateMemSortSettings(SortBase.DIALOG_TYPE.STORAGE_SKILL, SortSettings.SETTINGS_TYPE.GROW_SKILL_ITEM);
		sorting();
	}

	private SkillItemInfo ParamCopy(SkillItemInfo _ref, bool is_level_up = false)
	{
		int lv = (!is_level_up) ? _ref.level : (_ref.level + 1);
		SkillItemInfo skillItemInfo = new SkillItemInfo(0, (int)_ref.tableID, lv, _ref.exceedCnt);
		skillItemInfo.uniqueID = _ref.uniqueID;
		skillItemInfo.exp = _ref.exp;
		skillItemInfo.expPrev = MonoBehaviourSingleton<SmithManager>.I.GetGrowResultValue(skillItemInfo.tableData.baseNeedExp, skillItemInfo.growData.needExp);
		skillItemInfo.expNext = MonoBehaviourSingleton<SmithManager>.I.GetGrowResultValue(skillItemInfo.tableData.baseNeedExp, skillItemInfo.nextGrowData.needExp);
		skillItemInfo.growCost = _ref.growCost;
		return skillItemInfo;
	}

	private void UpdateMaterial()
	{
		SetLabelText(UI.LBL_SELECT_NUM, (10 - materialSkillItem.Count).ToString());
		needGold = (int)(skillItem.growCost * (float)materialSkillItem.Count);
		SetLabelText(UI.LBL_GOLD, needGold.ToString("N0"));
		if (MonoBehaviourSingleton<UserInfoManager>.I.userStatus.money < needGold)
		{
			SetColor(UI.LBL_GOLD, Color.red);
		}
		else
		{
			SetColor(UI.LBL_GOLD, goldColor);
		}
		int exp = 0;
		SkillItemInfo data = ParamCopy(skillItem);
		int level = data.level;
		materialSkillItem.ForEach(delegate(SkillItemInfo material)
		{
			if (!data.IsLevelMax() && material.level <= material.GetMaxLevel())
			{
				exp += material.giveExp;
				data.exp += material.giveExp;
				do
				{
					if (data.expNext > data.exp)
					{
						return;
					}
					data = ParamCopy(data, is_level_up: true);
				}
				while (!data.IsLevelMax());
				data.exp = data.expPrev;
			}
		});
		itemData = data;
		base.UpdateUI();
		SetLabelText(detailBase, UI.LBL_LV_NOW, data.level.ToString());
		SetLabelText(detailBase, UI.LBL_LV_MAX, data.GetMaxLevel().ToString());
		SetActive(detailBase, UI.OBJ_LV_EX, data.IsExceeded());
		SetLabelText(detailBase, UI.LBL_LV_EX, data.exceedCnt.ToString());
		SetLabelText(detailBase, UI.LBL_ATK, data.atk.ToString());
		SetLabelText(detailBase, UI.LBL_DEF, data.def.ToString());
		SetLabelText(detailBase, UI.LBL_HP, data.hp.ToString());
		SetLabelText(detailBase, UI.LBL_SELL, needGold.ToString());
		SetLabelText(detailBase, UI.STR_SELL, base.sectionData.GetText("STR_SELL"));
		SkillGrowProgress component = FindCtrl(detailBase, UI.PRG_EXP_BAR).GetComponent<SkillGrowProgress>();
		float fill_amount = (float)(skillItem.exp - skillItem.expPrev) / (float)(skillItem.expNext - skillItem.expPrev);
		component.SetGrowMode();
		component.SetBaseGauge(data.level == level, fill_amount);
		UpdateAnchors();
	}

	protected bool sorting()
	{
		return inventory.sortSettings.Sort(inventory.datas as SkillItemSortData[]);
	}

	private void OnQuery_SORT()
	{
		GameSection.SetEventData(new object[2]
		{
			skillItem,
			inventory.sortSettings.Clone()
		});
	}

	private void OnCloseDialog_SmithSkillGrowSort()
	{
		SortSettings sortSettings = (SortSettings)GameSection.GetEventData();
		if (sortSettings != null && inventory.Sort(sortSettings))
		{
			SetDirty(UI.GRD_INVENTORY);
			SetDirty(UI.GRD_INVENTORY_SMALL);
			RefreshUI();
		}
	}

	private bool IsEnableSelect(SortCompareData item)
	{
		if (item == null)
		{
			return false;
		}
		if (!item.IsFavorite())
		{
			return item.GetUniqID() != skillItem.uniqueID;
		}
		return false;
	}

	private void OnQuery_MATERIAL()
	{
		ResetSelectMaterialIcon();
		int num = (int)GameSection.GetEventData();
		SkillItemSortData item = inventory.datas[num] as SkillItemSortData;
		bool flag = materialSkillItem.Find((SkillItemInfo material) => material.uniqueID == item.GetUniqID()) != null;
		SkillItemInfo item2 = item.GetItemData() as SkillItemInfo;
		if (!IsEnableSelect(inventory.datas[num]))
		{
			toggleIndex = num;
			DispatchEvent("NOT_MATERIAL_FAVORITE");
		}
		else if (flag)
		{
			materialSkillItem.Remove(item2);
		}
		else if (materialSkillItem.Count < 10)
		{
			materialSkillItem.Add(item2);
		}
		else
		{
			toggleIndex = num;
		}
		UpdateMaterial();
		UpdateSelectMaterialIcon();
	}

	private void ResetSelectMaterialIcon()
	{
		_UpdateSelectMaterialIcon(reset: true);
	}

	private void UpdateSelectMaterialIcon()
	{
		_UpdateSelectMaterialIcon(reset: false);
	}

	private void _UpdateSelectMaterialIcon(bool reset)
	{
		Transform grid = GetCtrl(inventoryUI);
		int base_item_index = Array.FindIndex(inventory.datas, (SortCompareData data) => data.GetUniqID() == skillItem.uniqueID);
		int select_index = (!reset) ? 1 : (-1);
		materialSkillItem.ForEach(delegate(SkillItemInfo material)
		{
			bool flag = false;
			int num = -1;
			int i = 0;
			for (int num2 = inventory.datas.Length; i < num2; i++)
			{
				if (i == base_item_index)
				{
					flag = true;
				}
				else if (inventory.datas[i].GetUniqID() == material.uniqueID)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				if (flag)
				{
					num--;
				}
				Transform child = grid.GetChild(num);
				if (inventoryUI == UI.GRD_INVENTORY)
				{
					ItemIconDetail componentInChildren = child.GetComponentInChildren<ItemIconDetail>();
					if (componentInChildren != null)
					{
						componentInChildren.setupperSkill.SetupSelectNumberSprite(select_index);
					}
				}
				else
				{
					ItemIconDetailSmall componentInChildren2 = child.GetComponentInChildren<ItemIconDetailSmall>();
					if (componentInChildren2 != null)
					{
						componentInChildren2.SetupSelectNumberSprite(select_index);
					}
				}
				if (!reset)
				{
					int num3 = ++select_index;
				}
			}
		});
	}

	private void OnQuery_DECISION()
	{
		if (needGold > MonoBehaviourSingleton<UserInfoManager>.I.userStatus.money)
		{
			GameSection.ChangeEvent("NOT_ENOUGH_MONEY");
			return;
		}
		if (materialSkillItem == null || materialSkillItem.Count <= 0)
		{
			GameSection.ChangeEvent("NOT_MATERIAL");
			return;
		}
		if (skillItem.IsLevelMax() && !skillItem.IsExistNextExceed())
		{
			GameSection.ChangeEvent("NOT_INCLUDE_EXCEED");
			return;
		}
		isNoticeSendGrow = true;
		GameSection.SetEventData(new object[2]
		{
			skillItem,
			materialSkillItem.ToArray()
		});
	}

	private void OnCloseDialog_SmithGrowSkillConfirm()
	{
		isNoticeSendGrow = false;
	}

	private void OnQuery_DETAIL()
	{
		int num = (int)GameSection.GetEventData();
		SkillItemSortData skillItemSortData = inventory.datas[num] as SkillItemSortData;
		GameSection.SetEventData(new object[2]
		{
			ItemDetailEquip.CURRENT_SECTION.SMITH_SKILL_GROW,
			skillItemSortData
		});
	}

	public void IconToggleChange()
	{
		if (toggleIndex != -1)
		{
			Transform transform = GetCtrl(UI.GRD_INVENTORY).Find(toggleIndex.ToString());
			if (transform != null)
			{
				transform.GetComponentInChildren<UIToggle>().value = false;
				toggleIndex = -1;
			}
		}
	}

	protected void OnQuery_CHANGE_INVENTORY()
	{
		inventoryUIIndex = ((inventoryUIIndex + 1 < switchInventoryAry.Length) ? (inventoryUIIndex + 1) : 0);
		SetDirty(UI.GRD_INVENTORY);
		SetDirty(UI.GRD_INVENTORY_SMALL);
		RefreshUI();
	}

	protected void SetupEnableInventoryUI()
	{
		int i = 0;
		for (int num = switchInventoryAry.Length; i < num; i++)
		{
			SetActive(switchInventoryAry[i], is_visible: false);
		}
		SetActive(switchInventoryAry[inventoryUIIndex], is_visible: true);
		inventoryUI = switchInventoryAry[inventoryUIIndex];
		SetToggle(UI.TGL_CHANGE_INVENTORY, inventoryUI == UI.GRD_INVENTORY);
	}

	protected ItemIcon CreateItemIconDetail(ITEM_ICON_TYPE icon_type, int icon_id, RARITY_TYPE? rarity, SkillItemSortData item_data, bool is_show_main_status, Transform parent = null, string event_name = null, int event_data = 0, bool is_new = false, int toggle_group = -1, int select_number = -1, bool is_equipping = false)
	{
		if (inventoryUI == UI.GRD_INVENTORY)
		{
			return ItemIconDetail.CreateSkillDetailSelectNumberIcon(icon_type, icon_id, rarity, item_data, is_show_main_status, parent, event_name, event_data, is_new, toggle_group, select_number, is_equipping);
		}
		return ItemIconDetailSmall.CreateSmallSkillSelectDetailIcon(icon_type, icon_id, rarity, item_data, parent, event_name, event_data, is_new, toggle_group, select_number, is_equipping);
	}

	public override void OnNotify(NOTIFY_FLAG flags)
	{
		if ((flags & (NOTIFY_FLAG.UPDATE_SKILL_FAVORITE | NOTIFY_FLAG.UPDATE_SKILL_INVENTORY)) != (NOTIFY_FLAG)0L)
		{
			skillItem = MonoBehaviourSingleton<InventoryManager>.I.skillItemInventory.Find(skillItem.uniqueID);
			List<SkillItemInfo> del_list = new List<SkillItemInfo>();
			materialSkillItem.ForEach(delegate(SkillItemInfo skill)
			{
				SkillItemInfo skillItemInfo = MonoBehaviourSingleton<InventoryManager>.I.skillItemInventory.Find(skill.uniqueID);
				if (skillItemInfo == null || skillItemInfo.isFavorite)
				{
					del_list.Add(skill);
				}
			});
			del_list.ForEach(delegate(SkillItemInfo delitem)
			{
				materialSkillItem.Remove(delitem);
			});
			inventory = null;
			SetDirty(inventoryUI);
		}
		base.OnNotify(flags);
	}

	protected override NOTIFY_FLAG GetUpdateUINotifyFlags()
	{
		if (isNoticeSendGrow)
		{
			return (NOTIFY_FLAG)0L;
		}
		return NOTIFY_FLAG.UPDATE_SKILL_FAVORITE | NOTIFY_FLAG.UPDATE_SKILL_INVENTORY;
	}
}
