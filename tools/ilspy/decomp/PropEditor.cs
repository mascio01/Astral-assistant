using System;
using System.Text;
using UnityEngine;

public class PropEditor : DebugMenu
{
	private TileObject ObjectToEdit;

	private bool Refresh;

	private bool ProtoChanged;

	public static bool AllowTargetingAllProps;

	public static bool ShowWheelPositions;

	public static bool ShowEntrances;

	public static bool ShowPitTrapJoiners;

	public PropEditor()
		: base(GameImpl.Translate("DEBUG_PropEditor"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		TileObject obj = CharacterEditor.GetCurrentObject();
		if (ObjectToEdit == obj && !Refresh)
		{
			return;
		}
		Refresh = false;
		ObjectToEdit = obj;
		Items.Clear();
		if (ObjectToEdit == null)
		{
			return;
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Id"), ObjectToEdit.Id.ToString()));
		PropPrototype proto = ObjectToEdit.GetPropPrototype();
		if (proto != null)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Prototype"), proto.Name));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Type"), proto.TypeName.ToString()));
		}
		if (ObjectToEdit is MultiTileObject || ObjectToEdit is Town)
		{
			Items.Add(new DebugMenuUniqueID(GameImpl.Translate("DEBUG_UniqueID"), obj));
		}
		if (ObjectToEdit is Marker marker)
		{
			Items.Add(new DebugMenuUniqueID(GameImpl.Translate("DEBUG_UniqueID"), obj));
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Angle"), -180f, 180f, marker.GetAngle, marker.SetAngle, affectsGameState: true));
		}
		Zone zone = ObjectToEdit as Zone;
		if (zone != null)
		{
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Min") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size, zone.GetMinTileX, zone.SetMinTileX, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Max") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size, zone.GetMaxTileX, zone.SetMaxTileX, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Min") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size, zone.GetMinTileY, zone.SetMinTileY, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Max") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size, zone.GetMaxTileY, zone.SetMaxTileY, affectsGameState: true));
			for (int i = 0; i <= zone.Triggers.Count; i++)
			{
				int localIndex = i;
				Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Trigger") + " " + i, () => (localIndex >= zone.Triggers.Count) ? string.Empty : zone.Triggers[localIndex].TriggerName, delegate(string v)
				{
					if (localIndex >= zone.Triggers.Count)
					{
						ZoneTrigger item = new ZoneTrigger
						{
							TriggerName = v,
							Trigger = GameImpl.Instance.FindTriggerByUniqueID(v)
						};
						zone.Triggers.Add(item);
						Refresh = true;
					}
					else
					{
						ZoneTrigger value = zone.Triggers[localIndex];
						value.TriggerName = v;
						value.Trigger = GameImpl.Instance.FindTriggerByUniqueID(v);
						zone.Triggers[localIndex] = value;
					}
					if (string.IsNullOrEmpty(zone.Triggers[localIndex].TriggerName))
					{
						zone.Triggers.RemoveAt(localIndex);
						Refresh = true;
					}
				}, affectsGameState: true));
			}
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Delete"), delegate
			{
				ObjectToEdit.Delete();
			}, affectsGameState: true));
		}
		if (zone == null)
		{
			Items.Add(new DebugMenuAllegianceAdjuster(GameImpl.Translate("DEBUG_Community"), obj));
		}
		Prop prop = ObjectToEdit as Prop;
		if (prop != null)
		{
			Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Angle"), prop.RotateAnticlockwise, prop.RotateClockwise, prop.BuildAngleString, affectsGameState: true));
			Items.Add(new DebugMenuTownAdjuster(GameImpl.Translate("DEBUG_Town"), prop));
		}
		if (Session.Instance.Editor && prop != null && proto != null && proto.WantFlattenTerrain)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_FlattenPropGround"), delegate
			{
				prop.SetTerrainModifiedPatch(TerrainModificationType.Flatten);
			}, affectsGameState: true));
		}
		BridgeWall bridgeWall = ObjectToEdit as BridgeWall;
		if (bridgeWall != null)
		{
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_YOffset"), -1f, 1f, () => bridgeWall.YOffset, delegate(float v)
			{
				bridgeWall.YOffset = v;
				bridgeWall.UpdateWorldTransformAndBounds();
			}));
		}
		if (ObjectToEdit is PlayerStartPoint playerStartPoint)
		{
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Angle"), -180f, 180f, playerStartPoint.GetAngle, playerStartPoint.SetAngle, affectsGameState: true));
		}
		if (prop != null || ObjectToEdit is SingleTileObject)
		{
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Tile") + " " + GameImpl.Translate("DEBUG_X"), 0, GameTerrain.Instance.Size - 1, ObjectToEdit.GetTileX, ObjectToEdit.SetTileX, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Tile") + " " + GameImpl.Translate("DEBUG_Y"), 0, GameTerrain.Instance.Size - 1, ObjectToEdit.GetTileY, ObjectToEdit.SetTileY, affectsGameState: true));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Delete"), delegate
			{
				Session.Instance.DeterministicRand.Locked = false;
				ObjectToEdit.Delete();
				Session.Instance.DeterministicRand.Locked = true;
				Session.Instance.AchievementsEnabled = false;
			}));
			if (prop != null && prop.SupportsVariation() && prop.Prototype != null)
			{
				if (prop.Prototype.Prefabs.Count > 1)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_Variation"), -1, prop.Prototype.Prefabs.Count - 1, () => prop.Variation, delegate(int v)
					{
						prop.SetVariation(v);
					}));
				}
				if (prop.Prototype.GetNumMaterialVariations() > 1)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_MaterialVariation"), -1, prop.Prototype.MaterialVariations.Count - 1, () => prop.MaterialVariation, delegate(int v)
					{
						prop.SetMaterialVariation(v);
					}));
				}
				if (prop.Prototype.GetNumColorVariations() > 1)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ColorVariation"), -1, prop.Prototype.ColorVariations.Length - 1, () => prop.ColorVariation, delegate(int v)
					{
						prop.ColorVariation = v;
						prop.OnUnityModelChanged();
					}));
				}
				if (prop.Prototype.GetNumColorVariations2() > 1)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ColorVariation2"), -1, prop.Prototype.ColorVariations2.Length - 1, () => prop.ColorVariation2, delegate(int v)
					{
						prop.ColorVariation2 = v;
						prop.OnUnityModelChanged();
					}));
				}
				if (prop.Prototype.GetNumColorVariations3() > 1)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ColorVariation3"), -1, prop.Prototype.ColorVariations3.Length - 1, () => prop.ColorVariation3, delegate(int v)
					{
						prop.ColorVariation3 = v;
						prop.OnUnityModelChanged();
					}));
				}
				if (prop.Prototype.GetNumColorVariations4() > 0)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ColorVariation4"), -1, prop.Prototype.ColorVariations4.Length - 1, () => prop.ColorVariation4, delegate(int v)
					{
						prop.ColorVariation4 = v;
						prop.OnUnityModelChanged();
					}));
				}
			}
		}
		if (prop != null)
		{
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Damage"), 0f, 1f, () => prop.GetDamageFraction(), delegate(float v)
			{
				prop.SetDamageFraction(v);
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_MossCleanedAway"), () => prop.MossCleanedAway, delegate(bool v)
			{
				prop.SetMossCleanedAway(v);
			}, affectsGameState: true));
			if (prop.GetLiquidCapacity() > 0f)
			{
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_LiquidAmount"), 0f, prop.GetLiquidCapacity(), () => prop.GetLiquidAmount(), delegate(float v)
				{
					prop.SetLiquidAmount(v);
				}, affectsGameState: true));
			}
		}
		EnterableVehicle vehicle = ObjectToEdit as EnterableVehicle;
		if (vehicle != null)
		{
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_IsDriveable"), () => vehicle.IsDriveable, delegate(bool v)
			{
				vehicle.SetDriveable(v);
			}, affectsGameState: true));
		}
		if (ObjectToEdit is Helicopter obj2)
		{
			Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_DeleteOnTakeOff"), obj2, "DeleteOnTakeOff", affectsGameState: true));
		}
		Campfire campfire = ObjectToEdit as Campfire;
		if (campfire != null)
		{
			Items.Add(new DebugMenuItemEnum<CampfireState>(GameImpl.Translate("DEBUG_CampfireState"), CampfireState.BurnedOut, () => campfire.State, delegate(CampfireState v)
			{
				if (v == CampfireState.Fresh || v == CampfireState.Burning)
				{
					campfire.WoodRemaining = Math.Max(campfire.WoodRemaining, 1f);
				}
				if (v == CampfireState.BurnedOut)
				{
					campfire.WoodRemaining = 0f;
				}
				campfire.SetState(v, null);
				Session.Instance.AchievementsEnabled = false;
			}));
			Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_WoodRemaining"), 0f, 2f, campfire, "WoodRemaining", affectsGameState: true));
		}
		RabbitTrap rabbitTrap = ObjectToEdit as RabbitTrap;
		if (rabbitTrap != null)
		{
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_Open"), () => rabbitTrap.IsOpen, delegate(bool open)
			{
				rabbitTrap.SetOpen(open);
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_FreeResets"), 0, RabbitTrap.MaxFreeResets, () => rabbitTrap.FreeResets, delegate(int v)
			{
				rabbitTrap.FreeResets = v;
			}, affectsGameState: true));
		}
		if (ObjectToEdit is Boulder boulder)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_MiningProgress"), boulder.MiningProgress.ToString()));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_ResourcesRemaining"), boulder.ResourceRemaining.ToString()));
		}
		if (ObjectToEdit is Rock rock)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_MiningProgress"), rock.MiningProgress.ToString()));
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_ResourcesRemaining"), rock.ResourceRemaining.ToString()));
		}
		Gate gate = ObjectToEdit as Gate;
		if (gate != null)
		{
			Items.Add(new DebugMenuItemEnum<GateState>(GameImpl.Translate("DEBUG_GateState"), GateState.Locked, () => gate.GateState, delegate(GateState v)
			{
				if (v == GateState.Open)
				{
					gate.Open(null);
				}
				else if (v == GateState.Closed && gate.GateState == GateState.Open)
				{
					gate.Close(null);
				}
				else if (v == GateState.Closed && gate.GateState == GateState.Locked)
				{
					gate.Open(null);
					gate.Close(null);
				}
				else if (v == GateState.Locked)
				{
					gate.Lock();
				}
				Session.Instance.AchievementsEnabled = false;
			}));
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("HUD_SetBlockAnimals"), () => gate.BlockAnimals, delegate(bool v)
			{
				gate.SetBlockAnimals(v);
			}, affectsGameState: true));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Key"), () => (gate.KeyProto == null) ? string.Empty : gate.KeyProto.Name, delegate(string v)
			{
				gate.KeyProto = GameImpl.Instance.FindEquipmentPrototypeByName(v);
			}, affectsGameState: true));
		}
		if (ObjectToEdit is BaseFence || ObjectToEdit is Gate)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_PropagateFenceCommunity"), delegate
			{
				SetFenceCommunity(obj);
			}, affectsGameState: true));
		}
		if (ObjectToEdit is BaseFence || ObjectToEdit is Prop)
		{
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_Invulnerable"), () => obj.IsForcedInvulnerable(), delegate(bool v)
			{
				SetFenceInvulnerable(obj, v);
			}, affectsGameState: true));
		}
		Building building = ObjectToEdit as Building;
		if (building != null)
		{
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_IgnoreForQuests"), () => building.IgnoreForQuests, delegate(bool v)
			{
				building.IgnoreForQuests = v;
			}, affectsGameState: true));
		}
		Town town = ObjectToEdit as Town;
		if (town != null)
		{
			Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Name"), town.GetDisplayNameString()));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_SetNativeName"), () => town.TownName.CustomString, delegate(string v)
			{
				town.TownName.SetCustomString(v);
			}));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_SetNameTranslationTag"), () => town.TownName.TranslatedStringKey, delegate(string v)
			{
				town.TownName.SetTranslatedString(v);
			}));
			Items.Add(new DebugMenuFloatFieldAdjuster(GameImpl.Translate("DEBUG_Radius"), 0f, GameTerrain.Instance.Size, town, "Radius", affectsGameState: true));
			Items.Add(new DebugMenuItemAdjuster(GameImpl.Translate("DEBUG_Infection"), delegate
			{
				town.Infection = (InfectionType)((int)(town.Infection + 6 - 1) % 6);
			}, delegate
			{
				town.Infection = (InfectionType)((int)(town.Infection + 1) % 6);
			}, delegate(ref StringBuilder value)
			{
				value.Append(town.Infection.ToString());
			}, affectsGameState: true));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_InvaderName"), () => town.InvaderName, delegate(string v)
			{
				town.InvaderName = v;
			}, affectsGameState: true));
			Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_IgnoreForQuests"), () => town.IgnoreForQuests, delegate(bool v)
			{
				town.IgnoreForQuests = v;
			}, affectsGameState: true));
			if (Session.Instance.Editor)
			{
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_GenerateLoot"), delegate
				{
					foreach (Prop allProp in Session.Instance.PropManager.AllProps)
					{
						if (town.IsInRadius(allProp.GetNearestTileTo(town.Tile)) && allProp.GetMaxInventoryWeight() > 0f)
						{
							if (allProp.Town == null && allProp.Community == null && (allProp is Building || allProp is CraftingProp) && !(allProp is EnterableVehicle))
							{
								allProp.SetTown(town);
							}
							if (!allProp.Inventory.GeneratedLoot)
							{
								GameTerrain.GenerateLoot(allProp, MathUtil.NonDeterministicRand);
							}
						}
					}
					Session.Instance.AchievementsEnabled = false;
				}));
			}
		}
		if (prop != null && prop.GetMaxInventoryWeight() > 0f)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_GenerateLoot"), delegate
			{
				prop.Inventory.GeneratedLoot = false;
				prop.GetInventory().DeleteAll(prop, carrierBeingDeleted: false);
				GameTerrain.GenerateLoot(prop, MathUtil.NonDeterministicRand);
				Session.Instance.AchievementsEnabled = false;
			}));
		}
		if (proto != null && proto.ConnectedWirePoints != null && proto.ConnectedWirePoints.Length != 0 && prop != null)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_AutoConnectWires"), delegate
			{
				prop.AutoConnectWires();
			}, affectsGameState: true));
		}
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_AllowSelectingAllProps"), typeof(PropEditor), "AllowTargetingAllProps"));
		Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_ShowBoundingBoxes"), Character.GetDebugDrawBoundingBoxes, Character.SetDebugDrawBoundingBoxes));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowWheelPositions"), typeof(PropEditor), "ShowWheelPositions"));
		Items.Add(new DebugMenuItemToggleField(GameImpl.Translate("DEBUG_ShowDoors"), typeof(PropEditor), "ShowEntrances"));
		if (proto == null)
		{
			return;
		}
		int modelIndex = ((prop != null) ? prop.UnityModelIndexFromPrototype : 0);
		if (modelIndex < proto.ModelOffset.Count)
		{
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ExtentsMin") + " " + GameImpl.Translate("DEBUG_X"), Math.Min(-10, proto.ExtentsMin.x - 5), 0, () => proto.ExtentsMin.x, delegate(int v)
			{
				proto.ExtentsMin.x = v;
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ExtentsMin") + " " + GameImpl.Translate("DEBUG_Y"), Math.Min(-10, proto.ExtentsMin.y - 5), 0, () => proto.ExtentsMin.y, delegate(int v)
			{
				proto.ExtentsMin.y = v;
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ExtentsMax") + " " + GameImpl.Translate("DEBUG_X"), 0, Math.Max(10, proto.ExtentsMin.x + 5), () => proto.ExtentsMax.x, delegate(int v)
			{
				proto.ExtentsMax.x = v;
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ExtentsMax") + " " + GameImpl.Translate("DEBUG_Y"), 0, Math.Max(10, proto.ExtentsMin.y + 5), () => proto.ExtentsMax.y, delegate(int v)
			{
				proto.ExtentsMax.y = v;
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			float a = Mathf.Max(5f, Mathf.CeilToInt(ObjectToEdit.GetBoundingBox().size.magnitude));
			a = Mathf.Max(a, Mathf.Abs(proto.ModelOffset[modelIndex].x));
			a = Mathf.Max(a, Mathf.Abs(proto.ModelOffset[modelIndex].y));
			a = Mathf.Max(a, Mathf.Abs(proto.ModelOffset[modelIndex].z));
			a = Mathf.Max(a, Mathf.Abs(proto.WheelOffset.x));
			a = Mathf.Max(a, Mathf.Abs(proto.WheelOffset.y));
			a = Mathf.Max(a, Mathf.Abs(proto.WheelPos.x));
			a = Mathf.Max(a, Mathf.Abs(proto.WheelPos.y));
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_ModelOffset") + " " + GameImpl.Translate("DEBUG_X"), 0f - a, a, () => proto.ModelOffset[modelIndex].x, delegate(float v)
			{
				proto.ModelOffset[modelIndex] = new Vector3(v, proto.ModelOffset[modelIndex].y, proto.ModelOffset[modelIndex].z);
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_ModelOffset") + " " + GameImpl.Translate("DEBUG_Y"), 0f - a, a, () => proto.ModelOffset[modelIndex].y, delegate(float v)
			{
				proto.ModelOffset[modelIndex] = new Vector3(proto.ModelOffset[modelIndex].x, v, proto.ModelOffset[modelIndex].z);
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_ModelOffset") + " " + GameImpl.Translate("DEBUG_Z"), 0f - a, a, () => proto.ModelOffset[modelIndex].z, delegate(float v)
			{
				proto.ModelOffset[modelIndex] = new Vector3(proto.ModelOffset[modelIndex].x, proto.ModelOffset[modelIndex].y, v);
				OnUnityModelChanged();
				ProtoChanged = true;
				Refresh = true;
				Character.SetDebugDrawBoundingBoxes(on: true);
			}, affectsGameState: true));
			if (ObjectToEdit.IsTiltableProp())
			{
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WheelOffset") + " " + GameImpl.Translate("DEBUG_X"), 0f - a, a, () => proto.WheelOffset.x, delegate(float v)
				{
					proto.WheelOffset = new Vector3(v, proto.WheelOffset.y);
					OnUnityModelChanged();
					ProtoChanged = true;
					Refresh = true;
					ShowWheelPositions = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WheelOffset") + " " + GameImpl.Translate("DEBUG_Y"), 0f - a, a, () => proto.WheelOffset.y, delegate(float v)
				{
					proto.WheelOffset = new Vector3(proto.WheelOffset.x, v);
					OnUnityModelChanged();
					ProtoChanged = true;
					Refresh = true;
					ShowWheelPositions = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WheelPos") + " " + GameImpl.Translate("DEBUG_X"), 0f - a, a, () => proto.WheelPos.x, delegate(float v)
				{
					proto.WheelPos = new Vector3(v, proto.WheelPos.y);
					OnUnityModelChanged();
					ProtoChanged = true;
					Refresh = true;
					ShowWheelPositions = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_WheelPos") + " " + GameImpl.Translate("DEBUG_Y"), 0f - a, a, () => proto.WheelPos.y, delegate(float v)
				{
					proto.WheelPos = new Vector3(proto.WheelPos.x, v);
					OnUnityModelChanged();
					ProtoChanged = true;
					Refresh = true;
					ShowWheelPositions = true;
				}, affectsGameState: true));
			}
			if (ObjectToEdit is EnterableVehicle)
			{
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_CalculateWheelPos"), delegate
				{
					VehicleBehaviour.CalculateWheelPos(prop.UnityObj, ref proto.WheelPos, ref proto.WheelOffset, ref proto.ExtraFrontWheelSeparation, ref proto.VehicleWheelRadius);
					OnUnityModelChanged();
					ProtoChanged = true;
					Refresh = true;
					ShowWheelPositions = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleWheelRadius"), 0f, 1f, () => proto.VehicleWheelRadius, delegate(float v)
				{
					proto.VehicleWheelRadius = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleSuspensionBounce"), 0f, 2f, () => proto.VehicleSuspensionBounce, delegate(float v)
				{
					proto.VehicleSuspensionBounce = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_ExtraFrontWheelSeparation"), 0f, 1f, () => proto.ExtraFrontWheelSeparation, delegate(float v)
				{
					proto.ExtraFrontWheelSeparation = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleHP"), 0f, 300f, () => proto.VehicleHP, delegate(float v)
				{
					proto.VehicleHP = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleMaxRPM"), 0f, 10000f, () => proto.VehicleMaxRPM, delegate(float v)
				{
					proto.VehicleMaxRPM = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleStartEngineRPM"), 0f, 1000f, () => proto.VehicleStartEngineRPM, delegate(float v)
				{
					proto.VehicleStartEngineRPM = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleBrakePower"), 0f, 10000000f, () => proto.VehicleBrakePower, delegate(float v)
				{
					proto.VehicleBrakePower = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_VehicleMaxTurningAngle"), 0f, 90f, () => proto.VehicleMaxTurningAngle, delegate(float v)
				{
					proto.VehicleMaxTurningAngle = v;
					ProtoChanged = true;
					Refresh = true;
				}, affectsGameState: true));
			}
			Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("EDITOR_Mass"), 0f, 10000f, () => proto.Mass, delegate(float v)
			{
				proto.Mass = v;
				ProtoChanged = true;
				Refresh = true;
			}, affectsGameState: true));
			if (building != null)
			{
				Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_NumberOfEntrances"), 0, 10, () => proto.Entrances.Length, delegate(int v)
				{
					EntranceDef[] array = new EntranceDef[v];
					for (int j = 0; j < Math.Min(v, proto.Entrances.Length); j++)
					{
						array[j] = proto.Entrances[j];
					}
					proto.Entrances = array;
					ProtoChanged = true;
					Refresh = true;
					ShowEntrances = true;
					Items.Clear();
				}, affectsGameState: true));
				for (int num = 0; num < proto.Entrances.Length; num++)
				{
					int localIndex2 = num;
					Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Name"), () => proto.Entrances[localIndex2].NativeName, delegate(string v)
					{
						proto.Entrances[localIndex2].NativeName = v;
						ProtoChanged = true;
						Refresh = true;
						ShowEntrances = true;
					}));
					Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Offset"), () => proto.Entrances[localIndex2].EntranceOffset.ToString(), delegate(string v)
					{
						proto.Entrances[localIndex2].EntranceOffset = StringUtil.ParseTerrainCoord(v);
						ProtoChanged = true;
						Refresh = true;
						ShowEntrances = true;
					}, affectsGameState: true));
					Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Angle"), 0f, 315f, () => proto.Entrances[localIndex2].EntranceAngle * 57.29578f, delegate(float v)
					{
						proto.Entrances[localIndex2].EntranceAngle = Mathf.Round(v / 45f) * (MathF.PI / 4f);
						ProtoChanged = true;
						Refresh = true;
						ShowEntrances = true;
					}, affectsGameState: true));
				}
			}
		}
		if (!ProtoChanged)
		{
			return;
		}
		Story story = GameImpl.Instance.GetStoryThatOwnsPropPrototype(proto);
		if (story != null)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_SavePrototype"), delegate
			{
				proto.SaveToFile(story.Path + "/Props/" + proto.Name + ".xml");
				ProtoChanged = false;
				Refresh = true;
			}));
		}
	}

	public override void OnPostRenderImpl()
	{
		base.OnPostRenderImpl();
		if (ObjectToEdit is Town town)
		{
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			DebugGraphics.DrawCircleXZ(town.Pos, town.Radius, Color.red);
			DebugGraphics.EndDrawLines();
		}
	}

	private void SetBackpackVariations()
	{
		foreach (BaseObject baseObject in BaseObjectManager.Instance.BaseObjects)
		{
			if (baseObject is Equipment equipment && equipment.GetClothingType() == ClothingType.Backpack)
			{
				equipment.RandomiseVariations(MathUtil.NonDeterministicRand);
			}
		}
	}

	private void OnUnityModelChanged()
	{
		foreach (BaseObject baseObject in BaseObjectManager.Instance.BaseObjects)
		{
			if (baseObject is TileObject tileObject && tileObject.GetPropPrototype() == ObjectToEdit.GetPropPrototype())
			{
				tileObject.OnUnityModelChanged();
			}
		}
	}

	public void SetFenceCommunity(TileObject obj)
	{
		Community community = null;
		if (obj is Prop)
		{
			community = (obj as Prop).GetCommunity();
		}
		if (obj is BaseFence)
		{
			community = (obj as BaseFence).GetCommunity();
		}
		TerrainCoord minTile = obj.GetMinTile();
		TerrainCoord maxTile = obj.GetMaxTile();
		GameTerrain instance = GameTerrain.Instance;
		for (int i = minTile.x - 1; i <= maxTile.x + 1; i++)
		{
			for (int j = minTile.y - 1; j <= maxTile.y + 1; j++)
			{
				if (!instance.IsTileOutsideBounds(i, j))
				{
					TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(i, j);
					if ((fixedObjectOnTile is BaseFence || fixedObjectOnTile is Gate) && fixedObjectOnTile.GetCommunity() != community)
					{
						fixedObjectOnTile.SetCommunity(community);
						SetFenceCommunity(fixedObjectOnTile);
					}
				}
			}
		}
	}

	public void SetFenceInvulnerable(TileObject obj, bool v)
	{
		obj.SetForceInvulnerable(v);
		TerrainCoord minTile = obj.GetMinTile();
		TerrainCoord maxTile = obj.GetMaxTile();
		GameTerrain instance = GameTerrain.Instance;
		for (int i = minTile.x - 1; i <= maxTile.x + 1; i++)
		{
			for (int j = minTile.y - 1; j <= maxTile.y + 1; j++)
			{
				if (!instance.IsTileOutsideBounds(i, j))
				{
					TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(i, j);
					if ((fixedObjectOnTile is BaseFence || fixedObjectOnTile is Gate || fixedObjectOnTile is Boulder) && fixedObjectOnTile.IsForcedInvulnerable() != v)
					{
						SetFenceInvulnerable(fixedObjectOnTile, v);
					}
				}
			}
		}
	}
}
