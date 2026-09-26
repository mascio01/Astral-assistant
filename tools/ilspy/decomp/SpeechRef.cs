using System.Collections.Generic;
using System.Xml.Serialization;

public struct SpeechRef : IScriptListItem<SpeechRef>, IReflectable
{
	[XmlIgnore]
	public Speech Obj;

	[XmlAttribute]
	public string UniqueID;

	public Speech GetSpeech()
	{
		return Obj;
	}

	public string GetUniqueID()
	{
		return UniqueID;
	}

	public static SpeechRef Create(string uniqueID)
	{
		return new SpeechRef
		{
			UniqueID = uniqueID,
			Obj = (string.IsNullOrEmpty(uniqueID) ? null : GameImpl.Instance.GetCurrentlyEditingStory().FindSpeechByUniqueID(uniqueID))
		};
	}

	public static SpeechRef Create(Speech speech)
	{
		return new SpeechRef
		{
			UniqueID = speech.UniqueID,
			Obj = speech
		};
	}

	public SpeechRef FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		string value = UniqueID;
		if (newUniqueIDs != null && UniqueID != null && newUniqueIDs.TryGetValue(UniqueID, out value))
		{
			UniqueID = value;
		}
		return new SpeechRef
		{
			UniqueID = UniqueID,
			Obj = (string.IsNullOrEmpty(UniqueID) ? null : GameImpl.Instance.FindSpeechByUniqueID(UniqueID))
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref UniqueID);
		if (reflector.IsDeserialising)
		{
			Obj = (string.IsNullOrEmpty(UniqueID) ? null : GameImpl.Instance.FindSpeechByUniqueID(UniqueID));
		}
	}

	public string GetTypeName()
	{
		return "Speech Ref";
	}

	public void Construct()
	{
	}

	public static implicit operator Speech(SpeechRef r)
	{
		return r.Obj;
	}
}
