using UnityEngine;

public class UIInGameMenu : MonoBehaviourSingleton<UIInGameMenu>
{
	[SerializeField]
	protected GameObject partyMenuUI;

	[SerializeField]
	protected GameObject normalMenuUI;

	[SerializeField]
	protected GameObject happenMenuUI;

	[SerializeField]
	protected GameObject retryableMenuUI;

	[SerializeField]
	protected UILabel partyNumber;

	[SerializeField]
	protected GameObject m_missionRoot;

	[SerializeField]
	protected UILabel[] m_missionTexts;

	[SerializeField]
	protected GameObject[] m_missionCrownOn;

	[SerializeField]
	protected GameObject[] m_missionCrownOff;

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
	}

	public void Initialize()
	{
		QuestTable.QuestTableData questData = Singleton<QuestTable>.I.GetQuestData(MonoBehaviourSingleton<QuestManager>.I.currentQuestID);
		if (MonoBehaviourSingleton<QuestManager>.I.IsExplore())
		{
			partyMenuUI.SetActive(value: true);
			normalMenuUI.SetActive(value: false);
			happenMenuUI.SetActive(value: false);
			retryableMenuUI.SetActive(value: false);
			string text = MonoBehaviourSingleton<PartyManager>.I.GetPartyNumber();
			if (string.IsNullOrEmpty(text))
			{
				partyNumber.text = "-";
			}
			else
			{
				partyNumber.text = text;
			}
		}
		else if (questData != null && (questData.questType == QUEST_TYPE.HAPPEN || questData.questType == QUEST_TYPE.SERIES_ARENA))
		{
			partyMenuUI.SetActive(value: false);
			normalMenuUI.SetActive(value: false);
			happenMenuUI.SetActive(value: true);
			retryableMenuUI.SetActive(value: false);
			QuestInfoData.Mission[] array = null;
			array = QuestInfoData.CreateMissionData(questData);
			if (array != null)
			{
				int i = 0;
				for (int num = array.Length; i < num; i++)
				{
					QuestInfoData.Mission mission = array[i];
					m_missionCrownOn[i].SetActive(CLEAR_STATUS.CLEAR == mission.state);
					m_missionCrownOff[i].SetActive(CLEAR_STATUS.CLEAR != mission.state);
					m_missionTexts[i].text = mission.tableData.missionText;
				}
			}
		}
		else if (questData != null && questData.questType == QUEST_TYPE.ARENA)
		{
			partyMenuUI.SetActive(value: false);
			normalMenuUI.SetActive(value: false);
			happenMenuUI.SetActive(value: false);
			retryableMenuUI.SetActive(value: true);
		}
		else
		{
			partyMenuUI.SetActive(value: false);
			normalMenuUI.SetActive(value: true);
			happenMenuUI.SetActive(value: false);
			retryableMenuUI.SetActive(value: false);
		}
		if (MonoBehaviourSingleton<UIManager>.IsValid() && MonoBehaviourSingleton<UIManager>.I.mainChat != null)
		{
			MonoBehaviourSingleton<UIManager>.I.mainChat.HideAll();
		}
		base.gameObject.SetActive(value: true);
	}

	public void OnClickClose()
	{
		MonoBehaviourSingleton<InGameProgress>.I.CloseDialog();
		Close();
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnClickOption()
	{
		if (MonoBehaviourSingleton<GameSceneManager>.I.IsEventExecutionPossible())
		{
			MonoBehaviourSingleton<GameSceneManager>.I.ExecuteSceneEvent("UIInGameMenu.OnClickOption", base.gameObject, "OPTION");
		}
	}

	public void OnClickRetire()
	{
		if (!MonoBehaviourSingleton<GameSceneManager>.I.IsEventExecutionPossible())
		{
			return;
		}
		if (FieldManager.IsValidInGameNoBoss())
		{
			Self self = MonoBehaviourSingleton<StageObjectManager>.I.self;
			if (self != null && self.rescueTime > 0f)
			{
				MonoBehaviourSingleton<GameSceneManager>.I.ExecuteSceneEvent("UIContinueButton.OnClickRetire", base.gameObject, "RETIRE", StringTable.Get(STRING_CATEGORY.IN_GAME, 1008u));
			}
			else
			{
				MonoBehaviourSingleton<GameSceneManager>.I.ExecuteSceneEvent("UIContinueButton.OnClickRetire", base.gameObject, "RETIRE", StringTable.Get(STRING_CATEGORY.IN_GAME, 1009u));
			}
		}
		else
		{
			MonoBehaviourSingleton<GameSceneManager>.I.ExecuteSceneEvent("UIInGameMenu.OnClickRetire", base.gameObject, "RETIRE");
		}
	}

	public void DoRetire()
	{
		if (MonoBehaviourSingleton<InGameProgress>.IsValid())
		{
			MonoBehaviourSingleton<InGameProgress>.I.BattleRetire();
		}
	}

	public void OnClickRetry()
	{
		if (MonoBehaviourSingleton<GameSceneManager>.I.IsEventExecutionPossible())
		{
			MonoBehaviourSingleton<GameSceneManager>.I.ExecuteSceneEvent("UIInGameMenu.OnClickRetry", base.gameObject, "RETRY");
		}
	}
}
