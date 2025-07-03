using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateAccountSystem : MonoBehaviour
{
    private static CreateAccountSystem _instance;

    private GameObject _createAccountWindow;
    private GameObject _loginWindow;

    private enum CreateAccountInputFieldIndex
    {
        ID, Password, PasswordCheck
    }

    private enum CreateAccountButtonType
    {
        IDCheckBtn, PasswordCheckBtn, CreateBtn, CancelBtn
    }

    private enum CreateAccountCheckResultType
    {
        IDCheckResultText, PasswordCheckResultText
    }

    private Button[] _buttons;
    private TMP_InputField[] _inputFields;
    private TextMeshProUGUI[] _textMeshProUGUIs;

    private const int _minPasswordLength = 6;

    private bool _isIDAvailable = false;
    private bool _isPasswordMatched = false;
    private bool _shouldSwitchToLogin = false;

    private const string _iDDuplicateMessage = "�̹� ������� ���̵��Դϴ�.";
    private const string _iDAvailableMessage = "��밡���� ���̵��Դϴ�.";
    private const string _iDInputRequiredMessage = "�̸����� �Է����ּ���.";

    private const string _passwordMatchMessage = "��й�ȣ�� ��ġ�մϴ�.";
    private const string _passwordMismatchMessage = "��й�ȣ�� ��ġ���� �ʽ��ϴ�.";
    private const string _passwordInputRequiredMessage = "��й�ȣ�� �Է����ּ���.";

    private const string _iDCheckRequiredMessage = "���̵� �ߺ�üũ�� ���ּ���.";
    private const string _passwordCheckRequiredMessage = "��й�ȣ Ȯ��üũ�� ���ּ���.";

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _createAccountWindow = transform.Find("CreateAccountWindow").gameObject;
        _loginWindow = transform.Find("LoginWindow").gameObject;

        // CreateAccountWindow ���� Ʈ������ �������� ������Ʈ ã��
        Transform root = _createAccountWindow.transform;

        // �迭 �Ҵ�
        _inputFields = new TMP_InputField[3];
        _buttons = new Button[4];
        _textMeshProUGUIs = new TextMeshProUGUI[2];

        // TMP_InputField �ʱ�ȭ
        _inputFields[(int)CreateAccountInputFieldIndex.ID] = root.Find("IDInputField").GetComponent<TMP_InputField>();
        _inputFields[(int)CreateAccountInputFieldIndex.Password] = root.Find("PasswordInputField").GetComponent<TMP_InputField>();
        _inputFields[(int)CreateAccountInputFieldIndex.PasswordCheck] = root.Find("PasswordCheckInputField").GetComponent<TMP_InputField>();

        // Button �ʱ�ȭ
        _buttons[(int)CreateAccountButtonType.IDCheckBtn] = root.Find("IDDuplicateCheckButton").GetComponent<Button>();
        _buttons[(int)CreateAccountButtonType.PasswordCheckBtn] = root.Find("PasswordCheckButton").GetComponent<Button>();
        _buttons[(int)CreateAccountButtonType.CreateBtn] = root.Find("CreateButton").GetComponent<Button>();
        _buttons[(int)CreateAccountButtonType.CancelBtn] = root.Find("CancelButton").GetComponent<Button>();

        // Text ��� �ʱ�ȭ
        _textMeshProUGUIs[(int)CreateAccountCheckResultType.IDCheckResultText] = root.Find("IDInputField/IDDuplicateCheckText").GetComponent<TextMeshProUGUI>();
        _textMeshProUGUIs[(int)CreateAccountCheckResultType.PasswordCheckResultText] = root.Find("PasswordCheckInputField/PasswordCheckText").GetComponent<TextMeshProUGUI>();

        // ��ư ������ ����
        _buttons[(int)CreateAccountButtonType.CreateBtn].onClick.AddListener(OnCreateClicked);
        _buttons[(int)CreateAccountButtonType.CancelBtn].onClick.AddListener(OnCancelClicked);
        _buttons[(int)CreateAccountButtonType.IDCheckBtn].onClick.AddListener(OnIDCheckClicked);
        _buttons[(int)CreateAccountButtonType.PasswordCheckBtn].onClick.AddListener(OnPasswordCheckClicked);

        _buttons[(int)CreateAccountButtonType.CreateBtn].interactable = false;
        _createAccountWindow.SetActive(false);
    }

    private void Update()
    {
        if (_shouldSwitchToLogin)
        {
            _shouldSwitchToLogin = false;
            _createAccountWindow.SetActive(false);
            _loginWindow.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchInputFocus();
        }
    }

    private void SwitchInputFocus()
    {
        if (_inputFields[(int)CreateAccountInputFieldIndex.ID].isFocused)
        {
            _inputFields[(int)CreateAccountInputFieldIndex.Password].Select();
        }
        else if (_inputFields[(int)CreateAccountInputFieldIndex.Password].isFocused)
        {
            _inputFields[(int)CreateAccountInputFieldIndex.PasswordCheck].Select();
        }
        else if (_inputFields[(int)CreateAccountInputFieldIndex.PasswordCheck].isFocused)
        {
            _inputFields[(int)CreateAccountInputFieldIndex.ID].Select();
        }
    }

    public static void OpenWindow()
    {
        if (_instance != null)
        {
            _instance._OpenWindowInternal();
        }
    }

    private void _OpenWindowInternal()
    {
        _createAccountWindow.SetActive(true);
        _loginWindow.SetActive(false);

        foreach (var input in _inputFields)
            input.text = "";

        foreach (var text in _textMeshProUGUIs)
        {
            text.text = "";
            text.color = Color.white;
        }

        _isIDAvailable = false;
        _isPasswordMatched = false;

        _buttons[(int)CreateAccountButtonType.CreateBtn].interactable = false;
    }

    private void OnCancelClicked()
    {
        _createAccountWindow.SetActive(false);
        _loginWindow.SetActive(true);

        LoginSystem loginSystem = FindObjectOfType<LoginSystem>();
        if (loginSystem != null)
        {
            loginSystem.ResetLoginUI();
        }
    }

    private void OnIDCheckClicked()
    {
        string id = _inputFields[(int)CreateAccountInputFieldIndex.ID].text;

        if (string.IsNullOrEmpty(id))
        {
            SetCheckResult(CreateAccountCheckResultType.IDCheckResultText, _iDInputRequiredMessage, Color.red);
            return;
        }

        FirebaseAuthManager.Instance.CheckIDDuplicate(id, (isDuplicate) =>
        {
            if (isDuplicate)
            {
                SetCheckResult(CreateAccountCheckResultType.IDCheckResultText, _iDDuplicateMessage, Color.red);
                _isIDAvailable = false;
            }
            else
            {
                SetCheckResult(CreateAccountCheckResultType.IDCheckResultText, _iDAvailableMessage, Color.green);
                _isIDAvailable = true;
            }

            UpdateCreateButtonState();
        });
    }

    private void OnPasswordCheckClicked()
    {
        string pw = _inputFields[(int)CreateAccountInputFieldIndex.Password].text;
        string pwCheck = _inputFields[(int)CreateAccountInputFieldIndex.PasswordCheck].text;

        if (string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(pwCheck))
        {
            SetCheckResult(CreateAccountCheckResultType.PasswordCheckResultText, _passwordInputRequiredMessage, Color.red);
            _isPasswordMatched = false;
        }
        else if (pw == pwCheck && pw.Length >= _minPasswordLength)
        {
            SetCheckResult(CreateAccountCheckResultType.PasswordCheckResultText, _passwordMatchMessage, Color.green);
            _isPasswordMatched = true;
        }
        else
        {
            SetCheckResult(CreateAccountCheckResultType.PasswordCheckResultText, _passwordMismatchMessage, Color.red);
            _isPasswordMatched = false;
        }

        UpdateCreateButtonState();
    }

    private void OnCreateClicked()
    {
        if (!_isIDAvailable)
        {
            SetCheckResult(CreateAccountCheckResultType.IDCheckResultText, _iDCheckRequiredMessage, Color.red);
            return;
        }

        if (!_isPasswordMatched)
        {
            SetCheckResult(CreateAccountCheckResultType.PasswordCheckResultText, _passwordCheckRequiredMessage, Color.red);
            return;
        }

        string id = _inputFields[(int)CreateAccountInputFieldIndex.ID].text;
        string password = _inputFields[(int)CreateAccountInputFieldIndex.Password].text;

        FirebaseAuthManager.Instance.Create(id, password, () =>
        {
            _shouldSwitchToLogin = true;
        });
    }

    private void SetCheckResult(CreateAccountCheckResultType type, string message, Color color)
    {
        _textMeshProUGUIs[(int)type].text = message;
        _textMeshProUGUIs[(int)type].color = color;
    }

    private void UpdateCreateButtonState()
    {
        _buttons[(int)CreateAccountButtonType.CreateBtn].interactable =
            _isIDAvailable && _isPasswordMatched;
    }
}
