using System.Collections.Generic;
using System.Xml.Serialization;

public struct TemplateChild : IScriptListItem<TemplateChild>
{
	[XmlAttribute]
	public string UniqueID;

	[XmlAttribute]
	public int Min;

	[XmlAttribute]
	public int Max;

	public ConditionBlockRef MinFormula;

	public ConditionBlockRef MaxFormula;

	public string GetTypeName()
	{
		return "Template Child";
	}

	public void Construct()
	{
		Min = 1;
		Max = 1;
	}

	public TemplateChild FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		TemplateChild result = this;
		result.MinFormula = result.MinFormula.FixupAfterXmlLoad(script, story, newUniqueIDs);
		result.MaxFormula = result.MaxFormula.FixupAfterXmlLoad(script, story, newUniqueIDs);
		return result;
	}
}
