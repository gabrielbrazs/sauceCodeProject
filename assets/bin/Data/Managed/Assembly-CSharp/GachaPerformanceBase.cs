public class GachaPerformanceBase : GameSection
{
	public enum UI
	{
		BTN_SKIP,
		OBJ_RARITY_ROOT,
		OBJ_RARITY_D,
		OBJ_RARITY_C,
		OBJ_RARITY_B,
		OBJ_RARITY_A,
		OBJ_RARITY_S,
		OBJ_RARITY_SS,
		OBJ_RARITY_SSS,
		OBJ_RARITY_LIGHT,
		OBJ_RARITY_TEXT_ROOT
	}

	private UI[] rarityAnimRoot = new UI[7]
	{
		UI.OBJ_RARITY_D,
		UI.OBJ_RARITY_C,
		UI.OBJ_RARITY_B,
		UI.OBJ_RARITY_A,
		UI.OBJ_RARITY_S,
		UI.OBJ_RARITY_SS,
		UI.OBJ_RARITY_SSS
	};

	public override void UpdateUI()
	{
		SetActive(UI.OBJ_RARITY_ROOT, is_visible: false);
	}

	protected void ShowRarity(RARITY_TYPE rarity)
	{
		if (!AnimationDirector.I.IsSkip())
		{
			SetActive(UI.OBJ_RARITY_ROOT, is_visible: true);
			UI uI = rarityAnimRoot[(int)rarity];
			int i = 0;
			for (int num = rarityAnimRoot.Length; i < num; i++)
			{
				SetActive(rarityAnimRoot[i], rarityAnimRoot[i] == uI);
			}
			ResetTween(UI.OBJ_RARITY_TEXT_ROOT);
			ResetTween(rarityAnimRoot[(int)rarity]);
			if (rarity <= RARITY_TYPE.C)
			{
				ResetTween(UI.OBJ_RARITY_LIGHT);
				PlayTween(UI.OBJ_RARITY_LIGHT, forward: true, null, is_input_block: false);
			}
			PlayTween(rarityAnimRoot[(int)rarity], forward: true, null, is_input_block: false);
			PlayTween(UI.OBJ_RARITY_TEXT_ROOT, forward: true, null, is_input_block: false);
			if (AnimationDirector.I is QuestGachaDirectorBase)
			{
				(AnimationDirector.I as QuestGachaDirectorBase).PlayUIRarityEffect(rarity, GetCtrl(UI.OBJ_RARITY_ROOT), GetCtrl(uI));
			}
			else if (AnimationDirector.I is SkillGachaDirector)
			{
				(AnimationDirector.I as SkillGachaDirector).PlayUIRarityEffect(rarity, GetCtrl(UI.OBJ_RARITY_ROOT), GetCtrl(uI));
			}
		}
	}

	protected void HideRarity()
	{
		int i = 0;
		for (int num = rarityAnimRoot.Length; i < num; i++)
		{
			SetActive(rarityAnimRoot[i], is_visible: false);
		}
		SetActive(UI.OBJ_RARITY_ROOT, is_visible: false);
	}

	protected void End()
	{
		DispatchEvent("NEXT");
	}

	protected void OnQuery_SKIP()
	{
		SetActive(UI.BTN_SKIP, is_visible: false);
		SetActive(UI.OBJ_RARITY_ROOT, is_visible: false);
		AnimationDirector.I.Skip();
	}

	protected void OnQuery_SKIPALL()
	{
		AnimationDirector.I.SkipAll();
	}

	protected void ActivateButtonSkip()
	{
		SetActive(UI.BTN_SKIP, is_visible: true);
	}
}
