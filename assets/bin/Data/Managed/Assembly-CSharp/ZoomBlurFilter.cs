using System;
using System.Collections;
using UnityEngine;

public class ZoomBlurFilter : MonoBehaviour
{
	[SerializeField]
	private Material blurMaterial;

	[SerializeField]
	private Vector2 center;

	[SerializeField]
	private float blurPower;

	[SerializeField]
	private RenderTexture _cachedTexture;

	[SerializeField]
	private RenderTexture _filteredTexture;

	private bool chacheTarget;

	private Action onCompleteChecheTarget;

	private RenderTargetCacher cacher;

	private bool requestBlitFilterTexture;

	public RenderTexture filteredTexture
	{
		get
		{
			return _filteredTexture;
		}
		private set
		{
			filteredTexture = value;
		}
	}

	public void SetBlurPram(float _power, Vector2 _center)
	{
		blurPower = _power;
		center = _center;
	}

	public void CacheRenderTarget(Action onComplete, bool reqWithFilter = false)
	{
		chacheTarget = true;
		onCompleteChecheTarget = onComplete;
		GameObject gameObject = MonoBehaviourSingleton<UIManager>.I.uiCamera.gameObject;
		cacher = gameObject.GetComponent<RenderTargetCacher>();
		if ((UnityEngine.Object)null == (UnityEngine.Object)cacher)
		{
			cacher = gameObject.AddComponent<RenderTargetCacher>();
		}
		requestBlitFilterTexture = reqWithFilter;
	}

	private void Awake()
	{
		blurMaterial = new Material(ResourceUtility.FindShader("mobile/Custom/ImageEffect/RadialBlurFilter"));
		Restore();
	}

	private void OnDestroy()
	{
		if ((UnityEngine.Object)_filteredTexture != (UnityEngine.Object)null)
		{
			RenderTexture.ReleaseTemporary(_filteredTexture);
			_filteredTexture = null;
		}
		if ((UnityEngine.Object)blurMaterial != (UnityEngine.Object)null)
		{
			UnityEngine.Object.Destroy(blurMaterial);
			blurMaterial = null;
		}
		if ((UnityEngine.Object)null != (UnityEngine.Object)cacher)
		{
			UnityEngine.Object.Destroy(cacher);
			cacher = null;
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (chacheTarget && (UnityEngine.Object)null != (UnityEngine.Object)cacher)
		{
			Graphics.Blit(cacher.GetTexture(), _cachedTexture);
			Graphics.Blit(src, dst);
			chacheTarget = false;
			UnityEngine.Object.Destroy(cacher);
			cacher = null;
			if (requestBlitFilterTexture)
			{
				requestBlitFilterTexture = false;
				Graphics.Blit(_cachedTexture, filteredTexture);
			}
			if (onCompleteChecheTarget != null)
			{
				onCompleteChecheTarget();
				onCompleteChecheTarget = null;
			}
		}
		else if ((UnityEngine.Object)blurMaterial == (UnityEngine.Object)null || blurPower <= 0.01f)
		{
			Graphics.Blit(src, dst);
		}
		else
		{
			blurMaterial.SetVector("_Origin", new Vector4(center.x, center.y, 0f, 0f));
			blurMaterial.SetFloat("_Power", blurPower);
			_filteredTexture.DiscardContents(true, true);
			if ((UnityEngine.Object)_cachedTexture != (UnityEngine.Object)null)
			{
				Graphics.Blit(_cachedTexture, _filteredTexture, blurMaterial);
			}
			else
			{
				Graphics.Blit(src, _filteredTexture, blurMaterial);
			}
			Graphics.Blit(src, dst);
		}
	}

	public void Restore()
	{
		RenderTextureFormat format = RenderTextureFormat.RGB565;
		_filteredTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, format);
		_cachedTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, format);
	}

	public void StartBlurFilter(float powerStart, float powerEnd, float duration, Vector2 blurCenter, Action onComplete)
	{
		StartCoroutine(BlurFilterImpl(powerStart, powerEnd, duration, blurCenter, onComplete));
	}

	private IEnumerator BlurFilterImpl(float powerStart, float powerEnd, float duration, Vector2 blurCenter, Action onComplete)
	{
		float timer = 0f;
		while (timer < duration)
		{
			timer += Time.deltaTime;
			float currentPower = Mathf.Lerp(powerStart, powerEnd, timer / duration);
			SetBlurPram(currentPower, blurCenter);
			yield return (object)null;
		}
		onComplete?.Invoke();
	}
}
