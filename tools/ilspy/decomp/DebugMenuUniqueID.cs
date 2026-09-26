using UnityEngine;

public class DebugMenuUniqueID : DebugMenuItem
{
	private BaseObject Obj;

	private string UniqueID;

	public DebugMenuUniqueID(string name, BaseObject obj)
		: base(name)
	{
		Obj = obj;
		UniqueID = ((Obj != null) ? Obj.GetUniqueID() : string.Empty);
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), Name);
		UniqueID = GUI.TextField(new Rect(pos.x + 250f, pos.y, 200f, 30f), UniqueID);
		if (UniqueID != Obj.GetUniqueID() && BaseObjectManager.Instance.GetObjectByUniqueID(UniqueID) == null)
		{
			Obj.SetUniqueID(UniqueID);
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public override void OnDeactivate()
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObject objectByUniqueID = BaseObjectManager.Instance.GetObjectByUniqueID(UniqueID);
			if (objectByUniqueID != null && objectByUniqueID != Obj)
			{
				GameImpl.Instance.ShowMessageBox("There is already an object with id " + UniqueID + " (" + objectByUniqueID.GetDisplayNameString() + ")");
			}
		}
	}
}
