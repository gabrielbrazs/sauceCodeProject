using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildDonateInvitationList : GameSection
{
	private enum UI
	{
		SCR_QUEST,
		TBL_QUEST,
		STR_NON_LIST,
		LBL_USER_NAME,
		LBL_CHAT_MESSAGE,
		LBL_MATERIAL_NAME,
		SLD_PROGRESS,
		OBJ_MATERIAL_ICON,
		LBL_QUATITY,
		OBJ_FULL,
		OBJ_NORMAL,
		LBL_DONATE_NUM,
		LBL_DONATE_MAX,
		BTN_GIFT
	}

	private List<DonateInvitationInfo> _donateList;

	public override void Initialize()
	{
		StartCoroutine(DoInitialize());
	}

	private IEnumerator DoInitialize()
	{
		bool finish_donate_list = false;
		MonoBehaviourSingleton<GuildManager>.I.SendDonateInvitationList(delegate
		{
			finish_donate_list = true;
			_donateList = MonoBehaviourSingleton<GuildManager>.I.donateInviteList;
		});
		while (!finish_donate_list)
		{
			yield return null;
		}
		base.Initialize();
	}

	public override void UpdateUI()
	{
		SetActive(UI.STR_NON_LIST, _donateList.Count <= 0);
		SetTable(UI.TBL_QUEST, "GuildDonateInvitationListItem", _donateList.Count, reset: true, (int i, Transform t) => null, delegate(int i, Transform t, bool b)
		{
			GuildDonateInvitationList guildDonateInvitationList = this;
			DonateInvitationInfo info = _donateList[i];
			int itemNum = MonoBehaviourSingleton<InventoryManager>.I.GetItemNum((ItemInfo x) => x.tableData.id == info.itemId, 1);
			bool flag = info.itemNum >= info.quantity;
			SetActive(t, UI.OBJ_FULL, flag);
			SetActive(t, UI.OBJ_NORMAL, !flag);
			SetLabelText(UI.LBL_CHAT_MESSAGE, info.msg);
			SetLabelText(UI.LBL_USER_NAME, info.nickName);
			SetLabelText(UI.LBL_MATERIAL_NAME, info.itemName);
			SetLabelText(UI.LBL_QUATITY, itemNum);
			SetLabelText(UI.LBL_DONATE_NUM, info.itemNum);
			SetLabelText(UI.LBL_DONATE_MAX, info.quantity);
			SetSliderValue(UI.SLD_PROGRESS, (float)info.itemNum / (float)info.quantity);
			if (!flag && itemNum > 0 && info.itemNum < info.quantity)
			{
				SetButtonEvent(t, UI.BTN_GIFT, new EventDelegate(delegate
				{
					guildDonateInvitationList.DispatchEvent("SEND", info.ParseDonateInfo());
				}));
			}
			else
			{
				SetButtonEnabled(t, UI.BTN_GIFT, is_enabled: false);
			}
			Transform ctrl = GetCtrl(UI.OBJ_MATERIAL_ICON);
			ItemInfo item = ItemInfo.CreateItemInfo(new Item
			{
				uniqId = "0",
				itemId = info.itemId,
				num = info.itemNum
			});
			ItemSortData itemSortData = new ItemSortData();
			itemSortData.SetItem(item);
			SetItemIcon(ctrl, itemSortData, ctrl);
		});
	}

	private void OnQuery_RELOAD()
	{
		GameSection.StayEvent();
		MonoBehaviourSingleton<GuildManager>.I.SendDonateInvitationList(delegate
		{
			_donateList = MonoBehaviourSingleton<GuildManager>.I.donateInviteList;
			GameSection.ResumeEvent(is_resume: false);
			RefreshUI();
		});
	}

	private void SetItemIcon(Transform holder, ItemSortData data, Transform parent_scroll)
	{
		ITEM_ICON_TYPE iTEM_ICON_TYPE = ITEM_ICON_TYPE.NONE;
		RARITY_TYPE? rarity = null;
		ELEMENT_TYPE element = ELEMENT_TYPE.MAX;
		EQUIPMENT_TYPE? magi_enable_icon_type = null;
		int icon_id = -1;
		if (data != null)
		{
			iTEM_ICON_TYPE = data.GetIconType();
			icon_id = data.GetIconID();
			rarity = data.GetRarity();
			element = data.GetIconElement();
			magi_enable_icon_type = data.GetIconMagiEnableType();
			data.GetNum();
			_ = 1;
		}
		bool is_new = false;
		switch (iTEM_ICON_TYPE)
		{
		case ITEM_ICON_TYPE.ITEM:
		case ITEM_ICON_TYPE.QUEST_ITEM:
			if (data.GetUniqID() != 0L)
			{
				is_new = MonoBehaviourSingleton<InventoryManager>.I.IsNewItem(iTEM_ICON_TYPE, data.GetUniqID());
			}
			break;
		default:
			is_new = true;
			break;
		case ITEM_ICON_TYPE.NONE:
			break;
		}
		int enemy_icon_id = 0;
		if (iTEM_ICON_TYPE == ITEM_ICON_TYPE.ITEM)
		{
			enemy_icon_id = Singleton<ItemTable>.I.GetItemData(data.GetTableID()).enemyIconID;
		}
		ItemIcon itemIcon = null;
		itemIcon = ((data.GetIconType() != ITEM_ICON_TYPE.QUEST_ITEM) ? ItemIcon.Create(iTEM_ICON_TYPE, icon_id, rarity, holder, element, magi_enable_icon_type, -1, "DROP", 0, is_new, -1, is_select: false, null, is_equipping: false, enemy_icon_id) : ItemIcon.Create(new ItemIcon.ItemIconCreateParam
		{
			icon_type = data.GetIconType(),
			icon_id = data.GetIconID(),
			rarity = data.GetRarity(),
			parent = holder,
			element = data.GetIconElement(),
			magi_enable_equip_type = data.GetIconMagiEnableType(),
			num = data.GetNum(),
			enemy_icon_id = enemy_icon_id,
			questIconSizeType = ItemIcon.QUEST_ICON_SIZE_TYPE.REWARD_DELIVERY_LIST
		}));
		SetMaterialInfo(itemIcon.transform, data.GetMaterialType(), data.GetTableID(), parent_scroll);
	}
}
