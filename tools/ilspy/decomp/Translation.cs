using System.Collections.Generic;
using System.IO;

public class Translation
{
	public Dictionary<int, string> Keys = new Dictionary<int, string>();

	public static string Tag = "Tag";

	public static Translation LoadFromFile(string tsvFileName, bool justTitleAndDescription)
	{
		Translation translation = new Translation();
		if (File.Exists(tsvFileName))
		{
			using StreamReader streamReader = new StreamReader(tsvFileName);
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			while (!streamReader.EndOfStream)
			{
				string[] array = streamReader.ReadLine().Split('\t');
				if (array.Length >= 2)
				{
					string text = array[0].Trim();
					string text2 = array[1];
					if (text2.Length > 0)
					{
						if (num == 0 && text == Tag)
						{
							continue;
						}
						int key = StringUtil.JenkinsHash(text);
						flag = text == StorySettings.NameKey;
						flag2 = text == StorySettings.DescriptionKey;
						translation.Keys[key] = text2;
						if (justTitleAndDescription && flag && flag2)
						{
							break;
						}
					}
				}
				num++;
			}
		}
		return translation;
	}

	public string Translate(string key)
	{
		Keys.TryGetValue(StringUtil.JenkinsHash(key), out var value);
		return value;
	}

	public string Translate(int hash)
	{
		Keys.TryGetValue(hash, out var value);
		return value;
	}
}
