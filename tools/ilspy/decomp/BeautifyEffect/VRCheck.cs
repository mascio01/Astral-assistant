using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace BeautifyEffect;

internal static class VRCheck
{
	public static bool isActive;

	public static bool isVrRunning;

	private static readonly List<XRDisplaySubsystemDescriptor> displaysDescs = new List<XRDisplaySubsystemDescriptor>();

	private static readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();

	private static bool IsActive()
	{
		displaysDescs.Clear();
		SubsystemManager.GetSubsystemDescriptors(displaysDescs);
		return displaysDescs.Count > 0;
	}

	private static bool IsVrRunning()
	{
		bool result = false;
		displays.Clear();
		SubsystemManager.GetSubsystems(displays);
		foreach (XRDisplaySubsystem display in displays)
		{
			if (display.running)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static void Init()
	{
		isActive = IsActive();
		isVrRunning = IsVrRunning();
	}
}
