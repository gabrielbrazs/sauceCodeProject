using UnityEngine;

public class AnimEventTools : MonoBehaviour
{
	public enum TOOL_MODE
	{
		EVENT_ID_CHECK,
		EVENT_ID_DELETE
	}

	public TOOL_MODE toolMode;

	public bool targetAll = true;

	public AnimEventData animEventData;

	public AnimEventFormat.ID eventID;
}
