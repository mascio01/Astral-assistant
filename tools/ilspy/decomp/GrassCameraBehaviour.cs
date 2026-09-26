using System;
using System.Threading;
using UnityEngine;

public class GrassCameraBehaviour : MonoBehaviour
{
	public class EvaluateGrassPatchesTaskData : BaseTaskData
	{
		public ManualResetEvent FinishedEvent = new ManualResetEvent(initialState: true);

		public GrassCameraBehaviour Owner;

		public Vector3 CameraPosition;
	}

	private Camera Camera;

	private CameraBehaviour CameraBehaviour;

	public GrassRenderer GrassRenderer;

	private int GrassLayer;

	private MaterialPropertyBlock MaterialProperties;

	public float GrassRangeMin;

	public float GrassRangeMax = 32f;

	private EvaluateGrassPatchesTaskData TaskData = new EvaluateGrassPatchesTaskData();

	private bool TaskStarted;

	public static TaskFunc EvaluateGrassPatchesOnThreadFunc = EvaluateGrassPatchesOnThread;

	private void Awake()
	{
		Camera = GetComponent<Camera>();
		CameraBehaviour = GetComponent<CameraBehaviour>();
		GrassLayer = Character.DefaultLayer;
		MaterialProperties = new MaterialPropertyBlock();
		MaterialProperties.SetVectorArray(ShaderHash._WorldPosition, new Vector4[1023]);
	}

	public void StartGrassTask(GrassRenderer grassRenderer)
	{
		if (!(GameImpl.Instance.Settings.GrassDensity <= 0f))
		{
			_ = GameTerrain.Instance;
			GrassRenderer = grassRenderer;
			TaskData.Owner = this;
			TaskData.CameraPosition = base.transform.position;
			TaskData.FinishedEvent.Reset();
			GameImpl.Instance.UpdateThreadPool.AddTask(EvaluateGrassPatchesOnThreadFunc, null, TaskData, TaskPriority.High);
			TaskStarted = true;
		}
	}

	private static void EvaluateGrassPatchesOnThread(BaseTaskData data)
	{
		EvaluateGrassPatchesTaskData evaluateGrassPatchesTaskData = (EvaluateGrassPatchesTaskData)data;
		try
		{
			GameTerrain instance = GameTerrain.Instance;
			GrassRenderer grassRenderer = evaluateGrassPatchesTaskData.Owner.GrassRenderer;
			float grassDensity = GameImpl.Instance.Settings.GrassDensity;
			Rect a = MathUtil.CreateRectCentreExtents(MathUtil.ToXZ(evaluateGrassPatchesTaskData.Owner.CameraBehaviour.FrustumBounds.center), MathUtil.ToXZ(evaluateGrassPatchesTaskData.Owner.CameraBehaviour.FrustumBounds.extents));
			Rect rect = MathUtil.CreateRectCentreExtents(MathUtil.ToXZ(evaluateGrassPatchesTaskData.CameraPosition), new Vector2(evaluateGrassPatchesTaskData.Owner.GrassRangeMax, evaluateGrassPatchesTaskData.Owner.GrassRangeMax));
			if (a.Overlaps(rect))
			{
				Rect rect2 = MathUtil.RectIntersection(a, rect);
				TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(rect2.min);
				TerrainCoord tileCoordForPosXZ2 = instance.GetTileCoordForPosXZ(rect2.max);
				for (int i = 0; i < 8; i++)
				{
					for (int j = tileCoordForPosXZ.x / 8; j <= tileCoordForPosXZ2.x / 8; j++)
					{
						for (int k = tileCoordForPosXZ.y / 8; k <= tileCoordForPosXZ2.y / 8; k++)
						{
							int num = (int)((float)instance.GrassMap.GetMaxDensityForPatch(j, k, (GrassType)i) * grassDensity + 0.5f);
							if (num > 0)
							{
								FlatGrassPatch flatGrassPatchForDensity = grassRenderer.GetFlatGrassPatchForDensity((GrassType)i, num);
								Vector2 vertexPosXZ = instance.GetVertexPosXZ(j * 8, k * 8);
								Bounds bounds = MathUtil.CreateBoundsMinMax(MathUtil.ToX0Y(vertexPosXZ), MathUtil.ToXZY(vertexPosXZ + Vector2.one * 8f, 64f));
								if (MathUtil.TestPlanesAABB(evaluateGrassPatchesTaskData.Owner.CameraBehaviour.FrustumPlanes, bounds))
								{
									Vector4 pos = new Vector4(vertexPosXZ.x, 0f, vertexPosXZ.y, 1f);
									flatGrassPatchForDensity.AddToBatch(pos);
								}
							}
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Error in EvaluateGrassPatchesOnThread: " + ex.Message + ex.StackTrace);
		}
		evaluateGrassPatchesTaskData.FinishedEvent.Set();
	}

	private void LateUpdate()
	{
		if (!TaskStarted)
		{
			return;
		}
		if (TaskData.FinishedEvent.WaitOne())
		{
			GameTerrain instance = GameTerrain.Instance;
			MaterialProperties.SetTexture(ShaderHash._HeightMapTex, instance.HeightMapTex);
			MaterialProperties.SetTexture(ShaderHash._GrassMap, instance.GrassMap.Tex);
			MaterialProperties.SetVector(ShaderHash._TerrainHalfSize_HeightRange, new Vector4(instance.HalfSize, 64f, 0f, 0f));
			MaterialProperties.SetVector(ShaderHash._GrassRangeMinMax, new Vector4(GrassRangeMin, GrassRangeMax, 0f, 0f));
			for (int i = 0; i < 8; i++)
			{
				MaterialProperties.SetFloat(ShaderHash._GrassType, i);
				for (int j = 0; j < 5; j++)
				{
					GrassRenderer.FlatGrassPatches[i, j].DrawBatch(MaterialProperties, GrassLayer, Camera);
				}
			}
		}
		TaskStarted = false;
	}
}
