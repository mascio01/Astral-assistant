using PartyCSharpSDK;

namespace PlayFab.Party;

public class PlayFabNetworkConfiguration
{
	private uint _maxPlayerCount;

	private const uint _MAX_SUPPORTED_PLAYER_COUNT = 32u;

	private PARTY_DIRECT_PEER_CONNECTIVITY_OPTIONS _directPeerConnectivityOptions;

	private const string _ErrorMessageMaxUserCountValueOutOfRange = "Value must be between 1 and {0}";

	public uint MaxPlayerCount
	{
		get
		{
			return _maxPlayerCount;
		}
		set
		{
			if (value != 0 && value <= 32)
			{
				_maxPlayerCount = value;
			}
			else
			{
				PlayFabMultiplayerManager._LogError("Value must be between 1 and {0}".Replace("{0}", 32u.ToString()));
			}
		}
	}

	public PARTY_DIRECT_PEER_CONNECTIVITY_OPTIONS DirectPeerConnectivityOptions
	{
		get
		{
			return _directPeerConnectivityOptions;
		}
		set
		{
			_directPeerConnectivityOptions = value;
		}
	}

	public PlayFabNetworkConfiguration()
	{
		_maxPlayerCount = 32u;
		_directPeerConnectivityOptions = (PARTY_DIRECT_PEER_CONNECTIVITY_OPTIONS)15u;
	}
}
