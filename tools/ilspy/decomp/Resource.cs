using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Resource<T> : BaseResource where T : UnityEngine.Object
{
	public static List<Resource<T>> AllResourcesOfType = new List<Resource<T>>();

	protected T Asset;

	public Resource(string path)
	{
		SetPath(path);
	}

	public Resource(string path, bool includeInList)
	{
		SetPath(path);
		if (includeInList)
		{
			AllResourcesOfType.Add(this);
		}
	}

	public Resource(string path, AssetBundle assetBundle)
	{
		FromAssetBundle = assetBundle;
		SetPath(path);
	}

	public Resource(string path, AssetBundle assetBundle, bool includeInList)
	{
		FromAssetBundle = assetBundle;
		SetPath(path);
		if (includeInList)
		{
			AllResourcesOfType.Add(this);
		}
	}

	public Resource(T asset)
	{
		Asset = asset;
		LoadingState = ResourceLoadingState.Loaded;
	}

	public Resource(T asset, bool includeInList)
	{
		Asset = asset;
		LoadingState = ResourceLoadingState.Loaded;
		if (includeInList)
		{
			AllResourcesOfType.Add(this);
		}
	}

	public Resource(byte[] bytes)
	{
		Texture2D texture2D = new Texture2D(2, 2)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		texture2D.LoadImage(bytes);
		Asset = texture2D as T;
		LoadingState = ResourceLoadingState.Loaded;
	}

	public static Resource<T> CreateIfFileExists(string path)
	{
		if (File.Exists(path))
		{
			Resource<T> resource = FindResourceByPath(path);
			if (resource != null)
			{
				return resource;
			}
			return new Resource<T>(path);
		}
		return null;
	}

	public void SetPath(string path)
	{
		if (LoadingState == ResourceLoadingState.Pending || LoadingState == ResourceLoadingState.Loading)
		{
			Debug.LogError("Trying to change a resource path while it is still loading");
		}
		else
		{
			if (Path == path)
			{
				return;
			}
			Path = path;
			FromStreamingAssets = path.StartsWith(GameImpl.Instance.StreamingAssetsPath) || System.IO.Path.IsPathRooted(path);
			if (FromStreamingAssets)
			{
				try
				{
					using FileStream fileStream = File.OpenRead(Path);
					byte[] array = new byte[fileStream.Length];
					fileStream.Read(array, 0, array.Length);
					Bytes = array;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
				}
			}
			if (!Util.AmIOnMainThread() || GameImpl.Instance == null || GameImpl.Instance.GetState() == GameState.None)
			{
				lock (BaseResource.PendingResources)
				{
					LoadingState = ResourceLoadingState.Pending;
					BaseResource.PendingResources.Add(this);
					return;
				}
			}
			StartLoading();
		}
	}

	protected override void StartLoading()
	{
		if (FromStreamingAssets)
		{
			Texture2D texture2D = new Texture2D(128, 128, TextureFormat.ARGB32, mipChain: false, linear: true);
			if (Bytes != null)
			{
				texture2D.wrapMode = TextureWrapMode.Clamp;
				texture2D.LoadImage(Bytes);
				Bytes = null;
			}
			Asset = texture2D as T;
			FinishLoading();
			return;
		}
		if (FromAssetBundle != null)
		{
			AssetBundleRequest = FromAssetBundle.LoadAssetAsync<T>(Path);
			lock (BaseResource.LoadingResources)
			{
				LoadingState = ResourceLoadingState.Loading;
				BaseResource.LoadingResources.Add(this);
				return;
			}
		}
		Request = Resources.LoadAsync<T>(Path);
		lock (BaseResource.LoadingResources)
		{
			LoadingState = ResourceLoadingState.Loading;
			BaseResource.LoadingResources.Add(this);
		}
	}

	protected override void FinishLoading()
	{
		if (!FromStreamingAssets)
		{
			if (AssetBundleRequest != null)
			{
				Asset = (T)AssetBundleRequest.asset;
			}
			else
			{
				Asset = (T)Request.asset;
			}
			if (Asset == null)
			{
				Debug.LogError("Load resource failed: " + Path);
			}
		}
		LoadingState = ResourceLoadingState.Loaded;
	}

	public static implicit operator T(Resource<T> resource)
	{
		return resource.Asset;
	}

	public T GetAsset()
	{
		return Asset;
	}

	public override void UnloadResource()
	{
		if (Asset != null)
		{
			Texture2D texture2D = Asset as Texture2D;
			if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
			else if (!(FromAssetBundle != null))
			{
				Resources.UnloadAsset(Asset);
			}
			Asset = null;
			LoadingState = ResourceLoadingState.Unloaded;
		}
		AllResourcesOfType.Remove(this);
	}

	public static Resource<T> FindResourceByPath(string path)
	{
		for (int num = AllResourcesOfType.Count - 1; num >= 0; num--)
		{
			if (AllResourcesOfType[num].Path == path)
			{
				return AllResourcesOfType[num];
			}
		}
		return null;
	}
}
