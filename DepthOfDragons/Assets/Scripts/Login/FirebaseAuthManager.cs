using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;

public class FirebaseAuthManager
{
    private static FirebaseAuthManager _instance = null;
    public static FirebaseAuthManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new FirebaseAuthManager();
            return _instance;
        }
    }

    private FirebaseAuth _auth;
    private FirebaseUser _user;
    private DatabaseReference _dbRef;

    private bool _firebaseInitialized = false;
    private bool _isCreatingAccount = false;

    public string UserId => _user?.UserId;
    public Action<bool> LoginState;

    public async void Init()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            // Database URL ����
            app.Options.DatabaseUrl = new Uri("https://depthofdragons-default-rtdb.firebaseio.com/");
            _dbRef = FirebaseDatabase.GetInstance(app).RootReference;

            _auth = FirebaseAuth.DefaultInstance;
            _firebaseInitialized = true;

            string autoLogin = PlayerPrefs.GetString("AutoLogin", "false");
            if (autoLogin == "true")
            {
                string savedID = PlayerPrefs.GetString("ID", "");
                string savedPassword = PlayerPrefs.GetString("Password", "");

                if (!string.IsNullOrEmpty(savedID) && !string.IsNullOrEmpty(savedPassword))
                {
                    LogIn(savedID, savedPassword);
                }
            }
            else
            {
                // �ڵ� �α����� ���� ������ �α׾ƿ� ó��
                if (_auth.CurrentUser != null)
                {
                    LogOut();
                }
            }

            _auth.StateChanged += OnChanged;
            //Debug.Log("Firebase �ʱ�ȭ �Ϸ�");
        }
        else
        {
            //Debug.LogError("Firebase dependencies not available: " + dependencyStatus.ToString());
        }
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if (_auth.CurrentUser != _user)
        {
            bool signedIn = (_auth.CurrentUser != null);
            _user = _auth.CurrentUser;

            if (signedIn)
            {
                if (_isCreatingAccount)
                {
                    // ȸ������ ���� �ڵ� �α����� ����
                    //Debug.Log("ȸ������ �� �ڵ� �α��� ����");
                    return;
                }

                //Debug.Log("�α���");
                LoginState?.Invoke(true);
            }
            else
            {
                //Debug.Log("�α׾ƿ�");
                LoginState?.Invoke(false);
            }
        }
    }

    public async void CheckIDDuplicate(string id, Action<bool> onCheckComplete)
    {
        if (!_firebaseInitialized)
        {
            //Debug.LogError("Firebase�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            onCheckComplete?.Invoke(true); // �ߺ� ó��
            return;
        }

        if (string.IsNullOrEmpty(id))
        {
            onCheckComplete?.Invoke(true);
            return;
        }

        try
        {
            string safeIDKey = id.Replace(".", "_").Replace("@", "_at_");
            DataSnapshot snapshot = await _dbRef.Child("ID").Child(safeIDKey).GetValueAsync();
            bool exists = snapshot.Exists;
            onCheckComplete?.Invoke(exists);
        }
        catch (Exception ex)
        {
            //Debug.LogError("�̸��� �ߺ� Ȯ�� ����: " + ex.Message);
            onCheckComplete?.Invoke(true); // ���� �� �ߺ� ó��
        }
    }

    public void Create(string id, string password, Action onSuccess = null)
    {
        if (!_firebaseInitialized)
        {
            //Debug.LogError("Firebase�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        _isCreatingAccount = true;

        _auth.CreateUserWithEmailAndPasswordAsync(id, password).ContinueWith(async task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                //Debug.LogError("ȸ������ ����");
                _isCreatingAccount = false;
                return;
            }

            FirebaseUser newUser = task.Result.User;
            //Debug.Log("ȸ������ �Ϸ�: " + newUser.UserId);

            try
            {
                string safeEmailKey = id.Replace(".", "_").Replace("@", "_at_");
                await _dbRef.Child("ID").Child(safeEmailKey).SetValueAsync(newUser.UserId);
            }
            catch (Exception ex)
            {
                //Debug.LogError("���� ���� ���� ����: " + ex.Message);
            }

            LogOut(); // �α׾ƿ� ó��

            _isCreatingAccount = false;
            onSuccess?.Invoke();
        });
    }

    public void LogIn(string id, string password)
    {
        if (!_firebaseInitialized)
        {
            //Debug.LogError("Firebase�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        _auth.SignInWithEmailAndPasswordAsync(id, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                //Debug.LogError("�α��� ���");
                return;
            }
            if (task.IsFaulted)
            {
                //Debug.LogError("�α��� ����");
                return;
            }

            FirebaseUser newUser = task.Result.User;
            //Debug.Log("�α��� �Ϸ�: " + newUser.UserId);
        });
    }

    public void LogOut()
    {
        _auth?.SignOut();
        //Debug.Log("�α׾ƿ�");
    }
}
