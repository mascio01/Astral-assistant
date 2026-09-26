using System;

public class VoiceChatPacket
{
	public int PacketId;

	public int Length;

	public byte[] Data;

	public float[] DecodedData;

	public void Decode()
	{
		DecodedData = new float[Length / 2];
		for (int i = 0; i < DecodedData.Length; i++)
		{
			float num = BitConverter.ToInt16(Data, i * 2);
			DecodedData[i] = num / 32767f;
		}
	}
}
