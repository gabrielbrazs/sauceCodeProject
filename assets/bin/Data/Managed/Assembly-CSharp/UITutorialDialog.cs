using System;
using UnityEngine;

public class UITutorialDialog : MonoBehaviour
{
	private const int oneLine = 0;

	private const int twoLine = 1;

	private const int threeLine = 2;

	private const int threeLineLabel = 3;

	private const int threeLineLabel2 = 4;

	[SerializeField]
	private UIPanel[] root;

	[SerializeField]
	private UISprite[] messageLine0;

	[SerializeField]
	private UISprite[] messageLine1;

	[SerializeField]
	private UISprite[] messageLine2;

	[SerializeField]
	private UIAtlas[] atlases;

	public UILabel lbGreeting;

	public UILabel lbChargeWaypoint;

	public void Open(int atlasIndex0, string spriteName0)
	{
		root[1].gameObject.SetActive(value: false);
		root[2].gameObject.SetActive(value: false);
		if (!root[0].gameObject.activeInHierarchy)
		{
			root[0].gameObject.SetActive(value: true);
		}
		messageLine0[0].atlas = atlases[atlasIndex0];
		messageLine0[0].spriteName = spriteName0;
		root[0].alpha = 0f;
		TweenAlpha.Begin(root[0].gameObject, 0.3f, 1f);
	}

	public void Open(int atlasIndex0, string spriteName0, int atlasIndex1, string spriteName1)
	{
		root[0].gameObject.SetActive(value: false);
		root[2].gameObject.SetActive(value: false);
		if (!root[1].gameObject.activeInHierarchy)
		{
			root[1].gameObject.SetActive(value: true);
		}
		messageLine0[1].atlas = atlases[atlasIndex0];
		messageLine0[1].spriteName = spriteName0;
		messageLine1[1].atlas = atlases[atlasIndex1];
		messageLine1[1].spriteName = spriteName1;
		root[1].alpha = 0f;
		TweenAlpha.Begin(root[1].gameObject, 0.3f, 1f);
	}

	public void Open(int atlasIndex0, string spriteName0, int atlasIndex1, string spriteName1, int atlasIndex2, string spriteName2)
	{
		root[0].gameObject.SetActive(value: false);
		root[1].gameObject.SetActive(value: false);
		if (!root[2].gameObject.activeInHierarchy)
		{
			root[2].gameObject.SetActive(value: true);
		}
		messageLine0[2].atlas = atlases[atlasIndex0];
		messageLine0[2].spriteName = spriteName0;
		messageLine1[2].atlas = atlases[atlasIndex1];
		messageLine1[2].spriteName = spriteName1;
		messageLine2[2].atlas = atlases[atlasIndex2];
		messageLine2[2].spriteName = spriteName2;
		root[2].alpha = 0f;
		TweenAlpha.Begin(root[2].gameObject, 0.3f, 1f);
	}

	public void OpenThreeLineLabel()
	{
		if (root[1].gameObject.activeInHierarchy)
		{
			TweenAlpha.Begin(root[1].gameObject, 0f, 0f);
		}
		if (!root[3].gameObject.activeInHierarchy)
		{
			root[3].gameObject.SetActive(value: true);
		}
		lbGreeting.supportEncoding = true;
		root[3].alpha = 0f;
		TweenAlpha.Begin(root[3].gameObject, 0.3f, 1f);
	}

	public void HideThreeLineLabel()
	{
		TweenAlpha.Begin(root[3].gameObject, 0.3f, 0f);
	}

	public void OpenThreeLineLabel2()
	{
		if (!root[4].gameObject.activeInHierarchy)
		{
			root[4].gameObject.SetActive(value: true);
		}
		lbChargeWaypoint.supportEncoding = true;
		root[4].alpha = 1f;
		TweenAlpha.Begin(root[4].gameObject, 3f, 1f).AddOnFinished(delegate
		{
			root[4].gameObject.SetActive(value: false);
		});
	}

	public bool isThreeLineLabel2Active()
	{
		return root[4].isActiveAndEnabled;
	}

	public bool isTwoLineGameObjectActive()
	{
		return root[1].gameObject.activeSelf;
	}

	public void HideThreeLineLabel2()
	{
		root[4].gameObject.SetActive(value: false);
	}

	public void Close(int lineIndex = 0, Action onClose = null)
	{
		TweenAlpha ta = TweenAlpha.Begin(root[lineIndex].gameObject, 0.3f, 0f);
		if (onClose != null)
		{
			ta.AddOnFinished(delegate
			{
				UnityEngine.Object.DestroyImmediate(ta);
				if (onClose != null)
				{
					onClose();
				}
			});
		}
	}

	public void CloseaLLImmediately(int lineIndex = 0)
	{
		root[lineIndex].gameObject.SetActive(value: false);
	}
}
