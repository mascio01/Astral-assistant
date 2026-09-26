using System;
using System.Text;
using UnityEngine;

public class CustomBinaryReaderFromMemory : CustomBinaryReader
{
	public static bool IsSerialisingStoryIds;

	public CustomBinaryReaderFromMemory(byte[] buffer, int len)
	{
		_buffer = buffer;
		_bufferLength = len;
		TotalSize = _bufferLength;
		Progress = 0;
	}

	public void SetBuffer(byte[] buffer, int len)
	{
		_buffer = buffer;
		_bufferLength = len;
		TotalSize = _bufferLength;
		ResetIndex();
	}

	public void ResetIndex()
	{
		_index = 0;
		Progress = 0;
	}

	public override bool IsAtEnd()
	{
		return _index == _bufferLength;
	}

	public override int GetTotalLength()
	{
		return _bufferLength;
	}

	public override void FillBuffer()
	{
		if (!IsSerialisingStoryIds)
		{
			return;
		}
		OnlineParty.Instance.LeaveLobby();
		OnlineParty.Instance.ClearLobby();
		string text = ((LastStringRead != null) ? new string(LastStringRead) : string.Empty);
		StringBuilder stringBuilder = new StringBuilder(100);
		for (int i = 0; i < Math.Min(text.Length, 100); i++)
		{
			if (text[i] > '\u007f')
			{
				stringBuilder.Append('*');
			}
			else
			{
				stringBuilder.Append(text[i]);
			}
		}
		string text2 = "FillBuffer hit! Buffer: " + _buffer.Length + " Index: " + _index + " Last String: " + ((LastStringRead != null) ? LastStringRead.Length.ToString() : "null") + " " + stringBuilder.ToString();
		for (int j = 0; j < OnlineParty.Instance.Msg.Stories.Count; j++)
		{
			text2 += " (";
			text2 += OnlineParty.Instance.Msg.Stories[j].Folder.Substring(0, Math.Min(100, OnlineParty.Instance.Msg.Stories[j].Folder.Length));
			text2 += " ";
			text2 += OnlineParty.Instance.Msg.Stories[j].WorkshopId;
			text2 += ")";
		}
		Debug.LogError(text2);
	}

	public override void Add(ref byte[] bytes, ref int size)
	{
		Read(out size);
		bytes = new byte[size];
		Read(bytes, size);
	}
}
