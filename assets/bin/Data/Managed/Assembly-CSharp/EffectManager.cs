using rhyme;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviourSingleton<EffectManager>
{
	private class Pool_OneShotInfo : rymTPool<OneShotInfo>
	{
	}

	public class OneShotInfo
	{
		public string name;

		public Vector3 pos;

		public Quaternion rot;

		public Vector3 scale;

		public float time;

		public Action<Transform> onCreateCallBack;
	}

	private List<OneShotInfo> infoList = new List<OneShotInfo>();

	private List<OneShotInfo> infoSecondList = new List<OneShotInfo>();

	public bool enableStock;

	public int maxStockCount = 64;

	private Transform stockParent;

	public static void ClearPoolObjects()
	{
		if (MonoBehaviourSingleton<EffectManager>.IsValid())
		{
			MonoBehaviourSingleton<EffectManager>.I.ClearStocks();
		}
	}

	public static void Startup()
	{
		EeLSettings.Startup();
	}

	private void Start()
	{
		ClearStocks();
	}

	private void OnEnable()
	{
		Trail.onQueryDestroy = (Func<Trail, bool>)Delegate.Combine(Trail.onQueryDestroy, new Func<Trail, bool>(OnTrailQueryDestroy));
	}

	protected override void OnDisable()
	{
		int i = 0;
		for (int count = infoList.Count; i < count; i++)
		{
			OneShotInfo obj = infoList[i];
			rymTPool<OneShotInfo>.Release(ref obj);
		}
		infoList.Clear();
		int j = 0;
		for (int count2 = infoSecondList.Count; j < count2; j++)
		{
			OneShotInfo obj2 = infoSecondList[j];
			rymTPool<OneShotInfo>.Release(ref obj2);
		}
		infoSecondList.Clear();
		base.OnDisable();
		Trail.onQueryDestroy = (Func<Trail, bool>)Delegate.Remove(Trail.onQueryDestroy, new Func<Trail, bool>(OnTrailQueryDestroy));
	}

	private bool OnTrailQueryDestroy(Trail trail)
	{
		if (StockOrDestroy(trail.gameObject, no_stock_to_destroy: false))
		{
			return false;
		}
		return true;
	}

	private void LateUpdate()
	{
		if (infoList.Count > 0)
		{
			OneShotInfo obj = infoList[0];
			_OneShot(obj.name, obj.pos, obj.rot, obj.scale, obj.onCreateCallBack);
			rymTPool<OneShotInfo>.Release(ref obj);
			infoList.RemoveAt(0);
			return;
		}
		int count = infoSecondList.Count;
		if (count <= 0)
		{
			return;
		}
		float time = Time.time;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			OneShotInfo oneShotInfo = infoSecondList[i];
			if (time - oneShotInfo.time > 0.1f)
			{
				num++;
				continue;
			}
			_OneShot(oneShotInfo.name, oneShotInfo.pos, oneShotInfo.rot, oneShotInfo.scale, oneShotInfo.onCreateCallBack);
			num++;
			break;
		}
		for (int j = 0; j < num; j++)
		{
			OneShotInfo obj2 = infoSecondList[j];
			rymTPool<OneShotInfo>.Release(ref obj2);
		}
		infoSecondList.RemoveRange(0, num);
	}

	public bool StockOrDestroy(GameObject go, bool no_stock_to_destroy)
	{
		if (go == null)
		{
			return false;
		}
		if (enableStock)
		{
			EffectStock component = go.GetComponent<EffectStock>();
			if (component != null && !component.IsLoop())
			{
				component.Stock();
				go.transform.SetParent(stockParent, worldPositionStays: false);
				if (stockParent.childCount >= maxStockCount)
				{
					UnityEngine.Object.DestroyImmediate(stockParent.GetChild(0).gameObject);
				}
				return true;
			}
		}
		if (no_stock_to_destroy)
		{
			UnityEngine.Object.Destroy(go);
		}
		return false;
	}

	public void ClearStocks()
	{
		if (stockParent != null)
		{
			UnityEngine.Object.DestroyImmediate(stockParent.gameObject);
		}
		stockParent = Utility.CreateGameObject("Stocks", base._transform);
		stockParent.gameObject.SetActive(value: false);
	}

	public static Transform GetEffect(string effect_name, Transform parent = null)
	{
		return GetEffect(RESOURCE_CATEGORY.EFFECT_ACTION, effect_name, parent);
	}

	public static bool ExistEffect(string effect_name)
	{
		if (string.IsNullOrEmpty(effect_name))
		{
			return false;
		}
		if (!MonoBehaviourSingleton<EffectManager>.IsValid() || !MonoBehaviourSingleton<ResourceManager>.IsValid())
		{
			return false;
		}
		effect_name = ResourceName.AddAttributID(effect_name);
		return MonoBehaviourSingleton<ResourceManager>.I.IsCached(RESOURCE_CATEGORY.EFFECT_ACTION, effect_name);
	}

	public static Transform GetCameraLinkEffect(string effect_name, bool y0, Transform parent = null)
	{
		Transform effect = GetEffect(RESOURCE_CATEGORY.EFFECT_ACTION, effect_name, parent);
		if (effect == null)
		{
			return null;
		}
		CameraPosLink cameraPosLink = effect.gameObject.AddComponent<CameraPosLink>();
		if (cameraPosLink != null)
		{
			cameraPosLink.y0 = y0;
			EffectInfoComponent component = effect.gameObject.GetComponent<EffectInfoComponent>();
			if (component != null)
			{
				cameraPosLink.cameraOffsetZ = component.CameraPosLinkOffsetZ;
			}
		}
		return effect;
	}

	public static Transform GetUIEffect(string effect_name)
	{
		return GetUIEffect(effect_name, null, -0.001f, 0, null);
	}

	public static Transform GetUIEffect(string effect_name, UIWidget widget, float z = -0.001f, int add_render_queue = 0)
	{
		return GetUIEffect(effect_name, widget.transform, z, add_render_queue);
	}

	public static Transform GetUIEffect(string effect_name, Transform parent, float z = -0.001f, int add_render_queue = 0, UIWidget ref_render_queue = null)
	{
		if (parent == null)
		{
			parent = MonoBehaviourSingleton<GameSceneManager>.I.GetLastSectionExcludeCommonDialog()._transform;
		}
		Transform effect = GetEffect(RESOURCE_CATEGORY.EFFECT_UI, effect_name, parent, 5);
		if (effect != null && add_render_queue != -1)
		{
			SetUIEffectDepth(effect, parent, z, add_render_queue, ref_render_queue);
		}
		return effect;
	}

	public static void SetUIEffectDepth(Transform effect, Transform parent, float z = -0.001f, int add_render_queue = 0, UIWidget ref_render_queue = null)
	{
		effect.localPosition += new Vector3(0f, 0f, z);
		if (ref_render_queue == null)
		{
			ref_render_queue = parent.GetComponentInChildren<UIWidget>();
		}
		rymFX fx = effect.GetComponent<rymFX>();
		if (fx != null)
		{
			fx.Cameras = MonoBehaviourSingleton<UIManager>.I.cameras;
			if (ref_render_queue != null)
			{
				UIWidget uIWidget = ref_render_queue;
				uIWidget.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(uIWidget.onRender, (UIDrawCall.OnRenderCallback)delegate(Material mate)
				{
					if (fx != null)
					{
						fx.SetRenderQueue(mate.renderQueue + add_render_queue);
					}
				});
			}
			else
			{
				fx.ChangeRenderQueue = 3000 + add_render_queue;
			}
		}
		else if (effect.GetComponent<EffectCtrl>() != null)
		{
			Renderer[] renderers = effect.GetComponentsInChildren<Renderer>();
			if (renderers.Length != 0)
			{
				UIWidget uIWidget2 = ref_render_queue;
				uIWidget2.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(uIWidget2.onRender, (UIDrawCall.OnRenderCallback)delegate(Material mate)
				{
					int renderQueue = mate.renderQueue + add_render_queue;
					int i = 0;
					for (int num = renderers.Length; i < num; i++)
					{
						Renderer renderer = renderers[i];
						if (renderer != null)
						{
							Material[] materials = renderer.materials;
							int j = 0;
							for (int num2 = materials.Length; j < num2; j++)
							{
								Material material = materials[j];
								if (material != null)
								{
									material.renderQueue = renderQueue;
								}
							}
						}
					}
				});
			}
		}
	}

	private static Transform GetEffect(RESOURCE_CATEGORY category, string effect_name, Transform parent = null, int layer = -1, bool enable_stock = false)
	{
		if (string.IsNullOrEmpty(effect_name))
		{
			return null;
		}
		if (MonoBehaviourSingleton<EffectManager>.IsValid())
		{
			EffectManager i = MonoBehaviourSingleton<EffectManager>.I;
			effect_name = ResourceName.AddAttributID(effect_name);
			if (MonoBehaviourSingleton<ResourceManager>.IsValid())
			{
				if (parent == null)
				{
					parent = i._transform;
				}
				GameObject gameObject = null;
				GameObject gameObject2 = null;
				Transform transform = null;
				bool flag = i.enableStock && enable_stock;
				if (flag)
				{
					Transform transform2 = i.stockParent.Find(effect_name);
					if (transform2 != null)
					{
						transform2.GetComponent<EffectStock>().Recycle(parent, layer);
						return transform2;
					}
				}
				gameObject2 = (GameObject)InstantiateManager.FindStock(category, effect_name);
				if (gameObject2 != null)
				{
					transform = InstantiateManager.Realizes(ref gameObject2, parent, layer);
					gameObject2 = transform.gameObject;
				}
				else
				{
					gameObject = ((!ResourceManager.enableLoadDirect) ? ((GameObject)MonoBehaviourSingleton<ResourceManager>.I.cache.GetCachedObject(category, effect_name)) : ((GameObject)MonoBehaviourSingleton<ResourceManager>.I.LoadDirect(category, effect_name)));
					if (gameObject != null)
					{
						transform = ResourceUtility.Realizes(gameObject, parent, layer);
						gameObject2 = transform.gameObject;
					}
				}
				if (gameObject2 != null)
				{
					if (flag)
					{
						gameObject2.AddComponent<EffectStock>();
					}
					return transform;
				}
			}
		}
		return null;
	}

	public void AddOneShotInfo(OneShotInfo info, bool is_priority)
	{
		if (is_priority)
		{
			infoList.Add(info);
		}
		else
		{
			infoSecondList.Add(info);
		}
	}

	public static void OneShot(string effect_name, Vector3 pos, Quaternion rot, bool is_priority = false)
	{
		OneShot(effect_name, pos, rot, Vector3.one, is_priority);
	}

	public static void OneShot(string effect_name, Vector3 pos, Quaternion rot, Vector3 scale, bool is_priority = false, Action<Transform> callback = null)
	{
		bool flag = false;
		if (MonoBehaviourSingleton<InGameManager>.I.graphicOptionType >= 2)
		{
			flag = true;
		}
		if (flag)
		{
			_OneShot(effect_name, pos, rot, scale, callback);
			return;
		}
		Vector3 vector = MonoBehaviourSingleton<AppMain>.I.mainCamera.WorldToViewportPoint(pos);
		if (!(vector.x < -0.5f) && !(vector.x > 1.5f) && !(vector.y < -0.5f) && !(vector.y > 1.5f) && !(vector.z < 0f))
		{
			if (MonoBehaviourSingleton<EffectManager>.IsValid())
			{
				OneShotInfo oneShotInfo = rymTPool<OneShotInfo>.Get();
				oneShotInfo.name = effect_name;
				oneShotInfo.pos = pos;
				oneShotInfo.rot = rot;
				oneShotInfo.scale = scale;
				oneShotInfo.time = Time.time;
				oneShotInfo.onCreateCallBack = callback;
				MonoBehaviourSingleton<EffectManager>.I.AddOneShotInfo(oneShotInfo, is_priority);
			}
			else
			{
				_OneShot(effect_name, pos, rot, scale, callback);
			}
		}
	}

	public static void _OneShot(string effect_name, Vector3 pos, Quaternion rot, Vector3 scale, Action<Transform> callback = null)
	{
		Transform effect = GetEffect(RESOURCE_CATEGORY.EFFECT_ACTION, effect_name, null, -1, enable_stock: true);
		if (!(effect == null))
		{
			effect.position = pos;
			effect.rotation = rot;
			effect.localScale = Vector3.Scale(effect.localScale, scale);
			callback?.Invoke(effect);
		}
	}

	public void DeleteManagerChildrenEffects()
	{
		infoList.Clear();
		base.gameObject.GetComponentsInChildren(Temporary.fxList);
		int i = 0;
		for (int count = Temporary.fxList.Count; i < count; i++)
		{
			UnityEngine.Object.Destroy(Temporary.fxList[i].gameObject);
		}
		Temporary.fxList.Clear();
		base.gameObject.GetComponentsInChildren(Temporary.effectCtrlList);
		int j = 0;
		for (int count2 = Temporary.effectCtrlList.Count; j < count2; j++)
		{
			UnityEngine.Object.Destroy(Temporary.effectCtrlList[j].gameObject);
		}
		Temporary.effectCtrlList.Clear();
	}

	public static void ReleaseEffect(GameObject effect_object, bool isPlayEndAnimation = true, bool immediate = false)
	{
		if (effect_object == null)
		{
			return;
		}
		if (!MonoBehaviourSingleton<EffectManager>.IsValid())
		{
			UnityEngine.Object.Destroy(effect_object);
			return;
		}
		EffectManager i = MonoBehaviourSingleton<EffectManager>.I;
		EffectInfoComponent component = effect_object.GetComponent<EffectInfoComponent>();
		if (component != null && component.destroyLoopEnd)
		{
			component.SetLoopAudioObject(null);
			rymFX component2 = effect_object.GetComponent<rymFX>();
			EffectCtrl effectCtrl = null;
			if (component2 == null)
			{
				effectCtrl = effect_object.GetComponent<EffectCtrl>();
			}
			if (effectCtrl == null && effect_object.transform.childCount > 0)
			{
				effect_object.GetComponentsInChildren(Temporary.rendererList);
				int j = 0;
				for (int count = Temporary.rendererList.Count; j < count; j++)
				{
					Temporary.rendererList[j].enabled = false;
				}
				Temporary.rendererList.Clear();
			}
			effect_object.GetComponents(Temporary.trailList);
			bool flag = false;
			if (component2 != null && component2.enabled)
			{
				component2.AutoDelete = true;
				component2.LoopEnd = true;
				flag = true;
			}
			else if (effectCtrl != null && effectCtrl.enabled)
			{
				effectCtrl.EndLoop(isPlayEndAnimation);
				flag = true;
			}
			if (flag && !immediate)
			{
				int k = 0;
				for (int count2 = Temporary.trailList.Count; k < count2; k++)
				{
					Temporary.trailList[k].StartDeleteFade();
				}
				Temporary.trailList.Clear();
				return;
			}
			i.StockOrDestroy(effect_object, no_stock_to_destroy: true);
			int l = 0;
			for (int count3 = Temporary.trailList.Count; l < count3; l++)
			{
				Temporary.trailList[l].SetAutoDelete();
			}
			Temporary.trailList.Clear();
		}
		else
		{
			i.StockOrDestroy(effect_object, no_stock_to_destroy: true);
		}
	}

	public static void ReleaseEffect(ref Transform t)
	{
		if (t != null)
		{
			ReleaseEffect(t.gameObject);
			t = null;
		}
	}
}
