using System.Collections.Generic;
using System.Xml.Serialization;

public struct TriggerRef : IScriptListItem<TriggerRef>
{
	[XmlIgnore]
	public Trigger Obj;

	[XmlAttribute]
	public string UniqueID;

	public Trigger GetTrigger()
	{
		return Obj;
	}

	public string GetUniqueID()
	{
		return UniqueID;
	}

	public static TriggerRef Create(string uniqueID)
	{
		return new TriggerRef
		{
			UniqueID = uniqueID,
			Obj = (string.IsNullOrEmpty(uniqueID) ? null : GameImpl.Instance.GetCurrentlyEditingStory().FindTriggerByUniqueID(uniqueID))
		};
	}

	public TriggerRef FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		string value = UniqueID;
		if (newUniqueIDs != null && UniqueID != null && newUniqueIDs.TryGetValue(UniqueID, out value))
		{
			UniqueID = value;
		}
		return new TriggerRef
		{
			UniqueID = UniqueID,
			Obj = (string.IsNullOrEmpty(UniqueID) ? null : story.FindTriggerByUniqueID(UniqueID))
		};
	}

	public string GetTypeName()
	{
		return "Trigger Ref";
	}

	public void Construct()
	{
	}

	public static implicit operator Trigger(TriggerRef r)
	{
		return r.Obj;
	}
}
