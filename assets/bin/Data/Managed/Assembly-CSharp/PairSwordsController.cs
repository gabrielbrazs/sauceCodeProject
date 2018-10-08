using System.Collections.Generic;
using UnityEngine;

public class PairSwordsController : IObserver, IWeaponController
{
	public enum CHARGE_STATE
	{
		NONE,
		LOOP,
		LASER_SHOT,
		LASER_LOOP,
		END
	}

	private static readonly Vector3 OFFSET_LASER_WAIT_EFFECT = new Vector3(0f, 1f, 1f);

	private static readonly int HASH_EFFECT_ON_WEAPON_FULL = Animator.StringToHash("Base Layer.LOOP1");

	private static readonly int HASH_EFFECT_ON_WEAPON_DEFAULT = Animator.StringToHash("Base Layer.LOOP2");

	private CHARGE_STATE chargeState;

	private InGameSettingsManager.Player.PairSwordsActionInfo pairSwordsInfo;

	private Player owner;

	private float timerForSpActionGaugeDecreaseAfterHit;

	private bool isExecLaserEnd;

	private bool isEventShotLaserExec;

	private bool isSetGaugePercentForLaser;

	private float gaugePercentForLaser;

	private int comboLvBySync;

	private List<AnimEventShot> bulletLaserList = new List<AnimEventShot>(3);

	private List<Transform> effectTransOnWeaponList = new List<Transform>(2);

	private List<Animator> effectAnimatorOnWeaponList = new List<Animator>(2);

	private Transform effectTransStartShotLaser;

	private float spActionGauge
	{
		get
		{
			if ((Object)owner == (Object)null)
			{
				return 0f;
			}
			return owner.spActionGauge[owner.weaponIndex];
		}
		set
		{
			if (!((Object)owner == (Object)null))
			{
				owner.spActionGauge[owner.weaponIndex] = value;
			}
		}
	}

	private float spActionGaugeMax
	{
		get
		{
			if ((Object)owner == (Object)null)
			{
				return 0f;
			}
			return owner.spActionGaugeMax[owner.weaponIndex];
		}
		set
		{
			if (!((Object)owner == (Object)null))
			{
				owner.spActionGaugeMax[owner.weaponIndex] = value;
			}
		}
	}

	private void SetChargeState(CHARGE_STATE chargeState)
	{
		this.chargeState = chargeState;
	}

	private void ResetTimerForSpActionGaugeDecreaseAfterHit()
	{
		timerForSpActionGaugeDecreaseAfterHit = 0f;
	}

	public void SetEventShotLaserExec()
	{
		isEventShotLaserExec = true;
	}

	public void SetGaugePercentForLaser()
	{
		if (!isSetGaugePercentForLaser)
		{
			gaugePercentForLaser = GetGaugeChargedPercent();
			isSetGaugePercentForLaser = true;
		}
	}

	public void AddBulletLaser(AnimEventShot bullet)
	{
		bulletLaserList.Add(bullet);
	}

	public void GetEffectTransStartShotLaser()
	{
		if (MonoBehaviourSingleton<EffectManager>.IsValid() && (Object)effectTransStartShotLaser == (Object)null)
		{
			effectTransStartShotLaser = EffectManager.GetEffect(pairSwordsInfo.Soul_EffectForWaitingLaser, owner._transform);
			effectTransStartShotLaser.localPosition = OFFSET_LASER_WAIT_EFFECT;
		}
	}

	public void Init(Player player)
	{
		owner = player;
		if (MonoBehaviourSingleton<InGameSettingsManager>.IsValid())
		{
			pairSwordsInfo = MonoBehaviourSingleton<InGameSettingsManager>.I.player.pairSwordsActionInfo;
		}
	}

	public void OnLoadComplete()
	{
		if (owner.CheckAttackModeAndSpType(Player.ATTACK_MODE.PAIR_SWORDS, SP_ATTACK_TYPE.SOUL))
		{
			effectTransOnWeaponList.Clear();
			effectAnimatorOnWeaponList.Clear();
			int currentWeaponElement = owner.GetCurrentWeaponElement();
			string name = pairSwordsInfo.Soul_EffectsForWeapon[currentWeaponElement];
			Transform transform = Utility.Find(owner.FindNode("R_Wep"), name);
			Transform transform2 = Utility.Find(owner.FindNode("L_Wep"), name);
			effectTransOnWeaponList.Add(transform);
			effectTransOnWeaponList.Add(transform2);
			effectAnimatorOnWeaponList.Add(transform.GetComponent<Animator>());
			effectAnimatorOnWeaponList.Add(transform2.GetComponent<Animator>());
		}
	}

	public void Update()
	{
		switch (chargeState)
		{
		case CHARGE_STATE.LOOP:
			if (!owner.IsCoopNone() && !owner.IsOriginal() && isExecLaserEnd)
			{
				SetChargeState(CHARGE_STATE.END);
			}
			break;
		case CHARGE_STATE.LASER_SHOT:
			if (isEventShotLaserExec)
			{
				SetChargeState(CHARGE_STATE.LASER_LOOP);
			}
			break;
		case CHARGE_STATE.LASER_LOOP:
			if (spActionGauge <= 0f)
			{
				owner.SetNextTrigger(0);
				SetChargeState(CHARGE_STATE.END);
			}
			break;
		case CHARGE_STATE.END:
			OnLaserEnd(false);
			isExecLaserEnd = false;
			SetChargeState(CHARGE_STATE.NONE);
			break;
		}
		timerForSpActionGaugeDecreaseAfterHit += Time.deltaTime;
		UpdateSpActionGauge();
		UpdateEffectOnWeapon();
	}

	private void UpdateSpActionGauge()
	{
		if (!((Object)owner == (Object)null) && owner.CheckAttackMode(Player.ATTACK_MODE.PAIR_SWORDS))
		{
			switch (owner.spAttackType)
			{
			case SP_ATTACK_TYPE.HEAT:
			{
				if (!owner.isBoostMode)
				{
					return;
				}
				float num = 1f + owner.buffParam.GetGaugeDecreaseRate();
				owner.spActionGauge[owner.weaponIndex] -= pairSwordsInfo.boostGaugeDecreasePerSecond * num * Time.deltaTime;
				break;
			}
			case SP_ATTACK_TYPE.SOUL:
				if ((chargeState == CHARGE_STATE.LASER_SHOT || chargeState == CHARGE_STATE.LASER_LOOP) && isEventShotLaserExec)
				{
					spActionGauge -= pairSwordsInfo.Soul_GaugeDecreaseShootingLaserPerSecond * Time.deltaTime;
				}
				else if (timerForSpActionGaugeDecreaseAfterHit >= pairSwordsInfo.Soul_TimeForGaugeDecreaseAfterHit && (!IsComboLvMax() || !(timerForSpActionGaugeDecreaseAfterHit < pairSwordsInfo.Soul_TimeForGaugeDecreaseAfterHitOnComboLvMax)))
				{
					if (chargeState == CHARGE_STATE.LOOP || (chargeState == CHARGE_STATE.LASER_SHOT && !isEventShotLaserExec))
					{
						spActionGauge -= pairSwordsInfo.Soul_GaugeDecreaseWaitingLaserPerSecond * Time.deltaTime;
					}
					else
					{
						spActionGauge -= pairSwordsInfo.Soul_GaugeDecreasePerSecond * Time.deltaTime;
					}
				}
				break;
			}
			if (spActionGauge <= 0f)
			{
				spActionGauge = 0f;
			}
		}
	}

	private void UpdateEffectOnWeapon()
	{
		if (!effectAnimatorOnWeaponList.IsNullOrEmpty())
		{
			for (int i = 0; i < effectAnimatorOnWeaponList.Count; i++)
			{
				if (!((Object)effectAnimatorOnWeaponList[i] == (Object)null))
				{
					int fullPathHash = effectAnimatorOnWeaponList[i].GetCurrentAnimatorStateInfo(0).fullPathHash;
					if (IsComboLvMax())
					{
						if (fullPathHash != HASH_EFFECT_ON_WEAPON_FULL)
						{
							effectAnimatorOnWeaponList[i].Play(HASH_EFFECT_ON_WEAPON_FULL);
						}
					}
					else
					{
						if (fullPathHash != HASH_EFFECT_ON_WEAPON_DEFAULT)
						{
							effectAnimatorOnWeaponList[i].Play(HASH_EFFECT_ON_WEAPON_DEFAULT);
						}
						Vector3 localScale = effectTransOnWeaponList[i].localScale;
						localScale.z = GetGaugeRate();
						effectTransOnWeaponList[i].localScale = localScale;
					}
				}
			}
		}
	}

	public void OnStartCharge()
	{
		SetChargeState(CHARGE_STATE.LOOP);
	}

	public void OnLaserEnd(bool isPacket = false)
	{
		if (MonoBehaviourSingleton<EffectManager>.IsValid() && (Object)effectTransStartShotLaser != (Object)null)
		{
			EffectManager.ReleaseEffect(effectTransStartShotLaser.gameObject, true, false);
			effectTransStartShotLaser = null;
		}
		ClearLaserBullet();
		if (pairSwordsInfo.Soul_SeIds.Length >= 2)
		{
			SoundManager.StopLoopSE(pairSwordsInfo.Soul_SeIds[1], owner);
		}
		if (pairSwordsInfo.Soul_SeIds.Length >= 3 && pairSwordsInfo.Soul_SeIds[2] > 0)
		{
			SoundManager.PlayOneShotSE(pairSwordsInfo.Soul_SeIds[2], owner._position);
		}
		isEventShotLaserExec = false;
		isSetGaugePercentForLaser = false;
		gaugePercentForLaser = 0f;
		owner.EndWaitingPacket(StageObject.WAITING_PACKET.PLAYER_PAIR_SWORDS_LASER_END);
		if ((Object)owner.playerSender != (Object)null)
		{
			owner.playerSender.OnPairSwordsLaserEnd();
		}
		if (isPacket)
		{
			owner.SetNextTrigger(0);
			isExecLaserEnd = true;
		}
	}

	private void ClearLaserBullet()
	{
		if (!bulletLaserList.IsNullOrEmpty())
		{
			for (int i = 0; i < bulletLaserList.Count; i++)
			{
				bulletLaserList[i].OnDestroy();
			}
			bulletLaserList.Clear();
		}
	}

	public void DecreaseSoulGaugeByDamage()
	{
		if (owner.CheckAttackModeAndSpType(Player.ATTACK_MODE.PAIR_SWORDS, SP_ATTACK_TYPE.SOUL) && !owner.IsInBarrier())
		{
			spActionGauge -= pairSwordsInfo.Soul_GaugeDecreaseByDamage;
			if (spActionGauge < 0f)
			{
				spActionGauge = 0f;
			}
		}
	}

	private float GetGaugeChargedPercent()
	{
		if (spActionGaugeMax <= 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01(spActionGauge / spActionGaugeMax) * 100f;
	}

	private float GetGaugeRate()
	{
		if (spActionGaugeMax <= 0f)
		{
			return 0f;
		}
		return Mathf.Clamp01(spActionGauge / spActionGaugeMax);
	}

	public int GetComboLv()
	{
		float gaugeChargedPercent = GetGaugeChargedPercent();
		int result = 1;
		for (int i = 0; i < pairSwordsInfo.Soul_GaugePercentForComboLv.Length; i++)
		{
			if (gaugeChargedPercent >= pairSwordsInfo.Soul_GaugePercentForComboLv[i])
			{
				result = 1 + i;
			}
		}
		if (comboLvBySync > 0)
		{
			result = comboLvBySync;
			comboLvBySync = 0;
		}
		return result;
	}

	public void SetComboLv(int lv)
	{
		comboLvBySync = lv;
	}

	public float GetAttackSpeedUpRate()
	{
		if (!owner.CheckAttackModeAndSpType(Player.ATTACK_MODE.PAIR_SWORDS, SP_ATTACK_TYPE.SOUL))
		{
			return 0f;
		}
		if (pairSwordsInfo.Soul_GaugePercentForComboLv.Length < pairSwordsInfo.Soul_NumOfComboLv)
		{
			return 0f;
		}
		if (pairSwordsInfo.Soul_AttackSpeedUpRatesByComboLv.Length < pairSwordsInfo.Soul_NumOfComboLv)
		{
			return 0f;
		}
		float gaugeChargedPercent = GetGaugeChargedPercent();
		float result = pairSwordsInfo.Soul_AttackSpeedUpRatesByComboLv[0];
		for (int i = 0; i < pairSwordsInfo.Soul_GaugePercentForComboLv.Length; i++)
		{
			if (gaugeChargedPercent >= pairSwordsInfo.Soul_GaugePercentForComboLv[i])
			{
				result = pairSwordsInfo.Soul_AttackSpeedUpRatesByComboLv[i];
			}
		}
		return result;
	}

	public float GetAtkRate()
	{
		if (!owner.CheckAttackModeAndSpType(Player.ATTACK_MODE.PAIR_SWORDS, SP_ATTACK_TYPE.SOUL))
		{
			return 1f;
		}
		if (pairSwordsInfo.Soul_NumOfComboLv <= 0)
		{
			return 1f;
		}
		if (pairSwordsInfo.Soul_GaugePercentForComboLv.Length < pairSwordsInfo.Soul_NumOfComboLv)
		{
			return 1f;
		}
		float gaugeChargedPercent = GetGaugeChargedPercent();
		float num = 1f;
		if (owner.attackID == pairSwordsInfo.Soul_SpLaserShotAttackId)
		{
			if (pairSwordsInfo.Soul_AtkRatesForLaserByComboLv.Length < pairSwordsInfo.Soul_NumOfComboLv)
			{
				return 1f;
			}
			if (isSetGaugePercentForLaser)
			{
				gaugeChargedPercent = gaugePercentForLaser;
			}
			else
			{
				gaugePercentForLaser = gaugeChargedPercent;
				isSetGaugePercentForLaser = true;
			}
			num = pairSwordsInfo.Soul_AtkRatesForLaserByComboLv[0];
			for (int i = 0; i < pairSwordsInfo.Soul_GaugePercentForComboLv.Length; i++)
			{
				if (gaugeChargedPercent >= pairSwordsInfo.Soul_GaugePercentForComboLv[i])
				{
					num = pairSwordsInfo.Soul_AtkRatesForLaserByComboLv[i];
				}
			}
			return num;
		}
		if (pairSwordsInfo.Soul_AtkRatesForBulletByComboLv.Length < pairSwordsInfo.Soul_NumOfComboLv)
		{
			return 1f;
		}
		num = pairSwordsInfo.Soul_AtkRatesForBulletByComboLv[0];
		for (int j = 0; j < pairSwordsInfo.Soul_GaugePercentForComboLv.Length; j++)
		{
			if (gaugeChargedPercent >= pairSwordsInfo.Soul_GaugePercentForComboLv[j])
			{
				num = pairSwordsInfo.Soul_AtkRatesForBulletByComboLv[j];
			}
		}
		return num;
	}

	private bool IsAbleToShotSoulLaser()
	{
		if (chargeState != CHARGE_STATE.LOOP)
		{
			return false;
		}
		return true;
	}

	public bool IsAbleToAlterSpAction()
	{
		if (owner.CheckSpAttackType(SP_ATTACK_TYPE.HEAT))
		{
			if (owner.isBoostMode)
			{
				return false;
			}
			if (!owner.IsSpActionGaugeHalfCharged())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsComboLvMax()
	{
		if (GetComboLv() == pairSwordsInfo.Soul_NumOfComboLv)
		{
			return true;
		}
		return false;
	}

	public bool CheckContinueBoostMode()
	{
		switch (owner.spAttackType)
		{
		case SP_ATTACK_TYPE.HEAT:
			if (spActionGauge > 0f)
			{
				return true;
			}
			break;
		case SP_ATTACK_TYPE.SOUL:
			if (!owner.IsCoopNone() && !owner.IsOriginal())
			{
				return true;
			}
			if (IsComboLvMax())
			{
				return true;
			}
			break;
		}
		return false;
	}

	public void OnHit()
	{
		ResetTimerForSpActionGaugeDecreaseAfterHit();
	}

	public void OnEndAction()
	{
	}

	public void OnActDead()
	{
		OnReaction();
	}

	public void OnActAvoid()
	{
		OnAvoid();
	}

	public void OnRelease()
	{
		if (IsAbleToShotSoulLaser() || (owner.attackID == pairSwordsInfo.Soul_SpLaserWaitAttackId && chargeState == CHARGE_STATE.NONE))
		{
			owner.ActAttack(pairSwordsInfo.Soul_SpLaserShotAttackId, true, true, string.Empty);
			SetChargeState(CHARGE_STATE.LASER_SHOT);
			if ((Object)owner.playerSender != (Object)null)
			{
				owner.playerSender.OnSyncSpActionGauge();
			}
		}
	}

	public void OnAvoid()
	{
		if (chargeState != 0)
		{
			SetChargeState(CHARGE_STATE.END);
		}
	}

	public void OnReaction()
	{
		if (chargeState != 0)
		{
			SetChargeState(CHARGE_STATE.END);
		}
	}
}
