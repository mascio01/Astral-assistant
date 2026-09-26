using System;
using System.IO;
using UnityEngine;

public class CustomBinaryWriter : Reflector, IDisposable
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

	private Stream _stream;

	public override bool IsDeserialising => false;

	protected CustomBinaryWriter()
	{
	}

	public CustomBinaryWriter(Stream stream)
	{
		_buffer = new byte[65536];
		_stream = stream;
	}

	public void Dispose()
	{
		if (_index > 0)
		{
			Flush();
		}
	}

	public void Write(bool value)
	{
		byte value2 = (byte)(value ? 1 : 0);
		Write(value2);
	}

	public void Write(byte value)
	{
		if (_index + 1 >= _buffer.Length)
		{
			Flush();
		}
		_buffer[_index] = value;
		_index++;
	}

	public void Write(sbyte value)
	{
		if (_index + 1 >= _buffer.Length)
		{
			Flush();
		}
		_buffer[_index] = (byte)value;
		_index++;
	}

	public void Write(char value)
	{
		_char[0] = value;
		Write(_char, 2);
	}

	public void Write(short value)
	{
		_short[0] = value;
		Write(_short, 2);
	}

	public void Write(ushort value)
	{
		_ushort[0] = value;
		Write(_ushort, 2);
	}

	public void Write(int value)
	{
		_int[0] = value;
		Write(_int, 4);
	}

	public void Write(uint value)
	{
		_uint[0] = value;
		Write(_uint, 4);
	}

	public void Write(long value)
	{
		_long[0] = value;
		Write(_long, 8);
	}

	public void Write(ulong value)
	{
		_ulong[0] = value;
		Write(_ulong, 8);
	}

	public void Write(float value)
	{
		_float[0] = value;
		Write(_float, 4);
	}

	public void Write(double value)
	{
		_double[0] = value;
		Write(_double, 8);
	}

	public void Write(TimeSpan value)
	{
		Write(value.Ticks);
	}

	public void Write(TerrainCoord value)
	{
		Write(value.x);
		Write(value.y);
	}

	public void Write(Vector2 value)
	{
		Write(value.x);
		Write(value.y);
	}

	public void Write(Vector3 value)
	{
		Write(value.x);
		Write(value.y);
		Write(value.z);
	}

	public void Write(Vector4 value)
	{
		Write(value.x);
		Write(value.y);
		Write(value.z);
		Write(value.w);
	}

	public void Write(Half3 value)
	{
		Write(value.x);
		Write(value.y);
		Write(value.z);
	}

	public void Write(Quaternion value)
	{
		Write(value.x);
		Write(value.y);
		Write(value.z);
		Write(value.w);
	}

	public void Write(Color32 value)
	{
		Write(value.r);
		Write(value.g);
		Write(value.b);
		Write(value.a);
	}

	public void Write(Color value)
	{
		Write(value.r);
		Write(value.g);
		Write(value.b);
		Write(value.a);
	}

	public void Write(ref Matrix4x4 value)
	{
		Write(value.m00);
		Write(value.m01);
		Write(value.m02);
		Write(value.m03);
		Write(value.m10);
		Write(value.m11);
		Write(value.m12);
		Write(value.m13);
		Write(value.m20);
		Write(value.m21);
		Write(value.m22);
		Write(value.m23);
		Write(value.m30);
		Write(value.m31);
		Write(value.m32);
		Write(value.m33);
	}

	public void Write(string value)
	{
		int num = value?.Length ?? 0;
		Write(num);
		for (int i = 0; i < num; i++)
		{
			Write(value[i]);
		}
	}

	public void Write(BaseObject value)
	{
		int value2 = value?.Id ?? (-1);
		Write(value2);
	}

	public void Write(PlayerID value)
	{
		ulong steamID = value.SteamID.m_SteamID;
		Write(steamID);
	}

	public void Write(EquipmentPrototype value)
	{
		string value2 = ((value != null) ? value.Name : string.Empty);
		Write(value2);
	}

	public void Write(LiquidPrototype value)
	{
		string value2 = ((value != null) ? value.Name : string.Empty);
		Write(value2);
	}

	public void Write(MemoryPrototype value)
	{
		string value2 = ((value != null) ? value.UniqueID : string.Empty);
		Write(value2);
	}

	public void Write(Recipe value)
	{
		string value2 = ((value != null) ? value.UniqueID : string.Empty);
		Write(value2);
	}

	public void Write(PropPrototype value)
	{
		int value2 = value?.NameHash ?? 0;
		Write(value2);
	}

	public void Write(Trigger value)
	{
		string value2 = ((value != null) ? value.UniqueID : string.Empty);
		Write(value2);
	}

	public void Write(Speech value)
	{
		string value2 = ((value != null) ? value.UniqueID : string.Empty);
		Write(value2);
	}

	public void Write(Quest value)
	{
		string value2 = ((value != null) ? value.UniqueID : string.Empty);
		Write(value2);
	}

	public void Write(Invader value)
	{
		string value2 = ((value != null) ? value.UniqueID : string.Empty);
		Write(value2);
	}

	public void Write(Array src, int size)
	{
		int num = 0;
		if (_index + size > _buffer.Length)
		{
			Flush();
			while (_index + size > _buffer.Length)
			{
				int num2 = _buffer.Length - _index;
				Buffer.BlockCopy(src, num, _buffer, _index, num2);
				_index += num2;
				num += num2;
				size -= num2;
				Flush();
			}
		}
		Buffer.BlockCopy(src, num, _buffer, _index, size);
		_index += size;
	}

	public virtual void Flush()
	{
		_stream.Write(_buffer, 0, _index);
		_index = 0;
	}

	public override void Add(ref bool value)
	{
		Write(value);
	}

	public override void Add(ref byte value)
	{
		Write(value);
	}

	public override void Add(ref sbyte value)
	{
		Write(value);
	}

	public override void Add(ref char value)
	{
		Write(value);
	}

	public override void Add(ref short value)
	{
		Write(value);
	}

	public override void Add(ref int value)
	{
		Write(value);
	}

	public override void Add(ref uint value)
	{
		Write(value);
	}

	public override void Add(ref long value)
	{
		Write(value);
	}

	public override void Add(ref ulong value)
	{
		Write(value);
	}

	public override void Add(ref float value)
	{
		Write(value);
	}

	public override void Add(ref double value)
	{
		Write(value);
	}

	public override void Add(ref TimeSpan value)
	{
		Write(value);
	}

	public override void Add(ref TerrainCoord value)
	{
		Write(value);
	}

	public override void Add(ref Vector2 value)
	{
		Write(value);
	}

	public override void Add(ref Vector3 value)
	{
		Write(value);
	}

	public override void Add(ref Vector4 value)
	{
		Write(value);
	}

	public override void Add(ref Half3 value)
	{
		Write(value);
	}

	public override void Add(ref Quaternion value)
	{
		Write(value);
	}

	public override void Add(ref Color32 value)
	{
		Write(value);
	}

	public override void Add(ref Color value)
	{
		Write(value);
	}

	public override void Add(ref Matrix4x4 value)
	{
		Write(ref value);
	}

	public override void Add(ref string value)
	{
		Write(value);
	}

	public override void Add(ref BaseObject value)
	{
		Write(value);
	}

	public override void Add(ref PlayerID value)
	{
		Write(value);
	}

	public override void Add(ref EquipmentPrototype value)
	{
		Write(value);
	}

	public override void Add(ref LiquidPrototype value)
	{
		Write(value);
	}

	public override void Add(ref MemoryPrototype value)
	{
		Write(value);
	}

	public override void Add(ref Recipe value)
	{
		Write(value);
	}

	public override void Add(ref PropPrototype value)
	{
		Write(value);
	}

	public override void Add(ref Trigger value)
	{
		Write(value);
	}

	public override void Add(ref Speech value)
	{
		Write(value);
	}

	public override void Add(ref Quest value)
	{
		Write(value);
	}

	public override void Add(ref Invader value)
	{
		Write(value);
	}

	public override void Add(Array src, int size, int dimensions)
	{
		Write(src, size);
	}

	public override void Add(ref byte[] bytes, ref int size)
	{
	}
}
