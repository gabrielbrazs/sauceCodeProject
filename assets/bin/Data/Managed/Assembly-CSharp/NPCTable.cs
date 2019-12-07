using Network;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCTable : Singleton<NPCTable>, IDataTable
{
	public enum NPC_TYPE
	{
		OFFICIAL,
		FIGURE,
		FIGURE_TUTORIAL,
		QUEST_SPECIAL
	}

	public class NPCData
	{
		public int id;

		public string name;

		public string displayName;

		public NPC_TYPE npcType;

		public int npcModelID = -1;

		public int specialModelID = -1;

		public int sexID;

		public int faceTypeID;

		public int skinColorID;

		public int hairStyleID;

		public int hairColorID;

		public int bdy;

		public int hlm;

		public int arm;

		public int leg;

		public string anim;

		public int[] questids;

		public const string NT = "id,npcType,npcmdl,sex,face,scolor,hair,hcolor,bdy,hlm,arm,leg,anim,jp,displayName,specialMdl,questids";

		public static bool cb(CSVReader csv_reader, NPCData data, ref uint key)
		{
			data.id = (int)key;
			csv_reader.Pop(ref data.npcType);
			csv_reader.Pop(ref data.npcModelID);
			csv_reader.Pop(ref data.sexID);
			csv_reader.Pop(ref data.faceTypeID);
			csv_reader.Pop(ref data.skinColorID);
			csv_reader.Pop(ref data.hairStyleID);
			csv_reader.Pop(ref data.hairColorID);
			csv_reader.Pop(ref data.bdy);
			csv_reader.Pop(ref data.hlm);
			csv_reader.Pop(ref data.arm);
			csv_reader.Pop(ref data.leg);
			csv_reader.Pop(ref data.anim);
			csv_reader.Pop(ref data.name);
			csv_reader.Pop(ref data.displayName);
			csv_reader.Pop(ref data.specialModelID);
			if (string.IsNullOrEmpty(data.displayName))
			{
				data.displayName = data.name;
			}
			string value = "";
			csv_reader.Pop(ref value);
			data.questids = TableUtility.ParseStringToIntArray(value);
			return true;
		}

		public bool IsUsePlayerModel()
		{
			return npcModelID <= -1;
		}

		public PlayerLoadInfo CreatePlayerLoadInfo()
		{
			PlayerLoadInfo playerLoadInfo = new PlayerLoadInfo();
			playerLoadInfo.SetFace(sexID, faceTypeID, skinColorID);
			playerLoadInfo.SetHair(sexID, hairStyleID, hairColorID);
			playerLoadInfo.SetEquipBody(sexID, (uint)bdy);
			playerLoadInfo.SetEquipHead(sexID, (uint)hlm);
			playerLoadInfo.SetEquipArm(sexID, (uint)arm);
			playerLoadInfo.SetEquipLeg(sexID, (uint)leg);
			return playerLoadInfo;
		}

		public void CopyCharaInfo(CharaInfo info)
		{
			info.userId = 0;
			info.name = displayName;
			info.sex = sexID;
			info.faceId = faceTypeID;
			info.hairId = hairStyleID;
			info.hairColorId = hairColorID;
			info.skinId = skinColorID;
			info.aId = bdy;
			info.hId = hlm;
			info.rId = arm;
			info.lId = leg;
		}

		public ModelLoaderBase LoadModel(GameObject go, bool need_shadow, bool enable_light_probe, Action<Animator> on_complete, bool useSpecialModel)
		{
			if (IsUsePlayerModel())
			{
				PlayerLoader loader2 = go.AddComponent<PlayerLoader>();
				PlayerLoadInfo player_load_info = CreatePlayerLoadInfo();
				loader2.StartLoad(player_load_info, go.layer, 99, need_anim_event: false, need_foot_stamp: false, need_shadow, enable_light_probe, need_action_voice: false, need_high_reso_tex: false, FieldManager.IsValidInField(), need_dev_frame_instantiate: true, enable_light_probe ? ShaderGlobal.GetCharacterShaderType() : SHADER_TYPE.UI, delegate
				{
					if (on_complete != null)
					{
						on_complete(loader2.animator);
					}
				});
				return loader2;
			}
			NPCLoader loader = go.AddComponent<NPCLoader>();
			HomeThemeTable.HomeThemeData homeThemeData = Singleton<HomeThemeTable>.I.GetHomeThemeData(TimeManager.GetNow());
			int num = Singleton<HomeThemeTable>.I.GetNpcModelID(homeThemeData, id);
			int num2 = (num > 0) ? num : specialModelID;
			int npc_model_id = (useSpecialModel && num2 > 0) ? num2 : npcModelID;
			loader.Load(npc_model_id, go.layer, need_shadow, enable_light_probe, enable_light_probe ? ShaderGlobal.GetCharacterShaderType() : SHADER_TYPE.UI, delegate
			{
				if (on_complete != null)
				{
					on_complete(loader.animator);
				}
			});
			return loader;
		}

		public override string ToString()
		{
			return $"id={id}, name={name}, type={npcType}, sex={sexID}";
		}
	}

	private UIntKeyTable<NPCData> npcDataTable;

	public List<int>[] npcTypeOnNpcIdList = new List<int>[Enum.GetNames(typeof(NPC_TYPE)).Length];

	public void CreateTable(string csv_text)
	{
		npcDataTable = TableUtility.CreateUIntKeyTable<NPCData>(csv_text, NPCData.cb, "id,npcType,npcmdl,sex,face,scolor,hair,hcolor,bdy,hlm,arm,leg,anim,jp,displayName,specialMdl,questids");
		npcDataTable.TrimExcess();
		int i = 0;
		for (int num = npcTypeOnNpcIdList.Length; i < num; i++)
		{
			npcTypeOnNpcIdList[i] = new List<int>();
		}
		npcDataTable.ForEach(delegate(NPCData npcData)
		{
			npcTypeOnNpcIdList[(int)npcData.npcType].Add(npcData.id);
		});
	}

	public NPCData GetNPCData(int npc_id)
	{
		return npcDataTable.Get((uint)npc_id);
	}

	public NPCData GetNPCData(string name)
	{
		NPCData data = null;
		npcDataTable.ForEach(delegate(NPCData o)
		{
			if (data == null && o.name == name)
			{
				data = o;
			}
		});
		return data;
	}

	public NPCData GetNPCDataRandom(NPC_TYPE npc_type, List<int> exclusion_ids = null)
	{
		List<int> range = npcTypeOnNpcIdList[(int)npc_type].GetRange(0, npcTypeOnNpcIdList[(int)npc_type].Count);
		if (exclusion_ids != null)
		{
			int i = 0;
			for (int count = exclusion_ids.Count; i < count; i++)
			{
				int num = range.IndexOf(exclusion_ids[i]);
				if (num >= 0)
				{
					range.RemoveAt(num);
				}
			}
		}
		if (range.Count <= 0)
		{
			range = npcTypeOnNpcIdList[(int)npc_type].GetRange(0, npcTypeOnNpcIdList[(int)npc_type].Count);
		}
		int index = (int)(UnityEngine.Random.value * (float)range.Count);
		int key = range[index];
		return npcDataTable.Get((uint)key);
	}

	public NPCData GetNPCDataRandomFromQuestSpecial(int questid, List<int> exclusion_ids = null)
	{
		List<int> range = npcTypeOnNpcIdList[3].GetRange(0, npcTypeOnNpcIdList[3].Count);
		if (exclusion_ids != null)
		{
			int i = 0;
			for (int count = exclusion_ids.Count; i < count; i++)
			{
				int num = range.IndexOf(exclusion_ids[i]);
				if (num >= 0)
				{
					range.RemoveAt(num);
				}
			}
		}
		List<int> list = new List<int>();
		for (int j = 0; j < range.Count; j++)
		{
			NPCData nPCData = npcDataTable.Get((uint)range[j]);
			if (nPCData == null || nPCData.questids == null || nPCData.questids.Length == 0)
			{
				continue;
			}
			for (int k = 0; k < nPCData.questids.Length; k++)
			{
				if (nPCData.questids[k] == questid)
				{
					list.Add(nPCData.id);
					break;
				}
			}
		}
		if (list.Count > 0)
		{
			int index = (int)(UnityEngine.Random.value * (float)list.Count);
			int key = list[index];
			return npcDataTable.Get((uint)key);
		}
		return null;
	}
}
