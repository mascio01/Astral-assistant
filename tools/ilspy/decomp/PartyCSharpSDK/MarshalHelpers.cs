using System;
using System.Runtime.InteropServices;

namespace PartyCSharpSDK;

public static class MarshalHelpers
{
	public delegate uint GetContextFunc<InteropHandle>(InteropHandle handle, out IntPtr context);

	public delegate uint GetHandlesFun<InputInteropHandle>(InputInteropHandle input, out uint count, out IntPtr handles);

	public static uint GetCustomContext<InteropHandle>(GetContextFunc<InteropHandle> getContextFunc, InteropHandle handle, out object customContext)
	{
		customContext = null;
		IntPtr context;
		uint num = getContextFunc(handle, out context);
		if (PartyError.SUCCEEDED(num) && context != IntPtr.Zero)
		{
			customContext = GCHandle.FromIntPtr(context).Target;
		}
		return num;
	}

	public static uint SetCustomContext<InteropHandle>(GetContextFunc<InteropHandle> getContextFunc, Func<InteropHandle, IntPtr, uint> setContextFunc, InteropHandle handle, object customContext)
	{
		IntPtr context;
		uint num = getContextFunc(handle, out context);
		if (PartyError.SUCCEEDED(num))
		{
			IntPtr intPtr = IntPtr.Zero;
			if (customContext != null)
			{
				intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(customContext));
			}
			num = setContextFunc(handle, intPtr);
			if (PartyError.SUCCEEDED(num))
			{
				if (context != IntPtr.Zero)
				{
					GCHandle.FromIntPtr(context).Free();
				}
			}
			else if (intPtr != IntPtr.Zero)
			{
				GCHandle.FromIntPtr(intPtr).Free();
			}
		}
		return num;
	}

	public static uint GetArrayOfObjects<InputInteropHandle, IntermediaObject, OutputObject>(GetHandlesFun<InputInteropHandle> getHandlesFun, Func<IntermediaObject, OutputObject> ctorFun, InputInteropHandle inputHandle, out OutputObject[] outputHandles)
	{
		outputHandles = null;
		uint count;
		IntPtr handles;
		uint num = getHandlesFun(inputHandle, out count, out handles);
		if (PartyError.SUCCEEDED(num))
		{
			outputHandles = Converters.PtrToClassArray(handles, count, ctorFun);
		}
		return num;
	}
}
