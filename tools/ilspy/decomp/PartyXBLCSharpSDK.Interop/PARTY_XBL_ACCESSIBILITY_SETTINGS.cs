using PartyCSharpSDK;

namespace PartyXBLCSharpSDK.Interop;

internal struct PARTY_XBL_ACCESSIBILITY_SETTINGS
{
	internal readonly byte speechToTextEnabled;

	internal readonly byte textToSpeechEnabled;

	private unsafe fixed byte languageCode[85];

	internal readonly PARTY_GENDER gender;

	internal unsafe string GetLanguageCode()
	{
		fixed (byte* bytePointer = languageCode)
		{
			return Converters.BytePointerToString(bytePointer, 85);
		}
	}

	internal unsafe PARTY_XBL_ACCESSIBILITY_SETTINGS(PartyXBLCSharpSDK.PARTY_XBL_ACCESSIBILITY_SETTINGS publicObject)
	{
		speechToTextEnabled = publicObject.SpeechToTextEnabled;
		textToSpeechEnabled = publicObject.TextToSpeechEnabled;
		fixed (byte* bytePointer = languageCode)
		{
			Converters.StringToNullTerminatedUTF8FixedPointer(publicObject.LanguageCode, bytePointer, 85);
		}
		gender = publicObject.Gender;
	}
}
