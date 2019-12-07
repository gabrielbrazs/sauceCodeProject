using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClanAcceptDialog : GameSection
{
	protected enum UI
	{
		STR_NON_LIST,
		GRD_LIST,
		OBJ_SCROLL_BAR,
		LBL_NOW,
		LBL_MAX,
		OBJ_ACTIVE_ROOT,
		OBJ_INACTIVE_ROOT
	}

	private const int ITEM_COUNT_PER_PAGE = 10;

	private List<FriendCharaInfo> originalList;

	private List<FriendCharaInfo> pageList = new List<FriendCharaInfo>();

	private int nowPage;

	private int pageNumMax = 1;

	public override void Initialize()
	{
		StartCoroutine(DoInitialize());
	}

	protected IEnumerator DoInitialize()
	{
		bool isReceived = false;
		MonoBehaviourSingleton<ClanMatchingManager>.I.SendRequestList(delegate(bool b, List<FriendCharaInfo> l)
		{
			originalList = l;
			nowPage = 0;
			if (originalList != null)
			{
				pageNumMax = Mathf.CeilToInt((float)originalList.Count / 10f);
				MonoBehaviourSingleton<UserInfoManager>.I.SetClanRequestNum(originalList.Count);
			}
			else
			{
				pageNumMax = 1;
			}
			isReceived = true;
		});
		while (!isReceived)
		{
			yield return null;
		}
		base.Initialize();
	}

	public override void UpdateUI()
	{
		SetupSrollBarCollider();
		SetupPaging();
		int num = CreatePageList();
		SetActive(UI.STR_NON_LIST, num == 0);
		SetDynamicList(UI.GRD_LIST, "ClanAcceptListItem", num, reset: false, null, null, delegate(int i, Transform t, bool is_recycle)
		{
			SetupItem(t, i);
			SetActive(t, is_visible: true);
		});
	}

	private void SetupSrollBarCollider()
	{
		Transform ctrl = GetCtrl(UI.OBJ_SCROLL_BAR);
		if (ctrl == null)
		{
			return;
		}
		UIWidget component = ctrl.GetComponent<UIWidget>();
		if (!(component == null))
		{
			BoxCollider component2 = ctrl.GetComponent<BoxCollider>();
			if (!(component2 == null))
			{
				component2.size = new Vector2(component2.size.x, component.localSize.y);
			}
		}
	}

	private void SetupPaging()
	{
		SetPageNumText(UI.LBL_NOW, nowPage + 1);
		SetPageNumText(UI.LBL_MAX, pageNumMax);
		SetActive(UI.OBJ_ACTIVE_ROOT, pageNumMax != 1);
		SetActive(UI.OBJ_INACTIVE_ROOT, pageNumMax == 1);
	}

	private int CreatePageList()
	{
		pageList.Clear();
		if (originalList == null)
		{
			return 0;
		}
		int num = nowPage * 10;
		for (int i = 0; i < 10; i++)
		{
			int num2 = num + i;
			if (num2 >= originalList.Count)
			{
				break;
			}
			pageList.Add(originalList[num2]);
		}
		return pageList.Count;
	}

	private void SetupItem(Transform t, int index)
	{
		ClanAcceptListItem clanAcceptListItem = t.GetComponent<ClanAcceptListItem>();
		if (clanAcceptListItem == null)
		{
			clanAcceptListItem = t.gameObject.AddComponent<ClanAcceptListItem>();
		}
		clanAcceptListItem.InitUI();
		clanAcceptListItem.Setup(t, index, pageList[index]);
	}

	private void OnQuery_ACCEPT()
	{
		int index = (int)GameSection.GetEventData();
		if (index < pageList.Count)
		{
			int userId = pageList[index].userId;
			GameSection.StayEvent();
			MonoBehaviourSingleton<ClanMatchingManager>.I.SendAcceptRequest(userId, delegate(bool b)
			{
				if (b)
				{
					ExecRequestSuccess(index);
				}
				else
				{
					GameSection.ResumeEvent(is_resume: false);
				}
			});
		}
	}

	private void OnQuery_REJECT()
	{
		int index = (int)GameSection.GetEventData();
		if (index < pageList.Count)
		{
			int userId = pageList[index].userId;
			GameSection.StayEvent();
			MonoBehaviourSingleton<ClanMatchingManager>.I.SendRejectRequest(userId, delegate(bool b)
			{
				if (b)
				{
					ExecRequestSuccess(index);
				}
				else
				{
					GameSection.ResumeEvent(is_resume: false);
				}
			});
		}
	}

	private void ExecRequestSuccess(int index)
	{
		string name = pageList[index].name;
		int index2 = index + nowPage * 10;
		originalList.RemoveAt(index2);
		int num = Mathf.CeilToInt((float)originalList.Count / 10f);
		if (num > 0 && num != pageNumMax)
		{
			pageNumMax = num;
			if (nowPage >= pageNumMax)
			{
				nowPage--;
			}
		}
		MonoBehaviourSingleton<UserInfoManager>.I.DecreaseClanRequestNum();
		RefreshUI();
		GameSection.ResumeEvent(is_resume: true, new string[1]
		{
			name
		});
	}

	private void OnQuery_PAGE_PREV()
	{
		int num = nowPage = (nowPage - 1 + pageNumMax) % pageNumMax;
		SetDirty(UI.GRD_LIST);
		RefreshUI();
	}

	private void OnQuery_PAGE_NEXT()
	{
		int num = nowPage = (nowPage + 1) % pageNumMax;
		SetDirty(UI.GRD_LIST);
		RefreshUI();
	}

	private void OnQuery_DETAIL()
	{
		int num = (int)GameSection.GetEventData();
		FriendCharaInfo eventData = pageList[num];
		MonoBehaviourSingleton<StatusManager>.I.otherEquipSetSaveIndex = num + 4;
		GameSection.SetEventData(eventData);
	}
}
