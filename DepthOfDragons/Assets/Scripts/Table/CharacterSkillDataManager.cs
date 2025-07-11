using System.Collections.Generic;
using UnityEngine;
using System;

public struct CharacterSkillData
{
    public int Key;
    public string Name;
    public string Description;
    public int MpCost;
    public int Power;
    public int Duration;
    public int CoolTime;
    public int PlayerLevel;
    public int StartLevel;
    public int MaxLevel;
}

public class CharacterSkillDataManager : MonoBehaviour
{
    private static CharacterSkillDataManager _instance;
    public static CharacterSkillDataManager Instance => _instance;

    private Dictionary<int, CharacterSkillData> _skillDataDict = new Dictionary<int, CharacterSkillData>();

    private void Awake()
    {
        _instance = this;
        LoadCharacterSkillData();
    }

    public CharacterSkillData GetCharacterSkillData(int key)
    {
        return _skillDataDict[key];
    }

    private void LoadCharacterSkillData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Tables/CharacterSkillTable");

        if (textAsset == null)
        {
            return;
        }

        string[] rowData = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rowData.Length; i++)
        {
            string[] colData = rowData[i].Split(",");

            if (colData.Length < 10)
                continue;

            CharacterSkillData data;
            data.Key = int.Parse(colData[0]);
            data.Name = colData[1];
            data.Description = colData[2];
            data.MpCost = int.Parse(colData[3]);
            data.Power = int.Parse(colData[4]);
            data.Duration = int.Parse(colData[5]);
            data.CoolTime = int.Parse(colData[6]);
            data.PlayerLevel = int.Parse(colData[7]);
            data.StartLevel = int.Parse(colData[8]);
            data.MaxLevel = int.Parse(colData[9]);

            _skillDataDict[data.Key] = data;
        }
    }
}
