using System.Collections.Generic;

public class IconGenerationQueue
{
	private List<IconGenerationRequest> Requests = new List<IconGenerationRequest>();

	public IconGenerationRequest Processing;

	public void Push(IconGenerationRequest req)
	{
		if (Processing.Equals(req))
		{
			Processing = req;
			return;
		}
		for (int i = 0; i < Requests.Count; i++)
		{
			if (Requests[i].Equals(req))
			{
				Requests[i] = req;
				return;
			}
		}
		Requests.Add(req);
	}

	public bool Pop()
	{
		if (!Processing.IsNull())
		{
			return true;
		}
		int num = GameImpl.FrameCount - 10;
		int num2 = GameImpl.FrameCount - 1;
		int num3 = -1;
		int num4 = -1;
		for (int num5 = Requests.Count - 1; num5 >= 0; num5--)
		{
			if (Requests[num5].LastRequestedFrame < num)
			{
				Requests.RemoveAt(num5);
			}
		}
		for (int i = 0; i < Requests.Count; i++)
		{
			if (Requests[i].LastRequestedFrame > num4)
			{
				num3 = i;
				if (Requests[i].LastRequestedFrame >= num2)
				{
					break;
				}
			}
		}
		if (num3 != -1)
		{
			Processing = Requests[num3];
			Requests.RemoveAt(num3);
			return true;
		}
		return false;
	}

	public void Clear()
	{
		Requests.Clear();
		Processing = default(IconGenerationRequest);
	}

	public void FinishedProcessing()
	{
		Processing = default(IconGenerationRequest);
	}
}
