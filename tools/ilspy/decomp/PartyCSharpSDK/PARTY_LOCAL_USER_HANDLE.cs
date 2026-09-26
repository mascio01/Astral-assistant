using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_LOCAL_USER_HANDLE
{
	internal PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE InteropHandle { get; set; }

	internal PARTY_LOCAL_USER_HANDLE(PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE interopHandle, out PARTY_LOCAL_USER_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_LOCAL_USER_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE);
	}
}
