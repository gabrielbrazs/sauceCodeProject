using System.Collections;
using UnityEngine;

public class HomeTutorialManager : MonoBehaviour
{
	private enum DialogType
	{
		TALK_WITH_PAMERA,
		LAST_TUTORIAL,
		AFTER_GACHA2
	}

	private bool is_loading;

	private HomeTop m_home_top;

	private Object prefab_dialog;

	private Object prefab_target_area;

	private Transform t_target_area;

	private UITutorialHomeDialog ui_tutorial_dialog;

	private UI_LastTutorial last_tutorial;

	private Transform pamelaArrow;

	private Transform questArrow;

	private Vector3 pamelaPosition = new Vector3(-4.28f, 1.66f, 14.61f);

	private Vector3 questPosition = new Vector3(3.2f, 2.8f, 14f);

	public UITutorialHomeDialog dialog
	{
		get
		{
			if (ui_tutorial_dialog == null && prefab_dialog != null)
			{
				ui_tutorial_dialog = ResourceUtility.Realizes(prefab_dialog).GetComponent<UITutorialHomeDialog>();
				ui_tutorial_dialog.Close();
			}
			return ui_tutorial_dialog;
		}
	}

	public void OnDestroy()
	{
		if (dialog != null)
		{
			Object.Destroy(dialog.gameObject);
		}
	}

	public static bool DoesTutorial()
	{
		if (TutorialStep.IsPlayingFirstAccept() || TutorialStep.IsPlayingFirstDelivery())
		{
			return true;
		}
		return false;
	}

	public static bool DoesTutorialAfterGacha2()
	{
		if (MonoBehaviourSingleton<UserInfoManager>.I.userStatus.IsTutorialBitReady && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.SKILL_EQUIP) && MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.UPGRADE_ITEM) && (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_GACHA2) || !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_QUEST) || !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_MAINSTATUS)))
		{
			return true;
		}
		return false;
	}

	public static bool ShouldRunGachaTutorial()
	{
		if (MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.GACHA1) && !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.GACHA_QUEST_START))
		{
			return true;
		}
		return false;
	}

	public static bool ShouldRunQuestShadowTutorial()
	{
		if (MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.DONE_CHANGE_WEAPON) && !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.SHADOW_QUEST_WIN))
		{
			return true;
		}
		return false;
	}

	public void Setup()
	{
		is_loading = true;
		if (DoesTutorial())
		{
			StartCoroutine(DoSetupFirstHome());
		}
		else if (DoesTutorialAfterGacha2())
		{
			StartCoroutine(DoSetupTutorialAfterGacha2());
		}
		else
		{
			is_loading = false;
		}
	}

	public void ExcuteDoSetupTutorialAfterGacha2()
	{
		StartCoroutine(DoSetupTutorialAfterGacha2());
	}

	private IEnumerator DoSetupFirstHome()
	{
		if (m_home_top == null)
		{
			m_home_top = GetComponent<HomeTop>();
		}
		LoadingQueue loadingQueue = new LoadingQueue(this);
		LoadObject obj_target_area = loadingQueue.Load(RESOURCE_CATEGORY.EFFECT_ACTION, "ef_btl_tutorial_area_01");
		LoadObject obj_tutorial_dialog = loadingQueue.Load(RESOURCE_CATEGORY.UI, "UI_TutorialHomeDialog");
		if (loadingQueue.IsLoading())
		{
			yield return loadingQueue.Wait();
		}
		prefab_target_area = obj_target_area.loadedObject;
		prefab_dialog = obj_tutorial_dialog.loadedObject;
		HidePassengers();
		SetTargetAreaNPC(0);
		SetDialog(DialogType.TALK_WITH_PAMERA);
		is_loading = false;
	}

	private IEnumerator DoSetupTutorialAfterGacha2()
	{
		if (m_home_top == null)
		{
			m_home_top = GetComponent<HomeTop>();
		}
		LoadingQueue loadingQueue = new LoadingQueue(this);
		LoadObject obj_target_area = loadingQueue.Load(RESOURCE_CATEGORY.EFFECT_ACTION, "ef_btl_tutorial_area_01");
		LoadObject obj_tutorial_dialog = loadingQueue.Load(RESOURCE_CATEGORY.UI, "UI_TutorialHomeDialog");
		if (loadingQueue.IsLoading())
		{
			yield return loadingQueue.Wait();
		}
		prefab_target_area = obj_target_area.loadedObject;
		prefab_dialog = obj_tutorial_dialog.loadedObject;
		HidePassengers();
		SetDialog(DialogType.AFTER_GACHA2);
		is_loading = false;
	}

	public void SetupGachaQuestTutorial()
	{
		is_loading = true;
		StartCoroutine(DoSetupGachaQuestTutorial());
	}

	private IEnumerator DoSetupGachaQuestTutorial()
	{
		if (m_home_top == null)
		{
			m_home_top = GetComponent<HomeTop>();
		}
		LoadingQueue loadingQueue = new LoadingQueue(this);
		LoadObject obj_target_area = loadingQueue.Load(RESOURCE_CATEGORY.EFFECT_ACTION, "ef_btl_tutorial_area_01");
		LoadObject obj_tutorial_dialog = loadingQueue.Load(RESOURCE_CATEGORY.UI, "UI_TutorialHomeDialog");
		if (loadingQueue.IsLoading())
		{
			yield return loadingQueue.Wait();
		}
		prefab_target_area = obj_target_area.loadedObject;
		prefab_dialog = obj_tutorial_dialog.loadedObject;
		HidePassengers();
		SetTargetAreaNPC(2);
		is_loading = false;
	}

	private void HidePassengers()
	{
		if (MonoBehaviourSingleton<HomeManager>.IsValid() && MonoBehaviourSingleton<HomeManager>.I.IHomePeople != null)
		{
			MonoBehaviourSingleton<HomeManager>.I.IHomePeople.charas.ForEach(delegate(HomeCharacterBase o)
			{
				if (o is HomePlayerCharacter)
				{
					o.gameObject.SetActive(value: false);
					Transform namePlate = o.GetNamePlate();
					if (null != namePlate)
					{
						o.GetNamePlate().gameObject.SetActive(value: false);
					}
				}
			});
		}
	}

	private void SetTargetAreaNPC(int npc_id)
	{
		if (!(prefab_target_area == null) && MonoBehaviourSingleton<HomeManager>.IsValid() && MonoBehaviourSingleton<HomeManager>.I.IHomePeople != null)
		{
			HomeNPCCharacter homeNPCCharacter = MonoBehaviourSingleton<HomeManager>.I.IHomePeople.GetHomeNPCCharacter(npc_id);
			if (!(homeNPCCharacter == null))
			{
				t_target_area = ResourceUtility.Realizes(prefab_target_area, homeNPCCharacter.transform);
				t_target_area.position = homeNPCCharacter.transform.position;
			}
		}
	}

	private void SetDialog(DialogType type)
	{
		switch (type)
		{
		case DialogType.TALK_WITH_PAMERA:
			if (dialog != null)
			{
				dialog.Open(0, "Tutorial_Request_Text_0701");
			}
			break;
		case DialogType.LAST_TUTORIAL:
			if (last_tutorial != null)
			{
				last_tutorial.OpenLastTutorial();
			}
			break;
		case DialogType.AFTER_GACHA2:
			if (dialog != null)
			{
				StartCoroutine(IEAfterGacha());
			}
			break;
		}
	}

	private IEnumerator IEAfterGacha()
	{
		yield return SetupAllArrow();
	}

	private IEnumerator SetupArrow(Vector3 position, bool isPamela = true)
	{
		LoadingQueue loadingQueue = new LoadingQueue(this);
		LoadObject loadedArrow = loadingQueue.Load(RESOURCE_CATEGORY.SYSTEM, "SystemCommon", new string[1]
		{
			"mdl_arrow_01"
		});
		if (loadingQueue.IsLoading())
		{
			yield return loadingQueue.Wait();
		}
		Vector3 localScale = new Vector3(4f, 4f, 4f);
		if (isPamela)
		{
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_GACHA2))
			{
				pamelaArrow = Utility.CreateGameObject("MdlArrow", MonoBehaviourSingleton<AppMain>.I._transform);
				ResourceUtility.Realizes(loadedArrow.loadedObject, pamelaArrow);
				pamelaArrow.localScale = localScale;
				pamelaArrow.position = position;
				dialog.OpenAfterGacha2();
				dialog.OpenMessage(StringTable.Get(STRING_CATEGORY.TUTORIAL_NEW_STR, 1u).Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name));
			}
		}
		else if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_QUEST))
		{
			questArrow = Utility.CreateGameObject("MdlArrow", MonoBehaviourSingleton<AppMain>.I._transform);
			ResourceUtility.Realizes(loadedArrow.loadedObject, questArrow);
			questArrow.localScale = localScale;
			questArrow.position = position;
			dialog.OpenAfterGacha2();
			dialog.OpenMessage(StringTable.Get(STRING_CATEGORY.TUTORIAL_NEW_STR, 2u).Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name));
		}
	}

	private IEnumerator SetupAllArrow()
	{
		LoadingQueue loadingQueue = new LoadingQueue(this);
		LoadObject loadedArrow = loadingQueue.Load(RESOURCE_CATEGORY.SYSTEM, "SystemCommon", new string[1]
		{
			"mdl_arrow_01"
		});
		if (loadingQueue.IsLoading())
		{
			yield return loadingQueue.Wait();
		}
		Vector3 ARROW_SCALE = new Vector3(4f, 4f, 4f);
		if (HomeBase.isFirstTimeDisplayTextTutorial)
		{
			bool showMessage = false;
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_GACHA2))
			{
				yield return new WaitForSeconds(2f);
				pamelaArrow = Utility.CreateGameObject("MdlArrow", MonoBehaviourSingleton<AppMain>.I._transform);
				ResourceUtility.Realizes(loadedArrow.loadedObject, pamelaArrow);
				pamelaArrow.localScale = ARROW_SCALE;
				pamelaArrow.position = pamelaPosition;
				yield return new WaitForSeconds(0.5f);
				dialog.OpenAfterGacha2();
				dialog.OpenMessage(StringTable.Get(STRING_CATEGORY.TUTORIAL_NEW_STR, 1u).Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name));
				showMessage = true;
				yield return new WaitForSeconds(3.5f);
			}
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_QUEST))
			{
				if (showMessage)
				{
					dialog.Close();
				}
				questArrow = Utility.CreateGameObject("MdlArrow", MonoBehaviourSingleton<AppMain>.I._transform);
				ResourceUtility.Realizes(loadedArrow.loadedObject, questArrow);
				questArrow.localScale = ARROW_SCALE;
				questArrow.position = questPosition;
				yield return new WaitForSeconds(0.5f);
				dialog.OpenAfterGacha2();
				dialog.OpenMessage(StringTable.Get(STRING_CATEGORY.TUTORIAL_NEW_STR, 2u).Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name));
				showMessage = true;
				yield return new WaitForSeconds(3.5f);
			}
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_MAINSTATUS))
			{
				if (showMessage)
				{
					dialog.Close();
				}
				MonoBehaviourSingleton<UIManager>.I.mainStatus.SetTutArrowActive(isActive: true);
				yield return new WaitForSeconds(0.5f);
				dialog.OpenAfterGacha2();
				dialog.OpenMessage(StringTable.Get(STRING_CATEGORY.TUTORIAL_NEW_STR, 3u).Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name));
				yield return new WaitForSeconds(3.5f);
			}
			dialog.Close();
		}
		else
		{
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_GACHA2))
			{
				pamelaArrow = Utility.CreateGameObject("MdlArrow", MonoBehaviourSingleton<AppMain>.I._transform);
				ResourceUtility.Realizes(loadedArrow.loadedObject, pamelaArrow);
				pamelaArrow.localScale = ARROW_SCALE;
				pamelaArrow.position = pamelaPosition;
			}
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_QUEST))
			{
				questArrow = Utility.CreateGameObject("MdlArrow", MonoBehaviourSingleton<AppMain>.I._transform);
				ResourceUtility.Realizes(loadedArrow.loadedObject, questArrow);
				questArrow.localScale = ARROW_SCALE;
				questArrow.position = questPosition;
			}
			if (!MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_MAINSTATUS))
			{
				MonoBehaviourSingleton<UIManager>.I.mainStatus.SetTutArrowActive(isActive: true);
			}
		}
	}

	private void SetupUIArrow()
	{
		if (MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_MAINSTATUS))
		{
			if (MonoBehaviourSingleton<UIManager>.I.mainStatus != null)
			{
				MonoBehaviourSingleton<UIManager>.I.mainStatus.SetTutArrowActive(isActive: false);
			}
		}
		else
		{
			MonoBehaviourSingleton<UIManager>.I.mainStatus.SetTutArrowActive(isActive: true);
			dialog.OpenAfterGacha2();
			dialog.OpenMessage(StringTable.Get(STRING_CATEGORY.TUTORIAL_NEW_STR, 3u).Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name));
		}
	}

	public void DeleteArrow()
	{
		if (pamelaArrow != null)
		{
			Object.Destroy(pamelaArrow.gameObject);
		}
		if (questArrow != null)
		{
			Object.Destroy(questArrow.gameObject);
		}
	}

	public void ForceDeleteArrow(bool isPamela)
	{
		if (isPamela && pamelaArrow != null)
		{
			Object.Destroy(pamelaArrow.gameObject);
		}
		else if (!isPamela && questArrow != null)
		{
			Object.Destroy(questArrow.gameObject);
		}
	}

	private void OpenDialog()
	{
	}

	public void CloseDialog()
	{
		if (dialog != null)
		{
			dialog.Close();
		}
	}

	public bool IsLoading()
	{
		return is_loading;
	}

	public void DisableTargetArea()
	{
		if (t_target_area != null)
		{
			t_target_area.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		DeleteArrow();
	}
}
