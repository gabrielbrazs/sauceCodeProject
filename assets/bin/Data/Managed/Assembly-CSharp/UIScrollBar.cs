using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/NGUI Scroll Bar")]
public class UIScrollBar : UISlider
{
	private enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded
	}

	[SerializeField]
	[HideInInspector]
	protected float mSize = 1f;

	[HideInInspector]
	[SerializeField]
	private float mScroll;

	[SerializeField]
	[HideInInspector]
	private Direction mDir = Direction.Upgraded;

	[Obsolete("Use 'value' instead")]
	public float scrollValue
	{
		get
		{
			return base.value;
		}
		set
		{
			base.value = value;
		}
	}

	public float barSize
	{
		get
		{
			return mSize;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mSize != num)
			{
				mSize = num;
				mIsDirty = true;
				if (NGUITools.GetActive(this))
				{
					if ((UnityEngine.Object)UIProgressBar.current == (UnityEngine.Object)null && onChange != null)
					{
						UIProgressBar.current = this;
						EventDelegate.Execute(onChange);
						UIProgressBar.current = null;
					}
					ForceUpdate();
				}
			}
		}
	}

	protected override void Upgrade()
	{
		if (mDir != Direction.Upgraded)
		{
			mValue = mScroll;
			if (mDir == Direction.Horizontal)
			{
				mFill = (mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight);
			}
			else
			{
				mFill = ((!mInverted) ? FillDirection.TopToBottom : FillDirection.BottomToTop);
			}
			mDir = Direction.Upgraded;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((UnityEngine.Object)mFG != (UnityEngine.Object)null && (UnityEngine.Object)mFG.gameObject != (UnityEngine.Object)base.gameObject && ((UnityEngine.Object)mFG.GetComponent<Collider>() != (UnityEngine.Object)null || (UnityEngine.Object)mFG.GetComponent<Collider2D>() != (UnityEngine.Object)null))
		{
			UIEventListener uIEventListener = UIEventListener.Get(mFG.gameObject);
			UIEventListener uIEventListener2 = uIEventListener;
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(base.OnPressForeground));
			UIEventListener uIEventListener3 = uIEventListener;
			uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, new UIEventListener.VectorDelegate(base.OnDragForeground));
			mFG.autoResizeBoxCollider = true;
		}
	}

	protected override float LocalToValue(Vector2 localPos)
	{
		if ((UnityEngine.Object)mFG != (UnityEngine.Object)null)
		{
			float num = Mathf.Clamp01(mSize) * 0.5f;
			float t = num;
			float t2 = 1f - num;
			Vector3[] localCorners = mFG.localCorners;
			if (base.isHorizontal)
			{
				t = Mathf.Lerp(localCorners[0].x, localCorners[2].x, t);
				t2 = Mathf.Lerp(localCorners[0].x, localCorners[2].x, t2);
				float num2 = t2 - t;
				if (num2 == 0f)
				{
					return base.value;
				}
				return (!base.isInverted) ? ((localPos.x - t) / num2) : ((t2 - localPos.x) / num2);
			}
			t = Mathf.Lerp(localCorners[0].y, localCorners[1].y, t);
			t2 = Mathf.Lerp(localCorners[3].y, localCorners[2].y, t2);
			float num3 = t2 - t;
			if (num3 == 0f)
			{
				return base.value;
			}
			return (!base.isInverted) ? ((localPos.y - t) / num3) : ((t2 - localPos.y) / num3);
		}
		return base.LocalToValue(localPos);
	}

	public override void ForceUpdate()
	{
		if ((UnityEngine.Object)mFG != (UnityEngine.Object)null)
		{
			mIsDirty = false;
			float num = Mathf.Clamp01(mSize) * 0.5f;
			float num2 = Mathf.Lerp(num, 1f - num, base.value);
			float num3 = num2 - num;
			float num4 = num2 + num;
			if (base.isHorizontal)
			{
				mFG.drawRegion = ((!base.isInverted) ? new Vector4(num3, 0f, num4, 1f) : new Vector4(1f - num4, 0f, 1f - num3, 1f));
			}
			else
			{
				mFG.drawRegion = ((!base.isInverted) ? new Vector4(0f, num3, 1f, num4) : new Vector4(0f, 1f - num4, 1f, 1f - num3));
			}
			if ((UnityEngine.Object)thumb != (UnityEngine.Object)null)
			{
				Vector4 drawingDimensions = mFG.drawingDimensions;
				Vector3 position = new Vector3(Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, 0.5f), Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, 0.5f));
				SetThumbPosition(mFG.cachedTransform.TransformPoint(position));
			}
		}
		else
		{
			base.ForceUpdate();
		}
	}
}
