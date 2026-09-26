using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class CursorUtil
{
	private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;

		public RECT(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}

	public struct POINT
	{
		public int x;

		public int y;

		public POINT(int X, int Y)
		{
			x = X;
			y = Y;
		}
	}

	private static int ShowCursorCount;

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ClipCursor(ref RECT rcClip);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetClipCursor(out RECT rcClip);

	[DllImport("user32.dll")]
	private static extern int GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern int GetActiveWindow();

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetWindowRect(int hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetClientRect(int hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool ClientToScreen(int hWnd, ref POINT lpPoint);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out POINT point);

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	[DllImport("user32.dll")]
	private static extern int ShowCursor(bool bShow);

	[DllImport("user32")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

	public static bool GetClientScreenRect(ref RECT rect)
	{
		int activeWindow = GetActiveWindow();
		if (activeWindow == 0)
		{
			return false;
		}
		GetClientRect(activeWindow, ref rect);
		POINT lpPoint = new POINT
		{
			x = rect.Left,
			y = rect.Top
		};
		ClientToScreen(activeWindow, ref lpPoint);
		POINT lpPoint2 = new POINT
		{
			x = rect.Right,
			y = rect.Bottom
		};
		ClientToScreen(activeWindow, ref lpPoint2);
		rect.Left = lpPoint.x;
		rect.Top = lpPoint.y;
		rect.Right = lpPoint2.x;
		rect.Bottom = lpPoint2.y;
		return true;
	}

	public static bool IsMouseInClientBounds()
	{
		RECT rect = default(RECT);
		if (!GetClientScreenRect(ref rect))
		{
			return false;
		}
		POINT point = default(POINT);
		if (!GetCursorPos(out point))
		{
			return false;
		}
		if (point.x >= rect.Left && point.x <= rect.Right && point.y >= rect.Top)
		{
			return point.y <= rect.Bottom;
		}
		return false;
	}

	public static Vector2 GetMouseClientPos()
	{
		RECT rect = default(RECT);
		CalcClientScaleAndOffset(out var offsetX, out var offsetY, out var scaleX, out var scaleY, ref rect);
		POINT point = default(POINT);
		GetCursorPos(out point);
		return new Vector2(((float)(point.x - rect.Left) - offsetX) / scaleX, ((float)(point.y - rect.Top) - offsetY) / scaleY);
	}

	public static void SetMouseClientPos(Vector2 pos)
	{
		RECT rect = default(RECT);
		if (CalcClientScaleAndOffset(out var offsetX, out var offsetY, out var scaleX, out var scaleY, ref rect))
		{
			pos.x = pos.x * scaleX + offsetX;
			pos.y = pos.y * scaleY + offsetY;
			SetCursorPos((int)pos.x + rect.Left, (int)pos.y + rect.Top);
		}
	}

	private static bool CalcClientScaleAndOffset(out float offsetX, out float offsetY, out float scaleX, out float scaleY, ref RECT rect)
	{
		offsetX = 0f;
		offsetY = 0f;
		scaleX = 1f;
		scaleY = 1f;
		if (!GetClientScreenRect(ref rect))
		{
			return false;
		}
		float num = rect.Right - rect.Left;
		float num2 = rect.Bottom - rect.Top;
		float num3 = (float)Screen.width / (float)Screen.height;
		float num4 = num / num2;
		scaleX = num / (float)Screen.width;
		scaleY = num2 / (float)Screen.height;
		if (num3 < num4)
		{
			scaleX = num2 / (float)Screen.height;
			offsetX = (num - (float)Screen.width * scaleX) * 0.5f;
		}
		else if (num3 > num4)
		{
			scaleY = num / (float)Screen.width;
			offsetY = (num2 - (float)Screen.height * scaleY) * 0.5f;
		}
		return true;
	}

	public static void StartClipping(int x, int y, int width, int height)
	{
		RECT rect = default(RECT);
		if (CalcClientScaleAndOffset(out var offsetX, out var offsetY, out var scaleX, out var scaleY, ref rect))
		{
			float f = (float)x * scaleX + offsetX;
			float f2 = (float)y * scaleY + offsetY;
			float num = (float)width * scaleX;
			float num2 = (float)height * scaleY;
			rect.Left += Mathf.RoundToInt(f);
			rect.Top += Mathf.RoundToInt(f2);
			rect.Right = Mathf.RoundToInt((float)rect.Left + num);
			rect.Bottom = Mathf.RoundToInt((float)rect.Top + num2);
			ClipCursor(ref rect);
		}
	}

	public static void OnApplicationQuit()
	{
		StopClipping();
	}

	public static void StopClipping()
	{
		RECT rcClip = new RECT(-10000, -10000, 10000, 10000);
		ClipCursor(ref rcClip);
	}

	public static void SetCursorVisible(bool visible)
	{
		if (ShowCursorCount >= 0 != visible)
		{
			ShowCursorCount = ShowCursor(visible);
		}
	}
}
