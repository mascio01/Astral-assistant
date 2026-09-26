using UnityEngine;

public class TexturePool
{
	public RenderTexture RenderTex;

	private TexturePoolSlot[] Slots;

	public TexturePool(int imageSize, int count)
	{
		RenderTex = new RenderTexture(imageSize, imageSize, 24, RenderTextureFormat.ARGB32);
		Slots = new TexturePoolSlot[count];
		for (int i = 0; i < count; i++)
		{
			Slots[i].Tex = new Texture2D(imageSize, imageSize, TextureFormat.ARGB32, mipChain: false);
			Slots[i].Tex.wrapMode = TextureWrapMode.Clamp;
			Slots[i].LastUsedFrame = -1000;
		}
	}

	public void Release()
	{
		RenderTex.Release();
	}

	public Texture2D GetTexture(int index, int objectId)
	{
		if (index >= 0 && Slots[index].ObjectId == objectId)
		{
			Slots[index].LastUsedFrame = Time.frameCount;
			return Slots[index].Tex;
		}
		return null;
	}

	public Texture2D FindSharedTexture(int objectId, out int index)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].ObjectId == objectId)
			{
				index = i;
				Slots[index].LastUsedFrame = Time.frameCount;
				return Slots[i].Tex;
			}
		}
		index = -1;
		return null;
	}

	public int AddTexture(int objectId)
	{
		int num = -1;
		int num2 = Time.frameCount - 1;
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].LastUsedFrame < num2)
			{
				num = i;
				num2 = Slots[i].LastUsedFrame;
			}
		}
		if (num != -1)
		{
			Slots[num].ObjectId = objectId;
			Slots[num].LastUsedFrame = Time.frameCount;
		}
		return num;
	}

	public void ReleaseRenderTarget(int index, int objectId)
	{
		if (index >= 0 && Slots[index].ObjectId == objectId)
		{
			Slots[index].LastUsedFrame = -1000;
			Slots[index].ObjectId = 0;
		}
	}

	public void Reset()
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			Slots[i].LastUsedFrame = -1000;
			Slots[i].ObjectId = 0;
		}
	}
}
