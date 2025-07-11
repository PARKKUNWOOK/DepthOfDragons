using System.Collections.Generic;
using UnityEngine;

public struct CharacterData
{
    public int Key;
    public string Class;
    public int StartLevel;
    public int StartExp;
    public int StartSkillPt;
    public int StartHp;
    public int StartMp;
    public int StartGold;
    public int StartAttack;
    public int MaxExp;
    public int MaxLevel;
}

public class CharacterDataManager : MonoBehaviour
{
    private static CharacterDataManager _instance;
    public static CharacterDataManager Instance => _instance;

    private Dictionary<int, CharacterData> _characterDataDict = new Dictionary<int, CharacterData>();

    private void Awake()
    {
        _instance = this;
        LoadCharacterData();
    }

    public CharacterData GetCharacterData(int key)
    {
        return _characterDataDict[key];
    }

    private void LoadCharacterData()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Tables/CharacterTable");

        if (textAsset == null)
        {
            Debug.LogError("CharacterTable.csv not found in Resources/Tables folder.");
            return;
        }

        string[] rowData = textAsset.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < rowData.Length; i++)
        {
            string[] colData = rowData[i].Split(",");

            if (colData.Length < 11)
                continue;

            CharacterData data;
            data.Key = int.Parse(colData[0]);
            data.Class = colData[1];
            data.StartLevel = int.Parse(colData[2]);
            data.StartExp = int.Parse(colData[3]);
            data.StartSkillPt = int.Parse(colData[4]);
            data.StartHp = int.Parse(colData[5]);
            data.StartMp = int.Parse(colData[6]);
            data.StartGold = int.Parse(colData[7]);
            data.StartAttack = int.Parse(colData[8]);
            data.MaxExp = int.Parse(colData[9]);
            data.MaxLevel = int.Parse(colData[10]);

            _characterDataDict[data.Key] = data;
        }

        Debug.Log($"CharacterTable Loaded: {_characterDataDict.Count} entries");
    }
}
