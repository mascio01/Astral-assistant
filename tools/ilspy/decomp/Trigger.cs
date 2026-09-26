using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public class Trigger : BaseScriptObject
{
	public TriggerType Type;

	[DefaultValue(false)]
	public bool RunAtStart;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float TriggerRepeatTime;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool OnceOnly;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool StartEnabled;

	public List<Condition> Conditions;

	public List<StoryEvent> Events;

	public override void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		if (Conditions != null)
		{
			for (int i = 0; i < Conditions.Count; i++)
			{
				Conditions[i] = Conditions[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Events != null)
		{
			for (int j = 0; j < Events.Count; j++)
			{
				Events[j] = Events[j].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (RunAtStart)
		{
			Type = TriggerType.GameStart;
			RunAtStart = false;
		}
		if (Conditions != null && Conditions.Count == 0)
		{
			Conditions = null;
		}
		if (Events != null && Events.Count == 0)
		{
			Events = null;
		}
	}
}
