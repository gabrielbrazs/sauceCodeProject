using System.Collections.Generic;
using UnityEngine;

public class BulletObject : MonoBehaviour, IAttackCollider, IBulletObservable, StageObjectManager.IDetachedNotify
{
	private const float IS_LAND_HIT_MARGIN = 1f;

	public AtkAttribute masterAtk = new AtkAttribute();

	public SkillInfo.SkillParam masterSkill;

	public bool isShotArrow;

	public bool isAimBossMode;

	public bool isBossPierceArrow;

	private StageObject m_targetObject;

	protected AttackColliderProcessor colliderProcessor;

	protected BulletControllerBase controller;

	protected bool isColliderCreate;

	public AtkAttribute m_exAtk;

	public Player.ATTACK_MODE m_attackMode;

	private int m_endBulletSkillIndex = -1;

	protected CapsuleCollider capsuleCollider;

	protected bool isDestroyed;

	protected bool isLandHitDelete;

	protected Vector3 landHitPosition = Vector3.zero;

	protected Quaternion landHitRotation = Quaternion.identity;

	public Transform bulletEffect;

	protected AttackHitChecker attackHitChecker;

	protected bool m_isDisablePlayEndAnim;

	private int observedID;

	private List<IBulletObserver> bulletObserverList = new List<IBulletObserver>();

	public Transform _transform
	{
		get;
		protected set;
	}

	public Rigidbody _rigidbody
	{
		get;
		protected set;
	}

	public Collider _collider
	{
		get;
		protected set;
	}

	public StageObject stageObject
	{
		get;
		protected set;
	}

	public float timeCount
	{
		get;
		protected set;
	}

	public Vector3 endVec
	{
		get;
		private set;
	}

	public Vector3 prevPosition
	{
		get;
		protected set;
	}

	public bool HasEndBulletSkillIndex => m_endBulletSkillIndex > -1;

	public BulletData bulletData
	{
		get;
		private set;
	}

	protected BulletData.BULLET_TYPE type
	{
		get;
		private set;
	}

	protected Vector3 dispOffset
	{
		get;
		private set;
	}

	protected Vector3 dispRotation
	{
		get;
		private set;
	}

	protected Vector3 offset
	{
		get;
		private set;
	}

	protected Vector3 baseScale
	{
		get;
		private set;
	}

	protected float appearTime
	{
		get;
		private set;
	}

	protected Vector3 timeStartScale
	{
		get;
		private set;
	}

	protected Vector3 timeEndScale
	{
		get;
		private set;
	}

	protected BulletData endBullet
	{
		get;
		private set;
	}

	protected bool isCharacterHitDelete
	{
		get;
		private set;
	}

	protected bool isObjectHitDelete
	{
		get;
		private set;
	}

	protected bool isLandHit
	{
		get;
		private set;
	}

	protected string landHitEfect
	{
		get;
		private set;
	}

	protected bool isBulletTakeoverTarget
	{
		get;
		private set;
	}

	public float radius
	{
		get;
		private set;
	}

	public float capsuleHeight
	{
		get;
		private set;
	}

	public Vector3 boxSize
	{
		get;
		private set;
	}

	public Vector3 startColliderPos
	{
		get;
		private set;
	}

	public BulletObject()
	{
		prevPosition = Vector3.zero;
		dispOffset = Vector3.zero;
		dispRotation = Vector3.zero;
		offset = Vector3.zero;
		baseScale = Vector3.one;
		appearTime = 0f;
		radius = 0f;
		timeStartScale = Vector3.one;
		timeEndScale = Vector3.one;
		isCharacterHitDelete = true;
		isObjectHitDelete = true;
		isLandHit = false;
		isBulletTakeoverTarget = false;
		endVec = Vector3.zero;
		capsuleHeight = 0f;
		boxSize = Vector3.zero;
	}

	public void SetEndBulletSkillIndex(int skillIndex)
	{
		m_endBulletSkillIndex = skillIndex;
	}

	public void SetDisablePlayEndAnim()
	{
		m_isDisablePlayEndAnim = true;
	}

	protected virtual void Awake()
	{
		_transform = base.transform;
		_rigidbody = GetComponent<Rigidbody>();
		_collider = GetComponent<Collider>();
		if ((Object)_rigidbody == (Object)null)
		{
			_rigidbody = base.gameObject.AddComponent<Rigidbody>();
		}
		if ((Object)_collider == (Object)null)
		{
			_collider = base.gameObject.GetComponentInChildren<Collider>();
		}
		if ((Object)_collider != (Object)null)
		{
			_collider.isTrigger = true;
			capsuleCollider = (_collider as CapsuleCollider);
		}
		base.gameObject.SetActive(false);
		_rigidbody.useGravity = false;
		if (MonoBehaviourSingleton<StageObjectManager>.IsValid())
		{
			MonoBehaviourSingleton<StageObjectManager>.I.AddNotifyInterface(this);
		}
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		if (base.gameObject.activeSelf)
		{
			timeCount += Time.deltaTime;
			float t = timeCount / appearTime;
			if (appearTime < timeCount)
			{
				OnDestroy();
			}
			else
			{
				if (timeStartScale != timeEndScale)
				{
					Vector3 scale = Vector3.Lerp(timeStartScale, timeEndScale, t);
					SetScale(scale);
				}
				if (isLandHit)
				{
					Vector3 position = _transform.position;
					float num = StageManager.GetHeight(position);
					if (!isShotArrow)
					{
						num += radius;
					}
					if (num - 1f >= position.y)
					{
						Vector3 a = position - this.prevPosition;
						float d = 0f;
						if (a.y != 0f)
						{
							float num2 = num;
							Vector3 prevPosition = this.prevPosition;
							d = (num2 - prevPosition.y) / a.y;
						}
						if ((Object)controller != (Object)null)
						{
							controller.OnLandHit();
						}
						isLandHitDelete = true;
						landHitPosition = a * d + this.prevPosition;
						landHitRotation = Quaternion.identity;
						position.y = 0f;
						OnDestroy();
						return;
					}
				}
			}
			if (isLandHitDelete && !isDestroyed)
			{
				OnDestroy();
			}
		}
	}

	protected virtual void FixedUpdate()
	{
		endVec = Vector3.zero;
		if (capsuleHeight <= 0f)
		{
			float num = Vector3.Distance(prevPosition, _transform.position);
			float num2 = num;
			Vector3 localScale = _transform.localScale;
			num = num2 / localScale.x;
			if (num > 0f && (Object)capsuleCollider != (Object)null)
			{
				Vector3 offset = this.offset;
				offset.z -= num * 0.5f;
				capsuleCollider.center = offset;
				capsuleCollider.height = num + capsuleCollider.radius * 2f;
				endVec = _transform.rotation * (Vector3.back * (num * 0.5f + capsuleCollider.radius));
			}
		}
		prevPosition = _transform.position;
	}

	protected virtual bool IsLoopEnd()
	{
		return false;
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (isObjectHitDelete && (collider.gameObject.layer == 9 || collider.gameObject.layer == 21))
		{
			isLandHitDelete = true;
			landHitPosition = Utility.ClosestPointOnCollider(collider, prevPosition);
			landHitRotation = _transform.rotation * Quaternion.Euler(new Vector3(-90f, 0f, 0f));
			_rigidbody.Sleep();
		}
		else
		{
			if ((Object)controller != (Object)null)
			{
				if (!controller.IsHit(collider))
				{
					return;
				}
				controller.OnHit(collider);
				if (controller.IsBreak(collider))
				{
					NotifyBroken(true);
					endBullet = null;
					OnDestroy();
					return;
				}
			}
			if (colliderProcessor != null)
			{
				colliderProcessor.OnTriggerEnter(collider);
			}
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if ((Object)controller != (Object)null)
		{
			if (!controller.IsHit(collider))
			{
				return;
			}
			controller.OnHitStay(collider);
		}
		if (colliderProcessor != null)
		{
			colliderProcessor.OnTriggerStay(collider);
		}
	}

	public void OnTriggerExit(Collider collider)
	{
		if (colliderProcessor != null)
		{
			colliderProcessor.OnTriggerExit(collider);
		}
	}

	public virtual void OnDetachedObject(StageObject stage_object)
	{
		if ((Object)stageObject == (Object)stage_object)
		{
			stageObject = null;
		}
	}

	public virtual float GetTime()
	{
		return timeCount;
	}

	public virtual bool IsEnable()
	{
		return !isDestroyed;
	}

	public virtual void SortHitStackList(List<AttackHitColliderProcessor.HitResult> stack_list)
	{
		stack_list.Sort(delegate(AttackHitColliderProcessor.HitResult a, AttackHitColliderProcessor.HitResult b)
		{
			float num = (a.target._position - prevPosition).sqrMagnitude - (b.target._position - prevPosition).sqrMagnitude;
			return (num >= 0f) ? 1 : (-1);
		});
	}

	public virtual Vector3 GetCrossCheckPoint(Collider from_collider)
	{
		Vector3 vector = from_collider.bounds.center;
		if (endVec != Vector3.zero)
		{
			vector += endVec * 2f;
		}
		return vector;
	}

	public bool CheckHitAttack(AttackHitInfo info, Collider to_collider, StageObject to_object)
	{
		if (attackHitChecker != null && !attackHitChecker.CheckHitAttack(info, to_collider, to_object))
		{
			return false;
		}
		return true;
	}

	public void OnHitAttack(AttackHitInfo info, AttackHitColliderProcessor.HitParam hit_param)
	{
		if (attackHitChecker != null)
		{
			attackHitChecker.OnHitAttack(info, hit_param);
		}
		if (isCharacterHitDelete && hit_param.toObject is Character)
		{
			isLandHitDelete = false;
			OnDestroy();
		}
		else if ((isCharacterHitDelete || !isShotArrow) && (!(hit_param.fromObject is Player) || !(hit_param.toObject is BarrierBulletObject)) && isObjectHitDelete)
		{
			isLandHitDelete = true;
			landHitPosition = Utility.ClosestPointOnCollider(hit_param.toCollider, prevPosition);
			landHitRotation = _transform.rotation * Quaternion.Euler(new Vector3(-90f, 0f, 0f));
			_rigidbody.Sleep();
		}
	}

	public AttackInfo GetAttackInfo()
	{
		return colliderProcessor.attackInfo;
	}

	public StageObject GetFromObject()
	{
		return colliderProcessor.fromObject;
	}

	public virtual void OnDestroy()
	{
		if (!AppMain.isApplicationQuit && !isDestroyed)
		{
			isDestroyed = true;
			if ((Object)_rigidbody != (Object)null)
			{
				_rigidbody.Sleep();
			}
			if ((Object)_collider != (Object)null)
			{
				_collider.enabled = false;
			}
			capsuleCollider = null;
			if ((Object)controller != (Object)null)
			{
				controller.enabled = false;
			}
			if (colliderProcessor != null)
			{
				if ((Object)endBullet != (Object)null)
				{
					CreateEndBullet();
				}
				colliderProcessor.OnDestroy();
				colliderProcessor = null;
			}
			if (isLandHitDelete && !string.IsNullOrEmpty(landHitEfect))
			{
				Transform effect = EffectManager.GetEffect(landHitEfect, null);
				if ((Object)effect != (Object)null)
				{
					effect.localPosition = landHitPosition;
					effect.localRotation = landHitRotation;
				}
			}
			Transform parent = (!MonoBehaviourSingleton<StageObjectManager>.IsValid()) ? MonoBehaviourSingleton<EffectManager>.I._transform : MonoBehaviourSingleton<StageObjectManager>.I._transform;
			if ((Object)bulletEffect != (Object)null)
			{
				bulletEffect.parent = parent;
				EffectManager.ReleaseEffect(bulletEffect.gameObject, !m_isDisablePlayEndAnim, false);
			}
			if (MonoBehaviourSingleton<StageObjectManager>.IsValid())
			{
				MonoBehaviourSingleton<StageObjectManager>.I.RemoveNotifyInterface(this);
			}
			NotifyDestroy();
			Object.Destroy(base.gameObject);
		}
	}

	private void CreateEndBullet()
	{
		if (MonoBehaviourSingleton<StageObjectManager>.IsValid())
		{
			if (string.IsNullOrEmpty(endBullet.name))
			{
				Log.Error("endBullet.name is empty, so can't create EndBullet!!");
			}
			else if (endBullet.type == BulletData.BULLET_TYPE.ICE_FLOOR)
			{
				Enemy enemy = stageObject as Enemy;
				if (!((Object)enemy == (Object)null))
				{
					List<Vector3> list = new List<Vector3>(1);
					List<Quaternion> rotList = new List<Quaternion>();
					list.Add(_transform.position);
					enemy.ActCreateIceFloor(endBullet, list, rotList);
				}
			}
			else if (endBullet.type == BulletData.BULLET_TYPE.DECOY)
			{
				CreateEndBulletDecoy();
			}
			else
			{
				AttackInfo endBulletAttackInfo = GetEndBulletAttackInfo();
				if (endBulletAttackInfo == null)
				{
					Log.Error("AttackInfo is null!!");
				}
				else if (!((Object)stageObject == (Object)null))
				{
					Transform transform = MonoBehaviourSingleton<StageObjectManager>.I._transform;
					if ((Object)transform == (Object)null)
					{
						Log.Error("parentTrans is null, so can't create EndBullet!!");
					}
					else if (type == BulletData.BULLET_TYPE.HIGH_EXPLOSIVE)
					{
						HighExplosiveSettings(endBulletAttackInfo, transform);
					}
					else
					{
						BulletObject bulletObject = ShotEndBullet(endBulletAttackInfo, transform);
						BulletData.BulletHoming dataHoming = endBullet.dataHoming;
						if (dataHoming != null && dataHoming.isTakeOverTarget && (Object)m_targetObject != (Object)null)
						{
							bulletObject.SetTarget(m_targetObject);
						}
						if (type == BulletData.BULLET_TYPE.BREAKABLE && bulletData.dataBreakable != null)
						{
							if (bulletData.dataBreakable.isTakeOverTarget && (Object)m_targetObject != (Object)null)
							{
								bulletObject.SetTarget(m_targetObject);
							}
							if (bulletData.dataBreakable.isTakeOverHitCount)
							{
								BulletControllerBreakable bulletControllerBreakable = controller as BulletControllerBreakable;
								BulletControllerBreakable bulletControllerBreakable2 = bulletObject.controller as BulletControllerBreakable;
								if ((Object)bulletControllerBreakable != (Object)null && (Object)bulletControllerBreakable2 != (Object)null)
								{
									bulletControllerBreakable2.SetHitCount(bulletControllerBreakable.GetHitCount());
								}
							}
						}
					}
				}
			}
		}
	}

	private void CreateEndBulletDecoy()
	{
		Self self = stageObject as Self;
		if (!((Object)self == (Object)null))
		{
			AnimEventData.EventData eventData = new AnimEventData.EventData();
			eventData.stringArgs = new string[1]
			{
				endBullet.name
			};
			AnimEventData.EventData eventData2 = eventData;
			float[] obj = new float[3];
			Vector3 position = _transform.position;
			obj[0] = position.x;
			Vector3 position2 = _transform.position;
			obj[2] = position2.z;
			eventData2.floatArgs = obj;
			eventData.intArgs = new int[1]
			{
				(masterSkill == null) ? (-1) : masterSkill.skillIndex
			};
			self.EventShotDecoy(eventData);
		}
	}

	protected void HighExplosiveSettings(AttackInfo _atkInfo, Transform _parentTrans)
	{
		if (!((Object)bulletData == (Object)null))
		{
			BulletData.BulletHighExplosive dataHighExplosive = bulletData.dataHighExplosive;
			if (dataHighExplosive != null && MonoBehaviourSingleton<StageObjectManager>.IsValid())
			{
				List<StageObject> playerList = MonoBehaviourSingleton<StageObjectManager>.I.playerList;
				if (playerList != null && playerList.Count > 0)
				{
					switch (dataHighExplosive.targetingType)
					{
					case BulletData.BulletHighExplosive.TARGETING_TYPE.ALL_PLAYERS:
						for (int j = 0; j < playerList.Count; j++)
						{
							BulletObject bulletObject2 = ShotEndBullet(_atkInfo, _parentTrans);
							if (endBullet.dataHoming != null || endBullet.dataHealingHomingBullet != null)
							{
								bulletObject2.SetTarget(playerList[j]);
							}
						}
						break;
					case BulletData.BulletHighExplosive.TARGETING_TYPE.ALL_PLAYERS_EXCEPT_ME:
					{
						int i = 0;
						for (int count = playerList.Count; i < count; i++)
						{
							if (!((Object)playerList[i] == (Object)stageObject))
							{
								BulletObject bulletObject = ShotEndBullet(_atkInfo, _parentTrans);
								if (endBullet.dataHoming != null || endBullet.dataHealingHomingBullet != null)
								{
									bulletObject.SetTarget(playerList[i]);
								}
							}
						}
						break;
					}
					}
				}
			}
		}
	}

	private List<StageObject> GetSortedListByPlayerDistance(List<StageObject> allPlayerList, StageObject me)
	{
		if (allPlayerList == null || allPlayerList.Count < 1 || (Object)me == (Object)null)
		{
			return null;
		}
		List<StageObject> list = new List<StageObject>(allPlayerList);
		list.Remove(me);
		Vector3 targetPos = me.transform.position;
		list.Sort((StageObject a, StageObject b) => Mathf.RoundToInt((a.transform.position - targetPos).magnitude - (b.transform.position - targetPos).magnitude));
		return list;
	}

	private BulletObject ShotEndBullet(AttackInfo atkInfo, Transform parentTrans)
	{
		Transform transform = Utility.CreateGameObject(endBullet.name, parentTrans, -1);
		if ((Object)transform == (Object)null)
		{
			Log.Error("Failed to create Bullet!! name:" + endBullet.name);
			return null;
		}
		BulletObject bulletObject = transform.gameObject.AddComponent(GetType()) as BulletObject;
		if ((Object)bulletObject == (Object)null)
		{
			Object.Destroy(transform.gameObject);
			return null;
		}
		bulletObject.SetBaseScale(baseScale);
		if (masterSkill != null)
		{
			bulletObject.SetEndBulletSkillIndex(masterSkill.skillIndex);
		}
		bulletObject.Shot(stageObject, atkInfo, endBullet, _transform.position, _transform.rotation, null, false, m_exAtk, m_attackMode, null, null);
		bulletObject.attackHitChecker = attackHitChecker;
		if (isBulletTakeoverTarget)
		{
			bulletObject.SetTarget(m_targetObject);
		}
		return bulletObject;
	}

	private AttackInfo GetEndBulletAttackInfo()
	{
		AttackInfo attackInfo = colliderProcessor.attackInfo;
		if (attackInfo == null)
		{
			return null;
		}
		if ((Object)stageObject == (Object)null)
		{
			return attackInfo;
		}
		if (string.IsNullOrEmpty(attackInfo.nextBulletInfoName))
		{
			return attackInfo;
		}
		AttackInfo attackInfo2 = stageObject.FindAttackInfo(attackInfo.nextBulletInfoName, true, false);
		if (attackInfo2 == null)
		{
			Log.Error("Not found AttackInfo for EndBullet!! nextBulletInfoName:" + attackInfo.nextBulletInfoName);
			return attackInfo;
		}
		return attackInfo2;
	}

	public void SetBaseScale(Vector3 _scale)
	{
		baseScale = _scale;
	}

	public void SetRadius(float _radius)
	{
		radius = _radius;
		if (isColliderCreate && (Object)capsuleCollider != (Object)null)
		{
			if (radius <= 0f)
			{
				Object.Destroy(_collider);
				_collider = null;
			}
			else
			{
				capsuleCollider.radius = radius;
				capsuleCollider.height = radius * 2f;
			}
		}
	}

	public void SetScale(Vector3 scale)
	{
		_transform.localScale = Vector3.Scale(baseScale, scale);
	}

	public void SetHitOffset(Vector3 _offset)
	{
		offset = _offset;
		if ((Object)capsuleCollider != (Object)null)
		{
			capsuleCollider.center = _offset;
		}
	}

	public void SetCapsuleAxis(BulletData.AXIS axis)
	{
		if (isColliderCreate && (Object)capsuleCollider != (Object)null)
		{
			int direction = 2;
			if (axis != BulletData.AXIS.NONE)
			{
				direction = (int)axis;
			}
			capsuleCollider.direction = direction;
		}
	}

	public void SetCapsuleHeight(float height)
	{
		if (isColliderCreate && !(height <= 0f) && (Object)capsuleCollider != (Object)null)
		{
			capsuleCollider.height = height;
			capsuleHeight = height;
		}
	}

	public virtual void Shot(StageObject master, AttackInfo atkInfo, BulletData bulletData, Vector3 pos, Quaternion rot, string exEffectName = null, bool reference_attack = true, AtkAttribute exAtk = null, Player.ATTACK_MODE attackMode = Player.ATTACK_MODE.NONE, DamageDistanceTable.DamageDistanceData damageDistanceData = null, SkillInfo.SkillParam exSkillParam = null)
	{
		Player player = master as Player;
		base.gameObject.SetActive(true);
		stageObject = master;
		m_exAtk = exAtk;
		m_attackMode = attackMode;
		string text = bulletData.data.GetEffectName(player);
		if (!string.IsNullOrEmpty(exEffectName))
		{
			text = exEffectName;
		}
		if (!string.IsNullOrEmpty(text))
		{
			bulletEffect = EffectManager.GetEffect(text, _transform);
			if ((Object)bulletEffect != (Object)null)
			{
				bulletEffect.localPosition = bulletData.data.dispOffset;
				bulletEffect.localRotation = Quaternion.Euler(bulletData.data.dispRotation);
				bulletEffect.localScale = Vector3.one;
			}
		}
		AttackHitInfo attackHitInfo = atkInfo as AttackHitInfo;
		if (exAtk != null)
		{
			masterAtk = exAtk;
		}
		else if (attackHitInfo != null)
		{
			if ((Object)player != (Object)null && HasEndBulletSkillIndex)
			{
				int skillIndex = player.skillInfo.skillIndex;
				player.skillInfo.skillIndex = m_endBulletSkillIndex;
				master.GetAtk(attackHitInfo, ref masterAtk);
				player.skillInfo.skillIndex = skillIndex;
			}
			else
			{
				master.GetAtk(attackHitInfo, ref masterAtk);
			}
		}
		masterSkill = null;
		if ((Object)player != (Object)null)
		{
			if (exSkillParam != null)
			{
				masterSkill = exSkillParam;
			}
			else
			{
				masterSkill = player.skillInfo.actSkillParam;
				if ((Object)player.TrackingTargetBullet != (Object)null && player.TrackingTargetBullet.IsReplaceSkill && atkInfo.isSkillReference)
				{
					masterSkill = player.TrackingTargetBullet.SkillParamForBullet;
				}
				if (HasEndBulletSkillIndex)
				{
					masterSkill = player.GetSkillParam(m_endBulletSkillIndex);
				}
			}
		}
		if (bulletData.data.isEmitGround)
		{
			pos.y = 0f;
		}
		SetBulletData(bulletData, masterSkill, pos, rot);
		if (bulletData.type == BulletData.BULLET_TYPE.OBSTACLE)
		{
			AttackObstacle attackObstacle = base.gameObject.AddComponent<AttackObstacle>();
			attackObstacle.Initialize(this as AnimEventShot, bulletData.dataObstacle.colliderStartTime);
		}
		else if (bulletData.type == BulletData.BULLET_TYPE.BARRIER)
		{
			BarrierBulletObject barrierBulletObject = base.gameObject.AddComponent<BarrierBulletObject>();
			barrierBulletObject.Initialize(this);
		}
		else if (bulletData.type != BulletData.BULLET_TYPE.HEALING_HOMING && bulletData.type != BulletData.BULLET_TYPE.ENEMY_PRESENT)
		{
			int layer = (!(master is Player)) ? 15 : 14;
			Utility.SetLayerWithChildren(_transform, layer);
		}
		timeCount = 0f;
		if (MonoBehaviourSingleton<AttackColliderManager>.IsValid())
		{
			colliderProcessor = MonoBehaviourSingleton<AttackColliderManager>.I.CreateProcessor(atkInfo, stageObject, _collider, this, attackMode, damageDistanceData);
			if (reference_attack)
			{
				attackHitChecker = stageObject.ReferenceAttackHitChecker();
			}
			if (bulletData.type == BulletData.BULLET_TYPE.SNATCH || bulletData.type == BulletData.BULLET_TYPE.PAIR_SWORDS_LASER)
			{
				colliderProcessor.ValidTriggerStay();
			}
			if (bulletData.type == BulletData.BULLET_TYPE.CRASH_BIT)
			{
				colliderProcessor.ValidTriggerStay();
				colliderProcessor.ValidMultiHitInterval();
			}
		}
		Vector3 b = Vector3.zero;
		if (_collider is BoxCollider)
		{
			b = (_collider as BoxCollider).center;
		}
		else if (_collider is SphereCollider)
		{
			b = (_collider as SphereCollider).center;
		}
		else if (_collider is CapsuleCollider)
		{
			b = (_collider as CapsuleCollider).center;
		}
		startColliderPos = _transform.position + b;
		isDestroyed = false;
		prevPosition = pos;
		if ((Object)controller != (Object)null)
		{
			controller.OnShot();
		}
	}

	protected virtual void SetBulletData(BulletData bullet, SkillInfo.SkillParam _skillParam, Vector3 pos, Quaternion rot)
	{
		if (!((Object)bullet == (Object)null))
		{
			bulletData = bullet;
			type = bullet.type;
			appearTime = bullet.data.appearTime;
			dispOffset = bulletData.data.dispOffset;
			dispRotation = bulletData.data.dispRotation;
			SetRadius(bullet.data.radius);
			SetCapsuleHeight(bullet.data.capsuleHeight);
			SetCapsuleAxis(bullet.data.capsuleAxis);
			SetScale(bullet.data.timeStartScale);
			SetHitOffset(bullet.data.hitOffset);
			timeStartScale = bullet.data.timeStartScale;
			timeEndScale = bullet.data.timeEndScale;
			isCharacterHitDelete = bullet.data.isCharacterHitDelete;
			isObjectHitDelete = bullet.data.isObjectHitDelete;
			isLandHit = bullet.data.isLandHit;
			landHitEfect = bullet.data.landHiteffectName;
			endBullet = bullet.data.endBullet;
			isBulletTakeoverTarget = bullet.data.isBulletTakeoverTarget;
			switch (bullet.type)
			{
			case BulletData.BULLET_TYPE.FALL:
				controller = base.gameObject.AddComponent<BulletControllerFall>();
				break;
			case BulletData.BULLET_TYPE.HOMING:
				controller = base.gameObject.AddComponent<BulletControllerHoming>();
				break;
			case BulletData.BULLET_TYPE.CURVE:
				controller = base.gameObject.AddComponent<BulletControllerCurve>();
				break;
			case BulletData.BULLET_TYPE.BREAKABLE:
				controller = base.gameObject.AddComponent<BulletControllerBreakable>();
				break;
			case BulletData.BULLET_TYPE.OBSTACLE_CYLINDER:
				controller = base.gameObject.AddComponent<BulletControllerObstacleCylinder>();
				break;
			case BulletData.BULLET_TYPE.SNATCH:
				controller = base.gameObject.AddComponent<BulletControllerSnatch>();
				break;
			case BulletData.BULLET_TYPE.PAIR_SWORDS_SOUL:
				controller = base.gameObject.AddComponent<BulletControllerPairSwordsSoul>();
				break;
			case BulletData.BULLET_TYPE.PAIR_SWORDS_LASER:
				controller = base.gameObject.AddComponent<BulletControllerPairSwordsLaser>();
				break;
			case BulletData.BULLET_TYPE.HEALING_HOMING:
				controller = base.gameObject.AddComponent<BulletControllerHealingHoming>();
				break;
			case BulletData.BULLET_TYPE.ARROW_SOUL:
				controller = base.gameObject.AddComponent<BulletControllerArrowSoul>();
				break;
			case BulletData.BULLET_TYPE.ENEMY_PRESENT:
				controller = base.gameObject.AddComponent<BulletControllerEnemyPresent>();
				break;
			case BulletData.BULLET_TYPE.CRASH_BIT:
				controller = base.gameObject.AddComponent<BulletControllerCrashBit>();
				break;
			case BulletData.BULLET_TYPE.BARRIER:
				controller = base.gameObject.AddComponent<BulletControllerBarrier>();
				break;
			default:
				controller = base.gameObject.AddComponent<BulletControllerBase>();
				break;
			}
			if ((Object)controller != (Object)null)
			{
				controller.Initialize(bullet, _skillParam, pos, rot);
				controller.RegisterBulletObject(this);
				controller.RegisterFromObject(stageObject);
				switch (bullet.type)
				{
				case BulletData.BULLET_TYPE.PAIR_SWORDS_SOUL:
				{
					IObservable observable = controller as IObservable;
					if (observable != null)
					{
						Player player = stageObject as Player;
						if ((Object)player != (Object)null)
						{
							observable.RegisterObserver(player.pairSwordsCtrl);
						}
					}
					break;
				}
				case BulletData.BULLET_TYPE.BREAKABLE:
				case BulletData.BULLET_TYPE.ENEMY_PRESENT:
				case BulletData.BULLET_TYPE.BARRIER:
					RegisterObserver();
					break;
				}
			}
			Character character = stageObject as Character;
			if ((Object)character != (Object)null)
			{
				SetTarget(character.actionTarget);
			}
		}
	}

	public void SetTarget(StageObject obj)
	{
		m_targetObject = obj;
		if ((Object)stageObject != (Object)null)
		{
			Character character = stageObject as Character;
			if ((Object)character != (Object)null && character.IsValidBuffBlind())
			{
				m_targetObject = null;
			}
		}
		controller.RegisterTargetObject(m_targetObject);
	}

	public void SetTarget(TargetPoint targetPoint)
	{
		BulletControllerArrowSoul bulletControllerArrowSoul = controller as BulletControllerArrowSoul;
		if (!((Object)bulletControllerArrowSoul == (Object)null))
		{
			bulletControllerArrowSoul.SetTarget(targetPoint);
		}
	}

	public void SetPuppetTargetPos(Vector3 pos)
	{
		BulletControllerArrowSoul bulletControllerArrowSoul = controller as BulletControllerArrowSoul;
		if (!((Object)bulletControllerArrowSoul == (Object)null))
		{
			bulletControllerArrowSoul.SetPuppetTargetPos(pos);
		}
	}

	public void EndBossPierceArrow()
	{
		isBossPierceArrow = false;
		isCharacterHitDelete = false;
	}

	public int GetObservedID()
	{
		return observedID;
	}

	public void SetObservedID(int id)
	{
		observedID = id;
	}

	public void RegisterObserver()
	{
		if (!bulletObserverList.Contains(stageObject))
		{
			bulletObserverList.Add(stageObject);
			SetObservedID(stageObject.GetObservedID());
			stageObject.RegisterObservable(this);
		}
	}

	public void NotifyBroken(bool isSendOnlyOriginal = true)
	{
		for (int i = 0; i < bulletObserverList.Count; i++)
		{
			bulletObserverList[i].OnBreak(observedID, isSendOnlyOriginal);
		}
	}

	public void NotifyDestroy()
	{
		for (int i = 0; i < bulletObserverList.Count; i++)
		{
			bulletObserverList[i].OnBulletDestroy(observedID);
		}
	}

	public void ForceBreak()
	{
		endBullet = null;
		OnDestroy();
	}
}
