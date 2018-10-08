using UnityEngine;

public class Coop_Model_PlayerGatherGimmick : Coop_Model_ObjectSyncPositionBase
{
	public Vector3 act_pos = Vector3.zero;

	public bool act_pos_f;

	public int gimmickId;

	public Coop_Model_PlayerGatherGimmick()
	{
		base.packetType = PACKET_TYPE.PLAYER_GATHER_GIMMICK;
	}

	public override bool IsHandleable(StageObject owner)
	{
		Character character = owner as Character;
		if (!character.IsChangeableAction((Character.ACTION_ID)39))
		{
			return false;
		}
		return base.IsHandleable(owner);
	}
}
