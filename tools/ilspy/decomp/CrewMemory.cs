using System;

public struct CrewMemory
{
	public MemoryPrototype Prototype;

	public CrewMemberSettings Actor;

	public CrewMemberSettings Object;

	public CrewMemberSettings ThirdParty;

	public float MoraleContribution;

	public float ApprovalContribution;

	public float RespectContribution;

	public float QuantityFactor;

	public float LastQuantity;

	public TimeSpan TimeAgo;

	public bool FakeNews;
}
