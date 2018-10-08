using System;
using System.Collections;
using UnityEngine;

public class UIFieldBossInfo : MonoBehaviourSingleton<UIFieldBossInfo>
{
	[SerializeField]
	protected UILabel timeLabel;

	private bool active;

	private bool doingFadeOut;

	private bool doingFadeIn;

	private void Start()
	{
		if (!active)
		{
			SetActive(false);
		}
	}

	private void LateUpdate()
	{
		timeLabel.text = MonoBehaviourSingleton<InGameProgress>.I.GetRemainTime();
		if (MonoBehaviourSingleton<InGameProgress>.IsValid() && MonoBehaviourSingleton<InGameProgress>.I.remaindTime <= 0f)
		{
			FadeOut(0f, 1f, null);
		}
		if (MonoBehaviourSingleton<CoopManager>.IsValid() && !MonoBehaviourSingleton<CoopManager>.I.coopStage.GetIsInFieldEnemyBossBattle())
		{
			FadeOut(0f, 1f, null);
		}
	}

	public void SetActive(bool isActive)
	{
		active = isActive;
		base.gameObject.SetActive(isActive);
	}

	public void FadeIn(float delay, float duration, Action onComplete)
	{
		SetActive(true);
	}

	public void FadeOut(float delay, float duration, Action onComplete)
	{
		SetActive(false);
	}

	private IEnumerator DoFade(float delay, float duration, float start, float end, Action onComplete)
	{
		TweenAlpha.Begin(base.gameObject, 0f, start);
		yield return (object)new WaitForSeconds(delay);
		TweenAlpha.Begin(base.gameObject, duration, end);
		yield return (object)new WaitForSeconds(duration);
		onComplete?.Invoke();
	}
}
