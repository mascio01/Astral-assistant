using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class BaseScriptObject
{
	[XmlAttribute]
	public string UniqueID = "";

	[XmlAttribute]
	public float x;

	[XmlAttribute]
	public float y;

	[XmlIgnore]
	public Vector2 EditorPos
	{
		get
		{
			return new Vector2(x, y);
		}
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	public virtual void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
	}

	public virtual bool MatchText(string txt)
	{
		return UniqueID.ToLower().Contains(txt);
	}

	public virtual void OnUniqueIDChanged()
	{
	}
}
