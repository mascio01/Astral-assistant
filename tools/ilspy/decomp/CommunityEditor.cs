using System.Text;
using UnityEngine;

internal class CommunityEditor : DebugMenu
{
	private Community _community;

	private bool WantRepopulate;

	private int CommunityRelationshipsCount;

	public static string GetCommunityNameAsString(Community community)
	{
		StringBuilder stringBuilder = new StringBuilder(50);
		community.BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		return stringBuilder.ToString();
	}

	public CommunityEditor(Community community)
		: base(GetCommunityNameAsString(community))
	{
		_community = community;
		WantRepopulate = true;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (CommunityRelationshipsCount != _community.CommunityRelationships.Count)
		{
			WantRepopulate = true;
		}
		if (WantRepopulate)
		{
			WantRepopulate = false;
			Populate();
		}
		base.HandleInputImpl(inputFrame);
	}

	private void Populate()
	{
		CommunityRelationshipsCount = _community.CommunityRelationships.Count;
		_community.GetLivingNonZombieMemberCount();
		Items.Clear();
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Id"), _community.Id.ToString()));
		Items.Add(new DebugMenuUniqueID(GameImpl.Translate("DEBUG_UniqueID"), _community));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Name"), GetCommunityNameAsString(_community)));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_SetNativeName"), () => _community.CommunityName.CustomString, delegate(string v)
		{
			_community.CommunityName.SetCustomString(v);
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_SetNameTranslationTag"), () => _community.CommunityName.TranslatedStringKey, delegate(string v)
		{
			_community.CommunityName.SetTranslatedString(v);
		}));
		Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Type"), delegate
		{
			_community.CommunityType = (CommunityType)((int)(_community.CommunityType + 15 - 1) % 15);
			WantRepopulate = true;
		}, delegate
		{
			_community.CommunityType = (CommunityType)((int)(_community.CommunityType + 1) % 15);
			WantRepopulate = true;
		}, delegate(ref StringBuilder value)
		{
			value.Append(Community.CommunityTypeNames[(int)_community.CommunityType]);
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_NameKnown"), _community, "CommunityNameKnown", affectsGameState: true));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_DiscoverNameIfAtWar"), _community, "DiscoverNameIfAtWar", affectsGameState: true));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_StatusShown"), _community, "CommunityStatusShown", affectsGameState: true));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Delete"), Delete, affectsGameState: true));
		if (_community.IsAISettlement())
		{
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_BaseMin") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size, () => _community.BaseRect.min.x, delegate(int v)
			{
				_community.BaseRect.min.x = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_BaseMin") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size, () => _community.BaseRect.min.y, delegate(int v)
			{
				_community.BaseRect.min.y = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_BaseMax") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size, () => _community.BaseRect.max.x, delegate(int v)
			{
				_community.BaseRect.max.x = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_BaseMax") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size, () => _community.BaseRect.max.y, delegate(int v)
			{
				_community.BaseRect.max.y = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_PrisonMin") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size, () => _community.PrisonRect.min.x, delegate(int v)
			{
				_community.PrisonRect.min.x = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_PrisonMin") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size, () => _community.PrisonRect.min.y, delegate(int v)
			{
				_community.PrisonRect.min.y = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_PrisonMax") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size, () => _community.PrisonRect.max.x, delegate(int v)
			{
				_community.PrisonRect.max.x = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_PrisonMax") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size, () => _community.PrisonRect.max.y, delegate(int v)
			{
				_community.PrisonRect.max.y = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_PerimeterEditor"), delegate
			{
				SetState(DebugPageState.ChildActive, new CommunityPerimeterEditor(_community));
			}));
			if (Session.Instance.Editor)
			{
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_AssignRoles"), delegate
				{
					_community.UpdateRoles(null);
				}, affectsGameState: true));
			}
		}
		float memberCountWhenConsideringFoodNeeds = _community.GetMemberCountWhenConsideringFoodNeeds();
		if (memberCountWhenConsideringFoodNeeds > 0f)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("HUD_Nutrition"), _community.CalcCommunityNutritionLevel().ToString()));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_NutritionPerMember"), (_community.GetHarvestedNutritionAmount() / Sun.DayLengthSecs / memberCountWhenConsideringFoodNeeds).ToString()));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_ReservedNutritionPerMember"), (_community.GetReservedNutritionAmount() / Sun.DayLengthSecs / memberCountWhenConsideringFoodNeeds).ToString()));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_PlantedNutritionPerMember"), _community.GetPlantedNutritionAmount() / Sun.DayLengthSecs / memberCountWhenConsideringFoodNeeds + " / " + Community.RecommendedPlantedNutritionDays));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_CropPatchNutritionPerMember"), _community.GetCropPatchNutritionAmount() / Sun.DayLengthSecs / memberCountWhenConsideringFoodNeeds + " / " + Community.RecommendedPlantedNutritionDays));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_NeededForWinter"), (Weather.CalcNutritionNeededToStoreForWinterPerPerson(Session.Instance.DayOfYear, rampUpOverPlantingSeason: true) / Sun.DayLengthSecs).ToString()));
		}
		Items.Add(new DebugMenuLootLocationAdjuster(GameImpl.Translate("EDITOR_LootLocation"), _community));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_GenerateLoot"), GenerateCommunityLoot, affectsGameState: true));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_GenerateRelationships"), GenerateCommunityRelationships, affectsGameState: true));
		for (int num = 0; num < _community.CommunityRelationships.Count; num++)
		{
			Items.Add(new DebugMenuRelationshipAdjuster(GameImpl.Translate("DEBUG_RelationshipWith"), _community, num));
		}
		Items.Add(new DebugMenuRelationshipAdjuster(GameImpl.Translate("DEBUG_AddRelationship"), _community, _community.CommunityRelationships.Count));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_AreaOwnershipEditor"), EditAreaOwnership));
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_CropPatchEditor"), EditCropPatches));
		if (_community.InvasionTarget != null)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_InvasionTarget"), _community.InvasionTarget.GetDisplayNameString()));
		}
		if (_community.CommunityType == CommunityType.Looter)
		{
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ExtortionPresenceThreshold"), 0, 32, () => _community.ExtortionPresenceThreshold, delegate(int v)
			{
				_community.ExtortionPresenceThreshold = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ExtortAISettlements"), () => _community.ExtortAISettlements, delegate(bool v)
			{
				_community.ExtortAISettlements = v;
			}, affectsGameState: true));
		}
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("EDITOR_IsFEMA"), () => _community.IsFEMA, delegate(bool v)
		{
			_community.IsFEMA = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("EDITOR_HiddenCommunity"), () => _community.HiddenCommunity, delegate(bool v)
		{
			_community.HiddenCommunity = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_BuildingsCantBeCaptured"), () => _community.BuildingsCantBeCaptured, delegate(bool v)
		{
			_community.BuildingsCantBeCaptured = v;
		}, affectsGameState: true));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("EDITOR_PlayerSurrenderDisabled"), _community, "PlayerSurrenderDisabled", affectsGameState: true));
		if (_community.IsAISettlement())
		{
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Key"), () => (_community.KeyProtoForAllNewSpawns == null) ? string.Empty : _community.KeyProtoForAllNewSpawns.Name, delegate(string v)
			{
				_community.KeyProtoForAllNewSpawns = GameImpl.Instance.FindEquipmentPrototypeByName(v);
			}, affectsGameState: true));
		}
		string text = "";
		foreach (Character member in _community.Members)
		{
			text = text + member.GetDisplayNameString() + ", ";
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Members"), text));
		string text2 = "";
		foreach (Community community in Session.Instance.CommunityManager.Communities)
		{
			if (!community.IsZombieCommunity() && !community.IsAnimalCommunity() && !community.IsAlwaysHostileCommunity() && Session.Instance.CommunityManager.GetRelationship(_community, community) == CommunityRelationshipType.Hostile)
			{
				text2 = text2 + community.GetDisplayNameString() + ", ";
			}
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Enemies"), text2));
		foreach (Squad squad in _community.Squads)
		{
			string text3 = "";
			text3 = text3 + Community.SquadBehaviourNames[(int)squad.Behaviour] + ": ";
			text3 = text3 + Community.SquadActionNames[(int)squad.Action] + ": ";
			if (squad.EnemyCommunityId != 0)
			{
				BaseObject baseObject = BaseObjectManager.Instance.FindBaseObjectByID(squad.EnemyCommunityId);
				if (baseObject != null)
				{
					text3 = text3 + "<" + baseObject.GetDisplayNameString() + "> ";
				}
			}
			if (squad.GoalTile != TerrainCoord.Zero && squad.GoalTile != TerrainCoord.Invalid)
			{
				string text4 = text3;
				TerrainCoord goalTile = squad.GoalTile;
				text3 = text4 + "<" + goalTile.ToString() + "> ";
			}
			foreach (Character member2 in squad.Members)
			{
				text3 = text3 + member2.GetDisplayNameString() + ", ";
			}
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Squad"), text3));
		}
		foreach (Threat threat in _community.Threats)
		{
			string text5 = "";
			foreach (Character threatMember in threat.ThreatMembers)
			{
				text5 = text5 + threatMember.GetDisplayNameString() + ", ";
			}
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Threat"), text5));
		}
	}

	public override void OnPostRenderImpl()
	{
		base.OnPostRenderImpl();
		CommunityPerimeterEditor.DrawPerimeter(_community, -1);
		for (int i = 0; i < _community.ConstructionRecords.Count; i++)
		{
			_community.ConstructionRecords[i].Proto.CalcMinMaxTile(_community.ConstructionRecords[i].Tile, _community.ConstructionRecords[i].Orientation, out var minTile, out var maxTile);
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawRectOnGround(new TerrainRect(minTile, maxTile), Color.cyan);
			DebugGraphics.EndDrawLines();
		}
	}

	public void Delete()
	{
		_community.Delete();
		SetState(DebugPageState.Inactive, null);
	}

	public void EditAreaOwnership()
	{
		SetState(DebugPageState.ChildActive, new CommunityAreaOwnershipEditor(_community));
	}

	public void EditCropPatches()
	{
		SetState(DebugPageState.ChildActive, new CommunityCropPatchEditor(_community));
	}

	public void GenerateCommunityLoot()
	{
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		foreach (Character member in _community.Members)
		{
			if (member.GetBaseObjectType() == BaseObjectType.Human)
			{
				GameTerrain.GenerateCharacterEquipment(member, _community.IsLooterCommunity() ? PersonalityGroup.LooterFaction : PersonalityGroup.NormalFaction, nonDeterministicRand);
				if (!member.Inventory.GeneratedLoot)
				{
					GameTerrain.GenerateLoot(member, nonDeterministicRand);
				}
			}
		}
		foreach (Prop building in _community.Buildings)
		{
			if (!building.Inventory.GeneratedLoot)
			{
				GameTerrain.GenerateLoot(building, nonDeterministicRand);
			}
		}
	}

	public void GenerateCommunityRelationships()
	{
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		_community.RandomizeRelationships(nonDeterministicRand);
	}
}
