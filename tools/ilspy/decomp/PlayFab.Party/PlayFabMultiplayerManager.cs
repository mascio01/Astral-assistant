using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PartyCSharpSDK;
using PartyXBLCSharpSDK;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.Internal;
using PlayFab.Party._Internal;
using UnityEngine;

namespace PlayFab.Party;

public class PlayFabMultiplayerManager : MonoBehaviour
{
	public delegate void OnNetworkJoinedHandler(object sender, string networkId);

	public delegate void OnNetworkLeftHandler(object sender, string networkId);

	public delegate void OnRemotePlayerJoinedHandler(object sender, PlayFabPlayer player);

	public delegate void OnRemotePlayerLeftHandler(object sender, PlayFabPlayer player);

	public delegate void OnNetworkChangedHandler(object sender, string newNetworkId);

	public delegate void OnChatMessageReceivedHandler(object sender, PlayFabPlayer from, string message, ChatMessageType type);

	public delegate void OnDataMessageReceivedHandler(object sender, PlayFabPlayer from, byte[] buffer);

	public delegate void OnDataMessageReceivedNoCopyHandler(object sender, PlayFabPlayer from, IntPtr buffer, uint bufferSize);

	public delegate void OnErrorEventHandler(object sender, PlayFabMultiplayerManagerErrorArgs args);

	private abstract class WorkTask
	{
		public abstract bool Begin();

		public abstract bool Run();

		public abstract void End();
	}

	private class LeaveNetworkTask : WorkTask
	{
		public override bool Begin()
		{
			Debug.Log("Task: LeaveNetworkTask");
			PlayFabMultiplayerManager playFabMultiplayerManager = Get();
			if (playFabMultiplayerManager.IsConnectedToNetworkState())
			{
				playFabMultiplayerManager.LeaveNetwork();
				return true;
			}
			return false;
		}

		public override bool Run()
		{
			if (!Get().IsConnectedToNetworkState())
			{
				return true;
			}
			return false;
		}

		public override void End()
		{
		}
	}

	private class CleanPartyTask : WorkTask
	{
		public override bool Begin()
		{
			Debug.Log("Task: CleanPartyTask");
			PlayFabMultiplayerManager playFabMultiplayerManager = Get();
			if (!playFabMultiplayerManager.IsNotInitializedState())
			{
				playFabMultiplayerManager._CleanUp();
			}
			return true;
		}

		public override bool Run()
		{
			if (Get().IsNotInitializedState())
			{
				return true;
			}
			return false;
		}

		public override void End()
		{
		}
	}

	private class InitPartyTask : WorkTask
	{
		public override bool Begin()
		{
			Debug.Log("Task: InitPartyTask()");
			PlayFabMultiplayerManager playFabMultiplayerManager = Get();
			if (!playFabMultiplayerManager.IsInitializedState())
			{
				playFabMultiplayerManager._Initialize();
				return true;
			}
			return false;
		}

		public override bool Run()
		{
			if (Get().IsInitializedState())
			{
				return true;
			}
			return false;
		}

		public override void End()
		{
		}
	}

	private class JoinPartyTask : WorkTask
	{
		private string _networkId;

		public JoinPartyTask(string networkId)
		{
			_networkId = networkId;
		}

		public override bool Begin()
		{
			Debug.Log("Task: JoinPartyTask");
			PlayFabMultiplayerManager playFabMultiplayerManager = Get();
			if (!playFabMultiplayerManager.IsConnectedToNetworkState())
			{
				playFabMultiplayerManager.JoinNetwork(_networkId);
				return true;
			}
			return false;
		}

		public override bool Run()
		{
			if (Get().IsConnectedToNetworkState())
			{
				return true;
			}
			return false;
		}

		public override void End()
		{
		}
	}

	internal enum _InternalPlayFabMultiplayerManagerState
	{
		NotInitialized,
		PendingInitialization,
		Initialized,
		LoginRequestIssued,
		LocalUserCreated,
		LocalUserAuthenticated,
		ConnectedToNetwork
	}

	private struct QueuedStartCreateAndJoinNetworkOp
	{
		public bool queued;

		public PlayFabNetworkConfiguration networkConfiguration;
	}

	private struct QueuedCreateAndJoinAfterLeaveNetworkOp
	{
		public bool queued;

		public PlayFabNetworkConfiguration networkConfiguration;
	}

	private struct QueuedJoinNetworkOp
	{
		public bool queued;

		public string networkId;
	}

	private struct QueuedCompleteJoinAfterLeaveNetworkOp
	{
		public bool queued;

		public string networkId;
	}

	public enum LogLevelType
	{
		None,
		Minimal,
		Verbose
	}

	private enum PlayFabMultiplayerManagerMessageType : sbyte
	{
		Unset,
		Game,
		PolicyManager
	}

	private static PlayFabMultiplayerManager _multiplayerManager;

	private static LogLevelType _logLevel;

	private static bool _logLevelSetByUser;

	private IPlayFabChatPlatformPolicyProvider _platformPolicyProvider;

	private PlayFabLocalPlayer _localPlayer;

	private string _preferredLocalPlayerLanguageCode;

	private string _networkId;

	private string _generatedInvitationId;

	private List<PlayFabPlayer> _remotePlayers;

	private bool _translateChat;

	private AccessibilityMode _textToSpeechMode;

	private AccessibilityMode _speechToTextMode;

	private PARTY_HANDLE _partyHandle;

	private PARTY_NETWORK_HANDLE _networkHandle;

	private PARTY_LOCAL_USER_HANDLE _localUserHandle;

	private PARTY_DEVICE_HANDLE _localDeviceHandle;

	private PARTY_ENDPOINT_HANDLE _localEndPointHandle;

	private PARTY_CHAT_CONTROL_HANDLE _localChatControlHandle;

	private PARTY_NETWORK_DESCRIPTOR _networkDescriptor;

	private PARTY_SEND_MESSAGE_OPTIONS _defaultSendOptions;

	private PARTY_SEND_MESSAGE_QUEUING_CONFIGURATION _defaultQueuingConfiguration;

	private _InternalPlayFabMultiplayerManagerState _playFabMultiplayerManagerState;

	private bool _isLeaveNetworkInProgress;

	private bool _isJoinNetworkInProgress;

	private List<PARTY_ENDPOINT_HANDLE[]> _cachedSendMessageEndpointHandles;

	private List<PARTY_CHAT_CONTROL_HANDLE[]> _cachedSendMessageChatControlHandles;

	private PARTY_CHAT_CONTROL_HANDLE[] _cachedAllChatHandlesList;

	private List<PARTY_STATE_CHANGE> _partyStateChanges;

	private static PARTY_ENDPOINT_HANDLE[] _emptyEndpointHandlesArray = new PARTY_ENDPOINT_HANDLE[0];

	private static PARTY_CHAT_CONTROL_HANDLE[] _emptyChatControlHandlesArray = new PARTY_CHAT_CONTROL_HANDLE[0];

	private QueuedStartCreateAndJoinNetworkOp _queuedStartCreateAndJoinNetworkCreateLocalUserOp;

	private QueuedCreateAndJoinAfterLeaveNetworkOp _queuedCreateAndJoinAfterLeaveNetworkOp;

	private QueuedJoinNetworkOp _queuedJoinNetworkCreateLocalUserOp;

	private QueuedCompleteJoinAfterLeaveNetworkOp _queuedCompleteJoinAfterLeaveNetworkOp;

	private const int _DEVICES_PER_USER_COUNT = 1;

	private const int _ENDPOINTS_PER_DEVICE_COUNT = 1;

	private const int _USERS_PER_DEVICE = 1;

	private const string _NETWORK_ID_INVITE_AND_DESCRIPTOR_SEPERATOR = "|";

	private const uint _INTERNAL_EXCHANGE_MESSAGE_BUFFER_SIZE = 128u;

	private const string _INTERNAL_EXCHANGE_REQUEST_MESSAGE_PREFIX = "PFP-";

	private const PARTY_CHAT_PERMISSION_OPTIONS _CHAT_PERMISSIONS_ALL = (PARTY_CHAT_PERMISSION_OPTIONS)31u;

	private const PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS _PLATFORM_DEFAULT_CHAT_TRANSCRIPTION_OPTIONS = PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS.PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS_TRANSCRIBE_OTHER_CHAT_CONTROLS_WITH_MATCHING_LANGUAGES;

	private const string _ENTITY_TYPE_TITLE_PLAYER_ACCOUNT = "title_player_account";

	private const string _ErrorMessageNoUserLoggedIn = "No users logged in. You need to log in a user to PlayFab using the PlayFabClientAPI.LoginWithCustomID or similar API.";

	private const string _ErrorMessageMissingNetworkId = "networkId cannot be empty.";

	private const string _ErrorMessageMissingNetworkConfiguration = "networkConfiguration cannot be null.";

	private const string _ErrorMessageMissingPlayFabTitleId = "Missing Title ID. Please set your Title ID using PlayFab settings class or in the PlayFab Editor Extension.";

	private const string _ErrorMessagePartyAlreadyInitialized = "The Party DLL could not be unloaded. Please restart Unity to unload it.";

	private const string _ErrorMessagePlayerNotFound = "Player not found.";

	private const string _ErrorMessageEmptyDataMessagePayload = "Data message cannot be empty.";

	private const string _ErrorMessageTooManyRecipients = "Too many recipients.";

	private const string _ErrorMessageCannotCallAPINotConnectedToNetwork = "You need to connect to a network before you can call this method.";

	private const string _ErrorMessageMissingMultiplayerManagerPrefab = "PlayFabMultiplayerManager Prefab not found. You need to add the PlayFabMultiplayerManager prefab to your scene.";

	private const uint _c_ErrorFailedToFindResourceSpecified = 6u;

	private const uint _c_ErrorAlreadyInitialized = 4101u;

	private const uint _c_ErrorObjectIsBeingDestroyed = 4104u;

	private List<WorkTask> _tasks = new List<WorkTask>();

	private WorkTask _runningTask;

	private bool gameObjectPersisted;

	public LogLevelType LogLevel
	{
		get
		{
			return _logLevel;
		}
		set
		{
			_logLevelSetByUser = true;
			_logLevel = value;
		}
	}

	public PlayFabLocalPlayer LocalPlayer => _localPlayer;

	public string NetworkId => _networkId;

	public PlayFabMultiplayerManagerState State
	{
		get
		{
			if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.Initialized)
			{
				return PlayFabMultiplayerManagerState.NotInitialized;
			}
			if (_playFabMultiplayerManagerState == _InternalPlayFabMultiplayerManagerState.Initialized)
			{
				return PlayFabMultiplayerManagerState.Initialized;
			}
			if (_playFabMultiplayerManagerState > _InternalPlayFabMultiplayerManagerState.Initialized && _playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
			{
				return PlayFabMultiplayerManagerState.ConnectingToNetwork;
			}
			if (_playFabMultiplayerManagerState >= _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
			{
				return PlayFabMultiplayerManagerState.ConnectedToNetwork;
			}
			return PlayFabMultiplayerManagerState.NotInitialized;
		}
	}

	public IList<PlayFabPlayer> RemotePlayers => _remotePlayers.AsReadOnly();

	public bool TranslateChat
	{
		get
		{
			return _translateChat;
		}
		set
		{
			if (value)
			{
				SetTextChatOptions(PARTY_TEXT_CHAT_OPTIONS.PARTY_TEXT_CHAT_OPTIONS_TRANSLATE_TO_LOCAL_LANGUAGE);
			}
			else
			{
				SetTextChatOptions(PARTY_TEXT_CHAT_OPTIONS.PARTY_TEXT_CHAT_OPTIONS_NONE);
			}
			_translateChat = value;
		}
	}

	public AccessibilityMode SpeechToTextMode
	{
		get
		{
			return _speechToTextMode;
		}
		set
		{
			switch (value)
			{
			case AccessibilityMode.Enabled:
			{
				PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS transcriptionOptions = (PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS)22u;
				SetTranscriptionOptions(transcriptionOptions);
				break;
			}
			case AccessibilityMode.None:
				SetTranscriptionOptions(PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS.PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS_NONE);
				break;
			default:
				if (_platformPolicyProvider != null)
				{
					SetTranscriptionOptions(_platformPolicyProvider.GetPlatformUserChatTranscriptionPreferences());
				}
				else
				{
					SetTranscriptionOptions(PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS.PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS_TRANSCRIBE_OTHER_CHAT_CONTROLS_WITH_MATCHING_LANGUAGES);
				}
				break;
			}
			_speechToTextMode = value;
		}
	}

	public AccessibilityMode TextToSpeechMode
	{
		get
		{
			return _textToSpeechMode;
		}
		set
		{
			_textToSpeechMode = value;
		}
	}

	public event OnNetworkJoinedHandler OnNetworkJoined;

	public event OnNetworkLeftHandler OnNetworkLeft;

	public event OnRemotePlayerJoinedHandler OnRemotePlayerJoined;

	public event OnRemotePlayerLeftHandler OnRemotePlayerLeft;

	public event OnNetworkChangedHandler OnNetworkChanged;

	public event OnChatMessageReceivedHandler OnChatMessageReceived;

	public event OnDataMessageReceivedHandler OnDataMessageReceived;

	public event OnDataMessageReceivedNoCopyHandler OnDataMessageNoCopyReceived;

	public event OnErrorEventHandler OnError;

	private void Awake()
	{
	}

	private void Start()
	{
		_Initialize();
	}

	private void OnApplicationQuit()
	{
		_CleanUp();
	}

	private void Update()
	{
		if (_playFabMultiplayerManagerState >= _InternalPlayFabMultiplayerManagerState.Initialized)
		{
			ProcessQueuedOperations();
			ProcessStateChanges();
			if (_platformPolicyProvider != null)
			{
				_platformPolicyProvider.ProcessStateChanges();
			}
			SingletonMonoBehaviour<PlayFabEventTracer>.instance.DoWork();
		}
		if (HasTasks())
		{
			ProcessTask();
		}
	}

	private void OnDestroy()
	{
		_CleanUp();
	}

	public static PlayFabMultiplayerManager Get()
	{
		if (_multiplayerManager == null)
		{
			PlayFabMultiplayerManager[] array = UnityEngine.Object.FindObjectsOfType<PlayFabMultiplayerManager>();
			if (array.Length != 0)
			{
				_multiplayerManager = array[0];
				_multiplayerManager._Initialize();
			}
			else
			{
				_LogError("PlayFabMultiplayerManager Prefab not found. You need to add the PlayFabMultiplayerManager prefab to your scene.");
			}
		}
		return _multiplayerManager;
	}

	public void Resume()
	{
		_LogInfo("PlayFabMultiplayerManager:Resume()");
		InitializeImpl();
	}

	public void Suspend()
	{
		_LogInfo("PlayFabMultiplayerManager:Suspend()");
		CleanUpImpl();
		_tasks.Clear();
		_runningTask = null;
	}

	public void CreateAndJoinNetwork()
	{
		PlayFabNetworkConfiguration playFabNetworkConfiguration = new PlayFabNetworkConfiguration();
		playFabNetworkConfiguration.MaxPlayerCount = 32u;
		CreateAndJoinNetwork(playFabNetworkConfiguration);
	}

	public void CreateAndJoinNetwork(PlayFabNetworkConfiguration networkConfiguration)
	{
		CreateAndJoinNetworkImplStart(networkConfiguration);
	}

	public void JoinNetwork(string networkId)
	{
		JoinNetworkImplStart(networkId);
	}

	public void LeaveNetwork()
	{
		LeaveNetworkImpl(wasCallInitiatedByDeveloper: true);
	}

	public void SendDataMessageToAllPlayers(byte[] buffer)
	{
		_SendDataMessageToAllPlayers(buffer);
	}

	public bool SendDataMessage(byte[] buffer, IEnumerable<PlayFabPlayer> recipients, DeliveryOption deliveryOption)
	{
		return _SendDataMessage(buffer, recipients, deliveryOption);
	}

	public void SendDataMessage(IntPtr buffer, uint bufferSize, IEnumerable<PlayFabPlayer> recipients, DeliveryOption deliveryOption)
	{
		_SendDataMessage(buffer, bufferSize, recipients, deliveryOption);
	}

	public void SendChatMessageToAllPlayers(string message)
	{
		_SendChatMessageToAllPlayers(message);
	}

	public void SendChatMessage(string message, IEnumerable<PlayFabPlayer> recipients)
	{
		_SendChatMessage(message, recipients);
	}

	public void UpdateEntityToken(string entityToken)
	{
		if (_localUserHandle != null)
		{
			PartySucceeded(SDK.PartyLocalUserUpdateEntityToken(_localUserHandle, entityToken));
			_localPlayer._entityToken = entityToken;
		}
	}

	internal static void _LogError(string message)
	{
		if (_logLevel != LogLevelType.None)
		{
			Debug.LogError(message);
		}
	}

	internal static void _LogError(uint code)
	{
		_LogError(code, PlayFabMultiplayerManagerErrorType.Error);
	}

	internal static void _LogError(uint code, PlayFabMultiplayerManagerErrorType type)
	{
		string errorMessage = string.Empty;
		if (PartyError.FAILED(SDK.PartyGetErrorMessage(code, out errorMessage)))
		{
			errorMessage = "Unknown error.";
		}
		PlayFabMultiplayerManager playFabMultiplayerManager = Get();
		if (playFabMultiplayerManager.OnError != null)
		{
			PlayFabMultiplayerManagerErrorArgs args = new PlayFabMultiplayerManagerErrorArgs((int)code, errorMessage, type);
			playFabMultiplayerManager.OnError(playFabMultiplayerManager, args);
		}
		_LogError(errorMessage);
	}

	internal static void _LogError(uint code, string message, PlayFabMultiplayerManagerErrorArgs args)
	{
		PlayFabMultiplayerManager playFabMultiplayerManager = Get();
		if (playFabMultiplayerManager.OnError != null)
		{
			playFabMultiplayerManager.OnError(playFabMultiplayerManager, args);
		}
		_LogError(message);
	}

	internal static void _LogWarning(string warningMessage)
	{
		if (_logLevel >= LogLevelType.Verbose)
		{
			Debug.LogWarning(warningMessage);
		}
	}

	internal static void _LogInfo(string infoMessage)
	{
		if (_logLevel >= LogLevelType.Verbose)
		{
			Debug.Log(infoMessage);
		}
	}

	internal bool _StartsWithSequence(byte[] buffer, byte[] sequence)
	{
		bool result = true;
		if (buffer.Length > sequence.Length + 1)
		{
			for (int i = 0; i < sequence.Length; i++)
			{
				if (buffer[i] != sequence[i])
				{
					result = false;
					break;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	private bool IsInternalMessage(IntPtr messageBuffer, uint messageSize)
	{
		if (messageSize != 0 && messageSize < 128)
		{
			byte[] array = new byte[128];
			Marshal.Copy(messageBuffer, array, 0, (int)messageSize);
			return _StartsWithSequence(array, Encoding.ASCII.GetBytes("PFP-"));
		}
		return false;
	}

	private PlayFabPlayer GetPlayerByEntityId(string entityId)
	{
		if (_remotePlayers != null)
		{
			foreach (PlayFabPlayer remotePlayer in _remotePlayers)
			{
				if (remotePlayer.EntityKey.Id == entityId)
				{
					return remotePlayer;
				}
			}
		}
		return null;
	}

	private PARTY_ENDPOINT_HANDLE[] EndPointHandlesFromPlayFabPlayerListNoGC(IEnumerable<PlayFabPlayer> playerList)
	{
		int num = playerList.Count();
		if (num == 0)
		{
			return _emptyEndpointHandlesArray;
		}
		int num2 = _cachedSendMessageEndpointHandles.Count();
		if (num2 < num)
		{
			for (int i = num2 + 1; i <= num; i++)
			{
				_cachedSendMessageEndpointHandles.Add(new PARTY_ENDPOINT_HANDLE[i]);
			}
		}
		for (int j = 0; j < playerList.Count(); j++)
		{
			_cachedSendMessageEndpointHandles[num - 1][j] = playerList.ElementAt(j)._endPointHandle;
		}
		return _cachedSendMessageEndpointHandles[num - 1];
	}

	private PARTY_CHAT_CONTROL_HANDLE[] ChatControlHandlesFromPlayFabPlayerListNoGC(IEnumerable<PlayFabPlayer> playerList)
	{
		int num = playerList.Count();
		if (num == 0)
		{
			return _emptyChatControlHandlesArray;
		}
		int num2 = _cachedSendMessageChatControlHandles.Count();
		if (num2 < num)
		{
			for (int i = num2 + 1; i <= num; i++)
			{
				_cachedSendMessageChatControlHandles.Add(new PARTY_CHAT_CONTROL_HANDLE[i]);
			}
		}
		for (int j = 0; j < playerList.Count(); j++)
		{
			_cachedSendMessageChatControlHandles[num - 1][j] = playerList.ElementAt(j)._chatControlHandle;
		}
		return _cachedSendMessageChatControlHandles[num - 1];
	}

	private void _Initialize()
	{
		_LogInfo("PlayFabMultiplayerManager:_Initialize()");
		InitializeImpl();
	}

	private void _CleanUp()
	{
		_LogInfo("PlayFabMultiplayerManager:_CleanUp()");
		CleanUpImpl();
	}

	private void InitializeImpl()
	{
		if (_playFabMultiplayerManagerState <= _InternalPlayFabMultiplayerManagerState.NotInitialized)
		{
			_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.PendingInitialization;
			if (!_logLevelSetByUser)
			{
				_logLevel = LogLevelType.Minimal;
			}
			_platformPolicyProvider = null;
			_defaultSendOptions = (PARTY_SEND_MESSAGE_OPTIONS)3u;
			_defaultQueuingConfiguration = new PARTY_SEND_MESSAGE_QUEUING_CONFIGURATION
			{
				Priority = Convert.ToSByte((short)0),
				IdentityForCancelFilters = 0u,
				TimeoutInMilliseconds = 0u
			};
			_localPlayer = new PlayFabLocalPlayer();
			_remotePlayers = new List<PlayFabPlayer>();
			_partyStateChanges = new List<PARTY_STATE_CHANGE>();
			_cachedSendMessageEndpointHandles = new List<PARTY_ENDPOINT_HANDLE[]>();
			_cachedSendMessageChatControlHandles = new List<PARTY_CHAT_CONTROL_HANDLE[]>();
			if (!gameObjectPersisted)
			{
				gameObjectPersisted = true;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
			string titleId = PlayFabSettings.staticSettings.TitleId;
			if (string.IsNullOrEmpty(titleId))
			{
				_LogError("Missing Title ID. Please set your Title ID using PlayFab settings class or in the PlayFab Editor Extension.");
			}
			uint num = SDK.PartyInitialize(titleId, out _partyHandle);
			if (PartySucceeded(num))
			{
				_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.Initialized;
			}
			else if (num == 4101)
			{
				_LogError("The Party DLL could not be unloaded. Please restart Unity to unload it.");
			}
			SingletonMonoBehaviour<PlayFabEventTracer>.instance.OnPlayFabMultiPlayerManagerInitialize();
		}
	}

	private void CleanUpImpl()
	{
		if (_playFabMultiplayerManagerState > _InternalPlayFabMultiplayerManagerState.NotInitialized)
		{
			if (_partyHandle != null && PartySucceeded(SDK.PartyCleanup(_partyHandle)))
			{
				_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.NotInitialized;
			}
			_isLeaveNetworkInProgress = false;
			_isJoinNetworkInProgress = false;
			_queuedStartCreateAndJoinNetworkCreateLocalUserOp.queued = false;
			_queuedCreateAndJoinAfterLeaveNetworkOp.queued = false;
			_queuedJoinNetworkCreateLocalUserOp.queued = false;
			_queuedCompleteJoinAfterLeaveNetworkOp.queued = false;
			if (_platformPolicyProvider != null)
			{
				_platformPolicyProvider.CleanUp();
			}
			_defaultQueuingConfiguration = null;
			_remotePlayers = null;
			_cachedSendMessageEndpointHandles = null;
			_cachedSendMessageChatControlHandles = null;
			_cachedAllChatHandlesList = null;
			_partyStateChanges = null;
			_localPlayer = null;
			_partyHandle = null;
			_localUserHandle = null;
			_networkDescriptor = null;
			_generatedInvitationId = null;
			_networkHandle = null;
			_localDeviceHandle = null;
			_localEndPointHandle = null;
			_localChatControlHandle = null;
		}
	}

	private PARTY_NETWORK_DESCRIPTOR GetNetworkDescriptorFromNetworkId(string networkId)
	{
		_LogInfo("PlayFabMultiplayerManager:GetNetworkDescriptorFromNetworkId()");
		PARTY_NETWORK_DESCRIPTOR networkDescriptor = null;
		int num = networkId.IndexOf("|");
		if (num != -1)
		{
			_generatedInvitationId = networkId.Substring(0, num);
			string serializedNetworkDescriptorString = networkId.Substring(num + 1);
			PartySucceeded(SDK.PartyDeserializeNetworkDescriptor(serializedNetworkDescriptorString, out networkDescriptor));
		}
		return networkDescriptor;
	}

	private void ProcessQueuedOperations()
	{
		if (_playFabMultiplayerManagerState <= _InternalPlayFabMultiplayerManagerState.NotInitialized)
		{
			return;
		}
		if ((_queuedStartCreateAndJoinNetworkCreateLocalUserOp.queued || _queuedJoinNetworkCreateLocalUserOp.queued) && _playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.LoginRequestIssued)
		{
			if (_platformPolicyProvider != null)
			{
				_platformPolicyProvider.SignIn();
				_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.LoginRequestIssued;
			}
			else if (PlayFabAuthenticationAPI.IsEntityLoggedIn())
			{
				PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest(), GetEntityTokenCompleted, GetEntityTokenFailed);
				_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.LoginRequestIssued;
			}
			else
			{
				_LogError("No users logged in. You need to log in a user to PlayFab using the PlayFabClientAPI.LoginWithCustomID or similar API.");
				DropCurrentQueuedOps();
			}
		}
		if (_platformPolicyProvider != null)
		{
			_platformPolicyProvider.ProcessQueuedOperations();
		}
	}

	private void GetEntityTokenCompleted(GetEntityTokenResponse response)
	{
		_LogInfo("PlayFabMultiplayerManager:GetEntityTokenCompleted(), EntityID: " + response.Entity.Id);
		PlayFab.ClientModels.EntityKey entityKey = new PlayFab.ClientModels.EntityKey
		{
			Id = response.Entity.Id,
			Type = response.Entity.Type
		};
		_CreateLocalUser(entityKey, response.EntityToken);
	}

	internal void _CreateLocalUser(PlayFab.ClientModels.EntityKey entityKey, string entityToken)
	{
		_LogInfo("PlayFabMultiplayerManager:_CreateLocalUser(), EntityID: " + entityKey.Id);
		PartySucceeded(SDK.PartyGetLocalDevice(_partyHandle, out _localDeviceHandle));
		_localPlayer._entityToken = entityToken;
		_localPlayer._SetEntityKey(entityKey);
		uint errorCode = 0u;
		if (_localUserHandle == null)
		{
			errorCode = SDK.PartyCreateLocalUser(_partyHandle, entityKey.Id, _localPlayer._entityToken, out _localUserHandle);
		}
		if (PartySucceeded(errorCode))
		{
			PARTY_LOCAL_USER_HANDLE localUser = null;
			if (_localChatControlHandle != null)
			{
				PartySucceeded(SDK.PartyChatControlGetLocalUser(_localChatControlHandle, out localUser));
			}
			if (localUser == null)
			{
				PartySucceeded(SDK.PartyDeviceCreateChatControl(_localDeviceHandle, _localUserHandle, LocalPlayer._preferredLanguageCode, null, out _localChatControlHandle));
				PartySucceeded(SDK.PartyChatControlSetAudioInputMuted(_localChatControlHandle, LocalPlayer.IsMuted));
				PartySucceeded(SDK.PartyChatControlSetAudioRenderVolume(_localChatControlHandle, _localChatControlHandle, LocalPlayer.VoiceLevel));
			}
			if (_localChatControlHandle != null)
			{
				_localPlayer._chatControlHandle = _localChatControlHandle;
			}
			_SetPlayFabMultiplayerManagerInternalState(_InternalPlayFabMultiplayerManagerState.LocalUserCreated);
			if (_queuedStartCreateAndJoinNetworkCreateLocalUserOp.queued)
			{
				_queuedStartCreateAndJoinNetworkCreateLocalUserOp.queued = false;
				CreateAndJoinNetworkImplStart(_queuedStartCreateAndJoinNetworkCreateLocalUserOp.networkConfiguration);
			}
			if (_queuedJoinNetworkCreateLocalUserOp.queued)
			{
				_queuedJoinNetworkCreateLocalUserOp.queued = false;
				JoinNetworkImplStart(_queuedJoinNetworkCreateLocalUserOp.networkId);
			}
		}
		else
		{
			_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.Initialized;
			DropCurrentQueuedOps();
		}
	}

	internal void DropCurrentQueuedOps()
	{
		_queuedStartCreateAndJoinNetworkCreateLocalUserOp.queued = false;
		_queuedCreateAndJoinAfterLeaveNetworkOp.queued = false;
		_queuedJoinNetworkCreateLocalUserOp.queued = false;
		_queuedCompleteJoinAfterLeaveNetworkOp.queued = false;
	}

	private void GetEntityTokenFailed(PlayFabError error)
	{
		_LogError(error.ErrorMessage);
		_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.Initialized;
		DropCurrentQueuedOps();
	}

	private void CreateAndJoinNetworkImplStart(PlayFabNetworkConfiguration networkConfiguration)
	{
		_LogInfo("PlayFabMultiplayerManager:CreateAndJoinNetworkImplStart()");
		if (networkConfiguration == null)
		{
			_LogError("networkConfiguration cannot be null.");
			return;
		}
		if (_platformPolicyProvider == null && !PlayFabAuthenticationAPI.IsEntityLoggedIn())
		{
			_LogError("No users logged in. You need to log in a user to PlayFab using the PlayFabClientAPI.LoginWithCustomID or similar API.");
			return;
		}
		if (_isJoinNetworkInProgress)
		{
			_queuedStartCreateAndJoinNetworkCreateLocalUserOp.networkConfiguration = networkConfiguration;
		}
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.LocalUserCreated)
		{
			_LogInfo("PlayFabMultiplayerManager:CreateAndJoinNetworkImplStart():QueueStartCreateAndJoinNetworkCreateLocalUserOp");
			_queuedStartCreateAndJoinNetworkCreateLocalUserOp = new QueuedStartCreateAndJoinNetworkOp
			{
				queued = true,
				networkConfiguration = networkConfiguration
			};
			_isJoinNetworkInProgress = true;
		}
		else if (_playFabMultiplayerManagerState >= _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			_LogInfo("PlayFabMultiplayerManager:CreateAndJoinNetworkImplStart():QueuedCreateAndJoinAfterLeaveNetworkOp");
			_queuedCreateAndJoinAfterLeaveNetworkOp = new QueuedCreateAndJoinAfterLeaveNetworkOp
			{
				queued = true,
				networkConfiguration = networkConfiguration
			};
			LeaveNetworkImpl(wasCallInitiatedByDeveloper: false);
		}
		else
		{
			CreateAndJoinNetworkImplComplete(networkConfiguration);
		}
	}

	private void CreateAndJoinNetworkImplComplete(PlayFabNetworkConfiguration networkConfiguration)
	{
		_LogInfo("PlayFabMultiplayerManager:CreateAndJoinNetworkImplComplete()");
		PARTY_NETWORK_CONFIGURATION networkConfiguration2 = new PARTY_NETWORK_CONFIGURATION
		{
			MaxDeviceCount = networkConfiguration.MaxPlayerCount,
			MaxDevicesPerUserCount = 1u,
			MaxEndpointsPerDeviceCount = 1u,
			MaxUserCount = networkConfiguration.MaxPlayerCount,
			MaxUsersPerDeviceCount = 1u,
			DirectPeerConnectivityOptions = networkConfiguration.DirectPeerConnectivityOptions
		};
		PARTY_INVITATION_CONFIGURATION initialInvitationConfiguration = new PARTY_INVITATION_CONFIGURATION
		{
			Identifier = Guid.NewGuid().ToString(),
			Revocability = PARTY_INVITATION_REVOCABILITY.PARTY_INVITATION_REVOCABILITY_ANYONE,
			EntityIds = null
		};
		PARTY_REGION[] regions = new PARTY_REGION[0];
		_generatedInvitationId = string.Empty;
		PartySucceeded(SDK.PartyCreateNewNetwork(_partyHandle, _localUserHandle, networkConfiguration2, regions, initialInvitationConfiguration, null, out _networkDescriptor, out _generatedInvitationId));
		PartySucceeded(SDK.PartyConnectToNetwork(_partyHandle, _networkDescriptor, null, out _networkHandle));
	}

	private void JoinNetworkImplStart(string networkId)
	{
		_LogInfo("PlayFabMultiplayerManager:JoinNetworkImplStart");
		if (string.IsNullOrEmpty(networkId))
		{
			_LogError("networkId cannot be empty.");
			return;
		}
		if (_platformPolicyProvider == null && !PlayFabAuthenticationAPI.IsEntityLoggedIn())
		{
			_LogError("No users logged in. You need to log in a user to PlayFab using the PlayFabClientAPI.LoginWithCustomID or similar API.");
			return;
		}
		if (_isJoinNetworkInProgress)
		{
			_queuedJoinNetworkCreateLocalUserOp.networkId = networkId;
		}
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.LocalUserCreated)
		{
			_LogInfo("PlayFabMultiplayerManager:JoinNetworkImplStart:QueueJoinNetworkCreateLocalUserOp");
			_queuedJoinNetworkCreateLocalUserOp = new QueuedJoinNetworkOp
			{
				queued = true,
				networkId = networkId
			};
			_isJoinNetworkInProgress = true;
		}
		else if (_playFabMultiplayerManagerState >= _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			_LogInfo("PlayFabMultiplayerManager:JoinNetworkImplStart:QueuedCompleteJoinAfterLeaveNetworkOp");
			_queuedCompleteJoinAfterLeaveNetworkOp = new QueuedCompleteJoinAfterLeaveNetworkOp
			{
				queued = true,
				networkId = networkId
			};
			LeaveNetworkImpl(wasCallInitiatedByDeveloper: false);
		}
		else
		{
			JoinNetworkImplComplete(networkId);
		}
	}

	private void JoinNetworkImplComplete(string networkId)
	{
		_LogInfo("PlayFabMultiplayerManager:JoinNetworkImplComplete");
		_networkDescriptor = GetNetworkDescriptorFromNetworkId(networkId);
		if (_networkDescriptor != null)
		{
			PartySucceeded(SDK.PartyConnectToNetwork(_partyHandle, _networkDescriptor, null, out _networkHandle));
		}
		else
		{
			_LogError("Network ID is not the correct format.");
		}
	}

	private void LeaveNetworkImpl(bool wasCallInitiatedByDeveloper)
	{
		_LogInfo("PlayFabMultiplayerManager:LeaveNetworkImpl, wasCallInitiatedByDeveloper: " + wasCallInitiatedByDeveloper);
		if (wasCallInitiatedByDeveloper)
		{
			_queuedCreateAndJoinAfterLeaveNetworkOp.queued = false;
			_queuedCompleteJoinAfterLeaveNetworkOp.queued = false;
		}
		if (_isLeaveNetworkInProgress || _networkHandle == null)
		{
			return;
		}
		uint num = SDK.PartyNetworkLeaveNetwork(_networkHandle, null);
		if (PartyError.FAILED(num))
		{
			if (num == 4104)
			{
				_LogInfo("Client is trying to leave a network that does not exist anymore.");
			}
			else
			{
				_LogError(num);
			}
		}
		else
		{
			_cachedAllChatHandlesList = null;
			_isLeaveNetworkInProgress = true;
			_networkDescriptor = null;
			_networkHandle = null;
		}
	}

	private void UpdateNetworkId(string invitationId, PARTY_NETWORK_DESCRIPTOR networkDescriptor)
	{
		_LogInfo("PlayFabMultiplayerManager:UpdateNetworkId()");
		_networkDescriptor = networkDescriptor;
		string serializedNetworkDescriptorString = string.Empty;
		PartySucceeded(SDK.PartySerializeNetworkDescriptor(_networkDescriptor, out serializedNetworkDescriptorString));
		_networkId = invitationId + "|" + serializedNetworkDescriptorString;
	}

	private void ResetNetworkManagerStateAfterFailureToConnect()
	{
		_LogInfo("PlayFabMultiplayerManager:ResetNetworkManagerStateAfterFailureToConnect()");
		_networkHandle = null;
		_networkDescriptor = null;
		_generatedInvitationId = null;
	}

	private void AuthenticateLocalUserStart()
	{
		_LogInfo("PlayFabMultiplayerManager:AuthenticateLocalUserStart()");
		PartySucceeded(SDK.PartyNetworkAuthenticateLocalUser(_networkHandle, _localUserHandle, _generatedInvitationId, null));
		PartySucceeded(SDK.PartyNetworkCreateEndpoint(_networkHandle, _localUserHandle, null, null, out _localEndPointHandle));
		PartySucceeded(SDK.PartyNetworkConnectChatControl(_networkHandle, _localChatControlHandle, null));
	}

	private void AuthenticateLocalUserComplete()
	{
		_LogInfo("PlayFabMultiplayerManager:AuthenticateLocalUserComplete()");
		_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.LocalUserAuthenticated;
		SetUserSettings();
		_isJoinNetworkInProgress = false;
		if (this.OnNetworkJoined != null)
		{
			_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork;
			this.OnNetworkJoined(this, _networkId);
		}
		SpeechToTextMode = SpeechToTextMode;
		TranslateChat = TranslateChat;
	}

	private void SetUserSettings()
	{
		_LogInfo("PlayFabMultiplayerManager:SetUserSettings()");
		PartySucceeded(SDK.PartyChatControlSetAudioInput(_localChatControlHandle, PARTY_AUDIO_DEVICE_SELECTION_TYPE.PARTY_AUDIO_DEVICE_SELECTION_TYPE_SYSTEM_DEFAULT, string.Empty, null));
		PartySucceeded(SDK.PartyChatControlSetAudioOutput(_localChatControlHandle, PARTY_AUDIO_DEVICE_SELECTION_TYPE.PARTY_AUDIO_DEVICE_SELECTION_TYPE_SYSTEM_DEFAULT, string.Empty, null));
		PartySucceeded(SDK.PartyChatControlPopulateAvailableTextToSpeechProfiles(_localChatControlHandle, null));
	}

	private bool IsTextToSpeechEnabled()
	{
		if (TextToSpeechMode == AccessibilityMode.None)
		{
			return false;
		}
		if (TextToSpeechMode == AccessibilityMode.Enabled)
		{
			return true;
		}
		if (_platformPolicyProvider != null)
		{
			return _platformPolicyProvider.IsTextToSpeechEnabled();
		}
		return false;
	}

	private void SetTextChatOptions(PARTY_TEXT_CHAT_OPTIONS textChatOptions)
	{
		PartySucceeded(SDK.PartyChatControlSetTextChatOptions(_localChatControlHandle, textChatOptions, null));
	}

	private void SetTranscriptionOptions(PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS transcriptionOptions)
	{
		PartySucceeded(SDK.PartyChatControlSetTranscriptionOptions(_localChatControlHandle, transcriptionOptions, null));
	}

	internal void _SendDataMessageToAllPlayers(byte[] buffer)
	{
		_LogInfo("PlayFabMultiplayerManager:_SendDataMessageToAllPlayers(byte[] buffer)");
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			_LogError("You need to connect to a network before you can call this method.");
		}
		else if (_playFabMultiplayerManagerState >= _InternalPlayFabMultiplayerManagerState.LocalUserAuthenticated)
		{
			if (buffer.Count() == 0)
			{
				_LogError("Data message cannot be empty.");
			}
			else
			{
				PartySucceeded(SDK.PartyEndpointSendMessage(_localEndPointHandle, null, _defaultSendOptions, _defaultQueuingConfiguration, buffer));
			}
		}
	}

	internal bool _SendDataMessage(byte[] buffer, IEnumerable<PlayFabPlayer> recipients, DeliveryOption deliveryOption)
	{
		_LogInfo("PlayFabMultiplayerManager:_SendDataMessage(byte[] buffer, IEnumerable<PlayFabPlayer> recipients, DeliveryOption deliveryOption)");
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			_LogError("You need to connect to a network before you can call this method.");
			return false;
		}
		if (buffer.Count() == 0)
		{
			_LogError("Data message cannot be empty.");
			return false;
		}
		if (recipients.Count() > 32)
		{
			_LogError("Too many recipients.");
			return false;
		}
		PARTY_ENDPOINT_HANDLE[] targetEndpoints = EndPointHandlesFromPlayFabPlayerListNoGC(recipients);
		PARTY_SEND_MESSAGE_OPTIONS options = SendOptionsFromDeliveryOption(deliveryOption);
		return PartySucceeded(SDK.PartyEndpointSendMessage(_localEndPointHandle, targetEndpoints, options, _defaultQueuingConfiguration, buffer));
	}

	internal void _SendDataMessage(IntPtr buffer, uint bufferSize, IEnumerable<PlayFabPlayer> recipients, DeliveryOption deliveryOption)
	{
		_LogInfo("PlayFabMultiplayerManager:_SendDataMessage(IntPtr buffer, uint bufferSize, IEnumerable<PlayFabPlayer> recipients, DeliveryOption deliveryOption)");
		if (bufferSize == 0)
		{
			_LogError("Data message cannot be empty.");
			return;
		}
		PARTY_ENDPOINT_HANDLE[] targetEndpoints = EndPointHandlesFromPlayFabPlayerListNoGC(recipients);
		PARTY_SEND_MESSAGE_OPTIONS options = SendOptionsFromDeliveryOption(deliveryOption);
		PartySucceeded(SDK.PartyEndpointSendMessage(_localEndPointHandle, targetEndpoints, options, _defaultQueuingConfiguration, buffer, bufferSize));
	}

	internal void _SendChatMessageToAllPlayers(string message)
	{
		_LogInfo("PlayFabMultiplayerManager:_SendChatMessageToAllPlayers()");
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			_LogError("You need to connect to a network before you can call this method.");
		}
		else if (_cachedAllChatHandlesList != null)
		{
			_SendChatMessageImpl(message, _cachedAllChatHandlesList);
		}
	}

	internal void _SendChatMessage(string message, IEnumerable<PlayFabPlayer> recipients)
	{
		_LogInfo("PlayFabMultiplayerManager:_SendChatMessage()");
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			_LogError("You need to connect to a network before you can call this method.");
			return;
		}
		if (recipients == null || recipients.Count() == 0)
		{
			_LogWarning("Warning: No recipients specified.");
			return;
		}
		if (recipients.Count() > 32)
		{
			_LogError("Too many recipients.");
			return;
		}
		PARTY_CHAT_CONTROL_HANDLE[] targetChatControlHandles = ChatControlHandlesFromPlayFabPlayerListNoGC(recipients);
		_SendChatMessageImpl(message, targetChatControlHandles);
	}

	private void _SendChatMessageImpl(string message, PARTY_CHAT_CONTROL_HANDLE[] targetChatControlHandles)
	{
		_LogInfo("PlayFabMultiplayerManager:_SendChatMessageImpl()");
		if (IsTextToSpeechEnabled())
		{
			PartySucceeded(SDK.PartyChatControlSynthesizeTextToSpeech(_localChatControlHandle, PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE.PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE_VOICE_CHAT, message, null));
		}
		PartySucceeded(SDK.PartyChatControlSendText(_localChatControlHandle, targetChatControlHandles, message, null));
	}

	private PARTY_SEND_MESSAGE_OPTIONS SendOptionsFromDeliveryOption(DeliveryOption deliveryOption)
	{
		PARTY_SEND_MESSAGE_OPTIONS pARTY_SEND_MESSAGE_OPTIONS = PARTY_SEND_MESSAGE_OPTIONS.PARTY_SEND_MESSAGE_OPTIONS_SEQUENTIAL_DELIVERY;
		if (deliveryOption == DeliveryOption.BestEffort)
		{
			return pARTY_SEND_MESSAGE_OPTIONS | PARTY_SEND_MESSAGE_OPTIONS.PARTY_SEND_MESSAGE_OPTIONS_DEFAULT;
		}
		return pARTY_SEND_MESSAGE_OPTIONS | PARTY_SEND_MESSAGE_OPTIONS.PARTY_SEND_MESSAGE_OPTIONS_GUARANTEED_DELIVERY;
	}

	private void UpdateCachedChatControlsList()
	{
		List<PARTY_CHAT_CONTROL_HANDLE> list = new List<PARTY_CHAT_CONTROL_HANDLE>();
		foreach (PlayFabPlayer remotePlayer in _remotePlayers)
		{
			list.Add(remotePlayer._chatControlHandle);
		}
		_cachedAllChatHandlesList = list.ToArray();
	}

	internal void _SetMuted(PlayFab.ClientModels.EntityKey entityKey, bool isMuted, bool isLocal)
	{
		_LogInfo("PlayFabMultiplayerManager:_SetMuted()");
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.LocalUserCreated)
		{
			return;
		}
		if (isLocal)
		{
			PartySucceeded(SDK.PartyChatControlSetAudioInputMuted(_localChatControlHandle, isMuted));
			return;
		}
		PlayFabPlayer playerByEntityId = GetPlayerByEntityId(entityKey.Id);
		if (playerByEntityId != null)
		{
			PartySucceeded(SDK.PartyChatControlSetIncomingAudioMuted(_localChatControlHandle, playerByEntityId._chatControlHandle, isMuted));
			PARTY_CHAT_PERMISSION_OPTIONS pARTY_CHAT_PERMISSION_OPTIONS = (PARTY_CHAT_PERMISSION_OPTIONS)31u;
			if (isMuted)
			{
				pARTY_CHAT_PERMISSION_OPTIONS = PARTY_CHAT_PERMISSION_OPTIONS.PARTY_CHAT_PERMISSION_OPTIONS_NONE;
				PartySucceeded(SDK.PartyChatControlSetPermissions(_localChatControlHandle, playerByEntityId._chatControlHandle, pARTY_CHAT_PERMISSION_OPTIONS));
			}
			else
			{
				PartySucceeded(SDK.PartyChatControlSetPermissions(chatPermissionOptions: (_platformPolicyProvider == null) ? ((PARTY_CHAT_PERMISSION_OPTIONS)31u) : _platformPolicyProvider.GetChatPermissions(playerByEntityId), chatControl: _localChatControlHandle, targetChatControl: playerByEntityId._chatControlHandle));
			}
		}
		else
		{
			_LogError("Player not found.");
		}
	}

	internal void _RaiseDataMessageReceivedEvent(PlayFabPlayer fromPlayer, IntPtr buffer, uint bufferSize)
	{
		_LogInfo("PlayFabMultiplayerManager:_RaiseDataMessageReceivedEvent()");
		if (this.OnDataMessageReceived != null)
		{
			byte[] array = new byte[bufferSize];
			if (bufferSize != 0)
			{
				Marshal.Copy(buffer, array, 0, (int)bufferSize);
			}
			this.OnDataMessageReceived(this, fromPlayer, array);
		}
		if (this.OnDataMessageNoCopyReceived != null)
		{
			this.OnDataMessageNoCopyReceived(this, fromPlayer, buffer, bufferSize);
		}
	}

	internal void _RaiseChatMessageReceivedEvent(PlayFabPlayer fromPlayer, string message, ChatMessageType chatMessageType)
	{
		_LogInfo("PlayFabMultiplayerManager:_RaiseChatMessageReceivedEvent()");
		if (this.OnChatMessageReceived != null)
		{
			this.OnChatMessageReceived(this, fromPlayer, message, chatMessageType);
		}
	}

	internal bool _IsOnDataMessageSubscribedTo()
	{
		return this.OnDataMessageReceived != null;
	}

	internal string _GetPlatformSpecificUserId(PlayFab.ClientModels.EntityKey entityKey)
	{
		string result = string.Empty;
		if (entityKey != null)
		{
			PlayFabPlayer playerByEntityId = GetPlayerByEntityId(entityKey.Id);
			if (playerByEntityId != null)
			{
				result = playerByEntityId._platformSpecificUserId;
			}
		}
		return result;
	}

	internal ChatState _GetChatState(PlayFab.ClientModels.EntityKey entityKey, bool _isLocal)
	{
		ChatState result = ChatState.Silent;
		if (_isLocal)
		{
			if (_localChatControlHandle != null)
			{
				SDK.PartyChatControlGetLocalChatIndicator(_localChatControlHandle, out var chatIndicator);
				result = chatIndicator switch
				{
					PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR.PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR_NO_AUDIO_INPUT => ChatState.NoAudioInput, 
					PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR.PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR_AUDIO_INPUT_MUTED => ChatState.Muted, 
					PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR.PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR_SILENT => ChatState.Silent, 
					PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR.PARTY_LOCAL_CHAT_CONTROL_CHAT_INDICATOR_TALKING => ChatState.Talking, 
					_ => ChatState.Silent, 
				};
			}
		}
		else
		{
			PARTY_CHAT_CONTROL_HANDLE pARTY_CHAT_CONTROL_HANDLE = null;
			PlayFabPlayer playFabPlayer = null;
			playFabPlayer = GetPlayerByEntityId(entityKey.Id);
			if (playFabPlayer != null)
			{
				pARTY_CHAT_CONTROL_HANDLE = playFabPlayer._chatControlHandle;
			}
			if (_localChatControlHandle != null && pARTY_CHAT_CONTROL_HANDLE != null)
			{
				PartySucceeded(SDK.PartyChatControlGetChatIndicator(_localChatControlHandle, pARTY_CHAT_CONTROL_HANDLE, out var chatIndicator2));
				result = chatIndicator2 switch
				{
					PARTY_CHAT_CONTROL_CHAT_INDICATOR.PARTY_CHAT_CONTROL_CHAT_INDICATOR_INCOMING_COMMUNICATIONS_MUTED => (playFabPlayer == null || !playFabPlayer._mutedByPlatform) ? ChatState.Muted : ChatState.MutedByPlatform, 
					PARTY_CHAT_CONTROL_CHAT_INDICATOR.PARTY_CHAT_CONTROL_CHAT_INDICATOR_INCOMING_VOICE_DISABLED => ChatState.Muted, 
					PARTY_CHAT_CONTROL_CHAT_INDICATOR.PARTY_CHAT_CONTROL_CHAT_INDICATOR_SILENT => ChatState.Silent, 
					PARTY_CHAT_CONTROL_CHAT_INDICATOR.PARTY_CHAT_CONTROL_CHAT_INDICATOR_TALKING => ChatState.Talking, 
					_ => ChatState.Silent, 
				};
			}
			else
			{
				result = ChatState.NoAudioInput;
			}
		}
		return result;
	}

	internal float _GetVoiceLevel(PlayFab.ClientModels.EntityKey entityKey)
	{
		float volume = 0f;
		PlayFabPlayer playerByEntityId = GetPlayerByEntityId(entityKey.Id);
		if (playerByEntityId != null)
		{
			PartySucceeded(SDK.PartyChatControlGetAudioRenderVolume(_localChatControlHandle, playerByEntityId._chatControlHandle, out volume));
		}
		return volume;
	}

	internal void _SetVoiceLevel(PlayFab.ClientModels.EntityKey entityKey, float voiceLevel, bool _isLocal)
	{
		_LogInfo("PlayFabMultiplayerManager:_SetVoiceLevel()");
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.LocalUserCreated)
		{
			return;
		}
		PARTY_CHAT_CONTROL_HANDLE targetChatControl = null;
		if (_isLocal)
		{
			targetChatControl = _localChatControlHandle;
		}
		else
		{
			PlayFabPlayer playerByEntityId = GetPlayerByEntityId(entityKey.Id);
			if (playerByEntityId != null)
			{
				targetChatControl = playerByEntityId._chatControlHandle;
			}
		}
		PartySucceeded(SDK.PartyChatControlSetAudioRenderVolume(_localChatControlHandle, targetChatControl, voiceLevel));
	}

	internal string _GetLanguageCode(PlayFab.ClientModels.EntityKey entityKey, bool isLocal)
	{
		string languageCode = string.Empty;
		if (_playFabMultiplayerManagerState >= _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork)
		{
			if (isLocal)
			{
				PartySucceeded(SDK.PartyChatControlGetLanguage(_localChatControlHandle, out languageCode));
			}
			else
			{
				PlayFabPlayer playerByEntityId = GetPlayerByEntityId(entityKey.Id);
				if (playerByEntityId != null)
				{
					PartySucceeded(SDK.PartyChatControlGetLanguage(playerByEntityId._chatControlHandle, out languageCode));
				}
			}
		}
		return languageCode;
	}

	internal void _SetPlayFabMultiplayerManagerInternalState(_InternalPlayFabMultiplayerManagerState state)
	{
		_playFabMultiplayerManagerState = state;
	}

	private void SetRemotePlayerChatControlHandle(string entityId, PARTY_CHAT_CONTROL_HANDLE remoteChatControlHandle)
	{
		_LogInfo("PlayFabMultiplayerManager:SetRemotePlayerChatControlHandle()");
		foreach (PlayFabPlayer remotePlayer in _remotePlayers)
		{
			if (remotePlayer.EntityKey.Id == entityId)
			{
				remotePlayer._chatControlHandle = remoteChatControlHandle;
				break;
			}
		}
	}

	internal bool PartySucceeded(uint errorCode)
	{
		bool result = false;
		if (PartyError.FAILED(errorCode))
		{
			_LogError(errorCode);
		}
		else
		{
			result = true;
		}
		return result;
	}

	internal bool PartySucceeded(uint errorCode, PlayFabMultiplayerManagerErrorType errorType)
	{
		bool result = false;
		if (PartyError.FAILED(errorCode))
		{
			_LogError(errorCode, errorType);
		}
		else
		{
			result = true;
		}
		return result;
	}

	internal bool InternalCheckStateChangeSucceededOrLogErrorIfFailed(PARTY_STATE_CHANGE_RESULT result, uint errorCode)
	{
		bool result2 = false;
		switch (result)
		{
		case PARTY_STATE_CHANGE_RESULT.PARTY_STATE_CHANGE_RESULT_SUCCEEDED:
			result2 = true;
			break;
		case PARTY_STATE_CHANGE_RESULT.PARTY_STATE_CHANGE_RESULT_LEAVE_NETWORK_CALLED:
			result2 = true;
			break;
		default:
			InternalCheckStateChangeSucceededOrLogErrorIfFailedImpl(result.ToString(), errorCode);
			break;
		}
		return result2;
	}

	internal bool InternalCheckStateChangeSucceededOrLogErrorIfFailed(PARTY_XBL_STATE_CHANGE_RESULT result, uint errorCode)
	{
		bool result2 = false;
		if (result == PARTY_XBL_STATE_CHANGE_RESULT.PARTY_XBL_STATE_CHANGE_RESULT_SUCCEEDED)
		{
			result2 = true;
		}
		else
		{
			InternalCheckStateChangeSucceededOrLogErrorIfFailedImpl(result.ToString(), errorCode);
		}
		return result2;
	}

	private void InternalCheckStateChangeSucceededOrLogErrorIfFailedImpl(string stateChangeString, uint errorCode)
	{
		_LogError(stateChangeString);
		PartySucceeded(errorCode);
	}

	private bool RaiseErrorIfStateChangedFailed(PARTY_STATE_CHANGE_RESULT result, uint errorCode)
	{
		bool result2 = false;
		if (result == PARTY_STATE_CHANGE_RESULT.PARTY_STATE_CHANGE_RESULT_SUCCEEDED)
		{
			result2 = true;
		}
		else
		{
			PartySucceeded(errorCode);
		}
		return result2;
	}

	private void ProcessStateChanges()
	{
		if (_playFabMultiplayerManagerState < _InternalPlayFabMultiplayerManagerState.LocalUserCreated || _playFabMultiplayerManagerState == _InternalPlayFabMultiplayerManagerState.NotInitialized)
		{
			return;
		}
		if (_partyStateChanges == null)
		{
			_partyStateChanges = new List<PARTY_STATE_CHANGE>();
		}
		if (!PartySucceeded(SDK.PartyStartProcessingStateChanges(_partyHandle, out _partyStateChanges)))
		{
			return;
		}
		foreach (PARTY_STATE_CHANGE partyStateChange in _partyStateChanges)
		{
			_LogInfo("Party State change: " + partyStateChange.StateChangeType);
			switch (partyStateChange.StateChangeType)
			{
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_REGIONS_CHANGED:
			{
				PARTY_REGIONS_CHANGED_STATE_CHANGE pARTY_REGIONS_CHANGED_STATE_CHANGE = (PARTY_REGIONS_CHANGED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_REGIONS_CHANGED_STATE_CHANGE.result, pARTY_REGIONS_CHANGED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_AUTHENTICATE_LOCAL_USER_COMPLETED:
			{
				PARTY_AUTHENTICATE_LOCAL_USER_COMPLETED_STATE_CHANGE pARTY_AUTHENTICATE_LOCAL_USER_COMPLETED_STATE_CHANGE = (PARTY_AUTHENTICATE_LOCAL_USER_COMPLETED_STATE_CHANGE)partyStateChange;
				if (InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_AUTHENTICATE_LOCAL_USER_COMPLETED_STATE_CHANGE.result, pARTY_AUTHENTICATE_LOCAL_USER_COMPLETED_STATE_CHANGE.errorDetail))
				{
					AuthenticateLocalUserComplete();
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_DESTROY_LOCAL_USER_COMPLETED:
			{
				PARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE pARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE = (PARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE)partyStateChange;
				RaiseErrorIfStateChangedFailed(pARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE.result, pARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CREATE_CHAT_CONTROL_COMPLETED:
			{
				PARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE pARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE = (PARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE.result, pARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CREATE_ENDPOINT_COMPLETED:
			{
				PARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE pARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE = (PARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE.result, pARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CREATE_INVITATION_COMPLETED:
			{
				PARTY_CREATE_INVITATION_COMPLETED_STATE_CHANGE pARTY_CREATE_INVITATION_COMPLETED_STATE_CHANGE = (PARTY_CREATE_INVITATION_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_CREATE_INVITATION_COMPLETED_STATE_CHANGE.result, pARTY_CREATE_INVITATION_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CREATE_NEW_NETWORK_COMPLETED:
			{
				PARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE pARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE = (PARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE)partyStateChange;
				if (!InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE.result, pARTY_CREATE_NEW_NETWORK_COMPLETED_STATE_CHANGE.errorDetail))
				{
					ResetNetworkManagerStateAfterFailureToConnect();
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_DESTROY_CHAT_CONTROL_COMPLETED:
			{
				PARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE pARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE = (PARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE.result, pARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_DESTROY_ENDPOINT_COMPLETED:
			{
				PARTY_DESTROY_ENDPOINT_COMPLETED_STATE_CHANGE pARTY_DESTROY_ENDPOINT_COMPLETED_STATE_CHANGE = (PARTY_DESTROY_ENDPOINT_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_DESTROY_ENDPOINT_COMPLETED_STATE_CHANGE.result, pARTY_DESTROY_ENDPOINT_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_DISCONNECT_CHAT_CONTROL_COMPLETED:
			{
				PARTY_DISCONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE pARTY_DISCONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE = (PARTY_DISCONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_DISCONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE.result, pARTY_DISCONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_LEAVE_NETWORK_COMPLETED:
			{
				PARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE pARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE = (PARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE)partyStateChange;
				if (InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE.result, pARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE.errorDetail))
				{
					_isLeaveNetworkInProgress = false;
					if (this.OnNetworkLeft != null)
					{
						this.OnNetworkLeft(this, _networkId);
						_networkId = null;
					}
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_LOCAL_CHAT_AUDIO_INPUT_CHANGED:
			{
				uint errorDetail2 = ((PARTY_LOCAL_CHAT_AUDIO_INPUT_CHANGED_STATE_CHANGE)partyStateChange).errorDetail;
				if (!PartySucceeded(errorDetail2) && errorDetail2 == 6)
				{
					_LogWarning("No audio input device found.");
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_LOCAL_CHAT_AUDIO_OUTPUT_CHANGED:
			{
				uint errorDetail = ((PARTY_LOCAL_CHAT_AUDIO_OUTPUT_CHANGED_STATE_CHANGE)partyStateChange).errorDetail;
				if (!PartySucceeded(errorDetail) && errorDetail == 6)
				{
					_LogWarning("No audio output device found.");
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_NETWORK_DESTROYED:
			{
				PARTY_NETWORK_DESTROYED_STATE_CHANGE pARTY_NETWORK_DESTROYED_STATE_CHANGE = (PARTY_NETWORK_DESTROYED_STATE_CHANGE)partyStateChange;
				if (PartySucceeded(pARTY_NETWORK_DESTROYED_STATE_CHANGE.errorDetail) && (_queuedCreateAndJoinAfterLeaveNetworkOp.queued || _queuedCompleteJoinAfterLeaveNetworkOp.queued))
				{
					if (_queuedCreateAndJoinAfterLeaveNetworkOp.queued)
					{
						_queuedCreateAndJoinAfterLeaveNetworkOp.queued = false;
						CreateAndJoinNetworkImplComplete(_queuedCreateAndJoinAfterLeaveNetworkOp.networkConfiguration);
					}
					if (_queuedCompleteJoinAfterLeaveNetworkOp.queued)
					{
						_queuedCompleteJoinAfterLeaveNetworkOp.queued = false;
						JoinNetworkImplComplete(_queuedCompleteJoinAfterLeaveNetworkOp.networkId);
					}
				}
				_playFabMultiplayerManagerState = _InternalPlayFabMultiplayerManagerState.Initialized;
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_REMOVE_LOCAL_USER_COMPLETED:
			{
				PARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE pARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE = (PARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE.result, pARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_REVOKE_INVITATION_COMPLETED:
			{
				PARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE pARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE = (PARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE.result, pARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SET_TEXT_CHAT_OPTIONS_COMPLETED:
			{
				PARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE pARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE = (PARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE.result, pARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SET_TEXT_TO_SPEECH_PROFILE_COMPLETED:
			{
				PARTY_SET_TEXT_TO_SPEECH_PROFILE_COMPLETED_STATE_CHANGE pARTY_SET_TEXT_TO_SPEECH_PROFILE_COMPLETED_STATE_CHANGE = (PARTY_SET_TEXT_TO_SPEECH_PROFILE_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SET_TEXT_TO_SPEECH_PROFILE_COMPLETED_STATE_CHANGE.result, pARTY_SET_TEXT_TO_SPEECH_PROFILE_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SET_TRANSCRIPTION_OPTIONS_COMPLETED:
			{
				PARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE pARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE = (PARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE.result, pARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SYNTHESIZE_TEXT_TO_SPEECH_COMPLETED:
			{
				PARTY_SYNTHESIZE_TEXT_TO_SPEECH_COMPLETED_STATE_CHANGE pARTY_SYNTHESIZE_TEXT_TO_SPEECH_COMPLETED_STATE_CHANGE = (PARTY_SYNTHESIZE_TEXT_TO_SPEECH_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SYNTHESIZE_TEXT_TO_SPEECH_COMPLETED_STATE_CHANGE.result, pARTY_SYNTHESIZE_TEXT_TO_SPEECH_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_ENDPOINT_CREATED:
			{
				PARTY_ENDPOINT_CREATED_STATE_CHANGE pARTY_ENDPOINT_CREATED_STATE_CHANGE = (PARTY_ENDPOINT_CREATED_STATE_CHANGE)partyStateChange;
				PartySucceeded(SDK.PartyNetworkGetNetworkDescriptor(pARTY_ENDPOINT_CREATED_STATE_CHANGE.network, out _networkDescriptor));
				PARTY_ENDPOINT_HANDLE endpoint2 = pARTY_ENDPOINT_CREATED_STATE_CHANGE.endpoint;
				string entityId5 = string.Empty;
				PartySucceeded(SDK.PartyEndpointGetEntityId(endpoint2, out entityId5));
				bool isLocal = false;
				PartySucceeded(SDK.PartyEndpointIsLocal(endpoint2, out isLocal));
				if (isLocal)
				{
					break;
				}
				PlayFabPlayer playFabPlayer2 = GetPlayerByEntityId(entityId5);
				if (playFabPlayer2 == null)
				{
					playFabPlayer2 = new PlayFabPlayer();
					playFabPlayer2._endPointHandle = endpoint2;
					playFabPlayer2._isLocal = isLocal;
					PlayFab.ClientModels.EntityKey entityKey = new PlayFab.ClientModels.EntityKey();
					entityKey.Id = entityId5;
					entityKey.Type = "title_player_account";
					playFabPlayer2._SetEntityKey(entityKey);
					if (_platformPolicyProvider != null)
					{
						_platformPolicyProvider.CreateOrUpdatePlatformUser(playFabPlayer2, isLocal);
						_platformPolicyProvider.SendPlatformSpecificUserId(new List<PlayFabPlayer> { playFabPlayer2 });
					}
					_remotePlayers.Add(playFabPlayer2);
				}
				if (this.OnRemotePlayerJoined != null)
				{
					this.OnRemotePlayerJoined(this, playFabPlayer2);
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_ENDPOINT_DESTROYED:
			{
				PARTY_ENDPOINT_DESTROYED_STATE_CHANGE pARTY_ENDPOINT_DESTROYED_STATE_CHANGE = (PARTY_ENDPOINT_DESTROYED_STATE_CHANGE)partyStateChange;
				PartySucceeded(pARTY_ENDPOINT_DESTROYED_STATE_CHANGE.errorDetail);
				PartySucceeded(SDK.PartyNetworkGetNetworkDescriptor(pARTY_ENDPOINT_DESTROYED_STATE_CHANGE.network, out _networkDescriptor));
				PARTY_ENDPOINT_HANDLE endpoint = pARTY_ENDPOINT_DESTROYED_STATE_CHANGE.endpoint;
				string entityId3 = string.Empty;
				PartySucceeded(SDK.PartyEndpointGetEntityId(endpoint, out entityId3));
				if (!(entityId3 == _localPlayer.EntityKey.Id) && 0 == 0)
				{
					PlayFabPlayer playerByEntityId2 = GetPlayerByEntityId(entityId3);
					if (playerByEntityId2 != null)
					{
						_remotePlayers.Remove(playerByEntityId2);
					}
					if (this.OnRemotePlayerLeft != null)
					{
						this.OnRemotePlayerLeft(this, playerByEntityId2);
					}
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CHAT_CONTROL_CREATED:
			{
				PARTY_CHAT_CONTROL_HANDLE chatControl = ((PARTY_CHAT_CONTROL_CREATED_STATE_CHANGE)partyStateChange).chatControl;
				string entityId2 = string.Empty;
				SDK.PartyChatControlGetEntityId(chatControl, out entityId2);
				PlayFabPlayer playerByEntityId = GetPlayerByEntityId(entityId2);
				if (playerByEntityId == null)
				{
					break;
				}
				SetRemotePlayerChatControlHandle(playerByEntityId.EntityKey.Id, chatControl);
				UpdateCachedChatControlsList();
				if (!playerByEntityId.IsMuted && !playerByEntityId._isLocal)
				{
					PARTY_CHAT_PERMISSION_OPTIONS chatPermissionOptions = (PARTY_CHAT_PERMISSION_OPTIONS)31u;
					if (_platformPolicyProvider != null)
					{
						chatPermissionOptions = _platformPolicyProvider.GetChatPermissions(playerByEntityId);
					}
					PartySucceeded(SDK.PartyChatControlSetPermissions(_localChatControlHandle, chatControl, chatPermissionOptions));
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CHAT_CONTROL_DESTROYED:
			{
				PARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE pARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE = (PARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE)partyStateChange;
				PartySucceeded(pARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE.errorDetail);
				UpdateCachedChatControlsList();
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CHAT_CONTROL_LEFT_NETWORK:
			{
				PARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE pARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE = (PARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE)partyStateChange;
				PartySucceeded(pARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CHAT_TEXT_RECEIVED:
			{
				PARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE pARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE = (PARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE)partyStateChange;
				PARTY_CHAT_CONTROL_HANDLE senderChatControl2 = pARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE.senderChatControl;
				string entityId6 = string.Empty;
				PartySucceeded(SDK.PartyChatControlGetEntityId(senderChatControl2, out entityId6));
				PlayFabPlayer playerByEntityId4 = GetPlayerByEntityId(entityId6);
				if (playerByEntityId4 != null)
				{
					string message2 = ((pARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE.translations.Length == 0) ? pARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE.chatText : pARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE.translations[0].translation);
					_RaiseChatMessageReceivedEvent(playerByEntityId4, message2, ChatMessageType.Text);
				}
				else
				{
					_LogError("Player not found.");
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CONNECT_CHAT_CONTROL_COMPLETED:
			{
				PARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE pARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE = (PARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE.result, pARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_CONNECT_TO_NETWORK_COMPLETED:
			{
				PARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE pARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE = (PARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE)partyStateChange;
				if (InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE.result, pARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE.errorDetail))
				{
					_networkDescriptor = pARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE.networkDescriptor;
					UpdateNetworkId(_generatedInvitationId, _networkDescriptor);
					AuthenticateLocalUserStart();
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_REMOTE_DEVICE_LEFT_NETWORK:
			{
				PARTY_REMOTE_DEVICE_LEFT_NETWORK_STATE_CHANGE pARTY_REMOTE_DEVICE_LEFT_NETWORK_STATE_CHANGE = (PARTY_REMOTE_DEVICE_LEFT_NETWORK_STATE_CHANGE)partyStateChange;
				PartySucceeded(pARTY_REMOTE_DEVICE_LEFT_NETWORK_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_NETWORK_DESCRIPTOR_CHANGED:
			{
				PARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE pARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE = (PARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE)partyStateChange;
				if (this.OnNetworkChanged != null)
				{
					PARTY_NETWORK_HANDLE network = (_networkHandle = pARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE.network);
					SDK.PartyNetworkGetNetworkDescriptor(network, out var networkDescriptor);
					SDK.PartyNetworkGetInvitations(network, out var invitations);
					string invitationId = string.Empty;
					if (invitations.Length != 1 && PartySucceeded(SDK.PartyInvitationGetInvitationConfiguration(invitations[0], out var configuration)))
					{
						invitationId = configuration.Identifier;
					}
					UpdateNetworkId(invitationId, networkDescriptor);
					this.OnNetworkChanged(this, _networkId);
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_ENDPOINT_MESSAGE_RECEIVED:
			{
				PARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE = (PARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE)partyStateChange;
				PARTY_ENDPOINT_HANDLE senderEndpoint = pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.senderEndpoint;
				string entityId4 = string.Empty;
				PartySucceeded(SDK.PartyEndpointGetEntityId(senderEndpoint, out entityId4));
				PlayFabPlayer playerByEntityId3 = GetPlayerByEntityId(entityId4);
				if (playerByEntityId3 != null)
				{
					bool isInternalMessage = false;
					if (_platformPolicyProvider != null)
					{
						_platformPolicyProvider.ProcessEndpointMessage(playerByEntityId3, pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.messageBuffer, pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.messageSize, out isInternalMessage);
					}
					if (!isInternalMessage && !IsInternalMessage(pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.messageBuffer, pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.messageSize))
					{
						_RaiseDataMessageReceivedEvent(playerByEntityId3, pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.messageBuffer, pARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE.messageSize);
					}
				}
				else
				{
					_LogError("Player not found.");
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_INVITATION_DESTROYED:
			{
				PARTY_INVITATION_DESTROYED_STATE_CHANGE pARTY_INVITATION_DESTROYED_STATE_CHANGE = (PARTY_INVITATION_DESTROYED_STATE_CHANGE)partyStateChange;
				PartySucceeded(pARTY_INVITATION_DESTROYED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_VOICE_CHAT_TRANSCRIPTION_RECEIVED:
			{
				PARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE = (PARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE)partyStateChange;
				if (InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.result, pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.errorDetail) && pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.type == PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE.PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE_FINAL)
				{
					PARTY_CHAT_CONTROL_HANDLE senderChatControl = pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.senderChatControl;
					string entityId = string.Empty;
					PartySucceeded(SDK.PartyChatControlGetEntityId(senderChatControl, out entityId));
					PlayFabPlayer playFabPlayer = ((!(LocalPlayer.EntityKey.Id == entityId)) ? GetPlayerByEntityId(entityId) : LocalPlayer);
					if (playFabPlayer != null)
					{
						string message = ((pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.translations.Count <= 0) ? pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.transcription : pARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE.translations[0].translation);
						_RaiseChatMessageReceivedEvent(playFabPlayer, message, ChatMessageType.SpeechToText);
					}
					else
					{
						_LogError("Player not found.");
					}
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SET_LANGUAGE_COMPLETED:
			{
				PARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE pARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE = (PARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE.result, pARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED:
			{
				PARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE pARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE = (PARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE)partyStateChange;
				if (InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE.result, pARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE.errorDetail))
				{
					PARTY_GENDER gender = PARTY_GENDER.PARTY_GENDER_NEUTRAL;
					string identifier = string.Empty;
					string languageCode = string.Empty;
					string empty = string.Empty;
					if (PartySucceeded(SDK.PartyChatControlGetAvailableTextToSpeechProfiles(pARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE.localChatControl, out var profiles)) && profiles.Length != 0)
					{
						PartySucceeded(SDK.PartyTextToSpeechProfileGetGender(profiles[0], out gender));
						PartySucceeded(SDK.PartyTextToSpeechProfileGetIdentifier(profiles[0], out identifier));
						PartySucceeded(SDK.PartyTextToSpeechProfileGetLanguageCode(profiles[0], out languageCode));
						PartySucceeded(SDK.PartyTextToSpeechProfileGetName(profiles[0], out empty));
					}
					PartySucceeded(SDK.PartyChatControlSetTextToSpeechProfile(pARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE.localChatControl, PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE.PARTY_SYNTHESIZE_TEXT_TO_SPEECH_TYPE_VOICE_CHAT, identifier, null));
				}
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SET_CHAT_AUDIO_INPUT_COMPLETED:
			{
				PARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE pARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE = (PARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE.result, pARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			case PARTY_STATE_CHANGE_TYPE.PARTY_STATE_CHANGE_TYPE_SET_CHAT_AUDIO_OUTPUT_COMPLETED:
			{
				PARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE pARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE = (PARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE)partyStateChange;
				InternalCheckStateChangeSucceededOrLogErrorIfFailed(pARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE.result, pARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE.errorDetail);
				break;
			}
			}
		}
		PartySucceeded(SDK.PartyFinishProcessingStateChanges(_partyHandle, _partyStateChanges));
	}

	public void ResetParty()
	{
		Debug.Log("ResetParty");
		_tasks.Clear();
		_runningTask = null;
		PlayFabMultiplayerManager playFabMultiplayerManager = Get();
		if (playFabMultiplayerManager.IsNotInitializedState() || playFabMultiplayerManager.IsPendingInitializationState())
		{
			Debug.Log("No reinitialization required.");
			return;
		}
		if (_networkId != null && playFabMultiplayerManager.IsConnectedToNetworkState())
		{
			AddTask(new LeaveNetworkTask());
		}
		AddTask(new CleanPartyTask());
		AddTask(new InitPartyTask());
		if (_networkId != null && playFabMultiplayerManager.IsConnectedToNetworkState())
		{
			AddTask(new JoinPartyTask(_networkId));
		}
	}

	private void AddTask(WorkTask task)
	{
		_tasks.Add(task);
	}

	private bool IsNotInitializedState()
	{
		return _playFabMultiplayerManagerState == _InternalPlayFabMultiplayerManagerState.NotInitialized;
	}

	private bool IsPendingInitializationState()
	{
		return _playFabMultiplayerManagerState == _InternalPlayFabMultiplayerManagerState.PendingInitialization;
	}

	private bool IsInitializedState()
	{
		return _playFabMultiplayerManagerState == _InternalPlayFabMultiplayerManagerState.Initialized;
	}

	private bool IsConnectedToNetworkState()
	{
		return _playFabMultiplayerManagerState == _InternalPlayFabMultiplayerManagerState.ConnectedToNetwork;
	}

	private void ProcessTask()
	{
		if (_runningTask == null)
		{
			while (_tasks.Count > 0)
			{
				_runningTask = _tasks[0];
				_tasks.RemoveAt(0);
				if (_runningTask.Begin())
				{
					break;
				}
			}
		}
		else if (_runningTask.Run())
		{
			_runningTask.End();
			_runningTask = null;
		}
	}

	private bool HasTasks()
	{
		if (_runningTask != null)
		{
			return true;
		}
		return _tasks.Count > 0;
	}
}
