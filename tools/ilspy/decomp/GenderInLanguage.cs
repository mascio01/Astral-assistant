using System.ComponentModel;
using System.Xml.Serialization;

public class GenderInLanguage
{
	[XmlAttribute]
	[DefaultValue(GenderType.Count)]
	public GenderType Gender = GenderType.Count;

	[XmlAttribute]
	[DefaultValue(Language.Invalid)]
	public Language Language = Language.Invalid;

	public GenderInLanguage()
	{
	}

	public GenderInLanguage(GenderType gender, Language language)
	{
		Gender = gender;
		Language = language;
	}
}
