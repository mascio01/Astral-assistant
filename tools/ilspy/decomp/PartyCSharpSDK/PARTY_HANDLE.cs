using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_HANDLE
{
	internal PartyCSharpSDK.Interop.PARTY_HANDLE InteropHandle { get; set; }

	public long GetHandleValue()
	{
		return InteropHandle.handle.ToInt64();
	}

	public PARTY_HANDLE(long handleValue)
	{
		InteropHandle = new PartyCSharpSDK.Interop.PARTY_HANDLE(handleValue);
	}

	internal PARTY_HANDLE(PartyCSharpSDK.Interop.PARTY_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyCSharpSDK.Interop.PARTY_HANDLE interopHandle, out PARTY_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyCSharpSDK.Interop.PARTY_HANDLE);
	}
}
