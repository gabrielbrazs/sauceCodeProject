using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillGachaDirector : AnimationDirector
{
	public interface ISectionCommand
	{
		void OnShowRarity(RARITY_TYPE rarity);

		void OnHideRarity();

		void OnShowSkillModel(uint skill_item_id);

		void OnHideSkillModel();

		void OnEnd();
	}

	public enum AUDIO
	{
		RARITY_01 = 40000126,
		RARITY_02 = 40000127,
		DROP_HEAVY = 40000101,
		BALL_ROLL_REAM = 40000104,
		FLASH_RARITY_01 = 40000136,
		FLASH_RARITY_02 = 40000137,
		OPENING_01 = 40000142,
		OPENING_02 = 40000143,
		ROLLING_01 = 40000144,
		ROLLING_02 = 40000145,
		BALL_DROP = 40000146,
		BALL_ROLLING = 40000147,
		BALL_SHINE = 40000148,
		BALL_POP = 40000149,
		BALL_BREAK = 40000150,
		BALL_BREAK_S = 40000151
	}

	public enum FLASH_TYPE
	{
		INVALID = -1,
		GREEN,
		SILVER,
		GOLD
	}

	public class MagiBall : MonoBehaviour
	{
		public FLASH_TYPE FlashType
		{
			get;
			set;
		}
	}

	public const string STATE_SINGLE = "SKILL_GACHA_SINGLE";

	public const string STATE_REAM = "SKILL_GACHA_REAM";

	public const string STATE_REAM_DROP_1 = "SKILL_GACHA_REAM_DROP_1";

	public const string STATE_REAM_DROP_2 = "SKILL_GACHA_REAM_DROP_2";

	public const int DISPLAY_BALL_MAX = 20;

	public const float BALL_EXTERNAL_FORCE_RATE = 120f;

	public const int REAM_NUM = 11;

	public const int SINGLE_FLASH_EFFECT_MAX = 3;

	public const int REAM_FLASH_EFFECT_MAX = 4;

	public const int MODEL_TEXTURE_MAX = 3;

	public const int MODEL_TEXTURE_ID_TOP = 2;

	public Animator gachaAnimator;

	public Animator cameraAnimator;

	public AnimationClip[] camereAnimClips;

	public Transform ballSocket;

	public Transform basket;

	public Renderer line;

	public GameObject[] balls;

	public GameObject[] ballsRen;

	public GameObject[] openEffects;

	public GameObject npcEffect;

	public GameObject jumpBallEffect;

	public Transform jumpBallEffectPosition;

	public GameObject dropBallEffect;

	public Transform dropBallEffectPosition;

	public GameObject[] uiRarityEffectPrefabs;

	public Transform flashEffectPosition;

	public GameObject flashEffectPrefab;

	public GameObject[] flashEffectRarityPrefabs;

	private ISectionCommand sectionCommandReceiver;

	private IEnumerator coroutine;

	private Transform npcModel;

	private Animator npcAnimator;

	private Transform mainBall;

	private Collider[] basketColliders;

	private float saveFixedUpdateTime;

	private List<GameObject> managedObjects = new List<GameObject>();

	private List<GameObject> ballObjects = new List<GameObject>();

	private List<Transform> managedEffects = new List<Transform>();

	private List<Transform> flashEffectList = new List<Transform>();

	private int rarityIndex;

	private RARITY_TYPE rarity;

	private FLASH_TYPE firstFlashType;

	private int dropCount;

	private int flashCount;

	private GachaResult.GachaReward reward;

	private bool isReam;

	private bool m_isAlreadySkipped;

	private bool m_isFinishLoad;

	private Texture2D[] basketModelTextureList = new Texture2D[3];

	private Texture backupBasketModelTexture;

	private Color backupBasketSpeColor = Color.white;

	private bool IsSingleGacha => !isReam;

	protected override void Awake()
	{
		base.Awake();
		commandReceiver = this;
		if (balls.Length > 0)
		{
			for (int i = 0; i < balls.Length; i++)
			{
				if ((UnityEngine.Object)balls[i] != (UnityEngine.Object)null)
				{
					balls[i].SetActive(false);
				}
			}
		}
		SetActiveRenBalls(false);
		if ((UnityEngine.Object)ballSocket != (UnityEngine.Object)null)
		{
			basketColliders = ballSocket.GetComponentsInChildren<Collider>(true);
			SetActivateBasketCollider(false);
		}
		if ((UnityEngine.Object)line != (UnityEngine.Object)null)
		{
			line.enabled = false;
		}
		Material material = GetMaterial(basket);
		if ((UnityEngine.Object)material != (UnityEngine.Object)null)
		{
			backupBasketModelTexture = material.mainTexture;
			backupBasketSpeColor = material.GetColor("_SpeLightColor");
		}
	}

	protected override void OnDestroy()
	{
		backupBasketModelTexture = null;
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		skip = false;
		m_isAlreadySkipped = false;
		Play("INIT", null, 0f);
	}

	public void StartDirection(ISectionCommand command_receiver)
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
			coroutine = null;
		}
		if (command_receiver != null)
		{
			sectionCommandReceiver = command_receiver;
			StartCoroutine(coroutine = DoSkillGacha());
		}
	}

	private IEnumerator DoSkillGacha()
	{
		Transform _transform = base.transform;
		Reset();
		if ((UnityEngine.Object)line != (UnityEngine.Object)null)
		{
			line.enabled = true;
		}
		isReam = false;
		if (MonoBehaviourSingleton<GachaManager>.IsValid() && MonoBehaviourSingleton<GachaManager>.I.IsReam())
		{
			isReam = true;
		}
		SetActivateBasketCollider(true);
		Vector3 basket_pos = basket.position;
		for (int l = 0; l < 20; l++)
		{
			CreateBall(_transform, 0, basket_pos + Quaternion.AngleAxis((float)(l * 45), Vector3.right) * new Vector3(0.04f * (float)(l - 10), 0.08f, 0f), false);
		}
		LoadingQueue load_queue = new LoadingQueue(this);
		if (isReam)
		{
			GachaResult currentResult = MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult();
			int k = 0;
			for (int j = currentResult.reward.Count; k < j; k++)
			{
				GachaResult.GachaReward rwd = currentResult.reward[k];
				SkillItemTable.SkillItemData skill_item_data = Singleton<SkillItemTable>.I.GetSkillItemData((uint)rwd.itemId);
				if (skill_item_data != null)
				{
					load_queue.Load(RESOURCE_CATEGORY.ITEM_MODEL, ResourceName.GetSkillItemModel(skill_item_data.modelID), false);
					load_queue.Load(RESOURCE_CATEGORY.ITEM_MODEL, ResourceName.GetSkillItemSymbolModel(skill_item_data.iconID), false);
				}
			}
			List<GachaResult.GachaReward> rewards = currentResult.reward;
			if (ballsRen != null)
			{
				for (int i = 0; i < ballsRen.Length; i++)
				{
					if (i < rewards.Count)
					{
						GachaResult.GachaReward rwd2 = rewards[i];
						SkillItemTable.SkillItemData skill_item_data2 = Singleton<SkillItemTable>.I.GetSkillItemData((uint)rwd2.itemId);
						if (skill_item_data2 != null)
						{
							MeshRenderer mr = ballsRen[i].GetComponent<MeshRenderer>();
							int index = skill_item_data2.rarity.ToRarityExpressionID();
							MeshRenderer rareBall = balls[index].GetComponent<MeshRenderer>();
							mr.sharedMaterial = rareBall.sharedMaterial;
						}
					}
				}
			}
		}
		int[] se_id_list = (int[])Enum.GetValues(typeof(AUDIO));
		int[] array = se_id_list;
		foreach (int id in array)
		{
			load_queue.CacheSE(id, null);
		}
		LoadingQueue loadQueue = new LoadingQueue(this);
		for (int tx = 0; tx < 3; tx++)
		{
			LoadObject loadObj = loadQueue.Load(RESOURCE_CATEGORY.MAGI_BASKET_MODEL_TEX, ResourceName.GetMagiGachaModelTexutre(2 + tx), false);
			while (loadQueue.IsLoading())
			{
				yield return (object)loadQueue.Wait();
			}
			basketModelTextureList[tx] = (Texture2D)loadObj.loadedObject;
		}
		if ((UnityEngine.Object)backupBasketModelTexture != (UnityEngine.Object)null)
		{
			Material mat = GetMaterial(basket);
			if ((UnityEngine.Object)mat != (UnityEngine.Object)null)
			{
				mat.mainTexture = backupBasketModelTexture;
				mat.SetColor("_SpeLightColor", backupBasketSpeColor);
			}
		}
		yield return (object)new WaitForSeconds(1f);
		npcModel = Utility.CreateGameObject("NPC", base.transform.parent, -1);
		managedObjects.Add(npcModel.gameObject);
		NPCLoader npc_loader = npcModel.gameObject.AddComponent<NPCLoader>();
		npc_loader.Load(Singleton<NPCTable>.I.GetNPCData(1).npcModelID, 0, false, true, SHADER_TYPE.NORMAL, null);
		while (npc_loader.isLoading)
		{
			yield return (object)null;
		}
		if (load_queue != null && load_queue.IsLoading())
		{
			yield return (object)load_queue.Wait();
		}
		npcAnimator = npc_loader.animator;
		yield return (object)null;
		m_isFinishLoad = true;
		npcAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		npcAnimator.Rebind();
		npcAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		gachaAnimator.Rebind();
		cameraAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		cameraAnimator.Rebind();
		string anim_name = (!IsSingleGacha) ? "SKILL_GACHA_REAM" : "SKILL_GACHA_SINGLE";
		dropCount = 0;
		Drop();
		Play(anim_name, null, 0f);
		if (isReam)
		{
			while (dropCount < 11)
			{
				if (skip && IsFinishDrop10())
				{
					if (MonoBehaviourSingleton<TransitionManager>.I.isChanging)
					{
						yield return (object)null;
					}
					Time.timeScale = 1f;
					skip = false;
					yield return (object)MonoBehaviourSingleton<TransitionManager>.I.In();
				}
				yield return (object)null;
			}
		}
		while (mainBall.gameObject.activeSelf)
		{
			yield return (object)null;
		}
		if (skip)
		{
			yield return (object)MonoBehaviourSingleton<TransitionManager>.I.In();
			Time.timeScale = 1f;
		}
		yield return (object)null;
		sectionCommandReceiver.OnEnd();
		coroutine = null;
	}

	private void Play3Anim(string state_name)
	{
	}

	private void Drop()
	{
		sectionCommandReceiver.OnHideRarity();
		int i = 0;
		for (int count = managedEffects.Count; i < count; i++)
		{
			if ((UnityEngine.Object)managedEffects[i] != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy(managedEffects[i].gameObject);
			}
		}
		managedEffects.Clear();
		rarity = RARITY_TYPE.D;
		reward = null;
		if (MonoBehaviourSingleton<GachaManager>.IsValid() && MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult() != null && dropCount < MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult().reward.Count)
		{
			reward = MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult().reward[dropCount];
			SkillItemTable.SkillItemData skillItemData = Singleton<SkillItemTable>.I.GetSkillItemData((uint)reward.itemId);
			if (skillItemData != null)
			{
				rarity = skillItemData.rarity;
			}
		}
		if (reward == null)
		{
			reward = new GachaResult.GachaReward();
		}
		rarityIndex = rarity.ToRarityExpressionID();
		if ((UnityEngine.Object)mainBall != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy(mainBall.gameObject);
		}
		mainBall = CreateBall(ballSocket, rarityIndex, Vector3.zero, true);
		string text;
		if (dropCount != 0)
		{
			text = ((dropCount >= 10) ? "SKILL_GACHA_REAM_DROP_2" : "SKILL_GACHA_REAM_DROP_1");
		}
		else
		{
			text = ((!IsSingleGacha) ? "SKILL_GACHA_REAM" : "SKILL_GACHA_SINGLE");
			Play(text, null, 0f);
		}
		npcAnimator.Play(text, 0, 0f);
		gachaAnimator.Play(text, 0, 0f);
		cameraAnimator.Play(text, 0, 0f);
	}

	private Transform CreateBall(Transform parent, int rarityType, Vector3 pos, bool is_main)
	{
		GameObject gameObject = ResourceUtility.Instantiate(balls[rarityType]);
		Transform transform = gameObject.transform;
		transform.parent = parent;
		transform.localPosition = pos;
		transform.localScale = Vector3.one;
		if (is_main)
		{
			UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
		}
		else
		{
			MagiBall magiBall = gameObject.AddComponent<MagiBall>();
			if ((UnityEngine.Object)magiBall != (UnityEngine.Object)null)
			{
				magiBall.FlashType = (FLASH_TYPE)rarityType;
			}
			ballObjects.Add(gameObject);
		}
		gameObject.SetActive(true);
		return transform;
	}

	protected override void LateUpdate()
	{
		if ((UnityEngine.Object)useCamera != (UnityEngine.Object)null)
		{
			Vector3 localScale = cameraAnimator.transform.localScale;
			float x = localScale.x;
			if (x > 0f)
			{
				useCamera.fieldOfView = Utility.HorizontalToVerticalFOV(x);
			}
		}
		base.LateUpdate();
	}

	private void Delete()
	{
		int i = 0;
		for (int count = managedObjects.Count; i < count; i++)
		{
			UnityEngine.Object.DestroyImmediate(managedObjects[i]);
			managedObjects[i] = null;
		}
		managedObjects.Clear();
		if (flashEffectList != null && flashEffectList.Count > 0)
		{
			int count2 = flashEffectList.Count;
			for (int j = 0; j < count2; j++)
			{
				UnityEngine.Object.DestroyImmediate(flashEffectList[j].gameObject);
				flashEffectList[j] = null;
			}
		}
		flashEffectList.Clear();
		mainBall = null;
		npcModel = null;
		npcAnimator = null;
		int count3 = ballObjects.Count;
		for (int k = 0; k < count3; k++)
		{
			UnityEngine.Object.DestroyImmediate(ballObjects[k]);
			ballObjects[k] = null;
		}
		ballObjects.Clear();
		if ((UnityEngine.Object)line != (UnityEngine.Object)null)
		{
			line.enabled = false;
		}
		Play("INIT", null, 0f);
	}

	public override void Reset()
	{
		skip = false;
		m_isAlreadySkipped = false;
		m_isFinishLoad = false;
		dropCount = 0;
		flashCount = 0;
		reward = null;
		SetActiveRenBalls(false);
		Delete();
	}

	public override void Skip()
	{
		if (!skip && !IsFinishDrop10() && !m_isAlreadySkipped)
		{
			m_isAlreadySkipped = true;
			AudioObjectPool.StopAllLentObjects();
			base.Skip();
			StartCoroutine(DoSkip());
		}
	}

	private IEnumerator DoSkip()
	{
		yield return (object)MonoBehaviourSingleton<TransitionManager>.I.Out(TransitionManager.TYPE.BLACK);
		Time.timeScale = 100f;
		while (!m_isFinishLoad)
		{
			yield return (object)null;
		}
		if (IsSingleGacha)
		{
			UpdateGachaModelEffectSingle(3);
		}
	}

	public bool IsFinishDrop10()
	{
		return isReam && dropCount >= 10;
	}

	private void OnDirectionCommand(string cmd)
	{
		bool flag = !MonoBehaviourSingleton<TransitionManager>.I.isChanging && MonoBehaviourSingleton<TransitionManager>.I.isTransing;
		if (cmd == "NPC_EFFECT")
		{
			if ((UnityEngine.Object)npcModel != (UnityEngine.Object)null)
			{
				Transform transform = Utility.Find(npcModel, "Head");
				if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
				{
					transform = ResourceUtility.Realizes(npcEffect, transform, -1);
				}
			}
		}
		else if (cmd == "DROP_EFFECT")
		{
			if (!flag && (UnityEngine.Object)dropBallEffect != (UnityEngine.Object)null && (UnityEngine.Object)dropBallEffectPosition != (UnityEngine.Object)null)
			{
				managedEffects.Add(ResourceUtility.Realizes(dropBallEffect, dropBallEffectPosition, -1));
			}
		}
		else if (cmd == "JUMP_EFFECT")
		{
			if (!flag)
			{
				PlayAUDIO(AUDIO.BALL_POP);
				if ((UnityEngine.Object)jumpBallEffect != (UnityEngine.Object)null && (UnityEngine.Object)jumpBallEffectPosition != (UnityEngine.Object)null)
				{
					managedEffects.Add(ResourceUtility.Realizes(jumpBallEffect, jumpBallEffectPosition, -1));
				}
			}
		}
		else if (cmd == "SHOW_RARITY")
		{
			if (!flag)
			{
				if (isReam)
				{
					sectionCommandReceiver.OnShowRarity(rarity);
				}
				PlayAUDIO(AUDIO.BALL_SHINE);
			}
		}
		else if (cmd == "OPEN_EFFECT")
		{
			if (!flag)
			{
				managedEffects.Add(ResourceUtility.Realizes(openEffects[rarityIndex], ballSocket, -1));
				if ((rarityIndex == 0 && IsSingleGacha) || rarityIndex == 10)
				{
					PlayAUDIO(AUDIO.BALL_BREAK);
				}
				else
				{
					PlayAUDIO(AUDIO.BALL_BREAK_S);
				}
			}
		}
		else if (cmd == "FLASH_EFFECT")
		{
			EventFlashEffect(flag);
		}
		else if (cmd == "HIDE_BALL")
		{
			if ((UnityEngine.Object)mainBall != (UnityEngine.Object)null)
			{
				mainBall.gameObject.SetActive(false);
			}
			if (isReam && sectionCommandReceiver != null && reward != null)
			{
				sectionCommandReceiver.OnShowSkillModel((uint)reward.itemId);
			}
		}
		else if (cmd == "NEXT_DROP")
		{
			if (isReam)
			{
				int num = 9;
				if (skip && flag && dropCount < num)
				{
					dropCount = num;
				}
				dropCount++;
				if (dropCount <= 10)
				{
					Drop();
				}
				if (sectionCommandReceiver != null)
				{
					sectionCommandReceiver.OnHideSkillModel();
				}
			}
		}
		else
		{
			if (cmd == "SHOW_REN_BALLS")
			{
				SetActiveRenBalls(true);
			}
			if (cmd == "HIDE_REN_BALLS")
			{
				SetActiveRenBalls(false);
			}
		}
	}

	private void EventFlashEffect(bool isSkip)
	{
		if (isSkip)
		{
			if (isReam)
			{
				flashCount++;
				UpdateGachaModelEffectReam(flashCount);
			}
		}
		else if (!((UnityEngine.Object)flashEffectPrefab == (UnityEngine.Object)null) && flashEffectRarityPrefabs != null)
		{
			ApplyRandomVectorForBalls();
			flashCount++;
			GameObject gameObject = flashEffectPrefab;
			if (isReam)
			{
				if (flashCount >= 4)
				{
					FLASH_TYPE flashTypeAtLast = GetFlashTypeAtLast();
					gameObject = flashEffectRarityPrefabs[(int)flashTypeAtLast];
					SwitchBasketModelTexture(flashTypeAtLast);
					SwitchBasketBallColor(flashTypeAtLast);
					PlayAUDIOFlash(flashTypeAtLast);
				}
				else
				{
					FLASH_TYPE fLASH_TYPE = UpdateGachaModelEffectReam(flashCount);
					gameObject = flashEffectRarityPrefabs[(int)fLASH_TYPE];
				}
			}
			else
			{
				FLASH_TYPE fLASH_TYPE2 = UpdateGachaModelEffectSingle(flashCount);
				gameObject = flashEffectRarityPrefabs[(int)fLASH_TYPE2];
			}
			if ((UnityEngine.Object)gameObject != (UnityEngine.Object)null && (UnityEngine.Object)flashEffectPosition != (UnityEngine.Object)null)
			{
				Transform transform = ResourceUtility.Realizes(gameObject, flashEffectPosition, -1);
				if ((UnityEngine.Object)transform != (UnityEngine.Object)null)
				{
					flashEffectList.Add(transform);
				}
			}
		}
	}

	private FLASH_TYPE UpdateGachaModelEffectSingle(int targetFlashCount)
	{
		FLASH_TYPE fLASH_TYPE = FLASH_TYPE.GREEN;
		if (targetFlashCount > 1)
		{
			if (CalcNumRarityData(RARITY_TYPE.SS, false, false) > 0 || CalcNumRarityData(RARITY_TYPE.S, false, false) > 0)
			{
				fLASH_TYPE = FLASH_TYPE.GOLD;
				if (targetFlashCount < 3)
				{
					fLASH_TYPE = ((UnityEngine.Random.Range(0, 100) <= 50) ? FLASH_TYPE.SILVER : FLASH_TYPE.GREEN);
				}
			}
			else if (CalcNumRarityData(RARITY_TYPE.A, false, false) > 0)
			{
				fLASH_TYPE = FLASH_TYPE.SILVER;
				if (targetFlashCount < 3)
				{
					fLASH_TYPE = ((UnityEngine.Random.Range(0, 100) <= 50) ? FLASH_TYPE.SILVER : FLASH_TYPE.GREEN);
				}
			}
		}
		PlayAUDIOFlash(fLASH_TYPE);
		SwitchBasketBallColor(fLASH_TYPE);
		SwitchBasketModelTexture(fLASH_TYPE);
		return fLASH_TYPE;
	}

	private void SwitchBasketBallColor(FLASH_TYPE targetFlashType)
	{
		foreach (GameObject ballObject in ballObjects)
		{
			MeshRenderer component = ballObject.GetComponent<MeshRenderer>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
			{
				Material material = component.material;
				if (!((UnityEngine.Object)material == (UnityEngine.Object)null))
				{
					MeshRenderer component2 = balls[(int)targetFlashType].GetComponent<MeshRenderer>();
					Material material2 = component2.material;
					material.mainTexture = material2.mainTexture;
					material.SetColor("_SpeLightColor", material2.GetColor("_SpeLightColor"));
					material.SetFloat("_SpeWidth", material2.GetFloat("_SpeWidth"));
				}
			}
		}
	}

	private void SwitchBasketBallColorReam(FLASH_TYPE targetFlashType, int numChange)
	{
		int num = 0;
		foreach (GameObject ballObject in ballObjects)
		{
			if (num >= numChange)
			{
				break;
			}
			MagiBall component = ballObject.GetComponent<MagiBall>();
			if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && component.FlashType == FLASH_TYPE.GREEN)
			{
				MeshRenderer component2 = ballObject.GetComponent<MeshRenderer>();
				if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null))
				{
					Material material = component2.material;
					if (!((UnityEngine.Object)material == (UnityEngine.Object)null))
					{
						MeshRenderer component3 = balls[(int)targetFlashType].GetComponent<MeshRenderer>();
						Material material2 = component3.material;
						material.mainTexture = material2.mainTexture;
						material.SetColor("_SpeLightColor", material2.GetColor("_SpeLightColor"));
						material.SetFloat("_SpeWidth", material2.GetFloat("_SpeWidth"));
						component.FlashType = targetFlashType;
						num++;
					}
				}
			}
		}
	}

	private FLASH_TYPE UpdateGachaModelEffectReam(int targetFlashCount)
	{
		FLASH_TYPE fLASH_TYPE = FLASH_TYPE.GREEN;
		switch (targetFlashCount)
		{
		case 2:
		{
			int numRarityData = GetNumRarityData10(RARITY_TYPE.A);
			if (numRarityData > 0)
			{
				fLASH_TYPE = FLASH_TYPE.SILVER;
				SwitchBasketBallColorReam(fLASH_TYPE, numRarityData * 2);
			}
			firstFlashType = fLASH_TYPE;
			break;
		}
		case 3:
		{
			fLASH_TYPE = firstFlashType;
			int num = GetNumRarityData10(RARITY_TYPE.SS) + GetNumRarityData10(RARITY_TYPE.S);
			if (num > 0)
			{
				fLASH_TYPE = FLASH_TYPE.GOLD;
				SwitchBasketBallColorReam(fLASH_TYPE, num * 2);
			}
			break;
		}
		}
		SwitchBasketModelTexture(fLASH_TYPE);
		PlayAUDIOFlash(fLASH_TYPE);
		return fLASH_TYPE;
	}

	private void SwitchBasketModelTexture(FLASH_TYPE targetRarityType)
	{
		if (basketModelTextureList == null)
		{
			Log.Error("Not found downloaded texture!!");
		}
		else
		{
			Texture2D texture2D = basketModelTextureList[(int)targetRarityType];
			if ((UnityEngine.Object)texture2D == (UnityEngine.Object)null)
			{
				Log.Error("Not found texture for basket model!!");
			}
			else
			{
				Material material = GetMaterial(basket);
				if ((UnityEngine.Object)material != (UnityEngine.Object)null)
				{
					material.mainTexture = texture2D;
					material.SetColor("_SpeLightColor", Color.white);
				}
			}
		}
	}

	private Material GetMaterial(Transform targetTrans)
	{
		MeshRenderer component = targetTrans.GetComponent<MeshRenderer>();
		if ((UnityEngine.Object)component == (UnityEngine.Object)null)
		{
			Log.Error("Not found MeshRender!!");
			return null;
		}
		Material[] materials = component.materials;
		if (materials == null)
		{
			Log.Error("material list is null!!");
			return null;
		}
		return materials[0];
	}

	private void ApplyRandomVectorForBalls()
	{
		foreach (GameObject ballObject in ballObjects)
		{
			if (!((UnityEngine.Object)ballObject == (UnityEngine.Object)null))
			{
				Rigidbody component = ballObject.GetComponent<Rigidbody>();
				if (!((UnityEngine.Object)component == (UnityEngine.Object)null))
				{
					component.AddForce(Vector3.up * 120f);
				}
			}
		}
	}

	private FLASH_TYPE GetFlashTypeAtLast()
	{
		FLASH_TYPE result = FLASH_TYPE.GREEN;
		if (CheckContainRarityLast(RARITY_TYPE.SS) || CheckContainRarityLast(RARITY_TYPE.S))
		{
			result = FLASH_TYPE.GOLD;
		}
		else if (CheckContainRarityLast(RARITY_TYPE.A))
		{
			result = FLASH_TYPE.SILVER;
		}
		return result;
	}

	private bool CheckContainRarityLast(RARITY_TYPE targetRarityType)
	{
		return CalcNumRarityData(targetRarityType, true, false) > 0;
	}

	private int GetNumRarityData10(RARITY_TYPE targetRarityType)
	{
		return CalcNumRarityData(targetRarityType, false, true);
	}

	private int CalcNumRarityData(RARITY_TYPE targetRarityType, bool isCheckOnlyLast = false, bool isCheck10 = false)
	{
		int num = 0;
		if (!MonoBehaviourSingleton<GachaManager>.IsValid())
		{
			Log.Error("Invalid GachaManager!!");
			return num;
		}
		GachaResult currentGachaResult = MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult();
		if (currentGachaResult == null)
		{
			Log.Error("gachaResult is null!!");
			return num;
		}
		int num2 = currentGachaResult.reward.Count;
		List<GachaResult.GachaReward> list = currentGachaResult.reward;
		if (isCheckOnlyLast)
		{
			SkillItemTable.SkillItemData skillItemData = Singleton<SkillItemTable>.I.GetSkillItemData((uint)list[num2 - 1].itemId);
			return (skillItemData.rarity == targetRarityType) ? 1 : 0;
		}
		if (isCheck10 && isReam)
		{
			num2--;
		}
		for (int i = 0; i < num2; i++)
		{
			SkillItemTable.SkillItemData skillItemData2 = Singleton<SkillItemTable>.I.GetSkillItemData((uint)list[i].itemId);
			if (skillItemData2 != null && skillItemData2.rarity == targetRarityType)
			{
				num++;
			}
		}
		return num;
	}

	private void PlayAUDIOFlash(FLASH_TYPE flash)
	{
		AUDIO audio = AUDIO.FLASH_RARITY_01;
		if (flash > FLASH_TYPE.GREEN)
		{
			audio = AUDIO.FLASH_RARITY_02;
		}
		PlayAUDIO(audio);
	}

	private void PlayAUDIO(AUDIO audio)
	{
		if (!skip)
		{
			SoundManager.PlayOneShotUISE((int)audio);
		}
	}

	public void PlayUIRarityEffect(RARITY_TYPE rarity, Transform effect_parent_ui, Transform effect_target_ui)
	{
		GameObject gameObject = null;
		int num = rarity.ToRarityExpressionID2();
		if (num > 0)
		{
			gameObject = uiRarityEffectPrefabs[num - 1];
		}
		if (!((UnityEngine.Object)gameObject == (UnityEngine.Object)null))
		{
			UIWidget componentInChildren = effect_target_ui.GetComponentInChildren<UIWidget>();
			Transform transform = ResourceUtility.Realizes(gameObject, effect_parent_ui, 5);
			transform.position = effect_parent_ui.position;
			EffectManager.SetUIEffectDepth(transform, effect_parent_ui, -0.001f, 10, componentInChildren);
			PlayRarityAudio(rarity);
		}
	}

	public void PlayRarityAudio(RARITY_TYPE rarity_type)
	{
		int num = 4;
		int num2 = -1;
		num2 = (((int)rarity_type < num) ? 40000126 : 40000127);
		SoundManager.PlayOneShotUISE(num2);
	}

	public override void __FUNCTION__PlayCachedAudio(int se_id)
	{
		if (!skip)
		{
			base.__FUNCTION__PlayCachedAudio(se_id);
		}
	}

	public void SetActiveRenBalls(bool isActive)
	{
		if (ballsRen != null && ballsRen.Length > 0)
		{
			int i = 0;
			if (MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult() != null)
			{
				int count = MonoBehaviourSingleton<GachaManager>.I.GetCurrentGachaResult().reward.Count;
				for (int num = Mathf.Min(ballsRen.Length, count); i < num; i++)
				{
					if ((UnityEngine.Object)ballsRen[i] != (UnityEngine.Object)null)
					{
						ballsRen[i].SetActive(isActive);
					}
				}
			}
			for (; i < ballsRen.Length; i++)
			{
				if ((UnityEngine.Object)ballsRen[i] != (UnityEngine.Object)null)
				{
					ballsRen[i].SetActive(false);
				}
			}
		}
	}

	public void SetActivateBasketCollider(bool isActivate)
	{
		if (basketColliders == null)
		{
			Log.Error("basketCollider is null!!");
		}
		else
		{
			int i = 0;
			for (int num = basketColliders.Length; i < num; i++)
			{
				basketColliders[i].enabled = isActivate;
			}
		}
	}
}
