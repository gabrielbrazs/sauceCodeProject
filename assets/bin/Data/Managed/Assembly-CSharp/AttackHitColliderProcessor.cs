using System.Collections.Generic;
using UnityEngine;

public class AttackHitColliderProcessor : AttackColliderProcessor
{
	public class HitParam
	{
		public AttackHitColliderProcessor processor;

		public StageObject fromObject;

		public StageObject toObject;

		public Collider fromCollider;

		public Collider toCollider;

		public Vector3 point;

		public float distanceXZ;

		public Quaternion rot;

		public Vector3 crossCheckPoint;

		public float time;

		public List<TargetPoint> targetPointList;

		public int regionID = -1;

		public bool isHitAim;

		public bool isSpAttackHit;

		public Player.ATTACK_MODE attackMode;

		public DamageDistanceTable.DamageDistanceData damageDistanceData;

		public Vector3 exHitPos = Vector3.zero;
	}

	public class HitResult
	{
		public StageObject target;

		public List<HitParam> hitParams = new List<HitParam>();
	}

	private bool isAlreadyCheckedTask;

	private bool isAlreadyCheckedDeliveryBattleInfo;

	protected List<HitResult> stackList = new List<HitResult>();

	protected AttackHitInfo attackHitInfo => base.attackInfo as AttackHitInfo;

	public override void OnDestroy()
	{
		CheckStackList();
		base.OnDestroy();
	}

	public override bool IsBusy()
	{
		return stackList.Count > 0;
	}

	public override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		CheckStackList();
	}

	private void CheckStackList()
	{
		if (stackList.Count > 0)
		{
			if ((Object)base.fromObject == (Object)null)
			{
				stackList.Clear();
			}
			else
			{
				List<HitResult> range = stackList.GetRange(0, stackList.Count);
				stackList.Clear();
				if (base.colliderInterface != null)
				{
					base.colliderInterface.SortHitStackList(range);
				}
				int i = 0;
				for (int count = range.Count; i < count; i++)
				{
					HitResult hitResult = range[i];
					HitParam hitParam = hitResult.target.SelectHitCollider(this, hitResult.hitParams);
					if (base.colliderInterface == null || base.colliderInterface.CheckHitAttack(attackHitInfo, hitParam.toCollider, hitResult.target))
					{
						if (base.fromObject.CheckHitAttack(attackHitInfo, hitParam.toCollider, hitResult.target))
						{
							base.fromObject.OnHitAttack(attackHitInfo, hitParam);
							if (base.colliderInterface != null)
							{
								base.colliderInterface.OnHitAttack(attackHitInfo, hitParam);
								if (!base.colliderInterface.IsEnable())
								{
									break;
								}
							}
						}
						else
						{
							Self self = base.fromObject as Self;
							if ((Object)self != (Object)null)
							{
								self.CancelHit();
							}
						}
					}
				}
			}
		}
	}

	public override void OnTriggerStay(Collider to_collider)
	{
		base.OnTriggerStay(to_collider);
		if (m_isValidTriggerStay && !(attackHitInfo.hitIntervalTime <= 0f))
		{
			if (m_isValidMultiHitInterval)
			{
				HitProc(to_collider);
			}
			else
			{
				m_hitInterval -= Time.deltaTime;
				if (!(m_hitInterval > 0f))
				{
					m_hitInterval = attackHitInfo.hitIntervalTime;
					HitProc(to_collider);
				}
			}
		}
	}

	public override void OnTriggerEnter(Collider to_collider)
	{
		base.OnTriggerEnter(to_collider);
		HitProc(to_collider);
		m_hitInterval = attackHitInfo.hitIntervalTime;
	}

	private void HitProc(Collider to_collider)
	{
		if ((base.colliderInterface == null || base.colliderInterface.IsEnable()) && !((Object)base.fromCollider == (Object)null) && !((Object)base.fromObject == (Object)null) && base.fromCollider.enabled && !((Object)to_collider.gameObject == (Object)base.fromCollider.gameObject))
		{
			AnimEventCollider.AtkColliderHiter component = to_collider.gameObject.GetComponent<AnimEventCollider.AtkColliderHiter>();
			if (!((Object)component != (Object)null))
			{
				DangerRader component2 = to_collider.gameObject.GetComponent<DangerRader>();
				if (!((Object)component2 != (Object)null))
				{
					EscapePointObject component3 = to_collider.gameObject.GetComponent<EscapePointObject>();
					if (!((Object)component3 != (Object)null))
					{
						StageObject to_object = to_collider.gameObject.GetComponentInParent<StageObject>();
						if (!((Object)to_object == (Object)null) && !((Object)to_object == (Object)base.fromObject) && to_object.ignoreHitAttackColliders.IndexOf(to_collider) < 0)
						{
							if (m_isValidTriggerStay && m_isValidMultiHitInterval)
							{
								if (to_object.IsIgnoreByHitInterval(base.fromCollider))
								{
									return;
								}
								to_object.SetHitIntervalStatus(base.fromCollider, attackHitInfo.hitIntervalTime);
							}
							if ((base.colliderInterface == null || base.colliderInterface.CheckHitAttack(attackHitInfo, to_collider, to_object)) && base.fromObject.CheckHitAttack(attackHitInfo, to_collider, to_object))
							{
								Vector3 crossCheckPoint = base.colliderInterface.GetCrossCheckPoint(base.fromCollider);
								Vector3 vector = Utility.ClosestPointOnCollider(to_collider, crossCheckPoint);
								Vector3 vector2 = vector - crossCheckPoint;
								if (vector2 == Vector3.zero)
								{
									vector2 = to_collider.bounds.center - crossCheckPoint;
								}
								Quaternion rot = Quaternion.LookRotation(vector2);
								HitResult hitResult = null;
								stackList.ForEach(delegate(HitResult o)
								{
									if ((Object)o.target == (Object)to_object)
									{
										hitResult = o;
									}
								});
								if (hitResult == null)
								{
									hitResult = new HitResult();
									hitResult.target = to_object;
									stackList.Add(hitResult);
								}
								float time = 0f;
								if (base.colliderInterface != null)
								{
									time = base.colliderInterface.GetTime();
								}
								HitParam hitParam = new HitParam();
								hitParam.processor = this;
								hitParam.fromObject = base.fromObject;
								hitParam.toObject = to_object;
								hitParam.fromCollider = base.fromCollider;
								hitParam.toCollider = to_collider;
								hitParam.point = vector;
								if (m_damageDistanceData != null)
								{
									Vector3 vector3 = crossCheckPoint;
									BulletObject bulletObject = base.colliderInterface as BulletObject;
									if ((Object)bulletObject != (Object)null)
									{
										vector3 = bulletObject.startColliderPos;
									}
									Vector3 vector4 = Utility.ClosestPointOnColliderFix(to_collider, vector3);
									if (vector3 == vector4)
									{
										hitParam.distanceXZ = 0f;
									}
									else
									{
										Vector2 a = vector4.ToVector2XZ();
										Vector2 b = base.fromObject._position.ToVector2XZ();
										hitParam.distanceXZ = Vector2.Distance(a, b);
									}
								}
								hitParam.exHitPos = base.fromCollider.gameObject.transform.position;
								hitParam.rot = rot;
								hitParam.time = time;
								hitParam.targetPointList = base.targetPointList;
								hitParam.crossCheckPoint = crossCheckPoint;
								hitParam.attackMode = m_attackMode;
								hitParam.damageDistanceData = m_damageDistanceData;
								hitResult.hitParams.Add(hitParam);
								Self self = base.fromObject as Self;
								if ((Object)self != (Object)null && !isAlreadyCheckedTask)
								{
									BattleCheckerBase.JudgementParam judgementParam = BattleCheckerBase.JudgementParam.Create(base.attackInfo, self);
									self.taskChecker.OnAttackHit(base.attackInfo.name, judgementParam, 0);
									isAlreadyCheckedTask = true;
								}
								if (base.fromObject is Player && MonoBehaviourSingleton<InGameManager>.IsValid() && !isAlreadyCheckedDeliveryBattleInfo)
								{
									MonoBehaviourSingleton<InGameManager>.I.deliveryBattleChecker.AddTotalAttackCount(1);
									isAlreadyCheckedDeliveryBattleInfo = true;
								}
							}
						}
					}
				}
			}
		}
	}
}
