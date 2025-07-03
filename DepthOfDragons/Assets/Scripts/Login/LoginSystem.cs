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

        // �迭 �ʱ�ȭ
        _inputFields = new TMP_InputField[2];
        _buttons = new Button[3];

        Transform root = _loginWindow.transform;

        // InputField �Ҵ�
        _inputFields[(int)LoginInputFieldIndex.ID] = root.Find("IDInputField").GetComponent<TMP_InputField>();
        _inputFields[(int)LoginInputFieldIndex.Password] = root.Find("PasswordInputField").GetComponent<TMP_InputField>();

        // Button �Ҵ�
        _buttons[(int)LoginButtonType.LoginBtn] = root.Find("LoginButton").GetComponent<Button>();
        _buttons[(int)LoginButtonType.CreateAccountBtn] = root.Find("CreateAccountButton").GetComponent<Button>();
        _buttons[(int)LoginButtonType.LoginSaveBtn] = root.Find("LoginSaveText/LoginSaveButton").GetComponent<Button>();

        // CheckImage�� LoginSaveButton�� �ڽ� ������Ʈ
        _checkImage = _buttons[(int)LoginButtonType.LoginSaveBtn].transform.Find("CheckImage").gameObject;
        _checkImage.SetActive(false);

        // ��ư ������ ����
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
        // �α��� ��ư�� �����ų�, �ڵ� �α����� ��� ��� �κ�� �̵�
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
