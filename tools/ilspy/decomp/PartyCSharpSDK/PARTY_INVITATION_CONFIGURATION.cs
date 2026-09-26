using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_INVITATION_CONFIGURATION
{
	public string Identifier { get; set; }

	public PARTY_INVITATION_REVOCABILITY Revocability { get; set; }

	public string[] EntityIds { get; set; }

	internal PARTY_INVITATION_CONFIGURATION(PartyCSharpSDK.Interop.PARTY_INVITATION_CONFIGURATION interopStruct)
	{
		Identifier = interopStruct.identifier.GetString();
		Revocability = interopStruct.revocability;
		EntityIds = interopStruct.GetEntityIds((UTF8StringPtr x) => x.GetString());
	}

	public PARTY_INVITATION_CONFIGURATION()
	{
	}
}
