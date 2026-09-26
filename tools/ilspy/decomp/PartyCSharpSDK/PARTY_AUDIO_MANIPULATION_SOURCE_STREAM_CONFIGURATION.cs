using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_AUDIO_MANIPULATION_SOURCE_STREAM_CONFIGURATION
{
	public PARTY_AUDIO_FORMAT Format { get; }

	public uint MaxTotalAudioBufferSizeInMilliseconds { get; }

	internal PARTY_AUDIO_MANIPULATION_SOURCE_STREAM_CONFIGURATION(PartyCSharpSDK.Interop.PARTY_AUDIO_MANIPULATION_SOURCE_STREAM_CONFIGURATION interopStruct)
	{
		Format = new PARTY_AUDIO_FORMAT(interopStruct.format);
		MaxTotalAudioBufferSizeInMilliseconds = interopStruct.maxTotalAudioBufferSizeInMilliseconds;
	}
}
