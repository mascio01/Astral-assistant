using PartyCSharpSDK;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_HANDLE
{
	internal PartyXBLCSharpSDK.Interop.PARTY_XBL_HANDLE InteropHandle { get; set; }

	public long GetHandleValue()
	{
		return InteropHandle.handle.ToInt64();
	}

	public PARTY_XBL_HANDLE(long handleValue)
	{
		InteropHandle = new PartyXBLCSharpSDK.Interop.PARTY_XBL_HANDLE(handleValue);
	}

	internal PARTY_XBL_HANDLE(PartyXBLCSharpSDK.Interop.PARTY_XBL_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyXBLCSharpSDK.Interop.PARTY_XBL_HANDLE interopHandle, out PARTY_XBL_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_XBL_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyXBLCSharpSDK.Interop.PARTY_XBL_HANDLE);
	}
}
