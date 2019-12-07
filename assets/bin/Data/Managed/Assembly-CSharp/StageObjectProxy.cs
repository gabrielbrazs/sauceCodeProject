using UnityEngine;

public class StageObjectProxy : MonoBehaviour
{
	public StageObject stageObject;

	private void OnAnimatorMove()
	{
		if (stageObject != null)
		{
			stageObject.OnAnimatorMove();
		}
	}
}
