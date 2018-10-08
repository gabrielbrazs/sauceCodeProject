using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Toggled Objects")]
public class UIToggledObjects
{
	public List<GameObject> activate;

	public List<GameObject> deactivate;

	[SerializeField]
	[HideInInspector]
	private GameObject target;

	[HideInInspector]
	[SerializeField]
	private bool inverse;

	public UIToggledObjects()
		: this()
	{
	}

	private void Awake()
	{
		if (target != null)
		{
			if (activate.Count == 0 && deactivate.Count == 0)
			{
				if (inverse)
				{
					deactivate.Add(target);
				}
				else
				{
					activate.Add(target);
				}
			}
			else
			{
				target = null;
			}
		}
		UIToggle component = this.GetComponent<UIToggle>();
		EventDelegate.Add(component.onChange, Toggle);
	}

	public void Toggle()
	{
		bool value = UIToggle.current.value;
		if (this.get_enabled())
		{
			for (int i = 0; i < activate.Count; i++)
			{
				Set(activate[i], value);
			}
			for (int j = 0; j < deactivate.Count; j++)
			{
				Set(deactivate[j], !value);
			}
		}
	}

	private void Set(GameObject go, bool state)
	{
		if (go != null)
		{
			NGUITools.SetActive(go, state);
		}
	}
}