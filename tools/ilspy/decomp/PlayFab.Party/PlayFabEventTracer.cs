using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.EventsModels;
using PlayFab.Internal;
using UnityEngine;

namespace PlayFab.Party;

internal sealed class PlayFabEventTracer : SingletonMonoBehaviour<PlayFabEventTracer>
{
	private Guid gameSessionID;

	private Queue<EventContents> eventsRequests = new Queue<EventContents>();

	private Queue<EventContents> eventsPending = new Queue<EventContents>();

	private EntityKey entityKey = new EntityKey();

	private const string eventNamespace = "playfab.party";

	private const float delayBetweenEntityLoggedIn = 5f;

	private const int maxBatchSizeInEvents = 10;

	private long lastErrorTimeInMillisecond = GetCurrentTimeInMilliseconds();

	private int retryCount;

	private PlayFabEventsInstanceAPI eventApi;

	private PlayFabEventTracer()
	{
		eventApi = new PlayFabEventsInstanceAPI(PlayFabSettings.staticPlayer);
	}

	private void SetCommonTelemetryProperties(Dictionary<string, object> payload)
	{
		payload["OSName"] = SystemInfo.operatingSystem;
		payload["DeviceMake"] = SystemInfo.deviceName;
		payload["DeviceModel"] = SystemInfo.deviceModel;
		payload["Platform"] = Application.platform;
		payload["AppName"] = Application.productName;
		payload["AppVersion"] = Application.version;
	}

	private static long GetCurrentTimeInMilliseconds()
	{
		return DateTime.UtcNow.Ticks / 10000;
	}

	public void OnPlayFabMultiPlayerManagerInitialize()
	{
		gameSessionID = Guid.NewGuid();
		EventContents eventContents = new EventContents();
		eventContents.Name = "unity_client_initialization_completed";
		eventContents.EventNamespace = "playfab.party";
		eventContents.Entity = entityKey;
		eventContents.OriginalTimestamp = DateTime.UtcNow;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		SetCommonTelemetryProperties(dictionary);
		dictionary["ClientInstanceId"] = gameSessionID;
		dictionary["PartyVersion"] = Version.PartyNativeVersion;
		dictionary["PartyUnityVersion"] = Version.PartyUnityVersion;
		dictionary["UnityVersion"] = Application.unityVersion;
		eventContents.Payload = dictionary;
		if (entityKey.Id == null)
		{
			eventsPending.Enqueue(eventContents);
			StartCoroutine(WaitUntilEntityLoggedIn(5f));
		}
		else
		{
			eventsRequests.Enqueue(eventContents);
		}
	}

	private IEnumerator WaitUntilEntityLoggedIn(float secondsBetweenWait)
	{
		WaitForSeconds delay = new WaitForSeconds(secondsBetweenWait);
		while (entityKey.Id == null)
		{
			if (PlayFabAuthenticationAPI.IsEntityLoggedIn())
			{
				entityKey.Id = PlayFabSettings.staticPlayer.EntityId;
				entityKey.Type = PlayFabSettings.staticPlayer.EntityType;
				break;
			}
			yield return delay;
		}
	}

	public void DoWork()
	{
		if (!PlayFabSettings.staticPlayer.IsClientLoggedIn())
		{
			return;
		}
		while (eventsPending.Count > 0)
		{
			if (entityKey.Id == null)
			{
				return;
			}
			EventContents eventContents = eventsPending.Dequeue();
			eventContents.Entity = entityKey;
			eventsRequests.Enqueue(eventContents);
		}
		if (GetCurrentTimeInMilliseconds() > lastErrorTimeInMillisecond + retryCount * 1000 && eventsRequests.Count > 0)
		{
			WriteEventsRequest writeEventsRequest = new WriteEventsRequest();
			writeEventsRequest.Events = new List<EventContents>();
			while (eventsRequests.Count > 0 && writeEventsRequest.Events.Count < 10)
			{
				EventContents item = eventsRequests.Dequeue();
				writeEventsRequest.Events.Add(item);
			}
			if (writeEventsRequest.Events.Count > 0)
			{
				eventApi.WriteTelemetryEvents(writeEventsRequest, EventSentSuccessfulCallback, EventSentErrorCallback);
			}
		}
	}

	private void EventSentSuccessfulCallback(WriteEventsResponse response)
	{
		retryCount = 0;
	}

	private void EventSentErrorCallback(PlayFabError response)
	{
		Debug.LogWarning("Failed to send session data. Error: " + response.GenerateErrorReport());
		if (response.Error == PlayFabErrorCode.APIClientRequestRateLimitExceeded)
		{
			lastErrorTimeInMillisecond = GetCurrentTimeInMilliseconds();
			retryCount++;
		}
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
	}

	public void OnDestroy()
	{
	}
}
