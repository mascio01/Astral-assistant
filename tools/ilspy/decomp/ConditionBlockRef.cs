using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public struct ConditionBlockRef : IScriptListItem<ConditionBlockRef>
{
	[XmlIgnore]
	public ConditionBlock Obj;

	[XmlAttribute]
	[DefaultValue("")]
	public string UniqueID;

	public ConditionBlock GetConditionBlock()
	{
		return Obj;
	}

	public string GetUniqueID()
	{
		return UniqueID;
	}

	public static ConditionBlockRef Create(string uniqueID)
	{
		return new ConditionBlockRef
		{
			UniqueID = uniqueID,
			Obj = (string.IsNullOrEmpty(uniqueID) ? null : GameImpl.Instance.GetCurrentlyEditingStory().FindConditionBlockByUniqueID(uniqueID))
		};
	}

	public ConditionBlockRef FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		string value = UniqueID;
		if (newUniqueIDs != null && UniqueID != null && newUniqueIDs.TryGetValue(UniqueID, out value))
		{
			UniqueID = value;
		}
		return new ConditionBlockRef
		{
			UniqueID = UniqueID,
			Obj = (string.IsNullOrEmpty(UniqueID) ? null : GameImpl.Instance.FindConditionBlockByUniqueID(UniqueID))
		};
	}

	public string GetTypeName()
	{
		return "Condition Block Ref";
	}

	public void Construct()
	{
	}

	public static implicit operator ConditionBlock(ConditionBlockRef r)
	{
		return r.Obj;
	}
}
