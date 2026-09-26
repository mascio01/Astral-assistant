using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpinionGraphBehaviour : MonoBehaviour
{
	public struct SuccessfulResponse
	{
		public RawImage UnityImage;

		public Speech Speech;

		public List<Condition> SuccessfulResponseConditions;

		public float FailPriority;

		public float PrioritySumIgnoringConditions;
	}

	private struct CalcFailPriorityPerObjectFunc : StoryManager.IPerObjectFunc
	{
		public OpinionGraphBehaviour OpinionGraphBehaviour;

		public Speech Reply;

		public void Call(BaseObject obj, MemoryParam param, ref int intParam, ref float floatParam)
		{
			OpinionGraphBehaviour.CalcFailPriority(obj, param, Reply, ref floatParam, recursing: true);
		}
	}

	private struct CalcSuccessfulResponsesPerObjectFunc : StoryManager.IPerObjectFunc
	{
		public OpinionGraphBehaviour OpinionGraphBehaviour;

		public Speech Reply;

		public float FailPriority;

		public void Call(BaseObject obj, MemoryParam param, ref int intParam, ref float floatParam)
		{
			OpinionGraphBehaviour.CalcSuccessfulResponses(obj, param, Reply, FailPriority, ref intParam, recursing: true);
		}
	}

	private Character Character;

	private Character Subject;

	private Speech Speech;

	private BaseObject SpeechObject;

	private RawImage PlotPoint;

	public List<SuccessfulResponse> SuccessfulResponses = new List<SuccessfulResponse>();

	private static List<Condition> SuccessfulResponseConditions = new List<Condition>();

	private static Vector4[] ConditionData = new Vector4[16];

	public static int ShowSuccessfulResponseIndex = -1;

	private CustomRandom ConsistentRand = new CustomRandom();

	private void Awake()
	{
		PlotPoint = base.transform.Find("PlotPoint").GetComponent<RawImage>();
	}

	public void Init(Character character, Character subject, Speech speech, BaseObject speechObject)
	{
		Character = character;
		Subject = subject;
		Speech = speech;
		SpeechObject = speechObject;
	}

	public int GetShaderConditionType(ConditionType conditionType)
	{
		return conditionType switch
		{
			ConditionType.Approves => 1, 
			ConditionType.Respects => 2, 
			ConditionType.ApprovalRespectAngleBetween => 3, 
			ConditionType.ApprovalRespectAmountBetween => 4, 
			ConditionType.ApprovalRespectAmountAtLeast => 5, 
			ConditionType.ApprovalRespectAmountLessThan => 6, 
			ConditionType.ApprovalRespectAbsoluteSum => 7, 
			ConditionType.WantJoinCommunity => 8, 
			ConditionType.WantJoinCommunityWhenThreatened => 8, 
			ConditionType.WantLeaveCommunity => 9, 
			ConditionType.WantKickFromCommunity => 10, 
			ConditionType.Approval => 11, 
			ConditionType.Respect => 12, 
			ConditionType.ApprovalRespectSum => 13, 
			_ => 0, 
		};
	}

	private bool CheckCondition(Condition cond, ref bool satisfied, List<Condition> successfulResponseConditions, Character speaker, Character target, BaseObject obj, MemoryParam param, bool any)
	{
		bool flag = false;
		if (GetShaderConditionType(cond.Type) != 0)
		{
			Character subject = cond.GetSubject<Character>(speaker, target, obj, param);
			Character character = cond.GetObject<Character>(speaker, target, obj, param);
			if (subject == speaker && character == target)
			{
				successfulResponseConditions.Add(cond);
				flag = true;
			}
			else if (any)
			{
				satisfied |= cond.IsConditionSatisfied(speaker, target, obj, param);
			}
			else
			{
				satisfied &= cond.IsConditionSatisfied(speaker, target, obj, param);
			}
		}
		else if (cond.Type == ConditionType.Or || cond.Type == ConditionType.And)
		{
			bool flag2 = cond.Type != ConditionType.Or;
			if (cond.ConditionBlocks != null)
			{
				for (int i = 0; i < cond.ConditionBlocks.Count; i++)
				{
					if (cond.ConditionBlocks[i].Obj == null)
					{
						continue;
					}
					bool satisfied2 = true;
					List<Condition> list = new List<Condition>();
					ConditionBlock obj2 = cond.ConditionBlocks[i].Obj;
					for (int j = 0; j < obj2.Conditions.Count; j++)
					{
						flag |= CheckCondition(obj2.Conditions[j], ref satisfied2, list, speaker, target, obj, param, obj2.Type == ConditionBlockType.Or);
					}
					if (cond.Type == ConditionType.Or)
					{
						flag2 = flag2 || satisfied2;
					}
					else if (cond.Type == ConditionType.And)
					{
						flag2 = flag2 && satisfied2;
					}
					if (satisfied2)
					{
						for (int k = 0; k < list.Count; k++)
						{
							successfulResponseConditions.Add(list[k]);
						}
					}
				}
			}
			if (any)
			{
				satisfied |= flag2;
			}
			else
			{
				satisfied &= flag2;
			}
		}
		else if (any)
		{
			satisfied |= cond.IsConditionSatisfied(speaker, target, obj, param);
		}
		else
		{
			satisfied &= cond.IsConditionSatisfied(speaker, target, obj, param);
		}
		return flag;
	}

	private void CalcFailPriority(BaseObject obj, MemoryParam param, Speech reply, ref float failPriority, bool recursing)
	{
		if (reply != null && !reply.SuccessfulResponse)
		{
			if (!recursing && reply.PerObjectType != SpeechPerObjectType.None)
			{
				CalcFailPriorityPerObjectFunc calcFailPriorityPerObjectFunc = default(CalcFailPriorityPerObjectFunc);
				calcFailPriorityPerObjectFunc.OpinionGraphBehaviour = this;
				calcFailPriorityPerObjectFunc.Reply = reply;
				int intParam = 0;
				StoryManager.Instance.EvaluatePerObjectType(Character, Subject, obj, param, reply, calcFailPriorityPerObjectFunc, ref intParam, ref failPriority);
			}
			else
			{
				bool visibleButDisabled;
				float val = StoryManager.CalcSpeechPriority(Character, Subject, obj, reply, param, out visibleButDisabled, forOpinionGraph: true);
				failPriority = Math.Max(failPriority, val);
			}
		}
	}

	private void CalcSuccessfulResponses(BaseObject obj, MemoryParam param, Speech reply, float failPriority, ref int numSuccessfulResponses, bool recursing)
	{
		if (reply == null || !reply.SuccessfulResponse)
		{
			return;
		}
		if (!recursing && reply.PerObjectType != SpeechPerObjectType.None)
		{
			CalcSuccessfulResponsesPerObjectFunc calcSuccessfulResponsesPerObjectFunc = default(CalcSuccessfulResponsesPerObjectFunc);
			calcSuccessfulResponsesPerObjectFunc.OpinionGraphBehaviour = this;
			calcSuccessfulResponsesPerObjectFunc.Reply = reply;
			calcSuccessfulResponsesPerObjectFunc.FailPriority = failPriority;
			float floatParam = 0f;
			StoryManager.Instance.EvaluatePerObjectType(Character, Subject, obj, param, reply, calcSuccessfulResponsesPerObjectFunc, ref numSuccessfulResponses, ref floatParam);
			return;
		}
		bool satisfied = true;
		float num = 0f;
		float num2 = 0f;
		SuccessfulResponseConditions.Clear();
		if (reply.Conditions != null)
		{
			for (int i = 0; i < reply.Conditions.Count; i++)
			{
				CheckCondition(reply.Conditions[i], ref satisfied, SuccessfulResponseConditions, Character, Subject, obj, param, any: false);
			}
		}
		bool flag = false;
		if (reply.PriorityTerms != null)
		{
			for (int j = 0; j < reply.PriorityTerms.Count; j++)
			{
				bool satisfied2 = true;
				bool flag2 = CheckCondition(reply.PriorityTerms[j], ref satisfied2, SuccessfulResponseConditions, Character, Subject, obj, param, any: false);
				if (satisfied2)
				{
					float num3 = reply.PriorityTerms[j].EvaluatePriority(Character, Subject, obj, param);
					num += num3;
					flag = flag || flag2;
					if (!flag2)
					{
						num2 += num3;
					}
				}
			}
		}
		if (satisfied && (flag || num >= failPriority))
		{
			SuccessfulResponse successfulResponse;
			if (numSuccessfulResponses >= SuccessfulResponses.Count)
			{
				successfulResponse = new SuccessfulResponse
				{
					SuccessfulResponseConditions = new List<Condition>()
				};
				GameObject gameObject = new GameObject();
				RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
				gameObject.transform.SetParent(base.gameObject.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(numSuccessfulResponses + 1);
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.zero;
				rectTransform.pivot = Vector2.zero;
				rectTransform.anchoredPosition = Vector2.zero;
				successfulResponse.UnityImage = gameObject.AddComponent<RawImage>();
				successfulResponse.UnityImage.material = new Material(Hud.OpinionGraphMat);
				successfulResponse.UnityImage.color = new Color(0f, 1f, 0f, 1f);
			}
			else
			{
				successfulResponse = SuccessfulResponses[numSuccessfulResponses];
			}
			if (ShowSuccessfulResponseIndex != -1)
			{
				successfulResponse.UnityImage.color = new Color(0f, 1f, 0f, (ShowSuccessfulResponseIndex == numSuccessfulResponses) ? 1f : 0f);
			}
			successfulResponse.UnityImage.rectTransform.sizeDelta = HudBehaviour.Instance.UnityMinimapImage.rectTransform.sizeDelta;
			successfulResponse.Speech = reply;
			successfulResponse.FailPriority = failPriority;
			successfulResponse.PrioritySumIgnoringConditions = num2;
			successfulResponse.SuccessfulResponseConditions.Clear();
			for (int k = 0; k < SuccessfulResponseConditions.Count; k++)
			{
				successfulResponse.SuccessfulResponseConditions.Add(SuccessfulResponseConditions[k]);
			}
			if (numSuccessfulResponses >= SuccessfulResponses.Count)
			{
				SuccessfulResponses.Add(successfulResponse);
			}
			else
			{
				SuccessfulResponses[numSuccessfulResponses] = successfulResponse;
			}
			numSuccessfulResponses++;
		}
	}

	public void Update()
	{
		PlotPoint.gameObject.SetActive(Character != null);
		if (Character == null)
		{
			return;
		}
		Character.CalcApprovalRating(Subject, out var approval, out var respect);
		Vector2 sizeDelta = ((RectTransform)base.transform).sizeDelta;
		PlotPoint.rectTransform.anchoredPosition = new Vector2(sizeDelta.x * Mathf.Clamp01((respect + 100f) / 200f), sizeDelta.y * Mathf.Clamp01((approval + 100f) / 200f));
		Speech speech = Speech;
		BaseObject resultObj = SpeechObject;
		MemoryParam param = default(MemoryParam);
		while (speech != null && speech.Continues != null && speech.Continues.Count > 0 && (speech.Replies == null || speech.Replies.Count == 0))
		{
			ConsistentRand.Seed = 42u;
			speech = StoryManager.Instance.GetContinue(Subject, Character, resultObj, speech, out resultObj, ref param, ConsistentRand);
		}
		int numSuccessfulResponses = 0;
		if (speech != null && speech.Replies != null)
		{
			float failPriority = 0f;
			foreach (SpeechRef reply3 in speech.Replies)
			{
				Speech reply = reply3;
				CalcFailPriority(resultObj, param, reply, ref failPriority, recursing: false);
			}
			foreach (Story currentStory in GameImpl.Instance.CurrentStories)
			{
				List<Speech> value = null;
				currentStory.ExtraRepliesTo.TryGetValue(speech.UniqueID, out value);
				if (value == null)
				{
					continue;
				}
				foreach (Speech item in value)
				{
					CalcFailPriority(resultObj, param, item, ref failPriority, recursing: false);
				}
			}
			foreach (SpeechRef reply4 in speech.Replies)
			{
				Speech reply2 = reply4;
				CalcSuccessfulResponses(resultObj, param, reply2, failPriority, ref numSuccessfulResponses, recursing: false);
			}
			foreach (Story currentStory2 in GameImpl.Instance.CurrentStories)
			{
				List<Speech> value2 = null;
				currentStory2.ExtraRepliesTo.TryGetValue(speech.UniqueID, out value2);
				if (value2 == null)
				{
					continue;
				}
				foreach (Speech item2 in value2)
				{
					CalcSuccessfulResponses(resultObj, param, item2, failPriority, ref numSuccessfulResponses, recursing: false);
				}
			}
		}
		for (int i = 0; i < SuccessfulResponses.Count; i++)
		{
			SuccessfulResponse successfulResponse = SuccessfulResponses[i];
			if (i < numSuccessfulResponses)
			{
				Material material = successfulResponse.UnityImage.material;
				int num = Math.Min(successfulResponse.SuccessfulResponseConditions.Count, ConditionData.Length);
				material.SetInt(ShaderHash._NumConditions, num);
				material.SetVector(ShaderHash._ApprovalRespect, new Vector4(approval, respect, 0f, 0f));
				for (int j = 0; j < num; j++)
				{
					Condition condition = successfulResponse.SuccessfulResponseConditions[j];
					float withMinApproval = condition.Data;
					float withMaxApproval = condition.Data2;
					switch (condition.Type)
					{
					case ConditionType.WantJoinCommunity:
						Character.GetJoinCommunityRespectThreshold(out withMinApproval, out withMaxApproval, threatened: false);
						break;
					case ConditionType.WantJoinCommunityWhenThreatened:
						Character.GetJoinCommunityRespectThreshold(out withMinApproval, out withMaxApproval, threatened: true);
						break;
					case ConditionType.WantLeaveCommunity:
						Character.GetLeaveCommunityRespectThreshold(out withMinApproval, out withMaxApproval);
						break;
					case ConditionType.WantKickFromCommunity:
						Character.GetKickFromCommunityApprovalThreshold(out withMinApproval, out withMaxApproval);
						break;
					case ConditionType.Approval:
					case ConditionType.Respect:
					case ConditionType.ApprovalRespectSum:
						withMinApproval = successfulResponse.FailPriority - successfulResponse.PrioritySumIgnoringConditions;
						withMaxApproval = condition.PriorityFactor;
						break;
					}
					ConditionData[j] = new Vector4(withMinApproval, withMaxApproval, GetShaderConditionType(condition.Type), condition.Not ? 1f : 0f);
				}
				material.SetVectorArray(ShaderHash._ConditionData, ConditionData);
			}
			else
			{
				UnityEngine.Object.Destroy(successfulResponse.UnityImage.material);
				UnityEngine.Object.Destroy(successfulResponse.UnityImage.gameObject);
			}
		}
		if (SuccessfulResponses.Count > numSuccessfulResponses)
		{
			SuccessfulResponses.RemoveRange(numSuccessfulResponses, SuccessfulResponses.Count - numSuccessfulResponses);
		}
	}
}
