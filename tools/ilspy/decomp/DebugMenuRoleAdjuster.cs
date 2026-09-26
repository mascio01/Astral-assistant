using System.Text;
using UnityEngine;

public class DebugMenuRoleAdjuster : DebugMenuItemAdjuster
{
	private Character Character;

	private int RoleIndex;

	private static int DEBUG_Delete = StringUtil.JenkinsHash("DEBUG_Delete");

	public DebugMenuRoleAdjuster(string name, Character character, int roleIndex)
		: base(name)
	{
		Character = character;
		RoleIndex = roleIndex;
		DecrementEvent = OnDecrement;
		IncrementEvent = OnIncrement;
		BuildValueStringEvent = BuildRoleDisplayString;
		BuildDisplayString();
	}

	public void OnDecrement()
	{
		if (Character != null && RoleIndex < Character.Roles.Count)
		{
			RoleInfo value = Character.Roles[RoleIndex];
			value.Role = (Role)((int)(value.Role + 18 - 1) % 18);
			Character.Roles[RoleIndex] = value;
			BuildDisplayString();
		}
	}

	public void OnIncrement()
	{
		if (Character != null && RoleIndex < Character.Roles.Count)
		{
			RoleInfo value = Character.Roles[RoleIndex];
			value.Role = (Role)((int)(value.Role + 18 + 1) % 18);
			Character.Roles[RoleIndex] = value;
			BuildDisplayString();
		}
	}

	public void BuildRoleDisplayString(ref StringBuilder value)
	{
		value.Length = 0;
		value.Append(Name);
		value.Append(" < ");
		if (Character != null && RoleIndex < Character.Roles.Count)
		{
			value.Append(Character.Roles[RoleIndex].Role.ToString());
		}
		value.Append(" >");
	}

	public override void Update(Vector2 pos)
	{
		base.Update(pos);
		if (GUIButton(new Rect(pos.x + 250f + 70f, pos.y, 200f, 30f), GameImpl.Translate(DEBUG_Delete)) && Character != null && RoleIndex < Character.Roles.Count)
		{
			Character.CancelRole(Character.Roles[RoleIndex]);
		}
	}
}
