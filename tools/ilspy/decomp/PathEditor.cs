using System;
using System.Collections.Generic;
using UnityEngine;

public class PathEditor : DebugMenu
{
	public static PathEditor Current;

	public TerrainPath Path;

	public TerrainPath HoveredPath;

	public int SelectedNodeIndex;

	public int HoveredNodeIndex;

	public bool WantUpdatePath;

	public bool WantRecalcRiverTiles;

	public bool WantRefresh;

	public bool Dragging;

	public bool AngleDragging;

	public Vector2 PrevDragPosXZ;

	public float PrevDragAngle;

	public PathEditor()
		: base(GameImpl.Translate("DEBUG_PathEditor"))
	{
	}

	public override void ActivateImpl()
	{
		base.ActivateImpl();
		Current = this;
		WantRefresh = true;
		SelectedNodeIndex = 0;
		HoveredNodeIndex = 0;
	}

	public override void DeactivateImpl()
	{
		if (WantRecalcRiverTiles)
		{
			RecalcRiverTiles();
			WantRecalcRiverTiles = false;
		}
		Current = null;
		HoveredPath = null;
		Path = null;
		SelectedNodeIndex = 0;
		HoveredNodeIndex = 0;
		Dragging = false;
		AngleDragging = false;
		base.DeactivateImpl();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		GameTerrain terrain = GameTerrain.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance2.GetCursorPos(), 0);
		HoveredPath = null;
		foreach (TerrainPath path2 in terrain.Paths)
		{
			for (int i = 0; i < path2.ControlPoints.Count; i++)
			{
				Vector2 pos = path2.ControlPoints[i].Pos;
				float z = terrain.GetTileHeightAtPos(pos, ignoreIce: true, ignoreRoadCamber: true) + (path2.IsRiver ? path2.ControlPoints[i].RiverDepth : 0.2f);
				if (MathUtil.CreateBoundsCentreExtents(MathUtil.ToXZY(pos, z), Vector3.one * TerrainPath.ControlPointBoxSize).IntersectRay(raycastResult.Ray))
				{
					HoveredPath = path2;
					HoveredNodeIndex = i;
					WantRefresh = true;
				}
			}
		}
		if (Dragging)
		{
			if (instance2.IsPressed(InputFunction.MainAction))
			{
				Vector2 vector = MathUtil.ToXZ(raycastResult.GetHitPosition());
				ControlPoint value = Path.ControlPoints[SelectedNodeIndex];
				value.Pos += vector - PrevDragPosXZ;
				PrevDragPosXZ = vector;
				Path.ControlPoints[SelectedNodeIndex] = value;
				WantUpdatePath = true;
				Session.Instance.AchievementsEnabled = false;
			}
			else
			{
				Dragging = false;
			}
		}
		else if (instance2.IsJustPressed(InputFunction.MainAction, capture: false))
		{
			if (HoveredPath != null)
			{
				instance2.Capture(InputFunction.MainAction, untilReleased: false);
				Path = HoveredPath;
				SelectedNodeIndex = HoveredNodeIndex;
				Dragging = true;
				PrevDragPosXZ = MathUtil.ToXZ(raycastResult.GetHitPosition());
			}
			else if (Path != null && raycastResult.HitObject == terrain)
			{
				instance2.Capture(InputFunction.MainAction, untilReleased: true);
				ControlPoint item = default(ControlPoint);
				if (Path.ControlPoints.Count > 0)
				{
					item = Path.ControlPoints[SelectedNodeIndex];
					if (SelectedNodeIndex < Path.ControlPoints.Count - 1)
					{
						item.Width = Mathf.Lerp(item.Width, Path.ControlPoints[SelectedNodeIndex].Width, 0.5f);
						item.RiverDepth = Mathf.Lerp(item.RiverDepth, Path.ControlPoints[SelectedNodeIndex].RiverDepth, 0.5f);
					}
				}
				else
				{
					item.Dir = Vector2.left;
					item.RiverDepth = 0f;
					item.Width = Path.DefaultWidth;
				}
				item.Pos = MathUtil.ToXZ(raycastResult.GetHitPosition());
				if (SelectedNodeIndex < Path.ControlPoints.Count - 1)
				{
					SelectedNodeIndex++;
					Path.ControlPoints.Insert(SelectedNodeIndex, item);
				}
				else
				{
					SelectedNodeIndex = Path.ControlPoints.Count;
					Path.ControlPoints.Add(item);
				}
				WantUpdatePath = true;
			}
		}
		if (Path != null && Path.ControlPoints != null && Path.ControlPoints.Count > 0)
		{
			if (instance2.IsPressed(InputFunction.Clear))
			{
				if (raycastResult.HitObject == terrain)
				{
					float angleFromDir = MathUtil.GetAngleFromDir(MathUtil.SafeNormalize(MathUtil.ToXZ(raycastResult.GetHitPosition()) - Path.ControlPoints[SelectedNodeIndex].Pos, Path.ControlPoints[SelectedNodeIndex].Dir), 0f);
					if (AngleDragging)
					{
						float num = MathUtil.SignedAngleDiff(angleFromDir, PrevDragAngle);
						ControlPoint value2 = Path.ControlPoints[SelectedNodeIndex];
						value2.Dir = MathUtil.GetDirFromAngle(MathUtil.GetAngleFromDir(value2.Dir, 0f) + num);
						Path.ControlPoints[SelectedNodeIndex] = value2;
						WantUpdatePath = true;
					}
					else
					{
						AngleDragging = true;
					}
					PrevDragAngle = angleFromDir;
				}
			}
			else
			{
				AngleDragging = false;
			}
		}
		if (WantUpdatePath)
		{
			WantUpdatePath = false;
			Path.BuildPoints(terrain);
			Path.UnityDeletePath();
			Path.UnityInitPath();
			WantRecalcRiverTiles = true;
		}
		if (WantRefresh)
		{
			WantRefresh = false;
			if (Path == null)
			{
				Items.Clear();
				for (int j = 0; j < terrain.Paths.Count; j++)
				{
					TerrainPath path = terrain.Paths[j];
					Items.Add(new DebugMenuItemCustom((string.IsNullOrEmpty(path.Name) ? ("Path " + j) : path.Name) + (path.IsRiver ? " (River)" : " (Road)"), delegate
					{
						Edit(path);
					}));
				}
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_New"), New));
			}
			else
			{
				int pathIndex = terrain.Paths.IndexOf(Path);
				Items.Clear();
				Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Name"), () => Path.Name, delegate(string v)
				{
					Path.Name = v;
				}));
				Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_IsRiver"), () => Path.IsRiver, delegate(bool v)
				{
					Path.IsRiver = v;
					WantUpdatePath = true;
					WantRefresh = true;
				}, affectsGameState: true));
				Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_HasMiddleLine"), () => Path.HasMiddleLine, delegate(bool v)
				{
					Path.HasMiddleLine = v;
					WantUpdatePath = true;
				}));
				Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_HasEdgeLine"), () => Path.HasEdgeLine, delegate(bool v)
				{
					Path.HasEdgeLine = v;
					WantUpdatePath = true;
				}));
				Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_EntranceNodeIndex"), -1, () => Path.ControlPoints.Count - 1, () => Path.EntrancePointIndex, delegate(int v)
				{
					Path.EntrancePointIndex = v;
				}, affectsGameState: true));
				Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_ExitNodeIndex"), -1, () => Path.ControlPoints.Count - 1, () => Path.ExitPointIndex, delegate(int v)
				{
					Path.ExitPointIndex = v;
				}, affectsGameState: true));
				Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_SpawningEnabled"), () => terrain.IsRoadEntranceEnabled(pathIndex), delegate(bool v)
				{
					terrain.SetRoadEntranceEnabled(pathIndex, v);
				}, affectsGameState: true));
				Items.Add(new DebugMenuItemToggle(GameImpl.Translate("DEBUG_HitTheRoadEnabled"), () => terrain.IsHitTheRoadEnabled(pathIndex), delegate(bool v)
				{
					terrain.SetHitTheRoadEnabled(pathIndex, v);
				}, affectsGameState: true));
				Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_DeletePath"), Delete));
				if (Path.ControlPoints.Count > 0 && SelectedNodeIndex < Path.ControlPoints.Count)
				{
					Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("DEBUG_SelectedNode"), 0, () => Path.ControlPoints.Count - 1, () => SelectedNodeIndex, delegate(int v)
					{
						SelectedNodeIndex = v;
					}, affectsGameState: true));
					Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Angle"), -179f, 179f, () => MathUtil.GetAngleFromDir(Path.ControlPoints[SelectedNodeIndex].Dir, 0f) * 57.29578f, delegate(float v)
					{
						ControlPoint value3 = Path.ControlPoints[SelectedNodeIndex];
						value3.Dir = MathUtil.GetDirFromAngle(v * (MathF.PI / 180f));
						Path.ControlPoints[SelectedNodeIndex] = value3;
						WantUpdatePath = true;
					}, affectsGameState: true));
					if (Path.IsRiver)
					{
						float terrainHeight = terrain.GetTileHeightAtPos(Path.ControlPoints[SelectedNodeIndex].Pos, ignoreIce: true, ignoreRoadCamber: true);
						float num2 = terrainHeight + 2f;
						if (SelectedNodeIndex > 0)
						{
							float val = terrain.GetTileHeightAtPos(Path.ControlPoints[SelectedNodeIndex - 1].Pos, ignoreIce: true, ignoreRoadCamber: true) + Path.ControlPoints[SelectedNodeIndex - 1].RiverDepth;
							num2 = Math.Min(num2, val);
						}
						Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_RiverDepth") + " (" + Path.ControlPoints[SelectedNodeIndex].RiverDepth.ToString("N2") + ")", terrainHeight - 2f, num2, () => terrainHeight + Path.ControlPoints[SelectedNodeIndex].RiverDepth, delegate(float v)
						{
							ControlPoint value3 = Path.ControlPoints[SelectedNodeIndex];
							value3.RiverDepth = v - terrainHeight;
							Path.ControlPoints[SelectedNodeIndex] = value3;
							WantUpdatePath = true;
							WantRefresh = true;
						}, affectsGameState: true));
					}
					Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_CarveRiverBedAll"), delegate
					{
						terrain.CarveRiverBed(Path, -1);
					}, affectsGameState: true));
					Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_CarveRiverBed"), delegate
					{
						terrain.CarveRiverBed(Path, SelectedNodeIndex);
					}, affectsGameState: true));
					Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_SetTextureAll"), delegate
					{
						terrain.ApplyPathTexture(Path, -1);
					}, affectsGameState: true));
					Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_SetTexture"), delegate
					{
						terrain.ApplyPathTexture(Path, SelectedNodeIndex);
					}, affectsGameState: true));
					Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_Width"), 1f, 40f, () => Path.ControlPoints[SelectedNodeIndex].Width, delegate(float v)
					{
						ControlPoint value3 = Path.ControlPoints[SelectedNodeIndex];
						value3.Width = v;
						Path.ControlPoints[SelectedNodeIndex] = value3;
						WantUpdatePath = true;
					}, affectsGameState: true));
					Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_DeleteNode"), DeleteNode));
					if (!Path.IsRiver)
					{
						if (Path.EntrancePointIndex != -1)
						{
							Items.Add(new DebugMenuItemEnum<RoadDestinationType>(GameImpl.Translate("DEBUG_EntranceTraitsType"), RoadDestinationType.HarderSettings, () => Path.EntranceRoadTraits.Type, delegate(RoadDestinationType v)
							{
								RoadDestinationTraits entranceRoadTraits = Path.EntranceRoadTraits;
								entranceRoadTraits.Type = v;
								Path.EntranceRoadTraits = entranceRoadTraits;
								WantRefresh = true;
							}));
							if (Path.EntranceRoadTraits.Type == RoadDestinationType.Random)
							{
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_Urbanization"), 0f, 1f, () => Path.EntranceRoadTraits.Urbanized, delegate(float v)
								{
									RoadDestinationTraits entranceRoadTraits = Path.EntranceRoadTraits;
									entranceRoadTraits.Urbanized = v;
									Path.EntranceRoadTraits = entranceRoadTraits;
								}));
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_Population"), 0f, 1f, () => Path.EntranceRoadTraits.Populated, delegate(float v)
								{
									RoadDestinationTraits entranceRoadTraits = Path.EntranceRoadTraits;
									entranceRoadTraits.Populated = v;
									Path.EntranceRoadTraits = entranceRoadTraits;
								}));
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_Infection"), 0f, 1f, () => Path.EntranceRoadTraits.Infected, delegate(float v)
								{
									RoadDestinationTraits entranceRoadTraits = Path.EntranceRoadTraits;
									entranceRoadTraits.Infected = v;
									Path.EntranceRoadTraits = entranceRoadTraits;
								}));
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_InvisibleStrain"), 0f, 1f, () => Path.EntranceRoadTraits.InvisibleStrain, delegate(float v)
								{
									RoadDestinationTraits entranceRoadTraits = Path.EntranceRoadTraits;
									entranceRoadTraits.InvisibleStrain = v;
									Path.EntranceRoadTraits = entranceRoadTraits;
								}));
							}
							Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("HUD_SkillCapBonus"), 0, 5, () => Path.EntranceRoadTraits.SkillCapBonus, delegate(int v)
							{
								RoadDestinationTraits entranceRoadTraits = Path.EntranceRoadTraits;
								entranceRoadTraits.SkillCapBonus = v;
								Path.EntranceRoadTraits = entranceRoadTraits;
							}));
						}
						if (Path.ExitPointIndex != -1)
						{
							Items.Add(new DebugMenuItemEnum<RoadDestinationType>(GameImpl.Translate("DEBUG_ExitTraitsType"), RoadDestinationType.HarderSettings, () => Path.ExitRoadTraits.Type, delegate(RoadDestinationType v)
							{
								RoadDestinationTraits exitRoadTraits = Path.ExitRoadTraits;
								exitRoadTraits.Type = v;
								Path.ExitRoadTraits = exitRoadTraits;
								WantRefresh = true;
							}));
							if (Path.ExitRoadTraits.Type == RoadDestinationType.Random)
							{
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_Urbanization"), 0f, 1f, () => Path.ExitRoadTraits.Urbanized, delegate(float v)
								{
									RoadDestinationTraits exitRoadTraits = Path.ExitRoadTraits;
									exitRoadTraits.Urbanized = v;
									Path.ExitRoadTraits = exitRoadTraits;
								}));
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_Population"), 0f, 1f, () => Path.ExitRoadTraits.Populated, delegate(float v)
								{
									RoadDestinationTraits exitRoadTraits = Path.ExitRoadTraits;
									exitRoadTraits.Populated = v;
									Path.ExitRoadTraits = exitRoadTraits;
								}));
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_Infection"), 0f, 1f, () => Path.ExitRoadTraits.Infected, delegate(float v)
								{
									RoadDestinationTraits exitRoadTraits = Path.ExitRoadTraits;
									exitRoadTraits.Infected = v;
									Path.ExitRoadTraits = exitRoadTraits;
								}));
								Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("HUD_InvisibleStrain"), 0f, 1f, () => Path.ExitRoadTraits.InvisibleStrain, delegate(float v)
								{
									RoadDestinationTraits exitRoadTraits = Path.ExitRoadTraits;
									exitRoadTraits.InvisibleStrain = v;
									Path.ExitRoadTraits = exitRoadTraits;
								}));
							}
							Items.Add(new DebugMenuIntAdjuster(GameImpl.Translate("HUD_SkillCapBonus"), 0, 5, () => Path.ExitRoadTraits.SkillCapBonus, delegate(int v)
							{
								RoadDestinationTraits exitRoadTraits = Path.ExitRoadTraits;
								exitRoadTraits.SkillCapBonus = v;
								Path.ExitRoadTraits = exitRoadTraits;
							}));
						}
					}
				}
			}
		}
		if (Path == null)
		{
			base.HandleInputImpl(inputFrame);
		}
		else if (instance2.IsJustPressed(InputFunction.Back))
		{
			Edit(null);
		}
	}

	public void Edit(TerrainPath path)
	{
		Path = path;
		SelectedNodeIndex = 0;
		WantRefresh = true;
		Items.Clear();
	}

	public void New()
	{
		TerrainPath terrainPath = new TerrainPath();
		terrainPath.DefaultWidth = 5f;
		terrainPath.ControlPoints = new List<ControlPoint>();
		terrainPath.Points = new List<TerrainPathPoint>();
		GameTerrain.Instance.Paths.Add(terrainPath);
		GameTerrain.Instance.SetupPathIndices();
		Session.Instance.AchievementsEnabled = false;
		Edit(terrainPath);
	}

	public void DeleteNode()
	{
		Path.ControlPoints.RemoveAt(SelectedNodeIndex);
		SelectedNodeIndex = Math.Min(SelectedNodeIndex, Path.ControlPoints.Count - 1);
		WantUpdatePath = true;
		WantRefresh = true;
		Items.Clear();
		Session.Instance.AchievementsEnabled = false;
	}

	public void Delete()
	{
		GameTerrain.Instance.DeletePath(Path);
		Path = null;
		HoveredPath = null;
		SelectedNodeIndex = 0;
		HoveredNodeIndex = 0;
		WantRefresh = true;
		Items.Clear();
		Session.Instance.AchievementsEnabled = false;
	}

	public void RecalcRiverTiles()
	{
		GameTerrain instance = GameTerrain.Instance;
		instance.RecalcRiverTiles(new TerrainCoord(0, 0), new TerrainCoord(instance.Size - 1, instance.Size - 1));
		instance.BuildEntireMinimap();
	}
}
