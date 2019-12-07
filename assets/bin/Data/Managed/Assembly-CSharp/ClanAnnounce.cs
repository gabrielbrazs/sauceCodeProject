using System;
using UnityEngine;

public class ClanAnnounce : LoungeAnnounce
{
	public new enum UI
	{
		WGT_ANCHOR_POINT,
		OBJ_TWEENCTRL,
		OBJ_EFFECT,
		LBL_ANNOUNCE,
		LBL_USER_NAME
	}

	public override void Play(ANNOUNCE_TYPE type, string userName, Action onComplete)
	{
		SetActive(UI.WGT_ANCHOR_POINT, is_visible: true);
		if (widget == null || tweenCtrl == null)
		{
			if (onComplete != null)
			{
				onComplete();
			}
			return;
		}
		switch (type)
		{
		case ANNOUNCE_TYPE.CREATED_PARTY:
		{
			string text3 = StringTable.Get(STRING_CATEGORY.CLAN, 0u);
			SetLabelText(UI.LBL_ANNOUNCE, text3);
			break;
		}
		case ANNOUNCE_TYPE.JOIN_LOUNGE:
		{
			string text2 = StringTable.Get(STRING_CATEGORY.CLAN, 1u);
			SetLabelText(UI.LBL_ANNOUNCE, text2);
			break;
		}
		case ANNOUNCE_TYPE.LEAVED_LOUNGE:
		{
			string text = StringTable.Get(STRING_CATEGORY.CLAN, 2u);
			SetLabelText(UI.LBL_ANNOUNCE, text);
			break;
		}
		}
		SetLabelText(UI.LBL_USER_NAME, userName);
		SetFontStyle(UI.LBL_ANNOUNCE, FontStyle.Italic);
		SetFontStyle(UI.LBL_USER_NAME, FontStyle.Italic);
		tweenCtrl.Reset();
		tweenCtrl.Play(forward: true, delegate
		{
			if (onComplete != null)
			{
				onComplete();
			}
		});
	}
}
