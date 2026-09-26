using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class SDK
{
	internal static ObjectPool objectPool;

	static SDK()
	{
		objectPool = new ObjectPool();
		objectPool.AddEntry<List<PARTY_STATE_CHANGE>>(4, new Type[0]);
		objectPool.AddEntry<List<PARTY_ENDPOINT_HANDLE>>(32, new Type[0]);
		objectPool.AddEntry<PARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE>(32, new Type[2]
		{
			typeof(PARTY_STATE_CHANGE_UNION),
			typeof(IntPtr)
		});
		objectPool.AddEntry<PARTY_NETWORK_HANDLE>(32, new Type[1] { typeof(PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE) });
		objectPool.AddEntry<PARTY_ENDPOINT_HANDLE>(64, new Type[1] { typeof(PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE) });
		objectPool.AddEntry<PARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE>(32, new Type[2]
		{
			typeof(PARTY_STATE_CHANGE_UNION),
			typeof(IntPtr)
		});
		objectPool.AddEntry<List<PARTY_CHAT_CONTROL_HANDLE>>(32, new Type[0]);
		objectPool.AddEntry<List<PARTY_TRANSLATION>>(32, new Type[0]);
		objectPool.AddEntry<PARTY_CHAT_CONTROL_HANDLE>(64, new Type[1] { typeof(PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE) });
		objectPool.AddEntry<PARTY_TRANSLATION>(64, new Type[1] { typeof(PartyCSharpSDK.Interop.PARTY_TRANSLATION) });
	}

	public static uint PartyInitialize(string titleId, out PARTY_HANDLE handle)
	{
		PartyCSharpSDK.Interop.PARTY_HANDLE handle2;
		return PARTY_HANDLE.WrapAndReturnError(PFPInterop.PartyInitialize(Converters.StringToNullTerminatedUTF8ByteArray(titleId), out handle2), handle2, out handle);
	}

	public static uint PartyCleanup(PARTY_HANDLE handle)
	{
		return PFPInterop.PartyCleanup(handle.InteropHandle);
	}

	public static uint PartyCreateLocalUser(PARTY_HANDLE handle, string entityId, string titlePlayerEntityToken, out PARTY_LOCAL_USER_HANDLE localUser)
	{
		localUser = null;
		if (handle == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE localUser2;
		return PARTY_LOCAL_USER_HANDLE.WrapAndReturnError(PFPInterop.PartyCreateLocalUser(handle.InteropHandle, Converters.StringToNullTerminatedUTF8ByteArray(entityId), Converters.StringToNullTerminatedUTF8ByteArray(titlePlayerEntityToken), out localUser2), localUser2, out localUser);
	}

	public unsafe static uint PartyCreateNewNetwork(PARTY_HANDLE handle, PARTY_LOCAL_USER_HANDLE localUser, PARTY_NETWORK_CONFIGURATION networkConfiguration, PARTY_REGION[] regions, PARTY_INVITATION_CONFIGURATION initialInvitationConfiguration, object asyncIdentifier, out PARTY_NETWORK_DESCRIPTOR networkDescriptor, out string appliedInitialInvitationIdentifier)
	{
		networkDescriptor = null;
		appliedInitialInvitationIdentifier = null;
		if (handle == null || localUser == null)
		{
			return 4u;
		}
		uint num;
		using (DisposableCollection disposableCollection = new DisposableCollection())
		{
			PartyCSharpSDK.Interop.PARTY_NETWORK_CONFIGURATION pARTY_NETWORK_CONFIGURATION = new PartyCSharpSDK.Interop.PARTY_NETWORK_CONFIGURATION(networkConfiguration);
			SizeT arrayCount;
			IntPtr regions2 = Converters.ClassArrayToPtr(regions, (PARTY_REGION x, DisposableCollection xc) => new PartyCSharpSDK.Interop.PARTY_REGION(x), disposableCollection, out arrayCount);
			PartyCSharpSDK.Interop.PARTY_INVITATION_CONFIGURATION pARTY_INVITATION_CONFIGURATION = new PartyCSharpSDK.Interop.PARTY_INVITATION_CONFIGURATION(initialInvitationConfiguration, disposableCollection);
			IntPtr intPtr = IntPtr.Zero;
			if (asyncIdentifier != null)
			{
				intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
			}
			IntPtr intPtr2 = disposableCollection.Add(new DisposableBuffer(128)).IntPtr;
			num = PFPInterop.PartyCreateNewNetwork(handle.InteropHandle, localUser.InteropHandle, &pARTY_NETWORK_CONFIGURATION, arrayCount.ToUInt32(), regions2, &pARTY_INVITATION_CONFIGURATION, intPtr, out var networkDescriptor2, intPtr2);
			if (PartyError.SUCCEEDED(num))
			{
				networkDescriptor = new PARTY_NETWORK_DESCRIPTOR(networkDescriptor2);
				appliedInitialInvitationIdentifier = Converters.PtrToStringUTF8(intPtr2);
			}
			else if (intPtr != IntPtr.Zero)
			{
				GCHandle.FromIntPtr(intPtr).Free();
			}
		}
		return num;
	}

	public unsafe static uint PartyConnectToNetwork(PARTY_HANDLE handle, PARTY_NETWORK_DESCRIPTOR networkDescriptor, object asyncIdentifier, out PARTY_NETWORK_HANDLE network)
	{
		network = null;
		if (handle == null || networkDescriptor == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR pARTY_NETWORK_DESCRIPTOR = new PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR(networkDescriptor);
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE network2;
		uint error = PFPInterop.PartyConnectToNetwork(handle.InteropHandle, &pARTY_NETWORK_DESCRIPTOR, intPtr, out network2);
		if (PartyError.FAILED(error) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return PARTY_NETWORK_HANDLE.WrapAndReturnError(error, network2, out network);
	}

	public unsafe static uint PartyStartProcessingStateChanges(PARTY_HANDLE handle, out List<PARTY_STATE_CHANGE> stateChanges)
	{
		stateChanges = null;
		if (handle == null)
		{
			return 4u;
		}
		stateChanges = objectPool.Retrieve<List<PARTY_STATE_CHANGE>>();
		uint stateChangeCount;
		IntPtr stateChanges2;
		uint num = PFPInterop.PartyStartProcessingStateChanges(handle.InteropHandle, out stateChangeCount, out stateChanges2);
		if (PartyError.SUCCEEDED(num) && stateChangeCount != 0)
		{
			List<PARTY_STATE_CHANGE> list = null;
			IntPtr* ptr = (IntPtr*)stateChanges2.ToPointer();
			for (int i = 0; i < stateChangeCount; i++)
			{
				PARTY_STATE_CHANGE pARTY_STATE_CHANGE = PARTY_STATE_CHANGE.CreateFromPtr(ptr[i]);
				if (pARTY_STATE_CHANGE.GetType() != typeof(PARTY_STATE_CHANGE))
				{
					stateChanges.Add(pARTY_STATE_CHANGE);
					continue;
				}
				if (list == null)
				{
					list = objectPool.Retrieve<List<PARTY_STATE_CHANGE>>();
				}
				list.Add(pARTY_STATE_CHANGE);
			}
			if (list != null)
			{
				num = PartyFinishProcessingStateChanges(handle, list);
			}
		}
		return num;
	}

	public unsafe static uint PartyFinishProcessingStateChanges(PARTY_HANDLE handle, List<PARTY_STATE_CHANGE> stateChanges)
	{
		if (handle == null)
		{
			return 4u;
		}
		uint count = (uint)stateChanges.Count;
		IntPtr* ptr = stackalloc IntPtr[stateChanges.Count];
		for (int i = 0; i < stateChanges.Count; i++)
		{
			ptr[i] = stateChanges[i].StateChangeId;
			stateChanges[i].Cleanup();
		}
		stateChanges.Clear();
		objectPool.Return(stateChanges);
		return PFPInterop.PartyFinishProcessingStateChanges(handle.InteropHandle, count, new IntPtr(ptr));
	}

	public static uint PartyDestroyLocalUser(PARTY_HANDLE handle, PARTY_LOCAL_USER_HANDLE localUser, object asyncIdentifier)
	{
		if (handle == null || localUser == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyDestroyLocalUser(handle.InteropHandle, localUser.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyGetErrorMessage(uint error, out string errorMessage)
	{
		UTF8StringPtr errorMessage2;
		uint num = PFPInterop.PartyGetErrorMessage(error, out errorMessage2);
		if (PartyError.SUCCEEDED(num))
		{
			errorMessage = errorMessage2.GetString();
			return num;
		}
		errorMessage = null;
		return num;
	}

	public static uint PartyGetRegions(PARTY_HANDLE handle, out PARTY_REGION[] regionList)
	{
		regionList = null;
		if (handle == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyGetRegions, (PartyCSharpSDK.Interop.PARTY_REGION s) => new PARTY_REGION(s), handle.InteropHandle, out regionList);
	}

	public static uint PartyGetLocalDevice(PARTY_HANDLE handle, out PARTY_DEVICE_HANDLE localDevice)
	{
		localDevice = null;
		if (handle == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_DEVICE_HANDLE localDevice2;
		return PARTY_DEVICE_HANDLE.WrapAndReturnError(PFPInterop.PartyGetLocalDevice(handle.InteropHandle, out localDevice2), localDevice2, out localDevice);
	}

	public static uint PartyGetLocalUsers(PARTY_HANDLE handle, out PARTY_LOCAL_USER_HANDLE[] users)
	{
		users = null;
		if (handle == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyGetLocalUsers, (PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE s) => new PARTY_LOCAL_USER_HANDLE(s), handle.InteropHandle, out users);
	}

	public static uint PartyLocalUserGetEntityId(PARTY_LOCAL_USER_HANDLE localUser, out string entityId)
	{
		entityId = null;
		if (localUser == null)
		{
			return 4u;
		}
		UTF8StringPtr entityId2;
		uint num = PFPInterop.PartyLocalUserGetEntityId(localUser.InteropHandle, out entityId2);
		if (PartyError.SUCCEEDED(num))
		{
			entityId = entityId2.GetString();
		}
		return num;
	}

	public static uint PartyLocalUserUpdateEntityToken(PARTY_LOCAL_USER_HANDLE localUser, string titlePlayerEntityToken)
	{
		if (localUser == null)
		{
			return 4u;
		}
		return PFPInterop.PartyLocalUserUpdateEntityToken(localUser.InteropHandle, Converters.StringToNullTerminatedUTF8ByteArray(titlePlayerEntityToken));
	}

	public static uint PartyLocalUserGetCustomContext(PARTY_LOCAL_USER_HANDLE localUser, out object customContext)
	{
		if (localUser == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyLocalUserGetCustomContext, localUser.InteropHandle, out customContext);
	}

	public static uint PartyLocalUserSetCustomContext(PARTY_LOCAL_USER_HANDLE localUser, object customContext)
	{
		if (localUser == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyLocalUserGetCustomContext, PFPInterop.PartyLocalUserSetCustomContext, localUser.InteropHandle, customContext);
	}

	public static uint PartyGetNetworks(PARTY_HANDLE handle, out PARTY_NETWORK_HANDLE[] networks)
	{
		networks = null;
		if (handle == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyGetNetworks, (PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE s) => new PARTY_NETWORK_HANDLE(s), handle.InteropHandle, out networks);
	}

	public static uint PartySetOption(object contextObject, PARTY_OPTION option, object value)
	{
		uint result = 4305u;
		if (option == PARTY_OPTION.PARTY_OPTION_LOCAL_UDP_SOCKET_BIND_ADDRESS)
		{
			using DisposableCollection disposableCollection = new DisposableCollection();
			if (value != null && value.GetType() == typeof(PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION))
			{
				SizeT arrayCount;
				IntPtr value2 = Converters.ClassArrayToPtr(new PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION[1] { (PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION)value }, (PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION x, DisposableCollection d) => new PartyCSharpSDK.Interop.PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION(x), disposableCollection, out arrayCount);
				result = PFPInterop.PartySetOption(IntPtr.Zero, option, value2);
			}
			else
			{
				result = 4u;
			}
		}
		return result;
	}

	public unsafe static uint PartyGetOption(object contextObject, PARTY_OPTION option, out object value)
	{
		uint num = 4305u;
		value = null;
		if (option == PARTY_OPTION.PARTY_OPTION_LOCAL_UDP_SOCKET_BIND_ADDRESS)
		{
			PartyCSharpSDK.Interop.PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION interopStruct = default(PartyCSharpSDK.Interop.PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION);
			num = PFPInterop.PartyGetOption(IntPtr.Zero, option, (IntPtr)(&interopStruct));
			if (PartyError.SUCCEEDED(num))
			{
				value = new PARTY_LOCAL_UDP_SOCKET_BIND_ADDRESS_CONFIGURATION(interopStruct);
			}
		}
		return num;
	}

	public static uint PartySetThreadAffinityMask(PARTY_THREAD_ID threadId, ulong threadAffinityMask)
	{
		return PFPInterop.PartySetThreadAffinityMask(threadId, threadAffinityMask);
	}

	public static uint PartyGetThreadAffinityMask(PARTY_THREAD_ID threadId, out ulong threadAffinityMask)
	{
		return PFPInterop.PartyGetThreadAffinityMask(threadId, out threadAffinityMask);
	}

	public static uint PartySetWorkMode(PARTY_THREAD_ID threadId, PARTY_WORK_MODE workMode)
	{
		return PFPInterop.PartySetWorkMode(threadId, workMode);
	}

	public static uint PartyGetWorkMode(PARTY_THREAD_ID threadId, out PARTY_WORK_MODE workMode)
	{
		return PFPInterop.PartyGetWorkMode(threadId, out workMode);
	}

	public static uint PartyDoWork(PARTY_HANDLE handle, PARTY_THREAD_ID threadId)
	{
		return PFPInterop.PartyDoWork(handle.InteropHandle, threadId);
	}

	public static uint PartyGetChatControls(PARTY_HANDLE handle, out PARTY_CHAT_CONTROL_HANDLE[] chatControls)
	{
		chatControls = null;
		if (handle == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyGetChatControls, (PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE s) => new PARTY_CHAT_CONTROL_HANDLE(s), handle.InteropHandle, out chatControls);
	}

	public static uint PartyNetworkAuthenticateLocalUser(PARTY_NETWORK_HANDLE network, PARTY_LOCAL_USER_HANDLE localUser, string invitationIdentifier, object asyncIdentifier)
	{
		if (network == null || localUser == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkAuthenticateLocalUser(network.InteropHandle, localUser.InteropHandle, Converters.StringToNullTerminatedUTF8ByteArray(invitationIdentifier), intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkGetNetworkDescriptor(PARTY_NETWORK_HANDLE network, out PARTY_NETWORK_DESCRIPTOR networkDescriptor)
	{
		networkDescriptor = null;
		if (network == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR networkDescriptor2;
		uint num = PFPInterop.PartyNetworkGetNetworkDescriptor(network.InteropHandle, out networkDescriptor2);
		if (PartyError.SUCCEEDED(num))
		{
			networkDescriptor = new PARTY_NETWORK_DESCRIPTOR(networkDescriptor2);
		}
		return num;
	}

	public unsafe static uint PartyNetworkCreateInvitation(PARTY_NETWORK_HANDLE network, PARTY_LOCAL_USER_HANDLE localUser, PARTY_INVITATION_CONFIGURATION invitationConfiguration, object asyncIdentifier, out PARTY_INVITATION_HANDLE invitation)
	{
		invitation = null;
		if (network == null || localUser == null)
		{
			return 4u;
		}
		using DisposableCollection disposableCollection = new DisposableCollection();
		PartyCSharpSDK.Interop.PARTY_INVITATION_CONFIGURATION pARTY_INVITATION_CONFIGURATION = new PartyCSharpSDK.Interop.PARTY_INVITATION_CONFIGURATION(invitationConfiguration, disposableCollection);
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		PartyCSharpSDK.Interop.PARTY_INVITATION_HANDLE invitation2;
		uint num = PFPInterop.PartyNetworkCreateInvitation(network.InteropHandle, localUser.InteropHandle, &pARTY_INVITATION_CONFIGURATION, intPtr, out invitation2);
		if (PartyError.SUCCEEDED(num))
		{
			invitation = new PARTY_INVITATION_HANDLE(invitation2);
		}
		else if (intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkDestroyEndpoint(PARTY_NETWORK_HANDLE network, PARTY_ENDPOINT_HANDLE localEndpoint, object asyncIdentifier)
	{
		if (network == null || localEndpoint == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkDestroyEndpoint(network.InteropHandle, localEndpoint.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkGetChatControls(PARTY_NETWORK_HANDLE network, out PARTY_CHAT_CONTROL_HANDLE[] chatControls)
	{
		chatControls = null;
		if (network == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyNetworkGetChatControls, (PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE s) => new PARTY_CHAT_CONTROL_HANDLE(s), network.InteropHandle, out chatControls);
	}

	public static uint PartyNetworkFindEndpointByUniqueIdentifier(PARTY_NETWORK_HANDLE network, ushort uniqueIdentifier, out PARTY_ENDPOINT_HANDLE endpoint)
	{
		endpoint = null;
		if (network == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE endpoint2;
		return PARTY_ENDPOINT_HANDLE.WrapAndReturnError(PFPInterop.PartyNetworkFindEndpointByUniqueIdentifier(network.InteropHandle, uniqueIdentifier, out endpoint2), endpoint2, out endpoint);
	}

	public static uint PartyNetworkGetCustomContext(PARTY_NETWORK_HANDLE network, out object customContext)
	{
		if (network == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyNetworkGetCustomContext, network.InteropHandle, out customContext);
	}

	public static uint PartyNetworkSetCustomContext(PARTY_NETWORK_HANDLE network, object customContext)
	{
		if (network == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyNetworkGetCustomContext, PFPInterop.PartyNetworkSetCustomContext, network.InteropHandle, customContext);
	}

	public static uint PartyNetworkGetDevices(PARTY_NETWORK_HANDLE network, out PARTY_DEVICE_HANDLE[] devices)
	{
		devices = null;
		if (network == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyNetworkGetDevices, (PartyCSharpSDK.Interop.PARTY_DEVICE_HANDLE s) => new PARTY_DEVICE_HANDLE(s), network.InteropHandle, out devices);
	}

	public static uint PartyNetworkGetEndpoints(PARTY_NETWORK_HANDLE network, out PARTY_ENDPOINT_HANDLE[] endpoints)
	{
		endpoints = null;
		if (network == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyNetworkGetEndpoints, (PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE s) => new PARTY_ENDPOINT_HANDLE(s), network.InteropHandle, out endpoints);
	}

	public static uint PartyNetworkGetInvitations(PARTY_NETWORK_HANDLE network, out PARTY_INVITATION_HANDLE[] invitations)
	{
		invitations = null;
		if (network == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyNetworkGetInvitations, (PartyCSharpSDK.Interop.PARTY_INVITATION_HANDLE s) => new PARTY_INVITATION_HANDLE(s), network.InteropHandle, out invitations);
	}

	public static uint PartyNetworkGetLocalUsers(PARTY_NETWORK_HANDLE network, out PARTY_LOCAL_USER_HANDLE[] users)
	{
		users = null;
		if (network == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyNetworkGetLocalUsers, (PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE s) => new PARTY_LOCAL_USER_HANDLE(s), network.InteropHandle, out users);
	}

	public static uint PartyNetworkGetNetworkConfiguration(PARTY_NETWORK_HANDLE network, out PARTY_NETWORK_CONFIGURATION networkConfiguration)
	{
		networkConfiguration = null;
		if (network == null)
		{
			return 4u;
		}
		IntPtr networkConfiguration2;
		uint num = PFPInterop.PartyNetworkGetNetworkConfiguration(network.InteropHandle, out networkConfiguration2);
		if (PartyError.SUCCEEDED(num))
		{
			networkConfiguration = Converters.PtrToClass(networkConfiguration2, (PartyCSharpSDK.Interop.PARTY_NETWORK_CONFIGURATION s) => new PARTY_NETWORK_CONFIGURATION(s));
		}
		return num;
	}

	public static uint PartyNetworkGetNetworkStatistics(PARTY_NETWORK_HANDLE network, PARTY_NETWORK_STATISTIC[] statisticTypes, out ulong[] statisticValues)
	{
		statisticValues = new ulong[statisticTypes.Length];
		if (network == null)
		{
			return 4u;
		}
		return PFPInterop.PartyNetworkGetNetworkStatistics(network.InteropHandle, (uint)statisticTypes.Length, statisticTypes, statisticValues);
	}

	public static uint PartyNetworkKickDevice(PARTY_NETWORK_HANDLE network, PARTY_DEVICE_HANDLE targetDevice, object asyncIdentifier)
	{
		if (network == null || targetDevice == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkKickDevice(network.InteropHandle, targetDevice.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkKickUser(PARTY_NETWORK_HANDLE network, string targetEntityId, object asyncIdentifier)
	{
		if (network == null || targetEntityId == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkKickUser(network.InteropHandle, Converters.StringToNullTerminatedUTF8ByteArray(targetEntityId), intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkLeaveNetwork(PARTY_NETWORK_HANDLE network, object asyncIdentifier)
	{
		if (network == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkLeaveNetwork(network.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkRemoveLocalUser(PARTY_NETWORK_HANDLE network, PARTY_LOCAL_USER_HANDLE localUser, object asyncIdentifier)
	{
		if (network == null || localUser == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkRemoveLocalUser(network.InteropHandle, localUser.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkRevokeInvitation(PARTY_NETWORK_HANDLE network, PARTY_LOCAL_USER_HANDLE localUser, PARTY_INVITATION_HANDLE invitation, object asyncIdentifier)
	{
		if (network == null || localUser == null || invitation == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkRevokeInvitation(network.InteropHandle, localUser.InteropHandle, invitation.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public unsafe static uint PartySerializeNetworkDescriptor(PARTY_NETWORK_DESCRIPTOR networkDescriptor, out string serializedNetworkDescriptorString)
	{
		serializedNetworkDescriptorString = null;
		if (networkDescriptor == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR pARTY_NETWORK_DESCRIPTOR = new PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR(networkDescriptor);
		uint num;
		using (DisposableBuffer disposableBuffer = new DisposableBuffer(449))
		{
			IntPtr intPtr = disposableBuffer.IntPtr;
			num = PFPInterop.PartySerializeNetworkDescriptor(&pARTY_NETWORK_DESCRIPTOR, intPtr);
			if (PartyError.SUCCEEDED(num))
			{
				serializedNetworkDescriptorString = Converters.PtrToStringUTF8(intPtr);
			}
		}
		return num;
	}

	public static uint PartyDeserializeNetworkDescriptor(string serializedNetworkDescriptorString, out PARTY_NETWORK_DESCRIPTOR networkDescriptor)
	{
		networkDescriptor = null;
		if (serializedNetworkDescriptorString == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR networkDescriptor2;
		uint num = PFPInterop.PartyDeserializeNetworkDescriptor(Converters.StringToNullTerminatedUTF8ByteArray(serializedNetworkDescriptorString), out networkDescriptor2);
		if (PartyError.SUCCEEDED(num))
		{
			networkDescriptor = new PARTY_NETWORK_DESCRIPTOR(networkDescriptor2);
		}
		return num;
	}

	public static uint PartyNetworkConnectChatControl(PARTY_NETWORK_HANDLE network, PARTY_CHAT_CONTROL_HANDLE chatControl, object asyncIdentifier)
	{
		if (network == null || chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkConnectChatControl(network.InteropHandle, chatControl.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyNetworkDisconnectChatControl(PARTY_NETWORK_HANDLE network, PARTY_CHAT_CONTROL_HANDLE chatControl, object asyncIdentifier)
	{
		if (network == null || chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyNetworkDisconnectChatControl(network.InteropHandle, chatControl.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyInvitationGetCreatorEntityId(PARTY_INVITATION_HANDLE invitation, out string entityId)
	{
		entityId = null;
		if (invitation == null)
		{
			return 4u;
		}
		UTF8StringPtr entityId2;
		uint num = PFPInterop.PartyInvitationGetCreatorEntityId(invitation.InteropHandle, out entityId2);
		if (PartyError.SUCCEEDED(num))
		{
			entityId = entityId2.GetString();
		}
		return num;
	}

	public static uint PartyInvitationGetInvitationConfiguration(PARTY_INVITATION_HANDLE invitation, out PARTY_INVITATION_CONFIGURATION configuration)
	{
		configuration = null;
		if (invitation == null)
		{
			return 4u;
		}
		IntPtr configuration2;
		uint num = PFPInterop.PartyInvitationGetInvitationConfiguration(invitation.InteropHandle, out configuration2);
		if (PartyError.SUCCEEDED(num))
		{
			configuration = Converters.PtrToClass(configuration2, (PartyCSharpSDK.Interop.PARTY_INVITATION_CONFIGURATION s) => new PARTY_INVITATION_CONFIGURATION(s));
		}
		return num;
	}

	public static uint PartyInvitationGetCustomContext(PARTY_INVITATION_HANDLE invitation, out object customContext)
	{
		if (invitation == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyInvitationGetCustomContext, invitation.InteropHandle, out customContext);
	}

	public static uint PartyInvitationSetCustomContext(PARTY_INVITATION_HANDLE invitation, object customContext)
	{
		if (invitation == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyInvitationGetCustomContext, PFPInterop.PartyInvitationSetCustomContext, invitation.InteropHandle, customContext);
	}

	public static uint PartyNetworkCreateEndpoint(PARTY_NETWORK_HANDLE network, PARTY_LOCAL_USER_HANDLE localUser, Dictionary<string, byte[]> keyValuePairs, object asyncIdentifier, out PARTY_ENDPOINT_HANDLE endpoint)
	{
		endpoint = null;
		if (network == null || localUser == null)
		{
			return 4u;
		}
		uint num2;
		using (DisposableCollection disposableCollection = new DisposableCollection())
		{
			uint num = 0u;
			IntPtr keys = IntPtr.Zero;
			IntPtr values = IntPtr.Zero;
			if (keyValuePairs != null)
			{
				num = (uint)keyValuePairs.Count;
				if (num != 0)
				{
					List<string> list = new List<string>();
					List<byte[]> list2 = new List<byte[]>();
					foreach (KeyValuePair<string, byte[]> keyValuePair in keyValuePairs)
					{
						list.Add(keyValuePair.Key);
						list2.Add(keyValuePair.Value);
					}
					keys = Converters.ClassArrayToPtr(list.ToArray(), (string x, DisposableCollection d) => new UTF8StringPtr(x, d), disposableCollection, out var arrayCount);
					values = Converters.ClassArrayToPtr(list2.ToArray(), (byte[] x, DisposableCollection d) => new PARTY_DATA_BUFFER(x, d), disposableCollection, out arrayCount);
				}
			}
			IntPtr intPtr = IntPtr.Zero;
			if (asyncIdentifier != null)
			{
				intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
			}
			num2 = PFPInterop.PartyNetworkCreateEndpoint(network.InteropHandle, localUser.InteropHandle, num, keys, values, intPtr, out var endpoint2);
			if (PartyError.SUCCEEDED(num2))
			{
				endpoint = new PARTY_ENDPOINT_HANDLE(endpoint2);
			}
			else if (intPtr != IntPtr.Zero)
			{
				GCHandle.FromIntPtr(intPtr).Free();
			}
		}
		return num2;
	}

	public static uint PartyNetworkGetDeviceConnectionType(PARTY_NETWORK_HANDLE network, PARTY_DEVICE_HANDLE targetDevice, out PARTY_DEVICE_CONNECTION_TYPE deviceConnectionType)
	{
		return PFPInterop.PartyNetworkGetDeviceConnectionType(network.InteropHandle, targetDevice.InteropHandle, out deviceConnectionType);
	}

	public static uint PartyEndpointSendMessage(PARTY_ENDPOINT_HANDLE endpoint, PARTY_ENDPOINT_HANDLE[] targetEndpoints, PARTY_SEND_MESSAGE_OPTIONS options, PARTY_SEND_MESSAGE_QUEUING_CONFIGURATION queuingConfiguration, byte[] dataBuffer)
	{
		if (dataBuffer == null)
		{
			return 4u;
		}
		GCHandle gCHandle = GCHandle.Alloc(dataBuffer, GCHandleType.Pinned);
		uint result = PartyEndpointSendMessage(endpoint, targetEndpoints, options, queuingConfiguration, gCHandle.AddrOfPinnedObject(), (uint)dataBuffer.Length);
		gCHandle.Free();
		return result;
	}

	public unsafe static uint PartyEndpointSendMessage(PARTY_ENDPOINT_HANDLE endpoint, PARTY_ENDPOINT_HANDLE[] targetEndpoints, PARTY_SEND_MESSAGE_OPTIONS options, PARTY_SEND_MESSAGE_QUEUING_CONFIGURATION queuingConfiguration, IntPtr dataBuffer, uint dataBufferSize)
	{
		if (endpoint == null || queuingConfiguration == null)
		{
			return 4u;
		}
		uint targetEndpointCount = 0u;
		IntPtr targetEndpoints2 = IntPtr.Zero;
		if (targetEndpoints != null)
		{
			IntPtr* ptr = stackalloc IntPtr[targetEndpoints.Length];
			for (int i = 0; i < targetEndpoints.Length; i++)
			{
				ptr[i] = targetEndpoints[i].InteropHandle.handle;
			}
			targetEndpoints2 = new IntPtr(ptr);
			targetEndpointCount = (uint)targetEndpoints.Length;
		}
		PartyCSharpSDK.Interop.PARTY_SEND_MESSAGE_QUEUING_CONFIGURATION pARTY_SEND_MESSAGE_QUEUING_CONFIGURATION = new PartyCSharpSDK.Interop.PARTY_SEND_MESSAGE_QUEUING_CONFIGURATION(queuingConfiguration);
		PARTY_DATA_BUFFER pARTY_DATA_BUFFER = new PARTY_DATA_BUFFER(dataBuffer, dataBufferSize);
		return PFPInterop.PartyEndpointSendMessage(endpoint.InteropHandle, targetEndpointCount, targetEndpoints2, options, &pARTY_SEND_MESSAGE_QUEUING_CONFIGURATION, 1u, &pARTY_DATA_BUFFER, IntPtr.Zero);
	}

	public static uint PartyEndpointCancelMessages(PARTY_ENDPOINT_HANDLE endpoint, PARTY_ENDPOINT_HANDLE[] targetEndpoints, PARTY_CANCEL_MESSAGES_FILTER_EXPRESSION filterExpression, uint messageIdentityFilterMask, uint filteredMessageIdentitiesToMatch, out uint canceledMessagesCount)
	{
		canceledMessagesCount = 0u;
		if (endpoint == null)
		{
			return 4u;
		}
		using DisposableCollection disposableCollection = new DisposableCollection();
		SizeT arrayCount;
		IntPtr targetEndpoints2 = Converters.ClassArrayToPtr(targetEndpoints, (PARTY_ENDPOINT_HANDLE x, DisposableCollection d) => x.InteropHandle, disposableCollection, out arrayCount);
		return PFPInterop.PartyEndpointCancelMessages(endpoint.InteropHandle, arrayCount.ToUInt32(), targetEndpoints2, filterExpression, messageIdentityFilterMask, filteredMessageIdentitiesToMatch, out canceledMessagesCount);
	}

	public static uint PartyEndpointFlushMessages(PARTY_ENDPOINT_HANDLE endpoint, PARTY_ENDPOINT_HANDLE[] targetEndpoints)
	{
		if (endpoint == null)
		{
			return 4u;
		}
		using DisposableCollection disposableCollection = new DisposableCollection();
		SizeT arrayCount;
		IntPtr targetEndpoints2 = Converters.ClassArrayToPtr(targetEndpoints, (PARTY_ENDPOINT_HANDLE x, DisposableCollection d) => x.InteropHandle, disposableCollection, out arrayCount);
		return PFPInterop.PartyEndpointFlushMessages(endpoint.InteropHandle, arrayCount.ToUInt32(), targetEndpoints2);
	}

	public static uint PartyEndpointGetEndpointStatistics(PARTY_ENDPOINT_HANDLE endpoint, PARTY_ENDPOINT_HANDLE[] targetEndpoints, PARTY_ENDPOINT_STATISTIC[] statisticTypes, out ulong[] statisticValues)
	{
		statisticValues = new ulong[statisticTypes.Length];
		if (endpoint == null)
		{
			return 4u;
		}
		using DisposableCollection disposableCollection = new DisposableCollection();
		SizeT arrayCount;
		IntPtr targetEndpoints2 = Converters.ClassArrayToPtr(targetEndpoints, (PARTY_ENDPOINT_HANDLE x, DisposableCollection d) => x.InteropHandle, disposableCollection, out arrayCount);
		return PFPInterop.PartyEndpointGetEndpointStatistics(endpoint.InteropHandle, arrayCount.ToUInt32(), targetEndpoints2, (uint)statisticTypes.Length, statisticTypes, statisticValues);
	}

	public static uint PartyEndpointGetCustomContext(PARTY_ENDPOINT_HANDLE endpoint, out object customContext)
	{
		if (endpoint == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyEndpointGetCustomContext, endpoint.InteropHandle, out customContext);
	}

	public static uint PartyEndpointSetCustomContext(PARTY_ENDPOINT_HANDLE endpoint, object customContext)
	{
		if (endpoint == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyEndpointGetCustomContext, PFPInterop.PartyEndpointSetCustomContext, endpoint.InteropHandle, customContext);
	}

	public static uint PartyEndpointGetDevice(PARTY_ENDPOINT_HANDLE endpoint, out PARTY_DEVICE_HANDLE device)
	{
		device = null;
		if (endpoint == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_DEVICE_HANDLE device2;
		return PARTY_DEVICE_HANDLE.WrapAndReturnError(PFPInterop.PartyEndpointGetDevice(endpoint.InteropHandle, out device2), device2, out device);
	}

	public static uint PartyEndpointGetEntityId(PARTY_ENDPOINT_HANDLE endpoint, out string entityId)
	{
		entityId = null;
		if (endpoint == null)
		{
			return 4u;
		}
		UTF8StringPtr entityId2;
		uint num = PFPInterop.PartyEndpointGetEntityId(endpoint.InteropHandle, out entityId2);
		if (PartyError.SUCCEEDED(num))
		{
			entityId = entityId2.GetString();
		}
		return num;
	}

	public static uint PartyEndpointGetLocalUser(PARTY_ENDPOINT_HANDLE endpoint, out PARTY_LOCAL_USER_HANDLE localUser)
	{
		localUser = null;
		if (endpoint == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE localUser2;
		return PARTY_LOCAL_USER_HANDLE.WrapAndReturnError(PFPInterop.PartyEndpointGetLocalUser(endpoint.InteropHandle, out localUser2), localUser2, out localUser);
	}

	public static uint PartyEndpointGetNetwork(PARTY_ENDPOINT_HANDLE endpoint, out PARTY_NETWORK_HANDLE network)
	{
		network = null;
		if (endpoint == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE network2;
		return PARTY_NETWORK_HANDLE.WrapAndReturnError(PFPInterop.PartyEndpointGetNetwork(endpoint.InteropHandle, out network2), network2, out network);
	}

	public static uint PartyEndpointGetUniqueIdentifier(PARTY_ENDPOINT_HANDLE endpoint, out ushort uniqueIdentifier)
	{
		uniqueIdentifier = 0;
		if (endpoint == null)
		{
			return 4u;
		}
		return PFPInterop.PartyEndpointGetUniqueIdentifier(endpoint.InteropHandle, out uniqueIdentifier);
	}

	public static uint PartyEndpointIsLocal(PARTY_ENDPOINT_HANDLE endpoint, out bool isLocal)
	{
		isLocal = false;
		if (endpoint == null)
		{
			return 4u;
		}
		byte isLocal2;
		uint num = PFPInterop.PartyEndpointIsLocal(endpoint.InteropHandle, out isLocal2);
		if (PartyError.SUCCEEDED(num))
		{
			isLocal = isLocal2 != 0;
		}
		return num;
	}

	public static uint PartyDeviceIsLocal(PARTY_DEVICE_HANDLE device, out bool isLocal)
	{
		isLocal = false;
		if (device == null)
		{
			return 4u;
		}
		byte isLocal2;
		uint num = PFPInterop.PartyDeviceIsLocal(device.InteropHandle, out isLocal2);
		if (PartyError.SUCCEEDED(num))
		{
			isLocal = isLocal2 != 0;
		}
		return num;
	}

	public static uint PartyDeviceGetCustomContext(PARTY_DEVICE_HANDLE device, out object customContext)
	{
		if (device == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyDeviceGetCustomContext, device.InteropHandle, out customContext);
	}

	public static uint PartyDeviceSetCustomContext(PARTY_DEVICE_HANDLE device, object customContext)
	{
		if (device == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyDeviceGetCustomContext, PFPInterop.PartyDeviceSetCustomContext, device.InteropHandle, customContext);
	}

	public static uint PartyDeviceCreateChatControl(PARTY_DEVICE_HANDLE device, PARTY_LOCAL_USER_HANDLE localUser, string languageCode, object asyncIdentifier, out PARTY_CHAT_CONTROL_HANDLE chatControl)
	{
		chatControl = null;
		if (device == null || localUser == null)
		{
			return 4u;
		}
		byte[] languageCode2 = null;
		if (!string.IsNullOrEmpty(languageCode))
		{
			languageCode2 = Converters.StringToNullTerminatedUTF8ByteArray(languageCode);
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE chatControl2;
		uint num = PFPInterop.PartyDeviceCreateChatControl(device.InteropHandle, localUser.InteropHandle, languageCode2, intPtr, out chatControl2);
		if (PartyError.SUCCEEDED(num))
		{
			chatControl = new PARTY_CHAT_CONTROL_HANDLE(chatControl2);
			return num;
		}
		if (intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyDeviceDestroyChatControl(PARTY_DEVICE_HANDLE device, PARTY_CHAT_CONTROL_HANDLE chatControl, object asyncIdentifier)
	{
		if (device == null || chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyDeviceDestroyChatControl(device.InteropHandle, chatControl.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyDeviceGetChatControls(PARTY_DEVICE_HANDLE device, out PARTY_CHAT_CONTROL_HANDLE[] chatControls)
	{
		chatControls = null;
		if (device == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyDeviceGetChatControls, (PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE s) => new PARTY_CHAT_CONTROL_HANDLE(s), device.InteropHandle, out chatControls);
	}

	public static uint PartyChatControlSendText(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE[] targetChatControls, string chatText, byte[][] dataBuffers)
	{
		if (chatControl == null || targetChatControls == null || chatText == null)
		{
			return 4u;
		}
		DisposableCollection dc = new DisposableCollection();
		try
		{
			SizeT arrayCount;
			IntPtr targetChatControls2 = Converters.ClassArrayToPtr(targetChatControls, (PARTY_CHAT_CONTROL_HANDLE x, DisposableCollection d) => x.InteropHandle, dc, out arrayCount);
			SizeT arrayCount2;
			IntPtr dataBuffers2 = Converters.ClassArrayToPtr(dataBuffers, (byte[] x, DisposableCollection d) => new PARTY_DATA_BUFFER(x, dc), dc, out arrayCount2);
			return PFPInterop.PartyChatControlSendText(chatControl.InteropHandle, arrayCount.ToUInt32(), targetChatControls2, Converters.StringToNullTerminatedUTF8ByteArray(chatText), arrayCount2.ToUInt32(), dataBuffers2);
		}
		finally
		{
			if (dc != null)
			{
				((IDisposable)dc).Dispose();
			}
		}
	}

	public static uint PartyChatControlSetAudioInput(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_AUDIO_DEVICE_SELECTION_TYPE audioDeviceSelectionType, string audioDeviceSelectionContext, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		byte[] audioDeviceSelectionContext2 = null;
		if (!string.IsNullOrEmpty(audioDeviceSelectionContext))
		{
			audioDeviceSelectionContext2 = Converters.StringToNullTerminatedUTF8ByteArray(audioDeviceSelectionContext);
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetAudioInput(chatControl.InteropHandle, audioDeviceSelectionType, audioDeviceSelectionContext2, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetAudioInput(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_AUDIO_DEVICE_SELECTION_TYPE audioDeviceSelectionType, out string audioDeviceSelectionContext, out string deviceId)
	{
		audioDeviceSelectionType = PARTY_AUDIO_DEVICE_SELECTION_TYPE.PARTY_AUDIO_DEVICE_SELECTION_TYPE_NONE;
		audioDeviceSelectionContext = null;
		deviceId = null;
		if (chatControl == null)
		{
			return 4u;
		}
		UTF8StringPtr audioDeviceSelectionContext2;
		UTF8StringPtr deviceId2;
		uint num = PFPInterop.PartyChatControlGetAudioInput(chatControl.InteropHandle, out audioDeviceSelectionType, out audioDeviceSelectionContext2, out deviceId2);
		if (PartyError.SUCCEEDED(num))
		{
			audioDeviceSelectionContext = audioDeviceSelectionContext2.GetString();
			deviceId = deviceId2.GetString();
		}
		return num;
	}

	public static uint PartyChatControlSetAudioOutput(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_AUDIO_DEVICE_SELECTION_TYPE audioDeviceSelectionType, string audioDeviceSelectionContext, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		byte[] audioDeviceSelectionContext2 = null;
		if (!string.IsNullOrEmpty(audioDeviceSelectionContext))
		{
			audioDeviceSelectionContext2 = Converters.StringToNullTerminatedUTF8ByteArray(audioDeviceSelectionContext);
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetAudioOutput(chatControl.InteropHandle, audioDeviceSelectionType, audioDeviceSelectionContext2, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetAudioOutput(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_AUDIO_DEVICE_SELECTION_TYPE audioDeviceSelectionType, out string audioDeviceSelectionContext, out string deviceId)
	{
		audioDeviceSelectionType = PARTY_AUDIO_DEVICE_SELECTION_TYPE.PARTY_AUDIO_DEVICE_SELECTION_TYPE_NONE;
		audioDeviceSelectionContext = null;
		deviceId = null;
		if (chatControl == null)
		{
			return 4u;
		}
		UTF8StringPtr audioDeviceSelectionContext2;
		UTF8StringPtr deviceId2;
		uint num = PFPInterop.PartyChatControlGetAudioOutput(chatControl.InteropHandle, out audioDeviceSelectionType, out audioDeviceSelectionContext2, out deviceId2);
		if (PartyError.SUCCEEDED(num))
		{
			audioDeviceSelectionContext = audioDeviceSelectionContext2.GetString();
			deviceId = deviceId2.GetString();
		}
		return num;
	}

	public static uint PartyChatControlSetAudioEncoderBitrate(PARTY_CHAT_CONTROL_HANDLE chatControl, uint bitrate, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetAudioEncoderBitrate(chatControl.InteropHandle, bitrate, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetAudioEncoderBitrate(PARTY_CHAT_CONTROL_HANDLE chatControl, out uint bitrate)
	{
		bitrate = 0u;
		if (chatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetAudioEncoderBitrate(chatControl.InteropHandle, out bitrate);
	}

	public static uint PartyChatControlSetAudioRenderVolume(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, float volume)
	{
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlSetAudioRenderVolume(chatControl.InteropHandle, targetChatControl.InteropHandle, volume);
	}

	public static uint PartyChatControlGetAudioRenderVolume(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, out float volume)
	{
		volume = 0f;
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetAudioRenderVolume(chatControl.InteropHandle, targetChatControl.InteropHandle, out volume);
	}

	public static uint PartyChatControlSetAudioInputMuted(PARTY_CHAT_CONTROL_HANDLE chatControl, bool muted)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlSetAudioInputMuted(chatControl.InteropHandle, Convert.ToByte(muted));
	}

	public static uint PartyChatControlGetAudioInputMuted(PARTY_CHAT_CONTROL_HANDLE chatControl, out bool muted)
	{
		muted = false;
		if (chatControl == null)
		{
			return 4u;
		}
		byte muted2;
		uint num = PFPInterop.PartyChatControlGetAudioInputMuted(chatControl.InteropHandle, out muted2);
		if (PartyError.SUCCEEDED(num))
		{
			muted = muted2 != 0;
		}
		return num;
	}

	public static uint PartyChatControlSetIncomingAudioMuted(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, bool muted)
	{
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlSetIncomingAudioMuted(chatControl.InteropHandle, targetChatControl.InteropHandle, Convert.ToByte(muted));
	}

	public static uint PartyChatControlGetIncomingAudioMuted(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, out bool muted)
	{
		muted = false;
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		byte muted2;
		uint num = PFPInterop.PartyChatControlGetIncomingAudioMuted(chatControl.InteropHandle, targetChatControl.InteropHandle, out muted2);
		if (PartyError.SUCCEEDED(num))
		{
			muted = muted2 != 0;
		}
		return num;
	}

	public static uint PartyChatControlSetIncomingTextMuted(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, bool muted)
	{
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlSetIncomingTextMuted(chatControl.InteropHandle, targetChatControl.InteropHandle, Convert.ToByte(muted));
	}

	public static uint PartyChatControlGetIncomingTextMuted(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, out bool muted)
	{
		muted = false;
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		byte muted2;
		uint num = PFPInterop.PartyChatControlGetIncomingTextMuted(chatControl.InteropHandle, targetChatControl.InteropHandle, out muted2);
		if (PartyError.SUCCEEDED(num))
		{
			muted = muted2 != 0;
		}
		return num;
	}

	public static uint PartyChatControlIsLocal(PARTY_CHAT_CONTROL_HANDLE chatControl, out bool isLocal)
	{
		isLocal = false;
		if (chatControl == null)
		{
			return 4u;
		}
		byte isLocal2;
		uint num = PFPInterop.PartyChatControlIsLocal(chatControl.InteropHandle, out isLocal2);
		if (PartyError.SUCCEEDED(num))
		{
			isLocal = isLocal2 != 0;
		}
		return num;
	}

	public static uint PartyChatControlSetPermissions(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, PARTY_CHAT_PERMISSION_OPTIONS chatPermissionOptions)
	{
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlSetPermissions(chatControl.InteropHandle, targetChatControl.InteropHandle, chatPermissionOptions);
	}

	public static uint PartyChatControlGetPermissions(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, out PARTY_CHAT_PERMISSION_OPTIONS chatPermissionOptions)
	{
		chatPermissionOptions = PARTY_CHAT_PERMISSION_OPTIONS.PARTY_CHAT_PERMISSION_OPTIONS_NONE;
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetPermissions(chatControl.InteropHandle, targetChatControl.InteropHandle, out chatPermissionOptions);
	}

	public static uint PartyChatControlGetCustomContext(PARTY_CHAT_CONTROL_HANDLE chatControl, out object customContext)
	{
		if (chatControl == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyChatControlGetCustomContext, chatControl.InteropHandle, out customContext);
	}

	public static uint PartyChatControlSetCustomContext(PARTY_CHAT_CONTROL_HANDLE chatControl, object customContext)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyChatControlGetCustomContext, PFPInterop.PartyChatControlSetCustomContext, chatControl.InteropHandle, customContext);
	}

	public static uint PartyChatControlGetLanguage(PARTY_CHAT_CONTROL_HANDLE chatControl, out string languageCode)
	{
		languageCode = null;
		if (chatControl == null)
		{
			return 4u;
		}
		UTF8StringPtr languageCode2;
		uint num = PFPInterop.PartyChatControlGetLanguage(chatControl.InteropHandle, out languageCode2);
		if (PartyError.SUCCEEDED(num))
		{
			languageCode = languageCode2.GetString();
		}
		return num;
	}

	public static uint PartyChatControlSetLanguage(PARTY_CHAT_CONTROL_HANDLE chatControl, string languageCode, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		byte[] languageCode2 = null;
		if (!string.IsNullOrEmpty(languageCode))
		{
			languageCode2 = Converters.StringToNullTerminatedUTF8ByteArray(languageCode);
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetLanguage(chatControl.InteropHandle, languageCode2, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlSetTextChatOptions(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_TEXT_CHAT_OPTIONS options, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetTextChatOptions(chatControl.InteropHandle, options, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetTextChatOptions(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_TEXT_CHAT_OPTIONS options)
	{
		options = PARTY_TEXT_CHAT_OPTIONS.PARTY_TEXT_CHAT_OPTIONS_NONE;
		if (chatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetTextChatOptions(chatControl.InteropHandle, out options);
	}

	public static uint PartyChatControlSynthesizeTextToSpeech(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE type, string textToSynthesize, object asyncIdentifier)
	{
		if (chatControl == null || textToSynthesize == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSynthesizeTextToSpeech(chatControl.InteropHandle, type, Converters.StringToNullTerminatedUTF8ByteArray(textToSynthesize), intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlSetTranscriptionOptions(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS options, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetTranscriptionOptions(chatControl.InteropHandle, options, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetTranscriptionOptions(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS options)
	{
		options = PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS.PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS_NONE;
		if (chatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetTranscriptionOptions(chatControl.InteropHandle, out options);
	}

	public static uint PartyChatControlSetTextToSpeechProfile(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE type, string profileIdentifier, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlSetTextToSpeechProfile(chatControl.InteropHandle, type, Converters.StringToNullTerminatedUTF8ByteArray(profileIdentifier), intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetTextToSpeechProfile(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE type, out PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile)
	{
		profile = null;
		if (chatControl == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile2;
		return PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE.WrapAndReturnError(PFPInterop.PartyChatControlGetTextToSpeechProfile(chatControl.InteropHandle, type, out profile2), profile2, out profile);
	}

	public static uint PartyChatControlPopulateAvailableTextToSpeechProfiles(PARTY_CHAT_CONTROL_HANDLE chatControl, object asyncIdentifier)
	{
		if (chatControl == null)
		{
			return 4u;
		}
		IntPtr intPtr = IntPtr.Zero;
		if (asyncIdentifier != null)
		{
			intPtr = GCHandle.ToIntPtr(GCHandle.Alloc(asyncIdentifier));
		}
		uint num = PFPInterop.PartyChatControlPopulateAvailableTextToSpeechProfiles(chatControl.InteropHandle, intPtr);
		if (PartyError.FAILED(num) && intPtr != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(intPtr).Free();
		}
		return num;
	}

	public static uint PartyChatControlGetAvailableTextToSpeechProfiles(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE[] profiles)
	{
		profiles = null;
		if (chatControl == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyChatControlGetAvailableTextToSpeechProfiles, (PartyCSharpSDK.Interop.PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE s) => new PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE(s), chatControl.InteropHandle, out profiles);
	}

	public static uint PartyTextToSpeechProfileGetCustomContext(PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile, out object customContext)
	{
		if (profile == null)
		{
			customContext = null;
			return 4u;
		}
		return MarshalHelpers.GetCustomContext(PFPInterop.PartyTextToSpeechProfileGetCustomContext, profile.InteropHandle, out customContext);
	}

	public static uint PartyTextToSpeechProfileSetCustomContext(PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile, object customContext)
	{
		if (profile == null)
		{
			return 4u;
		}
		return MarshalHelpers.SetCustomContext(PFPInterop.PartyTextToSpeechProfileGetCustomContext, PFPInterop.PartyTextToSpeechProfileSetCustomContext, profile.InteropHandle, customContext);
	}

	public static uint PartyTextToSpeechProfileGetGender(PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile, out PARTY_GENDER gender)
	{
		gender = PARTY_GENDER.PARTY_GENDER_NEUTRAL;
		if (profile == null)
		{
			return 4u;
		}
		return PFPInterop.PartyTextToSpeechProfileGetGender(profile.InteropHandle, out gender);
	}

	public static uint PartyTextToSpeechProfileGetIdentifier(PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile, out string identifier)
	{
		identifier = null;
		if (profile == null)
		{
			return 4u;
		}
		UTF8StringPtr identifier2;
		uint num = PFPInterop.PartyTextToSpeechProfileGetIdentifier(profile.InteropHandle, out identifier2);
		if (PartyError.SUCCEEDED(num))
		{
			identifier = identifier2.GetString();
		}
		return num;
	}

	public static uint PartyTextToSpeechProfileGetLanguageCode(PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile, out string languageCode)
	{
		languageCode = null;
		if (profile == null)
		{
			return 4u;
		}
		UTF8StringPtr languageCode2;
		uint num = PFPInterop.PartyTextToSpeechProfileGetLanguageCode(profile.InteropHandle, out languageCode2);
		if (PartyError.SUCCEEDED(num))
		{
			languageCode = languageCode2.GetString();
		}
		return num;
	}

	public static uint PartyTextToSpeechProfileGetName(PARTY_TEXT_TO_SPEECH_PROFILE_HANDLE profile, out string name)
	{
		name = null;
		if (profile == null)
		{
			return 4u;
		}
		UTF8StringPtr name2;
		uint num = PFPInterop.PartyTextToSpeechProfileGetName(profile.InteropHandle, out name2);
		if (PartyError.SUCCEEDED(num))
		{
			name = name2.GetString();
		}
		return num;
	}

	public static uint PartyChatControlGetChatIndicator(PARTY_CHAT_CONTROL_HANDLE chatControl, PARTY_CHAT_CONTROL_HANDLE targetChatControl, out PARTY_CHAT_CONTROL_CHAT_INDICATOR chatIndicator)
	{
		chatIndicator = PARTY_CHAT_CONTROL_CHAT_INDICATOR.PARTY_CHAT_CONTROL_CHAT_INDICATOR_SILENT;
		if (chatControl == null || targetChatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetChatIndicator(chatControl.InteropHandle, targetChatControl.InteropHandle, out chatIndicator);
	}

	public static uint PartyChatControlGetLocalChatIndicator(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR chatIndicator)
	{
		chatIndicator = PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR.PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR_SILENT;
		if (chatControl == null)
		{
			return 4u;
		}
		return PFPInterop.PartyChatControlGetLocalChatIndicator(chatControl.InteropHandle, out chatIndicator);
	}

	public static uint PartyChatControlGetDevice(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_DEVICE_HANDLE device)
	{
		device = null;
		if (chatControl == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_DEVICE_HANDLE device2;
		return PARTY_DEVICE_HANDLE.WrapAndReturnError(PFPInterop.PartyChatControlGetDevice(chatControl.InteropHandle, out device2), device2, out device);
	}

	public static uint PartyChatControlGetLocalUser(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_LOCAL_USER_HANDLE localUser)
	{
		localUser = null;
		if (chatControl == null)
		{
			return 4u;
		}
		PartyCSharpSDK.Interop.PARTY_LOCAL_USER_HANDLE localUser2;
		return PARTY_LOCAL_USER_HANDLE.WrapAndReturnError(PFPInterop.PartyChatControlGetLocalUser(chatControl.InteropHandle, out localUser2), localUser2, out localUser);
	}

	public static uint PartyChatControlGetNetworks(PARTY_CHAT_CONTROL_HANDLE chatControl, out PARTY_NETWORK_HANDLE[] networks)
	{
		networks = null;
		if (chatControl == null)
		{
			return 4u;
		}
		return MarshalHelpers.GetArrayOfObjects(PFPInterop.PartyChatControlGetNetworks, (PartyCSharpSDK.Interop.PARTY_NETWORK_HANDLE s) => new PARTY_NETWORK_HANDLE(s), chatControl.InteropHandle, out networks);
	}

	public static uint PartyChatControlGetEntityId(PARTY_CHAT_CONTROL_HANDLE chatControl, out string entityId)
	{
		entityId = null;
		if (chatControl == null)
		{
			return 4u;
		}
		UTF8StringPtr entityId2;
		uint num = PFPInterop.PartyChatControlGetEntityId(chatControl.InteropHandle, out entityId2);
		if (PartyError.SUCCEEDED(num))
		{
			entityId = entityId2.GetString();
		}
		return num;
	}
}
