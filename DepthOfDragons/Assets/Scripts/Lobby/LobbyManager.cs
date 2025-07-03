using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private GameObject _characterSlots;

    private enum LobbyButtonType
    {
        GameStartBtn, DeleteCharacterBtn, CharacterSlot1Btn, CharacterSlot2Btn, CharacterSlot3Btn, CharacterSlot4Btn
    }

    private enum LobbyCharacterNickNameType
    {
        NickName1Text, NickName2Text, NickName3Text, NickName4Text
    }

    private Button[] _buttons;
    private TextMeshProUGUI[] _textMeshProUGUIs;
    private bool[] _isCharacterCreated = new bool[4];
    private int _selectedSlotIndex = -1;

    private void Start()
    {
        _characterSlots = transform.Find("CharacterSlots").gameObject;

        _buttons = new Button[6];
        _textMeshProUGUIs = new TextMeshProUGUI[4];

        Transform root = _characterSlots.transform;

        _buttons[(int)LobbyButtonType.GameStartBtn] = transform.Find("GameStartButton").GetComponent<Button>();
        _buttons[(int)LobbyButtonType.DeleteCharacterBtn] = transform.Find("DeleteCharacterButton").GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot1Btn] = root.Find("CharacterSlot1").GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot2Btn] = root.Find("CharacterSlot2").GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot3Btn] = root.Find("CharacterSlot3").GetComponent<Button>();
        _buttons[(int)LobbyButtonType.CharacterSlot4Btn] = root.Find("CharacterSlot4").GetComponent<Button>();

        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName1Text] = root.Find("NickName1").GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName2Text] = root.Find("NickName2").GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName3Text] = root.Find("NickName3").GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)LobbyCharacterNickNameType.NickName4Text] = root.Find("NickName4").GetComponent<TextMeshProUGUI>();

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            _buttons[(int)LobbyButtonType.CharacterSlot1Btn + i].onClick.AddListener(() => OnClickCharacterSlot(index));

            if (!_isCharacterCreated[i])
            {
                _textMeshProUGUIs[i].text = "캐릭터 생성";
            }
        }

        _buttons[(int)LobbyButtonType.DeleteCharacterBtn].onClick.AddListener(OnClickDeleteCharacter);
        _buttons[(int)LobbyButtonType.GameStartBtn].onClick.AddListener(OnClickGameStart);
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

    

}
