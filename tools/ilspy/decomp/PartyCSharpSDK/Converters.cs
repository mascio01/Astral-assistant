using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PartyCSharpSDK;

public static class Converters
{
	public static IntPtr OffsetPF(this IntPtr ptr, long that)
	{
		return new IntPtr(ptr.ToInt64() + that);
	}

	public static byte[] StringToNullTerminatedUTF8ByteArray(string str)
	{
		return StringToNullTerminatedUTF8ByteArrayInternal(str, -1);
	}

	public static byte[] StringToNullTerminatedUTF8ByteArray(string str, int requiredByteArrayLength)
	{
		return StringToNullTerminatedUTF8ByteArrayInternal(str, requiredByteArrayLength);
	}

	private static byte[] StringToNullTerminatedUTF8ByteArrayInternal(string str, int requiredByteArrayLength)
	{
		if (str == null)
		{
			return null;
		}
		if (requiredByteArrayLength == -1)
		{
			return Encoding.UTF8.GetBytes(str + "\0");
		}
		byte[] array = new byte[requiredByteArrayLength];
		Encoding.UTF8.GetBytes(str + "\0", 0, str.Length + 1, array, 0);
		return array;
	}

	public unsafe static void StringToNullTerminatedUTF8FixedPointer(string str, byte* bytePointer, int length)
	{
		Marshal.Copy(StringToNullTerminatedUTF8ByteArray(str, length), 0, (IntPtr)bytePointer, length);
	}

	public unsafe static string BytePointerToString(byte* bytePointer, int length)
	{
		byte[] array = new byte[length];
		Marshal.Copy((IntPtr)bytePointer, array, 0, length);
		return ByteArrayToString(array);
	}

	public static string ByteArrayToString(byte[] arr)
	{
		string text = Encoding.UTF8.GetString(arr);
		int num = text.IndexOf('\0');
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num);
	}

	public static string ByteArrayToString(byte[] arr, int index, int count)
	{
		return Encoding.UTF8.GetString(arr, index, count).TrimEnd(new char[1]);
	}

	public static string PtrToStringUTF8(IntPtr rawPtr)
	{
		if (rawPtr == IntPtr.Zero)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		while (true)
		{
			byte b = Marshal.ReadByte(rawPtr);
			if (b == 0)
			{
				break;
			}
			list.Add(b);
			rawPtr = rawPtr.OffsetPF(1L);
		}
		return Encoding.UTF8.GetString(list.ToArray());
	}

	public static ClassType PtrToClass<ClassType, InteropStructType>(IntPtr rawPtr, Func<InteropStructType, ClassType> ctor) where ClassType : class where InteropStructType : struct
	{
		if (rawPtr == IntPtr.Zero)
		{
			return null;
		}
		return ctor((InteropStructType)Marshal.PtrToStructure(rawPtr, typeof(InteropStructType)));
	}

	public static ClassType[] PtrToClassArray<ClassType, InteropStructType>(IntPtr rawPtr, SizeT count, Func<InteropStructType, ClassType> ctor)
	{
		return PtrToClassArray(rawPtr, count.ToUInt32(), ctor);
	}

	public static ClassType[] PtrToClassArray<ClassType, InteropStructType>(IntPtr rawPtr, uint count, Func<InteropStructType, ClassType> ctor)
	{
		ClassType[] array = new ClassType[count];
		if (IntPtr.Zero != rawPtr)
		{
			int num = Marshal.SizeOf(typeof(InteropStructType));
			for (int i = 0; i < count; i++)
			{
				InteropStructType arg = (InteropStructType)Marshal.PtrToStructure(rawPtr.OffsetPF(i * num), typeof(InteropStructType));
				array[i] = ctor(arg);
			}
		}
		return array;
	}

	public static List<ClassType> PtrToClassListFromPool<ClassType, InteropStructType>(IntPtr rawPtr, uint count, ObjectPool objectPool)
	{
		List<ClassType> list = objectPool.Retrieve<List<ClassType>>();
		if (IntPtr.Zero != rawPtr)
		{
			int num = Marshal.SizeOf(typeof(InteropStructType));
			for (int i = 0; i < count; i++)
			{
				InteropStructType val = (InteropStructType)Marshal.PtrToStructure(rawPtr.OffsetPF(i * num), typeof(InteropStructType));
				list.Add(objectPool.Retrieve<ClassType>(val));
			}
		}
		return list;
	}

	public static IntPtr ClassArrayToPtr<ClassType, InteropStructType>(ClassType[] inputTypes, Func<ClassType, DisposableCollection, InteropStructType> converter, DisposableCollection disposableCollection, out SizeT arrayCount)
	{
		if (inputTypes == null)
		{
			arrayCount = new SizeT(0);
			return IntPtr.Zero;
		}
		bool isEnum = typeof(InteropStructType).IsEnum;
		int num = Marshal.SizeOf(isEnum ? Enum.GetUnderlyingType(typeof(InteropStructType)) : typeof(InteropStructType));
		DisposableBuffer disposableBuffer = disposableCollection.Add(new DisposableBuffer(checked(num * inputTypes.Length)));
		IntPtr ptr = disposableBuffer.IntPtr;
		foreach (ClassType arg in inputTypes)
		{
			Marshal.StructureToPtr(isEnum ? Convert.ChangeType(converter(arg, disposableCollection), Enum.GetUnderlyingType(typeof(InteropStructType))) : ((object)converter(arg, disposableCollection)), ptr, fDeleteOld: false);
			ptr = ptr.OffsetPF(num);
		}
		arrayCount = new SizeT(inputTypes.Length);
		return disposableBuffer.IntPtr;
	}

	public static InteropStructType[] ConvertArrayToFixedLength<ClassType, InteropStructType>(ClassType[] classes, int length, Func<ClassType, InteropStructType> ctor)
	{
		InteropStructType[] array = new InteropStructType[length];
		int num = Math.Min(length, classes.Length);
		for (int i = 0; i < num; i++)
		{
			array[i] = ctor(classes[i]);
		}
		return array;
	}
}
