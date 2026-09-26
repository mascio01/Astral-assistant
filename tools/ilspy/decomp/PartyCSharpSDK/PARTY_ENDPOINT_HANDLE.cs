using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_ENDPOINT_HANDLE
{
	internal PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE InteropHandle { get; set; }

	internal PARTY_ENDPOINT_HANDLE(PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE interopHandle, out PARTY_ENDPOINT_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_ENDPOINT_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE);
	}
}
