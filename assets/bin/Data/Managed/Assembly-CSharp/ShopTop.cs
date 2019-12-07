using Network;
using OnePF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ShopTop : SkillInfoBase
{
	private enum UI
	{
		GRD_LIST,
		TBL_LIST,
		SCR_LIST,
		SCR_LIST_2,
		BTN_MAGI_GACHA,
		BTN_QUEST_GACHA,
		OBJ_BTN_ROOT,
		GRD_BTN,
		TBL_BTN_HOLIZONTAL,
		GRD_BTN_INNER,
		TXT_GACHA_ITEM_NUM,
		TEX_GACHA_EVENT_TITLE,
		SPR_RARITY_SSS,
		SPR_RARITY_SS,
		SPR_RARITY_S,
		SPR_RARITY_A,
		SPR_RARITY_B,
		SPR_BG_BLACK,
		LBL_NAME,
		LBL_DESCRIPTION,
		TEX_ENEMY_MODEL,
		LBL_ENEMY,
		LBL_ENEMY_LV,
		OBJ_REWARD_ICON_ROOT,
		OBJ_QUEST_INFO_ROOT,
		LBL_TIME,
		OBJ_SKILL_INFO_ROOT,
		LBL_SKILL_ITEM_NAME,
		LBL_TYPE_NAME,
		SPR_RARITY_BG,
		SPR_RARITY_ICON,
		SPR_EQUIP_TYPE_ICON,
		OBJ_SKILL_BANNER,
		OBJ_SKILL_MODEL_ROOT,
		TEX_SKILL_MODEL,
		TEX_SKILL_INNER_MODEL,
		TEX_SKILL_NPC_MODEL,
		TEX_SKILL_SUB_NPC_MODEL,
		BTN_GACHA,
		SPR_CRYSTAL,
		TEX_TICKET,
		LBL_CRYSTAL_PRICE,
		LBL_TICKET_PRICE,
		LBL_HAVE,
		SPR_MULTI,
		SPR_LINE,
		LBL_TITLE,
		SPR_GACHA_BUTTON_BG,
		LBL_GACHA_CAPTION,
		TEX_TICKET_HAVE,
		COUNTER_LBL,
		NUMBER_COUNTER_IMG,
		S_COUNTER,
		S_AVAILABLE,
		COUNTER_PROGRESSBAR_FOREGROUND,
		LBL_CRYSTAL_NUM,
		LBL_MORE_TICKET,
		OBJ_GUARANTEE_HEADER_ROOT,
		OBJ_GUARANTEE_FOOTER_ROOT,
		BTN_GUARANTEE_COUNT_DOWN,
		BTN_GUARANTEE_DETAIL,
		TEX_GUARANTEE_TIME,
		LBL_GACHA_DESCRIPTION,
		TEX_GACHA_BANNER,
		BTN_GACHA_BANNER,
		TEX_SKILL_BANNER,
		WGT_REWARD_EFFECT,
		SPR_NOTE_BG,
		LBL_NOTE,
		BTN_VIEW_PROBABIRITY,
		SPR_GACHA_EFFECT,
		SPR_GACHA_PLAY,
		SPR_GACHA_ICON,
		LBL_FRIEND_INVITATION_REMAIN,
		LBL_FRIEND_INVITATION_INVITED,
		LBL_FRIEND_INVITATION_TIME
	}

	public class PickUp
	{
		public int orderNo;

		protected uint gachaResultItemID;

		public uint GetGachaResultItemID()
		{
			return gachaResultItemID;
		}
	}

	public class PickUpQuest : PickUp
	{
		public uint materialID;

		public uint equipItemID;

		public uint questID
		{
			get
			{
				return gachaResultItemID;
			}
			set
			{
				gachaResultItemID = value;
			}
		}

		public PickUpQuest(int no, int quest, uint material, uint equip)
		{
			orderNo = no;
			questID = (uint)quest;
			materialID = material;
			equipItemID = equip;
		}
	}

	public class PickUpSkill : PickUp
	{
		public GachaList.GachaPickupAnim gachaAnim;

		public uint skillID
		{
			get
			{
				return gachaResultItemID;
			}
			set
			{
				gachaResultItemID = value;
			}
		}

		public PickUpSkill(int no, int skill, GachaList.GachaPickupAnim anim)
		{
			orderNo = no;
			skillID = (uint)skill;
			gachaAnim = anim;
		}
	}

	private class GachaModelInfo
	{
		public class GachaDataInfo
		{
			public int groupID;

			public int priority;

			public int showPickupIndex;

			public List<GachaList.Gacha> gachas;

			public List<GachaGuaranteeCampaignInfo> gachaGuaranteeCampaignInfos;

			public List<GachaFriendPromotionInfo> friendPromotionInfo;

			public List<PickUp> pickup;

			public string bannerImg;

			public string buttonImgId;

			public string url;

			public string note;

			public int counter = -1;

			public string expireAt;

			public NPCLoader npcLoader;

			public bool isFirstSkillListDirection = true;

			public uint rewardIconMaterialInfoID;

			public GachaGuaranteeCampaignInfo GetGachaGuaranteeCampaignInfo(List<GachaList.Gacha> gachas)
			{
				int num = gachas.Count();
				for (int i = 0; i < num; i++)
				{
					GachaList.Gacha gacha = gachas[i];
					GachaGuaranteeCampaignInfo gachaGuaranteeCampaignInfo = GetGachaGuaranteeCampaignInfo(gacha.gachaId);
					if (gachaGuaranteeCampaignInfo != null)
					{
						return gachaGuaranteeCampaignInfo;
					}
				}
				return null;
			}

			public GachaGuaranteeCampaignInfo GetGachaGuaranteeCampaignInfo(int gachaId)
			{
				if (gachaGuaranteeCampaignInfos == null)
				{
					return null;
				}
				return (from g in gachaGuaranteeCampaignInfos
					where g.gachaId == gachaId
					where g.IsValid()
					select g).FirstOrDefault();
			}

			public GachaFriendPromotionInfo GetGachaFriendPromotionInfo(int gachaId)
			{
				if (friendPromotionInfo == null || friendPromotionInfo.Count <= 0)
				{
					return null;
				}
				int i = 0;
				for (int count = friendPromotionInfo.Count; i < count; i++)
				{
					GachaFriendPromotionInfo gachaFriendPromotionInfo = friendPromotionInfo[i];
					if (gachaFriendPromotionInfo.gachaId == gachaId)
					{
						return gachaFriendPromotionInfo;
					}
				}
				return null;
			}
		}

		public int sortPriority;

		public GACHA_TYPE type;

		public string url;

		public List<GachaDataInfo> gachaDataInfo;
	}

	private class GachaUIInfo
	{
		public int index;

		public Transform parent;

		public GachaModelInfo gachaModelInfo;
	}

	private class GuaranteeInfoUIStatus
	{
	}

	private const string STR_ENEMY_TEX_ITEM = "enemy_tex_";

	private const string STR_SKILL_TEX_ITEM = "skill_tex_";

	private const string SPR_NAME_CRYSTAL = "Juel5";

	private const string SPR_NAME_TICKET = "Ticket";

	private const int PAGE_GACHA_QUEST = 0;

	private const int PAGE_GACHA_MAGI = 1;

	private string selectProductId = "";

	private bool isPurchase;

	private string pp;

	private bool _isFinishGetNativeProductlist;

	private StoreDataList _nativeStoreList;

	private List<Transform> ticketTitleRootList = new List<Transform>();

	private List<GachaModelInfo> gachaModelInfo;

	private List<uint> pickUpMaterialIDs;

	private int pageIndex;

	private bool isSectionStarted;

	private List<GachaUIInfo> gachaUIInfoList = new List<GachaUIInfo>();

	private List<UITable> gachaBtn = new List<UITable>();

	private LoadingQueue loadQueue;

	private bool isFinished;

	private List<Coroutine> coroutineList = new List<Coroutine>();

	private bool isDoGacha;

	private int currentCrystalRequestCount;

	private const float PICKUP_UPDATE_TIME = 5f;

	private float timer;

	public override bool useOnPressBackKey => true;

	public override void OnPressBackKey()
	{
		string goingHomeEvent = GameSection.GetGoingHomeEvent();
		DispatchEvent(goingHomeEvent);
	}

	public override void Initialize()
	{
		if (MonoBehaviourSingleton<GachaManager>.I.selectGachaType == GACHA_TYPE.QUEST || MonoBehaviourSingleton<GachaManager>.I.selectGachaType == (GACHA_TYPE)0)
		{
			pageIndex = 0;
		}
		else
		{
			pageIndex = 1;
		}
		pickUpMaterialIDs = new List<uint>();
		if (MonoBehaviourSingleton<UserInfoManager>.I.userStatus.IsTutorialBitReady && !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.GACHA1))
		{
			MonoBehaviourSingleton<GoWrapManager>.I.trackTutorialStep(TRACK_TUTORIAL_STEP_BIT.tutorial_8_gacha, "Tutorial");
			Debug.LogWarning("trackTutorialStep " + TRACK_TUTORIAL_STEP_BIT.tutorial_8_gacha.ToString());
			MonoBehaviourSingleton<GoWrapManager>.I.SendStatusTracking(TRACK_TUTORIAL_STEP_BIT.tutorial_8_gacha, "Tutorial");
		}
		StartCoroutine(DoInitialize());
	}

	private IEnumerator DoInitialize()
	{
		LoadingQueue load_queue = new LoadingQueue(this);
		EnemyLoader.CacheUIElementEffects(load_queue);
		bool wait = true;
		MonoBehaviourSingleton<GachaManager>.I.SendGetGacha(delegate
		{
			wait = false;
		});
		while (wait)
		{
			yield return null;
		}
		List<string> productIds = new List<string>();
		MonoBehaviourSingleton<GachaManager>.I.gachaData.types.ForEach(delegate(GachaList.GachaType data)
		{
			data.groups.ForEach(delegate(GachaList.GachaGroup group)
			{
				group.gachas.ForEach(delegate(GachaList.Gacha gacha)
				{
					if (!string.IsNullOrEmpty(gacha.productId))
					{
						productIds.Add(gacha.productId);
					}
				});
			});
		});
		ShopReceiver i = MonoBehaviourSingleton<ShopReceiver>.I;
		i.onGetProductDatas = (Action<StoreDataList>)Delegate.Combine(i.onGetProductDatas, new Action<StoreDataList>(OnGetProductDatas));
		_isFinishGetNativeProductlist = false;
		Native.GetProductDatas(string.Join("----", productIds.ToArray()));
		while (!_isFinishGetNativeProductlist)
		{
			yield return null;
		}
		gachaModelInfo = new List<GachaModelInfo>();
		MonoBehaviourSingleton<GachaManager>.I.gachaData.types.ForEach(delegate(GachaList.GachaType data)
		{
			GACHA_TYPE gacha_type = data.ViewType;
			GachaModelInfo add_data = gachaModelInfo.Find((GachaModelInfo g) => g.type == gacha_type);
			if (add_data == null)
			{
				add_data = new GachaModelInfo();
				add_data.type = gacha_type;
				add_data.url = data.url;
				add_data.gachaDataInfo = new List<GachaModelInfo.GachaDataInfo>();
				gachaModelInfo.Add(add_data);
			}
			switch (gacha_type)
			{
			case GACHA_TYPE.QUEST:
				add_data.sortPriority = 1;
				data.groups.ForEach(delegate(GachaList.GachaGroup groups)
				{
					GachaModelInfo.GachaDataInfo gacha_data_info = new GachaModelInfo.GachaDataInfo();
					gacha_data_info.showPickupIndex = 0;
					gacha_data_info.groupID = groups.group;
					gacha_data_info.bannerImg = groups.bannerImg;
					gacha_data_info.url = groups.url;
					gacha_data_info.gachas = groups.gachas;
					gacha_data_info.gachaGuaranteeCampaignInfos = groups.gachaGuaranteeCampaignInfo;
					gacha_data_info.friendPromotionInfo = groups.friendPromotionInfo;
					gacha_data_info.note = groups.note;
					gacha_data_info.priority = groups.priority;
					gacha_data_info.counter = groups.counter;
					Debug.Log("Gacha Info Counter: " + gacha_data_info.counter);
					gacha_data_info.expireAt = groups.expireAt;
					if (gacha_data_info.gachas != null && gacha_data_info.gachas[0] != null)
					{
						gacha_data_info.buttonImgId = groups.gachas[0].buttonImg;
					}
					gacha_data_info.pickup = new List<PickUp>();
					groups.pickupLineups.ForEach(delegate(GachaList.GachaLineup pickup_data)
					{
						int reward_pri = -1;
						uint reward_id = 0u;
						pickup_data.sellItems.ForEach(delegate(QuestItem.SellItem reward_data)
						{
							if (reward_data.type == 3)
							{
								ItemTable.ItemData itemData = Singleton<ItemTable>.I.GetItemData((uint)reward_data.itemId);
								if (itemData != null && itemData.type != ITEM_TYPE.USE_ITEM && itemData.type != ITEM_TYPE.FAIRY && itemData.type != 0 && (reward_id == 0 || reward_pri == -1 || reward_pri > reward_data.pri))
								{
									reward_pri = reward_data.pri;
									reward_id = (uint)reward_data.itemId;
								}
							}
						});
						uint equip = 0u;
						CreateEquipItemTable.CreateEquipItemData[] creatableEquipItem = Singleton<CreateEquipItemTable>.I.GetCreatableEquipItem(reward_id);
						if (creatableEquipItem != null && creatableEquipItem.Length != 0)
						{
							equip = creatableEquipItem[0].equipItemID;
						}
						gacha_data_info.pickup.Add(new PickUpQuest(pickup_data.orderNo, pickup_data.itemId, reward_id, equip));
						CacheQuestAudio((uint)pickup_data.itemId, load_queue);
					});
					gacha_data_info.pickup.Sort((PickUp l, PickUp r) => l.orderNo - r.orderNo);
					add_data.gachaDataInfo.Add(gacha_data_info);
				});
				break;
			case GACHA_TYPE.SKILL:
				add_data.sortPriority = 2;
				data.groups.ForEach(delegate(GachaList.GachaGroup groups)
				{
					GachaModelInfo.GachaDataInfo gacha_data_info2 = new GachaModelInfo.GachaDataInfo();
					gacha_data_info2.showPickupIndex = 0;
					gacha_data_info2.groupID = groups.group;
					gacha_data_info2.bannerImg = groups.bannerImg;
					gacha_data_info2.url = groups.url;
					gacha_data_info2.gachas = groups.gachas;
					gacha_data_info2.gachaGuaranteeCampaignInfos = groups.gachaGuaranteeCampaignInfo;
					gacha_data_info2.note = groups.note;
					gacha_data_info2.priority = groups.priority;
					gacha_data_info2.counter = groups.counter;
					Debug.Log("Gacha Info Counter: " + gacha_data_info2.counter);
					gacha_data_info2.expireAt = groups.expireAt;
					if (gacha_data_info2.gachas != null && gacha_data_info2.gachas[0] != null)
					{
						gacha_data_info2.buttonImgId = groups.gachas[0].buttonImg;
					}
					gacha_data_info2.pickup = new List<PickUp>();
					groups.pickupLineups.ForEach(delegate(GachaList.GachaLineup pickup_data)
					{
						gacha_data_info2.pickup.Add(new PickUpSkill(pickup_data.orderNo, pickup_data.itemId, pickup_data.anim));
					});
					add_data.gachaDataInfo.Add(gacha_data_info2);
				});
				break;
			default:
				add_data.sortPriority = 3;
				break;
			}
			add_data.gachaDataInfo.Sort((GachaModelInfo.GachaDataInfo l, GachaModelInfo.GachaDataInfo r) => r.priority - l.priority);
		});
		gachaModelInfo.Sort((GachaModelInfo l, GachaModelInfo r) => l.sortPriority - r.sortPriority);
		if (load_queue.IsLoading())
		{
			yield return load_queue.Wait();
		}
		base.Initialize();
	}

	public override void OnNotify(NOTIFY_FLAG flags)
	{
		if ((flags & NOTIFY_FLAG.UPDATE_ITEM_INVENTORY) != (NOTIFY_FLAG)0L && MonoBehaviourSingleton<InventoryManager>.IsValid() && pageIndex == 0)
		{
			string text = MonoBehaviourSingleton<InventoryManager>.I.GetItemNum((ItemInfo x) => x.tableData.type == ITEM_TYPE.TICKET).ToString();
			int i = 0;
			for (int count = ticketTitleRootList.Count; i < count; i++)
			{
				SetLabelText(ticketTitleRootList[i], UI.LBL_HAVE, text);
			}
		}
	}

	public override void StartSection()
	{
		GachaList.Gacha latestPopupGacha = GetLatestPopupGacha();
		GachaGuaranteeCampaignInfo latestPopupGualantee = GetLatestPopupGualantee();
		DateTime startDateTime;
		string campaignDetailImg;
		if (latestPopupGualantee == null)
		{
			startDateTime = latestPopupGacha.GetStartDateTime();
			campaignDetailImg = latestPopupGacha.campaignDetailImg;
		}
		else if (latestPopupGualantee.GetStartDateTime() > latestPopupGacha.GetStartDateTime())
		{
			startDateTime = latestPopupGualantee.GetStartDateTime();
			campaignDetailImg = latestPopupGualantee.campaignDetailImg;
		}
		else
		{
			startDateTime = latestPopupGacha.GetStartDateTime();
			campaignDetailImg = latestPopupGacha.campaignDetailImg;
		}
		bool flag = CheckShowPopUp(startDateTime, campaignDetailImg);
		if (!MonoBehaviourSingleton<GachaManager>.I.IsTutorial() && flag)
		{
			ShowPopUp(campaignDetailImg);
		}
		isSectionStarted = true;
		base.StartSection();
	}

	private bool CheckShowPopUp(DateTime startDate, string campaignDetailImg)
	{
		if (campaignDetailImg == "")
		{
			return false;
		}
		if (!MonoBehaviourSingleton<GachaManager>.I.HasBeenShowAdvertisement())
		{
			MonoBehaviourSingleton<GachaManager>.I.SetTimeShowShopAdvertisement(startDate);
			return true;
		}
		if (MonoBehaviourSingleton<GachaManager>.I.GetTimeShowShopAdvertisement() < startDate)
		{
			MonoBehaviourSingleton<GachaManager>.I.SetTimeShowShopAdvertisement(startDate);
			return true;
		}
		return false;
	}

	private void ShowPopUp(string popupImageName)
	{
		if (popupImageName != "")
		{
			EventData[] autoEvents = new EventData[1]
			{
				new EventData("POP_UP", popupImageName)
			};
			MonoBehaviourSingleton<GameSceneManager>.I.SetAutoEvents(autoEvents);
		}
	}

	private List<GachaGuaranteeCampaignInfo> GetStepUpInfos()
	{
		List<GachaGuaranteeCampaignInfo> list = new List<GachaGuaranteeCampaignInfo>();
		for (int i = 0; i < gachaModelInfo.Count; i++)
		{
			List<GachaModelInfo.GachaDataInfo> gachaDataInfo = gachaModelInfo[i].gachaDataInfo;
			for (int j = 0; j < gachaDataInfo.Count; j++)
			{
				List<GachaGuaranteeCampaignInfo> list2 = gachaDataInfo[j].gachaGuaranteeCampaignInfos.FindAll((GachaGuaranteeCampaignInfo g) => g.IsStepUp());
				if (list2 != null)
				{
					list.AddRange(list2);
				}
			}
		}
		return list;
	}

	private GachaList.Gacha GetLatestPopupGacha()
	{
		GachaList.Gacha gacha = null;
		for (int i = 0; i < gachaModelInfo.Count; i++)
		{
			for (int j = 0; j < gachaModelInfo[i].gachaDataInfo.Count; j++)
			{
				for (int k = 0; k < gachaModelInfo[i].gachaDataInfo[j].gachas.Count; k++)
				{
					GachaList.Gacha gacha2 = gachaModelInfo[i].gachaDataInfo[j].gachas[k];
					if (!(gacha2.campaignDetailImg == ""))
					{
						DateTime startDateTime = gacha2.GetStartDateTime();
						if (gacha == null || !(gacha.GetStartDateTime() >= startDateTime))
						{
							gacha = gacha2;
						}
					}
				}
			}
		}
		return gacha;
	}

	private GachaGuaranteeCampaignInfo GetLatestPopupGualantee()
	{
		List<GachaGuaranteeCampaignInfo> stepUpInfos = GetStepUpInfos();
		if (stepUpInfos.Count == 0)
		{
			return null;
		}
		GachaGuaranteeCampaignInfo gachaGuaranteeCampaignInfo = null;
		int count = stepUpInfos.Count;
		for (int i = 0; i < count; i++)
		{
			GachaGuaranteeCampaignInfo gachaGuaranteeCampaignInfo2 = stepUpInfos[i];
			if (!(gachaGuaranteeCampaignInfo2.campaignDetailImg == ""))
			{
				DateTime startDateTime = gachaGuaranteeCampaignInfo2.GetStartDateTime();
				if (gachaGuaranteeCampaignInfo == null || !(gachaGuaranteeCampaignInfo.GetStartDateTime() >= startDateTime))
				{
					gachaGuaranteeCampaignInfo = gachaGuaranteeCampaignInfo2;
				}
			}
		}
		return gachaGuaranteeCampaignInfo;
	}

	protected void CacheQuestAudio(uint quest_id, LoadingQueue lo_queue)
	{
		QuestTable.QuestTableData questData = Singleton<QuestTable>.I.GetQuestData(quest_id);
		if (questData == null)
		{
			return;
		}
		int mainEnemyID = questData.GetMainEnemyID();
		if (Singleton<EnemyTable>.IsValid())
		{
			EnemyTable.EnemyData enemyData = Singleton<EnemyTable>.I.GetEnemyData((uint)mainEnemyID);
			if (enemyData != null)
			{
				CacheEnemyAudio(enemyData, lo_queue);
			}
		}
	}

	protected void CacheEnemyAudio(EnemyTable.EnemyData enemyData, LoadingQueue lo_queue)
	{
		if (lo_queue != null)
		{
			OutGameSettingsManager.EnemyDisplayInfo enemyDisplayInfo = MonoBehaviourSingleton<OutGameSettingsManager>.I.SearchEnemyDisplayInfoForGacha(enemyData);
			if (enemyDisplayInfo != null && enemyDisplayInfo.seIdGachaShort > 0)
			{
				lo_queue.CacheSE(enemyDisplayInfo.seIdhowl);
			}
		}
	}

	protected override void OnOpen()
	{
		MonoBehaviourSingleton<UIManager>.I.enableShadow = true;
		MonoBehaviourSingleton<ShopReceiver>.I.onBuyGacha = null;
		ShopReceiver i = MonoBehaviourSingleton<ShopReceiver>.I;
		i.onBuyGacha = (Action<string>)Delegate.Combine(i.onBuyGacha, new Action<string>(OnBuyItem));
		ShopReceiver i2 = MonoBehaviourSingleton<ShopReceiver>.I;
		i2.onBuyItem = (Action<string>)Delegate.Combine(i2.onBuyItem, new Action<string>(OnBuyItem));
		isPurchase = false;
		base.OnOpen();
	}

	protected override void OnClose()
	{
		MonoBehaviourSingleton<UIManager>.I.enableShadow = false;
		base.OnClose();
	}

	public override void Close(UITransition.TYPE type)
	{
		base.Close(type);
		DeleteModel();
	}

	private void DeleteModel()
	{
		if (gachaModelInfo == null)
		{
			return;
		}
		Transform ctrl = GetCtrl(UI.SCR_LIST_2);
		if (!(ctrl != null) || ctrl.childCount <= 0)
		{
			return;
		}
		int i = 0;
		for (int childCount = ctrl.childCount; i < childCount; i++)
		{
			Transform child = ctrl.GetChild(i);
			int result2;
			if (child.name.Contains("enemy_tex_"))
			{
				if (int.TryParse(child.name.Remove(0, "enemy_tex_".Length), out int result))
				{
					Transform root = GetCtrl(UI.SCR_LIST_2).Find("enemy_tex_" + result);
					DeleteRenderTexture(root, UI.TEX_ENEMY_MODEL);
					SetVisibleWidgetEffect(UI.SCR_LIST_2, root, UI.TEX_ENEMY_MODEL, null);
				}
			}
			else if (child.name.Contains("skill_tex_") && int.TryParse(child.name.Remove(0, "skill_tex_".Length), out result2))
			{
				Transform root2 = GetCtrl(UI.SCR_LIST_2).Find("skill_tex_" + result2);
				DeleteRenderTexture(root2, UI.TEX_SKILL_NPC_MODEL);
				DeleteRenderTexture(root2, UI.TEX_SKILL_SUB_NPC_MODEL);
				DeleteRenderTexture(root2, UI.TEX_SKILL_BANNER);
			}
		}
	}

	private void SetGachaChangeButton(int pageIndex)
	{
		SetActive(UI.BTN_QUEST_GACHA, pageIndex != 0);
		SetActive(UI.BTN_MAGI_GACHA, pageIndex == 0);
	}

	private void SetGachaListUI()
	{
		SetGachaChangeButton(pageIndex);
		gachaUIInfoList.Clear();
		gachaBtn.Clear();
		int count = 0;
		if (gachaModelInfo != null && gachaModelInfo.Count > pageIndex && gachaModelInfo[pageIndex].gachaDataInfo != null)
		{
			count = gachaModelInfo[pageIndex].gachaDataInfo.Count;
			count *= 2;
		}
		int item_num = (count != 0) ? (count + 1) : 0;
		StopLoadCoroutine();
		SetTable(UI.TBL_LIST, "GachaListItem", item_num, reset: false, delegate(int i, Transform p)
		{
			if (i < count)
			{
				if (i % 2 == 0)
				{
					return Realizes("GachaBanner", p);
				}
				GACHA_TYPE type = gachaModelInfo[pageIndex].type;
				if (type == GACHA_TYPE.SKILL)
				{
					return Realizes("GachaSkillListItem", p);
				}
				return null;
			}
			return (i == count) ? Realizes("GachaDescriptionItem", p) : null;
		}, delegate(int i, Transform t, bool b)
		{
			if (i < count)
			{
				int index = i / 2;
				GachaModelInfo gachaModelInfo = this.gachaModelInfo[pageIndex];
				GachaModelInfo.GachaDataInfo gacha_info = gachaModelInfo.gachaDataInfo[index];
				if (i % 2 == 0)
				{
					SetGachaBanner(t, gacha_info);
				}
				else
				{
					if (gachaModelInfo.type == GACHA_TYPE.QUEST)
					{
						SetGachaNote(t, gacha_info);
					}
					SetGachaButtonsTable(t, gacha_info);
					gachaUIInfoList.Add(new GachaUIInfo
					{
						index = index,
						parent = t,
						gachaModelInfo = gachaModelInfo
					});
				}
			}
			else if (i == count)
			{
				FindCtrl(t, UI.LBL_GACHA_DESCRIPTION).GetComponent<UILabel>().supportEncoding = true;
				string str = string.IsNullOrEmpty(this.gachaModelInfo[pageIndex].url) ? null : StringTable.Get(STRING_CATEGORY.SHOP, (uint)(GameDefine.GACHA_VIEW_PROBABILITY + pageIndex));
				SetLabelText(t, UI.LBL_GACHA_DESCRIPTION, StringTable.Get(STRING_CATEGORY.SHOP, (uint)(10 + pageIndex)) + str);
				SetActive(t, UI.BTN_VIEW_PROBABIRITY, !string.IsNullOrEmpty(this.gachaModelInfo[pageIndex].url));
			}
		});
		RepositionTables();
	}

	private void SetGachaBanner(Transform t, GachaModelInfo.GachaDataInfo gacha_info)
	{
		bool flag = !string.IsNullOrEmpty(gacha_info.bannerImg);
		SetActive(t, flag);
		if (flag)
		{
			string bannerImg = gacha_info.bannerImg;
			Coroutine item = StartCoroutine(LoadGachaBanner(t, UI.TEX_GACHA_BANNER, bannerImg));
			coroutineList.Add(item);
			SetButtonEnabled(FindCtrl(t, UI.BTN_GACHA_BANNER), !string.IsNullOrEmpty(gacha_info.url));
			SetEvent(FindCtrl(t, UI.BTN_GACHA_BANNER), "GACHA_BANNER", gacha_info);
		}
	}

	private void SetGachaNote(Transform t, GachaModelInfo.GachaDataInfo gacha_info)
	{
		if (!string.IsNullOrEmpty(gacha_info.note))
		{
			FindCtrl(t, UI.SPR_NOTE_BG).gameObject.SetActive(value: true);
			FindCtrl(t, UI.LBL_NOTE).gameObject.SetActive(value: true);
			if (gacha_info.note.Contains("\\n"))
			{
				gacha_info.note = gacha_info.note.Replace("\\n", "\n");
				int num = gacha_info.note.Length - gacha_info.note.Replace("\n", "").Length;
				if (num > 0)
				{
					Transform transform = FindCtrl(t, UI.OBJ_BTN_ROOT);
					Vector3 localPosition = transform.transform.localPosition;
					transform.transform.localPosition = new Vector3(localPosition.x, -204f - (float)(num + 1) * 16f, localPosition.z);
					t.gameObject.GetComponent<UIWidget>().height = 471 + (num + 1) * 16;
				}
			}
			SetLabelText(t, UI.LBL_NOTE, gacha_info.note);
		}
		else
		{
			FindCtrl(t, UI.SPR_NOTE_BG).gameObject.SetActive(value: false);
			FindCtrl(t, UI.LBL_NOTE).gameObject.SetActive(value: false);
			Transform transform2 = FindCtrl(t, UI.OBJ_BTN_ROOT);
			Vector3 localPosition2 = transform2.transform.localPosition;
			transform2.transform.localPosition = new Vector3(localPosition2.x, -188f, localPosition2.z);
		}
	}

	private void SetGachaButtonsTable(Transform parent, GachaModelInfo.GachaDataInfo gacha_info)
	{
		List<int> subGroupIds = new List<int>();
		int count = gacha_info.gachas.Count;
		for (int j = 0; j < count; j++)
		{
			GachaList.Gacha gacha = gacha_info.gachas[j];
			if (!subGroupIds.Contains(gacha.subGroup))
			{
				subGroupIds.Add(gacha.subGroup);
			}
		}
		List<int> ticketTitleIndexList = new List<int>();
		List<GachaList.Gacha> list = null;
		List<int> promotionGachaIndexList = new List<int>();
		int num = 0;
		for (int k = 0; k < subGroupIds.Count; k++)
		{
			int subGroupId = subGroupIds[k];
			List<GachaList.Gacha> list2 = gacha_info.gachas.Where((GachaList.Gacha g) => g.subGroup == subGroupId).ToList();
			GachaList.Gacha gacha2 = list2.First();
			int requiredItemId = gacha2.requiredItemId;
			int item = k + ticketTitleIndexList.Count;
			if (requiredItemId > 0 && (list == null || requiredItemId != num))
			{
				ticketTitleIndexList.Add(item);
			}
			else if (gacha_info.GetGachaFriendPromotionInfo(gacha2.gachaId) != null)
			{
				promotionGachaIndexList.Add(item);
			}
			num = requiredItemId;
			list = list2;
		}
		gachaBtn.Add(FindCtrl(parent, UI.GRD_BTN).GetComponent<UITable>());
		int tableListCount = subGroupIds.Count + ticketTitleIndexList.Count;
		bool isHaveGachaTicket = false;
		int requireItemId = num;
		if (gachaModelInfo[pageIndex].type == GACHA_TYPE.QUEST && num > 0)
		{
			isHaveGachaTicket = true;
			tableListCount++;
		}
		int ticketNumOfGroup = 0;
		SetTable(parent, UI.GRD_BTN, "GachaButtonItemTwoLine", tableListCount, reset: false, delegate(int i, Transform t)
		{
			if (((gachaModelInfo[pageIndex].type == GACHA_TYPE.QUEST) & isHaveGachaTicket) && i == tableListCount - 1)
			{
				return Realizes("GachaTicketNumOwned", t);
			}
			if (ticketTitleIndexList.Exists((int x) => x == i))
			{
				if (gacha_info.counter >= 0)
				{
					return Realizes("GachaTicketTitleCounterS", t);
				}
				Transform transform = Realizes("GachaTicketTitle", t);
				ticketTitleRootList.Add(transform);
				return transform;
			}
			return promotionGachaIndexList.Contains(i) ? Realizes("GachaButtonItemFriendPromotion", t) : null;
		}, delegate(int i, Transform t, bool is_recycle)
		{
			if (((gachaModelInfo[pageIndex].type == GACHA_TYPE.QUEST) & isHaveGachaTicket) && i == tableListCount - 1)
			{
				if (ticketNumOfGroup >= 0)
				{
					SetSupportEncoding(t, UI.LBL_MORE_TICKET, isEnable: true);
					SetLabelText(t, UI.LBL_CRYSTAL_NUM, ticketNumOfGroup.ToString());
					ItemTable.ItemData itemData = Singleton<ItemTable>.I.GetItemData((uint)requireItemId);
					ResourceLoad.LoadItemIconTexture(FindCtrl(t, UI.TEX_TICKET_HAVE).GetComponent<UITexture>(), itemData.iconID);
				}
			}
			else
			{
				int index = CreateGachaSubGroupIndex(i, ticketTitleIndexList);
				int subGroupId2 = subGroupIds[index];
				List<GachaList.Gacha> list3 = gacha_info.gachas.Where((GachaList.Gacha g) => g.subGroup == subGroupId2).ToList();
				GachaList.Gacha gacha3 = list3.First();
				if (ticketTitleIndexList.Exists((int x) => x == i))
				{
					int id = gacha3.requiredItemId;
					int itemNum = MonoBehaviourSingleton<InventoryManager>.I.GetItemNum((ItemInfo x) => x.tableData.id == id, 1);
					ItemTable.ItemData itemData2 = Singleton<ItemTable>.I.GetItemData((uint)id);
					SetLabelText(t, UI.LBL_TITLE, itemData2.name);
					SetLabelText(t, UI.LBL_HAVE, itemNum.ToString());
					ticketNumOfGroup = itemNum;
					if (gacha_info.counter > 0)
					{
						SetActive(t, UI.S_COUNTER, is_visible: true);
						SetLabelText(t, UI.COUNTER_LBL, gacha_info.counter);
						SetActive(t, UI.NUMBER_COUNTER_IMG, is_visible: true);
						FindCtrl(t, UI.NUMBER_COUNTER_IMG).GetComponent<UISprite>().spriteName = gacha_info.counter.ToString();
						FindCtrl(t, UI.COUNTER_PROGRESSBAR_FOREGROUND).GetComponent<UISprite>().fillAmount = (float)(10 - gacha_info.counter) / 10f;
						SetActive(t, UI.S_AVAILABLE, is_visible: false);
					}
					else
					{
						SetActive(t, UI.S_COUNTER, is_visible: false);
						SetActive(t, UI.S_AVAILABLE, is_visible: true);
					}
				}
				else
				{
					GachaGuaranteeCampaignInfo gachaGuaranteeCampaignInfo = null;
					GachaFriendPromotionInfo gachaFriendPromotionInfo = gacha_info.GetGachaFriendPromotionInfo(gacha3.gachaId);
					if (gachaFriendPromotionInfo != null)
					{
						SetUpFriendPromotionGachaTableItem(t, gachaFriendPromotionInfo, gacha3);
					}
					else
					{
						gachaGuaranteeCampaignInfo = gacha_info.GetGachaGuaranteeCampaignInfo(list3);
						SetGachaDetailUI(t, gacha_info, list3, gachaGuaranteeCampaignInfo);
					}
					SetUpGachaButtonGrid(t, list3, gacha_info, gachaGuaranteeCampaignInfo);
					if (gachaGuaranteeCampaignInfo != null)
					{
						FindCtrl(t, UI.SPR_LINE).localPosition = new Vector3(0f, -40f, 0f);
					}
					List<GachaList.Gacha> list4 = new List<GachaList.Gacha>();
					list4 = list3.Where((GachaList.Gacha g) => g.detailButtonImg != "").ToList();
					bool flag = gachaGuaranteeCampaignInfo != null || list4.Count > 0;
					SetActive(t, UI.SPR_LINE, i == tableListCount - 1 && !flag);
					if (gacha3.caption != "")
					{
						SetLabelText(t, UI.LBL_GACHA_CAPTION, gacha3.caption);
						SetSupportEncoding(t, UI.LBL_GACHA_CAPTION, isEnable: true);
						UIWidget component = GetComponent<UIWidget>(t, UI.SPR_GACHA_BUTTON_BG);
						int height = component.height;
						int num2 = gacha3.caption.Count((char c) => c == '\n') + 1;
						component.height = height + 19 * num2;
					}
				}
			}
		});
	}

	private void SetUpGachaButtonGrid(Transform t, List<GachaList.Gacha> subGroupGachas, GachaModelInfo.GachaDataInfo gachaInfo, GachaGuaranteeCampaignInfo gachaGuaranteeInfo)
	{
		int gachaEventTitleCount = 0;
		string gachaEventTitleName = subGroupGachas[0].eventTitleImg;
		if (subGroupGachas.Count() == 1 && !string.IsNullOrEmpty(gachaEventTitleName))
		{
			gachaEventTitleCount = 1;
			if (subGroupGachas[0].IsDirectPurchase())
			{
				gachaEventTitleName += "_AND";
			}
		}
		int item_num = subGroupGachas.Count() + gachaEventTitleCount;
		SetGrid(t, UI.GRD_BTN_INNER, "GachaButtonRoot", item_num, reset: false, (int gridIdx, Transform gridTrans) => (gridIdx < gachaEventTitleCount) ? (IsTitleMini(gachaInfo, subGroupGachas) ? Realizes("GachaEventTitleMini", gridTrans) : Realizes("GachaEventTitle", gridTrans)) : null, delegate(int gridIdx, Transform gridTrans, bool gridIsRecycle)
		{
			if (gridIdx < gachaEventTitleCount)
			{
				Coroutine item = StartCoroutine(LoadGachaEventTitle(gridTrans, UI.TEX_GACHA_EVENT_TITLE, gachaEventTitleName));
				coroutineList.Add(item);
			}
			else
			{
				GachaList.Gacha gacha = subGroupGachas[gridIdx - gachaEventTitleCount];
				string text = "";
				if (!gacha.IsOncePurchase())
				{
					text = ((gacha.requiredItemId == 0) ? ("× " + gacha.crystalNum.ToString()) : ("× " + gacha.needItemNum.ToString()));
				}
				else
				{
					text = "$" + gacha.yenIncludeTax.ToString();
					if (_nativeStoreList != null)
					{
						text = _nativeStoreList.getProduct(gacha.productId).price;
					}
				}
				string empty = string.Empty;
				if (gachaInfo.counter == 0 && gacha.requiredItemId != 0)
				{
					empty = "BTN_GACHA_TICKET1_Skaku_VER2";
				}
				else
				{
					bool flag = gachaGuaranteeInfo != null && gacha.gachaId == gachaGuaranteeInfo.gachaId;
					empty = CreateButtonName(gacha, flag ? gachaGuaranteeInfo : null);
				}
				Coroutine item2 = StartCoroutine(LoadGachaButton(gridTrans, empty, text, gacha.gachaId, gachaInfo.gachas.IndexOf(gacha)));
				coroutineList.Add(item2);
			}
		});
	}

	private string CreateButtonName(GachaList.Gacha gacha, GachaGuaranteeCampaignInfo guarantee)
	{
		return MonoBehaviourSingleton<GachaManager>.I.CreateButtonBaseName(gacha, guarantee) + "_VER2";
	}

	private bool IsTitleMini(GachaModelInfo.GachaDataInfo gachaInfo, List<GachaList.Gacha> subGroupGachas)
	{
		if (gachaInfo.GetGachaFriendPromotionInfo(subGroupGachas[0].gachaId) != null)
		{
			return true;
		}
		return false;
	}

	private int CreateGachaSubGroupIndex(int tableIndex, List<int> ticketTitleIndexList)
	{
		int num = tableIndex;
		foreach (int ticketTitleIndex in ticketTitleIndexList)
		{
			if (ticketTitleIndex >= tableIndex)
			{
				return num;
			}
			num--;
		}
		return num;
	}

	private void SetGachaDetailUI(Transform t, GachaModelInfo.GachaDataInfo gachaInfo, List<GachaList.Gacha> subGroupGachas, GachaGuaranteeCampaignInfo gachaGuaranteeInfo)
	{
		List<GachaList.Gacha> detailImgGachas = subGroupGachas.Where((GachaList.Gacha g) => g.detailButtonImg != "").ToList();
		bool flag = gachaGuaranteeInfo != null;
		if (detailImgGachas.Count == 0 && !flag)
		{
			SetActive(t, UI.OBJ_GUARANTEE_HEADER_ROOT, is_visible: false);
			SetActive(t, UI.OBJ_GUARANTEE_FOOTER_ROOT, is_visible: false);
			return;
		}
		string text = "";
		string text2 = "";
		string text3 = "";
		string event_data = "";
		List<GachaList.Gacha> source;
		if (flag)
		{
			source = gachaInfo.gachas.Where((GachaList.Gacha g) => g.gachaId == gachaGuaranteeInfo.gachaId).ToList();
			text = gachaGuaranteeInfo.GetTitleImageName();
			text2 = gachaGuaranteeInfo.endAt;
			text3 = gachaGuaranteeInfo.link;
		}
		else
		{
			source = detailImgGachas.Where((GachaList.Gacha g) => g.detailButtonImg == detailImgGachas[0].GetTitleImageName()).ToList();
			text = detailImgGachas[0].GetTitleImageName();
			text2 = detailImgGachas[0].endDate;
			text3 = detailImgGachas[0].link;
			event_data = detailImgGachas[0].description;
		}
		bool num = subGroupGachas.Contains(source.First());
		bool flag2 = subGroupGachas.Contains(source.Last());
		if (num)
		{
			SetActive(t, UI.OBJ_GUARANTEE_HEADER_ROOT, is_visible: true);
			StartCoroutine(LoadGachaGuaranteeCounter(t, UI.BTN_GUARANTEE_COUNT_DOWN, gachaGuaranteeInfo.GetImageCount(), text));
			if (!flag || !gachaGuaranteeInfo.IsItemConfirmed())
			{
				Transform transform = FindCtrl(t, UI.BTN_GUARANTEE_COUNT_DOWN);
				transform.GetComponent<UIButton>().enabled = true;
				if (text3 == "")
				{
					SetEvent(transform, "GUARANTEE_GACHA_DETAIL", event_data);
				}
				else
				{
					SetEvent(transform, "GUARANTEE_GACHA_DETAIL_WEB", text3);
				}
			}
			else
			{
				switch (gachaGuaranteeInfo.type)
				{
				default:
					FindCtrl(t, UI.BTN_GUARANTEE_COUNT_DOWN).GetComponent<UIButton>().enabled = false;
					break;
				case 5:
					FindCtrl(t, UI.BTN_GUARANTEE_COUNT_DOWN).GetComponent<UIButton>().enabled = true;
					SetEvent(FindCtrl(t, UI.BTN_GUARANTEE_COUNT_DOWN), "SKILL_DETAIL", new object[2]
					{
						ItemDetailEquip.CURRENT_SECTION.SHOP_TOP,
						Singleton<SkillItemTable>.I.GetSkillItemData((uint)gachaGuaranteeInfo.itemId)
					});
					break;
				case 14:
				{
					FindCtrl(t, UI.BTN_GUARANTEE_COUNT_DOWN).GetComponent<UIButton>().enabled = true;
					AccessorySortData accessorySortData = new AccessorySortData();
					AccessoryInfo accessoryInfo = new AccessoryInfo();
					accessoryInfo.SetValue((uint)gachaGuaranteeInfo.itemId);
					accessorySortData.SetItem(accessoryInfo);
					SetEvent(FindCtrl(t, UI.BTN_GUARANTEE_COUNT_DOWN), "ACCESSORY_SELECT", new object[2]
					{
						ItemDetailEquip.CURRENT_SECTION.SHOP_TOP,
						accessorySortData
					});
					break;
				}
				}
			}
		}
		else
		{
			SetActive(t, UI.OBJ_GUARANTEE_HEADER_ROOT, is_visible: false);
		}
		if (flag2)
		{
			SetActive(t, UI.OBJ_GUARANTEE_FOOTER_ROOT, is_visible: true);
			StringTable.Get(STRING_CATEGORY.SHOP, 15u);
			SetLabelText(t, UI.TEX_GUARANTEE_TIME, string.Format(text2));
			if (text3 == "")
			{
				SetEvent(FindCtrl(t, UI.BTN_GUARANTEE_DETAIL), "GUARANTEE_GACHA_DETAIL", event_data);
			}
			else
			{
				SetEvent(FindCtrl(t, UI.BTN_GUARANTEE_DETAIL), "GUARANTEE_GACHA_DETAIL_WEB", text3);
			}
		}
		else
		{
			SetActive(t, UI.OBJ_GUARANTEE_FOOTER_ROOT, is_visible: false);
		}
	}

	private void SetUpFriendPromotionGachaTableItem(Transform t, GachaFriendPromotionInfo friendPromotionInfo, GachaList.Gacha firstGacha)
	{
		if (friendPromotionInfo != null)
		{
			string format = StringTable.Get(STRING_CATEGORY.SHOP, 15u);
			SetLabelText(t, UI.LBL_FRIEND_INVITATION_TIME, string.Format(format, firstGacha.endDate));
			string format2 = StringTable.Get(STRING_CATEGORY.SHOP, 16u);
			SetLabelText(t, UI.LBL_FRIEND_INVITATION_REMAIN, string.Format(format2, friendPromotionInfo.remainAllowedCount));
			string format3 = StringTable.Get(STRING_CATEGORY.SHOP, 17u);
			SetLabelText(t, UI.LBL_FRIEND_INVITATION_INVITED, string.Format(format3, friendPromotionInfo.invitedCount));
		}
	}

	private void UpdateGachaList()
	{
		for (int i = 0; i < gachaUIInfoList.Count; i++)
		{
			GachaUIInfo gachaUIInfo = gachaUIInfoList[i];
			if (gachaUIInfo.gachaModelInfo.type == GACHA_TYPE.QUEST)
			{
				Coroutine item = StartCoroutine(UpdateQuestList(gachaUIInfo.index, gachaUIInfo.parent));
				coroutineList.Add(item);
			}
			else if (gachaUIInfo.gachaModelInfo.type == GACHA_TYPE.SKILL)
			{
				Coroutine item2 = StartCoroutine(UpdateSkillList(gachaUIInfo.index, gachaUIInfo.parent));
				coroutineList.Add(item2);
			}
		}
	}

	private void RepositionTables()
	{
		UpdateAnchors();
		for (int i = 0; i < gachaBtn.Count; i++)
		{
			gachaBtn[i].Reposition();
		}
		GetComponent<UITable>(UI.TBL_LIST).Reposition();
	}

	public override void UpdateUI()
	{
		UIWidget component = GetCtrl(UI.SPR_BG_BLACK).GetComponent<UIWidget>();
		if (SpecialDeviceManager.HasSpecialDeviceInfo)
		{
			UIVirtualScreen componentInChildren = base.transform.GetComponentInChildren<UIVirtualScreen>();
			Debug.Log(base.transform.name);
			Debug.Log(componentInChildren);
			if (componentInChildren != null)
			{
				Debug.Log(component);
				component.width = (int)componentInChildren.ScreenWidthFull;
				component.height = (int)componentInChildren.ScreenHeightFull;
			}
		}
		if (!isSectionStarted)
		{
			SetGachaListUI();
		}
		UpdateGachaList();
	}

	private IEnumerator LoadEnemyModel(Transform t, Enum _enum, uint enemy_id, string foundation_name, ELEMENT_TYPE element_type, bool is_Howl)
	{
		yield return null;
		SetRenderEnemyModel(t, _enum, enemy_id, foundation_name, OutGameSettingsManager.EnemyDisplayInfo.SCENE.GACHA, delegate(bool ret, EnemyLoader loader)
		{
			if (ret && (object)loader != null && (object)loader.body != null)
			{
				SkinnedMeshRenderer[] componentsInChildren = loader.body.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
				if (componentsInChildren != null && componentsInChildren.Length != 0)
				{
					foreach (SkinnedMeshRenderer obj in componentsInChildren)
					{
						Bounds localBounds = obj.localBounds;
						localBounds.center = Vector3.zero;
						obj.localBounds = localBounds;
					}
				}
			}
		}, UIModelRenderTexture.ENEMY_MOVE_TYPE.DEFULT, is_Howl);
		if (element_type < ELEMENT_TYPE.MAX)
		{
			SetVisibleWidgetEffect(UI.SCR_LIST_2, t, UI.TEX_ENEMY_MODEL, EnemyLoader.GetElementEffectName(element_type));
		}
	}

	private IEnumerator LoadSkillModel(Transform t, Enum _enum, Enum _inner_enum, uint skill_id)
	{
		yield return null;
		SetRenderSkillItemModel(t, _enum, skill_id, rotation: false, light_rotation: true);
		SetRenderSkillItemSymbolModel(t, _inner_enum, skill_id, rotation: false);
	}

	private IEnumerator LoadNPCModel(Transform t, Enum main_npc_enum, Enum sub_npc_enum, GachaModelInfo.GachaDataInfo gacha_info)
	{
		yield return null;
		SetRenderNPCModel(t, main_npc_enum, 1, MonoBehaviourSingleton<OutGameSettingsManager>.I.shopScene.skillNPCPos, MonoBehaviourSingleton<OutGameSettingsManager>.I.shopScene.skillNPCRot, MonoBehaviourSingleton<OutGameSettingsManager>.I.shopScene.skillNPCFOV, delegate(NPCLoader loader)
		{
			gacha_info.npcLoader = loader;
			PlayerAnimCtrl.Get(loader.animator, PLCA.SKILL_GACHA_TOP);
		});
		SetRenderNPCModel(t, sub_npc_enum, 501, MonoBehaviourSingleton<OutGameSettingsManager>.I.shopScene.skillCatNPCPos, MonoBehaviourSingleton<OutGameSettingsManager>.I.shopScene.skillCatNPCRot, MonoBehaviourSingleton<OutGameSettingsManager>.I.shopScene.skillNPCFOV, delegate(NPCLoader loader)
		{
			PlayerAnimCtrl.Get(loader.animator, PLCA.LIE);
		});
	}

	private IEnumerator LoadSkillBanner(Transform t, Enum root_enum, Enum tex_enum, SkillItemTable.SkillItemData table, GachaModelInfo.GachaDataInfo gacha_info, PickUpSkill pickup)
	{
		if (loadQueue == null)
		{
			loadQueue = new LoadingQueue(this);
		}
		int pattern_index;
		switch (pickup.gachaAnim.pattern)
		{
		default:
			pattern_index = 0;
			break;
		case "B":
			pattern_index = 1;
			break;
		case "C":
			pattern_index = 2;
			break;
		case "D":
			pattern_index = 3;
			break;
		case "E":
			pattern_index = 4;
			break;
		}
		int id = (int)table.id;
		LoadObject lo_image = loadQueue.Load(isEventAsset: true, RESOURCE_CATEGORY.SHOP_IMG, ResourceName.GetSkillGachaBannerImage(id));
		if (loadQueue.IsLoading())
		{
			yield return loadQueue.Wait();
		}
		while (gacha_info.npcLoader == null)
		{
			yield return null;
		}
		Transform transform = FindCtrl(t, root_enum);
		if (!(transform != null))
		{
			yield break;
		}
		Transform transform2 = SetPrefab(transform, "GachaSkillBannerAnim");
		if (!(transform2 != null))
		{
			yield break;
		}
		GachaSkillBannerAnim component = transform2.GetComponent<GachaSkillBannerAnim>();
		if (component != null)
		{
			component.Init(pattern_index, table, lo_image.loadedObject as Texture, pickup.gachaAnim);
			if (gacha_info.pickup.Count == 1 || MonoBehaviourSingleton<GachaManager>.I.IsTutorialSkillGacha())
			{
				bool is_skip2 = gacha_info.isFirstSkillListDirection;
				component.Entry(is_skip2, delegate
				{
					PLCA default_anim2 = is_skip2 ? PLCA.SKILL_GACHA_TOP : PLCA.SKILL_GACHA_TOP_SLIDE_END;
					PlayerAnimCtrl.Get(gacha_info.npcLoader.animator, default_anim2);
				});
				gacha_info.isFirstSkillListDirection = false;
				component.WaitAndNextPickup(delegate
				{
					PlayerAnimCtrl.Get(gacha_info.npcLoader.animator, PLCA.SKILL_GACHA_TOP_SLIDE);
				});
			}
			else
			{
				bool is_skip = gacha_info.isFirstSkillListDirection;
				component.Entry(is_skip, delegate
				{
					PLCA default_anim = is_skip ? PLCA.SKILL_GACHA_TOP : PLCA.SKILL_GACHA_TOP_SLIDE_END;
					PlayerAnimCtrl.Get(gacha_info.npcLoader.animator, default_anim);
				});
				gacha_info.isFirstSkillListDirection = false;
				component.WaitAndNextPickup(delegate
				{
					PlayerAnimCtrl.Get(gacha_info.npcLoader.animator, PLCA.SKILL_GACHA_TOP_SLIDE);
				});
			}
		}
	}

	private IEnumerator LoadGachaBanner(Transform t, Enum _enum, string banner_img)
	{
		if (loadQueue == null)
		{
			loadQueue = new LoadingQueue(this);
		}
		LoadObject lo_image = loadQueue.Load(isEventAsset: true, RESOURCE_CATEGORY.GACHA_BANNER, banner_img);
		if (loadQueue.IsLoading())
		{
			yield return loadQueue.Wait();
		}
		if (lo_image.loadedObject != null)
		{
			SetTexture(t, _enum, lo_image.loadedObject as Texture);
		}
	}

	private IEnumerator LoadGachaGuaranteeCounter(Transform t, Enum _enum, int remainNum, string detailButtonImg = "")
	{
		if (loadQueue == null)
		{
			loadQueue = new LoadingQueue(this);
		}
		if (detailButtonImg == "")
		{
			detailButtonImg = "GGC_000000000";
		}
		LoadObject lo_image = loadQueue.Load(RESOURCE_CATEGORY.GACHA_GUARANTEE_COUNTER, detailButtonImg);
		if (loadQueue.IsLoading())
		{
			yield return loadQueue.Wait();
		}
		if (lo_image.loadedObject != null)
		{
			SetTexture(t, _enum, lo_image.loadedObject as Texture);
		}
	}

	private IEnumerator LoadGachaEventTitle(Transform t, Enum _enum, string eventTitleImg)
	{
		if (loadQueue == null)
		{
			loadQueue = new LoadingQueue(this);
		}
		LoadObject lo_image = loadQueue.Load(RESOURCE_CATEGORY.GACHA_EVENT_TITLE, eventTitleImg);
		if (loadQueue.IsLoading())
		{
			yield return loadQueue.Wait();
		}
		if (lo_image.loadedObject != null)
		{
			SetTexture(t, _enum, lo_image.loadedObject as Texture);
		}
	}

	private IEnumerator LoadGachaButton(Transform t, string buttonImg, string strItemNum, int gachaId, int gachaIndex)
	{
		if (loadQueue == null)
		{
			loadQueue = new LoadingQueue(this);
		}
		while (t.childCount != 0)
		{
			Transform child = t.GetChild(0);
			child.parent = null;
			child.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(child.gameObject);
		}
		LoadObject lo_button = loadQueue.Load(RESOURCE_CATEGORY.GACHA_BUTTON, buttonImg);
		if (loadQueue.IsLoading())
		{
			yield return loadQueue.Wait();
		}
		GameObject obj = UnityEngine.Object.Instantiate(lo_button.loadedObject) as GameObject;
		obj.transform.parent = t;
		obj.transform.name = UI.BTN_GACHA.ToString();
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
		SetLabelText(t, UI.TXT_GACHA_ITEM_NUM, strItemNum);
		int[] event_data = new int[2]
		{
			gachaId,
			gachaIndex
		};
		SetEvent(FindCtrl(t, UI.BTN_GACHA), "GACHA", event_data);
		RepositionTables();
	}

	private IEnumerator UpdateSkillList(int index, Transform t)
	{
		GachaModelInfo gachaModelInfo = this.gachaModelInfo[pageIndex];
		GachaModelInfo.GachaDataInfo gacha_info = gachaModelInfo.gachaDataInfo[index];
		int showPickupIndex = gacha_info.showPickupIndex;
		bool flag = false;
		Transform transform = GetCtrl(UI.SCR_LIST_2).Find("skill_tex_" + index);
		if (transform == null)
		{
			flag = true;
			transform = Realizes("GachaSkillListItem2", GetCtrl(UI.SCR_LIST_2));
			transform.name = "skill_tex_" + index;
		}
		transform.GetComponent<UIScrollOutSideObject>().SetTargetTransform(FindCtrl(t, UI.OBJ_SKILL_MODEL_ROOT));
		GetComponent<UIPanel>(UI.SCR_LIST_2).depth = GetComponent<UIPanel>(UI.SCR_LIST).depth - 1;
		PickUpSkill pickUpSkill = gacha_info.pickup[showPickupIndex] as PickUpSkill;
		SkillItemTable.SkillItemData skillItemData = Singleton<SkillItemTable>.I.GetSkillItemData(pickUpSkill.skillID);
		SetLabelText(t, UI.LBL_SKILL_ITEM_NAME, skillItemData.name);
		SetLabelText(t, UI.LBL_TYPE_NAME, MonoBehaviourSingleton<StatusManager>.I.GetSkillItemGroupString(skillItemData.type));
		SetSkillSlotTypeIcon(t, UI.SPR_EQUIP_TYPE_ICON, UI.SPR_RARITY_BG, UI.SPR_RARITY_ICON, skillItemData);
		Coroutine item = StartCoroutine(LoadNPCModel(transform, UI.TEX_SKILL_NPC_MODEL, UI.TEX_SKILL_SUB_NPC_MODEL, gacha_info));
		Coroutine item2 = StartCoroutine(LoadSkillBanner(transform, UI.OBJ_SKILL_BANNER, UI.TEX_SKILL_BANNER, skillItemData, gacha_info, pickUpSkill));
		coroutineList.Add(item);
		coroutineList.Add(item2);
		if (index > 0)
		{
			if (flag)
			{
				SetActive(t, UI.OBJ_SKILL_INFO_ROOT, is_visible: false);
			}
			yield return new WaitForSeconds(index);
			SetActive(t, UI.OBJ_SKILL_INFO_ROOT, is_visible: true);
		}
		if (string.IsNullOrEmpty(gacha_info.expireAt))
		{
			SetActive(t, UI.LBL_TIME, is_visible: false);
			yield break;
		}
		SetActive(t, UI.LBL_TIME, is_visible: true);
		FindCtrl(t, UI.LBL_TIME).GetComponent<UILabel>().text = GetTimeCountDown(gacha_info.expireAt);
	}

	private IEnumerator UpdateQuestList(int index, Transform t)
	{
		GachaModelInfo gachaModelInfo = this.gachaModelInfo[pageIndex];
		GachaModelInfo.GachaDataInfo gacha_info = gachaModelInfo.gachaDataInfo[index];
		int showPickupIndex = gacha_info.showPickupIndex;
		PickUpQuest pickUpQuest = gacha_info.pickup[showPickupIndex] as PickUpQuest;
		GetComponent<UIPanel>(UI.SCR_LIST_2).depth = GetComponent<UIPanel>(UI.SCR_LIST).depth - 1;
		bool flag = false;
		Transform enemy_tex_trans = GetCtrl(UI.SCR_LIST_2).Find("enemy_tex_" + index);
		if (enemy_tex_trans == null)
		{
			flag = true;
			enemy_tex_trans = Realizes("GachaListItem2", GetCtrl(UI.SCR_LIST_2));
			enemy_tex_trans.name = "enemy_tex_" + index;
		}
		enemy_tex_trans.GetComponent<UIScrollOutSideObject>().SetTargetTransform(t);
		string text = string.Empty;
		string text2 = string.Empty;
		uint enemy_id = 0u;
		string foundation_name = null;
		ELEMENT_TYPE element_type = ELEMENT_TYPE.MAX;
		RARITY_TYPE rARITY_TYPE = RARITY_TYPE.D;
		QuestTable.QuestTableData questData = Singleton<QuestTable>.I.GetQuestData(pickUpQuest.questID);
		if (questData != null)
		{
			EnemyTable.EnemyData enemyData = Singleton<EnemyTable>.I.GetEnemyData((uint)questData.GetMainEnemyID());
			if (enemyData != null)
			{
				enemy_id = enemyData.id;
				text = enemyData.name;
				element_type = enemyData.element;
				foundation_name = questData.GetFoundationName();
				text2 = questData.GetMainEnemyLv().ToString();
				rARITY_TYPE = questData.rarity;
			}
		}
		ClearRenderModel(enemy_tex_trans, UI.TEX_ENEMY_MODEL);
		Coroutine item = StartCoroutine(LoadEnemyModel(enemy_tex_trans, UI.TEX_ENEMY_MODEL, enemy_id, foundation_name, element_type, index == 0));
		coroutineList.Add(item);
		SetLabelText(enemy_tex_trans, UI.LBL_ENEMY, text);
		SetLabelText(enemy_tex_trans, UI.LBL_ENEMY_LV, text2);
		SetActive(enemy_tex_trans, UI.SPR_RARITY_SSS, rARITY_TYPE == RARITY_TYPE.SSS);
		SetActive(enemy_tex_trans, UI.SPR_RARITY_SS, rARITY_TYPE == RARITY_TYPE.SS);
		SetActive(enemy_tex_trans, UI.SPR_RARITY_S, rARITY_TYPE == RARITY_TYPE.S);
		SetActive(enemy_tex_trans, UI.SPR_RARITY_A, rARITY_TYPE == RARITY_TYPE.A);
		SetActive(enemy_tex_trans, UI.SPR_RARITY_B, rARITY_TYPE == RARITY_TYPE.B);
		SetActive(t, UI.OBJ_REWARD_ICON_ROOT, is_visible: true);
		if (pickUpQuest.materialID != 0)
		{
			int count = pickUpMaterialIDs.Count;
			pickUpMaterialIDs.Add(pickUpQuest.materialID);
			ItemTable.ItemData itemData = Singleton<ItemTable>.I.GetItemData(pickUpQuest.materialID);
			ItemIcon.Create(ITEM_ICON_TYPE.ITEM, itemData.iconID, itemData.rarity, FindCtrl(t, UI.OBJ_REWARD_ICON_ROOT), ELEMENT_TYPE.MAX, null, -1, "GACHA_EQUIP_LIST", count, is_new: false, -1, is_select: false, null, is_equipping: false, itemData.enemyIconID, itemData.enemyIconID2, disable_rarity_text: true);
			gacha_info.rewardIconMaterialInfoID = itemData.id;
		}
		SetActive(t, UI.OBJ_REWARD_ICON_ROOT, pickUpQuest.materialID != 0);
		UIVisibleWidgetShriken x = null;
		UIWidget widget = null;
		Transform transform = FindCtrl(t, UI.WGT_REWARD_EFFECT);
		if (transform != null)
		{
			x = transform.GetComponent<UIVisibleWidgetShriken>();
			widget = transform.GetComponent<UIWidget>();
		}
		if (x == null)
		{
			UIPanel component = GetComponent<UIPanel>(UI.SCR_LIST_2);
			if (component != null)
			{
				UIVisibleWidgetShriken.Set(component, widget);
			}
		}
		if (index >= 0)
		{
			if (flag)
			{
				SetActive(enemy_tex_trans, UI.OBJ_QUEST_INFO_ROOT, is_visible: false);
			}
			else
			{
				ItemIcon componentInChildren = t.GetComponentInChildren<ItemIcon>();
				if (componentInChildren != null && gacha_info.rewardIconMaterialInfoID != 0)
				{
					if (pickUpQuest.materialID != 0)
					{
						SetMaterialInfo(componentInChildren.transform, REWARD_TYPE.ITEM, Singleton<ItemTable>.I.GetItemData(pickUpQuest.materialID).id, GetCtrl(UI.SCR_LIST));
					}
					else
					{
						SetMaterialInfo(componentInChildren.transform, REWARD_TYPE.ITEM, gacha_info.rewardIconMaterialInfoID, GetCtrl(UI.SCR_LIST));
					}
				}
			}
			yield return new WaitForSeconds(index);
			SetActive(enemy_tex_trans, UI.OBJ_QUEST_INFO_ROOT, is_visible: true);
		}
		if (string.IsNullOrEmpty(gacha_info.expireAt))
		{
			SetActive(enemy_tex_trans, UI.LBL_TIME, is_visible: false);
			yield break;
		}
		SetActive(enemy_tex_trans, UI.LBL_TIME, is_visible: true);
		FindCtrl(enemy_tex_trans, UI.LBL_TIME).GetComponent<UILabel>().text = GetTimeCountDown(gacha_info.expireAt);
	}

	private void DestoryListChild()
	{
		GetCtrl(UI.TBL_LIST).DestroyChildren();
		GetCtrl(UI.SCR_LIST_2).DestroyChildren();
	}

	private void StopLoadCoroutine()
	{
		coroutineList.ForEach(delegate(Coroutine c)
		{
			if (c != null)
			{
				StopCoroutine(c);
			}
		});
		coroutineList.Clear();
	}

	public void OnQuery_MAGI_GACHA()
	{
		pageIndex = 1;
		ResetView();
	}

	public void OnQuery_QUEST_GACHA()
	{
		pageIndex = 0;
		ResetView();
	}

	private void ResetView()
	{
		MonoBehaviourSingleton<UIManager>.I.SetDisableMoment();
		StopLoadCoroutine();
		DeleteModel();
		DestoryListChild();
		UpdateShowIndex(is_reset: true);
		timer = 0f;
		gachaModelInfo[pageIndex].gachaDataInfo.ForEach(delegate(GachaModelInfo.GachaDataInfo data)
		{
			data.isFirstSkillListDirection = true;
			data.rewardIconMaterialInfoID = 0u;
		});
		ticketTitleRootList.Clear();
		SetDirty(UI.TBL_LIST);
		SetGachaListUI();
		RefreshUI();
	}

	public void OnQuery_GACHA_EQUIP_LIST()
	{
		int num = (int)GameSection.GetEventData();
		if (num >= pickUpMaterialIDs.Count || num <= -1)
		{
			GameSection.StopEvent();
			return;
		}
		uint num2 = pickUpMaterialIDs[num];
		GameSection.SetEventData(new object[1]
		{
			num2
		});
	}

	public void OnQuery_GACHA_EQUIP_LIST_FROM_NEWS()
	{
		object[] array = (object[])GameSection.GetEventData();
		_ = (int)array[0];
		int num = (int)array[1];
		uint num2 = 0u;
		List<GachaModelInfo.GachaDataInfo> gachaDataInfo = gachaModelInfo[0].gachaDataInfo;
		int i = 0;
		for (int count = gachaDataInfo.Count; i < count; i++)
		{
			GachaModelInfo.GachaDataInfo gachaDataInfo2 = gachaDataInfo[i];
			if (gachaDataInfo2 == null || gachaDataInfo2.pickup == null)
			{
				continue;
			}
			int j = 0;
			for (int count2 = gachaDataInfo2.pickup.Count; j < count2; j++)
			{
				PickUpQuest pickUpQuest = gachaDataInfo2.pickup[j] as PickUpQuest;
				if (pickUpQuest != null && pickUpQuest.questID == num)
				{
					num2 = pickUpQuest.materialID;
					break;
				}
			}
			if (num2 >= 1)
			{
				break;
			}
		}
		if (num2 == 0)
		{
			GameSection.ChangeEvent("EQUIP_NOT_EXIST");
			return;
		}
		int num3 = (int)array[2];
		if (num3 >= 0)
		{
			MonoBehaviourSingleton<GameSceneManager>.I.StopAutoEvent();
			EventData[] autoEvents = new EventData[1]
			{
				new EventData("GACHA_DETAIL_MAX_PARAM_FROM_NEWS", new object[2]
				{
					num2,
					num3
				})
			};
			MonoBehaviourSingleton<GameSceneManager>.I.SetAutoEvents(autoEvents);
		}
		GameSection.SetEventData(new object[1]
		{
			num2
		});
	}

	private void OnQuery_FORCE_ONCE_PURCHASE_GACHA()
	{
		string productId = GameSection.GetEventData() as string;
		GachaList.Gacha targetGacha = null;
		int targetIndex = 0;
		MonoBehaviourSingleton<GachaManager>.I.gachaData.types.ForEach(delegate(GachaList.GachaType type)
		{
			type.groups.ForEach(delegate(GachaList.GachaGroup gr)
			{
				GachaList.Gacha gacha = gr.gachas.Where((GachaList.Gacha g) => g.productId == productId).FirstOrDefault();
				if (gacha != null)
				{
					targetGacha = gacha;
					targetIndex = (from ano in gr.gachas.Select((GachaList.Gacha g, int j) => new
						{
							Content = g,
							Index = j
						})
						where ano.Content.gachaId == targetGacha.gachaId
						select ano.Index).First();
				}
			});
		});
		if (targetGacha != null)
		{
			MonoBehaviourSingleton<GachaManager>.I.SelectGacha(targetGacha.gachaId, targetIndex);
			DoGachaOrSendCanPurchaseable();
		}
	}

	private void OnQuery_GACHA()
	{
		int[] array = (int[])GameSection.GetEventData();
		if (array[0] < 0)
		{
			GameSection.StopEvent();
			return;
		}
		MonoBehaviourSingleton<GachaManager>.I.SelectGacha(array[0], array[1]);
		if (MonoBehaviourSingleton<UserInfoManager>.I.userStatus.IsTutorialBitReady && !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.GACHA1))
		{
			TutorialDoGacha();
			GameSection.ChangeEvent("GACHA_QUEST_TUTORIAL");
			return;
		}
		if (MonoBehaviourSingleton<GachaManager>.I.selectGacha.IsEnd)
		{
			RemoveEndDateModel();
			ResetView();
			GameSection.ChangeEvent("GACHA_END");
			return;
		}
		string text = "";
		if (MonoBehaviourSingleton<GachaManager>.I.selectGacha.requiredItemId > 0)
		{
			int ticketId = MonoBehaviourSingleton<GachaManager>.I.selectGacha.requiredItemId;
			int itemNum = MonoBehaviourSingleton<InventoryManager>.I.GetItemNum((ItemInfo x) => x.tableData.id == ticketId, 1);
			ItemTable.ItemData itemData = Singleton<ItemTable>.I.GetItemData((uint)ticketId);
			text = itemData.name + " " + MonoBehaviourSingleton<GachaManager>.I.selectGacha.needItemNum + StringTable.Get(STRING_CATEGORY.COMMON, 4000u) + "\n";
			if (MonoBehaviourSingleton<GachaManager>.I.selectGacha.needItemNum > itemNum)
			{
				object[] event_data = new object[2]
				{
					itemData.name,
					(MonoBehaviourSingleton<GachaManager>.I.selectGacha.needItemNum - itemNum).ToString() + StringTable.Get(STRING_CATEGORY.COMMON, 4000u)
				};
				GameSection.ChangeEvent("NOT_ENOUGH_GACHA_TICKET", event_data);
				return;
			}
		}
		else if (MonoBehaviourSingleton<GachaManager>.I.selectGacha.IsDirectPurchase())
		{
			selectProductId = MonoBehaviourSingleton<GachaManager>.I.selectGacha.productId;
			if (MonoBehaviourSingleton<GachaManager>.I.selectGacha.yenIncludeTax > 0f)
			{
				pp = string.Empty;
			}
			DoGachaOrSendCanPurchaseable();
		}
		else
		{
			text = StringTable.Get(STRING_CATEGORY.COMMON, 100u) + " " + MonoBehaviourSingleton<GachaManager>.I.selectGacha.crystalNum + StringTable.Get(STRING_CATEGORY.COMMON, 3000u);
		}
		GameSection.SetEventData(new object[1]
		{
			text
		});
	}

	protected void TutorialDoGacha()
	{
		if (GameSection.CheckCrystal((MonoBehaviourSingleton<GachaManager>.I.selectGacha.requiredItemId > 0) ? MonoBehaviourSingleton<GachaManager>.I.selectGacha.needItemNum : MonoBehaviourSingleton<GachaManager>.I.selectGacha.crystalNum, MonoBehaviourSingleton<GachaManager>.I.selectGacha.requiredItemId))
		{
			Protocol.Force(delegate
			{
				DoGacha(delegate(Error ret)
				{
					if (ret != 0 && ret != Error.ERR_CRYSTAL_NOT_ENOUGH)
					{
						GameSection.ResumeEvent(is_resume: false);
					}
				});
			});
		}
	}

	private void OnQuery_PP_TO_BUY()
	{
		pp = (GameSection.GetEventData() as string);
		GameSection.SetEventData(null);
		DoGachaOrSendCanPurchaseable();
	}

	private void OnQuery_ShopStopper_YES()
	{
		RequestEvent("STOPPER_TO_BUY");
	}

	private void OnQuery_STOPPER_TO_BUY()
	{
		if (MonoBehaviourSingleton<GachaManager>.I.selectGachaType == GACHA_TYPE.QUEST)
		{
			GameSection.ChangeEvent("BUY_QUEST_GACHA");
		}
		else
		{
			GameSection.ChangeEvent("BUY_SKILL_GACHA");
		}
		GameSection.StayEvent();
		DoPurchase();
	}

	private void OnQuery_DETAIL_PROMOTION()
	{
		GameSection.StayEvent();
		MonoBehaviourSingleton<FriendManager>.I.SendGetFollowLink(delegate(bool is_success)
		{
			GameSection.ResumeEvent(is_success);
		});
	}

	private void DoGachaOrSendCanPurchaseable()
	{
		if (MonoBehaviourSingleton<GachaManager>.I.selectGachaType == GACHA_TYPE.QUEST)
		{
			GameSection.ChangeEvent("BUY_QUEST_GACHA");
		}
		else
		{
			GameSection.ChangeEvent("BUY_SKILL_GACHA");
		}
		GameSection.StayEvent();
		DoGacha(delegate(Error ret)
		{
			if (ret == Error.None)
			{
				if (MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult().oncePurchaseItemToShop != null && !string.IsNullOrEmpty(MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult().oncePurchaseItemToShop.productId))
				{
					SendGachaCanPurchase();
				}
				else
				{
					GameSection.ResumeEvent(is_resume: true);
				}
			}
			else
			{
				GameSection.ResumeEvent(is_resume: false);
			}
		});
	}

	private void SendGachaCanPurchase()
	{
		MonoBehaviourSingleton<ShopManager>.I.SendGoldCanPurchase(selectProductId, pp, delegate(Error ret)
		{
			switch (ret)
			{
			case Error.WRN_GOLD_OVER_LIMITTER_OVERUSE:
				GameSection.ChangeStayEvent("STOPPER");
				GameSection.ResumeEvent(is_resume: true);
				break;
			default:
				GameSection.ResumeEvent(is_resume: false);
				break;
			case Error.None:
				DoPurchase();
				break;
			}
		});
	}

	private void DoPurchase()
	{
		isPurchase = true;
		Native.RequestPurchase(selectProductId, MonoBehaviourSingleton<UserInfoManager>.I.userInfo.id.ToString(), MonoBehaviourSingleton<UserInfoManager>.I.userIdHash);
	}

	private void OnBuyItem(string productId)
	{
		if (isDoGacha)
		{
			return;
		}
		if (!string.IsNullOrEmpty(productId))
		{
			ProductData product_data = null;
			MonoBehaviourSingleton<GachaManager>.I.gachaData.types.ForEach(delegate(GachaList.GachaType type)
			{
				type.groups.ForEach(delegate(GachaList.GachaGroup gr)
				{
					IEnumerable<GachaList.Gacha> source = gr.gachas.Where((GachaList.Gacha g) => g.productId == productId);
					if (source.Count() > 0)
					{
						GachaList.Gacha gacha = source.First();
						product_data = new ProductData
						{
							productId = productId,
							price = gacha.yen,
							crystalNum = gacha.crystalNum
						};
					}
				});
			});
			if (product_data != null)
			{
				isDoGacha = true;
				DoGacha(delegate(Error ret)
				{
					isPurchase = false;
					isDoGacha = false;
					if (ret == Error.None)
					{
						GameSection.ResumeEvent(is_resume: true);
					}
					else
					{
						GameSection.ResumeEvent(is_resume: false);
					}
				});
			}
			else
			{
				Action onFinish = delegate
				{
					isPurchase = false;
					GameSection.ResumeEvent(is_resume: false);
				};
				SendRequestCurrentCrystal(onFinish);
			}
		}
		else if (isPurchase)
		{
			GameSection.ResumeEvent(is_resume: false);
		}
	}

	private void OnBuyGoPayItem(GoPayDepositModel ret, Purchase purchase)
	{
		OnBuyItem(ret.result.productId);
	}

	private void SendRequestCurrentCrystal(Action onFinish)
	{
		Protocol.Send(OnceStatusInfoModel.URL, delegate(OnceStatusInfoModel result)
		{
			CheckCrystalNum(result, onFinish);
		});
	}

	private void CheckCrystalNum(OnceStatusInfoModel ret, Action onFinish)
	{
		if (ret.Error == Error.None)
		{
			MonoBehaviourSingleton<UserInfoManager>.I.userStatus.crystal = ret.result.userStatus.crystal;
			MonoBehaviourSingleton<UserInfoManager>.I.DirtyUserStatus();
			onFinish();
		}
	}

	private void OnQuery_GACHA_BANNER()
	{
		GachaModelInfo.GachaDataInfo gachaDataInfo = GameSection.GetEventData() as GachaModelInfo.GachaDataInfo;
		if (string.IsNullOrEmpty(gachaDataInfo.bannerImg) || string.IsNullOrEmpty(gachaDataInfo.url))
		{
			GameSection.StopEvent();
		}
		GameSection.SetEventData(gachaDataInfo.url);
	}

	private void Update()
	{
		if (base.state == STATE.OPEN)
		{
			if (timer < 5f)
			{
				timer += Time.deltaTime;
			}
			if (!(MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSectionName() != "ShopTop") && timer >= 5f)
			{
				timer = 0f;
				UpdateShowIndex();
				RefreshUI();
			}
		}
	}

	private string GetTimeCountDown(string endTime)
	{
		if (string.IsNullOrEmpty(endTime))
		{
			return null;
		}
		DateTime now = TimeManager.GetNow();
		DateTime dateTime = DateTime.Parse(endTime);
		if (dateTime.CompareTo(now) < 0)
		{
			return StringTable.Get(STRING_CATEGORY.TEXT_SCRIPT, 11u);
		}
		TimeSpan timeSpan = dateTime.Subtract(now);
		StringBuilder stringBuilder = new StringBuilder();
		if (timeSpan.Days > 0)
		{
			stringBuilder.Append(string.Format(StringTable.Get(STRING_CATEGORY.TEXT_SCRIPT, 9u), timeSpan.Days));
		}
		else if (timeSpan.Hours > 0)
		{
			stringBuilder.Append(string.Format(StringTable.Get(STRING_CATEGORY.TEXT_SCRIPT, 10u), timeSpan.Hours));
		}
		else
		{
			stringBuilder.Append(timeSpan.Minutes + " minutes");
		}
		stringBuilder.Append(" " + StringTable.Get(STRING_CATEGORY.TEXT_SCRIPT, 12u));
		return stringBuilder.ToString();
	}

	private void UpdateShowIndex(bool is_reset = false)
	{
		if (gachaModelInfo != null && gachaModelInfo.Count != 0)
		{
			gachaModelInfo.ForEach(delegate(GachaModelInfo types)
			{
				if (types != null && types.gachaDataInfo != null && types.gachaDataInfo.Count != 0)
				{
					types.gachaDataInfo.ForEach(delegate(GachaModelInfo.GachaDataInfo groups)
					{
						if (!is_reset)
						{
							groups.showPickupIndex++;
							if (groups.showPickupIndex >= groups.pickup.Count)
							{
								groups.showPickupIndex = 0;
							}
						}
						else
						{
							groups.showPickupIndex = 0;
						}
					});
				}
			});
			DeleteMaterialInfo();
		}
	}

	private void OnQuery_CURRENCY()
	{
		GameSection.ChangeEvent("INFO", WebViewManager.Currency);
	}

	private void OnQuery_COMMERCIAL()
	{
		GameSection.ChangeEvent("INFO", WebViewManager.Commercial);
	}

	private void OnQuery_FUND()
	{
		GameSection.ChangeEvent("INFO", WebViewManager.Found);
	}

	private void OnQuery_PROBABILITY()
	{
		GameSection.ChangeEvent("INFO", gachaModelInfo[pageIndex].url);
	}

	private void RemoveEndDateModel()
	{
		foreach (GachaModelInfo item in gachaModelInfo)
		{
			foreach (GachaModelInfo.GachaDataInfo item2 in item.gachaDataInfo)
			{
				item2.gachas.RemoveAll((GachaList.Gacha x) => x.IsEnd);
			}
			item.gachaDataInfo.RemoveAll((GachaModelInfo.GachaDataInfo x) => x.gachas.Count == 0);
		}
	}

	private void OnQuery_GUARANTEE_GACHA_DETAIL()
	{
		string text = (string)GameSection.GetEventData();
		if (text == "")
		{
			GameSection.StopEvent();
		}
		else
		{
			GameSection.SetEventData(text);
		}
	}

	protected override void OnDestroy()
	{
		if (MonoBehaviourSingleton<ShopReceiver>.IsValid())
		{
			ShopReceiver i = MonoBehaviourSingleton<ShopReceiver>.I;
			i.onBuyGacha = (Action<string>)Delegate.Remove(i.onBuyGacha, new Action<string>(OnBuyItem));
			ShopReceiver i2 = MonoBehaviourSingleton<ShopReceiver>.I;
			i2.onBuyItem = (Action<string>)Delegate.Remove(i2.onBuyItem, new Action<string>(OnBuyItem));
			ShopReceiver i3 = MonoBehaviourSingleton<ShopReceiver>.I;
			i3.onGetProductDatas = (Action<StoreDataList>)Delegate.Remove(i3.onGetProductDatas, new Action<StoreDataList>(OnGetProductDatas));
		}
		base.OnDestroy();
	}

	private void OnGetProductDatas(StoreDataList list)
	{
		_isFinishGetNativeProductlist = true;
		_nativeStoreList = list;
	}
}
