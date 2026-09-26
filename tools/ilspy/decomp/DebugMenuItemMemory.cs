using System;
using UnityEngine;

public class DebugMenuItemMemory : DebugMenuItem
{
	private Character Character;

	private int MemoryIndex;

	private string ActorName;

	private string ObjectName;

	private string ThirdPartyName;

	private bool ActorUpdated;

	private bool ObjectUpdated;

	private bool ThirdPartyUpdated;

	private const string Left = "<";

	private const string Right = ">";

	private static string TimeFormat = "dd\\.hh\\:mm\\:ss";

	public DebugMenuItemMemory(Character character, int memoryIndex)
		: base(null)
	{
		Character = character;
		MemoryIndex = memoryIndex;
		Character actor = Character.Memories[MemoryIndex].Actor;
		BaseObject baseObject = Character.Memories[MemoryIndex].Object;
		BaseObject thirdParty = Character.Memories[MemoryIndex].ThirdParty;
		ActorName = ((actor != null) ? actor.GetDisplayNameString() : "");
		ObjectName = ((baseObject != null) ? baseObject.GetDisplayNameString() : "");
		ThirdPartyName = ((thirdParty != null) ? thirdParty.GetDisplayNameString() : "");
	}

	public override void Update(Vector2 pos)
	{
		if (MemoryIndex < Character.Memories.Count)
		{
			float x = pos.x;
			GUI.Label(new Rect(x, pos.y, 200f, 30f), Character.Memories[MemoryIndex].Prototype.UniqueID);
			x += 200f;
			string text = GUI.TextField(new Rect(x, pos.y, 120f, 30f), ActorName);
			if (text != ActorName)
			{
				ActorName = text;
				ActorUpdated = true;
				Memory value = Character.Memories[MemoryIndex];
				value.Actor = FindBaseObjectFromString(ActorName) as Character;
				Character.Memories[MemoryIndex] = value;
			}
			if (ActorUpdated && GUI.Button(new Rect(x + 120f, pos.y, 30f, 30f), "*"))
			{
				Character actor = Character.Memories[MemoryIndex].Actor;
				ActorName = ((actor != null) ? actor.GetDisplayNameString() : "");
				ActorUpdated = false;
			}
			x += 150f;
			text = GUI.TextField(new Rect(x, pos.y, 120f, 30f), ObjectName);
			if (text != ObjectName)
			{
				ObjectName = text;
				ObjectUpdated = true;
				Memory value2 = Character.Memories[MemoryIndex];
				value2.Object = FindBaseObjectFromString(ObjectName);
				Character.Memories[MemoryIndex] = value2;
			}
			if (ObjectUpdated && GUI.Button(new Rect(x + 120f, pos.y, 30f, 30f), "*"))
			{
				BaseObject baseObject = Character.Memories[MemoryIndex].Object;
				ObjectName = ((baseObject != null) ? baseObject.GetDisplayNameString() : "");
				ObjectUpdated = false;
			}
			x += 150f;
			text = GUI.TextField(new Rect(x, pos.y, 120f, 30f), ThirdPartyName);
			if (text != ThirdPartyName)
			{
				ThirdPartyName = text;
				ThirdPartyUpdated = true;
				Memory value3 = Character.Memories[MemoryIndex];
				value3.ThirdParty = FindBaseObjectFromString(ThirdPartyName);
				Character.Memories[MemoryIndex] = value3;
			}
			if (ThirdPartyUpdated && GUI.Button(new Rect(x + 120f, pos.y, 30f, 30f), "*"))
			{
				BaseObject thirdParty = Character.Memories[MemoryIndex].ThirdParty;
				ThirdPartyName = ((thirdParty != null) ? thirdParty.GetDisplayNameString() : "");
				ThirdPartyUpdated = false;
			}
			x += 150f;
			GUI.Label(new Rect(x, pos.y, 80f, 30f), GameImpl.Translate("DEBUG_Approval"));
			x += 80f;
			text = GUI.TextField(new Rect(x, pos.y, 80f, 30f), Character.Memories[MemoryIndex].ApprovalContribution.ToString());
			if (float.TryParse(text, out var result))
			{
				Memory memory = Character.Memories[MemoryIndex];
				memory.ApprovalContribution = result;
				Character.Memories[MemoryIndex] = memory;
				Character.ClearOpinionCacheForMemory(ref memory);
				Session.Instance.AchievementsEnabled = false;
			}
			x += 90f;
			GUI.Label(new Rect(x, pos.y, 80f, 30f), GameImpl.Translate("DEBUG_Respect"));
			x += 80f;
			text = GUI.TextField(new Rect(x, pos.y, 80f, 30f), Character.Memories[MemoryIndex].RespectContribution.ToString());
			if (float.TryParse(text, out result))
			{
				Memory memory2 = Character.Memories[MemoryIndex];
				memory2.RespectContribution = result;
				Character.Memories[MemoryIndex] = memory2;
				Character.ClearOpinionCacheForMemory(ref memory2);
				Session.Instance.AchievementsEnabled = false;
			}
			x += 90f;
			GUI.Label(new Rect(x, pos.y, 80f, 30f), GameImpl.Translate("DEBUG_Morale") + ":");
			x += 80f;
			text = GUI.TextField(new Rect(x, pos.y, 80f, 30f), Character.Memories[MemoryIndex].MoraleContribution.ToString());
			if (float.TryParse(text, out result))
			{
				Memory value4 = Character.Memories[MemoryIndex];
				value4.MoraleContribution = result;
				Character.Memories[MemoryIndex] = value4;
				Character.SetMoraleCacheDirty();
				Session.Instance.AchievementsEnabled = false;
			}
			x += 90f;
			GUI.Label(new Rect(x, pos.y, 80f, 30f), GameImpl.Translate("DEBUG_Quantity"));
			x += 80f;
			text = GUI.TextField(new Rect(x, pos.y, 80f, 30f), Character.Memories[MemoryIndex].QuantityFactor.ToString());
			if (float.TryParse(text, out result))
			{
				Memory value5 = Character.Memories[MemoryIndex];
				value5.QuantityFactor = result;
				Character.Memories[MemoryIndex] = value5;
				Session.Instance.AchievementsEnabled = false;
			}
			x += 90f;
			bool flag = GUIToggle(new Rect(x, pos.y, 70f, 30f), Character.Memories[MemoryIndex].FakeNews, GameImpl.Translate("DEBUG_Fake"));
			if (flag != Character.Memories[MemoryIndex].FakeNews)
			{
				Memory value6 = Character.Memories[MemoryIndex];
				value6.FakeNews = flag;
				Character.Memories[MemoryIndex] = value6;
				Session.Instance.AchievementsEnabled = false;
			}
			x += 70f;
			TimeSpan timeSpan = Session.Instance.PlayTime - Character.Memories[MemoryIndex].Time;
			int num = Mathf.FloorToInt((float)timeSpan.TotalSeconds / Sun.DayLengthSecs);
			GUI.Label(new Rect(x, pos.y, 150f, 30f), GameImpl.Translate((num == 1) ? "HUD_DayAgo" : "HUD_DaysAgo").Replace("%1", num.ToString()) + " " + timeSpan.ToString(TimeFormat));
			x += 150f;
			if (GUIButton(new Rect(x, pos.y, 30f, 30f), "<"))
			{
				Memory value7 = Character.Memories[MemoryIndex];
				value7.Time -= Sun.DayLength;
				Character.Memories[MemoryIndex] = value7;
				Session.Instance.AchievementsEnabled = false;
			}
			x += 30f;
			if (GUIButton(new Rect(x, pos.y, 30f, 30f), ">"))
			{
				Memory value8 = Character.Memories[MemoryIndex];
				value8.Time += Sun.DayLength;
				Character.Memories[MemoryIndex] = value8;
				Session.Instance.AchievementsEnabled = false;
			}
			x += 30f;
			if (GUIButton(new Rect(x, pos.y, 100f, 30f), GameImpl.Translate("DEBUG_Delete")))
			{
				Character.Memories.RemoveAt(MemoryIndex);
				Character.OnMemoriesChanged();
				Character.ClearOpinionCache();
				CharacterMemoryEditor.Refresh = true;
				Session.Instance.AchievementsEnabled = false;
			}
			else
			{
				x += 110f;
				GUI.Label(new Rect(x, pos.y, 150f, 30f), Character.Memories[MemoryIndex].Priority.ToString());
				x += 150f;
			}
		}
	}

	public static BaseObject FindBaseObjectFromString(string str)
	{
		BaseObject baseObject = BaseObjectManager.Instance.GetObjectByUniqueID(str);
		if (baseObject == null)
		{
			baseObject = BaseObjectManager.Instance.FindBaseObjectByID(StringUtil.ParseInt(str));
		}
		return baseObject;
	}
}
