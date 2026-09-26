using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CHAT_CONTROL_HANDLE
{
	internal PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE InteropHandle { get; set; }

	internal PARTY_CHAT_CONTROL_HANDLE(PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE interopHandle, out PARTY_CHAT_CONTROL_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_CHAT_CONTROL_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE);
	}
}
