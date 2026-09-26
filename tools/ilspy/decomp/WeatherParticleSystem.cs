using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class WeatherParticleSystem
{
	public class CameraParticleSystem
	{
		public struct Particle
		{
			public ParticleType Type;

			public bool Active;

			public float SplashAnim;

			public Vector3 Pos;

			public Vector3 Vel;

			public Quaternion Rot;

			public Quaternion AngVel;

			public Vector3 ScaleVec;

			public float Scale;

			public float Gravity;

			public float Damping;

			public float Rippliness;
		}

		public class Batch
		{
			public ParticleType Type;

			public Matrix4x4[] Matrices;

			public int Count;
		}

		public class SplashBatch
		{
			public ParticleType Type;

			public Matrix4x4[] Matrices;

			public float[] Opacity;

			public int Count;
		}

		public Particle[] Particles;

		public Batch[] Batches;

		public SplashBatch[] SplashBatches;

		public MaterialPropertyBlock MaterialProperties;

		public MaterialPropertyBlock SplashMaterialProperties;

		public int MaxCount;

		public int[] CurCount = new int[3];

		public Vector3 FocusPos;

		public Vector3 CamPos;

		public Vector3 CamDir;

		public Plane[] CamFrustum;

		public float Range;

		public float SpawnHeight;

		public bool IsPip;

		public const int MaxBatchSize = 1023;

		private static float SnowGravity = -1f;

		private static float LeafGravity = -0.1f;

		private static float LightRainGravity = -1.5f;

		private static float HeavyRainGravity = -2.5f;

		private static float SnowDamping = 0.75f;

		private static float LeafDamping = 0.75f;

		private static float LightRainDamping = 0.85f;

		private static float HeavyRainDamping = 0.95f;

		private static float SnowflakeMinScale = 0.05f;

		private static float SnowflakeMaxScale = 0.1f;

		private static float RaindropLengthLight = 10f;

		private static float RaindropLengthHeavy = 40f;

		private static float LeafMaxSpawnHeight = 3f;

		private static float BaseRippliness = 0.25f;

		private static float WindRippliness = 0.25f;

		private static float WindStrengthFactor = 2f;

		private static float SplashAnimTime = 0.25f;

		private static float SplashStartScale = 0.1f;

		private static float SplashEndScale = 5f;

		private static float LeafRestTime = 2f;

		private int[] curBatch = new int[3];

		private int[] curSplashBatch = new int[3];

		private int[] DesiredCount = new int[3];

		private int[] Spawned = new int[3];

		private int[] MaxSpawnedPerFrame = new int[3];

		private static string ParticleUpdateStr = "ParticleUpdate";

		private static string WeatherDrawStr = "WeatherDraw";

		public void Init(int maxCount, int maxSplashCount, bool isPip)
		{
			IsPip = isPip;
			MaxCount = maxCount;
			MaterialProperties = new MaterialPropertyBlock();
			SplashMaterialProperties = new MaterialPropertyBlock();
			Batches = new Batch[Math.Max(3, maxCount / 1023)];
			SplashBatches = new SplashBatch[Math.Max(3, maxSplashCount / 1023)];
			for (int i = 0; i < Batches.Length; i++)
			{
				Batches[i] = new Batch();
				Batches[i].Matrices = new Matrix4x4[1023];
			}
			for (int j = 0; j < SplashBatches.Length; j++)
			{
				SplashBatches[j] = new SplashBatch();
				SplashBatches[j].Matrices = new Matrix4x4[1023];
				SplashBatches[j].Opacity = new float[1023];
			}
			Particles = new Particle[maxCount];
			CamFrustum = new Plane[6];
			float[] array = new float[1023];
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = 1f;
			}
			MaterialProperties.SetFloatArray(ShaderHash._Opacity, array);
		}

		public void ClearParticles()
		{
			for (int i = 0; i < Particles.Length; i++)
			{
				if (Particles[i].Active)
				{
					DestroyParticle(i);
				}
			}
			for (int j = 0; j < CurCount.Length; j++)
			{
			}
		}

		public void SpawnNewParticle(int i, CustomRandom rand, ParticleType particleType)
		{
			WeatherParticleSystem instance = Instance;
			Particles[i].Type = particleType;
			Particles[i].Active = true;
			Particles[i].SplashAnim = 0f;
			Particles[i].Vel = Vector3.zero;
			Particles[i].Rot = Quaternion.Euler(rand.RandomFloat() * 360f, rand.RandomFloat() * 360f, rand.RandomFloat() * 360f);
			Particles[i].AngVel = Quaternion.Euler(rand.RandomFloat() * 10f, rand.RandomFloat() * 10f, rand.RandomFloat() * 10f);
			Particles[i].ScaleVec = Vector3.one;
			Particles[i].Scale = Mathf.Lerp(SnowflakeMinScale, SnowflakeMaxScale, rand.RandomFloat());
			if (particleType == ParticleType.Snow || particleType == ParticleType.Rain)
			{
				Particles[i].Pos = FocusPos + Vector3.up * SpawnHeight + MathUtil.ToX0Y(rand.RandomVec2() * Range);
				Particles[i].ScaleVec = new Vector3(1f, Mathf.Lerp(1f, Mathf.Lerp(RaindropLengthLight, RaindropLengthHeavy, instance.PrecipitationAmount), instance.SleetToRainTransition), 1f);
				Particles[i].Scale *= Mathf.Lerp(1f, Mathf.Lerp(0.25f, 0.5f, instance.PrecipitationAmount), instance.SnowToSleetTransition);
				Particles[i].Gravity = Mathf.Lerp(SnowGravity, Mathf.Lerp(LightRainGravity, HeavyRainGravity, instance.PrecipitationAmount), instance.SnowToSleetTransition);
				Particles[i].Damping = Mathf.Lerp(SnowDamping, Mathf.Lerp(LightRainDamping, HeavyRainDamping, instance.PrecipitationAmount), instance.SnowToSleetTransition);
				Particles[i].Rippliness = 1f - instance.SnowToSleetTransition;
			}
			else
			{
				Particles[i].Pos = FocusPos + Vector3.up * LeafMaxSpawnHeight * rand.RandomFloat() + MathUtil.ToX0Y(rand.RandomVec2() * Range);
				Particles[i].Gravity = LeafGravity;
				Particles[i].Damping = LeafDamping;
				Particles[i].Rippliness = 1f;
			}
			CurCount[(int)particleType]++;
		}

		public void DestroyParticle(int i)
		{
			Particles[i].Active = false;
			CurCount[(int)Particles[i].Type]--;
		}

		public void Update(CustomRandom rand)
		{
			using (new UnityProfileMarker(ParticleUpdateStr))
			{
				GameTerrain instance = GameTerrain.Instance;
				WeatherParticleSystem instance2 = Instance;
				float deltaTime = instance2.DeltaTime;
				for (int i = 0; i < 3; i++)
				{
					Spawned[i] = 0;
					switch ((ParticleType)i)
					{
					case ParticleType.Snow:
						DesiredCount[i] = ((instance2.SleetToRainTransition <= 0f) ? ((int)(instance2.PrecipitationAmount * (float)MaxCount)) : 0);
						break;
					case ParticleType.Rain:
						DesiredCount[i] = ((instance2.SleetToRainTransition > 0f) ? ((int)(instance2.PrecipitationAmount * (float)MaxCount)) : 0);
						break;
					case ParticleType.Leaves:
						DesiredCount[i] = (int)Mathf.Lerp((float)MaxCount / 40f, 0f, Mathf.Clamp01(instance2.PrecipitationAmount * 4f));
						break;
					}
					switch ((ParticleType)i)
					{
					case ParticleType.Snow:
					case ParticleType.Rain:
						MaxSpawnedPerFrame[i] = (int)((float)(IsPip ? 4 : 40) * instance2.PrecipitationAmount * deltaTime / (1f / 60f));
						break;
					case ParticleType.Leaves:
						MaxSpawnedPerFrame[i] = ((!IsPip) ? 1 : 0);
						break;
					}
				}
				for (int j = 0; j < curBatch.Length; j++)
				{
					curBatch[j] = -1;
				}
				for (int k = 0; k < curSplashBatch.Length; k++)
				{
					curSplashBatch[k] = -1;
				}
				for (int l = 0; l < Batches.Length; l++)
				{
					Batches[l].Type = ParticleType.None;
					Batches[l].Count = 0;
				}
				for (int m = 0; m < SplashBatches.Length; m++)
				{
					SplashBatches[m].Type = ParticleType.None;
					SplashBatches[m].Count = 0;
				}
				bool flag = false;
				for (int n = 0; n < Particles.Length; n++)
				{
					if (!Particles[n].Active && !flag)
					{
						flag = true;
						for (int num = 2; num >= 0; num--)
						{
							if (CurCount[num] < DesiredCount[num] && Spawned[num] < MaxSpawnedPerFrame[num])
							{
								SpawnNewParticle(n, rand, (ParticleType)num);
								Spawned[num]++;
								flag = false;
								break;
							}
						}
					}
					if (!Particles[n].Active)
					{
						continue;
					}
					ParticleType type = Particles[n].Type;
					if (Particles[n].SplashAnim > 0f)
					{
						Particles[n].SplashAnim -= deltaTime;
						if (Particles[n].SplashAnim <= 0f)
						{
							DestroyParticle(n);
							flag = false;
							continue;
						}
					}
					else
					{
						if (Particles[n].Pos.x < FocusPos.x - Range)
						{
							Particles[n].Pos.x += Range * 2f * Mathf.Ceil((FocusPos.x - Particles[n].Pos.x) / (Range * 2f));
						}
						if (Particles[n].Pos.x > FocusPos.x + Range)
						{
							Particles[n].Pos.x -= Range * 2f * Mathf.Ceil((Particles[n].Pos.x - FocusPos.x) / (Range * 2f));
						}
						if (Particles[n].Pos.z < FocusPos.z - Range)
						{
							Particles[n].Pos.z += Range * 2f * Mathf.Ceil((FocusPos.z - Particles[n].Pos.z) / (Range * 2f));
						}
						if (Particles[n].Pos.z > FocusPos.z + Range)
						{
							Particles[n].Pos.z -= Range * 2f * Mathf.Ceil((Particles[n].Pos.z - FocusPos.z) / (Range * 2f));
						}
						Vector3 pos = Particles[n].Pos;
						Vector2 v = instance2.WindStrength * WindStrengthFactor * instance2.WindDirXZ;
						if (type != ParticleType.Rain)
						{
							Vector2 vector = new Vector2(Mathf.Sin(pos.x * 1.23f + pos.y * 1.45f + pos.z * 0.32f + 61.3f), Mathf.Sin(pos.x * 0.54f + pos.y * 0.68f + pos.z * 1.31f + 32.4f));
							Vector2 vector2 = new Vector2(Mathf.Sin(pos.x * 0.97f + pos.y * 0.86f + pos.z * 1.22f + 40.1f), Mathf.Sin(pos.x * 1.52f + pos.y * 1.11f + pos.z * 0.69f + 14.9f));
							v += instance2.WindStrength * WindStrengthFactor * vector * WindRippliness * Particles[n].Rippliness + vector2 * BaseRippliness * Particles[n].Rippliness;
						}
						Particles[n].Vel += Vector3.up * Particles[n].Gravity + MathUtil.ToX0Y(v);
						Particles[n].Vel *= Particles[n].Damping;
						Particles[n].Pos += Particles[n].Vel * deltaTime;
						float num2 = ((Particles[n].Pos.y > 0f) ? instance.GetTileHeightAtPos(Particles[n].Pos.x, Particles[n].Pos.z, ignoreIce: true, ignoreRoadCamber: true) : 0f);
						if (Particles[n].Pos.y <= num2)
						{
							if (type != ParticleType.Leaves && type != ParticleType.Rain)
							{
								DestroyParticle(n);
								continue;
							}
							float t = (Particles[n].Pos.y - num2) / Math.Max(Particles[n].Pos.y - pos.y, 0.0001f);
							Particles[n].SplashAnim = ((type == ParticleType.Leaves) ? LeafRestTime : SplashAnimTime);
							Particles[n].Pos = Vector3.Lerp(pos, Particles[n].Pos, t);
							Particles[n].Pos.y = instance.GetTileHeightAtPos(Particles[n].Pos.x, Particles[n].Pos.z, ignoreIce: true, ignoreRoadCamber: true);
							if (type == ParticleType.Leaves)
							{
								Vector3 normalAtPos = instance.GetNormalAtPos(Particles[n].Pos.x, Particles[n].Pos.z);
								Matrix4x4 mat = Matrix4x4.Rotate(Particles[n].Rot);
								Particles[n].Rot = Quaternion.FromToRotation(mat.Forward(), normalAtPos) * Particles[n].Rot;
							}
						}
					}
					if (!MathUtil.IsPointInFrustum(Particles[n].Pos, CamFrustum))
					{
						continue;
					}
					if (Particles[n].SplashAnim > 0f)
					{
						float num3 = Particles[n].SplashAnim / ((type == ParticleType.Leaves) ? LeafRestTime : SplashAnimTime);
						Vector3 s = Vector3.one * ((type == ParticleType.Leaves) ? 1f : Mathf.Lerp(SplashEndScale, SplashStartScale, num3)) * Particles[n].Scale;
						Vector3 vector3 = MathUtil.ToX0Y(MathUtil.ToXZ(CamPos - Particles[n].Pos));
						float magnitude = vector3.magnitude;
						num3 *= 1f - Mathf.Clamp01((magnitude - Range * 0.5f) / (Range * 0.5f));
						if (!(num3 > 0f) || !(magnitude > 0f))
						{
							continue;
						}
						Vector3 forward = vector3 / magnitude;
						Vector3 pos2 = Particles[n].Pos;
						if (type != ParticleType.Leaves)
						{
							Particles[n].Rot = Quaternion.LookRotation(forward, Vector3.up);
							pos2.y += s.y * 0.5f;
						}
						else
						{
							pos2.y += 0.1f;
						}
						if (curSplashBatch[(int)type] == -1)
						{
							while (true)
							{
								curSplashBatch[(int)type]++;
								if (curSplashBatch[(int)type] >= SplashBatches.Length)
								{
									curSplashBatch[(int)type] = -2;
									break;
								}
								if (SplashBatches[curSplashBatch[(int)type]].Type == ParticleType.None)
								{
									SplashBatches[curSplashBatch[(int)type]].Type = type;
									break;
								}
							}
						}
						if (curSplashBatch[(int)type] >= 0)
						{
							SplashBatch splashBatch = SplashBatches[curSplashBatch[(int)type]];
							int count = splashBatch.Count;
							splashBatch.Matrices[count] = Matrix4x4.TRS(pos2, Particles[n].Rot, s);
							splashBatch.Opacity[count] = num3;
							splashBatch.Count++;
							if (splashBatch.Count >= splashBatch.Matrices.Length)
							{
								curSplashBatch[(int)type] = -1;
							}
						}
						continue;
					}
					if (type == ParticleType.Rain)
					{
						Vector3 upwards = MathUtil.SafeNormalize(Particles[n].Vel, Vector3.down);
						Vector3 forward2 = MathUtil.SafeNormalize(CamPos - Particles[n].Pos, -CamDir);
						Particles[n].Rot = Quaternion.LookRotation(forward2, upwards);
					}
					else if (deltaTime > 0f)
					{
						Particles[n].Rot = (Particles[n].AngVel * Particles[n].Rot).normalized;
					}
					if (curBatch[(int)type] == -1)
					{
						while (true)
						{
							curBatch[(int)type]++;
							if (curBatch[(int)type] >= Batches.Length)
							{
								curBatch[(int)type] = -2;
								break;
							}
							if (Batches[curBatch[(int)type]].Type == ParticleType.None)
							{
								Batches[curBatch[(int)type]].Type = type;
								break;
							}
						}
					}
					if (curBatch[(int)type] >= 0)
					{
						Batch batch = Batches[curBatch[(int)type]];
						batch.Matrices[batch.Count] = Matrix4x4.TRS(Particles[n].Pos, Particles[n].Rot, Particles[n].Scale * Particles[n].ScaleVec);
						batch.Count++;
						if (batch.Count >= batch.Matrices.Length)
						{
							curBatch[(int)type] = -1;
						}
					}
				}
			}
		}

		public void Draw(Camera cam)
		{
			using (new UnityProfileMarker(WeatherDrawStr))
			{
				WeatherParticleSystem instance = Instance;
				for (int i = 0; i < Batches.Length && Batches[i].Count != 0; i++)
				{
					switch (Batches[i].Type)
					{
					case ParticleType.Rain:
						Graphics.DrawMeshInstanced(instance.RaindropMesh, 0, instance.RaindropMaterial, Batches[i].Matrices, Batches[i].Count, MaterialProperties, ShadowCastingMode.Off, receiveShadows: false, instance.SnowflakeLayer, cam);
						break;
					case ParticleType.Snow:
						Graphics.DrawMeshInstanced(instance.SnowflakeMesh, 0, instance.SnowflakeMaterial, Batches[i].Matrices, Batches[i].Count, MaterialProperties, ShadowCastingMode.Off, receiveShadows: false, instance.SnowflakeLayer, cam);
						break;
					case ParticleType.Leaves:
						Graphics.DrawMeshInstanced(instance.LeafMesh, 0, instance.LeafMaterial, Batches[i].Matrices, Batches[i].Count, MaterialProperties, ShadowCastingMode.Off, receiveShadows: false, instance.SnowflakeLayer, cam);
						break;
					}
				}
				for (int j = 0; j < SplashBatches.Length && SplashBatches[j].Count != 0; j++)
				{
					SplashMaterialProperties.SetFloatArray(ShaderHash._Opacity, SplashBatches[j].Opacity);
					switch (SplashBatches[j].Type)
					{
					case ParticleType.Snow:
					case ParticleType.Rain:
						Graphics.DrawMeshInstanced(instance.SplashMesh, 0, instance.SplashMaterial, SplashBatches[j].Matrices, SplashBatches[j].Count, SplashMaterialProperties, ShadowCastingMode.Off, receiveShadows: false, instance.SnowflakeLayer, cam);
						break;
					case ParticleType.Leaves:
						Graphics.DrawMeshInstanced(instance.LeafMesh, 0, instance.LeafMaterial, SplashBatches[j].Matrices, SplashBatches[j].Count, SplashMaterialProperties, ShadowCastingMode.Off, receiveShadows: false, instance.SnowflakeLayer, cam);
						break;
					}
				}
			}
		}
	}

	public static WeatherParticleSystem Instance;

	public Resource<GameObject> Leaf;

	public Resource<GameObject> Snowflake;

	public Resource<GameObject> Raindrop;

	public Resource<GameObject> Splash;

	public Mesh LeafMesh;

	public Mesh SnowflakeMesh;

	public Mesh RaindropMesh;

	public Mesh SplashMesh;

	public Material LeafMaterial;

	public Material SnowflakeMaterial;

	public Material RaindropMaterial;

	public Material SplashMaterial;

	public int SnowflakeLayer;

	private Thread Thread;

	private AutoResetEvent RequestEvent = new AutoResetEvent(initialState: false);

	private AutoResetEvent ReplyEvent = new AutoResetEvent(initialState: false);

	private bool Requested;

	private bool WantQuit;

	private CameraParticleSystem GameCamParticles = new CameraParticleSystem();

	private CameraParticleSystem PipCamParticles = new CameraParticleSystem();

	private float PrecipitationAmount;

	private float SleetToRainTransition;

	private float SnowToSleetTransition;

	private float DeltaTime;

	private float WindStrength;

	private Vector2 WindDirXZ;

	private CustomRandom Rand = new CustomRandom();

	private string WeatherUpdateStr = "WeatherParticleSystemUpdate";

	private Stopwatch ThreadTimer = new Stopwatch();

	public TimeSpan ThreadCalcTime;

	public void Init()
	{
		Instance = this;
		Thread = new Thread(ThreadFunc);
		Thread.IsBackground = true;
		Thread.Priority = System.Threading.ThreadPriority.AboveNormal;
		Thread.Start();
		SnowflakeLayer = Character.DefaultLayer;
		GameCamParticles.Init(10230, 1023, isPip: false);
		PipCamParticles.Init(1023, 1023, isPip: false);
	}

	public void LoadContent()
	{
		Leaf = new Resource<GameObject>("Prefabs/SpecialEffects/Leaf");
		Snowflake = new Resource<GameObject>("Prefabs/SpecialEffects/Snowflake");
		Raindrop = new Resource<GameObject>("Prefabs/SpecialEffects/Raindrop");
		Splash = new Resource<GameObject>("Prefabs/SpecialEffects/Splash");
	}

	public void OnAllContentLoaded()
	{
		LeafMesh = Leaf.GetAsset().GetComponent<MeshFilter>().sharedMesh;
		SnowflakeMesh = Snowflake.GetAsset().GetComponent<MeshFilter>().sharedMesh;
		RaindropMesh = Raindrop.GetAsset().GetComponent<MeshFilter>().sharedMesh;
		SplashMesh = Splash.GetAsset().GetComponent<MeshFilter>().sharedMesh;
		LeafMaterial = Leaf.GetAsset().GetComponent<MeshRenderer>().sharedMaterial;
		SnowflakeMaterial = Snowflake.GetAsset().GetComponent<MeshRenderer>().sharedMaterial;
		RaindropMaterial = Raindrop.GetAsset().GetComponent<MeshRenderer>().sharedMaterial;
		SplashMaterial = Splash.GetAsset().GetComponent<MeshRenderer>().sharedMaterial;
	}

	public void Unload()
	{
		WantQuit = true;
		RequestEvent.Set();
		Thread.Join();
		Instance = null;
	}

	public void Update(float precipitationAmount, Vector2 windDirXZ, float windStrength, float snowToSleetTransition, float sleetToRainTransition)
	{
		using (new UnityProfileMarker(WeatherUpdateStr))
		{
			if (Requested)
			{
				ReplyEvent.WaitOne();
				Requested = false;
				ThreadCalcTime = ThreadTimer.Elapsed;
				GameCamParticles.Draw(HudBehaviour.Instance.UnityGameCamera);
				if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
				{
					PipCamParticles.Draw(HudBehaviour.Instance.UnityPipCamera);
				}
			}
			PrecipitationAmount = precipitationAmount;
			SnowToSleetTransition = snowToSleetTransition;
			SleetToRainTransition = sleetToRainTransition;
			DeltaTime = Time.deltaTime;
			WindDirXZ = windDirXZ;
			WindStrength = windStrength;
			GameCamParticles.FocusPos = Session.Instance.GameCamera.Focus;
			GameCamParticles.CamPos = HudBehaviour.Instance.UnityGameCamera.transform.position;
			GameCamParticles.CamDir = HudBehaviour.Instance.UnityGameCamera.transform.forward;
			GameCamParticles.Range = 32f;
			GameCamParticles.SpawnHeight = Mathf.Lerp(10f, 0f, Session.Instance.GameCamera.FlyCamTransition);
			for (int i = 0; i < 6; i++)
			{
				GameCamParticles.CamFrustum[i] = HudBehaviour.Instance.UnityGameCameraBehaviour.FrustumPlanes[i];
			}
			if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
			{
				PipCamParticles.FocusPos = Hud.Instance.Pip.GetFocusPos();
				PipCamParticles.CamPos = HudBehaviour.Instance.UnityPipCamera.transform.position + HudBehaviour.Instance.UnityPipCamera.transform.forward;
				PipCamParticles.CamDir = HudBehaviour.Instance.UnityPipCamera.transform.forward;
				PipCamParticles.Range = 4f;
				PipCamParticles.SpawnHeight = 2f;
				for (int j = 0; j < 6; j++)
				{
					PipCamParticles.CamFrustum[j] = HudBehaviour.Instance.UnityPipCameraBehaviour.FrustumPlanes[j];
				}
			}
			Requested = true;
			RequestEvent.Set();
		}
	}

	public void OnFinishedSession()
	{
		if (Requested)
		{
			ReplyEvent.WaitOne();
			Requested = false;
			ThreadCalcTime = ThreadTimer.Elapsed;
			GameCamParticles.ClearParticles();
			PipCamParticles.ClearParticles();
		}
	}

	public void ThreadFunc()
	{
		try
		{
			while (true)
			{
				RequestEvent.WaitOne();
				if (WantQuit)
				{
					break;
				}
				ThreadTimer.Reset();
				ThreadTimer.Start();
				GameCamParticles.Update(Rand);
				if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
				{
					PipCamParticles.Update(Rand);
				}
				else
				{
					PipCamParticles.ClearParticles();
				}
				ThreadTimer.Stop();
				ReplyEvent.Set();
			}
			return;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.Message + ex.StackTrace);
		}
		Profiler.EndThreadProfiling();
	}
}
