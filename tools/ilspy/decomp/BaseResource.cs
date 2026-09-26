using System.Collections.Generic;
using UnityEngine;

public abstract class BaseResource
{
	protected ResourceLoadingState LoadingState;

	protected string Path;

	protected bool FromStreamingAssets;

	protected AssetBundle FromAssetBundle;

	protected ResourceRequest Request;

	protected AssetBundleRequest AssetBundleRequest;

	protected byte[] Bytes;

	protected static List<BaseResource> PendingResources = new List<BaseResource>();

	protected static List<BaseResource> LoadingResources = new List<BaseResource>();

	protected static int LoadedResourcesCount = 0;

	public static List<BaseResource> WantUnloadResources = new List<BaseResource>();

	private static int MaxTexturesLoadedPerFrame = 1;

	private static GameProfiler UpdateResourcesTimer = new GameProfiler("UpdateResources");

	public static bool AreAllResourcesLoaded()
	{
		lock (LoadingResources)
		{
			return LoadingResources.Count == 0;
		}
	}

	public static void GetResourcesLoadedCount(out int loaded, out int total)
	{
		lock (LoadingResources)
		{
			total = LoadedResourcesCount + LoadingResources.Count;
			loaded = LoadedResourcesCount;
		}
	}

	public static void UpdateResources()
	{
		using (new ProfileMarker(UpdateResourcesTimer))
		{
			lock (PendingResources)
			{
				int num = 0;
				for (int i = 0; i < PendingResources.Count; i++)
				{
					BaseResource baseResource = PendingResources[i];
					if (!baseResource.FromStreamingAssets || num < MaxTexturesLoadedPerFrame)
					{
						baseResource.StartLoading();
						PendingResources.RemoveAt(i);
						i--;
						if (baseResource.FromStreamingAssets)
						{
							num++;
						}
					}
				}
			}
			lock (LoadingResources)
			{
				for (int j = 0; j < LoadingResources.Count; j++)
				{
					BaseResource baseResource2 = LoadingResources[j];
					if ((baseResource2.AssetBundleRequest != null) ? baseResource2.AssetBundleRequest.isDone : baseResource2.Request.isDone)
					{
						baseResource2.FinishLoading();
						LoadingResources.RemoveAt(j);
						j--;
						LoadedResourcesCount++;
					}
				}
			}
			for (int num2 = WantUnloadResources.Count - 1; num2 >= 0; num2--)
			{
				if (WantUnloadResources[num2].IsFinishedLoading())
				{
					WantUnloadResources[num2].UnloadResource();
					WantUnloadResources.RemoveAt(num2);
				}
			}
		}
	}

	protected abstract void StartLoading();

	protected abstract void FinishLoading();

	public abstract void UnloadResource();

	public string GetPath()
	{
		return Path;
	}

	public bool IsFinishedLoading()
	{
		return LoadingState == ResourceLoadingState.Loaded;
	}
}
