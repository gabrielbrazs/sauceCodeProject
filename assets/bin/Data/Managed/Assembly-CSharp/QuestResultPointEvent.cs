using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestResultPointEvent : GameSection
{
	public enum UI
	{
		TXT_BANNER,
		OBJ_GET_EXP_ROOT,
		OBJ_TOTAL_EXP_ROOT,
		OBJ_NEXT_REWARD_ROOT,
		OBJ_GET_REWARD_ROOT,
		LBL_GET_POINT,
		LBL_TOTAL_POINT,
		LBL_NEXT_POINT,
		OBJ_NEXT_REWARD_ITEM_ICON_ROOT,
		GRD_ANIM_ITEM_ROOT,
		LBL_NEXT_ITEM_NUM,
		BTN_SKIP_FULL_SCREEN,
		BTN_OK,
		SPR_LOGO,
		SPR_LOGO_BG,
		OBJ_REWARD_POS,
		OBJ_REWARD_ROOT,
		OBJ_RANKING_ROOT,
		OBJ_ENEMY,
		LBL_QUEST_NAME,
		LBL_MONSTER_POINT,
		OBJ_BONUS1_ROOT,
		LBL_BONUS1_NAME,
		LBL_BONUS1_POINT,
		OBJ_BONUS2_ROOT,
		LBL_BONUS2_NAME,
		LBL_BONUS2_POINT,
		OBJ_BONUS3_ROOT,
		LBL_BONUS3_NAME,
		LBL_BONUS3_POINT,
		OBJ_LINE1,
		OBJ_LINE2,
		OBJ_LINE3,
		OBJ_LINE4,
		OBJ_ITEM_ROOT,
		OBJ_RANKING_ANIM_ROOT,
		OBJ_PASS_ANIM_ROOT,
		LBL_RANKING_TOTAL_POINT,
		LBL_PASS_NUM,
		BTN_END_OK,
		GRD_POSITION_PASS,
		SPR_POSITION_PASS,
		SPR_PASS_TEXT,
		GRD_COUNT_NUMBERS,
		OBJ_BONUS_TIME_ROOT,
		LBL_BONUS_RATE,
		OBJ_BONUS_ROOT,
		OBJ_BONUS_ANIM_ROOT,
		OBJ_POINT_DETAIL
	}

	private enum State
	{
		START,
		TO_REWARD,
		REWARD,
		BONUS,
		RANKING
	}

	private enum AUDIO
	{
		RESULT = 40000049,
		TITLE_LOGO = 40000227,
		CATEGORY = 40000228,
		POINTUP = 40000229,
		POINTREWARD = 40000230,
		START_BONUS_TIME = 40000268
	}

	private const string BOOST_BONUS_NAME = "ブ\u30fcストポイント";

	private const string TITLE_LOGO_NAME_FORMAT = "ef_ui_pointresult";

	private const float COUNT_ANIM_SPEED = 4f;

	private const float FADE_SPEED = 5f;

	private State pointResultState;

	private bool stateInitialized;

	private PointEventCurrentData currentData;

	private int boostPoint;

	private float boostRate;

	private UI[] bonusNames = new UI[3]
	{
		UI.LBL_BONUS1_NAME,
		UI.LBL_BONUS2_NAME,
		UI.LBL_BONUS3_NAME
	};

	private UI[] bonusPoints = new UI[3]
	{
		UI.LBL_BONUS1_POINT,
		UI.LBL_BONUS2_POINT,
		UI.LBL_BONUS3_POINT
	};

	private UI[] bonusRoots = new UI[3]
	{
		UI.OBJ_BONUS1_ROOT,
		UI.OBJ_BONUS2_ROOT,
		UI.OBJ_BONUS3_ROOT
	};

	private UI[] bonusLines = new UI[3]
	{
		UI.OBJ_LINE2,
		UI.OBJ_LINE3,
		UI.OBJ_LINE4
	};

	private List<PointEventCurrentData.Reward> rewardList = new List<PointEventCurrentData.Reward>();

	private List<GameObject> rewardObjects = new List<GameObject>();

	private bool skipRequest;

	private Transform bannerCtrl;

	private MeshRenderer[] bannerMeshRenderers;

	private readonly string[] SPR_RANKING_NUMBER = new string[10]
	{
		"RankingNumber_0",
		"RankingNumber_1",
		"RankingNumber_2",
		"RankingNumber_3",
		"RankingNumber_4",
		"RankingNumber_5",
		"RankingNumber_6",
		"RankingNumber_7",
		"RankingNumber_8",
		"RankingNumber_9"
	};

	private readonly string[] SPR_PASS_NUMBER = new string[10]
	{
		"RankingNumber2_0",
		"RankingNumber2_1",
		"RankingNumber2_2",
		"RankingNumber2_3",
		"RankingNumber2_4",
		"RankingNumber2_5",
		"RankingNumber2_6",
		"RankingNumber2_7",
		"RankingNumber2_8",
		"RankingNumber2_9"
	};

	private List<GameObject> rankingNumbers;

	private List<GameObject> passPositionNumbers;

	private int beforeDigits;

	public override void Initialize()
	{
		currentData = (GameSection.GetEventData() as PointEventCurrentData);
		SetActive(UI.OBJ_REWARD_ROOT, false);
		SetActive(UI.OBJ_RANKING_ROOT, false);
		SetActive(UI.OBJ_BONUS_ROOT, false);
		bannerCtrl = GetCtrl(UI.TXT_BANNER);
		StartCoroutine(DoInitalize());
	}

	private IEnumerator DoInitalize()
	{
		LoadingQueue loadQueue = new LoadingQueue(this);
		loadQueue.CacheEffect(RESOURCE_CATEGORY.EFFECT_UI, "ef_ui_pointresult");
		if (loadQueue.IsLoading())
		{
			yield return (object)loadQueue.Wait();
		}
		SetVisibleWidgetEffect(UI.TXT_BANNER, "ef_ui_pointresult");
		UIVisibleWidgetEffect visibleWidget = bannerCtrl.GetComponent<UIVisibleWidgetEffect>();
		visibleWidget.SetRendererQueue(4000);
		PlayAudio(AUDIO.TITLE_LOGO);
		ResourceLoad.LoadEventBannerResultTexture(GetCtrl(UI.SPR_LOGO).GetComponent<UITexture>(), (uint)currentData.eventId);
		ResourceLoad.LoadEventBannerResultBGTexture(GetCtrl(UI.SPR_LOGO_BG).GetComponent<UITexture>(), (uint)currentData.eventId);
		TweenAlpha logoBGAlpha = GetCtrl(UI.SPR_LOGO_BG).GetComponent<TweenAlpha>();
		if ((Object)logoBGAlpha != (Object)null)
		{
			logoBGAlpha.ResetToBeginning();
			logoBGAlpha.PlayForward();
		}
		base.Initialize();
	}

	private void Update()
	{
		switch (pointResultState)
		{
		case State.START:
		{
			Animation componentInChildren = bannerCtrl.GetComponentInChildren<Animation>(true);
			if (!((Object)componentInChildren == (Object)null) && !componentInChildren.isPlaying)
			{
				pointResultState = State.TO_REWARD;
			}
			break;
		}
		case State.TO_REWARD:
		{
			if (!stateInitialized)
			{
				bannerMeshRenderers = bannerCtrl.GetComponentsInChildren<MeshRenderer>();
				stateInitialized = true;
			}
			int num = 0;
			for (int i = 0; i < bannerMeshRenderers.Length; i++)
			{
				Color color = bannerMeshRenderers[i].material.color;
				color.a = Mathf.Max(0f, color.a - 5f * Time.deltaTime);
				bannerMeshRenderers[i].material.color = color;
				Color color2 = bannerMeshRenderers[i].material.color;
				if (color2.a <= 0f)
				{
					num++;
				}
				if (i == bannerMeshRenderers.Length - 1)
				{
					if (num == bannerMeshRenderers.Length)
					{
						bannerCtrl.position = GetCtrl(UI.OBJ_REWARD_POS).position;
						bannerCtrl.localScale = GetCtrl(UI.OBJ_REWARD_POS).localScale;
						SetActive(UI.SPR_LOGO_BG, false);
						StartCoroutine(WaitTiming(2f));
						pointResultState = State.REWARD;
						stateInitialized = false;
					}
					else
					{
						num = 0;
					}
				}
			}
			break;
		}
		case State.REWARD:
			if (!stateInitialized)
			{
				int num2 = 0;
				for (int j = 0; j < bannerMeshRenderers.Length; j++)
				{
					Color color3 = bannerMeshRenderers[j].material.color;
					color3.a = Mathf.Min(1f, color3.a + 5f * Time.deltaTime);
					bannerMeshRenderers[j].material.color = color3;
					Color color4 = bannerMeshRenderers[j].material.color;
					if (color4.a >= 1f)
					{
						num2++;
					}
					if (j == bannerMeshRenderers.Length - 1)
					{
						if (num2 == bannerMeshRenderers.Length)
						{
							SetActive(UI.OBJ_REWARD_ROOT, true);
							stateInitialized = true;
							SetRewardUI();
						}
						else
						{
							num2 = 0;
						}
					}
				}
			}
			break;
		case State.BONUS:
			if (!stateInitialized)
			{
				SetActive(UI.OBJ_REWARD_ROOT, false);
				SetActive(UI.OBJ_BONUS_ROOT, true);
				SetActive(UI.TXT_BANNER, false);
				SetBonusUI();
				stateInitialized = true;
			}
			break;
		case State.RANKING:
			if (!stateInitialized)
			{
				SetActive(UI.OBJ_REWARD_ROOT, false);
				SetActive(UI.OBJ_BONUS_ROOT, false);
				SetActive(UI.OBJ_RANKING_ROOT, true);
				SetActive(UI.TXT_BANNER, false);
				SetRankingUI();
				stateInitialized = true;
			}
			break;
		}
	}

	private void SetRewardUI()
	{
		SetFullScreenButton(UI.BTN_SKIP_FULL_SCREEN);
		SetActive(UI.BTN_OK, false);
		InitTween(UI.OBJ_GET_EXP_ROOT);
		InitTween(UI.OBJ_TOTAL_EXP_ROOT);
		InitTween(UI.OBJ_NEXT_REWARD_ROOT);
		InitTween(UI.OBJ_GET_REWARD_ROOT);
		InitTween(UI.OBJ_BONUS_TIME_ROOT);
		PointEventCurrentData.PointResultData pointRankingData = currentData.pointRankingData;
		if (MonoBehaviourSingleton<GuildRequestManager>.IsValid() && MonoBehaviourSingleton<GuildRequestManager>.I.isCompleteMulti)
		{
			Transform ctrl = GetCtrl(UI.OBJ_POINT_DETAIL);
			foreach (Transform item in ctrl)
			{
				item.gameObject.SetActive(false);
			}
		}
		else
		{
			SetLabelText(UI.LBL_QUEST_NAME, MonoBehaviourSingleton<QuestManager>.I.GetCurrentQuestName());
			EnemyTable.EnemyData enemyData = Singleton<EnemyTable>.I.GetEnemyData((uint)MonoBehaviourSingleton<QuestManager>.I.GetCurrentQuestEnemyID(0));
			ItemIcon.Create(ITEM_ICON_TYPE.QUEST_ITEM, enemyData.iconId, null, GetCtrl(UI.OBJ_ENEMY), ELEMENT_TYPE.MAX, null, -1, null, 0, false, -1, false, null, false, 0, 0, false, GET_TYPE.PAY, ELEMENT_TYPE.MAX);
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < pointRankingData.bonusPoint.Count; i++)
		{
			num += pointRankingData.bonusPoint[i].point;
			if (!MonoBehaviourSingleton<GuildRequestManager>.IsValid() || !MonoBehaviourSingleton<GuildRequestManager>.I.isCompleteMulti)
			{
				if (pointRankingData.bonusPoint[i].name != "ブ\u30fcストポイント" && pointRankingData.bonusPoint[i].point > 0)
				{
					SetBonusPoint(num2, pointRankingData.bonusPoint[i].name, pointRankingData.bonusPoint[i].point);
					num2++;
				}
				else
				{
					boostPoint = pointRankingData.bonusPoint[i].point;
					boostRate = pointRankingData.bonusPoint[i].boostRate;
				}
			}
		}
		MonoBehaviourSingleton<GuildRequestManager>.I.isCompleteMulti = false;
		SetLabelText(UI.LBL_MONSTER_POINT, "+" + (pointRankingData.getPoint - num).ToString("N0"));
		SetLabelText(UI.LBL_GET_POINT, pointRankingData.getPoint.ToString("N0"));
		SetLabelText(UI.LBL_TOTAL_POINT, (pointRankingData.userPoint + pointRankingData.getPoint).ToString("N0"));
		if (pointRankingData.nextReward != null)
		{
			SetLabelText(UI.LBL_NEXT_POINT, pointRankingData.nextReward.point.ToString());
			SetLabelText(UI.LBL_NEXT_ITEM_NUM, pointRankingData.nextReward.reward[0].num.ToString("N0"));
		}
		StartCoroutine(GetPointAnimation());
	}

	private IEnumerator GetPointAnimation()
	{
		int getPoint = currentData.pointRankingData.getPoint;
		int userPoint = currentData.pointRankingData.userPoint;
		int totalPoint = userPoint + getPoint;
		PlayAudio(AUDIO.CATEGORY);
		bool wait6 = true;
		SetLabelText(UI.LBL_GET_POINT, "0");
		PlayTween(UI.OBJ_GET_EXP_ROOT, true, delegate
		{
			((_003CGetPointAnimation_003Ec__Iterator139)/*Error near IL_00db: stateMachine*/)._003Cwait_003E__3 = false;
		}, true, 0);
		while (wait6)
		{
			if (skipRequest)
			{
				SkipTween(UI.OBJ_GET_EXP_ROOT, true, 0);
				wait6 = false;
			}
			yield return (object)0;
		}
		yield return (object)StartCoroutine(CountUpAnimation(0f, getPoint - boostPoint, UI.LBL_GET_POINT));
		if (boostPoint > 0)
		{
			SetLabelText(UI.LBL_BONUS_RATE, "x" + boostRate.ToString());
			wait6 = true;
			PlayTween(UI.OBJ_BONUS_TIME_ROOT, true, delegate
			{
				((_003CGetPointAnimation_003Ec__Iterator139)/*Error near IL_01d2: stateMachine*/)._003Cwait_003E__3 = false;
			}, true, 0);
			while (wait6)
			{
				if (skipRequest)
				{
					SkipTween(UI.OBJ_BONUS_TIME_ROOT, true, 0);
					wait6 = false;
				}
				yield return (object)0;
			}
			PlayAudio(AUDIO.RESULT);
			if (!skipRequest)
			{
				yield return (object)StartCoroutine(WaitTiming(1.2f));
			}
			yield return (object)StartCoroutine(CountUpAnimation((float)(getPoint - boostPoint), getPoint, UI.LBL_GET_POINT));
		}
		else
		{
			SetActive(UI.OBJ_BONUS_TIME_ROOT, false);
		}
		PlayAudio(AUDIO.CATEGORY);
		wait6 = true;
		SetLabelText(UI.LBL_TOTAL_POINT, userPoint.ToString("N0"));
		PlayTween(UI.OBJ_TOTAL_EXP_ROOT, true, delegate
		{
			((_003CGetPointAnimation_003Ec__Iterator139)/*Error near IL_0324: stateMachine*/)._003Cwait_003E__3 = false;
		}, true, 0);
		while (wait6)
		{
			if (skipRequest)
			{
				SkipTween(UI.OBJ_TOTAL_EXP_ROOT, true, 0);
				wait6 = false;
			}
			yield return (object)0;
		}
		yield return (object)StartCoroutine(CountUpAnimation((float)userPoint, totalPoint, UI.LBL_TOTAL_POINT));
		int lastGetRewardsPoint = 0;
		if (currentData.pointRankingData.getReward.Count > 0)
		{
			for (int m = 0; m < currentData.pointRankingData.getReward.Count; m++)
			{
				PointEventCurrentData.PointRewardData currentNextData = currentData.pointRankingData.getReward[m];
				for (int j = 0; j < currentNextData.reward.Count; j++)
				{
					rewardList.Add(currentNextData.reward[j]);
				}
			}
			if (rewardList.Count > 0)
			{
				SetAllRewardItem(rewardList);
				for (int l = 0; l < rewardObjects.Count; l++)
				{
					rewardObjects[l].SetActive(false);
				}
			}
			for (int k = 0; k < currentData.pointRankingData.getReward.Count; k++)
			{
				PointEventCurrentData.PointRewardData currentNextData = currentData.pointRankingData.getReward[k];
				SetNextItemIcon(currentNextData.reward);
				wait6 = true;
				ResetTween(UI.OBJ_NEXT_REWARD_ROOT, 0);
				int currentPoint = currentNextData.point - lastGetRewardsPoint;
				SetLabelText(UI.LBL_NEXT_POINT, currentPoint.ToString("N0"));
				PlayTween(UI.OBJ_NEXT_REWARD_ROOT, true, delegate
				{
					((_003CGetPointAnimation_003Ec__Iterator139)/*Error near IL_05ca: stateMachine*/)._003Cwait_003E__3 = false;
				}, true, 0);
				if (!skipRequest)
				{
					PlayAudio(AUDIO.CATEGORY);
				}
				while (wait6)
				{
					if (skipRequest)
					{
						SkipTween(UI.OBJ_NEXT_REWARD_ROOT, true, 0);
						wait6 = false;
					}
					yield return (object)0;
				}
				yield return (object)StartCoroutine(CountDownAnimation((float)currentPoint, 0, UI.LBL_NEXT_POINT));
				if (!skipRequest)
				{
					yield return (object)StartCoroutine(WaitTiming(0.5f));
				}
				lastGetRewardsPoint = currentNextData.point;
			}
		}
		if (currentData.pointRankingData.nextReward != null)
		{
			SetNextItemIcon(currentData.pointRankingData.nextReward.reward);
			wait6 = true;
			ResetTween(UI.OBJ_NEXT_REWARD_ROOT, 0);
			int currentPoint2 = currentData.pointRankingData.nextReward.point - lastGetRewardsPoint;
			SetLabelText(UI.LBL_NEXT_POINT, currentPoint2.ToString("N0"));
			PlayTween(UI.OBJ_NEXT_REWARD_ROOT, true, delegate
			{
				((_003CGetPointAnimation_003Ec__Iterator139)/*Error near IL_07b0: stateMachine*/)._003Cwait_003E__3 = false;
			}, true, 0);
			PlayAudio(AUDIO.CATEGORY);
			while (wait6)
			{
				if (skipRequest)
				{
					SkipTween(UI.OBJ_NEXT_REWARD_ROOT, true, 0);
					wait6 = false;
				}
				yield return (object)0;
			}
			yield return (object)StartCoroutine(CountDownAnimation(targetPoint: currentData.pointRankingData.nextReward.point - totalPoint, currentPoint: (float)currentPoint2, targetUI: UI.LBL_NEXT_POINT));
		}
		else
		{
			SetActive(UI.OBJ_NEXT_REWARD_ROOT, false);
		}
		if (rewardObjects.Count > 0)
		{
			wait6 = true;
			ResetTween(UI.OBJ_GET_REWARD_ROOT, 0);
			PlayTween(UI.OBJ_GET_REWARD_ROOT, true, delegate
			{
				((_003CGetPointAnimation_003Ec__Iterator139)/*Error near IL_08d7: stateMachine*/)._003Cwait_003E__3 = false;
			}, true, 0);
			PlayAudio(AUDIO.POINTREWARD);
			while (wait6)
			{
				if (skipRequest)
				{
					SkipTween(UI.OBJ_GET_REWARD_ROOT, true, 0);
					wait6 = false;
				}
				yield return (object)0;
			}
			for (int i = 0; i < rewardObjects.Count; i++)
			{
				rewardObjects[i].SetActive(true);
				TweenAlpha alpha = rewardObjects[i].GetComponent<TweenAlpha>();
				if (!((Object)alpha == (Object)null))
				{
					alpha.ResetToBeginning();
					alpha.PlayForward();
				}
			}
		}
		SetActive(UI.BTN_OK, true);
		SetActive(UI.BTN_SKIP_FULL_SCREEN, false);
	}

	private void SetNextItemIcon(List<PointEventCurrentData.Reward> reward)
	{
		SetDynamicList(UI.OBJ_NEXT_REWARD_ITEM_ICON_ROOT, "ItemIcon", reward.Count, true, null, null, delegate(int i, Transform t, bool is_recycle)
		{
			PointEventCurrentData.Reward reward2 = reward[i];
			ItemIcon.CreateRewardItemIcon((REWARD_TYPE)reward2.type, (uint)reward2.itemId, t, reward2.num, null, 0, false, -1, false, null, false, false, ItemIcon.QUEST_ICON_SIZE_TYPE.DEFAULT);
		});
	}

	private void SetAllRewardItem(List<PointEventCurrentData.Reward> rewardList)
	{
		Transform ctrl = GetCtrl(UI.OBJ_ITEM_ROOT);
		for (int j = 0; j < rewardList.Count; j++)
		{
			if (j == 0)
			{
				Transform transform = Utility.FindChild(ctrl, "itemNum");
				transform.GetComponent<UILabel>().text = "×" + rewardList[j].num;
				rewardObjects.Add(ctrl.gameObject);
			}
			else
			{
				GameObject gameObject = Object.Instantiate(ctrl.gameObject);
				gameObject.transform.parent = ctrl.parent;
				gameObject.transform.localPosition = ctrl.localPosition;
				gameObject.transform.localScale = ctrl.localScale;
				Transform transform2 = Utility.FindChild(gameObject.transform, "itemNum");
				transform2.GetComponent<UILabel>().text = "×" + rewardList[j].num;
				rewardObjects.Add(gameObject);
			}
		}
		SetDynamicList(UI.GRD_ANIM_ITEM_ROOT, "ItemIcon", rewardList.Count, true, null, null, delegate(int i, Transform t, bool is_recycle)
		{
			PointEventCurrentData.Reward reward = rewardList[i];
			ItemIcon itemIcon = ItemIcon.CreateRewardItemIcon((REWARD_TYPE)reward.type, (uint)reward.itemId, t.parent, reward.num, null, 0, false, -1, false, null, false, false, ItemIcon.QUEST_ICON_SIZE_TYPE.DEFAULT);
			if ((Object)itemIcon != (Object)null)
			{
				itemIcon.SetEnableCollider(false);
			}
		});
	}

	private void SetBonusPoint(int bonusIndex, string bonusName, int point)
	{
		if (bonusIndex <= 2)
		{
			SetLabelText(bonusNames[bonusIndex], bonusName);
			SetLabelText(bonusPoints[bonusIndex], "+" + point.ToString("N0"));
			SetActive(bonusRoots[bonusIndex], true);
			SetActive(bonusLines[bonusIndex], true);
		}
	}

	private IEnumerator CountUpAnimation(float currentPoint, int targetPoint, UI targetUI)
	{
		while (currentPoint < (float)targetPoint)
		{
			yield return (object)0;
			if (skipRequest)
			{
				currentPoint = (float)targetPoint;
			}
			int before = Mathf.FloorToInt(currentPoint);
			float addingPoint = Mathf.Max(((float)targetPoint - currentPoint) * CountDownCube(Time.deltaTime * 4f), 1f);
			currentPoint += addingPoint;
			currentPoint = Mathf.Min(currentPoint, (float)targetPoint);
			if (before < Mathf.FloorToInt(currentPoint))
			{
				PlayAudio(AUDIO.POINTUP);
			}
			SetLabelText(targetUI, Mathf.FloorToInt(currentPoint).ToString("N0"));
		}
	}

	private IEnumerator CountDownAnimation(float currentPoint, int targetPoint, UI targetUI)
	{
		while (currentPoint > (float)targetPoint)
		{
			yield return (object)0;
			if (skipRequest)
			{
				currentPoint = (float)targetPoint;
			}
			int before = Mathf.FloorToInt(currentPoint);
			float addingPoint = Mathf.Min(((float)targetPoint - currentPoint) * CountDownCube(Time.deltaTime * 4f), -1f);
			currentPoint += addingPoint;
			currentPoint = Mathf.Max(currentPoint, (float)targetPoint);
			if (before > Mathf.FloorToInt(currentPoint))
			{
				PlayAudio(AUDIO.POINTUP);
			}
			SetLabelText(targetUI, Mathf.CeilToInt(currentPoint).ToString("N0"));
		}
	}

	private void SetBonusUI()
	{
		InitTween(UI.OBJ_BONUS_ANIM_ROOT);
		StartCoroutine(StartBonusAnimation());
	}

	private IEnumerator StartBonusAnimation()
	{
		SoundManager.PlayOneshotJingle(40000268, null, null);
		PlayTween(UI.OBJ_BONUS_ANIM_ROOT, true, null, true, 0);
		yield return (object)StartCoroutine(WaitTiming(2.8f));
		ChangeToRankingState();
	}

	private void SetRankingUI()
	{
		SetActive(UI.BTN_SKIP_FULL_SCREEN, true);
		PointEventCurrentData.PointResultData pointRankingData = currentData.pointRankingData;
		SetLabelText(UI.LBL_RANKING_TOTAL_POINT, (pointRankingData.userPoint + pointRankingData.getPoint).ToString("N0"));
		SetLabelText(UI.LBL_PASS_NUM, Mathf.Max(0, pointRankingData.beforeRank - pointRankingData.afterRank).ToString("N0") + "人抜き");
		int num = Mathf.Min(999999, pointRankingData.beforeRank);
		int num2 = Mathf.Min(999999, pointRankingData.afterRank);
		int num3 = 0;
		num3 = ((num <= num2) ? num2.ToString().Length : num.ToString().Length);
		rankingNumbers = new List<GameObject>(6);
		Transform ctrl = GetCtrl(UI.GRD_COUNT_NUMBERS);
		for (int i = 0; i < 6; i++)
		{
			Transform transform = Utility.FindChild(ctrl, "Number" + i);
			if (!((Object)transform == (Object)null))
			{
				if (i >= num3)
				{
					transform.parent = null;
					Object.Destroy(transform.gameObject);
				}
				else
				{
					rankingNumbers.Add(transform.gameObject);
				}
			}
		}
		ctrl.GetComponent<UIGrid>().Reposition();
		if (num > num2)
		{
			SetSpriteNumber(num);
		}
		else
		{
			SetSpriteNumber(num2);
		}
		InitTween(UI.OBJ_RANKING_ANIM_ROOT);
		StartCoroutine(StartRankingAnimation());
	}

	private IEnumerator StartRankingAnimation()
	{
		PlayAudio(AUDIO.CATEGORY);
		bool wait2 = true;
		PlayTween(UI.OBJ_RANKING_ANIM_ROOT, true, delegate
		{
			((_003CStartRankingAnimation_003Ec__Iterator13D)/*Error near IL_0056: stateMachine*/)._003Cwait_003E__0 = false;
		}, true, 0);
		while (wait2)
		{
			if (skipRequest)
			{
				SkipTween(UI.OBJ_RANKING_ROOT, true, 0);
				wait2 = false;
			}
			yield return (object)0;
		}
		if (!skipRequest)
		{
			yield return (object)StartCoroutine(WaitTiming(1f));
		}
		int currentRank = currentData.pointRankingData.beforeRank;
		int targetRank = currentData.pointRankingData.afterRank;
		if (currentRank > targetRank)
		{
			yield return (object)StartCoroutine(CountSpriteAnimation((float)currentRank, targetRank));
			int passRank = currentRank - targetRank;
			Transform originPosNumber = GetCtrl(UI.SPR_POSITION_PASS);
			passPositionNumbers = new List<GameObject>();
			passPositionNumbers.Add(originPosNumber.gameObject);
			for (int i = 0; i < passRank.ToString().Length - 1; i++)
			{
				GameObject posObj = Object.Instantiate(originPosNumber.gameObject);
				posObj.transform.parent = originPosNumber.parent;
				posObj.transform.localPosition = originPosNumber.localPosition;
				posObj.transform.localScale = originPosNumber.localScale;
				passPositionNumbers.Add(posObj);
			}
			SetSpritePassNumber(passRank, passPositionNumbers);
			GetCtrl(UI.GRD_POSITION_PASS).GetComponent<UIGrid>().Reposition();
			Transform passText = GetCtrl(UI.SPR_PASS_TEXT);
			Vector3 lastPassNumberPos = passPositionNumbers[passPositionNumbers.Count - 1].transform.localPosition;
			passText.GetComponent<TweenPosition>().to.x = lastPassNumberPos.x + 41f;
			InitTween(UI.OBJ_PASS_ANIM_ROOT);
			if (!skipRequest)
			{
				yield return (object)StartCoroutine(WaitTiming(0.6f));
			}
			PlayAudio(AUDIO.RESULT);
			wait2 = true;
			PlayTween(UI.OBJ_PASS_ANIM_ROOT, true, delegate
			{
				((_003CStartRankingAnimation_003Ec__Iterator13D)/*Error near IL_039b: stateMachine*/)._003Cwait_003E__0 = false;
			}, true, 0);
			while (wait2)
			{
				if (skipRequest)
				{
					SkipTween(UI.OBJ_PASS_ANIM_ROOT, true, 0);
					wait2 = false;
				}
				yield return (object)0;
			}
		}
		SetActive(UI.BTN_SKIP_FULL_SCREEN, false);
		SetActive(UI.BTN_END_OK, true);
	}

	private IEnumerator CountSpriteAnimation(float currentRank, int targetRank)
	{
		while (currentRank > (float)targetRank)
		{
			yield return (object)0;
			if (skipRequest)
			{
				currentRank = (float)targetRank;
			}
			int before = Mathf.FloorToInt(currentRank);
			float addingRank = Mathf.Min(((float)targetRank - currentRank) * CountDownCube(Time.deltaTime * 4f), -1f);
			currentRank += addingRank;
			currentRank = Mathf.Max(currentRank, (float)targetRank);
			if (before > Mathf.FloorToInt(currentRank))
			{
				PlayAudio(AUDIO.POINTUP);
			}
			int showPoint = Mathf.Max((int)currentRank, 1);
			SetSpriteNumber(showPoint);
		}
	}

	private void SetSpriteNumber(int value)
	{
		string text = value.ToString();
		if (beforeDigits == 0)
		{
			beforeDigits = text.Length;
		}
		if (beforeDigits > text.Length)
		{
			Object.Destroy(rankingNumbers[0].gameObject);
			rankingNumbers.RemoveAt(0);
		}
		for (int i = 0; i < text.Length; i++)
		{
			int num = int.Parse(text[i].ToString());
			if (!((Object)rankingNumbers[i] == (Object)null))
			{
				SetSprite(rankingNumbers[i].transform, SPR_RANKING_NUMBER[num]);
			}
		}
		beforeDigits = text.Length;
	}

	private void SetSpritePassNumber(int value, List<GameObject> numbers)
	{
		string text = value.ToString();
		for (int i = 0; i < text.Length; i++)
		{
			int num = int.Parse(text[i].ToString());
			SetSprite(numbers[i].transform, SPR_PASS_NUMBER[num]);
		}
	}

	private IEnumerator WaitTiming(float waitTime)
	{
		yield return (object)new WaitForSeconds(waitTime);
	}

	private static float CountDownCube(float currentValue)
	{
		return currentValue * (2f - currentValue);
	}

	private void PlayAudio(AUDIO type)
	{
		SoundManager.PlayOneShotUISE((int)type);
	}

	private void ChangeToRankingState()
	{
		if (currentData.pointRankingData.beforeRank < 0 || currentData.pointRankingData.afterRank < 0)
		{
			GameSection.BackSection();
		}
		else
		{
			stateInitialized = false;
			skipRequest = false;
			pointResultState = State.RANKING;
		}
	}

	private void OnQuery_SKIP()
	{
		skipRequest = true;
		GameSection.StopEvent();
	}

	private void OnQuery_OK_REWARD()
	{
		if (currentData.pointRankingData.isStartedBoost)
		{
			stateInitialized = false;
			skipRequest = false;
			pointResultState = State.BONUS;
		}
		else
		{
			ChangeToRankingState();
		}
	}

	private void OnQuery_OK_END()
	{
		GameSection.BackSection();
	}
}
