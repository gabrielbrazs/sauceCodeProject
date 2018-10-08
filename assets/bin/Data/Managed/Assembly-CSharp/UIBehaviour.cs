using System;
using System.Collections.Generic;
using UnityEngine;

public class UIBehaviour : MonoBehaviour
{
	public enum STATE
	{
		CLOSE,
		TO_OPEN,
		OPEN,
		TO_CLOSE
	}

	private class PrefabData
	{
		public string name;

		public GameObject prefab;

		public GameObject inactiveObject;

		public Transform Realizes(Transform parent)
		{
			if ((UnityEngine.Object)prefab == (UnityEngine.Object)null)
			{
				return null;
			}
			if (!((UnityEngine.Object)inactiveObject != (UnityEngine.Object)null))
			{
				return ResourceUtility.Realizes(prefab, parent, 5);
			}
			return InstantiateManager.Realizes(ref inactiveObject, parent, 5);
		}
	}

	protected struct LabelWidthLimitter
	{
		private UILabel label;

		private int width;

		private bool fixAnchorForEffect;

		private int defaultHeight;

		public LabelWidthLimitter(UILabel label, int width, bool fixAnchorForEffect)
		{
			this.label = label;
			this.width = width;
			defaultHeight = label.height;
			this.fixAnchorForEffect = fixAnchorForEffect;
		}

		public void Update()
		{
			if (label.width > width)
			{
				label.overflowMethod = UILabel.Overflow.ShrinkContent;
				label.width = width;
				UILabel uILabel = label;
				Vector2 printedSize = label.printedSize;
				uILabel.height = Mathf.RoundToInt(printedSize.y);
			}
			else if (label.width < width)
			{
				label.overflowMethod = UILabel.Overflow.ResizeFreely;
				label.height = defaultHeight;
			}
			if (fixAnchorForEffect)
			{
				label.bottomAnchor.absolute = 0;
				label.topAnchor.absolute = label.height;
			}
		}
	}

	private const string MATERIAL_INFO_PREFAB_NAME = "MaterialInfo";

	private const string enemyIconNormalFrameName = "MonsterCircleN";

	private Transform[] ctrls;

	private STATE _state;

	private int _baseDepth = -1;

	protected bool uiUpdateInstant = true;

	private bool _uiVisible = true;

	private BetterList<PrefabData> prefabs;

	private static readonly string[] elementSpriteName = new string[7]
	{
		"elem_fire",
		"elem_water",
		"elem_thunder",
		"elem_soil",
		"elem_light",
		"elem_dark",
		"elem_all"
	};

	private static readonly string[] elementDefSpriteName = new string[7]
	{
		"EquipPalaElementDEF_Fire",
		"EquipPalaElementDEF_Water",
		"EquipPalaElementDEF_Thunder",
		"EquipPalaElementDEF_Soil",
		"EquipPalaElementDEF_Light",
		"EquipPalaElementDEF_Dark",
		"EquipPalaElement_None"
	};

	private static readonly string[] SKILL_ICON_SPRITE_NAME = new string[11]
	{
		"EquipBtnSlot_Circle_on",
		"EquipBtnSlot_Triangle_on",
		"EquipBtnSlot_Cross_on",
		null,
		null,
		null,
		null,
		"EquipBtnSlot_Square_on",
		null,
		null,
		null
	};

	private static readonly string[] EMPTY_SKILL_ICON_EQUIP_SPRITE_NAME = new string[11]
	{
		"EquipBtnSlot_Circle_off",
		"EquipBtnSlot_Triangle_off",
		"EquipBtnSlot_Cross_off",
		null,
		null,
		null,
		null,
		"EquipBtnSlot_Square_off",
		null,
		null,
		null
	};

	private static readonly string[] EMPTY_SKILL_ICON_SPRITE_NAME = new string[11]
	{
		"ItemIconSlot_Circle_off",
		"ItemIconSlot_Triangle_off",
		"ItemIconSlot_Cross_off",
		null,
		null,
		null,
		null,
		"ItemIconSlot_Square_off",
		null,
		null,
		null
	};

	private static readonly string[] EQUIP_INDEX_ICON_SP_NAME = new string[7]
	{
		"EquipMarkW01",
		"EquipMarkW02",
		"EquipMarkW03",
		"EquipMarkArmor",
		"EquipMarkHelm",
		"EquipMarkArm",
		"EquipMarkLeg"
	};

	private static readonly string[] ITEM_TYPE_ICON_SPRITE_NAME = new string[9]
	{
		"ItemIconKind_Sword",
		"ItemIconKind_Brade",
		"ItemIconKind_Lance",
		"ItemIconKind_Edge",
		"ItemIconKind_Allow",
		"ItemIconKind_Armor",
		"ItemIconKind_Helm",
		"ItemIconKind_Arm",
		"ItemIconKind_Leg"
	};

	private static readonly string[] SKILL_TYPE_ICON_SPRITE_NAME = new string[11]
	{
		"ItemIconKind_Attack",
		"ItemIconKind_Support",
		"ItemIconKind_Heal",
		"ItemIconKind_",
		"ItemIconKind_",
		"ItemIconKind_",
		"ItemIconKind_",
		"ItemIconKind_Passive",
		"ItemIconKind_",
		"ItemIconKind_",
		"ItemIconKind_Fragment"
	};

	private static readonly string[] ITEM_TYPE_ICON_SPRITE_BG_NAME = new string[7]
	{
		"ItemIconKind_Base_BCD",
		"ItemIconKind_Base_BCD",
		"ItemIconKind_Base_BCD",
		"ItemIconKind_Base_A",
		"ItemIconKind_Base_S",
		"ItemIconKind_Base_SS",
		"ItemIconKind_Base_SS"
	};

	private static readonly string[] MAGI_TYPE_ICON_SPRITE_NAME = new string[6]
	{
		"MagiEquipIcon_Sword",
		"MagiEquipIcon_Brade",
		"MagiEquipIcon_Lance",
		"MagiEquipIcon_Edge",
		"MagiEquipIcon_Allow",
		"MagiEquipIcon_Armor"
	};

	private static readonly string ABILITY_DETAIL_ITEM_PREFAB_NAME = "AbilityDetailItem";

	private static readonly Color32 buffGreen = new Color32(0, byte.MaxValue, 128, byte.MaxValue);

	private static readonly string[] enemyIconGradeFrameName = new string[7]
	{
		"MonsterFrame_C",
		"MonsterFrame_B",
		"MonsterFrame_A",
		"MonsterFrame_A",
		"MonsterFrame_S",
		"MonsterFrame_S",
		"MonsterFrame_SS"
	};

	public Transform _transform
	{
		get;
		private set;
	}

	public Transform collectUI
	{
		get;
		set;
	}

	public UIBehaviour transferUI
	{
		get;
		protected set;
	}

	public List<UIPanel> uiPanels
	{
		get;
		private set;
	}

	public int[] uiPanelDepths
	{
		get;
		private set;
	}

	public List<UITransition> transitions
	{
		get;
		private set;
	}

	public GameSceneTables.SectionData sectionData
	{
		get;
		set;
	}

	public ResourceLink resourceLink
	{
		get;
		private set;
	}

	public STATE state
	{
		get
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				return transferUI.state;
			}
			return _state;
		}
	}

	public bool isOpen
	{
		get
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				return transferUI.isOpen;
			}
			return _state == STATE.OPEN;
		}
	}

	public bool isClose
	{
		get
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				return transferUI.isClose;
			}
			return _state == STATE.CLOSE;
		}
	}

	public int baseDepth
	{
		get
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				return transferUI.baseDepth;
			}
			return _baseDepth;
		}
		set
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				transferUI.baseDepth = value;
			}
			else if (_baseDepth != value)
			{
				_baseDepth = value;
				if (uiPanels != null && uiPanels.Count != 0)
				{
					uiPanels[0].depth = value;
					int num = value + 1;
					int i = 1;
					for (int count = uiPanels.Count; i < count; i++)
					{
						uiPanels[i].depth = uiPanelDepths[i] + num;
					}
				}
			}
		}
	}

	public bool uiFirstUpdate
	{
		get;
		private set;
	}

	public bool uiVisible
	{
		get
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				return transferUI.uiVisible;
			}
			return _uiVisible;
		}
		set
		{
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				transferUI.uiVisible = value;
			}
			else if (_uiVisible != value)
			{
				_uiVisible = value;
				SetUIVisible(_uiVisible);
			}
		}
	}

	public bool IsCtrlEmpty()
	{
		return ctrls == null;
	}

	public virtual bool IsTransitioning()
	{
		return state == STATE.TO_OPEN || state == STATE.TO_CLOSE;
	}

	private void SetUIVisible(bool b)
	{
		if (!((UnityEngine.Object)collectUI == (UnityEngine.Object)null) && collectUI.gameObject.activeSelf != b)
		{
			collectUI.gameObject.SetActive(b);
			if (b && (UnityEngine.Object)collectUI != (UnityEngine.Object)null)
			{
				UIVirtualScreen componentInChildren = collectUI.gameObject.GetComponentInChildren<UIVirtualScreen>();
				if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)
				{
					componentInChildren.InitWidget();
				}
			}
		}
	}

	public void UpdateAnchors()
	{
		if ((UnityEngine.Object)collectUI != (UnityEngine.Object)null)
		{
			UIUtility.UpdateAnchors(collectUI);
		}
	}

	protected virtual void Awake()
	{
		_transform = base.transform;
		uiFirstUpdate = true;
		MonoBehaviourSingleton<UIManager>.I.uiList.Add(this);
	}

	public void InitUI()
	{
		if (ctrls == null && uiFirstUpdate)
		{
			base.gameObject.GetComponentsInChildren(Temporary.uiGameSceneEventSender);
			int i = 0;
			for (int count = Temporary.uiGameSceneEventSender.Count; i < count; i++)
			{
				Temporary.uiGameSceneEventSender[i].callback = OnEvent;
			}
			Temporary.uiGameSceneEventSender.Clear();
			transitions = new List<UITransition>();
			base.gameObject.GetComponentsInChildren(transitions);
			if ((UnityEngine.Object)collectUI == (UnityEngine.Object)null)
			{
				string collectUIName = GetCollectUIName();
				if (!string.IsNullOrEmpty(collectUIName))
				{
					collectUI = MonoBehaviourSingleton<UIManager>.I.Find(collectUIName);
				}
				else
				{
					collectUI = _transform;
				}
			}
			if (!((UnityEngine.Object)collectUI == (UnityEngine.Object)null))
			{
				resourceLink = collectUI.GetComponent<ResourceLink>();
				uiPanels = new List<UIPanel>();
				base.gameObject.GetComponentsInChildren(true, uiPanels);
				uiPanelDepths = new int[uiPanels.Count];
				int j = 0;
				for (int count2 = uiPanels.Count; j < count2; j++)
				{
					uiPanelDepths[j] = uiPanels[j].depth;
				}
				Type type = Type.GetType(collectUI.name + "+UI");
				if (type == null && sectionData != (GameSceneTables.SectionData)null)
				{
					type = Type.GetType(sectionData.sectionName + "+UI");
				}
				if (type != null)
				{
					CreateCtrlsArray(type);
				}
				SetUIVisible(_uiVisible);
			}
		}
	}

	protected void OnEvent(string event_name, object event_data, string check_app_ver)
	{
		MonoBehaviourSingleton<GameSceneManager>.I.ExecuteSceneEvent("UIBehaviour", base.gameObject, event_name, event_data, check_app_ver, true);
	}

	protected virtual void OnDestroy()
	{
		if (!AppMain.isApplicationQuit)
		{
			if (prefabs != null)
			{
				int i = 0;
				for (int size = prefabs.size; i < size; i++)
				{
					PrefabData prefabData = prefabs.buffer[i];
					if ((UnityEngine.Object)prefabData.inactiveObject != (UnityEngine.Object)null)
					{
						UnityEngine.Object.DestroyImmediate(prefabData.inactiveObject);
						prefabData.inactiveObject = null;
					}
				}
				prefabs.Release();
			}
			MonoBehaviourSingleton<UIManager>.I.uiList.Remove(this);
		}
	}

	protected void SetTransferUI(string ui_name, Type enum_type)
	{
		Transform transform = MonoBehaviourSingleton<UIManager>.I.Find(ui_name);
		if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
		{
			Log.Error(LOG.UI, ui_name + " is not found.");
		}
		else
		{
			transferUI = transform.gameObject.GetComponent<UIBehaviour>();
			if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
			{
				transferUI.CreateCtrlsArray(enum_type);
				transferUI.GetComponentsInChildren(true, Temporary.uiGameSceneEventSender);
				int i = 0;
				for (int count = Temporary.uiGameSceneEventSender.Count; i < count; i++)
				{
					Temporary.uiGameSceneEventSender[i].callback = OnEvent;
				}
				Temporary.uiGameSceneEventSender.Clear();
			}
		}
	}

	protected virtual string GetCollectUIName()
	{
		return null;
	}

	public void CreateCtrlsArray(Type enum_type)
	{
		if (ctrls != null)
		{
			Log.Error(LOG.UI, "Re CollectCtrls");
		}
		else
		{
			int num = Enum.GetNames(enum_type).Length;
			ctrls = new Transform[num];
		}
	}

	public Transform GetCtrl(Enum label_enum)
	{
		if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
		{
			return transferUI.GetCtrl(label_enum);
		}
		if (ctrls == null)
		{
			Log.Error(LOG.UI, "not collect ctrls.");
			return null;
		}
		int num = Convert.ToInt32(label_enum);
		if (num < 0 || num >= ctrls.Length)
		{
			return null;
		}
		if ((UnityEngine.Object)ctrls[num] == (UnityEngine.Object)null)
		{
			ctrls[num] = Utility.Find(collectUI, label_enum.ToString());
		}
		return ctrls[num];
	}

	public void AddPrefab(GameObject prefab, GameObject inactive_object)
	{
		if (prefabs == null)
		{
			prefabs = new BetterList<PrefabData>();
		}
		PrefabData prefabData = new PrefabData();
		prefabData.name = prefab.name;
		prefabData.prefab = prefab;
		prefabData.inactiveObject = inactive_object;
		prefabs.Add(prefabData);
	}

	private PrefabData GetPrefabData(string prefab_name)
	{
		if (prefabs != null && !string.IsNullOrEmpty(prefab_name))
		{
			int i = 0;
			for (int size = prefabs.size; i < size; i++)
			{
				if (prefabs.buffer[i].name == prefab_name)
				{
					return prefabs.buffer[i];
				}
			}
		}
		return null;
	}

	protected Transform SetPrefab(Enum parent_enum, string prefab_name)
	{
		if (prefabs == null)
		{
			return null;
		}
		return SetPrefab(GetCtrl(parent_enum), prefab_name, true);
	}

	protected Transform SetPrefab(Transform parent, string prefab_name, bool check_panel = true)
	{
		if (prefabs == null || (UnityEngine.Object)parent == (UnityEngine.Object)null)
		{
			return null;
		}
		Transform transform = parent.Find(prefab_name);
		if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
		{
			return transform;
		}
		return Realizes(prefab_name, parent, check_panel);
	}

	protected Transform Realizes(string prefab_name, Transform parent, bool check_panel = true)
	{
		if (prefabs == null || (UnityEngine.Object)parent == (UnityEngine.Object)null)
		{
			Debug.LogWarning("Relizesに失敗しました");
			return null;
		}
		return _Realizes(GetPrefabData(prefab_name), parent, check_panel);
	}

	private Transform _Realizes(PrefabData prefab_data, Transform parent, bool check_panel)
	{
		if (prefab_data == null)
		{
			return null;
		}
		Transform transform = prefab_data.Realizes(parent);
		if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
		{
			return null;
		}
		if (check_panel)
		{
			UIPanel componentInChildren = transform.GetComponentInChildren<UIPanel>();
			if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)
			{
				UIPanel componentInParent = parent.GetComponentInParent<UIPanel>();
				if ((UnityEngine.Object)componentInParent != (UnityEngine.Object)null)
				{
					componentInChildren.depth = componentInParent.depth + 1;
				}
			}
		}
		return transform;
	}

	protected Transform FindCtrl(Transform root, Enum enum_value)
	{
		if (enum_value == null || (UnityEngine.Object)root == (UnityEngine.Object)null)
		{
			return root;
		}
		return Utility.Find(root, enum_value.ToString());
	}

	protected Transform GetChild(Enum ctrl_enum, int index)
	{
		return GetChild(GetCtrl(ctrl_enum), index);
	}

	protected Transform GetChild(Transform root, Enum ctrl_enum, int index)
	{
		return GetChild(FindCtrl(root, ctrl_enum), index);
	}

	protected Transform GetChild(Transform t, int index)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return null;
		}
		return t.GetChild(index);
	}

	protected Transform GetChildSafe(Enum ctrl_enum, int index)
	{
		return GetChildSafe(GetCtrl(ctrl_enum), index);
	}

	protected Transform GetChildSafe(Transform root, Enum ctrl_enum, int index)
	{
		return GetChildSafe(FindCtrl(root, ctrl_enum), index);
	}

	protected Transform GetChildSafe(Transform t, int index)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return null;
		}
		if (index < 0 || index >= t.childCount)
		{
			return null;
		}
		return t.GetChild(index);
	}

	protected void SetActive(Enum ctrl_enum, bool is_visible)
	{
		SetActive(GetCtrl(ctrl_enum), is_visible);
	}

	protected void SetActive(Transform root, Enum ctrl_enum, bool is_visible)
	{
		SetActive(FindCtrl(root, ctrl_enum), is_visible);
	}

	protected void SetActive(Transform t, bool is_visible)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.gameObject.SetActive(is_visible);
		}
	}

	public bool IsActive(Enum ctrl_enum)
	{
		Transform ctrl = GetCtrl(ctrl_enum);
		if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
		{
			return false;
		}
		return ctrl.gameObject.activeSelf;
	}

	protected void InitDeactive(Enum ctrl_enum)
	{
		InitDeactive(GetCtrl(ctrl_enum));
	}

	protected void InitDeactive(Transform root, Enum ctrl_enum)
	{
		InitDeactive(FindCtrl(root, ctrl_enum));
	}

	protected void InitDeactive(Transform t)
	{
		if (uiFirstUpdate && !((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.gameObject.SetActive(false);
		}
	}

	protected COMPONENT GetComponent<COMPONENT>(Enum ctrl_enum) where COMPONENT : Component
	{
		return GetComponent<COMPONENT>(GetCtrl(ctrl_enum));
	}

	protected COMPONENT GetComponent<COMPONENT>(Transform root, Enum ctrl_enum) where COMPONENT : Component
	{
		return GetComponent<COMPONENT>(FindCtrl(root, ctrl_enum));
	}

	protected COMPONENT GetComponent<COMPONENT>(Transform t) where COMPONENT : Component
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return (COMPONENT)null;
		}
		return t.GetComponent<COMPONENT>();
	}

	protected void SetEnabled<COMPONENT>(Enum ctrl_enum, bool is_enabled) where COMPONENT : MonoBehaviour
	{
		SetEnabled<COMPONENT>(GetCtrl(ctrl_enum), is_enabled);
	}

	protected void SetEnabled<COMPONENT>(Transform root, Enum ctrl_enum, bool is_enabled) where COMPONENT : MonoBehaviour
	{
		SetEnabled<COMPONENT>(FindCtrl(root, ctrl_enum), is_enabled);
	}

	protected void SetEnabled<COMPONENT>(Transform t, bool is_enabled) where COMPONENT : MonoBehaviour
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			COMPONENT component = t.GetComponent<COMPONENT>();
			component.enabled = is_enabled;
		}
	}

	protected void SetDepth(Transform root, Enum panel_enum, int depth)
	{
		SetDepth(FindCtrl(root, panel_enum), depth);
	}

	protected void SetDepth(Enum panel_enum, int depth)
	{
		SetDepth(GetCtrl(panel_enum), depth);
	}

	protected void SetDepth(Transform t, int depth)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIPanel>().depth = depth;
		}
	}

	protected void SetWidth(Enum button_enum, int width)
	{
		SetWidth(GetCtrl(button_enum), width);
	}

	protected void SetWidth(Transform root, Enum button_enum, int width)
	{
		SetWidth(FindCtrl(root, button_enum), width);
	}

	protected void SetWidth(Transform t, int width)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIWidget>().width = width;
		}
	}

	protected int GetWidth(Enum ctrl_enum)
	{
		Transform ctrl = GetCtrl(ctrl_enum);
		if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
		{
			return 0;
		}
		return ctrl.GetComponent<UIWidget>().width;
	}

	protected int GetHeight(Enum ctrl_enum)
	{
		Transform ctrl = GetCtrl(ctrl_enum);
		if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
		{
			return 0;
		}
		return ctrl.GetComponent<UIWidget>().height;
	}

	protected void SetHeight(Enum _enum, int height)
	{
		SetHeight(GetCtrl(_enum), height);
	}

	protected void SetHeight(Transform t, int height)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIWidget>().height = height;
		}
	}

	protected void SetCellWidth(Enum ctrl_enum, int width, bool reposition = false)
	{
		Transform ctrl = GetCtrl(ctrl_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			UIGrid component = ctrl.GetComponent<UIGrid>();
			component.cellWidth = (float)width;
			if (reposition)
			{
				component.Reposition();
			}
		}
	}

	private UILabel _GetLabel(Transform root, Enum label_enum)
	{
		Transform transform = (!((UnityEngine.Object)root == (UnityEngine.Object)null)) ? FindCtrl(root, label_enum) : GetCtrl(label_enum);
		if (object.ReferenceEquals(transform, null))
		{
			return null;
		}
		return transform.GetComponent<UILabel>();
	}

	protected void SetSupportEncoding(Transform root, Enum label_enum, bool isEnable)
	{
		UILabel uILabel = _GetLabel(root, label_enum);
		if (!object.ReferenceEquals(uILabel, null))
		{
			uILabel.supportEncoding = isEnable;
		}
	}

	protected void SetSupportEncoding(Enum label_enum, bool isEnable)
	{
		UILabel uILabel = _GetLabel(null, label_enum);
		if (!object.ReferenceEquals(uILabel, null))
		{
			uILabel.supportEncoding = isEnable;
		}
	}

	protected bool IsSupportEncoding(Enum label_enum)
	{
		UILabel uILabel = _GetLabel(null, label_enum);
		if (object.ReferenceEquals(uILabel, null))
		{
			return false;
		}
		return uILabel.supportEncoding;
	}

	protected string GetLabel(Transform root, Enum label_enum)
	{
		UILabel uILabel = _GetLabel(root, label_enum);
		if ((UnityEngine.Object)uILabel == (UnityEngine.Object)null)
		{
			return string.Empty;
		}
		return uILabel.text;
	}

	protected void SetLabelText(Enum label_enum, object obj)
	{
		SetLabelText(GetCtrl(label_enum), obj.ToString());
	}

	protected void SetLabelText(Enum label_enum, string text)
	{
		SetLabelText(GetCtrl(label_enum), text);
	}

	protected void SetLabelText(Transform root, Enum label_enum, string text)
	{
		SetLabelText(FindCtrl(root, label_enum), text);
	}

	protected void SetLabelText(Transform root, Enum label_enum, object obj)
	{
		SetLabelText(FindCtrl(root, label_enum), obj.ToString());
	}

	protected void SetLabelText(Transform t, string text)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UILabel>().text = text;
		}
	}

	protected string GetLabelText(Enum label_enum)
	{
		return GetLabelText(GetCtrl(label_enum));
	}

	protected string GetLabelText(Transform root, Enum label_enum)
	{
		return GetLabelText(FindCtrl(root, label_enum));
	}

	protected string GetLabelText(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return string.Empty;
		}
		return t.GetComponent<UILabel>().text;
	}

	protected void SetText(Enum label_enum, string key)
	{
		SetText(GetCtrl(label_enum), key);
	}

	protected void SetText(Transform root, Enum label_enum, string key)
	{
		SetText(FindCtrl(root, label_enum), key);
	}

	protected void SetText(Transform t, string key)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null) && !(sectionData == (GameSceneTables.SectionData)null))
		{
			t.GetComponent<UILabel>().text = sectionData.GetText(key);
		}
	}

	protected void SetLevelText(Enum label_enum, int level, int digit = 3)
	{
		Transform ctrl = GetCtrl(label_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			UILabel component = ctrl.GetComponent<UILabel>();
			component.supportEncoding = true;
			string text = level.ToString();
			int num = digit - text.Length;
			if (num <= 0)
			{
				component.supportEncoding = false;
				component.text = text;
			}
			else
			{
				component.supportEncoding = true;
				component.text = new string('0', num) + text;
			}
		}
	}

	protected void SetApplicationVersionText(Enum label_enum)
	{
		SetLabelText(label_enum, StringTable.Format(STRING_CATEGORY.COMMON, 3u, NetworkNative.getNativeVersionName()));
	}

	protected void SetMaterialNumText(Transform root, Enum have_enum, Enum need_enum, int have_num, int need_num)
	{
		SetMaterialNumText(FindCtrl(root, have_enum), FindCtrl(root, need_enum), have_num, need_num);
	}

	protected void SetMaterialNumText(Enum have_enum, Enum need_enum, int have_num, int need_num)
	{
		SetMaterialNumText(GetCtrl(have_enum), GetCtrl(need_enum), have_num, need_num);
	}

	public static void SetMaterialNumText(Transform have_t, Transform need_t, int have_num, int need_num)
	{
		if (!((UnityEngine.Object)have_t == (UnityEngine.Object)null) && !((UnityEngine.Object)need_t == (UnityEngine.Object)null))
		{
			UILabel component = have_t.GetComponent<UILabel>();
			UILabel component2 = need_t.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && !((UnityEngine.Object)component2 == (UnityEngine.Object)null))
			{
				UIWidget component3 = component2.GetComponent<UIWidget>();
				if (have_num >= need_num)
				{
					component3.color = buffGreen;
				}
				else
				{
					component3.color = Color.red;
				}
				component.text = have_num.ToString();
				component2.text = need_num.ToString();
			}
		}
	}

	protected void SetFontStyle(Transform root, Enum label_enum, FontStyle font_style)
	{
		SetFontStyle(FindCtrl(root, label_enum), font_style);
	}

	protected void SetFontStyle(Enum label_enum, FontStyle font_style)
	{
		SetFontStyle(GetCtrl(label_enum), font_style);
	}

	protected void SetFontStyle(Transform t, FontStyle font_style)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UILabel component = t.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.fontStyle = font_style;
			}
		}
	}

	protected void SetStatusBuffText(Enum label_enum, int value, bool expression_include)
	{
		SetStatusBuffText(GetCtrl(label_enum), value, expression_include);
	}

	protected void SetStatusBuffText(Transform root, Enum label_enum, int value, bool expression_include)
	{
		SetStatusBuffText(FindCtrl(root, label_enum), value, expression_include);
	}

	protected void SetStatusBuffText(Transform t, int value, bool expression_include)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			uint num = 0u;
			num = (uint)((value < 0) ? 1 : 0);
			if (!expression_include)
			{
				num += 2;
			}
			string text = string.Format(StringTable.Get(STRING_CATEGORY.STATUS, num), value.ToString());
			UILabel component = t.GetComponent<UILabel>();
			component.text = text;
			if (value < 0)
			{
				component.color = Color.red;
			}
			else
			{
				component.color = buffGreen;
			}
		}
	}

	protected void SetInput(Enum input_enum, string text, int char_limit, EventDelegate.Callback on_change = null)
	{
		SetInput(GetCtrl(input_enum), text, char_limit, on_change);
	}

	protected void SetInput(Transform root, Enum input_enum, string text, int char_limit, EventDelegate.Callback on_change = null)
	{
		SetInput(FindCtrl(root, input_enum), text, char_limit, on_change);
	}

	protected void SetInput(Transform t, string text, int char_limit, EventDelegate.Callback on_change = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null) && uiFirstUpdate)
		{
			UIInput component = t.GetComponent<UIInput>();
			component.value = text;
			component.defaultText = text;
			component.characterLimit = char_limit;
			if (on_change != null)
			{
				EventDelegate.Add(component.onChange, on_change);
				on_change();
			}
		}
	}

	protected void SetInputValue(Enum input_enum, string value)
	{
		SetInputValue(GetCtrl(input_enum), value);
	}

	protected void SetInputValue(Transform root, Enum input_enum, string value)
	{
		SetInputValue(FindCtrl(root, input_enum), value);
	}

	protected void SetInputValue(Transform t, string value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIInput>().value = value;
		}
	}

	protected void SetInputLabel(Enum input_enum, string value)
	{
		SetInputLabel(GetCtrl(input_enum), value);
	}

	protected void SetInputLabel(Transform root, Enum input_enum, string value)
	{
		SetInputLabel(FindCtrl(root, input_enum), value);
	}

	protected void SetInputLabel(Transform t, string value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIInput component = t.GetComponent<UIInput>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null && (UnityEngine.Object)component.label != (UnityEngine.Object)null)
			{
				component.label.text = value;
			}
		}
	}

	protected void SetInputSubmitEvent(Enum elm, EventDelegate eventDelegate)
	{
		SetInputSubmitEvent(GetCtrl(elm), eventDelegate);
	}

	protected void SetInputSubmitEvent(Transform t, Enum elm, EventDelegate eventDelegate)
	{
		SetInputSubmitEvent(FindCtrl(t, elm), eventDelegate);
	}

	protected void SetInputSubmitEvent(Transform t, EventDelegate eventDelegate)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIInput component = t.GetComponent<UIInput>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.onSubmit.Clear();
				component.onSubmit.Add(eventDelegate);
			}
		}
	}

	protected string GetInputValue(Enum input_enum)
	{
		return GetInputValue(GetCtrl(input_enum));
	}

	protected string GetInputValue(Transform root, Enum input_enum)
	{
		return GetInputValue(FindCtrl(root, input_enum));
	}

	protected string GetInputValue(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return string.Empty;
		}
		UIInput component = t.GetComponent<UIInput>();
		string text = component.value;
		if (component.label.maxLineCount == 1)
		{
			text = text.Replace("\\n", string.Empty);
			text = text.Replace("\n", string.Empty);
			text = text.Replace("\r", string.Empty);
		}
		return text;
	}

	protected void SetSliderValue(Enum input_enum, float value)
	{
		SetSliderValue(GetCtrl(input_enum), value);
	}

	protected void SetSliderValue(Transform root, Enum input_enum, float value)
	{
		SetSliderValue(FindCtrl(root, input_enum), value);
	}

	protected void SetSliderValue(Transform t, float value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UISlider>().value = value;
		}
	}

	protected void SetColor(Enum label_enum, Color color)
	{
		SetColor(GetCtrl(label_enum), color);
	}

	protected void SetColor(Transform root, Enum label_enum, Color color)
	{
		SetColor(FindCtrl(root, label_enum), color);
	}

	protected void SetColor(Transform t, Color color)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIWidget>().color = color;
		}
	}

	protected Color GetColor(Enum label_enum)
	{
		return GetColor(GetCtrl(label_enum));
	}

	protected Color GetColor(Transform root, Enum label_enum)
	{
		return GetColor(FindCtrl(root, label_enum));
	}

	protected Color GetColor(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return Color.white;
		}
		return t.GetComponent<UIWidget>().color;
	}

	protected void SetToggle(Enum toggle_enum, bool value)
	{
		SetToggle(GetCtrl(toggle_enum), value);
	}

	protected void SetToggle(Transform root, Enum toggle_enum, bool value)
	{
		SetToggle(FindCtrl(root, toggle_enum), value);
	}

	protected void SetToggle(Transform t, bool value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIToggle>().value = value;
		}
	}

	protected void SetToggleGroup(Enum toggle_enum, int value)
	{
		SetToggleGroup(GetCtrl(toggle_enum), value);
	}

	protected void SetToggleGroup(Transform root, Enum toggle_enum, int value)
	{
		SetToggleGroup(FindCtrl(root, toggle_enum), value);
	}

	protected void SetToggleGroup(Transform t, int value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIToggle>().group = value;
		}
	}

	protected void SetToggleStartsActive(Enum toggle_enum, bool value)
	{
		SetToggleStartsActive(GetCtrl(toggle_enum), value);
	}

	protected void SetToggleStartsActive(Transform root, Enum toggle_enum, bool value)
	{
		SetToggleStartsActive(FindCtrl(root, toggle_enum), value);
	}

	protected void SetToggleStartsActive(Transform t, bool value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIToggle>().startsActive = value;
		}
	}

	protected void SetToggleButton(Enum toggle_enum, bool is_active, Action<bool> on_changed = null)
	{
		SetToggleButton(GetCtrl(toggle_enum), is_active, on_changed);
	}

	protected void SetToggleButton(Transform root, Enum toggle_enum, bool is_active, Action<bool> on_changed = null)
	{
		SetToggleButton(FindCtrl(root, toggle_enum), is_active, on_changed);
	}

	protected void SetToggleButton(Transform t, bool is_active, Action<bool> on_changed = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIToggleButton component = t.GetComponent<UIToggleButton>();
			component.isActive = is_active;
			component.onChanged = on_changed;
			component.Initialize();
		}
	}

	protected bool IsToggleActive(Enum toggle_enum)
	{
		return IsToggleActive(GetCtrl(toggle_enum));
	}

	protected bool IsToggleActive(Transform root, Enum toggle_enum)
	{
		return IsToggleActive(FindCtrl(root, toggle_enum));
	}

	protected bool IsToggleActive(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return false;
		}
		return t.GetComponent<UIToggleButton>().isActive;
	}

	protected void SetSprite(Enum sprite_enum, string sprite_name)
	{
		SetSprite(GetCtrl(sprite_enum), sprite_name);
	}

	protected void SetSprite(Transform root, Enum sprite_enum, string sprite_name)
	{
		SetSprite(FindCtrl(root, sprite_enum), sprite_name);
	}

	protected void SetSprite(Transform t, string sprite_name)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UISprite>().spriteName = sprite_name;
		}
	}

	protected void SetButtonSprite(Enum sprite_enum, string sprite_name, bool with_press = false)
	{
		SetButtonSprite(GetCtrl(sprite_enum), sprite_name, with_press);
	}

	protected void SetButtonSprite(Transform root, Enum sprite_enum, string sprite_name, bool with_press = false)
	{
		SetButtonSprite(FindCtrl(root, sprite_enum), sprite_name, with_press);
	}

	protected void SetButtonSprite(Transform t, string sprite_name, bool with_press = false)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButton component = t.GetComponent<UIButton>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				if (with_press)
				{
					component.pressedSprite = sprite_name;
				}
				component.normalSprite = sprite_name;
			}
		}
	}

	protected void SetButtonEvent(Enum elm, EventDelegate eventDelegate)
	{
		SetButtonEvent(GetCtrl(elm), eventDelegate);
	}

	protected void SetButtonEvent(Transform t, Enum elm, EventDelegate eventDelegate)
	{
		SetButtonEvent(FindCtrl(t, elm), eventDelegate);
	}

	protected void SetButtonEvent(Transform t, EventDelegate eventDelegate)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButton component = t.GetComponent<UIButton>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.onClick.Clear();
				component.onClick.Add(eventDelegate);
			}
		}
	}

	protected void SetTexture(Enum texture_enum, Texture texture)
	{
		SetTexture(GetCtrl(texture_enum), texture);
	}

	protected void SetTexture(Transform root, Enum texture_enum, Texture texture)
	{
		SetTexture(FindCtrl(root, texture_enum), texture);
	}

	protected void SetTexture(Transform t, Texture texture)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UITexture>().mainTexture = texture;
		}
	}

	protected void SetDownloadTexture(Enum texture_enum, string url)
	{
		SetDownloadTexture(GetCtrl(texture_enum), url);
	}

	protected void SetDownloadTexture(Transform root, Enum texture_enum, string url)
	{
		SetDownloadTexture(FindCtrl(root, texture_enum), url);
	}

	protected void SetDownloadTexture(Transform t, string url)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIDownloadTexture>().url = url;
		}
	}

	protected void SetButtonEnabled(Enum button_enum, bool is_enabled)
	{
		SetButtonEnabled(GetCtrl(button_enum), is_enabled);
	}

	protected void SetButtonEnabled(Transform root, Enum button_enum, bool is_enabled)
	{
		SetButtonEnabled(FindCtrl(root, button_enum), is_enabled);
	}

	protected void SetButtonEnabled(Transform t, bool is_enabled)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButton component = t.gameObject.GetComponent<UIButton>();
			component.isEnabled = is_enabled;
			if (uiUpdateInstant && !is_enabled)
			{
				component.UpdateColor(true);
			}
		}
	}

	protected void SetButtonEnabled(Enum button_enum, bool is_enabled, bool is_update_child_label)
	{
		SetButtonEnabled(GetCtrl(button_enum), is_enabled, is_update_child_label);
	}

	protected void SetButtonEnabled(Transform root, Enum button_enum, bool is_enabled, bool is_update_child_label)
	{
		SetButtonEnabled(FindCtrl(root, button_enum), is_enabled, is_update_child_label);
	}

	protected void SetButtonEnabled(Transform t, bool is_enabled, bool is_update_child_label)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButton button = t.gameObject.GetComponent<UIButton>();
			button.isEnabled = is_enabled;
			if (uiUpdateInstant && !is_enabled)
			{
				button.UpdateColor(true);
				if (is_update_child_label)
				{
					UILabel[] componentsInChildren = t.GetComponentsInChildren<UILabel>();
					Array.ForEach(componentsInChildren, delegate(UILabel child)
					{
						child.color = button.disabledColor;
					});
				}
			}
		}
	}

	protected void SetButtonColor(Enum button_enum, bool is_enabled, bool is_instant)
	{
		SetButtonColor(GetCtrl(button_enum), is_enabled, is_instant);
	}

	protected void SetButtonColor(Transform root, Enum button_enum, bool is_enabled, bool is_instant)
	{
		SetButtonColor(FindCtrl(root, button_enum), is_enabled, is_instant);
	}

	protected void SetButtonColor(Transform t, bool is_enabled, bool is_instant)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButton component = t.gameObject.GetComponent<UIButton>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.GetComponent<UIButtonColor>().SetState(component.state, false);
				if (is_enabled)
				{
					component.ResetDefaultColor();
				}
				else
				{
					component.defaultColor = component.disabledColor;
				}
				component.hover = component.defaultColor;
				component.UpdateColor(is_instant);
			}
		}
	}

	protected void SetLongTouch(Enum button_enum, string event_name, object event_data = null)
	{
		SetLongTouch(GetCtrl(button_enum), event_name, event_data);
	}

	protected void SetLongTouch(Transform root, Enum button_enum, string event_name, object event_data = null)
	{
		SetLongTouch(FindCtrl(root, button_enum), event_name, event_data);
	}

	protected void SetLongTouch(Transform t, string event_name, object event_data = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UILongTouch.Set(t.gameObject, event_name, event_data);
		}
	}

	protected void SetRepeatButton(Enum button_enum, string event_name, object event_data = null)
	{
		SetRepeatButton(GetCtrl(button_enum), event_name, event_data);
	}

	protected void SetRepeatButton(Transform root, Enum button_enum, string event_name, object event_data = null)
	{
		SetRepeatButton(FindCtrl(root, button_enum), event_name, event_data);
	}

	protected void SetRepeatButton(Transform t, string event_name, object event_data = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButtonRepeater.SetRepeatButton(t.gameObject, event_name, event_data);
		}
	}

	protected void TerminateRepeatButton(Enum button_enum)
	{
		TerminateRepeatButton(GetCtrl(button_enum));
	}

	protected void TerminateRepeatButton(Transform root, Enum button_enum)
	{
		TerminateRepeatButton(FindCtrl(root, button_enum));
	}

	protected void TerminateRepeatButton(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIButtonRepeater component = t.GetComponent<UIButtonRepeater>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.Terminate();
			}
		}
	}

	protected void SetTouchAndRelease(Enum button_enum, string touch_event_name, string release_event_name = null, object event_data = null)
	{
		SetTouchAndRelease(GetCtrl(button_enum), touch_event_name, release_event_name, event_data);
	}

	protected void SetTouchAndRelease(Transform t, string touch_event_name, string release_event_name = null, object event_data = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UITouchAndRelease.Set(t.gameObject, touch_event_name, release_event_name, event_data);
		}
	}

	protected void NoEventReleaseTouchAndRelease(Enum button_enum)
	{
		NoEventReleaseTouchAndRelease(GetCtrl(button_enum));
	}

	protected void NoEventReleaseTouchAndRelease(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UITouchAndRelease.NoEventRelease(t.gameObject);
		}
	}

	protected void SetFullScreenButton(Transform root, Enum button_enum)
	{
		SetFullScreenButton(FindCtrl(root, button_enum));
	}

	protected void SetFullScreenButton(Enum button_enum)
	{
		SetFullScreenButton(GetCtrl(button_enum));
	}

	protected void SetFullScreenButton(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIWidget component = t.GetComponent<UIWidget>();
			component.autoResizeBoxCollider = false;
			BoxCollider component2 = t.GetComponent<BoxCollider>();
			BoxCollider boxCollider = component2;
			float x = (float)(MonoBehaviourSingleton<UIManager>.I.uiRoot.manualWidth * 2);
			float y = (float)(MonoBehaviourSingleton<UIManager>.I.uiRoot.manualHeight * 2);
			Vector3 size = component2.size;
			boxCollider.size = new Vector3(x, y, size.z);
		}
	}

	protected void SetBadge(Enum button_enum, int num, SpriteAlignment align, int offset_x = 5, int offset_y = 5, bool is_scale_normalize = false)
	{
		if (!((UnityEngine.Object)MonoBehaviourSingleton<UIManager>.I.common == (UnityEngine.Object)null))
		{
			Transform ctrl = GetCtrl(button_enum);
			SetBadge(ctrl, num, align, offset_x, offset_y, is_scale_normalize);
		}
	}

	protected void SetBadge(Transform t, int num, SpriteAlignment align, int offset_x = 5, int offset_y = 5, bool is_scale_normalize = false)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			MonoBehaviourSingleton<UIManager>.I.common.AttachBadge(t.GetComponent<UIWidget>(), num, align, offset_x, offset_y, is_scale_normalize);
		}
	}

	protected void SetVisibleWidgetEffect(Enum widget_enum, string ui_effect_name)
	{
		SetVisibleWidgetEffect(null, GetCtrl(widget_enum), ui_effect_name);
	}

	protected void SetVisibleWidgetEffect(Enum panel_enum, Enum widget_enum, string ui_effect_name)
	{
		SetVisibleWidgetEffect(GetCtrl(panel_enum), GetCtrl(widget_enum), ui_effect_name);
	}

	protected void SetVisibleWidgetEffect(Enum panel_enum, Transform root, Enum widget_enum, string ui_effect_name)
	{
		SetVisibleWidgetEffect(GetCtrl(panel_enum), FindCtrl(root, widget_enum), ui_effect_name);
	}

	protected void SetVisibleWidgetEffect(Transform t_panel, Transform t_widget, string ui_effect_name)
	{
		if (!((UnityEngine.Object)t_widget == (UnityEngine.Object)null))
		{
			UIVisibleWidgetEffect.Set((!((UnityEngine.Object)t_panel != (UnityEngine.Object)null)) ? null : t_panel.GetComponent<UIPanel>(), t_widget.GetComponent<UIWidget>(), ui_effect_name, (!(sectionData != (GameSceneTables.SectionData)null)) ? null : sectionData.sectionName);
		}
	}

	protected void SetVisibleWidgetOneShotEffect(Transform t_panel, Transform t_widget, string ui_effect_name)
	{
		if (!((UnityEngine.Object)t_widget == (UnityEngine.Object)null))
		{
			UIVisibleWidgetEffect.OneShot((!((UnityEngine.Object)t_panel != (UnityEngine.Object)null)) ? null : t_panel.GetComponent<UIPanel>(), t_widget.GetComponent<UIWidget>(), ui_effect_name, (!(sectionData != (GameSceneTables.SectionData)null)) ? null : sectionData.sectionName);
		}
	}

	protected void SetEventName(Enum ctrl_enum, string event_name)
	{
		SetEventName(GetCtrl(ctrl_enum), event_name);
	}

	protected void SetEventName(Transform root, Enum ctrl_enum, string event_name)
	{
		SetEventName(FindCtrl(root, ctrl_enum), event_name);
	}

	protected void SetEventName(Transform t, string event_name)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIGameSceneEventSender>().eventName = event_name;
		}
	}

	protected void SetEvent(Enum ctrl_enum, string event_name, int event_data)
	{
		SetEvent(GetCtrl(ctrl_enum), event_name, event_data);
	}

	protected void SetEvent(Transform root, Enum ctrl_enum, string event_name, int event_data)
	{
		SetEvent(FindCtrl(root, ctrl_enum), event_name, event_data);
	}

	protected void SetEvent(Transform root, Enum ctrl_enum, string event_name, object event_data)
	{
		SetEvent(FindCtrl(root, ctrl_enum), event_name, event_data);
	}

	protected void SetEvent(Transform t, string event_name, int event_data)
	{
		SetEvent(t, event_name, (object)event_data);
	}

	protected void SetEvent(Transform t, string event_name, object event_data)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIGameSceneEventSender uIGameSceneEventSender = t.GetComponent<UIGameSceneEventSender>();
			if ((UnityEngine.Object)uIGameSceneEventSender == (UnityEngine.Object)null)
			{
				uIGameSceneEventSender = t.gameObject.AddComponent<UIGameSceneEventSender>();
			}
			uIGameSceneEventSender.eventName = event_name;
			uIGameSceneEventSender.eventData = event_data;
		}
	}

	protected void MoveRelativeScrollView(Enum ctrl_enum, Vector3 value)
	{
		MoveRelativeScrollView(GetCtrl(ctrl_enum), value);
	}

	protected void MoveRelativeScrollView(Transform root, Enum ctrl_enum, Vector3 value)
	{
		MoveRelativeScrollView(FindCtrl(root, ctrl_enum), value);
	}

	protected void MoveRelativeScrollView(Transform t, Vector3 _pos)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIScrollView>().MoveRelative(_pos);
		}
	}

	protected void SetScroll(Enum ctrl_enum, float value)
	{
		SetScroll(GetCtrl(ctrl_enum), value);
	}

	protected void SetScroll(Transform root, Enum ctrl_enum, float value)
	{
		SetScroll(FindCtrl(root, ctrl_enum), value);
	}

	protected void SetScroll(Transform t, float value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIScrollView>().Scroll(value);
		}
	}

	protected void ScrollViewResetPosition(Enum ctrl_enum)
	{
		ScrollViewResetPosition(GetCtrl(ctrl_enum));
	}

	protected void ScrollViewResetPosition(Transform root, Enum ctrl_enum)
	{
		ScrollViewResetPosition(FindCtrl(root, ctrl_enum));
	}

	protected void ScrollViewResetPosition(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIScrollView component = t.GetComponent<UIScrollView>();
			bool enabled = component.enabled;
			component.enabled = true;
			component.ResetPosition();
			component.enabled = enabled;
		}
	}

	protected bool IsScrollDragging(Enum ctrl_enum)
	{
		return IsScrollDragging(GetCtrl(ctrl_enum));
	}

	protected bool IsScrollDragging(Transform root, Enum ctrl_enum)
	{
		return IsScrollDragging(FindCtrl(root, ctrl_enum));
	}

	protected bool IsScrollDragging(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return false;
		}
		return t.GetComponent<UIScrollView>().isDragging;
	}

	protected void SetProgressValue(Enum ctrl_enum, float value)
	{
		SetProgressValue(GetCtrl(ctrl_enum), value);
	}

	protected void SetProgressValue(Transform root, Enum ctrl_enum, float value)
	{
		SetProgressValue(FindCtrl(root, ctrl_enum), value);
	}

	protected void SetProgressValue(Transform t, float value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIProgressBar>().value = value;
		}
	}

	protected void SetProgressSteps(Enum ctrl_enum, int value)
	{
		SetProgressSteps(GetCtrl(ctrl_enum), value);
	}

	protected void SetProgressSteps(Transform root, Enum ctrl_enum, int value)
	{
		SetProgressSteps(FindCtrl(root, ctrl_enum), value);
	}

	protected void SetProgressSteps(Transform t, int value)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.GetComponent<UIProgressBar>().numberOfSteps = value;
		}
	}

	protected void SetProgressOnChange(Enum ctrl_enum, EventDelegate.Callback on_change)
	{
		SetProgressOnChange(GetCtrl(ctrl_enum), on_change);
	}

	protected void SetProgressOnChange(Transform root, Enum ctrl_enum, EventDelegate.Callback on_change)
	{
		SetProgressOnChange(FindCtrl(root, ctrl_enum), on_change);
	}

	protected void SetProgressOnChange(Transform t, EventDelegate.Callback on_change)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			EventDelegate.Add(t.GetComponent<UIProgressBar>().onChange, on_change);
		}
	}

	protected void SetProgressInt(Enum ctrl_enum, int val, int min = -1, int max = -1, EventDelegate.Callback on_change = null)
	{
		SetProgressInt(GetCtrl(ctrl_enum), val, min, max, on_change);
	}

	protected void SetProgressInt(Transform root, Enum ctrl_enum, int val, int min = -1, int max = -1, EventDelegate.Callback on_change = null)
	{
		SetProgressInt(FindCtrl(root, ctrl_enum), val, min, max, on_change);
	}

	protected void SetProgressInt(Transform t, int val, int min = -1, int max = -1, EventDelegate.Callback on_change = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIProgressWork uIProgressWork = t.GetComponent<UIProgressWork>();
			if ((UnityEngine.Object)uIProgressWork == (UnityEngine.Object)null)
			{
				uIProgressWork = t.gameObject.AddComponent<UIProgressWork>();
			}
			if (max > -1)
			{
				uIProgressWork.maxValue = max;
			}
			if (min > -1)
			{
				uIProgressWork.minValue = min;
			}
			if (val > -1)
			{
				uIProgressWork.value = val;
			}
			if (on_change != null)
			{
				EventDelegate.Add(uIProgressWork.progress.onChange, on_change);
				on_change();
			}
		}
	}

	protected int GetProgressInt(Enum ctrl_enum)
	{
		return GetProgressInt(GetCtrl(ctrl_enum));
	}

	protected int GetProgressInt(Transform root, Enum ctrl_enum)
	{
		return GetProgressInt(FindCtrl(root, ctrl_enum));
	}

	protected int GetProgressInt(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return 0;
		}
		UIProgressWork component = t.GetComponent<UIProgressWork>();
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			return 0;
		}
		return component.value;
	}

	public void SetCenterOnChildFunc(Enum ctrl_enum, UICenterOnChild.OnCenterCallback func)
	{
		SetCenterOnChildFunc(GetCtrl(ctrl_enum), func);
	}

	public void SetCenterOnChildFunc(Transform root, Enum ctrl_enum, UICenterOnChild.OnCenterCallback func)
	{
		SetCenterOnChildFunc(FindCtrl(root, ctrl_enum), func);
	}

	public void SetCenterOnChildFunc(Transform t, UICenterOnChild.OnCenterCallback func)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UICenterOnChildCtrl.Get(t.gameObject).onCenter = func;
		}
	}

	protected void SetCenterOnChildFunc(Enum ctrl_enum, SpringPanel.OnFinished func)
	{
		SetCenterOnChildFunc(GetCtrl(ctrl_enum), func);
	}

	protected void SetCenterOnChildFunc(Transform root, Enum ctrl_enum, SpringPanel.OnFinished func)
	{
		SetCenterOnChildFunc(FindCtrl(root, ctrl_enum), func);
	}

	protected void SetCenterOnChildFunc(Transform t, SpringPanel.OnFinished func)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UICenterOnChildCtrl.Get(t.gameObject).onFinished = func;
		}
	}

	protected void SetPopupListOnChange(Enum ctrl_enum, Enum ctrl_lbl_enum, EventDelegate.Callback call_back = null)
	{
		SetPopupListOnChange(GetCtrl(ctrl_enum), GetCtrl(ctrl_lbl_enum), call_back);
	}

	protected void SetPopupListOnChange(Transform t, Transform t_lbl, EventDelegate.Callback call_back = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null) && !((UnityEngine.Object)t_lbl == (UnityEngine.Object)null))
		{
			UIPopupList component = t.GetComponent<UIPopupList>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				UILabel component2 = t_lbl.GetComponent<UILabel>();
				if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null))
				{
					component.onChange.Clear();
					EventDelegate.Add(component.onChange, component2.SetCurrentSelection);
					if (call_back != null)
					{
						EventDelegate.Add(component.onChange, call_back);
					}
				}
			}
		}
	}

	protected void SetPopupListText(Enum ctrl_enum, List<string> string_list, int first_index = -1)
	{
		SetPopupListText(GetCtrl(ctrl_enum), string_list, first_index);
	}

	protected void SetPopupListText(Transform t, List<string> string_list, int first_index = -1)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIPopupList component = t.GetComponent<UIPopupList>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				if (string_list == null)
				{
					component.items = new List<string>();
					component.value = string.Empty;
				}
				else
				{
					component.items = string_list;
					if (string_list.Count > 0)
					{
						if (first_index >= string_list.Count)
						{
							first_index = string_list.Count - 1;
						}
						if (first_index < 0)
						{
							first_index = 0;
						}
						component.value = string_list[first_index];
					}
					else
					{
						component.value = string.Empty;
					}
				}
			}
		}
	}

	protected void SetElementSprite(Transform root, Enum ctrl_enum, int elen_type)
	{
		SetElementSprite(FindCtrl(root, ctrl_enum), elen_type);
	}

	protected void SetElementSprite(Enum ctrl_enum, int elen_type)
	{
		SetElementSprite(GetCtrl(ctrl_enum), elen_type);
	}

	protected void SetElementSprite(Transform t, int elen_type)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UISprite component = t.GetComponent<UISprite>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				string elemSpriteName = GetElemSpriteName(elen_type);
				if (string.IsNullOrEmpty(elemSpriteName))
				{
					component.enabled = false;
				}
				else
				{
					component.enabled = true;
					SetSprite(t, elemSpriteName);
				}
			}
		}
	}

	protected void SetDefElementSprite(Transform root, Enum ctrl_enum, int elen_type)
	{
		SetDefElementSprite(FindCtrl(root, ctrl_enum), elen_type);
	}

	protected void SetDefElementSprite(Enum ctrl_enum, int elen_type)
	{
		SetDefElementSprite(GetCtrl(ctrl_enum), elen_type);
	}

	protected void SetDefElementSprite(Transform t, int elen_type)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UISprite component = t.GetComponent<UISprite>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				string elemDefSpriteName = GetElemDefSpriteName(elen_type);
				if (string.IsNullOrEmpty(elemDefSpriteName))
				{
					component.enabled = false;
				}
				else
				{
					component.enabled = true;
					SetSprite(t, elemDefSpriteName);
				}
			}
		}
	}

	public static string GetElemSpriteName(int elem_type)
	{
		if (elem_type == 6)
		{
			return null;
		}
		if (elem_type >= 6)
		{
			return null;
		}
		if (elem_type == -1)
		{
			return elementSpriteName[elementSpriteName.Length - 1];
		}
		return elementSpriteName[elem_type];
	}

	public static string GetElemDefSpriteName(int elem_type)
	{
		if (elem_type == 6)
		{
			return null;
		}
		if (elem_type >= 6)
		{
			return null;
		}
		if (elem_type == -1)
		{
			return elementDefSpriteName[elementSpriteName.Length - 1];
		}
		return elementDefSpriteName[elem_type];
	}

	protected bool SetCenter(Enum table_enum, int index, bool is_instant = false)
	{
		return SetCenter(GetCtrl(table_enum), index, is_instant);
	}

	protected bool SetCenter(Transform root, Enum table_enum, int index, bool is_instant = false)
	{
		return SetCenter(FindCtrl(root, table_enum), index, is_instant);
	}

	protected bool SetCenter(Transform t, int index, bool is_instant = false)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return false;
		}
		UICenterOnChildCtrl uICenterOnChildCtrl = UICenterOnChildCtrl.Get(t.gameObject);
		if ((UnityEngine.Object)uICenterOnChildCtrl == (UnityEngine.Object)null)
		{
			return false;
		}
		if (index < 0 || t.childCount <= index)
		{
			return false;
		}
		if (uiUpdateInstant)
		{
			is_instant = true;
		}
		uICenterOnChildCtrl.Centering(t.GetChild(index), is_instant);
		return true;
	}

	protected Transform GetCenter(Enum table_enum)
	{
		return GetCenter(GetCtrl(table_enum));
	}

	protected bool SetCenter(Transform root, Enum table_enum)
	{
		return GetCenter(FindCtrl(root, table_enum));
	}

	protected Transform GetCenter(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return null;
		}
		UICenterOnChildCtrl uICenterOnChildCtrl = UICenterOnChildCtrl.Get(t.gameObject);
		if ((UnityEngine.Object)uICenterOnChildCtrl == (UnityEngine.Object)null)
		{
			return null;
		}
		return uICenterOnChildCtrl.lastTarget;
	}

	protected void SetScrollArrows(Transform parent, Enum prev_enum, Enum next_enum, int index, int length, Enum prev_btn_enum = null, Enum next_btn_enum = null)
	{
		Transform prev_btn = (prev_btn_enum == null) ? null : FindCtrl(parent, prev_btn_enum);
		Transform next_btn = (next_btn_enum == null) ? null : FindCtrl(parent, next_btn_enum);
		SetScrollArrows(FindCtrl(parent, prev_enum), FindCtrl(parent, next_enum), index, length, prev_btn, next_btn);
	}

	protected void SetScrollArrows(Enum prev_enum, Enum next_enum, int index, int length, Enum prev_btn_enum = null, Enum next_btn_enum = null)
	{
		Transform prev_btn = (prev_btn_enum == null) ? null : GetCtrl(prev_btn_enum);
		Transform next_btn = (next_btn_enum == null) ? null : GetCtrl(next_btn_enum);
		SetScrollArrows(GetCtrl(prev_enum), GetCtrl(next_enum), index, length, prev_btn, next_btn);
	}

	protected void SetScrollArrows(Transform prev, Transform next, int index, int length, Transform prev_btn = null, Transform next_btn = null)
	{
		bool is_enabled = index > 0;
		bool is_enabled2 = index + 1 < length;
		Transform t = prev_btn ?? prev;
		Transform t2 = next_btn ?? next;
		SetEnabled<UISprite>(prev, is_enabled);
		SetButtonEnabled(t, is_enabled);
		SetEnabled<UISprite>(next, is_enabled2);
		SetButtonEnabled(t2, is_enabled2);
	}

	protected void SetActiveListIndex(Enum[] enums, int active_index)
	{
		if (enums != null && enums.Length != 0 && active_index < enums.Length)
		{
			int i = 0;
			for (int num = enums.Length; i < num; i++)
			{
				SetActive(enums[i], i == active_index);
			}
		}
	}

	public void SetTextTalk(Enum lbl_enum, List<string[]> texts, Action page_end_call_back = null, Action<string, string> tag_call_back = null, int num_per_sec = 0)
	{
		SetTextTalk(GetCtrl(lbl_enum), texts, page_end_call_back, tag_call_back, num_per_sec);
	}

	public void SetTextTalk(Transform root, Enum lbl_enum, List<string[]> texts, Action page_end_call_back = null, Action<string, string> tag_call_back = null, int num_per_sec = 0)
	{
		SetTextTalk(FindCtrl(root, lbl_enum), texts, page_end_call_back, tag_call_back, num_per_sec);
	}

	public void SetTextTalk(Transform t, List<string[]> texts, Action page_end_call_back = null, Action<string, string> tag_call_back = null, int num_per_sec = 0)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UILabel component = t.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				TextTalk component2 = t.GetComponent<TextTalk>();
				if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null))
				{
					component2.Initialize(t, texts, page_end_call_back, tag_call_back, num_per_sec);
				}
			}
		}
	}

	public TextTalk GetTextTalk(Enum lbl_enum)
	{
		return GetTextTalk(GetCtrl(lbl_enum));
	}

	public TextTalk GetTextTalk(Transform root, Enum lbl_enum)
	{
		return GetTextTalk(FindCtrl(root, lbl_enum));
	}

	public TextTalk GetTextTalk(Transform t)
	{
		return t.GetComponent<TextTalk>();
	}

	protected void SetRenderPlayerModel(Enum ui_texture_enum, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		SetRenderPlayerModel(GetCtrl(ui_texture_enum), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
	}

	protected void SetRenderPlayerModel(Transform root, Enum ui_texture_enum, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		SetRenderPlayerModel(FindCtrl(root, ui_texture_enum), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
	}

	protected void SetRenderPlayerModel(Transform t, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(t).InitPlayer(t.GetComponent<UITexture>(), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
		}
	}

	protected void SetRenderPlayerModelOneShot(Transform root, Enum ui_texture_enum, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		Transform transform = FindCtrl(root, ui_texture_enum);
		if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
		{
			UIModelRenderTexture uIModelRenderTexture = UIModelRenderTexture.Get(transform);
			if ((UnityEngine.Object)uIModelRenderTexture != (UnityEngine.Object)null)
			{
				uIModelRenderTexture.InitPlayerOneShot(transform.GetComponent<UITexture>(), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
			}
		}
	}

	protected void ForceSetRenderPlayerModel(Enum ui_texture_enum, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		ForceSetRenderPlayerModel(GetCtrl(ui_texture_enum), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
	}

	protected void ForceSetRenderPlayerModel(Transform root, Enum ui_texture_enum, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		ForceSetRenderPlayerModel(FindCtrl(root, ui_texture_enum), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
	}

	protected void ForceSetRenderPlayerModel(Transform t, PlayerLoadInfo info, int anim_id, Vector3 pos, Vector3 rot, bool is_priority_visual_equip, Action<PlayerLoader> onload_callback = null)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			onload_callback?.Invoke(null);
		}
		else
		{
			UIModelRenderTexture.Get(t).ForceInitPlayer(t.GetComponent<UITexture>(), info, anim_id, pos, rot, is_priority_visual_equip, onload_callback);
		}
	}

	protected void SetRenderModel(Enum ui_texture_enum, SortCompareData data)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(ctrl).Init(ctrl.GetComponent<UITexture>(), data);
		}
	}

	protected void SetRenderNPCModel(Enum ui_texture_enum, int npc_id, Vector3 pos, Vector3 rot, float fov = -1, Action<NPCLoader> onload_callback = null)
	{
		SetRenderNPCModel(GetCtrl(ui_texture_enum), npc_id, pos, rot, fov, onload_callback);
	}

	protected void SetRenderNPCModel(Transform root, Enum ui_texture_enum, int npc_id, Vector3 pos, Vector3 rot, float fov = -1, Action<NPCLoader> onload_callback = null)
	{
		SetRenderNPCModel(FindCtrl(root, ui_texture_enum), npc_id, pos, rot, fov, onload_callback);
	}

	protected void SetRenderNPCModel(Transform t, int npc_id, Vector3 pos, Vector3 rot, float fov = -1, Action<NPCLoader> onload_callback = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(t).InitNPC(t.GetComponent<UITexture>(), npc_id, pos, rot, fov, onload_callback);
		}
	}

	protected void SetRenderItemModel(Enum ui_texture_enum, uint item_id)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(ctrl).InitItem(ctrl.GetComponent<UITexture>(), item_id, true);
		}
	}

	protected void SetRenderEquipModel(Transform root, Enum ui_texture_enum, uint equip_item_id, int sex_id = -1, int face_id = -1, float scale = 1)
	{
		SetRenderEquipModel(FindCtrl(root, ui_texture_enum), equip_item_id, sex_id, face_id, scale);
	}

	protected void SetRenderEquipModel(Enum ui_texture_enum, uint equip_item_id, int sex_id = -1, int face_id = -1, float scale = 1)
	{
		SetRenderEquipModel(GetCtrl(ui_texture_enum), equip_item_id, sex_id, face_id, scale);
	}

	protected void SetRenderEquipModel(Transform t, uint equip_item_id, int sex_id = -1, int face_id = -1, float scale = 1)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(t).InitEquip(t.GetComponent<UITexture>(), equip_item_id, sex_id, face_id, scale);
		}
	}

	protected void SetRenderSkillItemModel(Transform root, Enum ui_texture_enum, uint skill_item_id, bool rotation = true, bool light_rotation = false)
	{
		SetRenderSkillItemModel(FindCtrl(root, ui_texture_enum), skill_item_id, rotation, light_rotation);
	}

	protected void SetRenderSkillItemModel(Enum ui_texture_enum, uint skill_item_id, bool rotation = true, bool light_rotation = false)
	{
		SetRenderSkillItemModel(GetCtrl(ui_texture_enum), skill_item_id, rotation, light_rotation);
	}

	protected void SetRenderSkillItemModel(Transform t, uint skill_item_id, bool rotation = true, bool light_rotation = false)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(t).InitSkillItem(t.GetComponent<UITexture>(), skill_item_id, rotation, light_rotation, 35f);
		}
	}

	protected void SetRenderSkillItemSymbolModel(Transform root, Enum ui_texture_enum, uint skill_item_id, bool rotation = true)
	{
		SetRenderSkillItemSymbolModel(FindCtrl(root, ui_texture_enum), skill_item_id, rotation);
	}

	protected void SetRenderSkillItemSymbolModel(Enum ui_texture_enum, uint skill_item_id, bool rotation = true)
	{
		SetRenderSkillItemSymbolModel(GetCtrl(ui_texture_enum), skill_item_id, rotation);
	}

	protected void SetRenderSkillItemSymbolModel(Transform t, uint skill_item_id, bool rotation = true)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UITexture component = t.GetComponent<UITexture>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				UIModelRenderTexture.Get(t).InitSkillItemSymbol(component, skill_item_id, rotation, 13f);
			}
		}
	}

	protected void SetRenderEnemyModel(Transform parent, Enum ui_texture_enum, uint enemy_id, string foundation_name, OutGameSettingsManager.EnemyDisplayInfo.SCENE target_scene, Action<bool, EnemyLoader> callback = null, UIModelRenderTexture.ENEMY_MOVE_TYPE moveType = UIModelRenderTexture.ENEMY_MOVE_TYPE.DEFULT, bool is_Howl = true)
	{
		SetRenderEnemyModel(FindCtrl(parent, ui_texture_enum), enemy_id, foundation_name, target_scene, callback, moveType, is_Howl);
	}

	protected void SetRenderEnemyModel(Enum ui_texture_enum, uint enemy_id, string foundation_name, OutGameSettingsManager.EnemyDisplayInfo.SCENE target_scene, Action<bool, EnemyLoader> callback = null, UIModelRenderTexture.ENEMY_MOVE_TYPE moveType = UIModelRenderTexture.ENEMY_MOVE_TYPE.DEFULT, bool is_Howl = true)
	{
		SetRenderEnemyModel(GetCtrl(ui_texture_enum), enemy_id, foundation_name, target_scene, callback, moveType, is_Howl);
	}

	protected void SetRenderEnemyModel(Transform t, uint enemy_id, string foundation_name, OutGameSettingsManager.EnemyDisplayInfo.SCENE target_scene, Action<bool, EnemyLoader> callback = null, UIModelRenderTexture.ENEMY_MOVE_TYPE moveType = UIModelRenderTexture.ENEMY_MOVE_TYPE.DEFULT, bool is_Howl = true)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			callback?.Invoke(false, null);
		}
		else
		{
			UIModelRenderTexture.Get(t).InitEnemy(t.GetComponent<UITexture>(), enemy_id, foundation_name, target_scene, callback, moveType, is_Howl);
		}
	}

	protected void SetRenderAccessoryModel(Transform root, Enum ui_texture_enum, uint accessory_id, float scale, bool rotation = true, bool light_rotation = false)
	{
		SetRenderAccessoryModel(FindCtrl(root, ui_texture_enum), accessory_id, scale, rotation, light_rotation);
	}

	protected void SetRenderAccessoryModel(Enum ui_texture_enum, uint accessory_id, float scale, bool rotation = true, bool light_rotation = false)
	{
		SetRenderAccessoryModel(GetCtrl(ui_texture_enum), accessory_id, scale, rotation, light_rotation);
	}

	protected void SetRenderAccessoryModel(Transform t, uint accessory_id, float scale, bool rotation = true, bool light_rotation = false)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture.Get(t).InitAccessory(t.GetComponent<UITexture>(), accessory_id, scale, rotation, light_rotation);
		}
	}

	protected void ClearRenderModel(Transform parent, Enum ui_texture_enum)
	{
		ClearRenderModel(FindCtrl(parent, ui_texture_enum));
	}

	protected void ClearRenderModel(Enum ui_texture_enum)
	{
		ClearRenderModel(GetCtrl(ui_texture_enum));
	}

	protected void ClearRenderModel(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture component = t.GetComponent<UIModelRenderTexture>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.Clear();
			}
		}
	}

	protected void SetModel(Transform root, Enum ui_texture_enum, string name)
	{
		SetModel(FindCtrl(root, ui_texture_enum), name);
	}

	protected void SetModel(Enum ui_texture_enum, string name)
	{
		SetModel(GetCtrl(ui_texture_enum), name);
	}

	protected void SetModel(Transform t, string name)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModel.Get(t).Init(name);
		}
	}

	protected void RemoveModel(Transform root, Enum ui_texture_enum)
	{
		RemoveModel(FindCtrl(root, ui_texture_enum));
	}

	protected void RemoveModel(Enum ui_texture_enum)
	{
		RemoveModel(GetCtrl(ui_texture_enum));
	}

	protected void RemoveModel(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModel.Get(t).Remove();
		}
	}

	protected void SetActiveModel(Transform root, Enum ui_texture_enum, bool active)
	{
		SetActiveModel(FindCtrl(root, ui_texture_enum), active);
	}

	protected void SetActiveModel(Enum ui_texture_enum, bool active)
	{
		SetActiveModel(GetCtrl(ui_texture_enum), active);
	}

	protected void SetActiveModel(Transform t, bool active)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModel component = t.GetComponent<UIModel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.SetActive(active);
			}
		}
	}

	public UIRenderTexture InitRenderTexture(Enum ui_texture_enum, float fov = -1f, bool link_main_camera = false)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
		{
			return null;
		}
		UIRenderTexture uIRenderTexture = UIRenderTexture.Get(ctrl.GetComponent<UITexture>(), fov, link_main_camera, -1);
		if ((UnityEngine.Object)uIRenderTexture != (UnityEngine.Object)null)
		{
			uIRenderTexture.Disable();
		}
		return uIRenderTexture;
	}

	protected void EnableRenderTexture(Enum ui_texture_enum)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			UIRenderTexture component = ctrl.GetComponent<UIRenderTexture>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.Enable(0.25f);
			}
		}
	}

	protected void DeleteRenderTexture(Transform root, Enum ui_texture_enum)
	{
		DeleteRenderTexture(FindCtrl(root, ui_texture_enum));
	}

	protected void DeleteRenderTexture(Enum ui_texture_enum)
	{
		DeleteRenderTexture(GetCtrl(ui_texture_enum));
	}

	protected void DeleteRenderTexture(Transform t)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIModelRenderTexture component = t.GetComponent<UIModelRenderTexture>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null)
			{
				component.Clear();
			}
			UIRenderTexture component2 = t.GetComponent<UIRenderTexture>();
			if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
			{
				UnityEngine.Object.DestroyImmediate(component2);
			}
		}
	}

	protected int GetRenderTextureLayer(Enum ui_texture_enum)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
		{
			return 0;
		}
		UIRenderTexture component = ctrl.GetComponent<UIRenderTexture>();
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			return 0;
		}
		return component.renderLayer;
	}

	protected Transform GetRenderTextureModelTransform(Enum ui_texture_enum)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if ((UnityEngine.Object)ctrl == (UnityEngine.Object)null)
		{
			return null;
		}
		UIRenderTexture component = ctrl.GetComponent<UIRenderTexture>();
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			return null;
		}
		return component.modelTransform;
	}

	protected void SetQuestLocationImage(Enum ui_texture_enum, int id, Action on_load_start = null, Action on_load_complete = null)
	{
		Transform ctrl = GetCtrl(ui_texture_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			UIQuestLocationImage.Set(ctrl.GetComponent<UITexture>(), id, on_load_start, on_load_complete);
		}
	}

	protected void SetDirty(Enum ctrl_enum)
	{
		Transform ctrl = GetCtrl(ctrl_enum);
		if (!((UnityEngine.Object)ctrl == (UnityEngine.Object)null))
		{
			ctrl.gameObject.tag = "Dirty";
		}
	}

	protected bool IsDirty(Enum ctrl_enum)
	{
		return IsDirty(GetCtrl(ctrl_enum));
	}

	protected bool IsDirty(Transform root, Enum ctrl_enum)
	{
		return IsDirty(FindCtrl(root, ctrl_enum));
	}

	protected bool IsDirty(Transform t)
	{
		if ((UnityEngine.Object)t == (UnityEngine.Object)null)
		{
			return false;
		}
		return t.gameObject.tag == "Dirty";
	}

	protected void SetSkillIconButton(Transform root, Enum ui_widget_enum, string skill_button_prefab_name, EquipItemTable.EquipItemData equip_item_table, SkillSlotUIData[] skill_tables, string button_event_name = "SKILL_ICON_BUTTON", int button_event_data = 0)
	{
		SettingSkillIconButton(FindCtrl(root, ui_widget_enum), skill_button_prefab_name, equip_item_table, skill_tables, button_event_name, button_event_data);
	}

	protected void SetSkillIconButton(Enum ui_widget_enum, string skill_button_prefab_name, EquipItemTable.EquipItemData equip_item_table, SkillSlotUIData[] skill_tables, string button_event_name = "SKILL_ICON_BUTTON", int button_event_data = 0)
	{
		SettingSkillIconButton(GetCtrl(ui_widget_enum), skill_button_prefab_name, equip_item_table, skill_tables, button_event_name, button_event_data);
	}

	protected void SettingSkillIconButton(Transform t, string prefab_name, EquipItemTable.EquipItemData equip_item_table, SkillSlotUIData[] slot_data, string button_event_name, int button_event_data)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.gameObject.SetActive(true);
			if (!string.IsNullOrEmpty(prefab_name))
			{
				Transform transform = t.FindChild(prefab_name);
				if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
				{
					PrefabData prefabData = GetPrefabData(prefab_name);
					if (prefabData == null)
					{
						Log.Error(LOG.UI, "{0} not found.", prefab_name);
						return;
					}
					transform = prefabData.Realizes(t);
				}
				Transform transform2 = transform.FindChild("SPR_BTN_ENABLE_BG");
				Transform transform3 = transform.FindChild("SPR_BTN_DISABLE_BG");
				Transform transform4 = transform2.FindChild("OBJ_SKILL_SLOT");
				if (equip_item_table == null || slot_data == null)
				{
					if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
					{
						SetEnabled<UIButton>(transform, false);
						int i = 0;
						for (int childCount = transform4.childCount; i < childCount; i++)
						{
							Transform child = transform4.GetChild(i);
							UISprite component = GetComponent<UISprite>(child);
							if ((UnityEngine.Object)component != (UnityEngine.Object)null)
							{
								SetEnabled<UISprite>(child, false);
							}
						}
					}
					transform2.gameObject.SetActive(false);
					transform3.gameObject.SetActive(true);
				}
				else
				{
					transform2.gameObject.SetActive(true);
					transform3.gameObject.SetActive(false);
					if (!string.IsNullOrEmpty(button_event_name))
					{
						SetEnabled<UIButton>(transform, true);
						SetEvent(transform, button_event_name, button_event_data);
						transform.GetComponent<BoxCollider>().enabled = true;
					}
					else
					{
						SetEnabled<UIButton>(transform, false);
						transform.GetComponent<BoxCollider>().enabled = false;
					}
					UIWidget component2 = GetComponent<UIWidget>(transform);
					if (!component2.isAnchored)
					{
						Vector2 vector = new Vector2((float)(component2.width >> 1), (float)(component2.height >> 1));
						component2.SetAnchor(t.gameObject, (int)(0f - vector.x), (int)(0f - vector.y), (int)vector.x, (int)vector.y);
					}
					if ((UnityEngine.Object)transform.GetComponent<UIDragScrollView>() == (UnityEngine.Object)null)
					{
						UIScrollView componentInParent = transform.GetComponentInParent<UIScrollView>();
						if ((UnityEngine.Object)componentInParent != (UnityEngine.Object)null)
						{
							UIDragScrollView uIDragScrollView = transform.gameObject.AddComponent<UIDragScrollView>();
							uIDragScrollView.scrollView = componentInParent;
							if ((UnityEngine.Object)transform.GetComponent<UICenterOnClickChild>() == (UnityEngine.Object)null)
							{
								transform.gameObject.AddComponent<UICenterOnClickChild>();
							}
						}
					}
					int j = 0;
					for (int childCount2 = transform4.childCount; j < childCount2; j++)
					{
						Transform transform5 = transform4.FindChild(j.ToString());
						if (j < slot_data.Length)
						{
							transform5.gameObject.SetActive(true);
							UISprite component3 = transform5.GetComponent<UISprite>();
							component3.enabled = true;
							bool is_attached = slot_data[j].itemData != null && slot_data[j].slotData.slotType == slot_data[j].itemData.tableData.type;
							SetSkillIcon(component3, slot_data[j].slotData.slotType, is_attached, true);
						}
						else if ((UnityEngine.Object)transform5 != (UnityEngine.Object)null)
						{
							transform5.gameObject.SetActive(false);
						}
					}
					UIGrid component4 = transform4.GetComponent<UIGrid>();
					if ((UnityEngine.Object)component4 != (UnityEngine.Object)null)
					{
						component4.Reposition();
					}
				}
			}
		}
	}

	public void SetSkillIcon(Enum ui_enum, SKILL_SLOT_TYPE slot_type, bool is_attached, bool is_button_icon)
	{
		SetSkillIcon(GetComponent<UISprite>(ui_enum), slot_type, is_attached, is_button_icon);
	}

	public void SetSkillIcon(Transform root, Enum ui_enum, SKILL_SLOT_TYPE slot_type, bool is_attached, bool is_button_icon)
	{
		SetSkillIcon(GetComponent<UISprite>(root, ui_enum), slot_type, is_attached, is_button_icon);
	}

	public void SetSkillIcon(UISprite sprite, SKILL_SLOT_TYPE slot_type, bool is_attached, bool is_button_icon)
	{
		if (!((UnityEngine.Object)sprite == (UnityEngine.Object)null))
		{
			sprite.spriteName = GetSkillIconSpriteName(slot_type, is_attached, is_button_icon);
		}
	}

	public static string GetSkillIconSpriteName(SKILL_SLOT_TYPE slot_type, bool is_attached, bool is_button_icon)
	{
		string empty = string.Empty;
		int num = (int)(slot_type - 1);
		if (!is_button_icon)
		{
			if (!is_attached)
			{
				return EMPTY_SKILL_ICON_SPRITE_NAME[num];
			}
			return SKILL_ICON_SPRITE_NAME[num];
		}
		if (!is_attached)
		{
			return EMPTY_SKILL_ICON_EQUIP_SPRITE_NAME[num];
		}
		return SKILL_ICON_SPRITE_NAME[num];
	}

	protected void SetEquipIndexIcon(Transform root, Enum _enum, int index)
	{
		SetEquipIndexIcon(FindCtrl(root, _enum), index);
	}

	protected void SetEquipIndexIcon(Enum _enum, int index)
	{
		SetEquipIndexIcon(GetCtrl(_enum), index);
	}

	protected void SetEquipIndexIcon(Transform t, int index)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null) && index < EQUIP_INDEX_ICON_SP_NAME.Length)
		{
			UISprite component = t.GetComponent<UISprite>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				switch (index)
				{
				case 3:
					index = 4;
					break;
				case 4:
					index = 3;
					break;
				}
				component.spriteName = EQUIP_INDEX_ICON_SP_NAME[index];
			}
		}
	}

	protected void SetEquipmentTypeIcon(Transform root, Enum enum_icon, Enum enum_bg, Enum enum_rarity, EquipItemTable.EquipItemData equip_table)
	{
		SetEquipmentTypeIcon(FindCtrl(root, enum_icon), FindCtrl(root, enum_bg), FindCtrl(root, enum_rarity), equip_table);
	}

	protected void SetEquipmentTypeIcon(Enum enum_icon, Enum enum_bg, Enum enum_rarity, EquipItemTable.EquipItemData equip_table)
	{
		SetEquipmentTypeIcon(GetCtrl(enum_icon), GetCtrl(enum_bg), GetCtrl(enum_rarity), equip_table);
	}

	protected void SetEquipmentTypeIcon(Transform t_icon, Transform t_bg, Transform t_rarity, EquipItemTable.EquipItemData equip_table)
	{
		if (!((UnityEngine.Object)t_icon == (UnityEngine.Object)null) && !((UnityEngine.Object)t_bg == (UnityEngine.Object)null) && !((UnityEngine.Object)t_rarity == (UnityEngine.Object)null))
		{
			bool is_visible = equip_table != null;
			SetActive(t_icon, is_visible);
			SetActive(t_bg, is_visible);
			SetActive(t_rarity, is_visible);
			if (equip_table != null)
			{
				UISprite component = t_icon.GetComponent<UISprite>();
				UISprite component2 = t_bg.GetComponent<UISprite>();
				UISprite component3 = t_rarity.GetComponent<UISprite>();
				if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && !((UnityEngine.Object)component2 == (UnityEngine.Object)null) && !((UnityEngine.Object)component3 == (UnityEngine.Object)null))
				{
					component.spriteName = GetTypeIconSpriteName(equip_table.type);
					SetTypeIconRaritySpriteName(equip_table.rarity, component2, component3, equip_table.getType);
				}
			}
		}
	}

	protected void SetSkillSlotTypeIcon(Transform root, Enum enum_icon, Enum enum_bg, Enum enum_rarity, SkillItemTable.SkillItemData table)
	{
		SetSkillSlotTypeIcon(FindCtrl(root, enum_icon), FindCtrl(root, enum_bg), FindCtrl(root, enum_rarity), table);
	}

	protected void SetSkillSlotTypeIcon(Enum enum_icon, Enum enum_bg, Enum enum_rarity, SkillItemTable.SkillItemData table)
	{
		SetSkillSlotTypeIcon(GetCtrl(enum_icon), GetCtrl(enum_bg), GetCtrl(enum_rarity), table);
	}

	protected void SetSkillSlotTypeIcon(Transform t_icon, Transform t_bg, Transform t_rarity, SkillItemTable.SkillItemData table)
	{
		if (!((UnityEngine.Object)t_icon == (UnityEngine.Object)null) && !((UnityEngine.Object)t_bg == (UnityEngine.Object)null) && !((UnityEngine.Object)t_rarity == (UnityEngine.Object)null))
		{
			bool is_visible = table != null;
			SetActive(t_icon, is_visible);
			SetActive(t_bg, is_visible);
			SetActive(t_rarity, is_visible);
			if (table != null)
			{
				UISprite component = t_icon.GetComponent<UISprite>();
				UISprite component2 = t_bg.GetComponent<UISprite>();
				UISprite component3 = t_rarity.GetComponent<UISprite>();
				if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && !((UnityEngine.Object)component2 == (UnityEngine.Object)null) && !((UnityEngine.Object)component3 == (UnityEngine.Object)null))
				{
					component.spriteName = GetTypeIconSpriteName(table.type);
					SetTypeIconRaritySpriteName(table.rarity, component2, component3, GET_TYPE.PAY);
				}
			}
		}
	}

	protected EQUIPMENT_TYPE GetEquipKindWeaponOrArmor(EQUIPMENT_TYPE type)
	{
		if (type > EQUIPMENT_TYPE.ARMOR)
		{
			return EQUIPMENT_TYPE.ARMOR;
		}
		return type;
	}

	protected void SetSkillEquipIconKind(Transform root, Enum enum_ctrl, EQUIPMENT_TYPE type, bool is_enable)
	{
		SetSkillEquipIconKind(FindCtrl(root, enum_ctrl), type, is_enable);
	}

	protected void SetSkillEquipIconKind(Enum enum_ctrl, EQUIPMENT_TYPE type, bool is_enable)
	{
		SetSkillEquipIconKind(GetCtrl(enum_ctrl), type, is_enable);
	}

	public static void SetSkillEquipIconKind(Transform t, EQUIPMENT_TYPE type, bool is_enable)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UISprite component = t.GetComponent<UISprite>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.spriteName = GetMagiIconSpriteName(type, is_enable);
			}
		}
	}

	protected void SetAccessoryRarityIcon(Transform t_bg, Transform t_rarity, AccessoryTable.AccessoryData table)
	{
		if (!((UnityEngine.Object)t_bg == (UnityEngine.Object)null) && !((UnityEngine.Object)t_rarity == (UnityEngine.Object)null))
		{
			bool is_visible = table != null;
			SetActive(t_bg, is_visible);
			SetActive(t_rarity, is_visible);
			if (table != null)
			{
				UISprite component = t_bg.GetComponent<UISprite>();
				UISprite component2 = t_rarity.GetComponent<UISprite>();
				if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && !((UnityEngine.Object)component2 == (UnityEngine.Object)null))
				{
					SetTypeIconRaritySpriteName(table.rarity, component, component2, table.getType);
				}
			}
		}
	}

	private string GetTypeIconSpriteName(EQUIPMENT_TYPE type)
	{
		return ITEM_TYPE_ICON_SPRITE_NAME[GetEquipmentTypeIndex(type)];
	}

	private string GetTypeIconSpriteName(SKILL_SLOT_TYPE type)
	{
		return SKILL_TYPE_ICON_SPRITE_NAME[(int)(type - 1)];
	}

	private void SetTypeIconRaritySpriteName(RARITY_TYPE rarity, UISprite sp_bg, UISprite sp_rarity, GET_TYPE getType)
	{
		sp_bg.spriteName = ITEM_TYPE_ICON_SPRITE_BG_NAME[(int)rarity];
		SetRarityColorType(rarity, sp_bg);
		sp_rarity.spriteName = ItemIcon.GetRarityTextSpriteName(rarity, getType);
	}

	private static string GetMagiIconSpriteName(EQUIPMENT_TYPE type, bool is_enable)
	{
		int num = GetEquipmentTypeIndex(type);
		if (num >= MAGI_TYPE_ICON_SPRITE_NAME.Length)
		{
			num = MAGI_TYPE_ICON_SPRITE_NAME.Length - 1;
		}
		string arg = MAGI_TYPE_ICON_SPRITE_NAME[num];
		return (!is_enable) ? $"{arg}_off" : $"{arg}_on";
	}

	public static int GetEquipmentTypeIndex(EQUIPMENT_TYPE type)
	{
		ITEM_ICON_TYPE itemIconType = ItemIcon.GetItemIconType(type);
		return Mathf.Max(0, (int)(itemIconType - 1));
	}

	public static void SetRarityColorType(RARITY_TYPE rarity, UIWidget w)
	{
		SetRarityColorType((int)rarity, w);
	}

	public static void SetRarityColorType(int rarity, UIWidget w)
	{
		if (!((UnityEngine.Object)w == (UnityEngine.Object)null))
		{
			switch (rarity)
			{
			case 0:
				w.color = new Color32(184, 243, byte.MaxValue, byte.MaxValue);
				break;
			case 1:
				w.color = new Color32(byte.MaxValue, 165, 131, byte.MaxValue);
				break;
			default:
				w.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
				break;
			}
		}
	}

	protected void SetAbilityItemEvent(Transform t, int index, List<Transform> touchAndReleaseList)
	{
		SetTouchAndRelease(t.GetComponentInChildren<UIButton>().transform, "ABILITY_DATA_POPUP", "RELEASE_ABILITY", new object[2]
		{
			index,
			t
		});
		touchAndReleaseList.Add(t);
	}

	public AbilityDetailPopUp CreateAndGetAbilityDetail(Enum parent_enum)
	{
		return CreateAndGetAbilityDetail(GetCtrl(parent_enum));
	}

	public AbilityDetailPopUp CreateAndGetAbilityDetail(Transform attachRoot)
	{
		return SetPrefab(attachRoot, ABILITY_DETAIL_ITEM_PREFAB_NAME, true).GetComponent<AbilityDetailPopUp>();
	}

	protected void NoEventReleaseTouchAndReleases(List<Transform> touchAndReleaseList)
	{
		if (touchAndReleaseList != null && touchAndReleaseList.Count > 0)
		{
			int i = 0;
			for (int count = touchAndReleaseList.Count; i < count; i++)
			{
				UIButton componentInChildren = touchAndReleaseList[i].GetComponentInChildren<UIButton>();
				if ((bool)componentInChildren)
				{
					NoEventReleaseTouchAndRelease(componentInChildren.transform);
				}
			}
		}
	}

	protected void SetMaterialInfo(Transform root, Enum btn_enum, REWARD_TYPE reward_type, uint id, Transform parent_scroll = null)
	{
		SetMaterialInfo(FindCtrl(root, btn_enum), reward_type, id, parent_scroll);
	}

	protected void SetMaterialInfo(Enum btn_enum, REWARD_TYPE reward_type, uint id, Transform parent_scroll = null)
	{
		SetMaterialInfo(GetCtrl(btn_enum), reward_type, id, parent_scroll);
	}

	protected void SetMaterialInfo(Transform btn_t, REWARD_TYPE reward_type, uint id, Transform parent_scroll = null)
	{
		if (!((UnityEngine.Object)btn_t == (UnityEngine.Object)null))
		{
			Transform material_info = CreateMaterialInfo(btn_t);
			MaterialInfoButton.Set(btn_t, material_info, reward_type, id, sectionData.sectionName, parent_scroll);
		}
	}

	public Transform CreateMaterialInfo(Enum parent_enum)
	{
		return CreateMaterialInfo(GetCtrl(parent_enum));
	}

	public Transform CreateMaterialInfo(Transform parent)
	{
		Transform transform = null;
		GameObject gameObject = GameObject.FindGameObjectWithTag("MaterialInfo");
		if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
		{
			MaterialInfo component = gameObject.GetComponent<MaterialInfo>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.nowSectionName == sectionData.sectionName)
			{
				transform = gameObject.transform;
			}
		}
		if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
		{
			PrefabData prefabData = GetPrefabData("MaterialInfo");
			if (prefabData == null)
			{
				Log.Error(LOG.UI, "{0} not found.", "MaterialInfo");
				return null;
			}
			transform = prefabData.Realizes(parent);
			transform.gameObject.tag = "MaterialInfo";
		}
		return transform;
	}

	protected void DeleteMaterialInfo()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("MaterialInfo");
		if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
		{
			MaterialInfo component = gameObject.GetComponent<MaterialInfo>();
			if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.nowSectionName == sectionData.sectionName)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
	}

	protected void SetLabelCompareParam(Transform root, Enum ui_enum, int value, Enum ui_after_enum, int after_value)
	{
		SetLabelCompareParam(FindCtrl(root, ui_enum), value, FindCtrl(root, ui_after_enum), after_value);
	}

	protected void SetLabelCompareParam(Enum ui_enum, int value, Enum ui_after_enum, int after_value)
	{
		SetLabelCompareParam(GetCtrl(ui_enum), value, GetCtrl(ui_after_enum), after_value);
	}

	protected void SetLabelCompareParam(Transform now_t, int value, Transform after_t, int after_value)
	{
		if (!((UnityEngine.Object)now_t == (UnityEngine.Object)null))
		{
			UILabel component = now_t.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.text = value.ToString();
				SetLabelCompareParam(after_t, after_value, value, -1);
			}
		}
	}

	protected void SetLabelCompareParam(Transform root, Enum ui_after_enum, int after_value, int before_value, int set_after_value = -1)
	{
		SetLabelCompareParam(FindCtrl(root, ui_after_enum), after_value, before_value, set_after_value);
	}

	protected void SetLabelCompareParam(Enum ui_after_enum, int after_value, int before_value, int set_after_value = -1)
	{
		SetLabelCompareParam(GetCtrl(ui_after_enum), after_value, before_value, set_after_value);
	}

	protected void SetLabelCompareParam(Transform t, int after_value, int before_value, int set_after_value = -1)
	{
		if (set_after_value == -1)
		{
			set_after_value = after_value;
		}
		SetLabelCompareParam(t, after_value, before_value, set_after_value.ToString());
	}

	protected void SetLabelCompareParam(Transform root, Enum ui_after_enum, int after_value, int before_value, string set_after_value_string)
	{
		SetLabelCompareParam(FindCtrl(root, ui_after_enum), after_value, before_value, set_after_value_string);
	}

	protected void SetLabelCompareParam(Enum ui_after_enum, int after_value, int before_value, string set_after_value_string)
	{
		SetLabelCompareParam(GetCtrl(ui_after_enum), after_value, before_value, set_after_value_string);
	}

	protected void SetLabelCompareParam(Transform t_after, int after_value, int before_value, string set_after_value_string)
	{
		if (!((UnityEngine.Object)t_after == (UnityEngine.Object)null))
		{
			UILabel component = t_after.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.text = set_after_value_string;
				if (before_value > after_value)
				{
					t_after.GetComponent<UIWidget>().color = Color.red;
				}
				else if (before_value < after_value)
				{
					t_after.GetComponent<UIWidget>().color = buffGreen;
				}
				else
				{
					t_after.GetComponent<UIWidget>().color = Color.white;
				}
			}
		}
	}

	protected void SetLabelDiffParam(Transform root, Enum ui_after_enum, int after_value, Enum ui_diff_enum, int before_value, Enum ui_default_enum, string diff_format = null)
	{
		SetLabelDiffParam(FindCtrl(root, ui_after_enum), after_value, FindCtrl(root, ui_diff_enum), before_value, FindCtrl(root, ui_default_enum), diff_format);
	}

	protected void SetLabelDiffParam(Enum ui_after_enum, int after_value, Enum ui_diff_enum, int before_value, Enum ui_default_enum, string diff_format = null)
	{
		SetLabelDiffParam(GetCtrl(ui_after_enum), after_value, GetCtrl(ui_diff_enum), before_value, GetCtrl(ui_default_enum), diff_format);
	}

	protected void SetLabelDiffParam(Transform now_t, int after_value, Transform diff_t, int before_value, Transform default_t, string diff_format = null)
	{
		int num = after_value - before_value;
		if (num == 0)
		{
			if ((UnityEngine.Object)now_t != (UnityEngine.Object)null)
			{
				SetActive(now_t, false);
			}
			if ((UnityEngine.Object)diff_t != (UnityEngine.Object)null)
			{
				SetActive(diff_t, false);
			}
			if ((UnityEngine.Object)default_t != (UnityEngine.Object)null)
			{
				SetActive(default_t, true);
			}
			SetLabelText(default_t, after_value.ToString());
		}
		else
		{
			if ((UnityEngine.Object)now_t != (UnityEngine.Object)null)
			{
				SetActive(now_t, true);
			}
			if ((UnityEngine.Object)diff_t != (UnityEngine.Object)null)
			{
				SetActive(diff_t, true);
			}
			if ((UnityEngine.Object)default_t != (UnityEngine.Object)null)
			{
				SetActive(default_t, false);
			}
			if (!((UnityEngine.Object)now_t == (UnityEngine.Object)null))
			{
				UILabel component = now_t.GetComponent<UILabel>();
				if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
				{
					component.text = after_value.ToString();
					if (!((UnityEngine.Object)diff_t == (UnityEngine.Object)null))
					{
						UILabel component2 = diff_t.GetComponent<UILabel>();
						if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null))
						{
							string text = ((num <= 0) ? string.Empty : "+") + num;
							if (!string.IsNullOrEmpty(diff_format))
							{
								text = string.Format(diff_format, text);
							}
							component2.text = text;
							SetActive(diff_t, true);
							if (num < 0)
							{
								component2.color = Color.red;
								component.color = Color.red;
							}
							else if (num > 0)
							{
								component2.color = buffGreen;
								component.color = buffGreen;
							}
						}
					}
				}
			}
		}
	}

	protected void SetNextExpValue(Transform root, Enum ui_exp_next_enum, Enum ui_exp_next_root_enum, SkillItemInfo skill_item)
	{
		SetNextExpValue(FindCtrl(root, ui_exp_next_enum), FindCtrl(root, ui_exp_next_root_enum), skill_item);
	}

	protected void SetNextExpValue(Enum ui_exp_next_enum, Enum ui_exp_next_root_enum, SkillItemInfo skill_item)
	{
		SetNextExpValue(GetCtrl(ui_exp_next_enum), GetCtrl(ui_exp_next_root_enum), skill_item);
	}

	protected void SetNextExpValue(Transform t, Transform exp_root_t, SkillItemInfo skill_item)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null) && skill_item != null)
		{
			UILabel component = t.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				if (skill_item.level == skill_item.tableData.GetMaxLv(skill_item.exceedCnt))
				{
					SetActive(exp_root_t, false);
				}
				else
				{
					SetActive(exp_root_t, true);
					component.text = (skill_item.expNext - skill_item.exp).ToString();
				}
			}
		}
	}

	protected string GetInputText(Enum ui_enum)
	{
		UIInput component = GetComponent<UIInput>(ui_enum);
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			return string.Empty;
		}
		return component.value;
	}

	protected void SetNPCIcon(Transform root, Enum _enum, int icon_id, bool is_smile = false)
	{
		SetNPCIcon(FindCtrl(root, _enum), icon_id, is_smile);
	}

	protected void SetNPCIcon(Enum _enum, int icon_id, bool is_smile = false)
	{
		SetNPCIcon(GetCtrl(_enum), icon_id, is_smile);
	}

	protected void SetNPCIcon(Transform t, int icon_id, bool is_smile = false)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UITexture component = t.GetComponent<UITexture>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				ResourceLoad.LoadNPCIconTexture(component, icon_id, is_smile);
			}
		}
	}

	protected void SetEnemyIcon(Transform root, Enum _enum, int icon_id)
	{
		SetEnemyIcon(FindCtrl(root, _enum), icon_id);
	}

	protected void SetEnemyIcon(Enum _enum, int icon_id)
	{
		SetEnemyIcon(GetCtrl(_enum), icon_id);
	}

	protected void SetEnemyIcon(Transform t, int icon_id)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UITexture component = t.GetComponent<UITexture>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				ResourceLoad.LoadEnemyIconTexture(component, icon_id);
			}
		}
	}

	protected void SetEnemyIconGradeFrame(Transform root, Enum _enum, QuestTable.QuestTableData quest_table)
	{
		SetEnemyIconGradeFrame(FindCtrl(root, _enum), quest_table);
	}

	protected void SetEnemyIconGradeFrame(Enum _enum, QuestTable.QuestTableData quest_table)
	{
		SetEnemyIconGradeFrame(GetCtrl(_enum), quest_table);
	}

	protected void SetEnemyIconGradeFrame(Transform t, QuestTable.QuestTableData quest_table)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null) && quest_table != null)
		{
			UISprite component = t.GetComponent<UISprite>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				int rarity = (int)quest_table.rarity;
				if (quest_table.questType != QUEST_TYPE.ORDER)
				{
					component.spriteName = "MonsterCircleN";
				}
				else if (rarity < enemyIconGradeFrameName.Length)
				{
					component.spriteName = enemyIconGradeFrameName[rarity];
				}
				else
				{
					component.spriteName = string.Empty;
				}
			}
		}
	}

	public void SetPageNumText(Transform root, Enum enum_lbl, int page_num)
	{
		SetPageNumText(FindCtrl(root, enum_lbl), page_num);
	}

	public void SetPageNumText(Enum enum_lbl, int page_num)
	{
		SetPageNumText(GetCtrl(enum_lbl), page_num);
	}

	public void SetPageNumText(Transform t, int page_num)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UILabel component = t.GetComponent<UILabel>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				string empty = string.Empty;
				empty = (component.text = ((page_num <= 999) ? page_num.ToString("D3") : StringTable.Get(STRING_CATEGORY.PAGE_UI, 0u)));
			}
		}
	}

	protected void SetTable(Enum table_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UITable>(GetCtrl(table_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	protected void SetTable(Enum table_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UITable>(GetCtrl(table_ctrl_enum), item_prefab_name, item_num, reset, null, create_item_func, item_init_func, false);
	}

	protected void SetTable(Transform root, Enum table_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UITable>(FindCtrl(root, table_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	protected void SetTable(Transform root, Enum table_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UITable>(FindCtrl(root, table_ctrl_enum), item_prefab_name, item_num, reset, null, create_item_func, item_init_func, false);
	}

	protected void SetGrid(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIGrid>(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	protected void SetGrid(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIGrid>(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, null, create_item_func, item_init_func, false);
	}

	protected void SetGrid(Transform root, Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIGrid>(FindCtrl(root, grid_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	protected void SetGrid(Transform root, Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIGrid>(FindCtrl(root, grid_ctrl_enum), item_prefab_name, item_num, reset, null, create_item_func, item_init_func, false);
	}

	protected void SetDynamicList(Transform root, Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, bool> check_item_func, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetDynamicList(FindCtrl(root, grid_ctrl_enum), item_prefab_name, item_num, reset, check_item_func, create_item_func, item_init_func);
	}

	protected void SetDynamicList(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, bool> check_item_func, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetDynamicList(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, check_item_func, create_item_func, item_init_func);
	}

	protected void SetDynamicList(Transform t, string item_prefab_name, int item_num, bool reset, Func<int, bool> check_item_func, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIGrid>(t, item_prefab_name, item_num, reset, check_item_func, create_item_func, item_init_func, true);
	}

	protected void SetWrapContent(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIWrapContent>(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	protected void SetWrapContent(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIWrapContent>(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, null, create_item_func, item_init_func, false);
	}

	protected void SetWrapContent(Transform root, Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<UIWrapContent>(FindCtrl(root, grid_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	protected void SetSimpleContent(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Func<int, bool> check_item_func, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> item_init_func)
	{
		SetItemList<SimpleContent>(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, check_item_func, create_item_func, item_init_func, false);
	}

	protected void SetWrapContentFilter(Enum grid_ctrl_enum, string item_prefab_name, int item_num, bool reset, Action<int, Transform, bool> item_init_func, Func<int, string, bool> filter_item_func)
	{
		SetItemList<UIWrapContentFilter>(GetCtrl(grid_ctrl_enum), item_prefab_name, item_num, reset, null, null, item_init_func, false);
	}

	public void SetWrapContentFilterText(Transform root, Enum enum_lbl, string text)
	{
		SetWrapContentFilterText(FindCtrl(root, enum_lbl), text);
	}

	public void SetWrapContentFilterText(Enum enum_lbl, string text)
	{
		SetWrapContentFilterText(GetCtrl(enum_lbl), text);
	}

	public void SetWrapContentFilterText(Transform t, string text)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UIWrapContentFilter component = t.GetComponent<UIWrapContentFilter>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				component.filter = text;
			}
		}
	}

	private void SetItemList<T>(Transform item_list_transform, string item_prefab_name, int item_num, bool reset, Func<int, bool> check_item_func, Func<int, Transform, Transform> create_item_func, Action<int, Transform, bool> init_item_func, bool is_dynamic) where T : Component
	{
		if (!((UnityEngine.Object)item_list_transform == (UnityEngine.Object)null))
		{
			T component = item_list_transform.gameObject.GetComponent<T>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				UITable uITable = component as UITable;
				UIGrid uIGrid = component as UIGrid;
				UIWrapContent uIWrapContent = component as UIWrapContent;
				UIWrapContentFilter uIWrapContentFilter = component as UIWrapContentFilter;
				PrefabData prefabData = null;
				if (!string.IsNullOrEmpty(item_prefab_name))
				{
					prefabData = GetPrefabData(item_prefab_name);
					if (prefabData == null)
					{
						Log.Error(LOG.UI, "{0} not found.", item_prefab_name);
						return;
					}
				}
				UIScrollView component2 = item_list_transform.parent.gameObject.GetComponent<UIScrollView>();
				UIPanel uIPanel = null;
				if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
				{
					component2.enabled = true;
					uIPanel = component2.GetComponent<UIPanel>();
				}
				UICenterOnChild component3 = item_list_transform.GetComponent<UICenterOnChild>();
				if (item_list_transform.gameObject.tag == "Dirty")
				{
					item_list_transform.gameObject.tag = "Untagged";
					reset = true;
				}
				else if (uiFirstUpdate)
				{
					reset = true;
				}
				UIDynamicList uIDynamicList = null;
				if (is_dynamic)
				{
					uIDynamicList = UIDynamicList.Set(component2, item_num, prefabData?.prefab, (UnityEngine.Object)component3 != (UnityEngine.Object)null, create_item_func, init_item_func);
				}
				int childCount = item_list_transform.childCount;
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < item_num; i++)
				{
					if (check_item_func == null || check_item_func(i))
					{
						bool arg;
						Transform transform;
						if (num >= childCount)
						{
							arg = false;
							if ((UnityEngine.Object)uIDynamicList != (UnityEngine.Object)null)
							{
								GameObject gameObject = new GameObject();
								gameObject.layer = 5;
								UIWidget uIWidget = gameObject.AddComponent<UIWidget>();
								uIWidget.SetDimensions((int)uIGrid.cellWidth, (int)uIGrid.cellHeight);
								transform = gameObject.transform;
								transform.SetParent(item_list_transform, false);
								uIDynamicList.AddItemWidget(uIWidget);
							}
							else if (prefabData != null || create_item_func != null)
							{
								transform = null;
								if (create_item_func != null)
								{
									transform = create_item_func(i, item_list_transform);
								}
								if ((UnityEngine.Object)transform == (UnityEngine.Object)null && prefabData != null)
								{
									transform = _Realizes(prefabData, item_list_transform, true);
								}
								if ((UnityEngine.Object)transform != (UnityEngine.Object)null && (UnityEngine.Object)uIPanel != (UnityEngine.Object)null)
								{
									UIPanel componentInChildren = transform.GetComponentInChildren<UIPanel>();
									if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null)
									{
										componentInChildren.depth = uIPanel.depth + 1;
									}
								}
							}
							else
							{
								GameObject gameObject2 = new GameObject();
								gameObject2.layer = 5;
								transform = gameObject2.transform;
								transform.parent = item_list_transform;
								transform.localPosition = Vector3.zero;
								transform.localScale = Vector3.one;
								if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
								{
									gameObject2.AddComponent<UIDragScrollView>().scrollView = component2;
								}
							}
							if ((UnityEngine.Object)component3 != (UnityEngine.Object)null)
							{
								UIUtility.AddCenterOnClickChild(transform);
							}
						}
						else
						{
							arg = true;
							transform = item_list_transform.GetChild(num);
							if ((UnityEngine.Object)component2 != (UnityEngine.Object)null && (UnityEngine.Object)component2.transform.GetChild(0) != (UnityEngine.Object)null)
							{
								UITweenAddToChildrenCtrl component4 = component2.transform.GetChild(0).GetComponent<UITweenAddToChildrenCtrl>();
								if ((UnityEngine.Object)component4 != (UnityEngine.Object)null && component4.enabled && (UnityEngine.Object)transform.GetComponent<UITweenAddCtrlChild>() != (UnityEngine.Object)null)
								{
									transform = transform.GetChild(0);
								}
							}
							transform.gameObject.SetActive(true);
							if ((UnityEngine.Object)uIDynamicList != (UnityEngine.Object)null)
							{
								uIDynamicList.AddItemWidget(transform.GetComponent<UIWidget>());
							}
							num++;
						}
						if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
						{
							return;
						}
						transform.name = i.ToString();
						if ((UnityEngine.Object)uIDynamicList == (UnityEngine.Object)null)
						{
							init_item_func(i, transform, arg);
							UIUtility.UpdateAnchors(transform);
						}
						if (transform.gameObject.activeSelf)
						{
							num2++;
						}
					}
				}
				for (int num3 = childCount - 1; num3 >= num; num3--)
				{
					UnityEngine.Object.DestroyImmediate(item_list_transform.GetChild(num3).gameObject);
				}
				UITweenAddToChildrenCtrl uITweenAddToChildrenCtrl = null;
				if ((UnityEngine.Object)uITable != (UnityEngine.Object)null)
				{
					if (reset && uITable.keepWithinPanel && uITable.pivot == UIWidget.Pivot.TopLeft)
					{
						uITable.transform.localPosition = new Vector3(-9999f, 9999f, 0f);
					}
					uITable.Reposition();
					uITweenAddToChildrenCtrl = uITable.GetComponent<UITweenAddToChildrenCtrl>();
				}
				else if ((UnityEngine.Object)uIGrid != (UnityEngine.Object)null)
				{
					if (reset && uIGrid.keepWithinPanel && uIGrid.pivot == UIWidget.Pivot.TopLeft)
					{
						uIGrid.transform.localPosition = new Vector3(-9999f, 9999f, 0f);
					}
					uIGrid.Reposition();
					if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
					{
						UIUtility.SetGridItemsDraggableWidget(component2, uIGrid, num2);
					}
					uITweenAddToChildrenCtrl = uIGrid.GetComponent<UITweenAddToChildrenCtrl>();
				}
				if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
				{
					if (reset)
					{
						component2.ResetPosition();
					}
					ActivateScrollBarCollider(component2, true);
					if ((component2.canMoveHorizontally && !component2.shouldMoveHorizontally) || (component2.canMoveVertically && !component2.shouldMoveVertically))
					{
						component2.enabled = false;
						if (component2.showScrollBars != 0)
						{
							if ((UnityEngine.Object)component2.horizontalScrollBar != (UnityEngine.Object)null)
							{
								component2.horizontalScrollBar.alpha = 0f;
							}
							if ((UnityEngine.Object)component2.verticalScrollBar != (UnityEngine.Object)null)
							{
								component2.verticalScrollBar.alpha = 0f;
								ActivateScrollBarCollider(component2, false);
							}
						}
					}
				}
				if ((UnityEngine.Object)uIDynamicList != (UnityEngine.Object)null)
				{
					if ((UnityEngine.Object)uITweenAddToChildrenCtrl != (UnityEngine.Object)null)
					{
						uITweenAddToChildrenCtrl.SkipTween();
					}
					uIDynamicList.UpdateItems();
					if ((UnityEngine.Object)component2 != (UnityEngine.Object)null && reset)
					{
						component2.ResetPosition();
						component2.MoveAbsolute(Vector3.zero);
					}
				}
				if ((UnityEngine.Object)uIWrapContent != (UnityEngine.Object)null)
				{
					if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
					{
						component2.ResetPosition();
					}
					uIWrapContent.SortAlphabetically();
				}
				else if ((UnityEngine.Object)uIWrapContentFilter != (UnityEngine.Object)null)
				{
					if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
					{
						component2.ResetPosition();
					}
					uIWrapContentFilter.Initialize(null);
				}
				if ((UnityEngine.Object)uITweenAddToChildrenCtrl != (UnityEngine.Object)null && reset)
				{
					uITweenAddToChildrenCtrl.TweenAdd();
				}
			}
		}
	}

	protected void InitTween(Enum ctrl_enum)
	{
		InitTween(GetCtrl(ctrl_enum), false, null);
	}

	protected void InitTween(Transform root, Enum ctrl_enum)
	{
		InitTween(FindCtrl(root, ctrl_enum), false, null);
	}

	protected void InitTween(Transform t, bool reverse = false, EventDelegate.Callback callback = null)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			UITweenCtrl.Set(t);
		}
	}

	protected void PlayTween(Enum ctrl_enum, bool forward = true, EventDelegate.Callback callback = null, bool is_input_block = true, int tween_ctrl_id = 0)
	{
		PlayTween(GetCtrl(ctrl_enum), forward, callback, is_input_block, tween_ctrl_id);
	}

	protected void PlayTween(Transform root, Enum ctrl_enum, bool forward = true, EventDelegate.Callback callback = null, bool is_input_block = true, int tween_ctrl_id = 0)
	{
		PlayTween(FindCtrl(root, ctrl_enum), forward, callback, is_input_block, tween_ctrl_id);
	}

	protected void PlayTween(Transform t, bool forward = true, EventDelegate.Callback callback = null, bool is_input_block = true, int tween_ctrl_id = 0)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			if (forward)
			{
				t.gameObject.tag = "Dirty";
			}
			else
			{
				t.gameObject.tag = "Untagged";
			}
			UITweenCtrl.Play(t, forward, callback, is_input_block, tween_ctrl_id);
		}
	}

	protected void SkipTween(Enum ctrl_enum, bool forward = true, int tween_ctrl_id = 0)
	{
		SkipTween(GetCtrl(ctrl_enum), forward, tween_ctrl_id);
	}

	protected void SkipTween(Transform root, Enum ctrl_enum, bool forward = true, int tween_ctrl_id = 0)
	{
		SkipTween(FindCtrl(root, ctrl_enum), forward, tween_ctrl_id);
	}

	protected void SkipTween(Transform t, bool forward = true, int tween_ctrl_id = 0)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			if (forward)
			{
				t.gameObject.tag = "Dirty";
			}
			else
			{
				t.gameObject.tag = "Untagged";
			}
			UITweenCtrl.Skip(t, forward, tween_ctrl_id);
		}
	}

	protected void ResetTween(Enum ctrl_enum, int tween_ctrl_id = 0)
	{
		ResetTween(GetCtrl(ctrl_enum), tween_ctrl_id);
	}

	protected void ResetTween(Transform root, Enum ctrl_enum, int tween_ctrl_id = 0)
	{
		ResetTween(FindCtrl(root, ctrl_enum), tween_ctrl_id);
	}

	protected void ResetTween(Transform t, int tween_ctrl_id = 0)
	{
		if (!((UnityEngine.Object)t == (UnityEngine.Object)null))
		{
			t.gameObject.tag = "Untagged";
			UITweenCtrl.Reset(t, tween_ctrl_id);
		}
	}

	protected void InitUITweener<T>(Transform root, Enum ctrl_enum, bool is_enable, EventDelegate.Callback on_finish = null) where T : UITweener
	{
		InitUITweener<T>(FindCtrl(root, ctrl_enum), is_enable, on_finish);
	}

	protected void InitUITweener<T>(Enum ctrl_enum, bool is_enable, EventDelegate.Callback on_finish = null) where T : UITweener
	{
		InitUITweener<T>(GetCtrl(ctrl_enum), is_enable, on_finish);
	}

	protected void InitUITweener<T>(Transform t, bool is_enable, EventDelegate.Callback on_finish = null) where T : UITweener
	{
		T component = GetComponent<T>(t);
		if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
		{
			component.ResetToBeginning();
			if (on_finish != null)
			{
				component.SetOnFinished(on_finish);
			}
			component.enabled = is_enable;
		}
	}

	protected virtual GameSection.NOTIFY_FLAG GetUpdateUINotifyFlags()
	{
		return (GameSection.NOTIFY_FLAG)0L;
	}

	public void Open(UITransition.TYPE type = UITransition.TYPE.OPEN)
	{
		if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
		{
			transferUI.Open(type);
		}
		else if (_state == STATE.CLOSE)
		{
			uiVisible = true;
			uiUpdateInstant = true;
			OnOpen();
			if (ctrls != null)
			{
				RefreshUI();
			}
			uiFirstUpdate = false;
			uiUpdateInstant = false;
			if (transitions != null && transitions.Count > 0)
			{
				int i = 0;
				for (int count = transitions.Count; i < count; i++)
				{
					transitions[i].Play(type, OnOpened);
				}
				_state = STATE.TO_OPEN;
			}
			else
			{
				OnOpened();
			}
		}
	}

	private void OnOpened()
	{
		if (transitions != null)
		{
			int i = 0;
			for (int count = transitions.Count; i < count; i++)
			{
				if (transitions[i].isBusy)
				{
					return;
				}
			}
		}
		_state = STATE.OPEN;
	}

	protected virtual void OnOpen()
	{
	}

	public virtual void Close(UITransition.TYPE type = UITransition.TYPE.CLOSE)
	{
		if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
		{
			transferUI.Close(type);
		}
		else if (_state == STATE.OPEN)
		{
			OnCloseStart();
			if (transitions != null && transitions.Count > 0)
			{
				int i = 0;
				for (int count = transitions.Count; i < count; i++)
				{
					transitions[i].Play(type, OnClosed);
				}
				_state = STATE.TO_CLOSE;
			}
			else
			{
				OnClosed();
			}
		}
	}

	private void OnClosed()
	{
		if (transitions != null)
		{
			int i = 0;
			for (int count = transitions.Count; i < count; i++)
			{
				if (transitions[i].isBusy)
				{
					return;
				}
			}
		}
		uiVisible = false;
		_state = STATE.CLOSE;
		OnClose();
	}

	protected virtual void OnCloseStart()
	{
	}

	protected virtual void OnClose()
	{
	}

	public void RefreshUI()
	{
		if ((UnityEngine.Object)transferUI != (UnityEngine.Object)null)
		{
			transferUI.RefreshUI();
		}
		else if ((UnityEngine.Object)collectUI == (UnityEngine.Object)null || collectUI.gameObject.activeInHierarchy)
		{
			UpdateUI();
		}
	}

	public virtual void UpdateUI()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			Log.Error(LOG.UI, "UpdateUI : activeInHierarchy = false");
		}
	}

	public virtual void OnNotify(GameSection.NOTIFY_FLAG flags)
	{
		if ((GetUpdateUINotifyFlags() & flags) != (GameSection.NOTIFY_FLAG)0L)
		{
			RefreshUI();
		}
	}

	public virtual void OnModifyChat(MainChat.NOTIFY_FLAG flag)
	{
	}

	public virtual string GetCaptionText()
	{
		return null;
	}

	public void PlayAudio(Enum audio_label, float volume = 1f, bool as_jingle = false)
	{
		if ((UnityEngine.Object)resourceLink == (UnityEngine.Object)null)
		{
			Log.Error("resourceLinkがありません");
		}
		else
		{
			int num = Convert.ToInt32(audio_label);
			PlayAudio(num, volume, num, as_jingle);
		}
	}

	public void PlayAudio(int se_id, float volume, int config_id, bool as_jingle)
	{
		string sE = ResourceName.GetSE(se_id);
		int config_id2 = (config_id == 0) ? se_id : config_id;
		AudioClip audioClip = resourceLink.Get<AudioClip>(sE);
		if ((UnityEngine.Object)audioClip == (UnityEngine.Object)null)
		{
			Log.Error("AudioClip{0}がありません", sE);
		}
		else if (as_jingle)
		{
			SoundManager.PlayUISE(audioClip, volume, false, null, config_id2);
		}
		else
		{
			SoundManager.PlayOneshotJingle(audioClip, se_id, null, null);
		}
	}

	public void CacheAudio(LoadingQueue load_queue)
	{
		Type type = Type.GetType(base.name + "+AUDIO");
		if (type != null)
		{
			int[] array = (int[])Enum.GetValues(type);
			int[] array2 = array;
			foreach (int se_id in array2)
			{
				load_queue.CacheSE(se_id, null);
			}
		}
	}

	public T[] GetPagingList<T>(T[] list, int numInPage, int nowPage) where T : class
	{
		int num = 1 + (list.Length - 1) / numInPage;
		int num2 = numInPage * (nowPage - 1);
		int num3 = (nowPage != num) ? numInPage : (list.Length - num2);
		T[] array = new T[num3];
		Array.Copy(list, num2, array, 0, num3);
		return array;
	}

	private void ActivateScrollBarCollider(UIScrollView scroll_view, bool activate)
	{
		if ((UnityEngine.Object)scroll_view.verticalScrollBar != (UnityEngine.Object)null)
		{
			GameObject gameObject = scroll_view.verticalScrollBar.gameObject;
			if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null)
			{
				Collider component = gameObject.GetComponent<Collider>();
				if ((UnityEngine.Object)component != (UnityEngine.Object)null)
				{
					component.enabled = activate;
				}
			}
		}
	}
}
