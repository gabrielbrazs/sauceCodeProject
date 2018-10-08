using UnityEngine;

public class PostEffector : MonoBehaviour
{
	[SerializeField]
	private FilterBase filter;

	public void SetFilter(FilterBase filter)
	{
		this.filter = filter;
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if ((Object)filter == (Object)null)
		{
			Graphics.Blit(src, dest);
		}
		else
		{
			filter.PostEffectProc(src, dest);
		}
	}
}
