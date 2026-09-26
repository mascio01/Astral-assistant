using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_STATE_CHANGE
{
	public PARTY_XBL_STATE_CHANGE_TYPE StateChangeType { get; }

	internal IntPtr StateChangeId { get; }

	protected PARTY_XBL_STATE_CHANGE(PARTY_XBL_STATE_CHANGE_TYPE StateChangeType, IntPtr StateChangeId)
	{
		this.StateChangeType = StateChangeType;
		this.StateChangeId = StateChangeId;
	}

	internal static PARTY_XBL_STATE_CHANGE CreateFromPtr(IntPtr stateChangePtr)
	{
		PARTY_XBL_STATE_CHANGE pARTY_XBL_STATE_CHANGE = null;
		PARTY_XBL_STATE_CHANGE_UNION stateChange = (PARTY_XBL_STATE_CHANGE_UNION)Marshal.PtrToStructure(stateChangePtr, typeof(PARTY_XBL_STATE_CHANGE_UNION));
		switch (stateChange.stateChange.stateChangeType)
		{
		case PARTY_XBL_STATE_CHANGE_TYPE.PARTY_XBL_STATE_CHANGE_TYPE_TOKEN_AND_SIGNATURE_REQUESTED:
			return new PARTY_XBL_TOKEN_AND_SIGNATURE_REQUESTED_STATE_CHANGE(stateChange, stateChangePtr);
		case PARTY_XBL_STATE_CHANGE_TYPE.PARTY_XBL_STATE_CHANGE_TYPE_LOCAL_CHAT_USER_DESTROYED:
			return new PARTY_XBL_LOCAL_CHAT_USER_DESTROYED_STATE_CHANGE(stateChange, stateChangePtr);
		case PARTY_XBL_STATE_CHANGE_TYPE.PARTY_XBL_STATE_CHANGE_TYPE_CREATE_LOCAL_CHAT_USER_COMPLETED:
			return new PARTY_XBL_CREATE_LOCAL_CHAT_USER_COMPLETED_STATE_CHANGE(stateChange, stateChangePtr);
		case PARTY_XBL_STATE_CHANGE_TYPE.PARTY_XBL_STATE_CHANGE_TYPE_LOGIN_TO_PLAYFAB_COMPLETED:
			return new PARTY_XBL_LOGIN_TO_PLAYFAB_COMPLETED_STATE_CHANGE(stateChange, stateChangePtr);
		case PARTY_XBL_STATE_CHANGE_TYPE.PARTY_XBL_STATE_CHANGE_TYPE_GET_ENTITY_IDS_FROM_XBOX_LIVE_USER_IDS_COMPLETED:
			return new PARTY_XBL_GET_ENTITY_IDS_FROM_XBOX_LIVE_USER_IDS_COMPLETED_STATE_CHANGE(stateChange, stateChangePtr);
		case PARTY_XBL_STATE_CHANGE_TYPE.PARTY_XBL_STATE_CHANGE_TYPE_REQUIRED_CHAT_PERMISSION_INFO_CHANGED:
			return new PARTY_XBL_REQUIRED_CHAT_PERMISSION_INFO_CHANGED_STATE_CHANGE(stateChange, stateChangePtr);
		default:
			Debugger.Break();
			return new PARTY_XBL_STATE_CHANGE(stateChange.stateChange.stateChangeType, stateChangePtr);
		}
	}
}
