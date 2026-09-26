using UnityEngine;

public class Portrait
{
	public const int DefaultWidth = 1920;

	public const int DefaultHeight = 1080;

	public TileObject PortraitSubject;

	public PortraitPose Pose;

	public int LastUsedFrame;

	public bool Ready = true;

	public Texture2D Tex;

	public Portrait(int w, int h)
	{
		Tex = new Texture2D(w, h, TextureFormat.ARGB32, mipChain: false);
	}
}
