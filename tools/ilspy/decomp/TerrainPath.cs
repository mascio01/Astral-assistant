using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPath : IReflectable
{
	public int Index;

	public bool IsRiver;

	public bool HasMiddleLine = true;

	public bool HasEdgeLine = true;

	public List<ControlPoint> ControlPoints;

	public List<TerrainPathPoint> Points;

	public float DefaultWidth;

	public List<MeshCollider> UnityPathObjects;

	public TerrainRect TotalBounds;

	public List<TerrainRect> LocalBounds;

	public string Name;

	public int EntrancePointIndex = -1;

	public int ExitPointIndex = -1;

	public RoadDestinationTraits EntranceRoadTraits = RoadDestinationTraits.Create();

	public RoadDestinationTraits ExitRoadTraits = RoadDestinationTraits.Create();

	public const int LocalBoundsSplit = 32;

	private const int CamberTransitionInPoints = 3;

	public static float ControlPointBoxSize = 0.1f;

	public static float SampleDensity = 0.5f;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Index);
		reflector.Add(ref IsRiver);
		reflector.AddAfter(ref HasMiddleLine, 473);
		reflector.AddAfter(ref HasEdgeLine, 473);
		reflector.Add(ref ControlPoints);
		reflector.Add(ref Points);
		reflector.Add(ref DefaultWidth);
		TotalBounds.Reflect(reflector);
		reflector.Add(ref LocalBounds);
		reflector.AddAfter(ref Name, 224);
		if (reflector.Version >= 477)
		{
			reflector.Add(ref EntrancePointIndex);
			reflector.Add(ref ExitPointIndex);
		}
		else if (ControlPoints.Count >= 3)
		{
			EntrancePointIndex = 1;
			ExitPointIndex = ControlPoints.Count - 2;
		}
		if (reflector.Version >= 480)
		{
			EntranceRoadTraits.Reflect(reflector);
			ExitRoadTraits.Reflect(reflector);
		}
		if (reflector.Version < 224)
		{
			for (int i = 0; i < ControlPoints.Count; i++)
			{
				ControlPoint value = ControlPoints[i];
				value.Width = DefaultWidth;
				value.RiverDepth = 1f;
				ControlPoints[i] = value;
			}
			for (int j = 0; j < Points.Count; j++)
			{
				TerrainPathPoint value2 = Points[j];
				value2.Width = DefaultWidth;
				Points[j] = value2;
			}
		}
	}

	public TerrainPathPoint GetPoint(float index)
	{
		int index2 = MathUtil.Clamp(Mathf.FloorToInt(index), 0, Points.Count - 1);
		int index3 = MathUtil.Clamp(Mathf.CeilToInt(index), 0, Points.Count - 1);
		float t = index - Mathf.Floor(index);
		return new TerrainPathPoint
		{
			Pos = Vector3.Lerp(Points[index2].Pos, Points[index3].Pos, t),
			DirXZ = Vector2.Lerp(Points[index2].DirXZ, Points[index3].DirXZ, t),
			Width = Mathf.Lerp(Points[index2].Width, Points[index3].Width, t)
		};
	}

	public float CalcPathLength()
	{
		float num = 0f;
		for (int i = 1; i < Points.Count; i++)
		{
			num += MathUtil.ToXZ(Points[i].Pos - Points[i - 1].Pos).magnitude;
		}
		return num;
	}

	public void BuildBoundingBoxes(GameTerrain terrain)
	{
		LocalBounds = new List<TerrainRect>();
		int num = 0;
		TerrainRect item = default(TerrainRect);
		item.min = (TotalBounds.min = new TerrainCoord(terrain.Size, terrain.Size));
		item.max = (TotalBounds.max = new TerrainCoord(-1, -1));
		for (int i = 0; i < Points.Count; i++)
		{
			TerrainCoord tileCoordForPos = terrain.GetTileCoordForPos(Points[i].Pos);
			int num2 = Mathf.CeilToInt(Points[i].Width * 0.5f);
			TotalBounds.min = TerrainCoord.Min(TotalBounds.min, tileCoordForPos - new TerrainCoord(num2, num2));
			TotalBounds.max = TerrainCoord.Max(TotalBounds.max, tileCoordForPos + new TerrainCoord(num2, num2));
			item.min = TerrainCoord.Min(item.min, tileCoordForPos - new TerrainCoord(num2, num2));
			item.max = TerrainCoord.Max(item.max, tileCoordForPos + new TerrainCoord(num2, num2));
			num++;
			if (num >= 32 || i >= Points.Count - 1)
			{
				LocalBounds.Add(item);
				item.min = tileCoordForPos - new TerrainCoord(num2, num2);
				item.max = tileCoordForPos + new TerrainCoord(num2, num2);
				num = 0;
			}
		}
	}

	public float GetAmountOnPath(GameTerrain terrain, Vector2 posXZ, out float h)
	{
		h = 0f;
		TerrainCoord tileCoordForPosXZ = terrain.GetTileCoordForPosXZ(posXZ);
		if (!TotalBounds.Contains(tileCoordForPosXZ))
		{
			return 1f;
		}
		float num = DefaultWidth * 0.5f;
		float num2 = float.MaxValue;
		for (int i = 0; i < LocalBounds.Count; i++)
		{
			if (!LocalBounds[i].Contains(tileCoordForPosXZ))
			{
				continue;
			}
			for (int j = i * 32; j < Math.Min((i + 1) * 32, Points.Count - 1); j++)
			{
				Vector3 pos = Points[j].Pos;
				Vector3 pos2 = Points[j + 1].Pos;
				float t;
				float num3 = Mathf.Sqrt(MathUtil.GetDistSqFromSegmentToPoint(MathUtil.ToXZ(pos), MathUtil.ToXZ(pos2), posXZ, out t));
				float num4 = Mathf.Lerp(Points[j].Width, Points[j + 1].Width, t) * 0.5f;
				if (num3 < num2 && num3 < num4)
				{
					h = Mathf.Lerp(pos.y, pos2.y, t);
					if (!IsRiver)
					{
						float num5 = ((!Session.Instance.Editor && !GameTerrain.Instance.Generating) ? GameTerrain.Instance.GetOriginalTileHeightAtPos(posXZ.x, posXZ.y) : GameTerrain.Instance.GetTileHeightAtPos(posXZ, ignoreIce: true, ignoreRoadCamber: true));
						float num6 = 1f - MathUtil.Squared(num3 / num4);
						num6 *= Mathf.Clamp01(((float)j + t) / 3f);
						num6 *= Mathf.Clamp01(((float)(Points.Count - 1) - ((float)j + t)) / 3f);
						h = num5 + num6 * 0.2f;
					}
					num2 = num3;
					num = num4;
				}
			}
		}
		return (float)Math.Sqrt(num2) / num;
	}

	public bool GetClosestPointOnPathToPos(GameTerrain terrain, Vector2 posXZ, ref float closestDistSq, ref Vector3 closestPointOnPath, ref Vector2 closestDirXZOnPath, ref float closestPathIndex)
	{
		return GetClosestPointOnPathToPos(terrain, posXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex, mustBeInWater: false);
	}

	public bool GetClosestPointOnPathToPos(GameTerrain terrain, Vector2 posXZ, ref float closestDistSq, ref Vector3 closestPointOnPath, ref Vector2 closestDirXZOnPath, ref float closestPathIndex, bool mustBeInWater)
	{
		bool result = false;
		TerrainCoord tileCoordForPosXZ = terrain.GetTileCoordForPosXZ(posXZ);
		if (TotalBounds.GetClosestDistSqTo(tileCoordForPosXZ) < closestDistSq)
		{
			for (int i = 0; i < LocalBounds.Count; i++)
			{
				if (!(LocalBounds[i].GetClosestDistSqTo(tileCoordForPosXZ) < closestDistSq))
				{
					continue;
				}
				for (int j = i * 32; j < Math.Min((i + 1) * 32, Points.Count - 1); j++)
				{
					Vector3 pos = Points[j].Pos;
					Vector3 pos2 = Points[j + 1].Pos;
					float t;
					float distSqFromSegmentToPoint = MathUtil.GetDistSqFromSegmentToPoint(MathUtil.ToXZ(pos), MathUtil.ToXZ(pos2), posXZ, out t);
					if (distSqFromSegmentToPoint < closestDistSq)
					{
						Vector3 vector = Vector3.Lerp(pos, pos2, t);
						TerrainCoord tileCoordForPos = terrain.GetTileCoordForPos(vector);
						if (!terrain.IsTileOutsideBounds(tileCoordForPos.x, tileCoordForPos.y) && (!mustBeInWater || (terrain.IsTileRiver(tileCoordForPos.x, tileCoordForPos.y) && terrain.GetFixedObjectOnTile(tileCoordForPos.x, tileCoordForPos.y) == null)))
						{
							closestPointOnPath = vector;
							closestDirXZOnPath = Vector2.Lerp(Points[j].DirXZ, Points[j + 1].DirXZ, t);
							closestPathIndex = (float)j + t;
							closestDistSq = distSqFromSegmentToPoint;
							result = true;
						}
					}
				}
			}
		}
		return result;
	}

	public bool GetEarliestIntersectionOfLineWithPath(GameTerrain terrain, Vector2 lineStartXZ, Vector2 lineEndXZ, out Vector3 intersection)
	{
		intersection = MathUtil.ToXZY(lineEndXZ, 0f);
		float num = 1f;
		for (int i = 0; i < Points.Count - 1; i++)
		{
			Vector3 pos = Points[i].Pos;
			Vector3 pos2 = Points[i + 1].Pos;
			if (MathUtil.GetLineIntersection(MathUtil.ToXZ(pos), MathUtil.ToXZ(pos2), lineStartXZ, lineEndXZ, out var a, out var b) && a >= 0f && a <= 1f && b >= 0f && b < num)
			{
				num = b;
				intersection = Vector2.Lerp(pos, pos2, a);
			}
		}
		if (num >= 1f)
		{
			float closestDistSq = float.MaxValue;
			float closestPathIndex = 0f;
			Vector3 closestPointOnPath = Vector3.zero;
			Vector2 closestDirXZOnPath = Vector2.zero;
			if (GetClosestPointOnPathToPos(terrain, lineEndXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex))
			{
				intersection.y = closestPointOnPath.y;
			}
		}
		return num < 1f;
	}

	public void DrawPath()
	{
		float num = ((PathEditor.Current != null && PathEditor.Current.Path != this) ? 0.5f : 1f);
		GameTerrain instance = GameTerrain.Instance;
		DebugGraphics.StartDrawLines(Matrix4x4.identity);
		if (Points != null)
		{
			for (int i = 0; i < Points.Count - 1; i++)
			{
				Vector3 start = Points[i].Pos + Vector3.up * ControlPointBoxSize;
				Vector3 end = Points[i + 1].Pos + Vector3.up * ControlPointBoxSize;
				DebugGraphics.DrawLine(start, end, Color.blue * num);
			}
		}
		if (ControlPoints != null)
		{
			for (int j = 0; j < ControlPoints.Count; j++)
			{
				Color color = Color.green;
				if (PathEditor.Current != null)
				{
					bool flag = PathEditor.Current.HoveredPath == this && PathEditor.Current.HoveredNodeIndex == j;
					if (PathEditor.Current.Path == this && PathEditor.Current.SelectedNodeIndex == j)
					{
						color = Color.red;
						if (flag)
						{
							color = new Color(1f, 0.75f, 0.75f);
						}
					}
					else if (flag)
					{
						color = new Color(0.75f, 1f, 0.75f);
					}
				}
				Vector2 pos = ControlPoints[j].Pos;
				Vector3 vector = MathUtil.ToXZY(pos, instance.GetTileHeightAtPos(pos.x, pos.y, ignoreIce: true, ignoreRoadCamber: true) + (IsRiver ? ControlPoints[j].RiverDepth : 0.2f) + ControlPointBoxSize);
				DebugGraphics.DrawBox(vector, new Vector3(ControlPointBoxSize, ControlPointBoxSize, ControlPointBoxSize), color * num);
				DebugGraphics.DrawLine(vector, vector + MathUtil.ToX0Y(ControlPoints[j].Dir), Color.green * num);
			}
		}
		if (LocalBounds != null)
		{
			for (int k = 0; k < LocalBounds.Count; k++)
			{
				float num2 = Points[k * 32].Pos.y;
				float num3 = Points[k * 32].Pos.y;
				for (int l = k * 32; l < Math.Min((k + 1) * 32, Points.Count - 1); l++)
				{
					num2 = Math.Min(num2, Points[l].Pos.y);
					num3 = Math.Max(num3, Points[l].Pos.y);
				}
				Vector2 vertexPosXZ = instance.GetVertexPosXZ(LocalBounds[k].min.x, LocalBounds[k].min.y);
				Vector2 vertexPosXZ2 = instance.GetVertexPosXZ(LocalBounds[k].max.x, LocalBounds[k].max.y);
				Vector3 vector2 = MathUtil.ToXZY(vertexPosXZ, num2);
				Vector3 vector3 = MathUtil.ToXZY(vertexPosXZ2, num3);
				DebugGraphics.DrawBox(Vector3.Lerp(vector2, vector3, 0.5f), (vector3 - vector2) * 0.5f, Color.cyan * num);
			}
		}
		DebugGraphics.EndDrawLines();
	}

	public void DrawPathGizmos()
	{
		GameTerrain instance = GameTerrain.Instance;
		Gizmos.color = Color.blue;
		for (int i = 0; i < Points.Count - 1; i++)
		{
			Vector3 vector = Points[i].Pos + Vector3.up * ControlPointBoxSize;
			Vector3 to = Points[i + 1].Pos + Vector3.up * ControlPointBoxSize;
			Vector3 direction = MathUtil.ToX0Y(Points[i].DirXZ);
			Gizmos.DrawSphere(vector, 0.1f);
			Gizmos.DrawLine(vector, to);
			Gizmos.DrawRay(vector, direction);
		}
		Gizmos.color = Color.green;
		for (int j = 0; j < ControlPoints.Count; j++)
		{
			Vector2 pos = ControlPoints[j].Pos;
			Vector3 vector2 = MathUtil.ToXZY(pos, instance.GetTileHeightAtPos(pos.x, pos.y, ignoreIce: true, ignoreRoadCamber: true) + (IsRiver ? ControlPoints[j].RiverDepth : 0.2f) + ControlPointBoxSize);
			Gizmos.DrawCube(vector2, new Vector3(ControlPointBoxSize, ControlPointBoxSize, ControlPointBoxSize));
			Gizmos.DrawRay(vector2, MathUtil.ToX0Y(ControlPoints[j].Dir));
		}
		Gizmos.color = Color.cyan;
		for (int k = 0; k < LocalBounds.Count; k++)
		{
			float num = Points[k * 32].Pos.y;
			float num2 = Points[k * 32].Pos.y;
			for (int l = k * 32; l < Math.Min((k + 1) * 32, Points.Count - 1); l++)
			{
				num = Math.Min(num, Points[l].Pos.y);
				num2 = Math.Max(num2, Points[l].Pos.y);
			}
			Vector2 vertexPosXZ = instance.GetVertexPosXZ(LocalBounds[k].min.x, LocalBounds[k].min.y);
			Vector2 vertexPosXZ2 = instance.GetVertexPosXZ(LocalBounds[k].max.x, LocalBounds[k].max.y);
			Vector3 vector3 = MathUtil.ToXZY(vertexPosXZ, num);
			Vector3 vector4 = MathUtil.ToXZY(vertexPosXZ2, num2);
			Gizmos.DrawLine(new Vector3(vector3.x, vector3.y, vector3.z), new Vector3(vector3.x, vector3.y, vector4.z));
			Gizmos.DrawLine(new Vector3(vector3.x, vector3.y, vector3.z), new Vector3(vector4.x, vector3.y, vector3.z));
			Gizmos.DrawLine(new Vector3(vector4.x, vector3.y, vector4.z), new Vector3(vector3.x, vector3.y, vector4.z));
			Gizmos.DrawLine(new Vector3(vector4.x, vector3.y, vector4.z), new Vector3(vector4.x, vector3.y, vector3.z));
			Gizmos.DrawLine(new Vector3(vector3.x, vector4.y, vector3.z), new Vector3(vector3.x, vector4.y, vector4.z));
			Gizmos.DrawLine(new Vector3(vector3.x, vector4.y, vector3.z), new Vector3(vector4.x, vector4.y, vector3.z));
			Gizmos.DrawLine(new Vector3(vector4.x, vector4.y, vector4.z), new Vector3(vector3.x, vector4.y, vector4.z));
			Gizmos.DrawLine(new Vector3(vector4.x, vector4.y, vector4.z), new Vector3(vector4.x, vector4.y, vector3.z));
			Gizmos.DrawLine(new Vector3(vector3.x, vector3.y, vector3.z), new Vector3(vector3.x, vector4.y, vector3.z));
			Gizmos.DrawLine(new Vector3(vector4.x, vector3.y, vector3.z), new Vector3(vector4.x, vector4.y, vector3.z));
			Gizmos.DrawLine(new Vector3(vector3.x, vector3.y, vector4.z), new Vector3(vector3.x, vector4.y, vector4.z));
			Gizmos.DrawLine(new Vector3(vector4.x, vector3.y, vector4.z), new Vector3(vector4.x, vector4.y, vector4.z));
		}
	}

	public void BuildPoints(GameTerrain terrain)
	{
		Points.Clear();
		for (int i = 0; i < ControlPoints.Count - 1; i++)
		{
			ControlPoint cur = ControlPoints[i];
			ControlPoint next = ControlPoints[i + 1];
			float len;
			CubicBezier2D bezierFromPathPoints = GameTerrain.GetBezierFromPathPoints(cur, next, out len);
			int num = Mathf.CeilToInt(len * SampleDensity);
			int num2 = num + ((i >= ControlPoints.Count - 2) ? 1 : 0);
			_ = Points.Count;
			for (int j = 0; j < num2; j++)
			{
				float num3 = (float)j / (float)num;
				Vector2 v = bezierFromPathPoints.EvaluatePos(num3);
				float num4 = GameTerrain.Instance.GetTileHeightAtPos(v.x, v.y, ignoreIce: true, ignoreRoadCamber: true) + (IsRiver ? Mathf.Lerp(cur.RiverDepth, next.RiverDepth, num3) : 0f);
				if (IsRiver && Points.Count > 0)
				{
					num4 = Math.Min(num4, Points[Points.Count - 1].Pos.y);
				}
				TerrainPathPoint item = new TerrainPathPoint
				{
					Pos = MathUtil.ToXZY(v, num4),
					DirXZ = bezierFromPathPoints.EvaluateDir(num3),
					Width = Mathf.Lerp(cur.Width, next.Width, num3),
					ControlPointIndex = num3 + (float)i
				};
				Points.Add(item);
			}
		}
		BuildBoundingBoxes(terrain);
	}

	public void Rebuild(GameTerrain terrain)
	{
		BuildPoints(terrain);
		UnityDeletePath();
		UnityInitPath();
	}

	public void UnityInitPath()
	{
		GameTerrain instance = GameTerrain.Instance;
		UnityPathObjects = new List<MeshCollider>();
		if (IsRiver)
		{
			float num = 0f;
			float num2 = 0f;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < Points.Count; i += 32)
			{
				int num3 = Math.Min(Points.Count - i - 1, 32);
				if (num3 <= 0)
				{
					break;
				}
				Vector3[] array = new Vector3[(num3 + 1) * 2];
				Vector2[] array2 = new Vector2[(num3 + 1) * 2];
				Vector2[] array3 = new Vector2[(num3 + 1) * 2];
				int[] array4 = new int[num3 * 6];
				for (int j = 0; j <= num3; j++)
				{
					Vector3 pos = Points[i + j].Pos;
					Vector2 dirXZ = Points[i + j].DirXZ;
					float num4 = Points[i + j].Width * 0.5f;
					float num5 = 0f;
					if (i + j + 1 < Points.Count)
					{
						Vector3 pos2 = Points[i + j + 1].Pos;
						float num6 = (pos.y - pos2.y) / MathUtil.ToXZ(pos2 - pos).magnitude;
						num5 = Math.Max(0f, num6 * 0.5f - 0.5f);
					}
					array[j * 2] = pos + MathUtil.ToXZY(MathUtil.RightNormal(dirXZ) * num4 + dirXZ * num5, GameTerrain.RiverHeightOffset);
					array[j * 2 + 1] = pos + MathUtil.ToXZY(-MathUtil.RightNormal(dirXZ) * num4 + dirXZ * num5, GameTerrain.RiverHeightOffset);
					if (j > 0)
					{
						float num7 = (vector.y - pos.y) / MathUtil.ToXZ(pos - vector).magnitude;
						float magnitude = (pos - vector).magnitude;
						num += magnitude / (1f + num7 * 5f);
						num2 += magnitude;
					}
					array2[j * 2] = new Vector2(0f - num4, num2);
					array2[j * 2 + 1] = new Vector2(num4, num2);
					array3[j * 2] = new Vector2(-1f, num);
					array3[j * 2 + 1] = new Vector2(1f, num);
					vector = pos;
				}
				for (int k = 0; k < num3; k++)
				{
					array4[k * 6] = k * 2;
					array4[k * 6 + 1] = k * 2 + 1;
					array4[k * 6 + 2] = k * 2 + 2;
					array4[k * 6 + 3] = k * 2 + 2;
					array4[k * 6 + 4] = k * 2 + 1;
					array4[k * 6 + 5] = k * 2 + 3;
				}
				GameObject gameObject = new GameObject();
				gameObject.name = (string.IsNullOrEmpty(Name) ? "River" : Name);
				gameObject.transform.parent = instance.UnityTerrainObj.transform;
				Mesh mesh = new Mesh();
				gameObject.AddComponent<MeshFilter>().mesh = mesh;
				mesh.vertices = array;
				mesh.uv = array2;
				mesh.uv2 = array3;
				mesh.triangles = array4;
				gameObject.AddComponent<MeshRenderer>().material = instance.UnityWaterMaterial;
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.sharedMesh = mesh;
				meshCollider.enabled = Session.Instance.Weather.TemperatureInCelsius < Weather.IceCollisionTemperatureInCelsius;
				UnityPathObjects.Add(meshCollider);
			}
			return;
		}
		float num8 = 0f;
		Vector3 vector2 = ((Points.Count > 0) ? Points[0].Pos : Vector3.zero);
		for (int l = 0; l < Points.Count; l += 32)
		{
			int num9 = Math.Min(Points.Count - l - 1, 32);
			if (num9 <= 0)
			{
				break;
			}
			Vector3[] array5 = new Vector3[(num9 + 1) * 7];
			Vector3[] array6 = new Vector3[(num9 + 1) * 7];
			Vector2[] array7 = new Vector2[(num9 + 1) * 7];
			Vector2[] array8 = new Vector2[(num9 + 1) * 7];
			Vector2[] array9 = new Vector2[(num9 + 1) * 7];
			int[] array10 = new int[num9 * 6 * 6];
			for (int m = 0; m <= num9; m++)
			{
				Vector3 pos3 = Points[l + m].Pos;
				Vector2 dirXZ2 = Points[l + m].DirXZ;
				float num10 = Points[l + m].Width * 0.5f;
				for (int n = 0; n < 7; n++)
				{
					float num11 = (float)n / 6f * 2f - 1f;
					Vector3 vector3 = pos3 + MathUtil.ToXZY(MathUtil.RightNormal(dirXZ2) * num10 * num11, 0f);
					float num12 = ((!Session.Instance.Editor) ? instance.GetOriginalTileHeightAtPos(vector3.x, vector3.z) : instance.GetTileHeightAtPos(MathUtil.ToXZ(vector3), ignoreIce: true, ignoreRoadCamber: true));
					float num13 = 1f - MathUtil.Squared(Mathf.Abs(num11));
					num13 *= Mathf.Clamp01((float)(l + m) / 3f);
					num13 *= Mathf.Clamp01((float)(Points.Count - 1 - (l + m)) / 3f);
					vector3.y = num12 + num13 * 0.2f;
					array5[m * 7 + n] = vector3;
					array6[m * 7 + n] = instance.GetNormalAtPos(vector3.x, vector3.z);
					array7[m * 7 + n] = MathUtil.ToXZ(vector3) / 4f;
					array8[m * 7 + n] = new Vector2(num11, num8);
					array9[m * 7 + n] = new Vector2(num10, 1f);
				}
				num8 += MathUtil.ToXZ(pos3 - vector2).magnitude;
				vector2 = pos3;
			}
			for (int num14 = 0; num14 < num9; num14++)
			{
				for (int num15 = 0; num15 < 6; num15++)
				{
					array10[num14 * 6 * 6 + num15 * 6] = num14 * 7 + num15;
					array10[num14 * 6 * 6 + num15 * 6 + 1] = num14 * 7 + 7 + num15;
					array10[num14 * 6 * 6 + num15 * 6 + 2] = num14 * 7 + num15 + 1;
					array10[num14 * 6 * 6 + num15 * 6 + 3] = num14 * 7 + 7 + num15;
					array10[num14 * 6 * 6 + num15 * 6 + 4] = num14 * 7 + 7 + num15 + 1;
					array10[num14 * 6 * 6 + num15 * 6 + 5] = num14 * 7 + num15 + 1;
				}
			}
			GameObject gameObject2 = new GameObject();
			gameObject2.name = (string.IsNullOrEmpty(Name) ? "Road" : Name);
			gameObject2.transform.parent = instance.UnityTerrainObj.transform;
			Mesh mesh2 = new Mesh();
			gameObject2.AddComponent<MeshFilter>().mesh = mesh2;
			mesh2.vertices = array5;
			mesh2.normals = array6;
			mesh2.uv = array7;
			mesh2.uv2 = array8;
			mesh2.uv3 = array9;
			mesh2.triangles = array10;
			MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
			meshRenderer.material = GameTerrain.RoadMaterial;
			meshRenderer.material.SetFloat(ShaderHash._RoadMiddleLine, HasMiddleLine ? 1f : 0f);
			meshRenderer.material.SetFloat(ShaderHash._RoadEdgeLine, HasEdgeLine ? 1f : 0f);
			MeshCollider meshCollider2 = gameObject2.AddComponent<MeshCollider>();
			meshCollider2.sharedMesh = mesh2;
			meshCollider2.enabled = true;
			UnityPathObjects.Add(meshCollider2);
		}
	}

	public void UnityDeletePath()
	{
		if (UnityPathObjects == null)
		{
			return;
		}
		foreach (MeshCollider unityPathObject in UnityPathObjects)
		{
			UnityEngine.Object.Destroy(unityPathObject.sharedMesh);
			UnityEngine.Object.Destroy(unityPathObject.gameObject);
		}
		UnityPathObjects = null;
	}
}
