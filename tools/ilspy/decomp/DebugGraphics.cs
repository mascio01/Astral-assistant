using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugGraphics
{
	private class PersistentLine
	{
		public Vector3 Start;

		public Vector3 End;

		public Color Col;

		public float Time;
	}

	private static Material lineMaterial;

	private static Material lineMaterialWithZTest;

	private static Material lineMaterialAdditive;

	private static Material TexturedQuadMaterial;

	private static int _MainTex = Shader.PropertyToID("_MainTex");

	private static int MapIconTexRows = 4;

	private static int MapIconTexColumns = 2;

	private static Material MapIconMaterial;

	private static Material MinimapQuadMaterial;

	private static int _FogTex = Shader.PropertyToID("_FogTex");

	private static int _FogCol = Shader.PropertyToID("_FogCol");

	private static int _GeologicalTex = Shader.PropertyToID("_GeologicalTex");

	private static int _MineralCol = Shader.PropertyToID("_MineralCol");

	private static int _MineralThreshold = Shader.PropertyToID("_MineralThreshold");

	private static List<PersistentLine> PersistentLines = new List<PersistentLine>();

	private static float Timeout = 1f;

	private static void CreateLineMaterial()
	{
		if (!lineMaterial)
		{
			lineMaterial = new Material(Shader.Find("Custom/Line"));
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.SetInt("_SrcBlend", 5);
			lineMaterial.SetInt("_DstBlend", 10);
			lineMaterial.SetInt("_Cull", 0);
			lineMaterial.SetInt("_ZWrite", 0);
			lineMaterial.SetInt("_ZTest", 8);
		}
		if (!lineMaterialWithZTest)
		{
			lineMaterialWithZTest = new Material(Shader.Find("Custom/Line"));
			lineMaterialWithZTest.hideFlags = HideFlags.HideAndDontSave;
			lineMaterialWithZTest.SetInt("_SrcBlend", 5);
			lineMaterialWithZTest.SetInt("_DstBlend", 10);
			lineMaterialWithZTest.SetInt("_Cull", 0);
			lineMaterialWithZTest.SetInt("_ZWrite", 0);
			lineMaterialWithZTest.SetInt("_ZTest", 4);
		}
		if (!lineMaterialAdditive)
		{
			lineMaterialAdditive = new Material(Shader.Find("Custom/Line"));
			lineMaterialAdditive.hideFlags = HideFlags.HideAndDontSave;
			lineMaterialAdditive.SetInt("_SrcBlend", 1);
			lineMaterialAdditive.SetInt("_DstBlend", 1);
			lineMaterialAdditive.SetInt("_Cull", 0);
			lineMaterialAdditive.SetInt("_ZWrite", 0);
			lineMaterialAdditive.SetInt("_ZTest", 8);
		}
	}

	public static void StartDrawLines(Matrix4x4 localToWorldMatrix)
	{
		CreateLineMaterial();
		lineMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(1);
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color col)
	{
		GL.Color(col);
		GL.Vertex(start);
		GL.Vertex(end);
	}

	public static void DrawAxes(Matrix4x4 m, float length)
	{
		DrawLine(m.Translation(), m.Translation() + m.Right() * length, Color.red);
		DrawLine(m.Translation(), m.Translation() + m.Up() * length, Color.green);
		DrawLine(m.Translation(), m.Translation() + m.Forward() * length, Color.blue);
	}

	public static void DrawRectOnGround(TerrainRect rect, Color col)
	{
		if (!(rect == TerrainRect.Invalid))
		{
			GameTerrain instance = GameTerrain.Instance;
			Vector3 vector = Vector3.up * 0.1f;
			StartDrawLines(Matrix4x4.identity);
			for (int i = rect.min.x; i <= rect.max.x; i++)
			{
				DrawLine(instance.GetVertexPosSafe(i, rect.min.y) + vector, instance.GetVertexPosSafe(i + 1, rect.min.y) + vector, col);
				DrawLine(instance.GetVertexPosSafe(i, rect.max.y + 1) + vector, instance.GetVertexPosSafe(i + 1, rect.max.y + 1) + vector, col);
			}
			for (int j = rect.min.y; j <= rect.max.y; j++)
			{
				DrawLine(instance.GetVertexPosSafe(rect.min.x, j) + vector, instance.GetVertexPosSafe(rect.min.x, j + 1) + vector, col);
				DrawLine(instance.GetVertexPosSafe(rect.max.x + 1, j) + vector, instance.GetVertexPosSafe(rect.max.x + 1, j + 1) + vector, col);
			}
			EndDrawLines();
		}
	}

	public static void DrawBox(Vector3 centre, Vector3 extents, Color col)
	{
		DrawLine(centre + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z), centre + new Vector3(0f - extents.x, 0f - extents.y, extents.z), col);
		DrawLine(centre + new Vector3(0f - extents.x, 0f - extents.y, extents.z), centre + new Vector3(extents.x, 0f - extents.y, extents.z), col);
		DrawLine(centre + new Vector3(extents.x, 0f - extents.y, extents.z), centre + new Vector3(extents.x, 0f - extents.y, 0f - extents.z), col);
		DrawLine(centre + new Vector3(extents.x, 0f - extents.y, 0f - extents.z), centre + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z), col);
		DrawLine(centre + new Vector3(0f - extents.x, extents.y, 0f - extents.z), centre + new Vector3(0f - extents.x, extents.y, extents.z), col);
		DrawLine(centre + new Vector3(0f - extents.x, extents.y, extents.z), centre + new Vector3(extents.x, extents.y, extents.z), col);
		DrawLine(centre + new Vector3(extents.x, extents.y, extents.z), centre + new Vector3(extents.x, extents.y, 0f - extents.z), col);
		DrawLine(centre + new Vector3(extents.x, extents.y, 0f - extents.z), centre + new Vector3(0f - extents.x, extents.y, 0f - extents.z), col);
		DrawLine(centre + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z), centre + new Vector3(0f - extents.x, extents.y, 0f - extents.z), col);
		DrawLine(centre + new Vector3(0f - extents.x, 0f - extents.y, extents.z), centre + new Vector3(0f - extents.x, extents.y, extents.z), col);
		DrawLine(centre + new Vector3(extents.x, 0f - extents.y, extents.z), centre + new Vector3(extents.x, extents.y, extents.z), col);
		DrawLine(centre + new Vector3(extents.x, 0f - extents.y, 0f - extents.z), centre + new Vector3(extents.x, extents.y, 0f - extents.z), col);
	}

	public static void DrawCircleXZ(Vector3 centre, float radius, Color col)
	{
		for (int i = 0; i < 16; i++)
		{
			float f = (float)i / 16f * (MathF.PI * 2f);
			float f2 = (float)(i + 1) / 16f * (MathF.PI * 2f);
			Vector3 start = centre + new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * radius;
			Vector3 end = centre + new Vector3(Mathf.Cos(f2), 0f, Mathf.Sin(f2)) * radius;
			DrawLine(start, end, col);
		}
	}

	public static void EndDrawLines()
	{
		GL.End();
		GL.PopMatrix();
	}

	public static void StartDrawQuads(Matrix4x4 localToWorldMatrix, bool wantZTest, bool dotted = false)
	{
		CreateLineMaterial();
		if (wantZTest)
		{
			lineMaterialWithZTest.SetPass(dotted ? 1 : 0);
		}
		else
		{
			lineMaterial.SetPass(dotted ? 1 : 0);
		}
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
	}

	public static void StartDrawQuadsAdditive(Matrix4x4 localToWorldMatrix)
	{
		CreateLineMaterial();
		lineMaterialAdditive.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
	}

	private static void CreateTexturedQuadMaterial()
	{
		if (!TexturedQuadMaterial)
		{
			TexturedQuadMaterial = new Material(Shader.Find("Custom/TexturedQuad"));
			TexturedQuadMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public static void StartDrawTexturedQuads(Matrix4x4 localToWorldMatrix, Texture2D tex)
	{
		CreateTexturedQuadMaterial();
		TexturedQuadMaterial.mainTexture = tex;
		TexturedQuadMaterial.SetTexture(_MainTex, tex);
		TexturedQuadMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
	}

	public static void DrawQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		GL.TexCoord2(0f, 0f);
		GL.Vertex(p0);
		GL.TexCoord2(1f, 0f);
		GL.Vertex(p1);
		GL.TexCoord2(1f, 1f);
		GL.Vertex(p2);
		GL.TexCoord2(0f, 1f);
		GL.Vertex(p3);
	}

	public static void DrawQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color col)
	{
		GL.Color(col);
		GL.Vertex(p0);
		GL.Vertex(p1);
		GL.Vertex(p2);
		GL.Vertex(p3);
	}

	public static void DrawQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color col0, Color col1)
	{
		GL.Color(col0);
		GL.Vertex(p0);
		GL.Vertex(p1);
		GL.Color(col1);
		GL.Vertex(p2);
		GL.Vertex(p3);
	}

	public static void DrawThickLine(Vector3 p0, Vector3 p1, Color col, float thickness)
	{
		Vector3 vector = MathUtil.ToX0Y(MathUtil.RightNormal(MathUtil.ToXZ(MathUtil.SafeNormalize(p1 - p0, Vector3.zero))));
		GL.Color(col);
		GL.Vertex(p0 - vector * thickness * 0.5f);
		GL.Vertex(p0 + vector * thickness * 0.5f);
		GL.Vertex(p1 + vector * thickness * 0.5f);
		GL.Vertex(p1 - vector * thickness * 0.5f);
	}

	public static void DrawThickDottedLine(Vector3 start, Vector3 end, float thickness, float dotSeparation, Color col)
	{
		Vector3 v = end - start;
		float magnitude = v.magnitude;
		if (!(magnitude < 0.001f))
		{
			v /= magnitude;
			Vector3 vector = MathUtil.ToX0Y(MathUtil.RightNormal(MathUtil.ToXZ(v)));
			float x = 0.5f / dotSeparation;
			float x2 = (magnitude + 0.5f) / dotSeparation;
			GL.Color(col);
			GL.TexCoord2(x, 0f);
			GL.Vertex(start - vector * thickness * 0.5f);
			GL.TexCoord2(x, 0f);
			GL.Vertex(start + vector * thickness * 0.5f);
			GL.TexCoord2(x2, 0f);
			GL.Vertex(end + vector * thickness * 0.5f);
			GL.TexCoord2(x2, 0f);
			GL.Vertex(end - vector * thickness * 0.5f);
		}
	}

	public static void DrawRectXZ(Vector2 tl, Vector2 br, Color col)
	{
		GL.Color(col);
		GL.Vertex(new Vector3(tl.x, 0f, tl.y));
		GL.Vertex(new Vector3(br.x, 0f, tl.y));
		GL.Vertex(new Vector3(br.x, 0f, br.y));
		GL.Vertex(new Vector3(tl.x, 0f, br.y));
	}

	public static void DrawRectOutlineXZ(Vector2 tl, Vector2 br, float inner, float outer, Color col)
	{
		DrawRectXZ(new Vector2(tl.x - outer, tl.y - outer), new Vector2(br.x + outer, tl.y - inner), col);
		DrawRectXZ(new Vector2(tl.x - outer, br.y + inner), new Vector2(br.x + outer, br.y + outer), col);
		DrawRectXZ(new Vector2(tl.x - outer, tl.y - outer), new Vector2(tl.x - inner, br.y + outer), col);
		DrawRectXZ(new Vector2(br.x + inner, tl.y - outer), new Vector2(br.x + outer, br.y + outer), col);
	}

	public static void DrawDottedRectXZ(Vector2 tl, Vector2 br, float thickness, float dotSeparation, Color col)
	{
		DrawThickDottedLine(new Vector3(tl.x, 0f, tl.y), new Vector3(br.x, 0f, tl.y), thickness, dotSeparation, col);
		DrawThickDottedLine(new Vector3(tl.x, 0f, br.y), new Vector3(br.x, 0f, br.y), thickness, dotSeparation, col);
		DrawThickDottedLine(new Vector3(tl.x, 0f, tl.y), new Vector3(tl.x, 0f, br.y), thickness, dotSeparation, col);
		DrawThickDottedLine(new Vector3(br.x, 0f, tl.y), new Vector3(br.x, 0f, br.y), thickness, dotSeparation, col);
	}

	public static void EndDrawQuads()
	{
		GL.End();
		GL.PopMatrix();
	}

	public static void StartDrawMapIcons(Matrix4x4 localToWorldMatrix, Material mat)
	{
		mat.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
	}

	public static void DrawMapCharacterIcon(Vector3 pos, Color32 col, float radius, Vector3 up, Vector3 right, int iconIndex, bool selected, float angle)
	{
		int num = iconIndex % MapIconTexColumns;
		int num2 = iconIndex / MapIconTexColumns;
		Vector2 vector = new Vector2((float)num / (float)MapIconTexColumns, (float)(MapIconTexRows - 1 - num2) / (float)MapIconTexRows);
		Vector2 vector2 = vector + new Vector2(1f / (float)MapIconTexColumns, 1f / (float)MapIconTexRows);
		float z = (selected ? 1f : 0f);
		if (iconIndex == 0 || iconIndex == 7)
		{
			angle = MathF.PI - angle;
			float c = Mathf.Cos(angle);
			float s = Mathf.Sin(angle);
			up = MathUtil.RotateVectorXZ(-Vector3.forward, c, s);
			right = MathUtil.RotateVectorXZ(Vector3.right, c, s);
		}
		GL.Color(col);
		GL.TexCoord3(vector.x, vector.y, z);
		GL.Vertex(pos - right * radius - up * radius);
		GL.TexCoord3(vector2.x, vector.y, z);
		GL.Vertex(pos + right * radius - up * radius);
		GL.TexCoord3(vector2.x, vector2.y, z);
		GL.Vertex(pos + right * radius + up * radius);
		GL.TexCoord3(vector.x, vector2.y, z);
		GL.Vertex(pos - right * radius + up * radius);
	}

	private static void CreateMapIconMaterial()
	{
		if (!MapIconMaterial)
		{
			MapIconMaterial = new Material(Shader.Find("UI/MapIcon"));
			MapIconMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public static void StartDrawMapIcons(Matrix4x4 localToWorldMatrix, Texture2D tex)
	{
		CreateMapIconMaterial();
		MapIconMaterial.mainTexture = tex;
		MapIconMaterial.SetTexture(_MainTex, tex);
		MapIconMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
	}

	public static void DrawMapIcon(Vector3 pos, Color32 col, float radius, Vector3 up, Vector3 right)
	{
		GL.Color(col);
		GL.TexCoord2(0f, 0f);
		GL.Vertex(pos - right * radius - up * radius);
		GL.TexCoord2(1f, 0f);
		GL.Vertex(pos + right * radius - up * radius);
		GL.TexCoord2(1f, 1f);
		GL.Vertex(pos + right * radius + up * radius);
		GL.TexCoord2(0f, 1f);
		GL.Vertex(pos - right * radius + up * radius);
	}

	public static void EndDrawMapIcons()
	{
		GL.End();
		GL.PopMatrix();
	}

	private static void CreateMinimapQuadMaterial()
	{
		if (!MinimapQuadMaterial)
		{
			MinimapQuadMaterial = new Material(Shader.Find("Custom/Minimap"));
			MinimapQuadMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public static void DrawMinimapQuadXZ(Matrix4x4 localToWorldMatrix, Vector2 tl, Vector2 br, float y, Color fogCol, Texture2D tex, Texture2D fogOfWarTex, Texture2D geologicalTex, Color mineralCol, float mineralThreshold)
	{
		CreateMinimapQuadMaterial();
		MinimapQuadMaterial.mainTexture = tex;
		MinimapQuadMaterial.SetTexture(_FogTex, fogOfWarTex);
		MinimapQuadMaterial.SetVector(_FogCol, fogCol);
		MinimapQuadMaterial.SetTexture(_GeologicalTex, geologicalTex);
		MinimapQuadMaterial.SetVector(_MineralCol, mineralCol);
		MinimapQuadMaterial.SetFloat(_MineralThreshold, mineralThreshold);
		MinimapQuadMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex(new Vector3(tl.x, y, tl.y));
		GL.TexCoord2(0f, 1f);
		GL.Vertex(new Vector3(tl.x, y, br.y));
		GL.TexCoord2(1f, 1f);
		GL.Vertex(new Vector3(br.x, y, br.y));
		GL.TexCoord2(1f, 0f);
		GL.Vertex(new Vector3(br.x, y, tl.y));
		GL.End();
		GL.PopMatrix();
	}

	public static void DrawMinimapQuadXZWithUVs(Matrix4x4 localToWorldMatrix, Vector2 tl, Vector2 br, Vector2 uv0, Vector2 uv1, float y, Color fogCol, Texture2D tex, Texture2D fogOfWarTex, Texture2D geologicalTex, Color mineralCol, float mineralThreshold)
	{
		CreateMinimapQuadMaterial();
		MinimapQuadMaterial.mainTexture = tex;
		MinimapQuadMaterial.SetTexture(_FogTex, fogOfWarTex);
		MinimapQuadMaterial.SetVector(_FogCol, fogCol);
		MinimapQuadMaterial.SetTexture(_GeologicalTex, geologicalTex);
		MinimapQuadMaterial.SetVector(_MineralCol, mineralCol);
		MinimapQuadMaterial.SetFloat(_MineralThreshold, mineralThreshold);
		MinimapQuadMaterial.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(localToWorldMatrix);
		GL.Begin(7);
		GL.TexCoord2(uv0.x, uv0.y);
		GL.Vertex(new Vector3(tl.x, y, tl.y));
		GL.TexCoord2(uv0.x, uv1.y);
		GL.Vertex(new Vector3(tl.x, y, br.y));
		GL.TexCoord2(uv1.x, uv1.y);
		GL.Vertex(new Vector3(br.x, y, br.y));
		GL.TexCoord2(uv1.x, uv0.y);
		GL.Vertex(new Vector3(br.x, y, tl.y));
		GL.End();
		GL.PopMatrix();
	}

	public static void AddPersistentLine(Vector3 start, Vector3 end, Color col)
	{
		lock (PersistentLines)
		{
			PersistentLines.Add(new PersistentLine
			{
				Start = start,
				End = end,
				Col = col,
				Time = 0f
			});
		}
	}

	public static void DrawPersistentLines()
	{
		lock (PersistentLines)
		{
			StartDrawLines(Matrix4x4.identity);
			float deltaTime = Time.deltaTime;
			for (int num = PersistentLines.Count - 1; num >= 0; num--)
			{
				PersistentLine persistentLine = PersistentLines[num];
				float a = 1f - persistentLine.Time / Timeout;
				Color col = persistentLine.Col;
				col.a = a;
				DrawLine(persistentLine.Start, persistentLine.End, col);
				persistentLine.Time = Mathf.Min(persistentLine.Time + deltaTime, Timeout);
				if (persistentLine.Time >= Timeout)
				{
					PersistentLines.RemoveAt(num);
				}
			}
			EndDrawLines();
		}
	}
}
