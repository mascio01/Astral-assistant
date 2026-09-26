using System;
using System.IO;
using Steamworks;
using UnityEngine;

public class CustomBinaryReader : Reflector, IDisposable
{
	public byte[] _buffer;

	private char[] _char = new char[1];

	private short[] _short = new short[1];

	private ushort[] _ushort = new ushort[1];

	private int[] _int = new int[1];

	private uint[] _uint = new uint[1];

	private long[] _long = new long[1];

	private ulong[] _ulong = new ulong[1];

	private float[] _float = new float[1];

	private double[] _double = new double[1];

	public int _index;

	public int _bufferLength;

	private Stream _stream;

	public volatile int Progress;

	public volatile int TotalSize;

	public char[] LastStringRead;

	public string LastReadName;

	public override bool IsDeserialising => true;

	protected CustomBinaryReader()
	{
	}

	public CustomBinaryReader(Stream stream)
	{
		_buffer = new byte[65536];
		_stream = stream;
		TotalSize = (int)_stream.Length;
		Progress = 0;
	}

	public void Dispose()
	{
	}

	public bool ReadBool()
	{
		Read(out bool value);
		return value;
	}

	public byte ReadByte()
	{
		Read(out byte value);
		return value;
	}

	public sbyte ReadSByte()
	{
		Read(out sbyte value);
		return value;
	}

	public char ReadChar()
	{
		Read(out char value);
		return value;
	}

	public short ReadShort()
	{
		Read(out short value);
		return value;
	}

	public int ReadInt()
	{
		Read(out int value);
		return value;
	}

	public uint ReadUInt()
	{
		Read(out uint value);
		return value;
	}

	public long ReadLong()
	{
		Read(out long value);
		return value;
	}

	public float ReadFloat()
	{
		Read(out float value);
		return value;
	}

	public Vector3 ReadVector3()
	{
		Read(out Vector3 value);
		return value;
	}

	public TerrainCoord ReadTerrainCoord()
	{
		Read(out TerrainCoord value);
		return value;
	}

	public TimeSpan ReadTimeSpan()
	{
		Read(out TimeSpan value);
		return value;
	}

	public string ReadString()
	{
		Read(out string value);
		return value;
	}

	public BaseObject ReadBaseObject()
	{
		Read(out BaseObject value);
		return value;
	}

	public void Read(out bool value)
	{
		Read(out byte value2);
		value = ((value2 != 0) ? true : false);
	}

	public void Read(out byte value)
	{
		if (_index == _bufferLength)
		{
			FillBuffer();
		}
		value = _buffer[_index];
		_index++;
		Progress++;
	}

	public void Read(out sbyte value)
	{
		if (_index == _bufferLength)
		{
			FillBuffer();
		}
		value = (sbyte)_buffer[_index];
		_index++;
		Progress++;
	}

	public void Read(out char value)
	{
		Read(_char, 2);
		value = _char[0];
	}

	public void Read(out short value)
	{
		Read(_short, 2);
		value = _short[0];
	}

	public void Read(out ushort value)
	{
		Read(_ushort, 2);
		value = _ushort[0];
	}

	public void Read(out int value)
	{
		Read(_int, 4);
		value = _int[0];
	}

	public void Read(out uint value)
	{
		Read(_uint, 4);
		value = _uint[0];
	}

	public void Read(out long value)
	{
		Read(_long, 8);
		value = _long[0];
	}

	public void Read(out ulong value)
	{
		Read(_ulong, 8);
		value = _ulong[0];
	}

	public void Read(out float value)
	{
		Read(_float, 4);
		value = _float[0];
	}

	public void Read(out double value)
	{
		Read(_double, 8);
		value = _double[0];
	}

	public void Read(out Vector2 value)
	{
		Read(out float value2);
		Read(out float value3);
		value = new Vector2(value2, value3);
	}

	public void Read(out Vector3 value)
	{
		Read(out float value2);
		Read(out float value3);
		Read(out float value4);
		value = new Vector3(value2, value3, value4);
	}

	public void Read(out Half3 value)
	{
		Read(out ushort value2);
		Read(out ushort value3);
		Read(out ushort value4);
		value = new Half3(value2, value3, value4);
	}

	public void Read(out Vector4 value)
	{
		Read(out float value2);
		Read(out float value3);
		Read(out float value4);
		Read(out float value5);
		value = new Vector4(value2, value3, value4, value5);
	}

	public void Read(out Quaternion value)
	{
		Read(out float value2);
		Read(out float value3);
		Read(out float value4);
		Read(out float value5);
		value = new Quaternion(value2, value3, value4, value5);
	}

	public void Read(out Color32 value)
	{
		Read(out byte value2);
		Read(out byte value3);
		Read(out byte value4);
		Read(out byte value5);
		value = new Color32(value2, value3, value4, value5);
	}

	public void Read(out Color value)
	{
		Read(out float value2);
		Read(out float value3);
		Read(out float value4);
		Read(out float value5);
		value = new Color(value2, value3, value4, value5);
	}

	public void Read(ref Matrix4x4 value)
	{
		Read(out value.m00);
		Read(out value.m01);
		Read(out value.m02);
		Read(out value.m03);
		Read(out value.m10);
		Read(out value.m11);
		Read(out value.m12);
		Read(out value.m13);
		Read(out value.m20);
		Read(out value.m21);
		Read(out value.m22);
		Read(out value.m23);
		Read(out value.m30);
		Read(out value.m31);
		Read(out value.m32);
		Read(out value.m33);
	}

	public void Read(out TerrainCoord value)
	{
		Read(out int value2);
		Read(out int value3);
		value = new TerrainCoord(value2, value3);
	}

	public void Read(out TimeSpan value)
	{
		Read(out long value2);
		value = new TimeSpan(value2);
	}

	public void Read(out string value)
	{
		Read(out int value2);
		if (value2 < 0)
		{
			Debug.Log(value2);
			value = string.Empty;
			return;
		}
		try
		{
			char[] array = (LastStringRead = new char[value2]);
			for (int i = 0; i < value2; i++)
			{
				Read(out array[i]);
			}
			LastStringRead = null;
			value = new string(array);
		}
		catch (Exception message)
		{
			Debug.Log(message);
			value = string.Empty;
		}
	}

	public void Read(out BaseObject value)
	{
		Read(out int value2);
		value = ((value2 >= 0) ? BaseObjectManager.Instance.FindBaseObjectByID(value2) : null);
	}

	public void Read(out PlayerID value)
	{
		Read(out ulong value2);
		value = new PlayerID(new CSteamID(value2));
	}

	public void Read(out EquipmentPrototype value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindEquipmentPrototypeByName(value2) : null);
		LastReadName = value2;
	}

	public void Read(out LiquidPrototype value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindLiquidPrototypeByName(value2) : null);
		LastReadName = value2;
	}

	public void Read(out MemoryPrototype value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindMemoryPrototypeByUniqueID(value2) : null);
		LastReadName = value2;
	}

	public void Read(out Recipe value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindRecipeByUniqueID(value2) : null);
		LastReadName = value2;
	}

	public void Read(out PropPrototype value)
	{
		if (Version < 222)
		{
			Read(out string value2);
			value = ((value2.Length > 0) ? GameImpl.Instance.FindPropPrototypeByName(value2) : null);
			LastReadName = value2;
		}
		else
		{
			Read(out int value3);
			value = ((value3 != 0) ? GameImpl.Instance.FindPropPrototypeByNameHash(value3) : null);
		}
	}

	public void Read(out Trigger value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindTriggerByUniqueID(value2) : null);
		LastReadName = value2;
	}

	public void Read(out Speech value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindSpeechByUniqueID(value2) : null);
		LastReadName = value2;
	}

	public void Read(out Quest value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindQuestByUniqueID(value2) : null);
		LastReadName = value2;
	}

	public void Read(out Invader value)
	{
		Read(out string value2);
		value = ((value2.Length > 0) ? GameImpl.Instance.FindInvaderByUniqueID(value2) : null);
		LastReadName = value2;
	}

	public void Read(Array dst, int size)
	{
		int num = 0;
		if (_index + size > _bufferLength)
		{
			FillBuffer();
			while (_index + size > _bufferLength)
			{
				Buffer.BlockCopy(_buffer, _index, dst, num, _bufferLength);
				_index += _bufferLength;
				num += _bufferLength;
				size -= _bufferLength;
				Progress += _bufferLength;
				FillBuffer();
			}
		}
		Buffer.BlockCopy(_buffer, _index, dst, num, size);
		_index += size;
		Progress += size;
	}

	public virtual void FillBuffer()
	{
		int num = _bufferLength - _index;
		if (num > 0)
		{
			Buffer.BlockCopy(_buffer, _index, _buffer, 0, num);
		}
		int num2 = (int)Math.Min(_stream.Length - _stream.Position, _buffer.Length - num);
		if (num2 == 0)
		{
			throw new EndOfStreamException("Error reading savegame!");
		}
		_index = 0;
		_bufferLength = num + num2;
		_stream.Read(_buffer, num, num2);
	}

	public virtual bool IsAtEnd()
	{
		if (_stream.Position == _stream.Length)
		{
			return _index == _bufferLength;
		}
		return false;
	}

	public virtual int GetTotalLength()
	{
		return (int)_stream.Length;
	}

	public override void Add(ref bool value)
	{
		Read(out value);
	}

	public override void Add(ref byte value)
	{
		Read(out value);
	}

	public override void Add(ref sbyte value)
	{
		Read(out value);
	}

	public override void Add(ref char value)
	{
		Read(out value);
	}

	public override void Add(ref short value)
	{
		Read(out value);
	}

	public override void Add(ref int value)
	{
		Read(out value);
	}

	public override void Add(ref uint value)
	{
		Read(out value);
	}

	public override void Add(ref long value)
	{
		Read(out value);
	}

	public override void Add(ref ulong value)
	{
		Read(out value);
	}

	public override void Add(ref float value)
	{
		Read(out value);
	}

	public override void Add(ref double value)
	{
		Read(out value);
	}

	public override void Add(ref TimeSpan value)
	{
		Read(out value);
	}

	public override void Add(ref TerrainCoord value)
	{
		Read(out value);
	}

	public override void Add(ref Vector2 value)
	{
		Read(out value);
	}

	public override void Add(ref Vector3 value)
	{
		Read(out value);
	}

	public override void Add(ref Vector4 value)
	{
		Read(out value);
	}

	public override void Add(ref Half3 value)
	{
		Read(out value);
	}

	public override void Add(ref Quaternion value)
	{
		Read(out value);
	}

	public override void Add(ref Color32 value)
	{
		Read(out value);
	}

	public override void Add(ref Color value)
	{
		Read(out value);
	}

	public override void Add(ref Matrix4x4 value)
	{
		Read(ref value);
	}

	public override void Add(ref string value)
	{
		Read(out value);
	}

	public override void Add(ref BaseObject value)
	{
		Read(out value);
	}

	public override void Add(ref PlayerID value)
	{
		Read(out value);
	}

	public override void Add(ref EquipmentPrototype value)
	{
		Read(out value);
	}

	public override void Add(ref LiquidPrototype value)
	{
		Read(out value);
	}

	public override void Add(ref MemoryPrototype value)
	{
		Read(out value);
	}

	public override void Add(ref Recipe value)
	{
		Read(out value);
	}

	public override void Add(ref PropPrototype value)
	{
		Read(out value);
	}

	public override void Add(ref Trigger value)
	{
		Read(out value);
	}

	public override void Add(ref Speech value)
	{
		Read(out value);
	}

	public override void Add(ref Quest value)
	{
		Read(out value);
	}

	public override void Add(ref Invader value)
	{
		Read(out value);
	}

	public override void Add(Array src, int size, int dimensions)
	{
		Read(src, size);
	}

	public override void Add(ref byte[] bytes, ref int size)
	{
	}
}
