using Firebase.Database;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;

public class CharacterCreationManager : MonoBehaviour
{
    private Transform _characterPosition;
    private Transform _selectCharacterClassPanel;
    private GameObject _currentCharacterModel;

    private TMP_InputField _nickNameInputField;
    private Button _nickNameDuplicateCheckButton;
    private TextMeshProUGUI _nickNameDuplicateCheckText;
    private Button _createButton;

    private Dictionary<string, GameObject> _classPrefabDict = new Dictionary<string, GameObject>();
    private bool _isNicknameAvailable = false;
    private string _selectedClassName = "";
    private int _selectedSlotIndex = -1;

    private readonly Queue<Action> _mainThreadQueue = new Queue<Action>();

    public void SetSelectedSlot(int index)
    {
        _selectedSlotIndex = index;
    }

    private void Start()
    {
        GameObject panelObj = GameObject.Find("Canvas/SelectCharacterClassPanel");
        GameObject positionObj = GameObject.Find("CharacterPosition");

        if (panelObj == null || positionObj == null)
        {
            Debug.LogError("SelectCharacterClassPanel 또는 CharacterPosition을 찾을 수 없습니다.");
            return;
        }

        _selectCharacterClassPanel = panelObj.transform;
        _characterPosition = positionObj.transform;

        LoadClassPrefabs();

        foreach (Transform child in _selectCharacterClassPanel)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                string className = child.name;
                button.onClick.AddListener(() => OnSelectClass(className));
            }
        }

        _nickNameInputField = GameObject.Find("Canvas/NickNameInputField").GetComponent<TMP_InputField>();
        _nickNameDuplicateCheckButton = GameObject.Find("Canvas/NickNameDuplicateCheckButton").GetComponent<Button>();
        _nickNameDuplicateCheckText = GameObject.Find("Canvas/NickNameInputField/NickNameDuplicateCheckText").GetComponent<TextMeshProUGUI>();
        _createButton = GameObject.Find("Canvas/CreateButton").GetComponent<Button>();
        _createButton.interactable = false;

        _nickNameDuplicateCheckButton.onClick.AddListener(OnClickNickNameCheck);
        _createButton.onClick.AddListener(OnClickCreateCharacter);


        _selectedSlotIndex = PlayerPrefs.GetInt("SelectedSlotIndex", -1);
        _nickNameInputField.text = "";
        _nickNameDuplicateCheckText.text = "";
        OnSelectClass("Knight");

        GameObject.Find("LobbyButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LobbyScene");
        });
    }

    private void Update()
    {
        while (_mainThreadQueue.Count > 0)
        {
            var action = _mainThreadQueue.Dequeue();
            action?.Invoke();
        }
    }

    private void RunOnMainThread(Action action)
    {
        lock (_mainThreadQueue)
        {
            _mainThreadQueue.Enqueue(action);
        }
    }

    private void LoadClassPrefabs()
    {
        string[] classNames = { "Knight", "Fighter", "Gunner", "Sorcerer", "Bishop" };

        foreach (string className in classNames)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Player/Character/{className}");
            if (prefab != null)
            {
                _classPrefabDict[className] = prefab;
            }
            else
            {
                Debug.LogWarning($"프리팹 로드 실패: {className} (경로: Resources/Prefabs/Player/Character/{className})");
            }
        }
    }

    private void OnSelectClass(string className)
    {
        _selectedClassName = className;

        if (_currentCharacterModel != null)
            Destroy(_currentCharacterModel);

        if (_classPrefabDict.TryGetValue(className, out GameObject prefab))
        {
            _currentCharacterModel = Instantiate(prefab, _characterPosition);
            _currentCharacterModel.transform.localPosition = Vector3.zero;
            _currentCharacterModel.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning($"선택된 직업({className})의 프리팹이 로드되지 않았습니다.");
        }
    }

    private void OnClickNickNameCheck()
    {
        string nickName = _nickNameInputField.text;

        if (string.IsNullOrEmpty(nickName))
        {
            _nickNameDuplicateCheckText.text = "닉네임을 입력해주세요.";
            _nickNameDuplicateCheckText.color = Color.red;
            _createButton.interactable = false;
            return;
        }

        FirebaseAuthManager.Instance.CheckNickNameDuplicate(nickName, (isDuplicate) =>
        {
            RunOnMainThread(() =>
            {
                if (isDuplicate)
                {
                    _nickNameDuplicateCheckText.text = "중복된 닉네임입니다.";
                    _nickNameDuplicateCheckText.color = Color.red;
                    _isNicknameAvailable = false;
                }
                else
                {
                    _nickNameDuplicateCheckText.text = "사용가능한 닉네임입니다.";
                    _nickNameDuplicateCheckText.color = Color.green;
                    _isNicknameAvailable = true;
                }

                _createButton.interactable = _isNicknameAvailable;
            });
        });
    }

    private void OnClickCreateCharacter()
    {
        string nickName = _nickNameInputField.text;
        string uid = FirebaseAuthManager.Instance.UserId;

        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(nickName) || string.IsNullOrEmpty(_selectedClassName))
        {
            Debug.LogError("UID, 닉네임 또는 클래스가 비어 있습니다.");
            return;
        }

        if (_selectedSlotIndex < 0 || _selectedSlotIndex > 3)
        {
            Debug.LogError("잘못된 슬롯 인덱스입니다.");
            return;
        }

        SceneManager.LoadScene("LobbyScene");

        //비동기 Firebase 저장 시작
        CharacterData? characterData = null;
        foreach (var kvp in CharacterDataManager.Instance.GetType().GetField("_characterDataDict", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(CharacterDataManager.Instance) as Dictionary<int, CharacterData>)
        {
            if (kvp.Value.Class == _selectedClassName)
            {
                characterData = kvp.Value;
                break;
            }
        }

        if (characterData == null)
        {
            Debug.LogError("해당 클래스 정보를 CharacterTable에서 찾을 수 없습니다.");
            return;
        }

        var dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        string slotPath = $"Users/{uid}/Slot{_selectedSlotIndex}";

        // 1단계: NickName과 Class 저장
        Dictionary<string, object> baseInfo = new Dictionary<string, object>
        {
            { "NickName", nickName },
            { "Class", _selectedClassName }
        };

        dbRef.Child(slotPath).UpdateChildrenAsync(baseInfo);

        // 2단계: 능력치 저장
        Dictionary<string, object> classData = new Dictionary<string, object>
        {
            { "Level", characterData.Value.StartLevel },
            { "Exp", characterData.Value.StartExp },
            { "SkillPoint", characterData.Value.StartSkillPoint },
            { "Hp", characterData.Value.StartHp },
            { "Mp", characterData.Value.StartMp },
            { "Gold", characterData.Value.StartGold },
            { "Attack", characterData.Value.StartAttack }
        };

        string classPath = $"{slotPath}/{_selectedClassName}";
        dbRef.Child(classPath).UpdateChildrenAsync(classData);

        // 3단계: 스킬 저장
        Dictionary<string, object> skillDict = new Dictionary<string, object>();

        foreach (var kvp in CharacterSkillDataManager.Instance.GetType().GetField("_skillDataDict", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(CharacterSkillDataManager.Instance) as Dictionary<int, CharacterSkillData>)
        {
            int skillKey = kvp.Key;
            CharacterSkillData skillData = kvp.Value;

            if (_selectedClassName == "Sorcerer" && skillKey >= 401 && skillKey <= 405 ||
                _selectedClassName == "Knight" && skillKey >= 101 && skillKey <= 105 ||
                _selectedClassName == "Fighter" && skillKey >= 201 && skillKey <= 205 ||
                _selectedClassName == "Gunner" && skillKey >= 301 && skillKey <= 305 ||
                _selectedClassName == "Bishop" && skillKey >= 501 && skillKey <= 505)
            {
                skillDict[skillKey.ToString()] = skillData.StartLevel;
            }
        }

        dbRef.Child($"{classPath}/Skills").SetValueAsync(skillDict);
    }
}