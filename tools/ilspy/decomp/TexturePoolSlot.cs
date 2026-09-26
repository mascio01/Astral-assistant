using UnityEngine;

internal struct TexturePoolSlot
{
	public Texture2D Tex;

	public int LastUsedFrame;

	public int ObjectId;
}
