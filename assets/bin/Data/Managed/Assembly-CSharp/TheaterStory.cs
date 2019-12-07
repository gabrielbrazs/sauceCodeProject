using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheaterStory : GameSection
{
	protected enum UI
	{
		SCR_LIST,
		GRD_LIST,
		STR_STORY_NON_LIST,
		LBL_CHAPTER_NAME,
		LBL_STORY_TITLE,
		OBJ_ACTIVE_ROOT,
		OBJ_INACTIVE_ROOT,
		LBL_NOW,
		LBL_MAX
	}

	public const int PAGING = 10;

	private List<TheaterModeTable.TheaterModeData> m_canViewStoryList;

	private string m_chapterName = "";

	private int m_nowPage = 1;

	private int m_pageMax = 1;

	public override string overrideBackKeyEvent => "CLOSE";

	public override void Initialize()
	{
		object[] array = GameSection.GetEventData() as object[];
		m_canViewStoryList = (array[0] as List<TheaterModeTable.TheaterModeData>);
		m_chapterName = (array[1] as string);
		m_canViewStoryList.Sort(delegate(TheaterModeTable.TheaterModeData a, TheaterModeTable.TheaterModeData b)
		{
			if (b.order != 0 && a.order != 0)
			{
				return a.order - b.order;
			}
			if (b.order != 0)
			{
				return 1;
			}
			return (a.order != 0) ? (-1) : ((int)(a.story_id - b.story_id));
		});
		SetPaging();
		StartCoroutine("DoInitialize");
	}

	private IEnumerator DoInitialize()
	{
		yield return null;
		base.Initialize();
	}

	public override void UpdateUI()
	{
		string goingHomeEvent = GameSection.GetGoingHomeEvent();
		EventData[] events = new EventData[4]
		{
			new EventData(goingHomeEvent, null),
			new EventData("MAIN_MENU_MENU", null),
			new EventData("THEATERMODE", null),
			new EventData("STORY", new object[2]
			{
				m_canViewStoryList,
				m_chapterName
			})
		};
		if (m_canViewStoryList.Count > 0)
		{
			SetActive(base.gameObject.transform, UI.STR_STORY_NON_LIST, is_visible: false);
		}
		else
		{
			SetActive(base.gameObject.transform, UI.STR_STORY_NON_LIST, is_visible: true);
		}
		SetActive(base.gameObject.transform, UI.LBL_CHAPTER_NAME, is_visible: true);
		SetLabelText(base.gameObject.transform, UI.LBL_CHAPTER_NAME, m_chapterName);
		SetLabelText(UI.LBL_MAX, m_pageMax.ToString());
		SetLabelText(UI.LBL_NOW, m_nowPage.ToString());
		List<TheaterModeTable.TheaterModeData> dispList = m_canViewStoryList;
		if (m_pageMax > 1)
		{
			List<TheaterModeTable.TheaterModeData> list = new List<TheaterModeTable.TheaterModeData>();
			int j = 0;
			for (int count = dispList.Count; j < count; j++)
			{
				if (j >= (m_nowPage - 1) * 10 && j < m_nowPage * 10)
				{
					list.Add(dispList[j]);
				}
			}
			dispList = list;
		}
		SetDynamicList(UI.GRD_LIST, "TheaterStoryListItem", dispList.Count, reset: true, null, null, delegate(int i, Transform t, bool is_recycle)
		{
			SetActive(t, UI.LBL_STORY_TITLE, is_visible: true);
			SetLabelText(t, UI.LBL_STORY_TITLE, dispList[i].title);
			SetEvent(t, "PLAY_STORY", new object[4]
			{
				dispList[i].script_id,
				0,
				0,
				events
			});
		});
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			RefreshUI();
		}
	}

	private void OnQuery_PLAY_STORY()
	{
	}

	private void OnQuery_PAGE_PREV()
	{
		m_nowPage = ((m_nowPage > 1) ? (m_nowPage - 1) : m_pageMax);
		RefreshUI();
	}

	private void OnQuery_PAGE_NEXT()
	{
		m_nowPage = ((m_nowPage >= m_pageMax) ? 1 : (m_nowPage + 1));
		RefreshUI();
	}

	private void SetPaging()
	{
		m_nowPage = 1;
		List<TheaterModeTable.TheaterModeData> canViewStoryList = m_canViewStoryList;
		if (canViewStoryList.Count <= 10)
		{
			SetActive(UI.OBJ_ACTIVE_ROOT, is_visible: false);
			SetActive(UI.OBJ_INACTIVE_ROOT, is_visible: true);
			m_pageMax = 1;
		}
		else
		{
			SetActive(UI.OBJ_ACTIVE_ROOT, is_visible: true);
			SetActive(UI.OBJ_INACTIVE_ROOT, is_visible: false);
			m_pageMax = canViewStoryList.Count / 10 + 1;
		}
		SetLabelText(UI.LBL_MAX, m_pageMax.ToString());
		SetLabelText(UI.LBL_NOW, m_nowPage.ToString());
	}
}
