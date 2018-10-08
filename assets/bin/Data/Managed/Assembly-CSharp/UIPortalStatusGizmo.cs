using UnityEngine;

public class UIPortalStatusGizmo : UIStatusGizmoBase
{
	[SerializeField]
	protected GameObject arrow;

	[SerializeField]
	protected UISprite statusSprite;

	[SerializeField]
	protected Vector3 offset;

	[SerializeField]
	[Tooltip("スクリ\u30fcン横オフセット")]
	protected float screenSideOffset = 22f;

	[SerializeField]
	[Tooltip("スクリ\u30fcン下オフセット")]
	protected float screenBottomOffset = 112f;

	[SerializeField]
	protected UITweenCtrl addTween;

	[SerializeField]
	protected UITweenCtrl fullTween;

	private PortalObject _portal;

	protected Transform portalTransform;

	protected Transform arrowTransform;

	public PortalObject portal
	{
		get
		{
			return _portal;
		}
		set
		{
			if ((Object)_portal != (Object)null)
			{
				_portal.uiGizmo = null;
			}
			_portal = value;
			if ((Object)_portal != (Object)null)
			{
				_portal.uiGizmo = this;
				base.gameObject.SetActive(true);
				portalTransform = value.transform;
				UpdateParam();
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if ((Object)arrow != (Object)null)
		{
			arrowTransform = arrow.transform;
		}
	}

	protected override void UpdateParam()
	{
		if ((Object)portal == (Object)null || !portal.gameObject.activeSelf)
		{
			SetActiveSafe(statusSprite.gameObject, false);
			SetActiveSafe(arrow, false);
			SetActiveSafe(addTween.gameObject, false);
			SetActiveSafe(fullTween.gameObject, false);
		}
		else
		{
			Vector3 screenUIPosition = Utility.GetScreenUIPosition(MonoBehaviourSingleton<AppMain>.I.mainCamera, MonoBehaviourSingleton<InGameCameraManager>.I.cameraTransform, portalTransform.position + offset);
			screenZ = screenUIPosition.z;
			screenUIPosition.z = 0f;
			float num = 1f / MonoBehaviourSingleton<UIManager>.I.uiRoot.pixelSizeAdjustment;
			Vector3 a = screenUIPosition;
			bool flag = false;
			float num2 = (float)Screen.width;
			if (screenUIPosition.x < screenSideOffset * num)
			{
				screenUIPosition.x = screenSideOffset * num;
				flag = true;
			}
			else if (screenUIPosition.x > num2 - screenSideOffset * num)
			{
				screenUIPosition.x = num2 - screenSideOffset * num;
				flag = true;
			}
			if (screenUIPosition.y < screenBottomOffset * num)
			{
				screenUIPosition.y = screenBottomOffset * num;
				flag = true;
			}
			if (flag)
			{
				SetActiveSafe(statusSprite.gameObject, true);
				SetActiveSafe(arrow, true);
				Vector3 vector = MonoBehaviourSingleton<UIManager>.I.uiCamera.ScreenToWorldPoint(screenUIPosition);
				if ((transform.position - vector).sqrMagnitude >= 2E-05f)
				{
					transform.position = vector;
				}
				if ((Object)arrowTransform != (Object)null)
				{
					Vector3 vector2 = a - screenUIPosition;
					if (vector2 != Vector3.zero)
					{
						float z = 90f - Vector3.Angle(Vector3.right, vector2);
						arrowTransform.eulerAngles = new Vector3(0f, 0f, z);
					}
					else
					{
						arrowTransform.eulerAngles = new Vector3(0f, 0f, 0f);
					}
				}
				bool flag2 = false;
				if ((Object)statusSprite != (Object)null)
				{
					switch (portal.viewType)
					{
					case PortalObject.VIEW_TYPE.NORMAL:
						statusSprite.spriteName = "Ingame_portal_vitalsign_blue";
						break;
					case PortalObject.VIEW_TYPE.NOT_TRAVELED:
						if (portal.isFull)
						{
							flag2 = true;
							statusSprite.spriteName = "Ingame_portal_vitalsign_red";
						}
						else
						{
							float num3 = (float)portal.nowPoint / (float)portal.maxPoint;
							int num4 = (int)(num3 * (float)MonoBehaviourSingleton<InGameSettingsManager>.I.portal.pointRankNum);
							if (num4 >= MonoBehaviourSingleton<InGameSettingsManager>.I.portal.pointRankNum - 1)
							{
								num4 = MonoBehaviourSingleton<InGameSettingsManager>.I.portal.pointRankNum - 2;
							}
							statusSprite.spriteName = string.Format("Ingame_portal_vitalsign_red_{0}", num4.ToString("D2"));
						}
						break;
					case PortalObject.VIEW_TYPE.TO_HOME:
						statusSprite.spriteName = "Ingame_portal_vitalsign_green";
						break;
					case PortalObject.VIEW_TYPE.TO_HARD_MAP:
						statusSprite.spriteName = "Ingame_portal_vitalsign_purple";
						break;
					case PortalObject.VIEW_TYPE.NOT_CLEAR_ORDER:
						statusSprite.spriteName = "Ingame_portal_vitalsign_not_clear_order";
						break;
					}
				}
				if (flag2)
				{
					SetActiveSafe(fullTween.gameObject, true);
					fullTween.Play(true, null);
				}
				else
				{
					SetActiveSafe(fullTween.gameObject, false);
				}
			}
			else
			{
				SetActiveSafe(statusSprite.gameObject, false);
				SetActiveSafe(arrow, false);
				SetActiveSafe(addTween.gameObject, false);
				SetActiveSafe(fullTween.gameObject, false);
			}
		}
	}

	public void OnGetPortalPoint()
	{
		SetActiveSafe(addTween.gameObject, true);
		addTween.Reset();
		addTween.Play(true, delegate
		{
			SetActiveSafe(fullTween.gameObject, false);
		});
	}
}
