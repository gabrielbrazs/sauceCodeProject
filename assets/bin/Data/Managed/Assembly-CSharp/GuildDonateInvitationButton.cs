public class GuildDonateInvitationButton : UIBehaviour
{
	private enum UI
	{
		OBJ_TWEEN
	}

	protected override void OnOpen()
	{
		PlayTween(UI.OBJ_TWEEN, forward: true, null, is_input_block: false);
		base.OnOpen();
	}
}
