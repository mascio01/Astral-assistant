public class RepairAnim : AnimationGoal
{
	private TileObject Building;

	public RepairAnim()
	{
	}

	public RepairAnim(TileObject building, ActionAnim anim, float animSpeed)
		: base(anim, animSpeed)
	{
		Building = building;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Building);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.RepairAnim;
	}
}
