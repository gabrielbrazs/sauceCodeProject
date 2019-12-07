using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HomePointShop : GameSection
{
	private enum VIEW_TYPE
	{
		NORMAL,
		EVENT_LIST
	}

	private enum UI
	{
		OBJ_TAB_ROOT,
		OBJ_ON_TAB_EVENT,
		OBJ_ON_TAB_NORMAL,
		TEX_NPCMODEL,
		LBL_NPC_MESSAGE,
		GRD_NORMAL,
		GRD_EVENT_LIST,
		LBL_NORMAL_POINT,
		TEX_NORMAL_POINT_ICON,
		LBL_HAVE,
		LBL_FILTER,
		LBL_EVENT_LIST_POINT,
		LBL_EVENT_LIST_POINT_TITLE,
		TEX_EVENT_LIST_BANNER,
		LBL_EVENT_LIST_SOLD_OUT,
		LBL_EVENT_LIST_REMAINING_TIME,
		TXT_EVENT_LIST_POINT_ICON,
		OBJ_NORMAL,
		OBJ_EVENT_LIST,
		BTN_EVENT,
		OBJ_EVENT_NON_ACTIVE,
		OBJ_NPC,
		LBL_ARROW_NOW,
		LBL_ARROW_MAX
	}

	private List<PointShop> pointShop = new List<PointShop>();

	private UIModelRenderTexture modelTexture;

	private List<PointShopItem> currentPointShopItem = new List<PointShopItem>();

	protected int currentPage;

	protected int maxPage;

	private PointShopFilterBase.Filter filter;

	private VIEW_TYPE currentType;

	public override void Initialize()
	{
		StartCoroutine(DoInitialize());
	}

	public IEnumerator DoInitialize()
	{
		currentType = VIEW_TYPE.NORMAL;
		currentPage = 1;
		bool hasList = false;
		LoadingQueue loadingQueue = new LoadingQueue(this);
		loadingQueue.Load(RESOURCE_CATEGORY.UI, "PointShopListItem");
		MonoBehaviourSingleton<UserInfoManager>.I.PointShopManager.SendGetPointShops(delegate(bool isSuccess, List<PointShop> resultList)
		{
			if (isSuccess)
			{
				pointShop = resultList;
				foreach (PointShop item in pointShop)
				{
					loadingQueue.Load(RESOURCE_CATEGORY.COMMON, ResourceName.GetPointIconImageName(item.pointShopId));
				}
				hasList = true;
			}
		});
		while (!hasList)
		{
			yield return null;
		}
		if (loadingQueue.IsLoading())
		{
			yield return loadingQueue.Wait();
		}
		base.Initialize();
	}

	public override void UpdateUI()
	{
		UpdateNPC();
		UpdateTab();
	}

	protected void UpdateNPC()
	{
		string empty = string.Empty;
		NPCMessageTable.Section section = Singleton<NPCMessageTable>.I.GetSection(base.sectionData.sectionName + "_TEXT");
		if (section != null)
		{
			NPCMessageTable.Message message = section.GetNPCMessage();
			if (message != null)
			{
				empty = message.message;
				SetRenderNPCModel(UI.TEX_NPCMODEL, message.npc, message.pos, message.rot, MonoBehaviourSingleton<OutGameSettingsManager>.I.homeScene.questCenterNPCFOV, delegate(NPCLoader loader)
				{
					loader.GetAnimator().Play(message.animationStateName);
				});
				SetLabelText(UI.LBL_NPC_MESSAGE, empty);
			}
		}
	}

	private void UpdateTab()
	{
		VIEW_TYPE vIEW_TYPE = currentType;
		if (vIEW_TYPE == VIEW_TYPE.NORMAL || vIEW_TYPE != VIEW_TYPE.EVENT_LIST)
		{
			ViewNormalTab();
		}
		else
		{
			ViewEventTab();
		}
	}

	private void ViewNormalTab()
	{
		SetActive(UI.OBJ_NORMAL, is_visible: true);
		SetActive(UI.OBJ_TAB_ROOT, is_visible: true);
		SetActive(UI.OBJ_ON_TAB_NORMAL, is_visible: true);
		SetActive(UI.OBJ_ON_TAB_EVENT, is_visible: false);
		SetActive(UI.OBJ_EVENT_LIST, is_visible: false);
		PointShop shop = pointShop.First((PointShop x) => !x.isEvent);
		currentPointShopItem = GetBuyableItemList();
		if (filter != null)
		{
			filter.DoFiltering(ref currentPointShopItem);
		}
		SetLabelText(UI.LBL_NORMAL_POINT, string.Format(StringTable.Get(STRING_CATEGORY.POINT_SHOP, 2u), shop.userPoint));
		ResourceLoad.LoadPointIconImageTexture(GetCtrl(UI.TEX_NORMAL_POINT_ICON).GetComponent<UITexture>(), (uint)shop.pointShopId);
		maxPage = currentPointShopItem.Count / GameDefine.POINT_SHOP_LIST_COUNT;
		if (currentPointShopItem.Count % GameDefine.POINT_SHOP_LIST_COUNT > 0)
		{
			maxPage++;
		}
		SetLabelText(UI.LBL_ARROW_NOW, (maxPage > 0) ? currentPage.ToString() : "0");
		SetLabelText(UI.LBL_ARROW_MAX, maxPage.ToString());
		int item_num = Mathf.Min(GameDefine.POINT_SHOP_LIST_COUNT, currentPointShopItem.Count - (currentPage - 1) * GameDefine.POINT_SHOP_LIST_COUNT);
		SetGrid(UI.GRD_NORMAL, "PointShopListItem", item_num, reset: true, delegate(int i, Transform t, bool b)
		{
			int index = i + (currentPage - 1) * GameDefine.POINT_SHOP_LIST_COUNT;
			PointShopItem pointShopItem = currentPointShopItem[index];
			object event_data = new object[3]
			{
				pointShopItem,
				shop,
				new Action<PointShopItem, int>(OnBuy)
			};
			SetEvent(t, "CONFIRM_BUY", event_data);
			t.GetComponent<PointShopItemList>().SetUp(pointShopItem, (uint)shop.pointShopId, pointShopItem.needPoint <= shop.userPoint);
			int num = -1;
			REWARD_TYPE type = (REWARD_TYPE)pointShopItem.type;
			if (type == REWARD_TYPE.ITEM)
			{
				num = MonoBehaviourSingleton<InventoryManager>.I.GetHaveingItemNum((uint)pointShopItem.itemId);
			}
			SetLabelText(t, UI.LBL_HAVE, string.Format(StringTable.Get(STRING_CATEGORY.POINT_SHOP, 6u), num.ToString()));
			SetActive(t, UI.LBL_HAVE, num >= 0);
		});
		bool flag = pointShop.Any((PointShop x) => x.isEvent);
		SetActive(UI.OBJ_EVENT_NON_ACTIVE, !flag);
		SetActive(UI.BTN_EVENT, flag);
	}

	private void ViewEventTab()
	{
		SetActive(UI.OBJ_NORMAL, is_visible: false);
		SetActive(UI.OBJ_TAB_ROOT, is_visible: true);
		SetActive(UI.OBJ_ON_TAB_NORMAL, is_visible: false);
		SetActive(UI.OBJ_ON_TAB_EVENT, is_visible: true);
		SetActive(UI.OBJ_EVENT_LIST, is_visible: true);
		List<PointShop> current = pointShop.Where((PointShop x) => x.isEvent).ToList();
		SetGrid(UI.GRD_EVENT_LIST, "PointShopEventList", current.Count, reset: true, delegate(int i, Transform t, bool b)
		{
			PointShop pointShop = current[i];
			UITexture component = FindCtrl(t, UI.TEX_EVENT_LIST_BANNER).GetComponent<UITexture>();
			UITexture component2 = FindCtrl(t, UI.TXT_EVENT_LIST_POINT_ICON).GetComponent<UITexture>();
			SetLabelText(t, UI.LBL_EVENT_LIST_POINT, string.Format(StringTable.Get(STRING_CATEGORY.POINT_SHOP, 2u), pointShop.userPoint));
			ResourceLoad.LoadPointIconImageTexture(component2, (uint)pointShop.pointShopId);
			ResourceLoad.LoadPointShopBannerTexture(component, (uint)pointShop.pointShopId);
			SetEvent(FindCtrl(t, UI.TEX_EVENT_LIST_BANNER), "EVENT_SHOP", pointShop);
			bool flag = (from x in pointShop.items
				where x.isBuyable
				where x.type != 8 || !MonoBehaviourSingleton<UserInfoManager>.I.IsUnlockedStamp(x.itemId)
				where x.type != 9 || !MonoBehaviourSingleton<UserInfoManager>.I.IsUnlockedDegree(x.itemId)
				where x.type != 7 || !MonoBehaviourSingleton<GlobalSettingsManager>.I.IsUnlockedAvatar(x.itemId)
				select x).Count() == 0;
			SetActive(t, UI.LBL_EVENT_LIST_SOLD_OUT, flag);
			SetButtonEnabled(t, UI.TEX_EVENT_LIST_BANNER, !flag);
			SetLabelText(t, UI.LBL_EVENT_LIST_REMAINING_TIME, pointShop.expire);
		});
	}

	private void OnQuery_CONFIRM_BUY()
	{
		object[] obj = GameSection.GetEventData() as object[];
		PointShopItem pointShopItem = obj[0] as PointShopItem;
		if ((obj[1] as PointShop).userPoint < pointShopItem.needPoint)
		{
			GameSection.ChangeEvent("SHORTAGE_POINT");
		}
	}

	private void OnQuery_ON_EVENT()
	{
		currentType = VIEW_TYPE.EVENT_LIST;
		UpdateTab();
	}

	private void OnQuery_ON_NORMAL()
	{
		currentType = VIEW_TYPE.NORMAL;
		UpdateTab();
	}

	private void OnQuery_HOW_TO()
	{
		GameSection.SetEventData(WebViewManager.PointShop);
	}

	private void OnBuy(PointShopItem item, int num)
	{
		GameSection.SetEventData(PointShopManager.GetBoughtMessage(item, num));
		GameSection.StayEvent();
		PointShop pointShop = this.pointShop.First((PointShop x) => x.items.Contains(item));
		MonoBehaviourSingleton<UserInfoManager>.I.PointShopManager.SendPointShopBuy(item, pointShop, num, delegate(bool isSuccess)
		{
			if (isSuccess)
			{
				UpdateTab();
			}
			GameSection.ResumeEvent(isSuccess);
		});
	}

	private void OnQuery_PAGE_NEXT()
	{
		currentPage++;
		if (currentPage > maxPage)
		{
			currentPage = 1;
		}
		UpdateTab();
	}

	private void OnQuery_PAGE_PREV()
	{
		currentPage--;
		if (currentPage < 1)
		{
			currentPage = ((maxPage <= 0) ? 1 : maxPage);
		}
		UpdateTab();
	}

	private void OnQuery_FILTER()
	{
		List<PointShopItem> buyableItemList = GetBuyableItemList();
		GameSection.SetEventData(new object[2]
		{
			filter,
			buyableItemList
		});
	}

	private void OnCloseDialog_PointShopFilter()
	{
		PointShopFilterBase.Filter filter = GameSection.GetEventData() as PointShopFilterBase.Filter;
		if (filter != null)
		{
			this.filter = filter;
			currentPage = 1;
			RefreshUI();
		}
	}

	private List<PointShopItem> GetBuyableItemList()
	{
		return (from x in pointShop.Where((PointShop x) => !x.isEvent).SelectMany((PointShop x) => x.items)
			where x.isBuyable
			where x.type != 8 || !MonoBehaviourSingleton<UserInfoManager>.I.IsUnlockedStamp(x.itemId)
			where x.type != 9 || !MonoBehaviourSingleton<UserInfoManager>.I.IsUnlockedDegree(x.itemId)
			where x.type != 7 || !MonoBehaviourSingleton<GlobalSettingsManager>.I.IsUnlockedAvatar(x.itemId)
			select x).ToList();
	}
}
