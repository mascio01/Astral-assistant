namespace PartyCSharpSDK.Interop;

internal struct PARTY_AUDIO_MANIPULATION_SOURCE_STREAM_CONFIGURATION
{
	internal readonly PARTY_AUDIO_FORMAT format;

	internal readonly uint maxTotalAudioBufferSizeInMilliseconds;

	internal PARTY_AUDIO_MANIPULATION_SOURCE_STREAM_CONFIGURATION(PartyCSharpSDK.PARTY_AUDIO_MANIPULATION_SOURCE_STREAM_CONFIGURATION publicObject)
	{
		format = new PARTY_AUDIO_FORMAT(publicObject.Format);
		maxTotalAudioBufferSizeInMilliseconds = publicObject.MaxTotalAudioBufferSizeInMilliseconds;
	}
}
