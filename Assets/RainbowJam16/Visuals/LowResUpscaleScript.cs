using UnityEngine;

[ExecuteInEditMode]
public class LowResUpscaleScript : MonoBehaviour
{
	public int Width = 720;

	private int Height;

	void Update()
	{
		float ratio = ( (float) Camera.main.pixelHeight / (float) Camera.main.pixelWidth );
		Height = Mathf.RoundToInt( Width * ratio );
	}

	void OnRenderImage( RenderTexture source, RenderTexture dest )
	{
		source.filterMode = FilterMode.Point;

		RenderTexture buffer = RenderTexture.GetTemporary( Width, Height, -1 );
		{
			buffer.filterMode = FilterMode.Point;

			Graphics.Blit( source, buffer );
			Graphics.Blit( buffer, dest );
		}
		RenderTexture.ReleaseTemporary( buffer );
	}
}