using UnityEngine;

public class DebugMenuBaseObjectID : DebugMenuItem
{
	private BaseObject Obj;

	private string BaseObjectID;

	private string ObjName;

	private DebugMenuItemBaseObjectSelected OnSelected;

	public DebugMenuBaseObjectID(string name, BaseObject obj, DebugMenuItemBaseObjectSelected onSelected)
		: base(name)
	{
		Obj = obj;
		BaseObjectID = ((Obj != null) ? Obj.Id.ToString() : string.Empty);
		ObjName = ((Obj != null) ? Obj.GetDisplayNameString() : string.Empty);
		OnSelected = onSelected;
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), Name);
		BaseObjectID = GUI.TextField(new Rect(pos.x + 250f, pos.y, 200f, 30f), BaseObjectID);
		int num = StringUtil.ParseInt(BaseObjectID);
		if (num != ((Obj != null) ? Obj.Id : 0))
		{
			Obj = BaseObjectManager.Instance.FindBaseObjectByID(num);
			ObjName = ((Obj != null) ? Obj.GetDisplayNameString() : string.Empty);
			OnSelected(Obj);
		}
		GUI.Label(new Rect(pos.x + 250f + 220f, pos.y, 200f, 30f), ObjName);
	}
}
