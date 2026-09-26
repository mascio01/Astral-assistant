namespace PartyCSharpSDK.Interop;

internal struct PARTY_AUDIO_FORMAT
{
	internal readonly uint samplesPerSecond;

	internal readonly uint channelMask;

	internal readonly ushort channelCount;

	internal readonly ushort bitsPerSample;

	internal readonly PARTY_AUDIO_SAMPLE_TYPE sampleType;

	internal readonly byte interleaved;

	internal PARTY_AUDIO_FORMAT(PartyCSharpSDK.PARTY_AUDIO_FORMAT publicObject)
	{
		samplesPerSecond = publicObject.SamplesPerSecond;
		channelMask = publicObject.ChannelMask;
		channelCount = publicObject.ChannelCount;
		bitsPerSample = publicObject.BitsPerSample;
		sampleType = publicObject.SampleType;
		interleaved = publicObject.Interleaved;
	}
}
