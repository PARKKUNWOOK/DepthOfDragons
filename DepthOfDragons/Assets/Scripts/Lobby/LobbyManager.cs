using Firebase.Database;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class LobbyManager : MonoBehaviour
{
    private GameObject _characterSlots;

    private enum LobbyButtonType
    {
        GameStartBtn, DeleteCharacterBtn, LogoutButton, CharacterSlot1Btn, CharacterSlot2Btn, CharacterSlot3Btn, CharacterSlot4Btn
    }

    private enum LobbyCharacterNickNameType
    {
        NickName1Text, NickName2Text, NickName3Text, NickName4Text
    }

    private Button[] _buttons;
    private TextMeshProUGUI[] _textMeshProUGUIs;
    private bool[] _isCharacterCreated = new bool[4];
    private int _selectedSlotIndex = -1;

    private Queue<Action> _mainThreadQueue = new Queue<Action>();

    private void Start()
    {
        _characterSlots = GameObject.Find("CharacterSlots");

        _buttons = new Button[7];
        _textMeshProUGUIs = new TextMeshProUGUI[4];

        _buttons[(int)LobbyButtonType.GameStartBtn] = GameObject.Find("GameStartButton")?.GetComponent<Button>();
        _buttons[(int)LobbyButtonType.DeleteCharacterBtn] = GameObject.Find("DeleteCharacterButton")?.GetComponent<Button>();
        _buttons[(int)LobbyButtonType.LogoutButton] = GameObject.Find("LogoutButton")?.GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot1Btn] = GameObject.Find("CharacterSlot1")?.GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot2Btn] = GameObject.Find("CharacterSlot2")?.GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot3Btn] = GameObject.Find("CharacterSlot3")?.GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot4Btn] = GameObject.Find("CharacterSlot4")?.GetComponent<Button>();

        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName1Text] = GameObject.Find("CharacterSlot1/CharacterSlot1NickNamePanel/NickName1")?.GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName2Text] = GameObject.Find("CharacterSlot2/CharacterSlot2NickNamePanel/NickName2")?.GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName3Text] = GameObject.Find("CharacterSlot3/CharacterSlot3NickNamePanel/NickName3")?.GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName4Text] = GameObject.Find("CharacterSlot4/CharacterSlot4NickNamePanel/NickName4")?.GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            _buttons[(int)LobbyButtonType.CharacterSlot1Btn + i].onClick.AddListener(() => OnClickCharacterSlot(index));

            _textMeshProUGUIs[i].text = "캐릭터 생성";
        }

        _buttons[(int)LobbyButtonType.DeleteCharacterBtn].onClick.AddListener(OnClickDeleteCharacter);
        _buttons[(int)LobbyButtonType.GameStartBtn].onClick.AddListener(OnClickGameStart);
        _buttons[(int)LobbyButtonType.LogoutButton].onClick.AddListener(OnClickLogout);

        LoadAllSlotData();
    }

    private void Update()
    {
        while (_mainThreadQueue.Count > 0)
        {
            Action action = _mainThreadQueue.Dequeue();
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

    private void LoadAllSlotData()
    {
        string uid = FirebaseAuthManager.Instance.UserId;
        var dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            string slotPath = $"Users/{uid}/Slot{index}";

            dbRef.Child(slotPath).GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var snapshot = task.Result;

                    if (snapshot.Exists && snapshot.HasChild("Class"))
                    {
                        string charClass = snapshot.Child("Class").Value.ToString();
                        string nickName = snapshot.Child("NickName").Value.ToString();

                        Debug.Log($"[슬롯 {index}] Class: {charClass}, NickName: {nickName}");

                        RunOnMainThread(() =>
                        {
                            _isCharacterCreated[index] = true;

                            if (_textMeshProUGUIs[index] != null)
                                _textMeshProUGUIs[index].text = nickName;

                            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Player/Character/{charClass}");
                            if (prefab == null)
                            {
                                Debug.LogError($"[슬롯 {index}] 프리팹 로드 실패: {charClass}");
                                return;
                            }

                            GameObject slotPos = GameObject.Find($"SlotCharacterPositions/Slot{index + 1}CharacterPos");
                            if (slotPos == null)
                            {
                                Debug.LogError($"[슬롯 {index}] SlotCharacterPositions/Slot{index + 1}CharacterPos 오브젝트가 없습니다.");
                                return;
                            }

                            GameObject character = Instantiate(prefab, slotPos.transform);
                            character.transform.localPosition = Vector3.zero;
                            character.transform.localRotation = Quaternion.identity;
                        });
                    }
                }
            });
        }
    }

    private void OnClickCharacterSlot(int index)
    {
        if (_isCharacterCreated[index])
        {
            _selectedSlotIndex = index;
            Debug.Log($"캐릭터 선택됨: {index + 1}번 슬롯");
        }
        else
        {
            PlayerPrefs.SetInt("SelectedSlotIndex", index);
            Debug.Log($"캐릭터 없음. 생성 씬으로 이동: {index + 1}번 슬롯");
            SceneManager.LoadScene("CreateCharacterScene");
        }
    }

    private void OnClickDeleteCharacter()
    {
        if (_selectedSlotIndex != -1 && _isCharacterCreated[_selectedSlotIndex])
        {
            Debug.Log($"캐릭터 삭제: 슬롯 {_selectedSlotIndex + 1}");

            _isCharacterCreated[_selectedSlotIndex] = false;
            _textMeshProUGUIs[_selectedSlotIndex].text = "캐릭터 생성";
            _selectedSlotIndex = -1;
        }
    }

    private void OnClickGameStart()
    {
        if (_selectedSlotIndex != -1 && _isCharacterCreated[_selectedSlotIndex])
        {
            Debug.Log($"게임 시작: {_selectedSlotIndex + 1}번 캐릭터");
            SceneManager.LoadScene("BravemarchScene");
        }
    }

    private void OnClickLogout()
    {
        Debug.Log("로그아웃 버튼 클릭됨");

        FirebaseAuthManager.Instance.LogOut();

        PlayerPrefs.SetString("AutoLogin", "false");
        PlayerPrefs.DeleteKey("ID");
        PlayerPrefs.DeleteKey("Password");

        SceneManager.LoadScene("LoginScene");
    }
}