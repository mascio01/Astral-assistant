using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_NETWORK_HANDLE
{
	internal PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE InteropHandle { get; set; }

	internal PARTY_NETWORK_HANDLE(PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE interopHandle, out PARTY_NETWORK_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_NETWORK_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE);
	}
}
