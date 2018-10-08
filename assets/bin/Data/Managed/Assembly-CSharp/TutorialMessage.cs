using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMessage : UIBehaviour
{
	private enum UI
	{
		OBJ_ROOT,
		OBJ_DESC_ROOT,
		OBJ_MESSAGE_ROOT,
		OBJ_IMAGE_ROOT,
		OBJ_ANCHOR_MESSAGE_UP,
		OBJ_ANCHOR_MESSAGE_DOWN,
		OBJ_ANCHOR_MESSAGE_CENTER,
		SPR_MESSAGE,
		LBL_MESSAGE,
		TEX_BG,
		OBJ_TUTORIAL_CURSOR,
		SPR_TUTORIAL_CURSOR_DOWN,
		WGT_FORCS_FRAME,
		SPR_FORCS_FRAME
	}

	private enum State
	{
		CLOSE,
		INIT,
		DESC,
		WAIT
	}

	private enum TWEEN_CTRL_ID
	{
		MESSAGE,
		IMAGE
	}

	private class TutorialData
	{
		public int m_current_index;

		private bool[] is_loading_flag;

		private Transform[] load_image;

		public TutorialMessageTable.TutorialMessageData Messages
		{
			get;
			private set;
		}

		public string name => Messages.sectionName;

		public int count => Messages.messageData.Count;

		public TutorialMessageTable.TutorialMessageData.MessageData Current()
		{
			if (Messages == null)
			{
				return null;
			}
			return Messages.messageData[m_current_index];
		}

		public string CurrentWaitEventName()
		{
			TutorialMessageTable.TutorialMessageData.MessageData messageData = Current();
			if (messageData == null || !messageData.is_wait_event)
			{
				return string.Empty;
			}
			return messageData.waitEventName;
		}

		public bool CurrentShowMessageOrImage()
		{
			return CurrentShowMessage() || CurrentShowImage();
		}

		public bool CurrentShowMessage()
		{
			TutorialMessageTable.TutorialMessageData.MessageData messageData = Current();
			if (messageData == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(messageData.message))
			{
				return false;
			}
			return true;
		}

		public bool CurrentShowImage()
		{
			TutorialMessageTable.TutorialMessageData.MessageData messageData = Current();
			if (messageData == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(messageData.imageResourceName))
			{
				return false;
			}
			return true;
		}

		public bool IsLoadingCurrentImage()
		{
			if (!CurrentShowImage())
			{
				return false;
			}
			TutorialMessageTable.TutorialMessageData.MessageData messageData = Current();
			if (messageData == null)
			{
				return false;
			}
			return is_loading_flag[m_current_index];
		}

		public void SetImage(int index, Transform t)
		{
			if (load_image != null && load_image.Length > index && is_loading_flag != null && is_loading_flag.Length > index)
			{
				load_image[index] = t;
				load_image[index].gameObject.SetActive(false);
				is_loading_flag[index] = false;
			}
		}

		public void SetActiveCurrentImage()
		{
			if (load_image != null && load_image.Length > m_current_index)
			{
				load_image[m_current_index].gameObject.SetActive(true);
			}
		}

		public void DestoryCurrentImage()
		{
			if ((UnityEngine.Object)load_image[m_current_index] != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy(load_image[m_current_index].gameObject);
				load_image[m_current_index] = null;
			}
		}

		public void Init(TutorialMessageTable.TutorialMessageData data)
		{
			m_current_index = 0;
			Messages = data;
			load_image = new Transform[Messages.messageData.Count];
			is_loading_flag = new bool[Messages.messageData.Count];
			int index = 0;
			Messages.messageData.ForEach(delegate(TutorialMessageTable.TutorialMessageData.MessageData msg)
			{
				load_image[index] = null;
				is_loading_flag[index] = !string.IsNullOrEmpty(msg.imageResourceName);
				index++;
			});
		}

		public bool HasNext()
		{
			if (Messages == null)
			{
				return false;
			}
			return (m_current_index + 1 < count) ? true : false;
		}

		public bool ShiftNext()
		{
			if (!HasNext())
			{
				return false;
			}
			m_current_index++;
			return true;
		}
	}

	private struct CursorInfo
	{
		public Transform target;

		public GameObject button;

		public Transform cursor;

		public bool isInDynamicList;
	}

	private const int TUTORIAL_MESSAGE_MAX = 8;

	private UITexture m_textureBG;

	private UIPanel m_panelRoot;

	private State m_status;

	private bool waiting;

	private TutorialData m_tutorial;

	public Transform m_last_target;

	private Action onCloseCallback;

	private bool enableSkip;

	private int skipSectionRunCount;

	[Range(0f, 1f)]
	[SerializeField]
	private Vector3 HOLE_SIZE = new Vector3(0.3f, 0.2f, 1f);

	private BetterList<CursorInfo> cursorAttachList = new BetterList<CursorInfo>();

	private UITexture TextureBG
	{
		get
		{
			if ((UnityEngine.Object)m_textureBG == (UnityEngine.Object)null)
			{
				Transform ctrl = GetCtrl(UI.TEX_BG);
				if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
				{
					return null;
				}
				m_textureBG = ctrl.GetComponent<UITexture>();
			}
			return m_textureBG;
		}
	}

	private UIPanel PanelRoot
	{
		get
		{
			if ((UnityEngine.Object)m_panelRoot == (UnityEngine.Object)null)
			{
				m_panelRoot = GetComponent<UIPanel>();
			}
			return m_panelRoot;
		}
	}

	public bool isErrorResend
	{
		get;
		private set;
	}

	public bool isErrorResendQuestGacha
	{
		get;
		private set;
	}

	private int GetTweenCtrlID(TWEEN_CTRL_ID _enum)
	{
		return (int)_enum;
	}

	public bool IsEnableMessage()
	{
		return m_tutorial != null || base.isOpen;
	}

	public bool IsOnlyShowImage()
	{
		return m_tutorial != null && m_tutorial.CurrentShowImage() && !m_tutorial.CurrentShowMessage();
	}

	private void SetErrorResendFlag(FORCE_RESEND_DIALOG_FLAG flag)
	{
		if (flag != 0)
		{
			switch (flag)
			{
			case FORCE_RESEND_DIALOG_FLAG.FLAG_UP:
				isErrorResend = true;
				break;
			case FORCE_RESEND_DIALOG_FLAG.FLAG_DOWN:
				isErrorResend = false;
				break;
			}
		}
	}

	public void SetErrorResendQuestGachaFlag()
	{
		isErrorResendQuestGacha = false;
		if (MonoBehaviourSingleton<UserInfoManager>.IsValid())
		{
			bool flag = MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.GACHA1);
			if (MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.GACHA_QUEST_START))
			{
				isErrorResendQuestGacha = false;
			}
			else if (flag)
			{
				isErrorResendQuestGacha = true;
			}
		}
	}

	public override bool IsTransitioning()
	{
		return false;
	}

	private void OnEnable()
	{
		InputManager.OnTouchOffAlways = (InputManager.OnTouchDelegate)Delegate.Combine(InputManager.OnTouchOffAlways, new InputManager.OnTouchDelegate(OnTouchOffAlways));
	}

	private void OnDisable()
	{
		InputManager.OnTouchOffAlways = (InputManager.OnTouchDelegate)Delegate.Remove(InputManager.OnTouchOffAlways, new InputManager.OnTouchDelegate(OnTouchOffAlways));
	}

	private void OnTouchOffAlways(InputManager.TouchInfo info)
	{
		if (enableSkip && GetComponent<UIWidget>(UI.SPR_MESSAGE).finalAlpha >= 0.99f && !MonoBehaviourSingleton<UIManager>.I.IsTransitioning())
		{
			enableSkip = false;
			SkipTween(UI.OBJ_DESC_ROOT, true, GetTweenCtrlID(TWEEN_CTRL_ID.MESSAGE));
			SkipTween(UI.OBJ_DESC_ROOT, true, GetTweenCtrlID(TWEEN_CTRL_ID.IMAGE));
			SkipTween(GetCursor(0), UI.SPR_TUTORIAL_CURSOR_DOWN, true, 0);
			HideFocusFrame();
		}
	}

	protected override void OnDestroy()
	{
		if (!AppMain.isApplicationQuit)
		{
			if ((UnityEngine.Object)TextureBG != (UnityEngine.Object)null)
			{
				TextureBG.material.SetVector("_HoleSize", Vector4.zero);
				TextureBG.material.SetVector("_HolePos", Vector4.zero);
			}
			base.OnDestroy();
		}
	}

	public void SetSkipSectionRunCount(int len)
	{
		skipSectionRunCount = Mathf.Max(0, len);
	}

	private bool IsTutorialCompleted(string scene_name, string section_name)
	{
		if (!Singleton<TutorialMessageTable>.IsValid())
		{
			return true;
		}
		if (Singleton<TutorialMessageTable>.I.ReadData.HasReadAll())
		{
			return true;
		}
		if (!Singleton<TutorialMessageTable>.I.HasSection(section_name))
		{
			return true;
		}
		return false;
	}

	private bool SetupTutorialData(string scene_name, string section_name, bool is_force, bool is_new_section = false, string event_name = null)
	{
		m_tutorial = null;
		if (!Singleton<TutorialMessageTable>.IsValid())
		{
			return false;
		}
		TutorialMessageTable.TutorialMessageData enableExecTutorial = Singleton<TutorialMessageTable>.I.GetEnableExecTutorial(section_name, is_force, is_new_section, event_name);
		if (enableExecTutorial == null || enableExecTutorial.messageData.Count == 0)
		{
			return false;
		}
		m_tutorial = new TutorialData();
		m_tutorial.Init(enableExecTutorial);
		StartCoroutine(_LoadMessageImage(m_tutorial));
		return true;
	}

	private IEnumerator _LoadMessageImage(TutorialData tutorial_data)
	{
		LoadingQueue lo_queue = new LoadingQueue(this);
		List<LoadObject> list = new List<LoadObject>();
		m_tutorial.Messages.messageData.ForEach(delegate(TutorialMessageTable.TutorialMessageData.MessageData msg)
		{
			if (msg == null)
			{
				((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_0052: stateMachine*/)._003Clist_003E__1.Add(null);
			}
			else if (string.IsNullOrEmpty(msg.imageResourceName))
			{
				((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_0052: stateMachine*/)._003Clist_003E__1.Add(null);
			}
			else
			{
				LoadObject item = ((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_0052: stateMachine*/)._003Clo_queue_003E__0.Load(RESOURCE_CATEGORY.UI, msg.imageResourceName, false);
				((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_0052: stateMachine*/)._003Clist_003E__1.Add(item);
			}
		});
		if (lo_queue.IsLoading())
		{
			yield return (object)lo_queue.Wait();
		}
		list.ForEach(delegate(LoadObject data)
		{
			((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_009d: stateMachine*/)._003Cindex_003E__2++;
			if (data != null)
			{
				Transform transform = ResourceUtility.Realizes(data.loadedObject, ((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_009d: stateMachine*/)._003C_003Ef__this.GetCtrl(UI.OBJ_IMAGE_ROOT), 5);
				if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
				{
					transform.name = ((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_009d: stateMachine*/)._003Cindex_003E__2.ToString();
					((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_009d: stateMachine*/).tutorial_data.SetImage(((_003C_LoadMessageImage_003Ec__Iterator28E)/*Error near IL_009d: stateMachine*/)._003Cindex_003E__2, transform);
				}
			}
		});
	}

	public void ForceRun(string scene_name, string section_name, Action callback = null)
	{
		if (!SetupTutorialData(scene_name, section_name, true, false, null))
		{
			callback?.Invoke();
		}
		else
		{
			onCloseCallback = callback;
			if (section_name == "TutorialStep4_1_1")
			{
				StartCoroutine(Delay(1f));
			}
			else
			{
				StartTutorial(false);
			}
		}
	}

	private IEnumerator Delay(float delayTime)
	{
		yield return (object)new WaitForSeconds(delayTime);
		StartTutorial(false);
	}

	public void Run(string scene_name, string section_name, bool is_new_section, bool hide_cursol, Action callback = null)
	{
		if (skipSectionRunCount > 0)
		{
			skipSectionRunCount--;
		}
		else
		{
			if (hide_cursol)
			{
				HideCursor(true, true);
			}
			if (IsTutorialCompleted(scene_name, section_name))
			{
				callback?.Invoke();
			}
			else if (!SetupTutorialData(scene_name, section_name, false, is_new_section, null))
			{
				callback?.Invoke();
			}
			else
			{
				onCloseCallback = callback;
				StartTutorial(hide_cursol);
			}
		}
	}

	public void TriggerRun(string scene_name, string section_name, string event_name)
	{
		if (m_status == State.CLOSE)
		{
			HideCursor(true, true);
			if (!IsTutorialCompleted(scene_name, section_name) && SetupTutorialData(scene_name, section_name, false, false, event_name))
			{
				StartTutorial(true);
			}
		}
	}

	public void SubmitCursor(string sender_name, string event_name)
	{
		if (m_tutorial != null && !MonoBehaviourSingleton<GameSceneManager>.I.isOpenImportantDialog && !(MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSectionName() == "CommonErrorDialog"))
		{
			if (event_name == "TUTORIAL_NEXT")
			{
				if (waiting)
				{
					return;
				}
			}
			else
			{
				string currentSectionName = MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSectionName();
				if (currentSectionName.Contains("InGameMain") && currentSectionName.Contains("Confirm"))
				{
					return;
				}
			}
			if (m_status == State.DESC)
			{
				m_tutorial.DestoryCurrentImage();
				ChangeNext();
			}
			else if (m_status == State.WAIT)
			{
				if (m_tutorial != null && m_tutorial.Current() != null && m_tutorial.Current().is_wait_event)
				{
					if (IsExpectedEvent(sender_name, event_name))
					{
						SetReadCurrentTutorial();
						m_tutorial.DestoryCurrentImage();
						m_status = State.CLOSE;
						HideCursor(true, true);
					}
				}
				else if (IsExpectedEvent(sender_name, event_name))
				{
					SetReadCurrentTutorial();
					if (m_tutorial != null)
					{
						m_tutorial.DestoryCurrentImage();
					}
					m_status = State.CLOSE;
					HideCursor(true, true);
				}
			}
		}
	}

	private bool IsExpectedEvent(string sender_name, string event_name)
	{
		if (m_tutorial == null)
		{
			return true;
		}
		string text = m_tutorial.CurrentWaitEventName();
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		return text == event_name;
	}

	private void Finish(bool show_image)
	{
		if (onCloseCallback != null)
		{
			onCloseCallback();
		}
		onCloseCallback = null;
		if ((UnityEngine.Object)TextureBG != (UnityEngine.Object)null && !waiting)
		{
			if (!show_image)
			{
				TextureBG.enabled = false;
			}
			enableSkip = true;
		}
	}

	private void SetReadCurrentTutorial()
	{
		if (Singleton<TutorialMessageTable>.IsValid() && Singleton<TutorialMessageTable>.I.ReadData != null && m_tutorial != null && m_tutorial.Messages != null && m_tutorial.count >= 1)
		{
			Singleton<TutorialMessageTable>.I.ReadData.SetReadId(m_tutorial.Messages.tutorialId, true);
			SaveRead();
		}
	}

	private void SaveRead()
	{
		if (Singleton<TutorialMessageTable>.IsValid() && Singleton<TutorialMessageTable>.I.ReadData != null)
		{
			Singleton<TutorialMessageTable>.I.ReadData.Save();
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
	}

	protected override void OnClose()
	{
		base.OnClose();
	}

	public void TutorialClose()
	{
		Transform ctrl = GetCtrl(UI.OBJ_IMAGE_ROOT);
		if ((UnityEngine.Object)ctrl != (UnityEngine.Object)null)
		{
			ctrl.DestroyChildren();
		}
		m_tutorial = null;
		Close(UITransition.TYPE.CLOSE);
		m_status = State.CLOSE;
	}

	private void StartTutorial(bool is_hide_cursol)
	{
		skipSectionRunCount = 0;
		m_status = State.INIT;
		if (m_tutorial != null)
		{
			SetErrorResendFlag(m_tutorial.Messages.resendFrag);
			HideCursor(is_hide_cursol, false);
			Open(UITransition.TYPE.OPEN);
			m_status = State.DESC;
			UpdateMessage();
			if (m_tutorial != null && m_tutorial.Messages != null && !string.IsNullOrEmpty(m_tutorial.Messages.strSetBit))
			{
				TUTORIAL_MENU_BIT? setBit = m_tutorial.Messages.GetSetBit();
				if (setBit.HasValue)
				{
					TutorialMessageTable.SendTutorialBit(setBit.Value, null);
				}
			}
			CheckLastMessage();
		}
	}

	private void UpdateMessage()
	{
		SetActiveMessage();
		if (m_status == State.DESC && m_tutorial != null)
		{
			TutorialMessageTable.TutorialMessageData.MessageData messageData = m_tutorial.Current();
			if (messageData != null)
			{
				string text = messageData.message.Replace("{USER_NAME}", MonoBehaviourSingleton<UserInfoManager>.I.userInfo.name);
				UpdateMessagePosition(messageData.position_type);
				PutText(UI.LBL_MESSAGE, text);
				if (messageData.wait == 0f)
				{
					StartMessage();
				}
				else
				{
					WaitingUI(true);
					waiting = true;
					enableSkip = false;
					StartCoroutine(DoWaitToStartMessage(messageData.wait));
				}
			}
		}
	}

	private void WaitingUI(bool is_wait_start)
	{
		if (is_wait_start)
		{
			SetActive(UI.OBJ_MESSAGE_ROOT, false);
			SetActive(UI.OBJ_IMAGE_ROOT, false);
			SetColor(UI.OBJ_DESC_ROOT, Color.white);
			HideBGHole();
		}
		else
		{
			SetActiveMessage();
		}
	}

	private IEnumerator DoWaitToStartMessage(float time)
	{
		yield return (object)new WaitForSeconds(time);
		WaitingUI(false);
		StartMessage();
		waiting = false;
		enableSkip = true;
		TextureBG.enabled = true;
		if (m_tutorial != null)
		{
			TutorialMessageTable.TutorialMessageData.MessageData item = m_tutorial.Current();
			if (item != null && item.has_target)
			{
				TextureBG.enabled = false;
			}
		}
	}

	private void StartMessage()
	{
		if (m_tutorial != null)
		{
			TutorialMessageTable.TutorialMessageData.MessageData messageData = m_tutorial.Current();
			if (messageData != null)
			{
				TWEEN_CTRL_ID @enum = m_tutorial.CurrentShowImage() ? TWEEN_CTRL_ID.IMAGE : TWEEN_CTRL_ID.MESSAGE;
				int tweenCtrlID = GetTweenCtrlID(@enum);
				if (messageData.has_target)
				{
					EventDelegate.Callback callback = delegate
					{
						enableSkip = false;
						HideFocusFrame();
					};
					ResetTween(UI.OBJ_DESC_ROOT, GetTweenCtrlID(TWEEN_CTRL_ID.MESSAGE));
					ResetTween(UI.OBJ_DESC_ROOT, GetTweenCtrlID(TWEEN_CTRL_ID.IMAGE));
					if (!m_tutorial.CurrentShowMessageOrImage() && !IsWaitTween())
					{
						callback();
					}
					else
					{
						TutorialPlayTween(UI.OBJ_DESC_ROOT, callback, tweenCtrlID);
					}
					FocusCursor(messageData);
				}
				else
				{
					EventDelegate.Callback callback2 = delegate
					{
						bool flag = m_tutorial == null;
						if (!flag && m_tutorial != null && !m_tutorial.HasNext() && string.IsNullOrEmpty(m_tutorial.CurrentWaitEventName()))
						{
							flag = true;
						}
						if (flag)
						{
							TutorialClose();
						}
						else if (m_tutorial == null || !m_tutorial.CurrentShowImage())
						{
							SubmitCursor("SELF", "TUTORIAL_NEXT");
						}
					};
					ResetTween(UI.OBJ_DESC_ROOT, GetTweenCtrlID(TWEEN_CTRL_ID.MESSAGE));
					ResetTween(UI.OBJ_DESC_ROOT, GetTweenCtrlID(TWEEN_CTRL_ID.IMAGE));
					if (!m_tutorial.CurrentShowMessageOrImage() && !IsWaitTween())
					{
						callback2();
					}
					else
					{
						TutorialPlayTween(UI.OBJ_DESC_ROOT, callback2, tweenCtrlID);
					}
				}
				UpdateFocusFrame();
			}
		}
	}

	private bool IsWaitTween()
	{
		return MonoBehaviourSingleton<UIManager>.I.IsTransitioning() || GameSceneManager.isAutoEventSkip;
	}

	private void TutorialPlayTween(Enum ui, EventDelegate.Callback callback, int tween_ctrl_id)
	{
		if (IsWaitTween() || (m_tutorial != null && m_tutorial.CurrentShowImage()))
		{
			StartCoroutine(_TweenCoroutine(ui, callback, tween_ctrl_id));
		}
		else
		{
			PlayTween(ui, true, callback, false, tween_ctrl_id);
		}
	}

	private IEnumerator _TweenCoroutine(Enum ui, EventDelegate.Callback callback, int tween_ctrl_id)
	{
		while (IsWaitTween())
		{
			yield return (object)null;
		}
		if (m_tutorial != null && m_tutorial.CurrentShowImage())
		{
			while (m_tutorial.IsLoadingCurrentImage())
			{
				yield return (object)null;
			}
			m_tutorial.SetActiveCurrentImage();
		}
		PlayTween(ui, true, callback, false, tween_ctrl_id);
	}

	private void UpdateFocusFrame()
	{
		HideFocusFrame();
		if (m_tutorial != null)
		{
			TutorialMessageTable.TutorialMessageData.MessageData messageData = m_tutorial.Current();
			if (messageData != null && !string.IsNullOrEmpty(messageData.focusFrame))
			{
				string[] array = messageData.focusFrame.Split(',');
				if (array.Length >= 4)
				{
					UIWidget component = GetComponent<UIWidget>(UI.SPR_FORCS_FRAME);
					if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
					{
						float.TryParse(array[0], out float result);
						float.TryParse(array[1], out float result2);
						int.TryParse(array[2], out int result3);
						int.TryParse(array[3], out int result4);
						component.cachedTransform.localPosition = new Vector3(result, result2, 0f);
						component.width = result3;
						component.height = result4;
						SetActive(UI.WGT_FORCS_FRAME, true);
						ResetTween(UI.WGT_FORCS_FRAME, 0);
						PlayTween(UI.WGT_FORCS_FRAME, true, null, false, 0);
					}
				}
			}
		}
	}

	private void HideFocusFrame()
	{
		SetActive(UI.WGT_FORCS_FRAME, false);
	}

	private void UpdateMessagePosition(TutorialMessageTable.TutorialMessageData.MessageData.Position pos)
	{
		UI uI = UI.OBJ_ANCHOR_MESSAGE_UP;
		switch (pos)
		{
		case TutorialMessageTable.TutorialMessageData.MessageData.Position.DOWN:
			uI = UI.OBJ_ANCHOR_MESSAGE_DOWN;
			break;
		case TutorialMessageTable.TutorialMessageData.MessageData.Position.CENTER:
			uI = UI.OBJ_ANCHOR_MESSAGE_CENTER;
			break;
		}
		Transform ctrl = GetCtrl(uI);
		Transform ctrl2 = GetCtrl(UI.OBJ_MESSAGE_ROOT);
		if ((UnityEngine.Object)ctrl != (UnityEngine.Object)null && (UnityEngine.Object)ctrl2 != (UnityEngine.Object)null)
		{
			ctrl2.position = ctrl.position;
		}
	}

	private void SetActiveMessage()
	{
		Transform ctrl = GetCtrl(UI.OBJ_DESC_ROOT);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			Transform ctrl2 = GetCtrl(UI.OBJ_MESSAGE_ROOT);
			if (!((UnityEngine.Object)ctrl2 == (UnityEngine.Object)null))
			{
				Transform ctrl3 = GetCtrl(UI.OBJ_IMAGE_ROOT);
				if (!((UnityEngine.Object)ctrl3 == (UnityEngine.Object)null))
				{
					if (m_tutorial != null)
					{
						ctrl.gameObject.SetActive(true);
						ctrl3.gameObject.SetActive(false);
						ctrl2.gameObject.SetActive(false);
						if (m_tutorial.CurrentShowImage())
						{
							ctrl3.gameObject.SetActive(true);
						}
						if (m_tutorial.CurrentShowMessage())
						{
							ctrl2.gameObject.SetActive(true);
						}
					}
					else
					{
						ctrl.gameObject.SetActive(false);
					}
				}
			}
		}
	}

	private void PutText(UI label_enum, string text)
	{
		Transform ctrl = GetCtrl(label_enum);
		if ((UnityEngine.Object)ctrl != (UnityEngine.Object)null)
		{
			UILabel component = ctrl.GetComponent<UILabel>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.text = text;
			}
		}
	}

	private void PlayVoice(int voice_id)
	{
		SoundManager.PlayVoice(voice_id, 1f, 0u, null, null);
	}

	public void UpdateFocusCursol()
	{
		TutorialMessageTable.TutorialMessageData.MessageData messageData = m_tutorial.Current();
		if (messageData != null && messageData.has_target)
		{
			HideCursor(true, false);
			FocusCursor(messageData);
		}
	}

	private void FocusCursor(TutorialMessageTable.TutorialMessageData.MessageData data)
	{
		GameSection currentSection = MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSection();
		if (!((UnityEngine.Object)currentSection == (UnityEngine.Object)null) && !((UnityEngine.Object)currentSection._transform == (UnityEngine.Object)null))
		{
			Transform target = null;
			Transform transform = currentSection._transform;
			string cursorTarget = data.cursorTarget;
			if (cursorTarget.Contains("[ID]") || cursorTarget.Contains("[ID!]"))
			{
				bool is_not_equal = cursorTarget.Contains("[ID!]");
				int id = 0;
				string s = cursorTarget.Remove(0, (!is_not_equal) ? 4 : 5);
				if (int.TryParse(s, out id))
				{
					ItemIcon target_icon = null;
					ItemIcon[] componentsInChildren = MonoBehaviourSingleton<UIManager>.I.uiRootTransform.GetComponentsInChildren<ItemIcon>();
					if (componentsInChildren != null)
					{
						Array.ForEach(componentsInChildren, delegate(ItemIcon _data)
						{
							if (!((UnityEngine.Object)target != (UnityEngine.Object)null) && !((UnityEngine.Object)_data == (UnityEngine.Object)null) && !((UnityEngine.Object)_data.transform == (UnityEngine.Object)null))
							{
								if (is_not_equal)
								{
									if (_data.GetItemID != id)
									{
										target_icon = _data;
									}
								}
								else if (_data.GetItemID == id)
								{
									target_icon = _data;
								}
							}
						});
					}
					if ((UnityEngine.Object)target_icon != (UnityEngine.Object)null)
					{
						UIButton componentInParent = target_icon.GetComponentInParent<UIButton>();
						if ((UnityEngine.Object)componentInParent != (UnityEngine.Object)null)
						{
							target = componentInParent.transform;
						}
					}
				}
			}
			else
			{
				string[] array = cursorTarget.Split('/');
				int i = 0;
				for (int num = array.Length; i < num; i++)
				{
					target = Utility.FindChild(transform, array[i]);
					transform = target;
				}
			}
			if ((UnityEngine.Object)target == (UnityEngine.Object)null && MonoBehaviourSingleton<UIManager>.IsValid())
			{
				string[] array2 = cursorTarget.Split('/');
				transform = MonoBehaviourSingleton<UIManager>.I.uiRootTransform;
				int j = 0;
				for (int num2 = array2.Length; j < num2; j++)
				{
					target = Utility.FindActiveChild(transform, array2[j]);
					transform = target;
				}
				if ((UnityEngine.Object)target == (UnityEngine.Object)null)
				{
					return;
				}
			}
			m_last_target = AttachCursor(target, data);
			FOCUS_PATTERN focusPattern = m_tutorial.Current().focusPattern;
			if (m_tutorial != null && focusPattern != 0)
			{
				ShowBGHole(target.transform.position, focusPattern, target);
			}
			else
			{
				HideBGHole();
			}
		}
	}

	private void HideBGHole()
	{
		UITexture textureBG = TextureBG;
		if (!((UnityEngine.Object)textureBG == (UnityEngine.Object)null))
		{
			textureBG.enabled = true;
			textureBG.material.SetVector("_HoleSize", new Vector4((float)Screen.width, (float)Screen.height, 1f));
			RefreeshDraw();
		}
	}

	private void ShowBGHole(Vector3 hole_pos, FOCUS_PATTERN focus, Transform target = null)
	{
		UITexture textureBG = TextureBG;
		if (!((UnityEngine.Object)textureBG == (UnityEngine.Object)null))
		{
			textureBG.enabled = true;
			Vector3 vector = MonoBehaviourSingleton<UIManager>.I.uiCamera.WorldToViewportPoint(hole_pos);
			Vector4 vector2 = new Vector4(vector.x * 2f - 1f, vector.y * 2f - 1f, 1f);
			Vector3 hOLE_SIZE = HOLE_SIZE;
			BoxCollider component = target.GetComponent<BoxCollider>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				float num;
				if (focus == FOCUS_PATTERN.TARGET_FOCUS)
				{
					Vector3 size = component.size;
					float x = size.x;
					Vector3 localScale = target.localScale;
					num = x * localScale.x;
				}
				else
				{
					num = 0f;
				}
				float num2 = num;
				float num3;
				if (focus == FOCUS_PATTERN.TARGET_FOCUS)
				{
					Vector3 size2 = component.size;
					float y = size2.y;
					Vector3 localScale2 = target.localScale;
					num3 = y * localScale2.y;
				}
				else
				{
					num3 = 0f;
				}
				float num4 = num3;
				float x2 = num2 / (float)Screen.width;
				float y2 = num4 / (float)Screen.height;
				hOLE_SIZE.x = x2;
				hOLE_SIZE.y = y2;
				if (focus == FOCUS_PATTERN.TARGET_FOCUS)
				{
					float x3 = vector2.x;
					Vector3 center = component.center;
					float num5 = center.x / (float)Screen.width;
					Vector3 localScale3 = target.localScale;
					vector2.x = x3 + num5 * localScale3.x;
					float y3 = vector2.y;
					Vector3 center2 = component.center;
					float num6 = center2.y / (float)Screen.height;
					Vector3 localScale4 = target.localScale;
					vector2.y = y3 + num6 * localScale4.y;
				}
				else
				{
					vector2.x = 0f;
					vector2.y = 0f;
				}
			}
			textureBG.material.SetVector("_HoleSize", hOLE_SIZE);
			textureBG.material.SetVector("_HolePos", vector2);
			RefreeshDraw();
		}
	}

	private void RefreeshDraw()
	{
		if (!((UnityEngine.Object)PanelRoot == (UnityEngine.Object)null))
		{
			PanelRoot.Refresh();
		}
	}

	private void HideCursor(bool force = false, bool is_close = true)
	{
		HideBGHole();
		if ((m_status == State.WAIT && (UnityEngine.Object)m_last_target != (UnityEngine.Object)null) || force)
		{
			GameSection currentSection = MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSection();
			if (!((UnityEngine.Object)currentSection == (UnityEngine.Object)null) && !((UnityEngine.Object)currentSection.transform == (UnityEngine.Object)null))
			{
				DetachCursor(m_last_target, is_close);
				m_last_target = null;
			}
		}
	}

	public void ChangeNext()
	{
		if (base.isOpen && m_status == State.DESC && m_tutorial != null)
		{
			if (m_tutorial.HasNext())
			{
				m_tutorial.ShiftNext();
				UpdateMessage();
			}
			CheckLastMessage();
		}
	}

	private void CheckLastMessage()
	{
		if (m_tutorial != null && !m_tutorial.HasNext())
		{
			bool show_image = m_tutorial.CurrentShowImage();
			if (!string.IsNullOrEmpty(m_tutorial.CurrentWaitEventName()))
			{
				m_status = State.WAIT;
			}
			else
			{
				SetReadCurrentTutorial();
				m_tutorial = null;
			}
			Finish(show_image);
		}
	}

	private float CalcCenterXOffset(UIWidget widget)
	{
		Vector2 pivotOffset = widget.pivotOffset;
		float num = (0f - pivotOffset.x) * (float)widget.width;
		float num2 = num + (float)widget.width;
		return (num + num2) * 0.5f;
	}

	private float CalcCenterYOffset(UIWidget widget)
	{
		Vector2 pivotOffset = widget.pivotOffset;
		float num = (0f - pivotOffset.y) * (float)widget.height;
		float num2 = num + (float)widget.height;
		return (num + num2) * 0.5f;
	}

	private void SetupCursor(Transform cursor, Transform target, float center_x, float center_y)
	{
		float num = 10f;
		float num2 = num;
		Vector3 lossyScale = target.lossyScale;
		float num3 = num2 * lossyScale.x;
		UISprite component = cursor.GetComponent<UISprite>();
		Vector2 a = new Vector2(center_x, center_y);
		Vector3 lossyScale2 = target.lossyScale;
		Vector2 vector = a * lossyScale2.x;
		Vector3 position = target.position;
		float y = position.y;
		float y2 = vector.y;
		float num4 = (float)component.height;
		Vector3 lossyScale3 = component.cachedTransform.lossyScale;
		float y3 = y + (y2 + num4 * lossyScale3.y);
		Vector3 vector2 = PanelRoot.cachedTransform.InverseTransformPoint(new Vector3(0f, y3, 0f));
		float y4 = vector2.y;
		float num5 = 427f;
		Vector2 offset;
		if (y4 + num < num5)
		{
			offset = new Vector2(vector.x, vector.y + num3);
			component.pivot = UIWidget.Pivot.Bottom;
			component.flip = UIBasicSprite.Flip.Nothing;
		}
		else
		{
			offset = new Vector2(vector.x, vector.y - num3);
			component.pivot = UIWidget.Pivot.Top;
			component.flip = UIBasicSprite.Flip.Vertically;
		}
		cursor.localRotation = Quaternion.identity;
		TutorialUIObjectFollower tutorialUIObjectFollower = cursor.gameObject.AddComponent<TutorialUIObjectFollower>();
		tutorialUIObjectFollower.Setup(target, offset);
	}

	private void SetupCursor(Transform cursor, UIWidget target_widget)
	{
		SetupCursor(cursor, target_widget.cachedTransform, CalcCenterXOffset(target_widget), CalcCenterYOffset(target_widget));
	}

	private void SetupCursor(Transform cursor, BoxCollider collider)
	{
		Transform transform = collider.transform;
		Vector3 center = collider.center;
		float x = center.x;
		Vector3 center2 = collider.center;
		SetupCursor(cursor, transform, x, center2.y);
	}

	private Transform _AttachTutorialCursor(Transform target, TutorialMessageTable.TutorialMessageData.MessageData data)
	{
		if ((UnityEngine.Object)target == (UnityEngine.Object)null)
		{
			return null;
		}
		Transform transform = null;
		BoxCollider component2;
		UIWidget component3;
		if (data != null && data.cursorType == TutorialMessageTable.TutorialMessageData.MessageData.CursorType.MANUAL)
		{
			transform = CreateTutorialCursor();
			UISprite component = transform.GetComponent<UISprite>();
			component.pivot = UIWidget.Pivot.Bottom;
			TutorialUIObjectFollower tutorialUIObjectFollower = transform.gameObject.AddComponent<TutorialUIObjectFollower>();
			TutorialUIObjectFollower tutorialUIObjectFollower2 = tutorialUIObjectFollower;
			Vector2 cursorOffset = data.cursorOffset;
			Vector3 lossyScale = target.lossyScale;
			tutorialUIObjectFollower2.Setup(target, cursorOffset * lossyScale.x);
			transform.localRotation = Quaternion.AngleAxis((float)data.cursorRotDeg, Vector3.forward);
		}
		else if ((bool)(component2 = target.GetComponent<BoxCollider>()))
		{
			transform = CreateTutorialCursor();
			SetupCursor(transform, component2);
		}
		else if ((bool)(component3 = target.GetComponent<UIWidget>()))
		{
			transform = CreateTutorialCursor();
			SetupCursor(transform, component3);
		}
		if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
		{
			bool flag = false;
			if (m_tutorial != null && !string.IsNullOrEmpty(m_tutorial.Messages.messageData[0].message))
			{
				flag = true;
			}
			if (data != null && data.cursorDelay >= 0f)
			{
				UITweener component4 = GetComponent<UITweener>(transform, UI.SPR_TUTORIAL_CURSOR_DOWN);
				if ((UnityEngine.Object)component4 != (UnityEngine.Object)null)
				{
					component4.delay = data.cursorDelay;
				}
			}
			if (flag)
			{
				ResetTween(transform, UI.SPR_TUTORIAL_CURSOR_DOWN, 0);
				PlayTween(transform, UI.SPR_TUTORIAL_CURSOR_DOWN, true, null, false, 0);
			}
			else
			{
				SkipTween(transform, UI.SPR_TUTORIAL_CURSOR_DOWN, true, 0);
			}
		}
		return transform;
	}

	private Transform CreateTutorialCursor()
	{
		SetActive(UI.OBJ_TUTORIAL_CURSOR, true);
		Transform ctrl = GetCtrl(UI.SPR_TUTORIAL_CURSOR_DOWN);
		Transform result = ResourceUtility.Realizes(ctrl.gameObject, PanelRoot.transform, -1);
		SetActive(UI.OBJ_TUTORIAL_CURSOR, false);
		return result;
	}

	private void _AttachCursor(Transform target, TutorialMessageTable.TutorialMessageData.MessageData data)
	{
		UIButton uIButton = target.GetComponent<UIButton>();
		if ((UnityEngine.Object)uIButton == (UnityEngine.Object)null)
		{
			uIButton = target.GetComponentInChildren<UIButton>();
			if ((UnityEngine.Object)uIButton == (UnityEngine.Object)null)
			{
				uIButton = target.GetComponentInParent<UIButton>();
				if ((UnityEngine.Object)uIButton == (UnityEngine.Object)null)
				{
					return;
				}
			}
		}
		int i = 0;
		for (int num = cursorAttachList.size; i < num; i++)
		{
			CursorInfo cursorInfo = cursorAttachList[i];
			if ((UnityEngine.Object)cursorInfo.cursor != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy(cursorInfo.cursor.gameObject);
				cursorAttachList.RemoveAt(i);
				i--;
				num--;
			}
		}
		cursorAttachList.Clear();
		CursorInfo item = default(CursorInfo);
		item.target = target;
		item.button = uIButton.gameObject;
		item.cursor = _AttachTutorialCursor(target, data);
		item.isInDynamicList = ((UnityEngine.Object)target.GetComponentInParent<UIDynamicList>() != (UnityEngine.Object)null);
		cursorAttachList.Add(item);
		m_last_target = target;
	}

	private void _DetachCursor(Transform target)
	{
		int i = 0;
		for (int num = cursorAttachList.size; i < num; i++)
		{
			CursorInfo cursorInfo = cursorAttachList[i];
			if ((UnityEngine.Object)cursorInfo.target == (UnityEngine.Object)target)
			{
				UnityEngine.Object.Destroy(cursorInfo.cursor.gameObject);
				cursorAttachList.RemoveAt(i);
				i--;
				num--;
			}
		}
	}

	private void _RemoveCursor(Transform cursor)
	{
		int i = 0;
		for (int num = cursorAttachList.size; i < num; i++)
		{
			CursorInfo cursorInfo = cursorAttachList[i];
			if ((UnityEngine.Object)cursorInfo.cursor == (UnityEngine.Object)cursor)
			{
				UnityEngine.Object.Destroy(cursorInfo.cursor.gameObject);
				cursorAttachList.RemoveAt(i);
				i--;
				num--;
			}
		}
	}

	public static Transform AttachCursor(Transform t, TutorialMessageTable.TutorialMessageData.MessageData data = null)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return null;
		}
		if ((UnityEngine.Object)MonoBehaviourSingleton<UIManager>.I.tutorialMessage == (UnityEngine.Object)null)
		{
			return null;
		}
		MonoBehaviourSingleton<UIManager>.I.tutorialMessage._AttachCursor(t, data);
		return t;
	}

	public static void DetachCursor(Transform t, bool is_close = true)
	{
		if (!((UnityEngine.Object)MonoBehaviourSingleton<UIManager>.I.tutorialMessage == (UnityEngine.Object)null))
		{
			if (is_close && MonoBehaviourSingleton<UIManager>.I.tutorialMessage.isOpen)
			{
				MonoBehaviourSingleton<UIManager>.I.tutorialMessage.TutorialClose();
			}
			if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
			{
				MonoBehaviourSingleton<UIManager>.I.tutorialMessage._DetachCursor(t);
			}
		}
	}

	public static void RemoveCursor(Transform cursor)
	{
		if (!((UnityEngine.Object)cursor == (UnityEngine.Object)null) && !((UnityEngine.Object)MonoBehaviourSingleton<UIManager>.I.tutorialMessage == (UnityEngine.Object)null))
		{
			MonoBehaviourSingleton<UIManager>.I.tutorialMessage._RemoveCursor(cursor);
		}
	}

	public static bool IsActiveButton(GameObject button)
	{
		if (!MonoBehaviourSingleton<GameSceneManager>.IsValid() || (MonoBehaviourSingleton<UIManager>.IsValid() && (UnityEngine.Object)MonoBehaviourSingleton<UIManager>.I.tutorialMessage == (UnityEngine.Object)null) || (UnityEngine.Object)button == (UnityEngine.Object)null)
		{
			return true;
		}
		if (MonoBehaviourSingleton<GameSceneManager>.IsValid() && (MonoBehaviourSingleton<GameSceneManager>.I.isOpenImportantDialog || MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSectionName() == "CommonErrorDialog"))
		{
			return true;
		}
		string currentSectionName = MonoBehaviourSingleton<GameSceneManager>.I.GetCurrentSectionName();
		if (!string.IsNullOrEmpty(currentSectionName) && currentSectionName.Contains("InGameMain") && currentSectionName.Contains("Confirm"))
		{
			return true;
		}
		if (currentSectionName == "HomeTop" && ((HomeBase.OnAfterGacha2Tutorial && MonoBehaviourSingleton<UserInfoManager>.I.userStatus.IsTutorialBitReady && !MonoBehaviourSingleton<UserInfoManager>.I.CheckTutorialBit(TUTORIAL_MENU_BIT.AFTER_GACHA2)) || HomeBase.OnTalkPamelaTutorial))
		{
			return false;
		}
		BetterList<CursorInfo> betterList = MonoBehaviourSingleton<UIManager>.I.tutorialMessage.cursorAttachList;
		if (betterList.size == 0)
		{
			return true;
		}
		int i = 0;
		for (int num = betterList.size; i < num; i++)
		{
			CursorInfo cursorInfo = betterList[i];
			if ((UnityEngine.Object)cursorInfo.target == (UnityEngine.Object)null || (UnityEngine.Object)cursorInfo.button == (UnityEngine.Object)null || (UnityEngine.Object)cursorInfo.cursor == (UnityEngine.Object)null)
			{
				betterList.RemoveAt(i);
				i--;
				num--;
			}
			else if (cursorInfo.isInDynamicList)
			{
				Transform transform = button.transform;
				while ((UnityEngine.Object)transform != (UnityEngine.Object)null)
				{
					if ((UnityEngine.Object)cursorInfo.target == (UnityEngine.Object)transform)
					{
						return true;
					}
					transform = transform.parent;
				}
			}
			else if ((UnityEngine.Object)cursorInfo.button == (UnityEngine.Object)button)
			{
				return true;
			}
		}
		if (button.name == "TEX_BG")
		{
			return true;
		}
		return false;
	}

	public static Transform GetCursor(int index = 0)
	{
		TutorialMessage tutorialMessage = MonoBehaviourSingleton<UIManager>.I.tutorialMessage;
		if ((UnityEngine.Object)tutorialMessage == (UnityEngine.Object)null)
		{
			return null;
		}
		if (index + 1 > tutorialMessage.cursorAttachList.size)
		{
			return null;
		}
		CursorInfo cursorInfo = tutorialMessage.cursorAttachList[index];
		return cursorInfo.cursor;
	}
}
