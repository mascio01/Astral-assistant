using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	public PARTY_NETWORK_CONFIGURATION networkConfiguration { get; }

	public uint regionCount { get; }

	public PARTY_REGION[] regions { get; }

	public object asyncIdentifier { get; }

	public PARTY_NETWORK_DESCRIPTOR networkDescriptor { get; }

	public string appliedInitialInvitationIdentifier { get; }

	internal PARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE createNewNetworkCompleted = stateChange.createNewNetworkCompleted;
		result = createNewNetworkCompleted.result;
		errorDetail = createNewNetworkCompleted.errorDetail;
		localUser = new PARTY_LOCAL_USER_HANDLE(createNewNetworkCompleted.localUser);
		networkConfiguration = new PARTY_NETWORK_CONFIGURATION(createNewNetworkCompleted.networkConfiguration);
		regionCount = createNewNetworkCompleted.regionCount;
		regions = Converters.PtrToClassArray(createNewNetworkCompleted.regions, regionCount, (PartyCSharpSDK.Interop.PARTY_REGION x) => new PARTY_REGION(x));
		asyncIdentifier = null;
		if (createNewNetworkCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(createNewNetworkCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
		networkDescriptor = new PARTY_NETWORK_DESCRIPTOR(createNewNetworkCompleted.networkDescriptor);
		appliedInitialInvitationIdentifier = Converters.PtrToStringUTF8(createNewNetworkCompleted.appliedInitialInvitationIdentifier);
	}
}
