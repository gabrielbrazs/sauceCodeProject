using Network;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class UserListBase<T> : GameSection where T : CharaInfo
{
	protected int nowPage;

	protected int pageNumMax;

	protected List<T> recvList;

	protected bool isInitializeSend = true;

	protected bool isInitializeSendReopen;

	public override void Initialize()
	{
		if (isInitializeSend)
		{
			StartCoroutine(DoInitialize());
		}
		else
		{
			base.Initialize();
		}
	}

	private IEnumerator DoInitialize()
	{
		bool is_recv = false;
		this.SendGetList(this.nowPage, (Action<bool>)delegate
		{
			((_003CDoInitialize_003Ec__Iterator3D)/*Error near IL_0039: stateMachine*/)._003Cis_recv_003E__0 = true;
		});
		while (!is_recv)
		{
			yield return (object)null;
		}
		this.InitializeBase();
	}

	protected void InitializeBase()
	{
		base.Initialize();
	}

	public override void InitializeReopen()
	{
		if (isInitializeSendReopen)
		{
			StartCoroutine(DoInitializeReopen());
		}
		else
		{
			base.InitializeReopen();
		}
	}

	private IEnumerator DoInitializeReopen()
	{
		bool is_recv = false;
		this.SendGetList(this.nowPage, (Action<bool>)delegate
		{
			((_003CDoInitializeReopen_003Ec__Iterator3E)/*Error near IL_0039: stateMachine*/)._003C_003Ef__this.PostSendGetListByReopen(((_003CDoInitializeReopen_003Ec__Iterator3E)/*Error near IL_0039: stateMachine*/)._003C_003Ef__this.nowPage);
			((_003CDoInitializeReopen_003Ec__Iterator3E)/*Error near IL_0039: stateMachine*/)._003Cis_recv_003E__0 = true;
		});
		while (!is_recv)
		{
			yield return (object)null;
		}
		this.isInitializeSendReopen = false;
		base.InitializeReopen();
	}

	protected virtual void SendGetList(int page, Action<bool> callback)
	{
	}

	protected virtual void PostSendGetListByReopen(int page)
	{
	}

	protected IEnumerator GetPrevPage(Action<bool> call_back)
	{
		bool wait = true;
		bool is_success = true;
		int send_page = (this.nowPage <= 0) ? (this.pageNumMax - 1) : (this.nowPage - 1);
		this.SendGetList(send_page, (Action<bool>)delegate(bool b)
		{
			((_003CGetPrevPage_003Ec__Iterator3F)/*Error near IL_0071: stateMachine*/)._003Cwait_003E__0 = false;
			((_003CGetPrevPage_003Ec__Iterator3F)/*Error near IL_0071: stateMachine*/)._003Cis_success_003E__1 = b;
		});
		while (wait)
		{
			yield return (object)null;
		}
		call_back(is_success);
	}

	protected IEnumerator GetNextPage(Action<bool> call_back)
	{
		bool wait = true;
		bool is_success = true;
		int send_page = (this.nowPage < this.pageNumMax - 1) ? (this.nowPage + 1) : 0;
		this.SendGetList(send_page, (Action<bool>)delegate(bool b)
		{
			((_003CGetNextPage_003Ec__Iterator40)/*Error near IL_0071: stateMachine*/)._003Cwait_003E__0 = false;
			((_003CGetNextPage_003Ec__Iterator40)/*Error near IL_0071: stateMachine*/)._003Cis_success_003E__1 = b;
		});
		while (wait)
		{
			yield return (object)null;
		}
		call_back(is_success);
	}

	protected virtual void OnQuery_PAGE_PREV()
	{
		GameSection.StayEvent();
		StartCoroutine(GetPrevPage(delegate(bool b)
		{
			GameSection.ResumeEvent(b, null);
		}));
	}

	protected virtual void OnQuery_PAGE_NEXT()
	{
		GameSection.StayEvent();
		StartCoroutine(GetNextPage(delegate(bool b)
		{
			GameSection.ResumeEvent(b, null);
		}));
	}
}
