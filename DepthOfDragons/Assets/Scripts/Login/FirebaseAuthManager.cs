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

            // Database URL 설정
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
                // 자동 로그인이 꺼져 있으면 로그아웃 처리
                if (_auth.CurrentUser != null)
                {
                    LogOut();
                }
            }

            _auth.StateChanged += OnChanged;
            //Debug.Log("Firebase 초기화 완료");
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
                    // 회원가입 직후 자동 로그인은 무시
                    //Debug.Log("회원가입 중 자동 로그인 무시");
                    return;
                }

                //Debug.Log("로그인");
                LoginState?.Invoke(true);
            }
            else
            {
                //Debug.Log("로그아웃");
                LoginState?.Invoke(false);
            }
        }
    }

    public async void CheckIDDuplicate(string id, Action<bool> onCheckComplete)
    {
        if (!_firebaseInitialized)
        {
            //Debug.LogError("Firebase가 초기화되지 않았습니다.");
            onCheckComplete?.Invoke(true); // 중복 처리
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
            //Debug.LogError("이메일 중복 확인 실패: " + ex.Message);
            onCheckComplete?.Invoke(true); // 실패 시 중복 처리
        }
    }

    public void Create(string id, string password, Action onSuccess = null)
    {
        if (!_firebaseInitialized)
        {
            //Debug.LogError("Firebase가 초기화되지 않았습니다.");
            return;
        }

        _isCreatingAccount = true;

        _auth.CreateUserWithEmailAndPasswordAsync(id, password).ContinueWith(async task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                //Debug.LogError("회원가입 실패");
                _isCreatingAccount = false;
                return;
            }

            FirebaseUser newUser = task.Result.User;
            //Debug.Log("회원가입 완료: " + newUser.UserId);

            try
            {
                string safeEmailKey = id.Replace(".", "_").Replace("@", "_at_");
                await _dbRef.Child("ID").Child(safeEmailKey).SetValueAsync(newUser.UserId);
            }
            catch (Exception ex)
            {
                //Debug.LogError("유저 정보 저장 실패: " + ex.Message);
            }

            LogOut(); // 로그아웃 처리

            _isCreatingAccount = false;
            onSuccess?.Invoke();
        });
    }

    public void LogIn(string id, string password)
    {
        if (!_firebaseInitialized)
        {
            //Debug.LogError("Firebase가 초기화되지 않았습니다.");
            return;
        }

        _auth.SignInWithEmailAndPasswordAsync(id, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                //Debug.LogError("로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                //Debug.LogError("로그인 실패");
                return;
            }

            FirebaseUser newUser = task.Result.User;
            //Debug.Log("로그인 완료: " + newUser.UserId);
        });
    }

    public void LogOut()
    {
        _auth?.SignOut();
        //Debug.Log("로그아웃");
    }
}
