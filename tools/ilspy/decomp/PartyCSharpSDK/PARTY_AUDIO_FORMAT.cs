using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_AUDIO_FORMAT
{
	public uint SamplesPerSecond { get; }

	public uint ChannelMask { get; }

	public ushort ChannelCount { get; }

	public ushort BitsPerSample { get; }

	public PARTY_AUDIO_SAMPLE_TYPE SampleType { get; }

	public byte Interleaved { get; }

	internal PARTY_AUDIO_FORMAT(PartyCSharpSDK.Interop.PARTY_AUDIO_FORMAT interopStruct)
	{
		SamplesPerSecond = interopStruct.samplesPerSecond;
		ChannelMask = interopStruct.channelMask;
		ChannelCount = interopStruct.channelCount;
		BitsPerSample = interopStruct.bitsPerSample;
		SampleType = interopStruct.sampleType;
		Interleaved = interopStruct.interleaved;
	}
}
