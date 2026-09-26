using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class Util
{
	public const int _MCW_DN = 50331648;

	public const int _MCW_EM = 524319;

	public const int _MCW_IC = 262144;

	public const int _MCW_PC = 196608;

	public const int _MCW_RC = 768;

	public const int _PC_24 = 131072;

	public const int _PC_64 = 0;

	public const int _RC_NEAR = 0;

	public const int _IC_PROJECTIVE = 0;

	public const int _DN_SAVE = 0;

	public const int _DN_FLUSH = 16777216;

	private static bool ControlFPSFound = true;

	private static List<Shadow> Shadows = new List<Shadow>();

	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "_controlfp_s")]
	public static extern int ControlFPS(IntPtr currentControl, uint newControl, uint mask);

	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "_controlfp")]
	public static extern int ControlFP(uint newControl, uint mask);

	public unsafe static int GetCurrentControlWord()
	{
		if (ControlFPSFound)
		{
			int result = 0;
			ControlFPS((IntPtr)(&result), 0u, 0u);
			return result;
		}
		return ControlFP(0u, 0u);
	}

	public static int SetCurrentControlWord(uint newControl, uint mask)
	{
		if (ControlFPSFound)
		{
			return ControlFPS((IntPtr)0, newControl, mask);
		}
		ControlFP(newControl, mask);
		return 0;
	}

	public static void SetFloatingPointControl()
	{
		if (SetCurrentControlWord(0u, 196608u) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_PC_64, _MCW_PC) failed!");
		}
		if (SetCurrentControlWord(0u, 768u) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_RC_NEAR, _MCW_RC) failed!");
		}
		if (SetCurrentControlWord(0u, 50331648u) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_DN_SAVE, _MCW_DN) failed!");
		}
		if (SetCurrentControlWord(0u, 262144u) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_IC_PROJECTIVE, _MCW_IC) failed!");
		}
		if (SetCurrentControlWord(524319u, 524319u) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_MCW_EM, _MCW_EM) failed!");
		}
	}

	public static void CheckFloatingPointControl()
	{
		if ((GetCurrentControlWord() & 0x30000) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_PC_64, _MCW_PC) got changed!");
		}
		if ((GetCurrentControlWord() & 0x300) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_RC_NEAR, _MCW_RC) got changed!");
		}
		if ((GetCurrentControlWord() & 0x3000000) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_DN_SAVE, _MCW_DN) got changed!");
		}
		if ((GetCurrentControlWord() & 0x40000) != 0)
		{
			Debug.LogError("SetCurrentControlWord(_IC_PROJECTIVE, _MCW_IC) got changed!");
		}
		if ((GetCurrentControlWord() & 0x8001F) != 524319)
		{
			Debug.LogError("SetCurrentControlWord(_MCW_EM, _MCW_EM) got changed!");
		}
	}

	public static bool AmIOnMainThread()
	{
		return Thread.CurrentThread == GameImpl.Instance.MainThread;
	}

	public static GameObject FindChild(this GameObject obj, string name)
	{
		Transform transform = obj.transform.Find(name);
		if (!(transform != null))
		{
			return null;
		}
		return transform.gameObject;
	}

	public static GameObject FindChildWithNameContaining(this GameObject obj, string str)
	{
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			if (gameObject.name.Contains(str))
			{
				return gameObject;
			}
			GameObject gameObject2 = gameObject.FindChildWithNameContaining(str);
			if (gameObject2 != null)
			{
				return gameObject2;
			}
		}
		return null;
	}

	public static void DeleteAllChildren(this GameObject obj)
	{
		for (int num = obj.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(obj.transform.GetChild(num).gameObject);
		}
	}

	public static void DeleteAllChildrenImmediately(this GameObject obj)
	{
		for (int num = obj.transform.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(obj.transform.GetChild(num).gameObject);
		}
	}

	public static Shadow GetShadowComponent(this GameObject obj)
	{
		Shadow result = null;
		obj.GetComponents(Shadows);
		foreach (Shadow shadow in Shadows)
		{
			if (!(shadow is Outline))
			{
				result = shadow;
				break;
			}
		}
		Shadows.Clear();
		return result;
	}

	public static void DeleteDirectoryAndAllFilesIfItExists(string dirName)
	{
		if (Directory.Exists(dirName))
		{
			string[] directories = Directory.GetDirectories(dirName);
			for (int i = 0; i < directories.Length; i++)
			{
				DeleteDirectoryAndAllFilesIfItExists(directories[i]);
			}
			directories = Directory.GetFiles(dirName);
			for (int i = 0; i < directories.Length; i++)
			{
				File.Delete(directories[i]);
			}
			Directory.Delete(dirName);
		}
	}

	public static void MoveFileIfItExists(string from, string to)
	{
		if (File.Exists(to))
		{
			Debug.Log("Deleting " + to);
			File.Delete(to);
		}
		if (File.Exists(from))
		{
			Debug.Log("Moving " + from + " to " + to);
			File.Move(from, to);
		}
	}

	public static void SetFileLastAccessTime(string fileName, DateTime lastAccessTime)
	{
		try
		{
			File.SetLastAccessTime(fileName, lastAccessTime);
		}
		catch (Exception ex)
		{
			Debug.Log("SetLastAccessTime error: " + ex.Message);
		}
	}

	public static void DeleteFile(string fileName)
	{
		try
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Unable to delete " + fileName + ": " + ex.Message);
		}
	}

	public static void AddUnique<T>(this List<T> list, T item)
	{
		if (!list.Contains(item))
		{
			list.Add(item);
		}
	}

	public static int IndexOf<T>(this ref Span<T> span, T val, int startIndex, int count)
	{
		for (int i = startIndex; i < count; i++)
		{
			ref T reference = ref span[i];
			object obj = val;
			if (reference.Equals(obj))
			{
				return i;
			}
		}
		return -1;
	}
}
