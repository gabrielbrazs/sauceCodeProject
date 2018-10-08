using System;

public class ChatState_ClanTab : ChatState
{
	public override void Enter(MainChat _manager)
	{
		base.Enter(_manager);
		EndInitialize();
	}

	public override Type GetNextState()
	{
		if (m_manager == null || !base.IsInitialized)
		{
			return base.GetNextState();
		}
		return m_manager.GetTopState();
	}
}