using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public struct QuestMarker : IScriptListItem<QuestMarker>
{
	[XmlAttribute]
	[DefaultValue(Specifier.Invalid)]
	public Specifier Specifier;

	[XmlAttribute]
	[DefaultValue(SpecifierModifier.None)]
	public SpecifierModifier Modifier;

	[XmlAttribute]
	public string UniqueID;

	public QuestMarker FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		QuestMarker result = this;
		Condition.FixupSpecifier(ref result.Specifier, ref result.Modifier, ref result.UniqueID);
		return result;
	}

	public string GetTypeName()
	{
		return "Quest Marker";
	}

	public void Construct()
	{
	}

	public T GetSubject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return Condition.ResolveSpecifier<T>(Specifier, Modifier, UniqueID, actor, target, obj, param);
	}
}
