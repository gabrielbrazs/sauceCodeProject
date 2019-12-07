using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
	private DataCache cache;

	private List<DataLoadRequest> requestList = new List<DataLoadRequest>();

	private MultiThreadTaskRunner taskRunner;

	private const int DL_MAX = 5;

	private int downloadCount;

	private void Awake()
	{
		taskRunner = new MultiThreadTaskRunner();
	}

	private void OnDestroy()
	{
		if (taskRunner != null)
		{
			taskRunner.DestroyThread();
			taskRunner = null;
		}
	}

	public bool IsLoading(string name)
	{
		return requestList.Find((DataLoadRequest o) => o.name == name) != null;
	}

	public bool IsLoading()
	{
		return 0 < requestList.Count;
	}

	public void SetCache(DataCache cache)
	{
		this.cache = cache;
	}

	public void Request(DataLoadRequest req)
	{
		StartCoroutine(Load(req, req.downloadOnly, useQueue: false));
	}

	public void Request(List<DataLoadRequest> reqs)
	{
		taskRunner.CreateThread();
		requestList.AddRange(reqs);
		reqs.ForEach(delegate(DataLoadRequest o)
		{
			StartCoroutine(Load(o, o.downloadOnly, useQueue: true));
		});
	}

	public void RequestManifest(DataLoadRequest req)
	{
		StartCoroutine(Load(req, req.downloadOnly, useQueue: false, forceDownload: true));
	}

	private void RemoveQueue(DataLoadRequest req)
	{
		requestList.Remove(req);
		if (0 >= requestList.Count)
		{
			taskRunner.DestroyThread();
		}
	}

	private IEnumerator Load(DataLoadRequest req, bool downloadOnly, bool useQueue, bool forceDownload = false)
	{
		int i = 0;
		for (int len = req.depReqs.Count; i < len; i++)
		{
			while (!req.depReqs[i].isCompleted)
			{
				yield return null;
			}
		}
		DataTableLoadError error = DataTableLoadError.None;
		byte[] bytes = null;
		if (!cache.IsCached(req) | forceDownload)
		{
			IEnumerator download = Download(req, delegate(byte[] b)
			{
				bytes = b;
			}, delegate(DataTableLoadError e)
			{
				error = e;
			});
			while (download.MoveNext())
			{
				yield return download.Current;
			}
			if (error != 0)
			{
				if (useQueue)
				{
					RemoveQueue(req);
				}
				if (req.enableLoadBinaryData)
				{
					req.enableLoadBinaryData = false;
					Request(req);
				}
				else
				{
					req.OnError(error);
				}
				yield break;
			}
		}
		if (downloadOnly)
		{
			bool wait = true;
			ThreadPoolWrapper.QueueUserWorkItem(delegate
			{
				try
				{
					if (bytes != null)
					{
						error = Save(req, bytes);
					}
				}
				catch (Exception)
				{
					error = DataTableLoadError.FileReadError;
				}
				finally
				{
					wait = false;
				}
			});
			while (wait)
			{
				yield return null;
			}
			if (error == DataTableLoadError.None)
			{
				req.OnComplete();
			}
			else
			{
				req.OnError(error);
			}
			if (useQueue)
			{
				RemoveQueue(req);
			}
			yield break;
		}
		Stopwatch sw = Stopwatch.StartNew();
		IEnumerator loading;
		if (req.enableLoadBinaryData)
		{
			loading = LoadCompressedBinary(req, bytes, delegate(DataTableLoadError e)
			{
				error = e;
			}, useQueue);
			while (loading.MoveNext())
			{
				yield return loading.Current;
			}
			if (error != 0)
			{
				if (useQueue)
				{
					requestList.Remove(req);
				}
				req.enableLoadBinaryData = false;
				Request(req);
				yield break;
			}
		}
		else
		{
			loading = LoadCompressedTextWithSignature(req, bytes, delegate(DataTableLoadError e)
			{
				error = e;
			}, useQueue);
		}
		while (loading.MoveNext())
		{
			yield return loading.Current;
		}
		sw.Stop();
		if (error != 0)
		{
			req.OnError(error);
		}
		else
		{
			req.OnComplete();
		}
		if (useQueue)
		{
			RemoveQueue(req);
		}
	}

	private IEnumerator LoadCompressedTextWithSignature(DataLoadRequest req, byte[] bytes, Action<DataTableLoadError> onEnd, bool useQueue)
	{
		bool wait = true;
		DataTableLoadError error = DataTableLoadError.None;
		Action act = delegate
		{
			try
			{
				if (bytes != null)
				{
					error = Save(req, bytes);
				}
				else
				{
					bytes = cache.Load(req);
				}
				if (bytes != null)
				{
					if (Verify(req, bytes))
					{
						error = ProcessCompressedText(req, bytes);
					}
					else
					{
						error = DataTableLoadError.VerifyError;
					}
				}
				else
				{
					error = DataTableLoadError.FileReadError;
				}
			}
			catch (Exception)
			{
				error = DataTableLoadError.FileReadError;
			}
			finally
			{
				wait = false;
			}
		};
		if (useQueue)
		{
			taskRunner.Add(req.name, act);
		}
		else
		{
			ThreadPoolWrapper.QueueUserWorkItem(delegate
			{
				act();
			});
		}
		while (wait)
		{
			yield return null;
		}
		onEnd(error);
	}

	private IEnumerator LoadCompressedBinary(DataLoadRequest req, byte[] bytes, Action<DataTableLoadError> onEnd, bool useQueue)
	{
		bool wait = true;
		DataTableLoadError error = DataTableLoadError.None;
		Action act = delegate
		{
			try
			{
				if (bytes != null)
				{
					error = Save(req, bytes);
				}
				else
				{
					bytes = cache.Load(req);
				}
				if (bytes != null)
				{
					if (Verify(req, bytes))
					{
						error = ProcessCompressedBinary(req, bytes);
					}
					else
					{
						error = DataTableLoadError.VerifyError;
					}
				}
				else
				{
					error = DataTableLoadError.FileReadError;
				}
			}
			catch (Exception)
			{
				error = DataTableLoadError.FileReadError;
			}
			finally
			{
				wait = false;
			}
		};
		if (useQueue)
		{
			taskRunner.Add(req.name, act);
		}
		else
		{
			ThreadPoolWrapper.QueueUserWorkItem(delegate
			{
				act();
			});
		}
		while (wait)
		{
			yield return null;
		}
		onEnd(error);
	}

	private bool Verify(DataLoadRequest req, byte[] bytes)
	{
		bool flag = false;
		using (MemoryStream memoryStream = new MemoryStream(bytes))
		{
			int num = 256;
			byte[] array = new byte[num];
			memoryStream.Read(array, 0, num);
			try
			{
				flag = Cipher.verifyBytes(memoryStream, array);
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error(LOG.DATA_TABLE, "verify exception({0}): {1}", req.name, ex);
			}
			if (!flag)
			{
				MD5Hash mD5Hash = MD5Hash.Calc(bytes);
				return req.OnVerifyError(mD5Hash.ToString());
			}
			return flag;
		}
	}

	private DataTableLoadError ProcessCompressedText(DataLoadRequest req, byte[] bytes)
	{
		DataTableLoadError result = DataTableLoadError.None;
		try
		{
			req.processCompressedTextData(bytes);
			return result;
		}
		catch (Exception ex)
		{
			Log.Error(LOG.DATA_TABLE, "DataLoadError({0}): {1}", req.name, ex);
			return DataTableLoadError.FileReadError;
		}
	}

	private DataTableLoadError ProcessCompressedBinary(DataLoadRequest req, byte[] bytes)
	{
		DataTableLoadError result = DataTableLoadError.None;
		try
		{
			req.processCompressedBinaryData(bytes);
			return result;
		}
		catch (Exception ex)
		{
			Log.Error(LOG.DATA_TABLE, "DataLoadError({0}): {1}", req.name, ex);
			return DataTableLoadError.FileReadError;
		}
	}

	private DataTableLoadError Save(DataLoadRequest req, byte[] bytes)
	{
		DataTableLoadError result = DataTableLoadError.None;
		try
		{
			cache.Save(req, bytes);
			return result;
		}
		catch (Exception ex)
		{
			Log.Error(LOG.DATA_TABLE, "SaveError({0}): {1}", req.name, ex);
			return DataTableLoadError.FileWriteError;
		}
	}

	private IEnumerator Download(DataLoadRequest req, Action<byte[]> onComplete, Action<DataTableLoadError> onError)
	{
		while (downloadCount >= 5)
		{
			yield return null;
		}
		downloadCount++;
		string str = NetworkManager.TABLE_HOST + MonoBehaviourSingleton<ResourceManager>.I.tableIndex.ToString() + "/";
		UnityWebRequest www = new UnityWebRequest(str + req.path);
		www.downloadHandler = new DownloadHandlerBuffer();
		www.SendWebRequest();
		float progress = 0f;
		float timeOut = 15f;
		while (!www.isDone)
		{
			yield return null;
			timeOut -= Time.unscaledDeltaTime;
			if (www.downloadProgress != progress)
			{
				progress = www.downloadProgress;
				timeOut = 15f;
			}
			if (timeOut < 0f)
			{
				onError(DataTableLoadError.DownloadTimeOut);
				www.Dispose();
				downloadCount--;
				yield break;
			}
		}
		if (!string.IsNullOrEmpty(www.error))
		{
			if (www.error.Contains("404"))
			{
				onError(DataTableLoadError.AssetNotFoundError);
			}
			else
			{
				onError(DataTableLoadError.NetworkError);
			}
			www.Dispose();
			downloadCount--;
		}
		else
		{
			byte[] data = www.downloadHandler.data;
			www.Dispose();
			downloadCount--;
			onComplete(data);
		}
	}

	public void ChangePriorityTop(string tableName)
	{
		taskRunner.ChangePriorityTop(tableName);
		DataLoadRequest dataLoadRequest = requestList.Find((DataLoadRequest o) => o.name == tableName);
		if (dataLoadRequest != null && dataLoadRequest.depReqs != null)
		{
			dataLoadRequest.depReqs.ForEach(delegate(DataLoadRequest o)
			{
				taskRunner.ChangePriorityTop(o.name);
			});
		}
	}
}
