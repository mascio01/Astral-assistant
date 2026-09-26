using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MinimapCameraBehaviour : MonoBehaviour
{
	private static Vector3?[] LeftResults = new Vector3?[4];

	private static Vector3?[] RightResults = new Vector3?[4];

	public static float HuntersMapIconSizeZoomedOut = 24f;

	public static float HuntersMapIconSizeZoomedIn = 4f;

	public static float HuntersMapIconMaxHeight = 512f;

	public static float QuestDestIconSize = 12f;

	public static float CursorSize = 0.0625f;

	public static float DotSeparation = 4f;

	public static float DotRadius = 1f;

	public static float SelectionLineThickness = 1f / 128f;

	public static float ZoneLineThickness = 1f;

	public static float ZoneDotSeparation = 2f;

	public static bool DrawTownsOnMap;

	private Camera UnityCamera;

	public TileObject HoveredObj;

	public Vector2 CursorPosWorldXZ;

	public MineralType ShowMineralType = MineralType.None;

	public static TaskFunc BuildCharactersToRenderTaskFunc = BuildCharactersToRenderListOnThread;

	public static ManualResetEvent BuildCharactersToRenderFinishedEvent = new ManualResetEvent(initialState: true);

	public static List<KeyValuePair<Character, Color>> CharactersToRender = new List<KeyValuePair<Character, Color>>();

	private static bool BuildingCharactersToRenderList = false;

	private static string MinimapRenderWithoutCameraStr = "MinimapRenderWithoutCamera";

	private HashSet<TerrainRect> AlreadyDrawn = new HashSet<TerrainRect>();

	private void Awake()
	{
		UnityCamera = GetComponent<Camera>();
	}

	public static void StartBuildingCharactersToRenderListOnThread()
	{
		MakeSureBuildingCharactersToRenderThreadIsFinished();
		CharactersToRender.Clear();
		BuildCharactersToRenderFinishedEvent.Reset();
		BuildingCharactersToRenderList = true;
		GameImpl.Instance.UpdateThreadPool.AddTask(BuildCharactersToRenderTaskFunc, null, null, TaskPriority.High);
	}

	public static void MakeSureBuildingCharactersToRenderThreadIsFinished()
	{
		if (BuildingCharactersToRenderList)
		{
			BuildCharactersToRenderFinishedEvent.WaitOne();
		}
	}

	public static void BuildCharactersToRenderListOnThread(BaseTaskData data)
	{
		try
		{
			Vector2 vector = MathUtil.ToXZ(Session.Instance.GameCamera.Focus);
			float num = 2f * MathUtil.Squared(Minimap.DefaultHeight + 1f);
			foreach (Character character in Session.Instance.CharacterManager.Characters)
			{
				Color32 mapColor = character.MapColor;
				if (mapColor.a != 0 && !((character.PosXZ - vector).sqrMagnitude > num))
				{
					CharactersToRender.Add(new KeyValuePair<Character, Color>(character, mapColor));
				}
			}
			BuildCameraGroundIntersections();
		}
		catch (Exception ex)
		{
			Debug.Log("Error in BuildCharactersToRenderListOnThread: " + ex.Message + ex.StackTrace);
		}
		BuildCharactersToRenderFinishedEvent.Set();
		BuildingCharactersToRenderList = false;
	}

	public static void BuildCameraGroundIntersections()
	{
		GameCamera gameCamera = Session.Instance.GameCamera;
		GameTerrain instance = GameTerrain.Instance;
		CameraBehaviour unityGameCameraBehaviour = HudBehaviour.Instance.UnityGameCameraBehaviour;
		float tileHeightAtPos = instance.GetTileHeightAtPos(gameCamera.Focus.x, gameCamera.Focus.z);
		Plane plane = new Plane(Vector3.up, 0f - tileHeightAtPos);
		LeftResults[0] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.NearFrustumCorners[0], unityGameCameraBehaviour.NearFrustumCorners[1]);
		LeftResults[1] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.NearFrustumCorners[0], unityGameCameraBehaviour.FogFrustumCorners[0]);
		LeftResults[2] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.NearFrustumCorners[1], unityGameCameraBehaviour.FogFrustumCorners[1]);
		LeftResults[3] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.FogFrustumCorners[0], unityGameCameraBehaviour.FogFrustumCorners[1]);
		RightResults[0] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.NearFrustumCorners[3], unityGameCameraBehaviour.NearFrustumCorners[2]);
		RightResults[1] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.NearFrustumCorners[3], unityGameCameraBehaviour.FogFrustumCorners[3]);
		RightResults[2] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.NearFrustumCorners[2], unityGameCameraBehaviour.FogFrustumCorners[2]);
		RightResults[3] = MathUtil.GetLinePlaneIntersection(plane, unityGameCameraBehaviour.FogFrustumCorners[3], unityGameCameraBehaviour.FogFrustumCorners[2]);
	}

	public void RenderWithoutCamera()
	{
		using (new UnityProfileMarker(MinimapRenderWithoutCameraStr))
		{
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = UnityCamera.targetTexture;
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			GL.PushMatrix();
			GL.LoadProjectionMatrix(UnityCamera.projectionMatrix);
			GL.modelview = UnityCamera.worldToCameraMatrix;
			OnPostRender();
			GL.PopMatrix();
			RenderTexture.active = active;
		}
	}

	private void OnPostRender()
	{
		Session instance = Session.Instance;
		if (instance == null || instance.State != SessionState.Started)
		{
			return;
		}
		StoryManager instance2 = StoryManager.Instance;
		_ = instance.CharacterManager;
		CommunityManager communityManager = instance.CommunityManager;
		GameTerrain instance3 = GameTerrain.Instance;
		Hud instance4 = Hud.Instance;
		PlayerRecord localPlayerRecord = instance.GetLocalPlayerRecord();
		float halfSize = instance3.HalfSize;
		Texture2D geologicalTex = ((ShowMineralType != MineralType.None) ? instance3.GeologicalTex[(int)ShowMineralType] : null);
		Color mineralCol = ((ShowMineralType != MineralType.None) ? GameTerrain.MinimapSettings.GetMineralCol(ShowMineralType) : Color.white);
		int num = ((ShowMineralType != MineralType.None) ? (instance.Editor ? 15 : communityManager.PlayerCommunity.GetGeologicalMapQuadrants()) : 0);
		int num2 = Math.Max(1, instance3.Size >> 9);
		float num3 = num2;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int num4 = i + j * num2;
				float mineralThreshold = ((((1 << num4) & num) != 0 || !FogOfWar.DebugFogOfWarEnabled) ? GameTerrain.MineralThreshold : 1000f);
				Vector2 uv = new Vector2((float)i / num3, (float)j / num3);
				Vector2 uv2 = new Vector2((float)(i + 1) / num3, (float)(j + 1) / num3);
				DebugGraphics.DrawMinimapQuadXZWithUVs(tl: new Vector2(Mathf.Lerp(0f - halfSize, halfSize, uv.x), Mathf.Lerp(0f - halfSize, halfSize, uv.y)), br: new Vector2(Mathf.Lerp(0f - halfSize, halfSize, uv2.x), Mathf.Lerp(0f - halfSize, halfSize, uv2.y)), localToWorldMatrix: Matrix4x4.identity, uv0: uv, uv1: uv2, y: 0f, fogCol: GameImpl.Instance.Sun.LightingSettings.FogTop, tex: instance3.MinimapTex, fogOfWarTex: instance3.FogOfWar.GetCurTex(), geologicalTex: geologicalTex, mineralCol: mineralCol, mineralThreshold: mineralThreshold);
			}
		}
		Vector3 up = base.transform.up;
		Vector3 right = base.transform.right;
		if (!instance4.DraggingZone && !instance4.SentMovementZone)
		{
			AlreadyDrawn.Clear();
			foreach (Character selectedCharacter in instance4.SelectedCharacters)
			{
				if (selectedCharacter != instance4.LocalControlledCharacter && selectedCharacter.MovementZonePaused)
				{
					DrawMovementZone(selectedCharacter, isControlledCharacter: false);
				}
			}
			AlreadyDrawn.Clear();
			foreach (Character selectedCharacter2 in instance4.SelectedCharacters)
			{
				if (selectedCharacter2 != instance4.LocalControlledCharacter && !selectedCharacter2.MovementZonePaused)
				{
					DrawMovementZone(selectedCharacter2, isControlledCharacter: false);
				}
			}
			AlreadyDrawn.Clear();
			if (instance4.LocalControlledCharacter != null)
			{
				DrawMovementZone(instance4.LocalControlledCharacter, isControlledCharacter: true);
			}
		}
		if (instance4.LocalControlledCharacter != null)
		{
			Character character = instance4.LocalControlledCharacter;
			if (InfoScreen.GetAllowViewInfoOnAnyone() && Hud.Instance.Pip.FocusObject is Character)
			{
				character = Hud.Instance.Pip.FocusObject as Character;
			}
			if (character != null && character.GetRouteCount() > 0)
			{
				DebugGraphics.StartDrawTexturedQuads(Matrix4x4.identity, Hud.SmallDot);
				Vector2 vector = MathUtil.ToXZ(base.transform.position);
				Vector2 vector2 = new Vector2(base.transform.position.y * UnityCamera.aspect, base.transform.position.y);
				Rect other = Rect.MinMaxRect(vector.x - vector2.x, vector.y - vector2.y, vector.x + vector2.x, vector.y + vector2.y);
				Vector3 vector3 = up * DotRadius;
				Vector3 vector4 = right * DotRadius;
				Vector2 vector5 = instance3.GetTileCentreXZ(character.GetRouteNode(character.GetRouteCount() - 1));
				float num5 = 0f;
				for (int num6 = character.GetRouteCount() - 2; num6 >= -1; num6--)
				{
					Vector2 vector6 = ((num6 >= 0) ? instance3.GetTileCentreXZ(character.GetRouteNode(num6)) : character.PosXZ);
					if (Rect.MinMaxRect(Mathf.Min(vector5.x, vector6.x), Mathf.Min(vector5.y, vector6.y), Mathf.Max(vector5.x, vector6.x), Mathf.Max(vector5.y, vector6.y)).Overlaps(other))
					{
						Vector2 vector7 = vector6 - vector5;
						float magnitude = vector7.magnitude;
						if (!(magnitude < 0.0001f))
						{
							vector7 /= magnitude;
							num5 += magnitude;
							while (num5 > DotSeparation)
							{
								vector5 += vector7 * DotSeparation;
								num5 -= DotSeparation;
								Vector3 vector8 = MathUtil.ToX0Y(vector5);
								DebugGraphics.DrawQuad(vector8 + vector3 - vector4, vector8 + vector3 + vector4, vector8 - vector3 + vector4, vector8 - vector3 - vector4);
							}
						}
					}
					else
					{
						num5 = 0f;
						vector5 = vector6;
					}
				}
				DebugGraphics.EndDrawQuads();
			}
		}
		DebugGraphics.StartDrawMapIcons(Matrix4x4.identity, Hud.DeerIcon);
		foreach (DeerSpawnPoint deerSpawnPoint in instance.CommunityManager.DeerSpawnPoints)
		{
			if (deerSpawnPoint.Discovered || SpawnPoint.DebugShowSpawnPoints || instance.Editor)
			{
				float radius = Mathf.Lerp(HuntersMapIconSizeZoomedIn, HuntersMapIconSizeZoomedOut, Mathf.Clamp01((base.transform.position.y - MapPage.MinHeight) / (HuntersMapIconMaxHeight - MapPage.MinHeight))) * 0.5f;
				bool flag = deerSpawnPoint.HerdSize == 0 || !deerSpawnPoint.CanSpawnOnTile();
				DebugGraphics.DrawMapIcon(MathUtil.ToX0Y(deerSpawnPoint.PosXZ), flag ? new Color32(128, 128, 128, byte.MaxValue) : MathUtil.White, radius, up, right);
			}
		}
		DebugGraphics.EndDrawMapIcons();
		DebugGraphics.StartDrawMapIcons(Matrix4x4.identity, Hud.MapIconMat);
		if (InfoScreen.Instance.IsShowingMap())
		{
			BuildCameraGroundIntersections();
			foreach (Character character3 in instance.CharacterManager.Characters)
			{
				Color32 mapColor = character3.MapColor;
				if (mapColor.a != 0)
				{
					DebugGraphics.DrawMapCharacterIcon(MathUtil.ToX0Y(character3.PosXZ), mapColor, character3.MapDotSize, up, right, (int)character3.MapIconType, character3.NonDeterministicSelected, character3.FacingAngle);
				}
			}
		}
		else
		{
			BuildCharactersToRenderFinishedEvent.WaitOne();
			for (int k = 0; k < CharactersToRender.Count; k++)
			{
				Character key = CharactersToRender[k].Key;
				DebugGraphics.DrawMapCharacterIcon(MathUtil.ToX0Y(key.PosXZ), CharactersToRender[k].Value, key.MapDotSize, up, right, (int)key.MapIconType, key.NonDeterministicSelected, key.FacingAngle);
			}
			CharactersToRender.Clear();
		}
		DebugGraphics.EndDrawMapIcons();
		if (instance3.FogOfWar.HasRadio)
		{
			foreach (Squad squadsWithMapIcon in communityManager.SquadsWithMapIcons)
			{
				Resource<Texture2D> iconForSquadBehaviour = GetIconForSquadBehaviour(squadsWithMapIcon.Behaviour);
				if (iconForSquadBehaviour != null && iconForSquadBehaviour.GetAsset() != null)
				{
					DrawInvaderIcon(squadsWithMapIcon.GetLeader(), up, right, iconForSquadBehaviour.GetAsset());
				}
			}
			foreach (BaseObject hunter in communityManager.Hunters)
			{
				InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(hunter);
				if (invaderInstanceThatCreatedHunter != null && invaderInstanceThatCreatedHunter.Invader != null && invaderInstanceThatCreatedHunter.Invader.IconResource != null && invaderInstanceThatCreatedHunter.Invader.IconResource.GetAsset() != null)
				{
					Community community = hunter as Community;
					BaseObject baseObject = null;
					baseObject = ((community == null) ? hunter : ((community.Squads.Count <= 0 || community.Squads[0].GetLeader() == null) ? community.GetFirstActiveMember() : community.Squads[0].GetLeader()));
					DrawInvaderIcon(baseObject, up, right, invaderInstanceThatCreatedHunter.Invader.IconResource.GetAsset());
				}
			}
		}
		if (DrawTownsOnMap)
		{
			foreach (Town town in Session.Instance.CommunityManager.Towns)
			{
				DebugGraphics.StartDrawLines(Matrix4x4.identity);
				DebugGraphics.DrawCircleXZ(MathUtil.ToX0Y(town.PosXZ), town.Radius, Town.TownCol);
				DebugGraphics.EndDrawLines();
			}
		}
		if (HoveredObj != null)
		{
			Bounds boundingBox = HoveredObj.GetBoundingBox();
			Vector2 tl = MathUtil.ToXZ(boundingBox.min);
			Vector2 br = MathUtil.ToXZ(boundingBox.max);
			if (HoveredObj is Character character2)
			{
				tl = character2.PosXZ - Vector2.one * character2.MapDotSize;
				br = character2.PosXZ + Vector2.one * character2.MapDotSize;
			}
			if (HoveredObj.GetBaseObjectType() != BaseObjectType.Town)
			{
				DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: false);
				DebugGraphics.DrawRectOutlineXZ(tl, br, 0f, 1f, Color.black);
				DebugGraphics.DrawRectOutlineXZ(tl, br, 1f, 3f, Color.white);
				DebugGraphics.DrawRectOutlineXZ(tl, br, 3f, 4f, Color.black);
				DebugGraphics.EndDrawQuads();
			}
		}
		if (localPlayerRecord != null)
		{
			for (int l = 0; l < localPlayerRecord.MapMarkerLocations.Count; l++)
			{
				DrawDestMarker(null, MathUtil.ToX0Y(instance3.GetTileCentreXZ(localPlayerRecord.MapMarkerLocations[l].Tile)), up, right, localPlayerRecord.MapMarkerLocations[l].Type);
			}
		}
		if (instance2.QuestDestinations.Count > 0)
		{
			foreach (TileObject questDestination in instance2.QuestDestinations)
			{
				Vector3 pos = questDestination.Pos;
				pos.y = 0f;
				DrawDestMarker(questDestination, pos, up, right, MapMarkerType.White);
			}
		}
		Vector3 p = Vector3.zero;
		Vector3 p2 = Vector3.zero;
		Vector3 p3 = Vector3.zero;
		Vector3 p4 = Vector3.zero;
		int num7 = 0;
		int num8 = 0;
		for (int m = 0; m < 4; m++)
		{
			if (LeftResults[m].HasValue)
			{
				switch (num7)
				{
				case 0:
					p = LeftResults[m].Value;
					break;
				case 1:
					p2 = LeftResults[m].Value;
					break;
				}
				num7++;
			}
			if (RightResults[m].HasValue)
			{
				switch (num8)
				{
				case 0:
					p3 = RightResults[m].Value;
					break;
				case 1:
					p4 = RightResults[m].Value;
					break;
				}
				num8++;
			}
		}
		if (num7 == 2 && num8 == 2)
		{
			p.y = 0f;
			p2.y = 0f;
			p3.y = 0f;
			p4.y = 0f;
			DebugGraphics.StartDrawQuadsAdditive(Matrix4x4.identity);
			DebugGraphics.DrawQuad(p3, p, p2, p4, new Color(0.25f, 0.25f, 0.25f, 0.25f), MathUtil.TransparentBlackCol);
			DebugGraphics.EndDrawQuads();
		}
		if (instance4.DraggingZone)
		{
			DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: false, dotted: true);
			DebugGraphics.DrawDottedRectXZ(instance3.GetTileCentreXZ(instance4.DragZoneStartTile), instance3.GetTileCentreXZ(instance4.DragZoneFinishTile), ZoneLineThickness, ZoneDotSeparation, Color.white);
			DebugGraphics.EndDrawMapIcons();
		}
		else if (MapPage.Instance.AltDragging)
		{
			float outer = base.transform.position.y * SelectionLineThickness;
			DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: false);
			DebugGraphics.DrawRectOutlineXZ(MapPage.Instance.AltDragStartPosWorldXZ, MapPage.Instance.CursorPosWorldXZ, 0f, outer, Color.white);
			DebugGraphics.EndDrawMapIcons();
		}
	}

	public static Resource<Texture2D> GetIconForSquadBehaviour(SquadBehaviour behaviour)
	{
		switch (behaviour)
		{
		case SquadBehaviour.Trade:
			return GameImpl.Instance.TraderSquadMapIcon;
		case SquadBehaviour.Extortion:
		case SquadBehaviour.Beg:
		case SquadBehaviour.WarnOffAlliance:
		case SquadBehaviour.WarnPopulation:
		case SquadBehaviour.RequestAlliance:
			return GameImpl.Instance.ExtortionSquadMapIcon;
		default:
			return null;
		}
	}

	private void DrawInvaderIcon(BaseObject obj, Vector3 up, Vector3 right, Texture2D icon)
	{
		bool flag = false;
		Vector2 v = Vector2.zero;
		Color color = Color.white;
		if (obj is Character character)
		{
			v = character.PosXZ;
			flag = true;
			if (character.Infection != InfectionType.None)
			{
				color = GameTerrain.MinimapSettings.GetInfectionCol(character.Infection);
			}
			else
			{
				Community community = character.Community;
				if (community.IsLooterCommunity() && Session.Instance.CommunityManager.PlayerCommunity.GetRelationship(community, out var community1Surrendered) == CommunityRelationshipType.Ceasefire && !community1Surrendered)
				{
					color = Color.gray;
				}
			}
		}
		else if (obj is TileObject tileObject)
		{
			v = tileObject.PosXZ;
			flag = true;
		}
		if (flag)
		{
			float radius = Mathf.Lerp(HuntersMapIconSizeZoomedIn, HuntersMapIconSizeZoomedOut, Mathf.Clamp01((base.transform.position.y - MapPage.MinHeight) / (HuntersMapIconMaxHeight - MapPage.MinHeight)));
			DebugGraphics.StartDrawMapIcons(Matrix4x4.identity, icon);
			DebugGraphics.DrawMapIcon(MathUtil.ToX0Y(v), color, radius, up, right);
			DebugGraphics.EndDrawMapIcons();
		}
	}

	private void DrawMovementZone(Character character, bool isControlledCharacter)
	{
		if (character.MovementZone != TerrainRect.Invalid && !AlreadyDrawn.Contains(character.MovementZone))
		{
			AlreadyDrawn.Add(character.MovementZone);
			GameTerrain instance = GameTerrain.Instance;
			Color col = (character.MovementZonePaused ? Color.gray : Color.white);
			if (!isControlledCharacter)
			{
				col.a = 0.25f;
			}
			Vector2 tileCentreXZ = instance.GetTileCentreXZ(character.MovementZone.min);
			Vector2 tileCentreXZ2 = instance.GetTileCentreXZ(character.MovementZone.max);
			DebugGraphics.StartDrawQuads(Matrix4x4.identity, wantZTest: false, dotted: true);
			DebugGraphics.DrawDottedRectXZ(tileCentreXZ, tileCentreXZ2, ZoneLineThickness, ZoneDotSeparation, col);
			DebugGraphics.EndDrawQuads();
		}
	}

	private void DrawDestMarker(TileObject obj, Vector3 worldPos, Vector3 up, Vector3 right, MapMarkerType markerType)
	{
		Color mapMarkerColor = MapPage.GetMapMarkerColor(markerType);
		Vector3 position = UnityCamera.WorldToViewportPoint(worldPos);
		if (position.x < 0f || position.x > 1f || position.y < 0f || position.y > 1f)
		{
			float magnitude = MathUtil.ToXZ(base.transform.position - worldPos).magnitude;
			position.x = Mathf.Clamp01(position.x);
			position.y = Mathf.Clamp01(position.y);
			Vector3 vector = UnityCamera.ViewportToWorldPoint(position);
			Vector2 vector2 = MathUtil.ToXZ(base.transform.position - vector);
			Vector2 v = MathUtil.SafeNormalize(vector2, MathUtil.ToXZ(up));
			Vector3 vector3 = MathUtil.ToX0Y(vector2 / new Vector2(base.transform.position.y * UnityCamera.aspect, base.transform.position.y));
			float num = Mathf.Lerp(1f, 0.75f, Mathf.Sqrt(Mathf.Clamp01(magnitude / 1024f)));
			DebugGraphics.StartDrawMapIcons(Matrix4x4.identity, GameCursor.CurrentQuestIconCharacter);
			DebugGraphics.DrawMapIcon(vector + vector3 * QuestDestIconSize * 0.5f * num, mapMarkerColor, QuestDestIconSize * num, MathUtil.ToX0Y(v), MathUtil.ToX0Y(MathUtil.RightNormal(v)));
			DebugGraphics.EndDrawMapIcons();
		}
		else
		{
			Texture2D tex = ((obj is Character) ? GameCursor.CurrentQuestIconCharacter : GameCursor.CurrentQuestIcon);
			DebugGraphics.StartDrawMapIcons(Matrix4x4.identity, tex);
			DebugGraphics.DrawMapIcon(worldPos + up * QuestDestIconSize * 0.5f, mapMarkerColor, QuestDestIconSize, up, right);
			DebugGraphics.EndDrawMapIcons();
		}
	}
}
