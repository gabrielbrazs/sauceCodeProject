using UnityEngine;

public class Coop_Model_CharacterMoveLookAt : Coop_Model_ObjectSyncPositionBase
{
	public Vector3 moveLookAtPos = Vector3.zero;

	public Coop_Model_CharacterMoveLookAt()
	{
		base.packetType = PACKET_TYPE.CHARACTER_MOVE_LOOKAT;
	}

	public override bool IsHandleable(StageObject owner)
	{
		if (!(owner as Character).IsChangeableAction(Character.ACTION_ID.MOVE_POINT))
		{
			return false;
		}
		return base.IsHandleable(owner);
	}
}
