public class Goal_ActSkill : Goal
{
	protected override GOAL_TYPE GetGoalType()
	{
		return GOAL_TYPE.ACT_SKILL;
	}

	protected override void Activate(Brain brain)
	{
		SetStatus(STATUS.ACTIVE);
		(brain as AutoBrain).skillCtr.IsAct = true;
	}

	protected override STATUS Process(Brain brain)
	{
		return status;
	}

	protected override void Terminate(Brain brain)
	{
		(brain as AutoBrain).skillCtr.IsAct = false;
	}

	public override void HandleEvent(Brain brain, BRAIN_EVENT ev, object param)
	{
		base.HandleEvent(brain, ev, param);
		if (ev == BRAIN_EVENT.END_ACTION)
		{
			int num = (int)param;
			Character.ACTION_ID aCTION_ID = (Character.ACTION_ID)22;
			if (num == (int)aCTION_ID)
			{
				SetStatus(STATUS.COMPLETED);
			}
		}
	}
}
