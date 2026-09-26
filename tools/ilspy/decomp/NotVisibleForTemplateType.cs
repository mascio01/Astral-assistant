using System;

[AttributeUsage(AttributeTargets.Field)]
public class NotVisibleForTemplateType : Attribute
{
	public TemplateType TemplateType;

	public NotVisibleForTemplateType(TemplateType templateType)
	{
		TemplateType = templateType;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			return template.Type != TemplateType;
		}
		return false;
	}
}
