using System.Collections.Generic;

public interface IScriptListItem<T>
{
	string GetTypeName();

	void Construct();

	T FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs);
}
