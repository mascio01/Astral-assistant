using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_REGIONS_CHANGED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	internal PARTY_REGIONS_CHANGED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_REGIONS_CHANGED_STATE_CHANGE regionsChanged = stateChange.regionsChanged;
		result = regionsChanged.result;
		errorDetail = regionsChanged.errorDetail;
	}
}
