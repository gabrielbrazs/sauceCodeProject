using UnityEngine;

public class ConfigSound : GameSection
{
	private enum UI
	{
		SLD_BGM_VOLUME,
		SLD_SE_VOLUME,
		TGL_VOICE_EN,
		TGL_VOICE_JP,
		TGL_VOICE_MUTE
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	public override void Exit()
	{
		GameSaveData.Save();
		base.Exit();
	}

	public override void UpdateUI()
	{
		SetProgressInt(UI.SLD_BGM_VOLUME, Mathf.RoundToInt(GameSaveData.instance.volumeBGM * 100f), 0, 100, OnChangeBGMVolume);
		SetProgressInt(UI.SLD_SE_VOLUME, Mathf.RoundToInt(GameSaveData.instance.volumeSE * 100f), 0, 100, OnChangeSEVolume);
		SetToggle(UI.TGL_VOICE_EN, GameSaveData.instance.voiceOption == 0);
		SetToggle(UI.TGL_VOICE_JP, GameSaveData.instance.voiceOption == 1);
		SetToggle(UI.TGL_VOICE_MUTE, GameSaveData.instance.voiceOption == 2);
	}

	private void OnChangeBGMVolume()
	{
		GameSaveData.instance.volumeBGM = (float)GetProgressInt(UI.SLD_BGM_VOLUME) * 0.01f;
		MonoBehaviourSingleton<SoundManager>.I.UpdateConfigVolume();
	}

	private void OnChangeSEVolume()
	{
		GameSaveData.instance.volumeSE = (float)GetProgressInt(UI.SLD_SE_VOLUME) * 0.01f;
		MonoBehaviourSingleton<SoundManager>.I.UpdateConfigVolume();
	}

	private void OnQuery_VOICE_ENGLISH()
	{
		GameSaveData.instance.voiceOption = 0;
		RefreshUI();
	}

	private void OnQuery_VOICE_JAPANESE()
	{
		GameSaveData.instance.voiceOption = 1;
		RefreshUI();
	}

	private void OnQuery_VOICE_MUTE()
	{
		GameSaveData.instance.voiceOption = 2;
		RefreshUI();
	}
}
