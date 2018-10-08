using System;
using UnityEngine;

[AddComponentMenu("ProjectUI/UIButtonTweenEventCtrl")]
[RequireComponent(typeof(UIGameSceneEventSender))]
public class UIButtonTweenEventCtrl : UITweenCtrl
{
	public UITweener[] pushTweens;

	private bool isEnd;

	private void OnValidate()
	{
		if (tweens != null && tweens.Length > 0)
		{
			Array.ForEach(tweens, delegate(UITweener t)
			{
				if ((UnityEngine.Object)t != (UnityEngine.Object)null)
				{
					base._TweenReset(t);
					t.enabled = false;
				}
			});
		}
		if (pushTweens != null && pushTweens.Length > 0)
		{
			Array.ForEach(pushTweens, delegate(UITweener t)
			{
				if ((UnityEngine.Object)t != (UnityEngine.Object)null)
				{
					base._TweenReset(t);
					t.enabled = false;
				}
			});
		}
	}

	private void OnEnable()
	{
		OnValidate();
	}

	private void Strat()
	{
		UIGameSceneEventSender uIGameSceneEventSender = base.gameObject.GetComponent<UIGameSceneEventSender>();
		if ((UnityEngine.Object)uIGameSceneEventSender == (UnityEngine.Object)null)
		{
			uIGameSceneEventSender = base.gameObject.AddComponent<UIGameSceneEventSender>();
		}
		if (string.IsNullOrEmpty(uIGameSceneEventSender.eventName))
		{
			uIGameSceneEventSender.eventName = "NONE";
		}
	}

	public void PlayPush(bool isDown)
	{
		if (isDown)
		{
			isEnd = false;
			_Reset(pushTweens);
			isPlaying = false;
			_Play(pushTweens, isDown, null);
		}
		else
		{
			End(pushTweens);
		}
	}

	protected override void _TweenPlay(UITweener target, bool forward)
	{
		if (target.style != 0)
		{
			if (forward)
			{
				target.Play(forward);
			}
			else
			{
				_TweenReset(target);
			}
		}
		else
		{
			target.Play(forward);
		}
	}

	protected void OnDragOut()
	{
		if (isPlaying)
		{
			End(pushTweens);
		}
	}

	private void End(UITweener[] target_tweens)
	{
		if (target_tweens != null && target_tweens.Length != 0 && !isEnd)
		{
			isEnd = true;
			int i = 0;
			for (int num = target_tweens.Length; i < num; i++)
			{
				if (!((UnityEngine.Object)target_tweens[i] == (UnityEngine.Object)null))
				{
					UITweener.Style style = target_tweens[i].style;
					target_tweens[i].style = UITweener.Style.Once;
					target_tweens[i].tweenFactor = 1f;
					target_tweens[i].Sample(target_tweens[i].tweenFactor, true);
					target_tweens[i].PlayForward();
					target_tweens[i].style = style;
				}
			}
		}
	}
}
