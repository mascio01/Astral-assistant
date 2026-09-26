using System;
using System.Text;
using UnityEngine;

public class Grave : Prop
{
	public GraveState GraveState;

	public Character Corpse;

	public TimeSpan LastVisitedTime;

	public bool DontDeteriorate;

	public int OpenTerrainModifiedId;

	public int GrassTexTerrainModifiedId;

	public static TimeSpan MinTimeBetweenVisits = Sun.DayLength;

	private static PrefabResource[] UnityModels = new PrefabResource[9]
	{
		new PrefabResource("Prefabs/Props/Grave/Grave1"),
		new PrefabResource("Prefabs/Props/Grave/Grave1_1"),
		new PrefabResource("Prefabs/Props/Grave/Grave1_2"),
		new PrefabResource("Prefabs/Props/Grave/Grave2"),
		new PrefabResource("Prefabs/Props/Grave/Grave2_1"),
		new PrefabResource("Prefabs/Props/Grave/Grave2_2"),
		new PrefabResource("Prefabs/Props/Grave/Grave3"),
		new PrefabResource("Prefabs/Props/Grave/Grave3_1"),
		new PrefabResource("Prefabs/Props/Grave/Grave3_2")
	};

	private static int PROP_NamedGrave = StringUtil.JenkinsHash("PROP_NamedGrave");

	private static string Param1 = "%1";

	private bool UnityActive;

	public static float GraveDepth = 0.5f;

	public override Color32 MapColor => GameTerrain.MinimapSettings.PropCol;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Grave;
	}

	public override PrefabResource GetUnityModel()
	{
		switch (GraveState)
		{
		case GraveState.Open:
			return null;
		case GraveState.Buried:
			if (Prototype == null || UnityModelIndexFromPrototype >= Prototype.Prefabs.Count)
			{
				return base.GetUnityModel();
			}
			return Prototype.Prefabs[UnityModelIndexFromPrototype];
		case GraveState.Buried_Deteriorated1:
			if (Prototype == null || Prototype.Stage1Prefabs == null || UnityModelIndexFromPrototype >= Prototype.Stage1Prefabs.Count)
			{
				return base.GetUnityModel();
			}
			return Prototype.Stage1Prefabs[UnityModelIndexFromPrototype];
		case GraveState.Buried_Deteriorated2:
			if (Prototype == null || Prototype.Stage2Prefabs == null || UnityModelIndexFromPrototype >= Prototype.Stage2Prefabs.Count)
			{
				return base.GetUnityModel();
			}
			return Prototype.Stage2Prefabs[UnityModelIndexFromPrototype];
		default:
			return null;
		}
	}

	public override float GetMaxInventoryWeight()
	{
		if (GraveState == GraveState.Open)
		{
			return 0f;
		}
		return base.GetMaxInventoryWeight();
	}

	public override bool CanSetPropName()
	{
		return false;
	}

	public override bool CanSetStoragePolicy()
	{
		return false;
	}

	public override bool IsImpassableProp()
	{
		if (GraveState == GraveState.Open)
		{
			return false;
		}
		return base.IsImpassableProp();
	}

	public override Texture2D GetIconResource()
	{
		return Prop.GraveIcon;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (GraveState != GraveState.Open && Corpse != null && Corpse.NameKnown)
		{
			string text = GameImpl.Translate(PROP_NamedGrave);
			int num = text.IndexOf(Param1);
			if (num != -1)
			{
				for (int i = 0; i < num; i++)
				{
					sb.Append(text[i]);
				}
				Corpse.BuildDisplayName(sb, noStrangers, englishOnly);
				for (int j = num + Param1.Length; j < text.Length; j++)
				{
					sb.Append(text[j]);
				}
				StringUtil.ApplyFormulae(sb, null, Corpse, englishOnly);
			}
			else
			{
				Corpse.BuildDisplayName(sb, noStrangers, englishOnly);
				sb.Append(text);
			}
		}
		else
		{
			base.BuildDisplayName(sb, noStrangers, englishOnly);
		}
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		if (language == Language.BrazilianPortuguese && GraveState != GraveState.Open && Corpse != null && Corpse.NameKnown)
		{
			return GenderType.Male;
		}
		return base.GetGender(language);
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref GraveState);
		if (reflector.Version < 393 && UnderConstructionInfo != null && GraveState != GraveState.Open)
		{
			SetUnderConstructionInfo(null);
		}
		reflector.AddAfter(ref Corpse, 27);
		if (reflector.Version < 284)
		{
			LastVisitedTime = Session.Instance.PlayTime;
		}
		else
		{
			reflector.Add(ref LastVisitedTime);
		}
		reflector.AddAfter(ref DontDeteriorate, 341);
		reflector.AddAfter(ref OpenTerrainModifiedId, 439);
		reflector.AddAfter(ref GrassTexTerrainModifiedId, 439);
	}

	public override void OnSpawn()
	{
		GraveState = GraveState.Open;
		base.OnSpawn();
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		UnityActive = true;
	}

	public override void UnityDeactivate()
	{
		UnityActive = false;
		base.UnityDeactivate();
	}

	public override void SetUnderConstructionInfo(UnderConstructionInfo underConstructionInfo)
	{
		base.SetUnderConstructionInfo(underConstructionInfo);
		if (underConstructionInfo == null)
		{
			GameTerrain instance = GameTerrain.Instance;
			OpenTerrainModifiedId = instance.AddModifiedPatch(new ModifiedPatch
			{
				Type = TerrainModificationType.Flatten,
				MinTile = MinTile,
				MaxTile = MaxTile,
				FlattenHeight = Pos.y - GraveDepth,
				FlattenBorder = 0f
			});
		}
	}

	public void Bury(Character body)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (IsImpassableProp())
		{
			instance.AStar.AddChange(AStarChange.RemovedFixedContents(this));
		}
		bool unityActive = UnityActive;
		if (unityActive)
		{
			UnityDeactivate();
		}
		if (true)
		{
			UnityDelete();
		}
		GraveState = GraveState.Buried;
		Corpse = body;
		Corpse.BuriedInGrave = this;
		LastVisitedTime = Session.Instance.PlayTime;
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		if (true)
		{
			UnityInit();
		}
		if (unityActive)
		{
			UnityActivate();
		}
		if (IsImpassableProp())
		{
			instance.AStar.AddChange(AStarChange.AddedFixedContents(this));
		}
		if (OpenTerrainModifiedId != 0)
		{
			GameTerrain.Instance.RemoveModifiedPatch(OpenTerrainModifiedId);
			OpenTerrainModifiedId = 0;
		}
		while (body.Inventory.Count > 0)
		{
			Equipment item = body.Inventory.GetItem(0);
			item = body.Inventory.Take(body, item, item.GetAmount());
			Inventory.Add(this, item);
		}
		body.Disappear(fromGoal: false);
		body.SetPosition(Pos);
		SoundManager.PlaySound3DFromList(SoundManager.DigSounds, Pos);
	}

	public override bool IsTargetable()
	{
		if (GraveState == GraveState.Open)
		{
			Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
			if (localControlledCharacter != null && localControlledCharacter.CarryingObject != null && localControlledCharacter.CarryingObject is Character && !((Character)localControlledCharacter.CarryingObject).Alive)
			{
				return true;
			}
		}
		return base.IsTargetable();
	}

	public override void Init()
	{
		base.Init();
		if (GraveState != GraveState.Open)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		}
		if (Corpse != null)
		{
			Corpse.BuriedInGrave = this;
		}
	}

	public override void Delete()
	{
		if (GrassTexTerrainModifiedId != 0)
		{
			GameTerrain.Instance.RemoveModifiedPatch(GrassTexTerrainModifiedId);
			GrassTexTerrainModifiedId = 0;
		}
		if (OpenTerrainModifiedId != 0)
		{
			GameTerrain.Instance.RemoveModifiedPatch(OpenTerrainModifiedId);
			OpenTerrainModifiedId = 0;
		}
		if (Corpse != null)
		{
			Corpse.BuriedInGrave = null;
		}
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		base.Delete();
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		if (GraveState != GraveState.Open && Inventory.CanBeDestroyed())
		{
			UpdateGraveStateIfNeeded();
			stillNeedUpdating = true;
		}
		base.PropUpdateRare(ref stillNeedUpdating);
	}

	private void UpdateGraveStateIfNeeded()
	{
		float num = (float)(Session.Instance.PlayTime - LastVisitedTime).TotalSeconds;
		GraveState graveState = ((!DontDeteriorate) ? ((num >= Sun.DayLengthSecs * 21f) ? GraveState.Deleted : ((num >= Sun.DayLengthSecs * 14f) ? GraveState.Buried_Deteriorated2 : ((num >= Sun.DayLengthSecs * 7f) ? GraveState.Buried_Deteriorated1 : GraveState.Buried))) : GraveState.Buried);
		if (graveState != GraveState)
		{
			bool unityActive = UnityActive;
			if (unityActive)
			{
				UnityDeactivate();
			}
			if (true)
			{
				UnityDelete();
			}
			if (GraveState == GraveState.Buried && graveState == GraveState.Buried_Deteriorated1)
			{
				int flattenPatchExtra = GameTerrain.FlattenPatchExtra;
				ModifiedPatch patch = new ModifiedPatch
				{
					Type = TerrainModificationType.ApplyTexture,
					MinTile = MinTile - new TerrainCoord(flattenPatchExtra, flattenPatchExtra),
					MaxTile = MaxTile + new TerrainCoord(flattenPatchExtra, flattenPatchExtra),
					FlattenBorder = 0f,
					Tex = TerrainTex.Grass
				};
				GrassTexTerrainModifiedId = GameTerrain.Instance.AddModifiedPatch(patch);
			}
			GraveState = graveState;
			if (true)
			{
				UnityInit();
			}
			if (unityActive)
			{
				UnityActivate();
			}
		}
	}

	public override bool PropWantDelete()
	{
		if (GraveState != GraveState.Deleted)
		{
			return base.PropWantDelete();
		}
		return true;
	}

	public void VisitGrave()
	{
		LastVisitedTime = Session.Instance.PlayTime;
		UpdateGraveStateIfNeeded();
	}
}
