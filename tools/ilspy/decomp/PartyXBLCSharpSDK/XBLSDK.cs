using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PartyCSharpSDK;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class XBLSDK
{
	private const uint PartyErrorXblChatUserAlreadyExists = 20481u;

	internal static ObjectPool objectPool;

	static XBLSDK()
	{
		objectPool = new ObjectPool();
		objectPool.AddEntry<List<PARTY_XBL_STATE_CHANGE>>(4, new Type[0]);
	}

	public static uint PartyXblChatUserIsLocal(PARTY_XBL_CHAT_USER_HANDLE handle, out bool isLocal)
	{
		isLocal = false;
		if (handle == null)
		{
			return 4u;
		}
		byte isLocal2;
		uint num = PartyXblInterop.PartyXblChatUserIsLocal(handle.InteropHandle, out isLocal2);
		if (PartyError.SUCCEEDED(num))
		{
			isLocal = Convert.ToBoolean(isLocal2);
		}
		return num;
	}

	public static uint PartyXblChatUserGetXboxUserId(PARTY_XBL_CHAT_USER_HANDLE handle, out ulong xboxUserId)
	{
		xboxUserId = 0uL;
		if (handle == null)
		{
			return 4u;
		}
		return PartyXblInterop.PartyXblChatUserGetXboxUserId(handle.InteropHandle, out xboxUserId);
	}

	public static uint PartyXblChatUserSetCustomContext(PARTY_XBL_CHAT_USER_HANDLE handle, object customContext)
	{
		if (handle == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PartyXblInterop.PartyXblChatUserGetCustomContext, PartyXblInterop.PartyXblChatUserSetCustomContext, handle.InteropHandle, customContext);
	}

	public static uint PartyXblChatUserGetCustomContext(PARTY_XBL_CHAT_USER_HANDLE handle, out object customContext)
	{
		if (handle == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PartyXblInterop.PartyXblChatUserGetCustomContext, handle.InteropHandle, out customContext);
	}

	public static uint PartyXblLocalChatUserGetAccessibilitySettings(PARTY_XBL_CHAT_USER_HANDLE handle, out PARTY_XBL_ACCESSIBILITY_SETTINGS settings)
	{
		settings = null;
		if (handle == null)
		{
			return 4u;
		}
		PartyXBLCSharpSDK.Interop.PARTY_XBL_ACCESSIBILITY_SETTINGS settings2;
		uint num = PartyXblInterop.PartyXblLocalChatUserGetAccessibilitySettings(handle.InteropHandle, out settings2);
		if (PartyError.SUCCEEDED(num))
		{
			settings = new PARTY_XBL_ACCESSIBILITY_SETTINGS(settings2);
		}
		return num;
	}

	public static uint PartyXblLocalChatUserGetRequiredChatPermissionInfo(PARTY_XBL_CHAT_USER_HANDLE handle, PARTY_XBL_CHAT_USER_HANDLE targetChaUser, out PARTY_XBL_CHAT_PERMISSION_INFO chatPermissionInfo)
	{
		chatPermissionInfo = null;
		if (handle == null || targetChaUser == null)
		{
			return 4u;
		}
		PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_PERMISSION_INFO chatPermissionInfo2;
		uint num = PartyXblInterop.PartyXblLocalChatUserGetRequiredChatPermissionInfo(handle.InteropHandle, targetChaUser.InteropHandle, out chatPermissionInfo2);
		if (PartyError.SUCCEEDED(num))
		{
			chatPermissionInfo = new PARTY_XBL_CHAT_PERMISSION_INFO(chatPermissionInfo2);
		}
		return num;
	}

	public static uint PartyXblLocalChatUserGetCrossNetworkCommunicationPrivacySetting(PARTY_XBL_CHAT_USER_HANDLE handle, out PARTY_XBL_CROSS_NETWORK_COMMUNICATION_PRIVACY_SETTING setting)
	{
		setting = PARTY_XBL_CROSS_NETWORK_COMMUNICATION_PRIVACY_SETTING.PARTY_XBL_CROSS_NETWORK_COMMUNICATION_PRIVACY_SETTING_ALLOWED;
		if (handle == null)
		{
			return 4u;
		}
		return PartyXblInterop.PartyXblLocalChatUserGetCrossNetworkCommunicationPrivacySetting(handle.InteropHandle, out setting);
	}

	public static uint PartyXblGetErrorMessage(uint error, out string errorMessage)
	{
		UTF8StringPtr errorMessage2;
		uint num = PartyXblInterop.PartyXblGetErrorMessage(error, out errorMessage2);
		if (PartyError.SUCCEEDED(num))
		{
			errorMessage = errorMessage2.GetString();
			return num;
		}
		errorMessage = null;
		return num;
	}

	public static uint PartyXblSetThreadAffinityMask(PARTY_XBL_THREAD_ID threadId, ulong threadAffinityMask)
	{
		return PartyXblInterop.PartyXblSetThreadAffinityMask(threadId, threadAffinityMask);
	}

	public static uint PartyXblGetThreadAffinityMask(PARTY_XBL_THREAD_ID threadId, out ulong threadAffinityMask)
	{
		return PartyXblInterop.PartyXblGetThreadAffinityMask(threadId, out threadAffinityMask);
	}

	public static uint PartyXblInitialize(string titleId, out PARTY_XBL_HANDLE handle)
	{
		PartyXBLCSharpSDK.Interop.PARTY_XBL_HANDLE handle2;
		return PARTY_XBL_HANDLE.WrapAndReturnError(PartyXblInterop.PartyXblInitialize(IntPtr.Zero, Converters.StringToNullTerminatedUTF8ByteArray(titleId), out handle2), handle2, out handle);
	}

	public static uint PartyXblCleanup(PARTY_XBL_HANDLE handle)
	{
		return PartyXblInterop.PartyXblCleanup(handle.InteropHandle);
	}

	public unsafe static uint PartyXblStartProcessingStateChanges(PARTY_XBL_HANDLE handle, out List<PARTY_XBL_STATE_CHANGE> stateChanges)
	{
		stateChanges = null;
		if (handle == null)
		{
			return 4u;
		}
		stateChanges = objectPool.Retrieve<List<PARTY_XBL_STATE_CHANGE>>();
		uint stateChangeCount;
		IntPtr stateChanges2;
		uint num = PartyXblInterop.PartyXblStartProcessingStateChanges(handle.InteropHandle, out stateChangeCount, out stateChanges2);
		if (PartyError.SUCCEEDED(num) && stateChangeCount != 0)
		{
			List<PARTY_XBL_STATE_CHANGE> list = null;
			IntPtr* ptr = (IntPtr*)stateChanges2.ToPointer();
			for (int i = 0; i < stateChangeCount; i++)
			{
				PARTY_XBL_STATE_CHANGE pARTY_XBL_STATE_CHANGE = PARTY_XBL_STATE_CHANGE.CreateFromPtr(ptr[i]);
				if (pARTY_XBL_STATE_CHANGE.GetType() != typeof(PARTY_XBL_STATE_CHANGE))
				{
					stateChanges.Add(pARTY_XBL_STATE_CHANGE);
					continue;
				}
				if (list == null)
				{
					list = objectPool.Retrieve<List<PARTY_XBL_STATE_CHANGE>>();
				}
				list.Add(pARTY_XBL_STATE_CHANGE);
			}
			if (list != null)
			{
				num = PartyXblFinishProcessingStateChanges(handle, list);
			}
		}
		return num;
	}

	public unsafe static uint PartyXblFinishProcessingStateChanges(PARTY_XBL_HANDLE handle, List<PARTY_XBL_STATE_CHANGE> stateChanges)
	{
		if (handle == null)
		{
			return 4u;
		}
		IntPtr* ptr = stackalloc IntPtr[stateChanges.Count];
		for (int i = 0; i < stateChanges.Count; i++)
		{
			ptr[i] = stateChanges[i].StateChangeId;
		}
		stateChanges.Clear();
		objectPool.Return(stateChanges);
		return PartyXblInterop.PartyXblFinishProcessingStateChanges(handle.InteropHandle, (uint)stateChanges.Count, new IntPtr(ptr));
	}

	public static uint PartyXblCreateLocalChatUser(PARTY_XBL_HANDLE handle, ulong xboxUserId, object asyncIdentifier, out PARTY_XBL_CHAT_USER_HANDLE localXboxLiveUser)
	{
		localXboxLiveUser = null;
		if (handle == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE localXboxLiveUser2;
		uint num = PartyXblInterop.PartyXblCreateLocalChatUser(handle.InteropHandle, xboxUserId, intPtr, out localXboxLiveUser2);
		if (PartyError.SUCCEEDED(num))
		{
			localXboxLiveUser = new PARTY_XBL_CHAT_USER_HANDLE(localXboxLiveUser2);
			return num;
		}
		if (intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyXblCompleteGetTokenAndSignatureRequest(PARTY_XBL_HANDLE handle, uint correlationId, bool succeeded, string token, string signature)
	{
		if (handle == null)
		{
			return 4u;
		}
		return PartyXblInterop.PartyXblCompleteGetTokenAndSignatureRequest(handle.InteropHandle, correlationId, Convert.ToByte(succeeded), Converters.StringToNullTerminatedUTF8ByteArray(token), Converters.StringToNullTerminatedUTF8ByteArray(signature));
	}

	public static uint PartyXblCreateRemoteChatUser(PARTY_XBL_HANDLE handle, ulong xboxUserId, out PARTY_XBL_CHAT_USER_HANDLE chatUser)
	{
		chatUser = null;
		if (handle == null)
		{
			return 4u;
		}
		PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE chatUser2;
		uint num = PartyXblInterop.PartyXblCreateRemoteChatUser(handle.InteropHandle, xboxUserId, out chatUser2);
		if (num == 20481)
		{
			num = 0u;
		}
		return PARTY_XBL_CHAT_USER_HANDLE.WrapAndReturnError(num, chatUser2, out chatUser);
	}

	public static uint PartyXblDestroyChatUser(PARTY_XBL_HANDLE handle, PARTY_XBL_CHAT_USER_HANDLE chatUser)
	{
		if (handle == null || chatUser == null)
		{
			return 4u;
		}
		return PartyXblInterop.PartyXblDestroyChatUser(handle.InteropHandle, chatUser.InteropHandle);
	}

	public static uint PartyXblGetChatUsers(PARTY_XBL_HANDLE handle, out PARTY_XBL_CHAT_USER_HANDLE[] chatUsers)
	{
		chatUsers = null;
		if (handle == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PartyXblInterop.PartyXblGetChatUsers, (PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE s) => new PARTY_XBL_CHAT_USER_HANDLE(s), handle.InteropHandle, out chatUsers);
	}

	public static uint PartyXblLoginToPlayFab(PARTY_XBL_CHAT_USER_HANDLE localChatUser, object asyncIdentifier)
	{
		if (localChatUser == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PartyXblInterop.PartyXblLoginToPlayFab(localChatUser.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyXblGetEntityIdsFromXboxLiveUserIds(PARTY_XBL_HANDLE handle, ulong[] xboxLiveUserIds, PARTY_XBL_CHAT_USER_HANDLE localChatUser, object asyncIdentifier)
	{
		if (handle == null || xboxLiveUserIds == null || localChatUser == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PartyXblInterop.PartyXblGetEntityIdsFromXboxLiveUserIds(handle.InteropHandle, (uint)xboxLiveUserIds.Length, xboxLiveUserIds, localChatUser.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}
}
