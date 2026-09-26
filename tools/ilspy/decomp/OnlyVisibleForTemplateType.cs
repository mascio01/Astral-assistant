using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForTemplateType : Attribute
{
	public TemplateType TemplateType;

	public TemplateType TemplateType2;

	public OnlyVisibleForTemplateType(TemplateType templateType, TemplateType templateType2 = TemplateType.Invalid)
	{
		TemplateType = templateType;
		TemplateType2 = templateType2;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			if (template.Type != TemplateType)
			{
				return template.Type == TemplateType2;
			}
			return true;
		}
		return false;
	}
}
