using System.Collections.Generic;

public struct LogEventFeed
{
	public List<LogEvent> Feed;

	public void AddToFeed(LogEvent logEvent, int limit, bool canDeleteEvents)
	{
		if (Feed == null)
		{
			Feed = new List<LogEvent>();
		}
		while (Feed.Count >= limit)
		{
			LogEvent logEvent2 = Feed[0];
			Feed.RemoveAt(0);
			logEvent2.FeedRefCount--;
			if (logEvent2.FeedRefCount <= 0 && canDeleteEvents)
			{
				Session.Instance.LogEvents.Remove(logEvent2);
			}
		}
		Feed.Add(logEvent);
		logEvent.FeedRefCount++;
	}

	public void ClearFeed()
	{
		if (Feed == null)
		{
			return;
		}
		foreach (LogEvent item in Feed)
		{
			item.FeedRefCount--;
			if (item.FeedRefCount <= 0)
			{
				Session.Instance.LogEvents.Remove(item);
			}
		}
		Feed.Clear();
		Feed = null;
	}
}
