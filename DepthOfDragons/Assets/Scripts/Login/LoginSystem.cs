using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginSystem : MonoBehaviour
{
    private GameObject _loginWindow;

    private enum LoginInputFieldIndex
    {
        ID, Password
    }

    private enum LoginButtonType
    {
        LoginBtn, CreateAccountBtn, LoginSaveBtn
    }

    private Button[] _buttons;
    private TMP_InputField[] _inputFields;
    private GameObject _checkImage;

    private void Start()
    {
        FirebaseAuthManager.Instance.LoginState += OnChangedState;
        FirebaseAuthManager.Instance.Init();

        _loginWindow = transform.Find("LoginWindow").gameObject;

        // 배열 초기화
        _inputFields = new TMP_InputField[2];
        _buttons = new Button[3];

        Transform root = _loginWindow.transform;

        // InputField 할당
        _inputFields[(int)LoginInputFieldIndex.ID] = root.Find("IDInputField").GetComponent<TMP_InputField>();
        _inputFields[(int)LoginInputFieldIndex.Password] = root.Find("PasswordInputField").GetComponent<TMP_InputField>();

        // Button 할당
        _buttons[(int)LoginButtonType.LoginBtn] = root.Find("LoginButton").GetComponent<Button>();
        _buttons[(int)LoginButtonType.CreateAccountBtn] = root.Find("CreateAccountButton").GetComponent<Button>();
        _buttons[(int)LoginButtonType.LoginSaveBtn] = root.Find("LoginSaveText/LoginSaveButton").GetComponent<Button>();

        // CheckImage는 LoginSaveButton의 자식 오브젝트
        _checkImage = _buttons[(int)LoginButtonType.LoginSaveBtn].transform.Find("CheckImage").gameObject;
        _checkImage.SetActive(false);

        // 버튼 리스너 연결
        _buttons[(int)LoginButtonType.LoginBtn].onClick.AddListener(LogIn);
        _buttons[(int)LoginButtonType.CreateAccountBtn].onClick.AddListener(OnClickCreateAccount);
        _buttons[(int)LoginButtonType.LoginSaveBtn].onClick.AddListener(ToggleCheckImage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchInputFocus();
        }
    }

    private void SwitchInputFocus()
    {
        if (_inputFields[(int)LoginInputFieldIndex.ID].isFocused)
        {
            _inputFields[(int)LoginInputFieldIndex.Password].Select();
        }
        else if (_inputFields[(int)LoginInputFieldIndex.Password].isFocused)
        {
            _inputFields[(int)LoginInputFieldIndex.ID].Select();
        }
    }

    private void ToggleCheckImage()
    {
        _checkImage.SetActive(!_checkImage.activeSelf);
    }

    private void OnChangedState(bool sign)
    {
        // 로그인 버튼을 눌렀거나, 자동 로그인일 경우 모두 로비로 이동
        if (sign)
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }

    private void LogIn()
    {
        string id = _inputFields[(int)LoginInputFieldIndex.ID].text;
        string password = _inputFields[(int)LoginInputFieldIndex.Password].text;
        bool isAutoLogin = _checkImage.activeSelf;

        if (isAutoLogin)
        {
            PlayerPrefs.SetString("AutoLogin", "true");
            PlayerPrefs.SetString("ID", id);
            PlayerPrefs.SetString("Password", password);
        }
        else
        {
            PlayerPrefs.SetString("AutoLogin", "false");
            PlayerPrefs.DeleteKey("ID");
            PlayerPrefs.DeleteKey("Password");
        }

        FirebaseAuthManager.Instance.LogIn(id, password);
    }

    private void OnClickCreateAccount()
    {
        CreateAccountSystem.OpenWindow();
    }

    public void ResetLoginUI()
    {
        _inputFields[(int)LoginInputFieldIndex.ID].text = "";
        _inputFields[(int)LoginInputFieldIndex.Password].text = "";
        _checkImage.SetActive(false);
    }
}
