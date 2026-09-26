using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public struct MD5Hash : IEquatable<MD5Hash>, IReflectable
{
	private ulong A;

	private ulong B;

	private static GameProfiler HashTimer = new GameProfiler("CalcHash");

	private static char[] Hex = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};

	public MD5Hash(byte[] buffer, int size)
	{
		A = 0uL;
		B = 0uL;
		using (new ProfileMarker(HashTimer))
		{
			using MD5 mD = MD5.Create();
			byte[] array = mD.ComputeHash(buffer, 0, size);
			if (array.Length == 16)
			{
				for (int i = 0; i < 8; i++)
				{
					A |= (ulong)array[i] << i * 8;
					B |= (ulong)array[i + 8] << i * 8;
				}
			}
		}
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 203)
		{
			int value = 16;
			reflector.Add(ref value);
			if (value != 16)
			{
				Debug.Log("Expected hash length to be 16 bytes");
			}
		}
		reflector.Add(ref A);
		reflector.Add(ref B);
	}

	public static bool operator ==(MD5Hash a, MD5Hash b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(MD5Hash a, MD5Hash b)
	{
		return !a.Equals(b);
	}

	public bool Equals(MD5Hash other)
	{
		if (A == other.A)
		{
			return B == other.B;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is MD5Hash)
		{
			return this == (MD5Hash)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int i;
		int num = (i = 0);
		for (; i < 8; i++)
		{
			num += (byte)(A >> i * 8);
			num += num << 10;
			num ^= num >> 6;
		}
		num = (i = 0);
		for (; i < 8; i++)
		{
			num += (byte)(B >> i * 8);
			num += num << 10;
			num ^= num >> 6;
		}
		num += num << 3;
		num ^= num >> 11;
		return num + (num << 15);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(32);
		for (int i = 0; i < 8; i++)
		{
			stringBuilder.Append(Hex[(A >> i * 8 + 4) & 0xF]);
			stringBuilder.Append(Hex[(A >> i * 8) & 0xF]);
		}
		for (int j = 0; j < 8; j++)
		{
			stringBuilder.Append(Hex[(B >> j * 8 + 4) & 0xF]);
			stringBuilder.Append(Hex[(B >> j * 8) & 0xF]);
		}
		return stringBuilder.ToString();
	}

	public MD5Hash(string str)
	{
		if (str.Length == 32 && ulong.TryParse(str.Substring(0, 16), NumberStyles.HexNumber, null, out A) && ulong.TryParse(str.Substring(16, 16), NumberStyles.HexNumber, null, out B))
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			for (int i = 0; i < 64; i += 8)
			{
				ulong num3 = (A >> i) & 0xFF;
				ulong num4 = (B >> i) & 0xFF;
				num |= num3 << 56 - i;
				num2 |= num4 << 56 - i;
			}
			A = num;
			B = num2;
		}
		else
		{
			A = (B = 0uL);
		}
	}

	public bool IsNull()
	{
		if (A == 0L)
		{
			return B == 0;
		}
		return false;
	}

	public bool IsValid()
	{
		if (A == 0L)
		{
			return B != 0;
		}
		return true;
	}
}
