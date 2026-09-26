using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_GET_ENTITY_IDS_FROM_XBOX_LIVE_USER_IDS_COMPLETED_STATE_CHANGE : PARTY_XBL_STATE_CHANGE
{
	public PARTY_XBL_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public string xboxLiveSandbox { get; }

	public PARTY_XBL_CHAT_USER_HANDLE localChatUser { get; }

	public object asyncIdentifier { get; }

	public PARTY_XBL_XBOX_USER_ID_TO_PLAYFAB_ENTITY_ID_MAPPING[] entityIdMappings { get; }

	internal PARTY_XBL_GET_ENTITY_IDS_FROM_XBOX_LIVE_USER_IDS_COMPLETED_STATE_CHANGE(PARTY_XBL_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyXBLCSharpSDK.Interop.PARTY_XBL_GET_ENTITY_IDS_FROM_XBOX_LIVE_USER_IDS_COMPLETED_STATE_CHANGE getEntityIdsFromXboxLiveUserIdsCompleted = stateChange.getEntityIdsFromXboxLiveUserIdsCompleted;
		result = getEntityIdsFromXboxLiveUserIdsCompleted.result;
		errorDetail = getEntityIdsFromXboxLiveUserIdsCompleted.errorDetail;
		xboxLiveSandbox = Converters.PtrToStringUTF8(getEntityIdsFromXboxLiveUserIdsCompleted.xboxLiveSandbox);
		localChatUser = new PARTY_XBL_CHAT_USER_HANDLE(getEntityIdsFromXboxLiveUserIdsCompleted.localChatUser);
		asyncIdentifier = null;
		if (getEntityIdsFromXboxLiveUserIdsCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(getEntityIdsFromXboxLiveUserIdsCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
		entityIdMappings = Converters.PtrToClassArray(getEntityIdsFromXboxLiveUserIdsCompleted.entityIdMappings, getEntityIdsFromXboxLiveUserIdsCompleted.entityIdMappingCount, (PartyXBLCSharpSDK.Interop.PARTY_XBL_XBOX_USER_ID_TO_PLAYFAB_ENTITY_ID_MAPPING x) => new PARTY_XBL_XBOX_USER_ID_TO_PLAYFAB_ENTITY_ID_MAPPING(x));
	}
}
