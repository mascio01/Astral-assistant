using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_NETWORK_CONFIGURATION_MADE_AVAILABLE_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_NETWORK_CONFIGURATION networkConfiguration { get; }

	internal PARTY_NETWORK_CONFIGURATION_MADE_AVAILABLE_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_NETWORK_CONFIGURATION_MADE_AVAILABLE_STATE_CHANGE networkConfigurationMadeAvailable = stateChange.networkConfigurationMadeAvailable;
		network = new PARTY_NETWORK_HANDLE(networkConfigurationMadeAvailable.network);
		networkConfiguration = Converters.PtrToClass(networkConfigurationMadeAvailable.networkConfiguration, (PartyCSharpSDK.Interop.PARTY_NETWORK_CONFIGURATION x) => new PARTY_NETWORK_CONFIGURATION(x));
	}
}
