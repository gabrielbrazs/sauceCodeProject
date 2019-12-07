using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBManager : MonoBehaviourSingleton<FBManager>
{
	[Serializable]
	public class FriendData
	{
		[Serializable]
		public class Picture
		{
			[Serializable]
			public class PictureData
			{
				public bool is_silhouette;

				public string url;
			}

			public PictureData data;
		}

		public string id;

		public string name;

		public Picture picture;

		public override string ToString()
		{
			return $"id:{id} name:{name} pictureurl:{picture.data.url}";
		}
	}

	[Serializable]
	public class Paging
	{
		[Serializable]
		public class Cursors
		{
			public string before;

			public string after;
		}

		public Cursors cursors;

		public string next;
	}

	[Serializable]
	public class InvitableFriendInfo
	{
		public List<FriendData> data;

		public Paging paging;
	}

	[Serializable]
	public class FriendInfo
	{
		[Serializable]
		public class Summary
		{
			public int total_count;
		}

		public Summary summary;

		public List<FriendData> data;
	}

	[Serializable]
	public class AppRequestResult
	{
		public string request;

		public string to;
	}

	private const string GRAPH_API_FIELD_AFTER = "&after=";

	private const string GRAPH_API_QUERY_INVITABLE_FRIENDS = "/me/invitable_friends?fields=id,name,picture&pretty=0&limit=5000";

	private const string GRAPH_API_QUERY_FRIENDS = "/me/friends?fields=id,name,picture&pretty=0&limit=5000";

	private const string FACEBOOK_FRIEND_PLAYERPREF_KEY = "fb_friend_key";

	private Action<bool, string> OnActionCallback;

	private bool isActionExecuting;

	private const float LOGOUT_TIMEOUT = 5f;

	public bool isInitialized => FB.IsInitialized;

	public bool isLoggedIn => FB.IsLoggedIn;

	public string accessToken => AccessToken.CurrentAccessToken.TokenString;

	public InvitableFriendInfo invitableFriendInfo
	{
		get;
		set;
	}

	public FriendInfo friendInfo
	{
		get;
		set;
	}

	protected override void Awake()
	{
		if (FB.IsInitialized)
		{
			FB.ActivateApp();
		}
		else
		{
			FB.Init(delegate
			{
				FB.ActivateApp();
			});
		}
	}

	private bool CheckAndSetActionExecuting()
	{
		if (isActionExecuting)
		{
			Log.Error(LOG.SOCIAL, "isActionExecuting is currently true!");
			return false;
		}
		SetActionExecutingFlag(is_enable: true);
		return true;
	}

	private void SetActionExecutingFlag(bool is_enable)
	{
		isActionExecuting = is_enable;
		if (MonoBehaviourSingleton<UIManager>.IsValid())
		{
			MonoBehaviourSingleton<UIManager>.I.SetDisable(UIManager.DISABLE_FACTOR.PROTOCOL, is_enable);
		}
	}

	private bool CheckLogin(Action<bool> callback)
	{
		if (!FB.IsLoggedIn)
		{
			return false;
		}
		return true;
	}

	public void LoginWithReadPermission(Action<bool, string> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = callback;
			FB.LogInWithReadPermissions(new List<string>
			{
				"public_profile",
				"email",
				"user_friends"
			}, _OnActionComplete);
		}
	}

	public void LoginWithPublishPermission(Action<bool, string> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = callback;
			FB.LogInWithPublishPermissions(new List<string>
			{
				"publish_actions"
			}, _OnActionComplete);
		}
	}

	public void Logout(Action<bool, string> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = callback;
			FB.LogOut();
			StartCoroutine(CheckLogOutStatus());
		}
	}

	private IEnumerator CheckLogOutStatus()
	{
		float timecount = 0f;
		bool success = true;
		while (FB.IsLoggedIn)
		{
			timecount += Time.deltaTime;
			if (timecount < 5f)
			{
				yield return null;
				continue;
			}
			success = false;
			break;
		}
		SetActionExecutingFlag(is_enable: false);
		if (OnActionCallback != null)
		{
			OnActionCallback(success, null);
		}
	}

	public void ShareLink(string url, string contentTitle = "", string contentDescription = "", string photoURL = "", Action<bool, string> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = callback;
			try
			{
				FB.ShareLink(new Uri(url), contentTitle, contentDescription, new Uri(photoURL), _OnActionComplete);
			}
			catch
			{
				SetActionExecutingFlag(is_enable: false);
			}
		}
	}

	public void ShareFeed(string told = "", string url = "", string title = "", string caption = "", string description = "", string pictureUrl = "", string mediaSource = "", Action<bool, string> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = callback;
			try
			{
				FB.FeedShare(told, new Uri(url), title, caption, description, new Uri(pictureUrl), mediaSource, _OnActionComplete);
			}
			catch
			{
				SetActionExecutingFlag(is_enable: false);
			}
		}
	}

	public void AppInvite(Action<bool, string> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = callback;
			try
			{
				FB.Mobile.AppInvite(new Uri("https://fb.me/892708710750483"), new Uri("http://i.imgur.com/zkYlB.jpg"), _OnActionComplete);
			}
			catch (Exception)
			{
				SetActionExecutingFlag(is_enable: false);
			}
		}
	}

	public void AppRequest(string message, List<string> to, string data, string title, Action<bool, AppRequestResult> callback = null)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = delegate(bool success, string json)
			{
				AppRequestResult arg = null;
				if (success)
				{
					arg = JSONSerializer.Deserialize<AppRequestResult>(json);
				}
				callback(success, arg);
			};
			FB.AppRequest(message, to, null, null, null, data, title, _OnActionComplete);
		}
	}

	public void GetInvitableFriends(Action<bool> callback)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = delegate(bool success, string data)
			{
				if (success)
				{
					try
					{
						invitableFriendInfo = JsonUtility.FromJson<InvitableFriendInfo>(data);
						callback(obj: true);
					}
					catch (Exception)
					{
						callback(obj: false);
					}
				}
				else
				{
					callback(obj: false);
				}
			};
			FB.API("/me/invitable_friends?fields=id,name,picture&pretty=0&limit=5000", HttpMethod.GET, _OnActionComplete);
		}
	}

	public void GetFriends(Action<bool> callback)
	{
		if (CheckAndSetActionExecuting())
		{
			OnActionCallback = delegate(bool success, string data)
			{
				if (success)
				{
					try
					{
						friendInfo = JsonUtility.FromJson<FriendInfo>(data);
						callback(obj: true);
					}
					catch (Exception)
					{
						callback(obj: false);
					}
				}
				else
				{
					callback(obj: false);
				}
			};
			FB.API("/me/friends?fields=id,name,picture&pretty=0&limit=5000", HttpMethod.GET, _OnActionComplete);
		}
	}

	private void _OnActionComplete(IResult result)
	{
		SetActionExecutingFlag(is_enable: false);
		if (OnActionCallback == null)
		{
			Log.Warning(LOG.SOCIAL, "OnActionCallback is null => do nothing!");
		}
		else if (result == null)
		{
			OnActionCallback(arg1: false, null);
		}
		else if (!string.IsNullOrEmpty(result.Error))
		{
			OnActionCallback(arg1: false, result.Error);
		}
		else if (result.Cancelled)
		{
			OnActionCallback(arg1: false, result.RawResult);
		}
		else if (!string.IsNullOrEmpty(result.RawResult))
		{
			OnActionCallback(arg1: true, result.RawResult);
		}
		else
		{
			OnActionCallback(arg1: false, null);
		}
	}
}
