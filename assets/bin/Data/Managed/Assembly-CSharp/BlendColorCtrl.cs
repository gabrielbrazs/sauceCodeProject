using System.Collections.Generic;
using UnityEngine;

public class BlendColorCtrl
{
	private class floatUpdater
	{
		public float nValue;

		public float tValue;

		public float speed;

		public bool isEnd;

		public void CalcSpeed(float sec)
		{
			speed = (tValue - nValue) / sec;
		}

		public void Update()
		{
			if (!isEnd)
			{
				float num = speed * Time.get_deltaTime();
				nValue += num;
				if (num > 0f)
				{
					if (nValue >= tValue)
					{
						nValue = tValue;
						isEnd = true;
					}
				}
				else if (nValue <= tValue)
				{
					nValue = tValue;
					isEnd = true;
				}
			}
		}
	}

	private class ShaderParam
	{
		public string shaderName = string.Empty;

		public string propetyName = string.Empty;

		public bool forceEndFlag;

		public bool aliveFlag;

		public bool isColor;

		public floatUpdater[] fColor = new floatUpdater[3];

		public bool isBlend;

		public floatUpdater fBlend = new floatUpdater();

		public bool isBlendEnable;

		public bool blendEnable;

		public void Init()
		{
			for (int i = 0; i < 3; i++)
			{
				fColor[i] = new floatUpdater();
			}
		}

		public ShaderSyncParam GetSyncParam()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			ShaderSyncParam shaderSyncParam = new ShaderSyncParam();
			shaderSyncParam.shaderName = shaderName;
			shaderSyncParam.propetyName = propetyName;
			shaderSyncParam.isColor = isColor;
			shaderSyncParam.color = new Color(fColor[0].tValue, fColor[1].tValue, fColor[2].tValue);
			shaderSyncParam.isBlendRate = isBlend;
			shaderSyncParam.blendRate = fBlend.tValue;
			shaderSyncParam.isBlendEnable = isBlendEnable;
			shaderSyncParam.blendEnable = blendEnable;
			return shaderSyncParam;
		}
	}

	public class ShaderSyncParam
	{
		public string shaderName;

		public string propetyName;

		public bool isColor;

		public Color color;

		public bool isBlendRate;

		public float blendRate;

		public bool isBlendEnable;

		public bool blendEnable;
	}

	public const string kDefaultShaderName = "enemy_custamaizable_blend";

	public const string kDefaultPropetyName = "_BlendColor";

	private const int kColorElementNum = 3;

	private Color cacheColor = Color.get_white();

	private List<Material> materialList = new List<Material>();

	private Dictionary<string, ShaderParam> shaderParams = new Dictionary<string, ShaderParam>();

	public void Enable(AnimEventData.EventData data, bool enable, SkinnedMeshRenderer[] renderers)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		string text = "enemy_custamaizable_blend";
		if (!data.stringArgs.IsNullOrEmpty())
		{
			text = data.stringArgs[0];
		}
		ShaderParam shaderParam;
		if (shaderParams.ContainsKey(text))
		{
			shaderParam = shaderParams[text];
		}
		else
		{
			shaderParam = new ShaderParam();
			shaderParam.Init();
			shaderParams.Add(text, shaderParam);
		}
		shaderParam.shaderName = text;
		shaderParam.isBlendEnable = true;
		shaderParam.blendEnable = enable;
		int i = 0;
		for (int num = renderers.Length; i < num; i++)
		{
			int j = 0;
			for (int num2 = renderers[i].get_materials().Length; j < num2; j++)
			{
				Material val = renderers[i].get_materials()[j];
				if (val.get_shader().get_name().Contains(text))
				{
					val.SetFloat("_BlendEnable", (!enable) ? 0f : 1f);
				}
			}
		}
	}

	public void Change(AnimEventData.EventData data, SkinnedMeshRenderer[] renderers)
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		if (!renderers.IsNullOrEmpty())
		{
			string text = "enemy_custamaizable_blend";
			string propetyName = "_BlendColor";
			if (!data.stringArgs.IsNullOrEmpty())
			{
				text = data.stringArgs[0];
				if (data.stringArgs.Length > 1)
				{
					propetyName = data.stringArgs[1];
				}
			}
			ShaderParam shaderParam;
			if (shaderParams.ContainsKey(text))
			{
				shaderParam = shaderParams[text];
			}
			else
			{
				shaderParam = new ShaderParam();
				shaderParam.Init();
				shaderParams.Add(text, shaderParam);
			}
			shaderParam.shaderName = text;
			shaderParam.propetyName = propetyName;
			for (int i = 0; i < 3; i++)
			{
				bool isEnd = false;
				float num = (float)data.intArgs[i] / 255f;
				if (num < 0f)
				{
					num = 0f;
					isEnd = true;
				}
				if (num > 1f)
				{
					num = 1f;
					isEnd = true;
				}
				shaderParam.fColor[i].isEnd = isEnd;
				shaderParam.fColor[i].tValue = num;
			}
			if (data.intArgs.Length > 3)
			{
				shaderParam.forceEndFlag = ((data.intArgs[3] != 0) ? true : false);
			}
			bool isEnd2 = false;
			float num2 = data.floatArgs[0];
			if (num2 < -1f)
			{
				num2 = -1f;
				isEnd2 = true;
			}
			if (num2 > 1f)
			{
				num2 = 1f;
				isEnd2 = true;
			}
			shaderParam.fBlend.isEnd = isEnd2;
			shaderParam.fBlend.tValue = num2;
			float num3 = data.floatArgs[1];
			shaderParam.aliveFlag = false;
			materialList.Clear();
			bool flag = true;
			if (num3 == 0f)
			{
				Color val = Color.get_white();
				int j = 0;
				for (int num4 = renderers.Length; j < num4; j++)
				{
					int k = 0;
					for (int num5 = renderers[j].get_materials().Length; k < num5; k++)
					{
						Material val2 = renderers[j].get_materials()[k];
						if (val2.get_shader().get_name().Contains(shaderParam.shaderName))
						{
							if (flag)
							{
								val = val2.GetColor(shaderParam.propetyName);
								if (!shaderParam.fColor[0].isEnd)
								{
									val.r = shaderParam.fColor[0].tValue;
								}
								if (!shaderParam.fColor[1].isEnd)
								{
									val.g = shaderParam.fColor[1].tValue;
								}
								if (!shaderParam.fColor[2].isEnd)
								{
									val.b = shaderParam.fColor[2].tValue;
								}
								flag = false;
							}
							val2.SetColor(shaderParam.propetyName, val);
							if (!shaderParam.fBlend.isEnd)
							{
								val2.SetFloat("_BlendRate", shaderParam.fBlend.tValue);
							}
						}
					}
				}
			}
			else
			{
				int l = 0;
				for (int num6 = renderers.Length; l < num6; l++)
				{
					int m = 0;
					for (int num7 = renderers[l].get_materials().Length; m < num7; m++)
					{
						Material val3 = renderers[l].get_materials()[m];
						if (val3.get_shader().get_name().Contains(shaderParam.shaderName))
						{
							if (flag)
							{
								Color color = val3.GetColor(shaderParam.propetyName);
								shaderParam.fColor[0].nValue = color.r;
								if (shaderParam.fColor[0].isEnd)
								{
									shaderParam.fColor[0].tValue = shaderParam.fColor[0].nValue;
								}
								shaderParam.fColor[1].nValue = color.g;
								if (shaderParam.fColor[1].isEnd)
								{
									shaderParam.fColor[1].tValue = shaderParam.fColor[1].nValue;
								}
								shaderParam.fColor[2].nValue = color.b;
								if (shaderParam.fColor[2].isEnd)
								{
									shaderParam.fColor[2].tValue = shaderParam.fColor[2].nValue;
								}
								shaderParam.fBlend.nValue = val3.GetFloat("_BlendRate");
								if (shaderParam.fBlend.isEnd)
								{
									shaderParam.fBlend.tValue = shaderParam.fBlend.nValue;
								}
							}
							materialList.Add(val3);
							break;
						}
					}
				}
				if (materialList.Count == 0)
				{
					Debug.LogError((object)("not shader [" + shaderParam.shaderName + "]"));
					return;
				}
				if (shaderParam.fColor[0].nValue == shaderParam.fColor[0].tValue && shaderParam.fColor[1].nValue == shaderParam.fColor[1].tValue && shaderParam.fColor[2].nValue == shaderParam.fColor[2].tValue && shaderParam.fBlend.tValue == shaderParam.fBlend.nValue && shaderParam.fBlend.tValue >= 0f)
				{
					return;
				}
				shaderParam.aliveFlag = true;
				for (int n = 0; n < 3; n++)
				{
					shaderParam.fColor[n].CalcSpeed(num3);
				}
				shaderParam.fBlend.CalcSpeed(num3);
			}
			shaderParam.isColor = true;
			if (!shaderParam.fBlend.isEnd)
			{
				shaderParam.isBlend = true;
			}
		}
	}

	public void ForceEnd()
	{
		foreach (KeyValuePair<string, ShaderParam> shaderParam in shaderParams)
		{
			ShaderParam value = shaderParam.Value;
			if (value.aliveFlag && value.forceEndFlag)
			{
				value.aliveFlag = false;
			}
		}
	}

	public void Update()
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		if (!materialList.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, ShaderParam> shaderParam in shaderParams)
			{
				ShaderParam value = shaderParam.Value;
				if (value.aliveFlag)
				{
					for (int i = 0; i < 3; i++)
					{
						value.fColor[i].Update();
					}
					value.fBlend.Update();
					cacheColor.r = value.fColor[0].nValue;
					cacheColor.g = value.fColor[1].nValue;
					cacheColor.b = value.fColor[2].nValue;
					int j = 0;
					for (int count = materialList.Count; j < count; j++)
					{
						Material val = materialList[j];
						val.SetColor(value.propetyName, cacheColor);
						val.SetFloat("_BlendRate", value.fBlend.nValue);
					}
					if (value.fColor[0].isEnd && value.fColor[1].isEnd && value.fColor[2].isEnd && value.fBlend.isEnd)
					{
						value.aliveFlag = false;
					}
				}
			}
		}
	}

	public List<ShaderSyncParam> GetShaderParamList()
	{
		if (shaderParams.Count == 0)
		{
			return null;
		}
		List<ShaderSyncParam> list = new List<ShaderSyncParam>();
		foreach (KeyValuePair<string, ShaderParam> shaderParam in shaderParams)
		{
			list.Add(shaderParam.Value.GetSyncParam());
		}
		return list;
	}

	public void Sync(SkinnedMeshRenderer[] renderers, List<ShaderSyncParam> shaderParamList)
	{
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		if (!renderers.IsNullOrEmpty() && !shaderParamList.IsNullOrEmpty())
		{
			for (int i = 0; i < shaderParamList.Count; i++)
			{
				ShaderSyncParam shaderSyncParam = shaderParamList[i];
				ShaderParam shaderParam;
				if (shaderParams.ContainsKey(shaderSyncParam.shaderName))
				{
					shaderParam = shaderParams[shaderSyncParam.shaderName];
				}
				else
				{
					shaderParam = new ShaderParam();
					shaderParam.Init();
					shaderParam.shaderName = shaderSyncParam.shaderName;
					shaderParam.propetyName = shaderSyncParam.propetyName;
					shaderParams.Add(shaderSyncParam.shaderName, shaderParam);
				}
				shaderParam.shaderName = shaderSyncParam.shaderName;
				shaderParam.propetyName = shaderSyncParam.propetyName;
				shaderParam.isColor = shaderSyncParam.isColor;
				if (shaderParam.isColor)
				{
					shaderParam.fColor[0].tValue = shaderSyncParam.color.r;
					shaderParam.fColor[0].isEnd = true;
					shaderParam.fColor[1].tValue = shaderSyncParam.color.g;
					shaderParam.fColor[1].isEnd = true;
					shaderParam.fColor[2].tValue = shaderSyncParam.color.b;
					shaderParam.fColor[2].isEnd = true;
				}
				shaderParam.isBlend = shaderSyncParam.isBlendRate;
				if (shaderParam.isBlend)
				{
					shaderParam.fBlend.tValue = shaderSyncParam.blendRate;
					shaderParam.fBlend.isEnd = true;
				}
				shaderParam.isBlendEnable = shaderSyncParam.isBlendEnable;
				shaderParam.blendEnable = shaderSyncParam.blendEnable;
				int j = 0;
				for (int num = renderers.Length; j < num; j++)
				{
					int k = 0;
					for (int num2 = renderers[j].get_materials().Length; k < num2; k++)
					{
						Material val = renderers[j].get_materials()[k];
						if (val.get_shader().get_name().Contains(shaderParam.shaderName))
						{
							if (shaderParam.isColor)
							{
								val.SetColor(shaderParam.propetyName, shaderSyncParam.color);
							}
							if (shaderParam.isBlend)
							{
								val.SetFloat("_BlendRate", shaderSyncParam.blendRate);
							}
							if (shaderParam.isBlendEnable)
							{
								val.SetFloat("_BlendEnable", (!shaderParam.blendEnable) ? 0f : 1f);
							}
						}
					}
				}
			}
		}
	}
}