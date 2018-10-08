using Network;
using rhyme;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeLoginBonusTheater : GameSection
{
	public enum AUDIO
	{
		SE_TOP = 40000040,
		SE_FIRE
	}

	public enum VOICE
	{
		PAMERA_GREET_0 = 14,
		PAMERA_GREET_1 = 0xF,
		PAMERA_GREET_2 = 0x10,
		PAMERA_GREET_3 = 19,
		PAMERA_CHEER_0 = 301,
		PAMERA_CHEER_1 = 302,
		PAMERA_CHEER_2 = 303
	}

	private class FSMInfo
	{
		public float deltaTime;

		public HomeNPCCharacter npc00;

		public HomeNPCCharacter npc06;

		public Transform light;

		public Transform dragonJaw;

		public Transform fireball;

		public Transform fireEffect1;

		public Transform fireEffect2;

		public Transform itemModel;

		public Vector3 fireballStartPos = Vector3.zero;

		public Vector3 fireballEndPos = Vector3.zero;

		public int dayIndex;

		public Material todayPaper;

		public Material todayPanel;

		public Material todayPanel2;

		public TransformInterpolator interpolator;

		public AnimationCurve moveCurve;

		public AnimationCurve scaleCurve;

		public bool goNextMain;

		public bool goNextNpc00;

		public bool goNextNpc00Facial;

		public bool goNextBoard;

		public bool goNextNpc06;

		public bool goNextFireball;

		public bool goNextCamera;

		public Vector3 previousNPC00Position;

		public Quaternion previousNPC00Rotation;
	}

	private abstract class FSM
	{
		protected delegate bool Act();

		protected Act act_;

		protected float waitTime_;

		protected FSMInfo info_;

		public virtual void Init(FSMInfo info)
		{
			info_ = info;
		}

		public bool DoAction()
		{
			waitTime_ -= info_.deltaTime;
			if (act_ != null)
			{
				return act_();
			}
			return false;
		}

		protected bool IsWaitComplete()
		{
			return 0f >= waitTime_;
		}

		protected void StartTimer(float time)
		{
			waitTime_ = time;
		}

		protected virtual bool PhaseExit()
		{
			return false;
		}

		protected void ToExit()
		{
			act_ = PhaseExit;
		}
	}

	private class FSMNpc00 : FSM
	{
		private HomeNPCCharacter npc00_;

		private PlayerAnimCtrl npc00AnimCtrl_;

		private Action<PlayerAnimCtrl, PLCA> tempAction_;

		private Vector3 previousNPC00Position_;

		private Quaternion previousNPC00Rotation_;

		public override void Init(FSMInfo info)
		{
			base.Init(info);
			act_ = Phase00;
			npc00_ = info.npc00;
			npc00AnimCtrl_ = npc00_.gameObject.GetComponentInChildren<PlayerAnimCtrl>();
			tempAction_ = npc00AnimCtrl_.onEnd;
			npc00AnimCtrl_.onEnd = null;
			npc00AnimCtrl_.Play(PLCA.BOW, false);
			previousNPC00Position_ = info.previousNPC00Position;
			previousNPC00Rotation_ = info.previousNPC00Rotation;
			PlayRandomVoice(voiceGreetings);
		}

		private bool Phase00()
		{
			if (npc00AnimCtrl_.IsPlayingIdleAnims(0))
			{
				info_.goNextCamera = true;
				act_ = Phase01;
			}
			return true;
		}

		private bool Phase01()
		{
			if (info_.goNextNpc00)
			{
				info_.goNextNpc00 = false;
				StartTimer(0.15f);
				act_ = Phase02;
			}
			return true;
		}

		private bool Phase02()
		{
			if (IsWaitComplete())
			{
				npc00AnimCtrl_.Play(PLCA.HAPPY, false);
				StartTimer(3.5f);
				act_ = Phase03;
			}
			return true;
		}

		private bool Phase03()
		{
			if (npc00AnimCtrl_.IsCurrentState(PLCA.HAPPY) || IsWaitComplete())
			{
				npc00AnimCtrl_.onEnd = tempAction_;
				info_.goNextNpc00Facial = true;
				act_ = Phase04;
				StartTimer(2f);
			}
			return true;
		}

		private bool Phase04()
		{
			if (IsWaitComplete())
			{
				info_.goNextMain = true;
				act_ = Phase99;
			}
			return true;
		}

		private bool Phase99()
		{
			if (info_.goNextNpc00)
			{
				info_.goNextNpc00 = false;
				npc00_.defaultPosition = previousNPC00Position_;
				npc00_.defaultRotation = previousNPC00Rotation_;
				npc00_.PopState();
				npc00_.PushBackPosition();
				ToExit();
			}
			return true;
		}
	}

	private class FSMNpc00Facial : FSM
	{
		private NPCFacial facial_;

		public override void Init(FSMInfo info)
		{
			base.Init(info);
			act_ = Phase00;
			facial_ = info.npc00.gameObject.GetComponentInChildren<NPCFacial>();
		}

		private bool Phase00()
		{
			if (info_.goNextNpc00Facial)
			{
				info_.goNextNpc00Facial = false;
				facial_.eyeType = NPCFacial.TYPE.JOY;
				facial_.mouthType = NPCFacial.TYPE.JOY;
				act_ = Phase01;
				StartTimer(2f);
			}
			return true;
		}

		private bool Phase01()
		{
			if (IsWaitComplete())
			{
				facial_.eyeType = NPCFacial.TYPE.NORMAL;
				facial_.mouthType = NPCFacial.TYPE.NORMAL;
				ToExit();
			}
			return true;
		}
	}

	private class FSMNpc06 : FSM
	{
		private HomeNPCCharacter npc06_;

		private PlayerAnimCtrl npc06AnimCtrl_;

		private Transform jaw_;

		private PLCA tempDefaultAnim_;

		private Action<PlayerAnimCtrl, PLCA> tempAction_;

		public override void Init(FSMInfo info)
		{
			base.Init(info);
			act_ = Phase00;
			npc06_ = info.npc06;
			npc06AnimCtrl_ = npc06_.gameObject.GetComponentInChildren<PlayerAnimCtrl>();
			jaw_ = Utility.Find(npc06_._transform, "Jaw");
			info_.dragonJaw = jaw_;
			tempDefaultAnim_ = npc06AnimCtrl_.defaultAnim;
			npc06AnimCtrl_.defaultAnim = PLCA.LOGIN_IDLE;
			tempAction_ = npc06AnimCtrl_.onEnd;
			npc06AnimCtrl_.onEnd = null;
		}

		private bool Phase00()
		{
			if (info_.goNextNpc06)
			{
				info_.goNextNpc06 = false;
				npc06AnimCtrl_.Play(PLCA.LOGIN_FIRE, false);
				act_ = Phase01;
				info_.goNextFireball = true;
			}
			return true;
		}

		private bool Phase01()
		{
			if (info_.goNextNpc06)
			{
				info_.goNextNpc06 = false;
				npc06AnimCtrl_.defaultAnim = tempDefaultAnim_;
				npc06AnimCtrl_.onEnd = tempAction_;
				npc06AnimCtrl_.moveAnim = PLCA.LOGIN_FLY;
				npc06AnimCtrl_.Play(PLCA.LOGIN_FLY, false);
				npc06_.PopState();
				npc06_.PushLeave();
				ToExit();
			}
			return true;
		}
	}

	private class FSMFireball : FSM
	{
		private Transform fireball_;

		private Transform fireEffect1_;

		private Transform fireEffect2_;

		private Vector3 endPos_ = Vector3.zero;

		private Vector3 dir_ = Vector3.up;

		private float velocity_ = 0.098f;

		private Vector3 position_;

		public override void Init(FSMInfo info)
		{
			base.Init(info);
			act_ = Phase00;
			fireball_ = info.fireball;
			fireEffect1_ = info.fireEffect1;
			fireEffect2_ = info.fireEffect2;
			endPos_ = info.fireballEndPos;
			fireEffect1_.position = endPos_;
			fireEffect2_.position = endPos_;
		}

		private bool Phase00()
		{
			if (info_.goNextFireball)
			{
				info_.goNextFireball = false;
				float time = 1.06f;
				if (info_.dayIndex % 3 == 1)
				{
					time = 0.99f;
				}
				else if (info_.dayIndex % 3 == 2)
				{
					time = 0.9f;
				}
				StartTimer(time);
				act_ = Phase01;
			}
			return true;
		}

		private bool Phase01()
		{
			if (IsWaitComplete())
			{
				fireball_.gameObject.SetActive(true);
				position_ = info_.dragonJaw.TransformPoint(new Vector3(-0.12f, 0f, 0.01f));
				dir_ = endPos_ - position_;
				dir_.Normalize();
				fireball_.position = position_;
				act_ = Phase02;
				PlayAudio(AUDIO.SE_FIRE);
			}
			return true;
		}

		private bool Phase02()
		{
			position_ += dir_ * velocity_;
			fireball_.position = position_;
			Vector3 lhs = endPos_ - position_;
			lhs.Normalize();
			if (0.8f >= Vector3.Dot(lhs, dir_))
			{
				position_ = endPos_;
				fireball_.position = endPos_;
				rymFX component = fireball_.gameObject.GetComponent<rymFX>();
				if ((UnityEngine.Object)component != (UnityEngine.Object)null)
				{
					component.AutoDelete = true;
					component.LoopEnd = true;
				}
				fireEffect1_.gameObject.SetActive(true);
				fireEffect2_.gameObject.SetActive(true);
				info_.goNextBoard = true;
				info_.goNextNpc00 = true;
				info_.goNextBoard = true;
				StartTimer(0.5f);
				act_ = Phase03;
			}
			return true;
		}

		private bool Phase03()
		{
			if (IsWaitComplete())
			{
				rymFX component = fireEffect2_.gameObject.GetComponent<rymFX>();
				if ((UnityEngine.Object)null != (UnityEngine.Object)component)
				{
					component.AutoDelete = true;
					component.LoopEnd = true;
				}
				ToExit();
			}
			return true;
		}
	}

	private class FSMBoard : FSM
	{
		private Material paperMat_;

		private Material panelMat_;

		private Material panel2Mat_;

		private Transform light_;

		private float offset_;

		private float speed_;

		private Vector3 itemEndPos_ = new Vector3(-0.004f, 1.431f, 7.3f);

		private float itemEndScale_ = 0.4128781f;

		private Vector3 moveDir_ = Vector3.up;

		private Vector3 movePos_ = Vector3.zero;

		private Vector3 lightPosOffset_ = new Vector3(0f, 0f, 0.2f);

		private float moveScale_ = 1f;

		private Transform itemModel_;

		private int animFrame_ = 17;

		private float scaleVelocity_;

		private float itemMoveTime = 0.84f;

		private Vector3 itemInitPos = default(Vector3);

		private float itemStartScale;

		private float phase3Time;

		private float endRotation = 720f;

		private float rotation;

		public override void Init(FSMInfo info)
		{
			base.Init(info);
			act_ = Phase00;
			paperMat_ = info.todayPaper;
			panelMat_ = info.todayPanel;
			panel2Mat_ = info.todayPanel2;
			light_ = info.light;
			itemModel_ = info.itemModel;
		}

		private bool Phase00()
		{
			if (info_.goNextBoard)
			{
				info_.goNextBoard = false;
				act_ = Phase01;
				offset_ = 0f;
				speed_ = 0.2f;
				itemModel_.gameObject.SetActive(true);
				itemModel_.position = info_.fireballEndPos;
				PlayAudio(AUDIO.SE_TOP);
				movePos_ = info_.fireballEndPos;
				light_.position = movePos_ + lightPosOffset_;
				itemInitPos = itemModel_.position;
				Vector3 localScale = itemModel_.localScale;
				itemStartScale = localScale.x;
			}
			return true;
		}

		private bool Phase01()
		{
			offset_ += speed_;
			paperMat_.SetFloat("_Offset", offset_);
			panelMat_.SetFloat("_Offset", offset_);
			panel2Mat_.SetFloat("_Offset", offset_);
			if (offset_ > 0.5f)
			{
				Phase03();
			}
			if (1f < offset_)
			{
				act_ = Phase03;
			}
			return true;
		}

		private bool Phase02()
		{
			if (IsWaitComplete())
			{
				act_ = Phase03;
				float num = itemEndScale_;
				Vector3 localScale = itemModel_.localScale;
				scaleVelocity_ = (num - localScale.x) / (float)animFrame_;
				movePos_ = info_.fireballEndPos;
				Vector3 localScale2 = itemModel_.localScale;
				moveScale_ = localScale2.x;
				itemModel_.position = movePos_;
				light_.position = movePos_ + lightPosOffset_;
				moveDir_ = itemEndPos_ - movePos_;
				moveDir_.Normalize();
				light_.gameObject.SetActive(true);
				itemInitPos = itemModel_.position;
				Vector3 localScale3 = itemModel_.localScale;
				itemStartScale = localScale3.x;
			}
			return true;
		}

		private bool Phase03()
		{
			phase3Time += Time.deltaTime;
			float num = phase3Time / itemMoveTime;
			float t = info_.moveCurve.Evaluate(num);
			float num2 = info_.scaleCurve.Evaluate(num);
			movePos_ = Vector3.Lerp(itemInitPos, itemEndPos_, t);
			moveScale_ = (itemEndScale_ - itemStartScale) * num2 + itemStartScale;
			rotation = endRotation * num;
			if (phase3Time > itemMoveTime)
			{
				light_.gameObject.SetActive(true);
				act_ = Phase05;
				movePos_ = itemEndPos_;
				moveScale_ = itemEndScale_;
				rotation = endRotation;
			}
			itemModel_.position = movePos_;
			itemModel_.localScale = new Vector3(moveScale_, moveScale_, moveScale_);
			itemModel_.localRotation = Quaternion.AngleAxis(rotation, Vector3.up);
			light_.position = movePos_ + lightPosOffset_;
			return true;
		}

		private bool Phase04()
		{
			moveScale_ += scaleVelocity_;
			if (moveScale_ >= itemEndScale_)
			{
				moveScale_ = itemEndScale_;
				act_ = Phase05;
			}
			itemModel_.localScale = new Vector3(moveScale_, moveScale_, moveScale_);
			return true;
		}

		private bool Phase05()
		{
			if (info_.goNextBoard)
			{
				itemModel_.gameObject.SetActive(false);
				light_.gameObject.SetActive(false);
				ToExit();
			}
			return true;
		}
	}

	private class FSMCamera : FSM
	{
		public override void Init(FSMInfo info)
		{
			base.Init(info);
			act_ = Phase00;
		}

		private bool Phase00()
		{
			if (info_.goNextCamera)
			{
				info_.goNextCamera = false;
				act_ = Phase01;
				OutGameSettingsManager.LoginBonusScene loginBonusScene = MonoBehaviourSingleton<OutGameSettingsManager>.I.loginBonusScene;
				Vector3 cameraPos = loginBonusScene.cameraPos;
				Vector3 cameraRot = loginBonusScene.cameraRot;
				info_.interpolator.Translate(0.7f, cameraPos, null, default(Vector3), null);
				info_.interpolator.Rotate(0.7f, cameraRot, null, default(Vector3), null, true);
			}
			return true;
		}

		private bool Phase01()
		{
			if (!info_.interpolator.IsPlaying())
			{
				info_.goNextNpc06 = true;
				ToExit();
			}
			return true;
		}
	}

	public static readonly VOICE[] voiceGreetings = new VOICE[4]
	{
		VOICE.PAMERA_GREET_0,
		VOICE.PAMERA_GREET_1,
		VOICE.PAMERA_GREET_2,
		VOICE.PAMERA_GREET_3
	};

	public static readonly VOICE[] voiceCheers = new VOICE[3]
	{
		VOICE.PAMERA_CHEER_0,
		VOICE.PAMERA_CHEER_1,
		VOICE.PAMERA_CHEER_2
	};

	private List<FSM> fsmList_ = new List<FSM>();

	private FSMInfo fsmInfo_ = new FSMInfo();

	private Action mainAction_;

	private float waitTime_;

	private Camera homeCamera_;

	private bool isMoveEndCamera_;

	private Transform board_;

	private Transform light_;

	private Transform fireball_;

	private Transform itemModel_;

	private Transform itemLoader_;

	private Transform fireEffect1_;

	private Transform fireEffect2_;

	private TransformInterpolator interpolator_;

	private float homeFieldOfView_;

	private float fovSpeed_ = 1.5f;

	private Vector3 previousCameraPosition = Vector3.zero;

	private Quaternion previousCameraRotation = Quaternion.identity;

	private Vector3 previousNPC00Position = Vector3.zero;

	private Quaternion previousNPC00Rotation = Quaternion.identity;

	public override IEnumerable<string> requireDataTable
	{
		get
		{
			yield return "ItemTable";
		}
	}

	public void PreInitialize()
	{
		HomeCamera homeCamera = null;
		HomeNPCCharacter homeNPCCharacter = null;
		HomeNPCCharacter homeNPCCharacter2 = null;
		HomeSelfCharacter homeSelfCharacter = null;
		List<HomeCharacterBase> list = new List<HomeCharacterBase>();
		if (MonoBehaviourSingleton<HomeManager>.IsValid())
		{
			homeCamera = MonoBehaviourSingleton<HomeManager>.I.HomeCamera;
			homeNPCCharacter = MonoBehaviourSingleton<HomeManager>.I.HomePeople.GetHomeNPCCharacter(0);
			homeNPCCharacter2 = MonoBehaviourSingleton<HomeManager>.I.HomePeople.GetHomeNPCCharacter(6);
			homeSelfCharacter = MonoBehaviourSingleton<HomeManager>.I.HomePeople.selfChara;
			list = MonoBehaviourSingleton<HomeManager>.I.HomePeople.charas;
		}
		else if (MonoBehaviourSingleton<LoungeManager>.IsValid())
		{
			homeCamera = MonoBehaviourSingleton<LoungeManager>.I.HomeCamera;
			homeNPCCharacter = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.GetHomeNPCCharacter(0);
			homeNPCCharacter2 = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.GetHomeNPCCharacter(6);
			homeSelfCharacter = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.selfChara;
			list = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.charas;
		}
		homeCamera.LateUpdate();
		homeCamera.targetCamera.fieldOfView = 19f;
		HomeSelfCharacter.CTRL = false;
		OutGameSettingsManager.LoginBonusScene loginBonusScene = MonoBehaviourSingleton<OutGameSettingsManager>.I.loginBonusScene;
		Vector3 npc00CameraPos = loginBonusScene.npc00CameraPos;
		Quaternion localRotation = Quaternion.Euler(loginBonusScene.npc00CameraRot);
		previousCameraPosition = homeCamera.targetCamera.transform.position;
		previousCameraRotation = homeCamera.targetCamera.transform.rotation;
		homeCamera.targetCamera.transform.localPosition = npc00CameraPos;
		homeCamera.targetCamera.transform.localRotation = localRotation;
		if ((UnityEngine.Object)null != (UnityEngine.Object)homeNPCCharacter)
		{
			Transform transform = homeNPCCharacter.transform;
			previousNPC00Position = transform.position;
			previousNPC00Rotation = transform.rotation;
			Vector3 npc00Pos = loginBonusScene.npc00Pos;
			Quaternion rotation = Quaternion.Euler(loginBonusScene.npc00Rot);
			transform.position = npc00Pos;
			transform.rotation = rotation;
			homeNPCCharacter.PushOutControll();
		}
		if ((UnityEngine.Object)null != (UnityEngine.Object)homeNPCCharacter2)
		{
			Transform transform2 = homeNPCCharacter2.transform;
			Vector3 npc06Pos = loginBonusScene.npc06Pos;
			Quaternion rotation2 = Quaternion.Euler(loginBonusScene.npc06Rot);
			transform2.position = npc06Pos;
			transform2.rotation = rotation2;
			homeNPCCharacter2.PushOutControll();
			PlayerAnimCtrl componentInChildren = homeNPCCharacter2.gameObject.GetComponentInChildren<PlayerAnimCtrl>();
			componentInChildren.Play(PLCA.LOGIN_IDLE, false);
			homeNPCCharacter2.HideShadow();
		}
		if ((UnityEngine.Object)null != (UnityEngine.Object)homeSelfCharacter)
		{
			homeSelfCharacter.gameObject.SetActive(false);
		}
		list.ForEach(delegate(HomeCharacterBase o)
		{
			if (o is HomePlayerCharacter || o is LoungePlayer)
			{
				o.gameObject.SetActive(false);
				Transform namePlate = o.GetNamePlate();
				if ((UnityEngine.Object)null != (UnityEngine.Object)namePlate)
				{
					o.GetNamePlate().gameObject.SetActive(false);
				}
			}
		});
	}

	public override void Initialize()
	{
		PreInitialize();
		StartCoroutine("DoInitialize");
	}

	private IEnumerator DoInitialize()
	{
		HomeCamera homeCamera = null;
		HomeNPCCharacter npc0 = null;
		HomeNPCCharacter npc = null;
		if (MonoBehaviourSingleton<HomeManager>.IsValid())
		{
			homeCamera = MonoBehaviourSingleton<HomeManager>.I.HomeCamera;
			npc0 = MonoBehaviourSingleton<HomeManager>.I.HomePeople.GetHomeNPCCharacter(0);
			npc = MonoBehaviourSingleton<HomeManager>.I.HomePeople.GetHomeNPCCharacter(6);
		}
		else if (MonoBehaviourSingleton<LoungeManager>.IsValid())
		{
			homeCamera = MonoBehaviourSingleton<LoungeManager>.I.HomeCamera;
			npc0 = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.GetHomeNPCCharacter(0);
			npc = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.GetHomeNPCCharacter(6);
		}
		homeCamera.targetCamera.fieldOfView = 19f;
		MonoBehaviourSingleton<AccountManager>.I.DisplayLogInBonusSection();
		LoadingQueue loadQueue = new LoadingQueue(this);
		LoadObject boardLO = loadQueue.Load(RESOURCE_CATEGORY.ITEM_MODEL, "LIB_00000001", false);
		LoadObject lightLO = loadQueue.Load(RESOURCE_CATEGORY.ITEM_MODEL, "LIB_00000002", false);
		LoadObject fireballLO = loadQueue.Load(RESOURCE_CATEGORY.EFFECT_ACTION, "ef_btl_dragon_breath_01", false);
		LoadObject fireEffect1LO = loadQueue.Load(RESOURCE_CATEGORY.EFFECT_ACTION, "ef_btl_damage_slash_fire_01", false);
		LoadObject fireEffect2LO = loadQueue.Load(RESOURCE_CATEGORY.EFFECT_ACTION, "ef_btl_damage_fire_01", false);
		int[] voiceList = (int[])Enum.GetValues(typeof(VOICE));
		int[] array = voiceList;
		foreach (int v in array)
		{
			loadQueue.CacheVoice(v, null);
		}
		int[] audioList = (int[])Enum.GetValues(typeof(AUDIO));
		int[] array2 = audioList;
		foreach (int a in array2)
		{
			loadQueue.CacheSE(a, null);
		}
		LoginBonus bonus = MonoBehaviourSingleton<AccountManager>.I.logInBonus.Find((LoginBonus obj) => obj.type == 0);
		if (bonus != null)
		{
			List<LoginBonus.NextReward> nextRewards = bonus.next;
			if (nextRewards != null)
			{
				LoadObject[] itemIconLOs = new LoadObject[9];
				LoadObject[] itemBGIconLOs = new LoadObject[9];
				string iconName = string.Empty;
				string iconBGName = string.Empty;
				GetIconName(bonus.reward[0], out iconName, out iconBGName);
				itemIconLOs[bonus.nowCount - 1] = loadQueue.LoadItemIcon(iconName);
				if (string.Empty != iconBGName)
				{
					itemBGIconLOs[bonus.nowCount - 1] = loadQueue.LoadItemIcon(iconBGName);
				}
				nextRewards.ForEach(delegate(LoginBonus.NextReward o)
				{
					if (0 < o.reward.Count && 0 < o.count && 9 >= o.count)
					{
						GetIconName(o.reward[0], out ((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CiconName_003E__21, out ((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CiconBGName_003E__22);
						((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CitemIconLOs_003E__19[o.count - 1] = ((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CloadQueue_003E__3.LoadItemIcon(((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CiconName_003E__21);
						if (string.Empty != ((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CiconBGName_003E__22)
						{
							((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CitemBGIconLOs_003E__20[o.count - 1] = ((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CloadQueue_003E__3.LoadItemIcon(((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0382: stateMachine*/)._003CiconBGName_003E__22);
						}
					}
				});
				itemLoader_ = Utility.CreateGameObject("ItemLoader", MonoBehaviourSingleton<AppMain>.I._transform, -1);
				ItemLoader loader = itemLoader_.gameObject.AddComponent<ItemLoader>();
				uint itemID = GetItemModelID((REWARD_TYPE)bonus.reward[0].type, bonus.reward[0].itemId);
				loader.LoadItem(itemID, itemModel_, 0, null);
				while (loader.isLoading)
				{
					yield return (object)null;
				}
				itemModel_ = Utility.CreateGameObject("ItemModel", MonoBehaviourSingleton<AppMain>.I._transform, -1);
				loader.nodeMain.SetParent(itemModel_);
				itemModel_.gameObject.SetActive(false);
				float itemModelScale = 0.16f;
				itemModel_.localScale = new Vector3(itemModelScale, itemModelScale, itemModelScale);
				homeCamera_ = homeCamera.targetCamera;
				interpolator_ = homeCamera_.gameObject.GetComponent<TransformInterpolator>();
				if ((UnityEngine.Object)null == (UnityEngine.Object)interpolator_)
				{
					interpolator_ = homeCamera_.gameObject.AddComponent<TransformInterpolator>();
				}
				homeCamera_.fieldOfView = MonoBehaviourSingleton<OutGameSettingsManager>.I.loginBonusScene.cameraFov;
				homeFieldOfView_ = MonoBehaviourSingleton<GlobalSettingsManager>.I.cameraParam.outGameFieldOfView;
				if (loadQueue.IsLoading())
				{
					yield return (object)loadQueue.Wait();
				}
				board_ = ResourceUtility.Realizes(parent: Utility.Find(npc._transform, "Move"), obj: boardLO.loadedObject, layer: -1);
				light_ = ResourceUtility.Realizes(lightLO.loadedObject, npc._transform, -1);
				light_.gameObject.SetActive(false);
				fireball_ = ResourceUtility.Realizes(fireballLO.loadedObject, npc._transform, -1);
				fireball_.localScale = new Vector3(0.3f, 0.3f, 0.3f);
				fireball_.gameObject.SetActive(false);
				fireEffect1_ = ResourceUtility.Realizes(fireEffect1LO.loadedObject, MonoBehaviourSingleton<AppMain>.I._transform, -1);
				fireEffect1_.localScale = new Vector3(0.18f, 0.18f, 0.18f);
				fireEffect1_.gameObject.SetActive(false);
				fireEffect2_ = ResourceUtility.Realizes(fireEffect2LO.loadedObject, MonoBehaviourSingleton<AppMain>.I._transform, -1);
				fireEffect2_.localScale = new Vector3(0.25f, 0.25f, 0.25f);
				fireEffect2_.gameObject.SetActive(false);
				Material[] panelMaterials = new Material[9];
				Transform[] panelTransforms = new Transform[9];
				Material[] panel2Materials = new Material[9];
				Transform[] panel2Transforms = new Transform[9];
				Renderer[] papers = new Renderer[9];
				Transform Day_set = board_.Find("Day_set");
				if ((UnityEngine.Object)null != (UnityEngine.Object)Day_set)
				{
					for (int k = 0; k <= 8; k++)
					{
						string nameHead = "Day" + (k + 1).ToString();
						Transform panel3 = Day_set.Find(nameHead + "/" + nameHead + "_panel");
						if (!((UnityEngine.Object)null == (UnityEngine.Object)panel3))
						{
							Renderer renderer3 = panel3.GetComponent<Renderer>();
							if ((UnityEngine.Object)null != (UnityEngine.Object)renderer3)
							{
								panelMaterials[k] = renderer3.material;
							}
							panelTransforms[k] = panel3;
							Transform panel2 = Day_set.Find(nameHead + "/" + nameHead + "_panel2");
							if (!((UnityEngine.Object)null == (UnityEngine.Object)panel3))
							{
								Renderer renderer2 = panel2.GetComponent<Renderer>();
								if ((UnityEngine.Object)null != (UnityEngine.Object)renderer3)
								{
									panel2Materials[k] = renderer2.material;
								}
								panel2Transforms[k] = panel2;
								Transform paper = Day_set.Find(nameHead + "/" + nameHead + "_paper");
								if (!((UnityEngine.Object)null == (UnityEngine.Object)paper))
								{
									papers[k] = paper.gameObject.GetComponent<Renderer>();
								}
							}
						}
					}
				}
				Texture[] itemIcons = new Texture[9];
				for (int j = 0; j < itemIconLOs.Length; j++)
				{
					if (itemIconLOs[j] != null)
					{
						itemIcons[j] = (itemIconLOs[j].loadedObject as Texture);
					}
				}
				Texture[] itemBGIcons = new Texture[9];
				for (int i = 0; i < itemBGIconLOs.Length; i++)
				{
					if (itemBGIconLOs[i] != null)
					{
						itemBGIcons[i] = (itemBGIconLOs[i].loadedObject as Texture);
					}
				}
				int panelIndex;
				for (panelIndex = 0; panelIndex < bonus.nowCount - 1; panelIndex++)
				{
					panelTransforms[panelIndex].gameObject.SetActive(false);
					panel2Transforms[panelIndex].gameObject.SetActive(false);
					papers[panelIndex].material.SetFloat("_Offset", 1f);
				}
				for (; panelIndex < 9; panelIndex++)
				{
					if ((UnityEngine.Object)null != (UnityEngine.Object)itemBGIcons[panelIndex])
					{
						panelMaterials[panelIndex].mainTexture = itemBGIcons[panelIndex];
						panel2Materials[panelIndex].mainTexture = itemIcons[panelIndex];
					}
					else
					{
						panelMaterials[panelIndex].mainTexture = itemIcons[panelIndex];
					}
				}
				fsmInfo_.npc00 = npc0;
				fsmInfo_.npc06 = npc;
				fsmInfo_.light = light_;
				fsmInfo_.fireball = fireball_;
				fsmInfo_.fireEffect1 = fireEffect1_;
				fsmInfo_.fireEffect2 = fireEffect2_;
				fsmInfo_.itemModel = itemModel_;
				fsmInfo_.fireballEndPos = panelTransforms[bonus.nowCount - 1].position;
				fsmInfo_.fireballEndPos.z -= 0.08f;
				fsmInfo_.dayIndex = bonus.nowCount - 1;
				fsmInfo_.todayPanel = panelMaterials[bonus.nowCount - 1];
				fsmInfo_.todayPanel2 = panel2Materials[bonus.nowCount - 1];
				fsmInfo_.todayPaper = papers[bonus.nowCount - 1].material;
				fsmInfo_.interpolator = interpolator_;
				fsmInfo_.moveCurve = MonoBehaviourSingleton<OutGameSettingsManager>.I.homeScene.loginBonusMoveCureve;
				fsmInfo_.scaleCurve = MonoBehaviourSingleton<OutGameSettingsManager>.I.homeScene.loginBonusScaleCureve;
				fsmInfo_.previousNPC00Position = previousNPC00Position;
				fsmInfo_.previousNPC00Rotation = previousNPC00Rotation;
				fsmList_.Add(new FSMNpc00());
				fsmList_.Add(new FSMNpc00Facial());
				fsmList_.Add(new FSMNpc06());
				fsmList_.Add(new FSMBoard());
				fsmList_.Add(new FSMFireball());
				fsmList_.Add(new FSMCamera());
				fsmList_.ForEach(delegate(FSM o)
				{
					o.Init(((_003CDoInitialize_003Ec__Iterator91)/*Error near IL_0e0b: stateMachine*/)._003C_003Ef__this.fsmInfo_);
				});
				mainAction_ = Phase00;
				base.Initialize();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((UnityEngine.Object)null != (UnityEngine.Object)light_)
		{
			UnityEngine.Object.Destroy(light_.gameObject);
			light_ = null;
		}
		if ((UnityEngine.Object)null != (UnityEngine.Object)fireball_)
		{
			UnityEngine.Object.Destroy(fireball_.gameObject);
			fireball_ = null;
		}
		if ((UnityEngine.Object)null != (UnityEngine.Object)itemModel_)
		{
			UnityEngine.Object.Destroy(itemModel_.gameObject);
			itemModel_ = null;
		}
		if ((UnityEngine.Object)null != (UnityEngine.Object)itemLoader_)
		{
			UnityEngine.Object.Destroy(itemLoader_.gameObject);
			itemLoader_ = null;
		}
	}

	private void Update()
	{
		fsmInfo_.deltaTime = Time.deltaTime;
		waitTime_ -= fsmInfo_.deltaTime;
		List<FSM> removeList = new List<FSM>();
		fsmList_.ForEach(delegate(FSM o)
		{
			if (!o.DoAction())
			{
				removeList.Add(o);
			}
		});
		removeList.ForEach(delegate(FSM o)
		{
			fsmList_.Remove(o);
		});
		if (mainAction_ != null)
		{
			mainAction_();
		}
	}

	private void Phase00()
	{
		if (fsmInfo_.goNextMain)
		{
			fsmInfo_.goNextMain = false;
			DispatchEvent("NOTICE", null);
			mainAction_ = Phase01;
			StartTimer(0.5f);
		}
	}

	private void Phase01()
	{
		if (isWaitComplete())
		{
			fsmInfo_.goNextBoard = true;
			mainAction_ = Phase02;
		}
	}

	private void Phase02()
	{
		if (fsmInfo_.goNextMain)
		{
			fsmInfo_.goNextMain = false;
			if ((UnityEngine.Object)null != (UnityEngine.Object)interpolator_)
			{
				interpolator_.Translate(1.3f, previousCameraPosition, null, default(Vector3), null);
				interpolator_.Rotate(1.3f, previousCameraRotation.eulerAngles, null, default(Vector3), null, true);
			}
			if ((UnityEngine.Object)null != (UnityEngine.Object)light_)
			{
				light_.gameObject.SetActive(false);
			}
			mainAction_ = Phase03;
			StartTimer(0.3f);
			fsmInfo_.goNextNpc00 = true;
			fsmInfo_.goNextNpc06 = true;
		}
	}

	private void Phase03()
	{
		float fieldOfView = homeCamera_.fieldOfView;
		fieldOfView += fovSpeed_;
		if (fieldOfView >= homeFieldOfView_)
		{
			fieldOfView = homeFieldOfView_;
		}
		homeCamera_.fieldOfView = fieldOfView;
		if (isWaitComplete())
		{
			HomeSelfCharacter homeSelfCharacter = null;
			if (MonoBehaviourSingleton<HomeManager>.IsValid())
			{
				homeSelfCharacter = MonoBehaviourSingleton<HomeManager>.I.HomePeople.selfChara;
			}
			else if (MonoBehaviourSingleton<LoungeManager>.IsValid())
			{
				homeSelfCharacter = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.selfChara;
			}
			if ((UnityEngine.Object)null != (UnityEngine.Object)homeSelfCharacter)
			{
				homeSelfCharacter.gameObject.SetActive(true);
			}
			mainAction_ = Phase04;
		}
	}

	private void Phase04()
	{
		float fieldOfView = homeCamera_.fieldOfView;
		fieldOfView += fovSpeed_;
		if (fieldOfView >= homeFieldOfView_)
		{
			fieldOfView = homeFieldOfView_;
		}
		homeCamera_.fieldOfView = fieldOfView;
		if ((UnityEngine.Object)null != (UnityEngine.Object)homeCamera_ && !isMoveEndCamera_ && !interpolator_.IsPlaying())
		{
			isMoveEndCamera_ = true;
		}
		if (isMoveEndCamera_ && CanChangeScene())
		{
			GameSection.BackSection();
			HomeSelfCharacter.CTRL = true;
			List<HomeCharacterBase> list = new List<HomeCharacterBase>();
			if (MonoBehaviourSingleton<HomeManager>.IsValid())
			{
				list = MonoBehaviourSingleton<HomeManager>.I.HomePeople.charas;
			}
			else if (MonoBehaviourSingleton<LoungeManager>.IsValid())
			{
				list = MonoBehaviourSingleton<LoungeManager>.I.HomePeople.charas;
			}
			list.ForEach(delegate(HomeCharacterBase o)
			{
				if (o is HomePlayerCharacter || o is LoungePlayer)
				{
					o.gameObject.SetActive(true);
					Transform namePlate = o.GetNamePlate();
					if ((UnityEngine.Object)null != (UnityEngine.Object)namePlate)
					{
						o.GetNamePlate().gameObject.SetActive(true);
					}
				}
			});
			mainAction_ = null;
		}
	}

	private void StartTimer(float t)
	{
		waitTime_ = t;
	}

	private bool isWaitComplete()
	{
		return 0f > waitTime_;
	}

	private void OnCloseDialog(string close_section_name)
	{
		if (MonoBehaviourSingleton<AccountManager>.I.logInBonus.Count <= 0)
		{
			fsmInfo_.goNextMain = true;
		}
	}

	private bool CanChangeScene()
	{
		return MonoBehaviourSingleton<GameSceneManager>.I.IsEventExecutionPossible() && !MonoBehaviourSingleton<GameSceneManager>.I.isChangeing;
	}

	private static int GetIconBGID(ITEM_ICON_TYPE icon_type, int icon_id)
	{
		int result = -1;
		switch (icon_type)
		{
		case ITEM_ICON_TYPE.SKILL_ATTACK:
		case ITEM_ICON_TYPE.SKILL_SUPPORT:
		case ITEM_ICON_TYPE.SKILL_HEAL:
		case ITEM_ICON_TYPE.SKILL_PASSIVE:
		case ITEM_ICON_TYPE.SKILL_GROW:
			result = ItemIcon.GetIconBGID(icon_type, icon_id, null);
			break;
		}
		return result;
	}

	private static uint GetItemModelID(REWARD_TYPE type, int itemID)
	{
		uint result = uint.MaxValue;
		switch (type)
		{
		case REWARD_TYPE.CRYSTAL:
			result = 1u;
			break;
		case REWARD_TYPE.MONEY:
			result = 2u;
			break;
		case REWARD_TYPE.ITEM:
			result = (uint)itemID;
			break;
		}
		return result;
	}

	private static void GetIconName(LoginBonus.LoginBonusReward reward, out string iconName, out string iconBGName)
	{
		ITEM_ICON_TYPE icon_type = ITEM_ICON_TYPE.NONE;
		RARITY_TYPE? rarity = null;
		ELEMENT_TYPE element = ELEMENT_TYPE.MAX;
		ELEMENT_TYPE element2 = ELEMENT_TYPE.MAX;
		ItemIcon.GetIconShowData((REWARD_TYPE)reward.type, (uint)reward.itemId, out int icon_id, out icon_type, out rarity, out element, out element2, out EQUIPMENT_TYPE? _, out int _, out int _, out GET_TYPE _, 0);
		if (icon_type == ITEM_ICON_TYPE.ACCESSORY)
		{
			iconName = ResourceName.GetAccessoryIcon(icon_id);
		}
		else
		{
			iconName = ResourceName.GetItemIcon(icon_id);
		}
		int iconBGID = GetIconBGID(icon_type, icon_id);
		iconBGName = ResourceName.GetItemIcon(iconBGID);
	}

	public static void PlayAudio(AUDIO audio)
	{
		SoundManager.PlayOneShotSE((int)audio, null, null);
	}

	public static void PlayRandomVoice(VOICE[] voiceList)
	{
		int num = voiceList.Length;
		if (num >= 1)
		{
			int num2 = Utility.Random(num);
			int voice_id = (int)voiceList[num2];
			SoundManager.PlayVoice(voice_id, 1f, 0u, null, null);
		}
	}
}
