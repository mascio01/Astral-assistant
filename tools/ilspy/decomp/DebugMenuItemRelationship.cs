using UnityEngine;

public class DebugMenuItemRelationship : DebugMenuItem
{
	private Character Character;

	private int RelationshipIndex;

	private const string Left = "<";

	private const string Right = ">";

	public DebugMenuItemRelationship(Character character, int relationshipIndex)
		: base(null)
	{
		Character = character;
		RelationshipIndex = relationshipIndex;
	}

	public override void Update(Vector2 pos)
	{
		if (RelationshipIndex >= Character.Relationships.Count)
		{
			return;
		}
		Character relationshipTarget = Character.Relationships[RelationshipIndex].RelationshipTarget;
		float x = pos.x;
		if (GUIButton(new Rect(x, pos.y, 30f, 30f), "<"))
		{
			SetRelationshipType(-1);
		}
		x += 30f;
		GUI.Label(new Rect(x, pos.y, 200f, 30f), Character.Relationships[RelationshipIndex].RelationshipType.ToString());
		x += 200f;
		if (GUIButton(new Rect(x, pos.y, 30f, 30f), ">"))
		{
			SetRelationshipType(1);
		}
		x += 30f;
		if (GUIButton(new Rect(x, pos.y, 30f, 30f), "<"))
		{
			SetRelationshipTarget(-1);
		}
		x += 30f;
		GUI.Label(new Rect(x, pos.y, 200f, 30f), (relationshipTarget != null) ? relationshipTarget.GetDisplayNameString() : "");
		x += 200f;
		if (GUIButton(new Rect(x, pos.y, 30f, 30f), ">"))
		{
			SetRelationshipTarget(1);
		}
		x += 30f;
		GUI.Label(new Rect(x, pos.y, 100f, 30f), GameImpl.Translate("DEBUG_Approval"));
		x += 100f;
		if (float.TryParse(GUI.TextField(new Rect(x, pos.y, 100f, 30f), Character.Relationships[RelationshipIndex].ApprovalContribution.ToString()), out var result))
		{
			Relationship value = Character.Relationships[RelationshipIndex];
			value.ApprovalContribution = result;
			Character.Relationships[RelationshipIndex] = value;
			Session.Instance.AchievementsEnabled = false;
		}
		x += 100f;
		GUI.Label(new Rect(x, pos.y, 100f, 30f), GameImpl.Translate("DEBUG_Respect"));
		x += 100f;
		if (float.TryParse(GUI.TextField(new Rect(x, pos.y, 100f, 30f), Character.Relationships[RelationshipIndex].RespectContribution.ToString()), out result))
		{
			Relationship value2 = Character.Relationships[RelationshipIndex];
			value2.RespectContribution = result;
			Character.Relationships[RelationshipIndex] = value2;
			Session.Instance.AchievementsEnabled = false;
		}
		x += 100f;
		bool flag = GUIToggle(new Rect(x, pos.y, 200f, 30f), Character.Relationships[RelationshipIndex].KnownToPlayer, GameImpl.Translate("DEBUG_KnownToPlayer"));
		if (flag != Character.Relationships[RelationshipIndex].KnownToPlayer)
		{
			Relationship value3 = Character.Relationships[RelationshipIndex];
			value3.KnownToPlayer = flag;
			Character.Relationships[RelationshipIndex] = value3;
			Session.Instance.AchievementsEnabled = false;
		}
		x += 200f;
		if (GUIButton(new Rect(x, pos.y, 100f, 30f), GameImpl.Translate("DEBUG_Delete")))
		{
			Character relationshipTarget2 = Character.Relationships[RelationshipIndex].RelationshipTarget;
			if (relationshipTarget2 != null)
			{
				for (int i = 0; i < relationshipTarget2.Relationships.Count; i++)
				{
					if (relationshipTarget2.Relationships[i].RelationshipTarget == Character)
					{
						relationshipTarget2.Relationships.RemoveAt(i);
						break;
					}
				}
			}
			Character.Relationships.RemoveAt(RelationshipIndex);
			CharacterRelationshipEditor.WantRefresh = true;
			Session.Instance.AchievementsEnabled = false;
		}
		x += 100f;
	}

	private void SetRelationshipType(int dir)
	{
		Session.Instance.AchievementsEnabled = false;
		RelationshipType relationshipType = (RelationshipType)((int)(Character.Relationships[RelationshipIndex].RelationshipType + dir + 10) % 10);
		Relationship value = Character.Relationships[RelationshipIndex];
		value.RelationshipType = relationshipType;
		Character.Relationships[RelationshipIndex] = value;
		Character relationshipTarget = Character.Relationships[RelationshipIndex].RelationshipTarget;
		if (relationshipTarget != null)
		{
			int num = Relationship.FindRelationshipIndex(relationshipTarget, Character);
			if (num == -1)
			{
				num = relationshipTarget.Relationships.Count;
				Relationship item = new Relationship
				{
					RelationshipTarget = Character
				};
				relationshipTarget.Relationships.Add(item);
			}
			Relationship value2 = relationshipTarget.Relationships[num];
			if (relationshipType != RelationshipType.InLoveWith || value2.RelationshipType != RelationshipType.InLoveWith)
			{
				value2.RelationshipType = Relationship.GetOppositeRelationshipType(relationshipType);
				relationshipTarget.Relationships[num] = value2;
			}
		}
	}

	private void SetRelationshipTarget(int dir)
	{
		Session.Instance.AchievementsEnabled = false;
		CharacterManager characterManager = Session.Instance.CharacterManager;
		Character relationshipTarget = Character.Relationships[RelationshipIndex].RelationshipTarget;
		int num = characterManager.Characters.IndexOf(relationshipTarget);
		if (relationshipTarget != null)
		{
			for (int i = 0; i < relationshipTarget.Relationships.Count; i++)
			{
				if (relationshipTarget.Relationships[i].RelationshipTarget == Character)
				{
					relationshipTarget.Relationships.RemoveAt(i);
					break;
				}
			}
		}
		Character character = null;
		for (int j = 0; j < characterManager.Characters.Count; j++)
		{
			num = (num + characterManager.Characters.Count + dir) % characterManager.Characters.Count;
			Character character2 = characterManager.Characters[num];
			if (character2 != Character && !string.IsNullOrEmpty(character2.FirstName) && !Relationship.HasAnyRelationship(Character, character2))
			{
				character = character2;
				break;
			}
		}
		Relationship value = Character.Relationships[RelationshipIndex];
		value.RelationshipTarget = character;
		Character.Relationships[RelationshipIndex] = value;
		if (character != null)
		{
			Relationship item = new Relationship
			{
				RelationshipTarget = Character,
				RelationshipType = Relationship.GetOppositeRelationshipType(value.RelationshipType)
			};
			character.Relationships.Add(item);
		}
	}
}
