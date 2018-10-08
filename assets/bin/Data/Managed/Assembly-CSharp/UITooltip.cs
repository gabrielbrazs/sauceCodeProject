using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Tooltip")]
public class UITooltip : MonoBehaviour
{
	protected static UITooltip mInstance;

	public Camera uiCamera;

	public UILabel text;

	public UISprite background;

	public float appearSpeed = 10f;

	public bool scalingTransitions = true;

	protected GameObject mTooltip;

	protected Transform mTrans;

	protected float mTarget;

	protected float mCurrent;

	protected Vector3 mPos;

	protected Vector3 mSize = Vector3.zero;

	protected UIWidget[] mWidgets;

	public static bool isVisible => (UnityEngine.Object)mInstance != (UnityEngine.Object)null && mInstance.mTarget == 1f;

	private void Awake()
	{
		mInstance = this;
	}

	private void OnDestroy()
	{
		mInstance = null;
	}

	protected virtual void Start()
	{
		mTrans = base.transform;
		mWidgets = GetComponentsInChildren<UIWidget>();
		mPos = mTrans.localPosition;
		if ((UnityEngine.Object)uiCamera == (UnityEngine.Object)null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		SetAlpha(0f);
	}

	protected virtual void Update()
	{
		if ((UnityEngine.Object)mTooltip != (UnityEngine.Object)UICamera.tooltipObject)
		{
			mTooltip = null;
			mTarget = 0f;
		}
		if (mCurrent != mTarget)
		{
			mCurrent = Mathf.Lerp(mCurrent, mTarget, RealTime.deltaTime * appearSpeed);
			if (Mathf.Abs(mCurrent - mTarget) < 0.001f)
			{
				mCurrent = mTarget;
			}
			SetAlpha(mCurrent * mCurrent);
			if (scalingTransitions)
			{
				Vector3 b = mSize * 0.25f;
				b.y = 0f - b.y;
				Vector3 localScale = Vector3.one * (1.5f - mCurrent * 0.5f);
				Vector3 localPosition = Vector3.Lerp(mPos - b, mPos, mCurrent);
				mTrans.localPosition = localPosition;
				mTrans.localScale = localScale;
			}
		}
	}

	protected virtual void SetAlpha(float val)
	{
		int i = 0;
		for (int num = mWidgets.Length; i < num; i++)
		{
			UIWidget uIWidget = mWidgets[i];
			Color color = uIWidget.color;
			color.a = val;
			uIWidget.color = color;
		}
	}

	protected virtual void SetText(string tooltipText)
	{
		if ((UnityEngine.Object)text != (UnityEngine.Object)null && !string.IsNullOrEmpty(tooltipText))
		{
			mTarget = 1f;
			mTooltip = UICamera.tooltipObject;
			text.text = tooltipText;
			mPos = UICamera.lastEventPosition;
			Transform transform = text.transform;
			Vector3 localPosition = transform.localPosition;
			Vector3 localScale = transform.localScale;
			mSize = text.printedSize;
			mSize.x *= localScale.x;
			mSize.y *= localScale.y;
			if ((UnityEngine.Object)background != (UnityEngine.Object)null)
			{
				Vector4 border = background.border;
				mSize.x += border.x + border.z + (localPosition.x - border.x) * 2f;
				mSize.y += border.y + border.w + (0f - localPosition.y - border.y) * 2f;
				background.width = Mathf.RoundToInt(mSize.x);
				background.height = Mathf.RoundToInt(mSize.y);
			}
			if ((UnityEngine.Object)uiCamera != (UnityEngine.Object)null)
			{
				mPos.x = Mathf.Clamp01(mPos.x / (float)Screen.width);
				mPos.y = Mathf.Clamp01(mPos.y / (float)Screen.height);
				float orthographicSize = uiCamera.orthographicSize;
				Vector3 lossyScale = mTrans.parent.lossyScale;
				float num = orthographicSize / lossyScale.y;
				float num2 = (float)Screen.height * 0.5f / num;
				Vector2 vector = new Vector2(num2 * mSize.x / (float)Screen.width, num2 * mSize.y / (float)Screen.height);
				mPos.x = Mathf.Min(mPos.x, 1f - vector.x);
				mPos.y = Mathf.Max(mPos.y, vector.y);
				mTrans.position = uiCamera.ViewportToWorldPoint(mPos);
				mPos = mTrans.localPosition;
				mPos.x = Mathf.Round(mPos.x);
				mPos.y = Mathf.Round(mPos.y);
				mTrans.localPosition = mPos;
			}
			else
			{
				if (mPos.x + mSize.x > (float)Screen.width)
				{
					mPos.x = (float)Screen.width - mSize.x;
				}
				if (mPos.y - mSize.y < 0f)
				{
					mPos.y = mSize.y;
				}
				mPos.x -= (float)Screen.width * 0.5f;
				mPos.y -= (float)Screen.height * 0.5f;
			}
		}
		else
		{
			mTooltip = null;
			mTarget = 0f;
		}
	}

	[Obsolete("Use UITooltip.Show instead")]
	public static void ShowText(string text)
	{
		if ((UnityEngine.Object)mInstance != (UnityEngine.Object)null)
		{
			mInstance.SetText(text);
		}
	}

	public static void Show(string text)
	{
		if ((UnityEngine.Object)mInstance != (UnityEngine.Object)null)
		{
			mInstance.SetText(text);
		}
	}

	public static void Hide()
	{
		if ((UnityEngine.Object)mInstance != (UnityEngine.Object)null)
		{
			mInstance.mTooltip = null;
			mInstance.mTarget = 0f;
		}
	}
}
