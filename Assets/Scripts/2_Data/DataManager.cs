using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class WrongItemData
{
    public string trashName;
    public string correctCategory;
    public int errorCount;
}

[System.Serializable]
public class PlayerData
{
    public string username;
    public string password;
    public int totalScore;
    public List<WrongItemData> wrongBook = new List<WrongItemData>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [Header("面板容器")]
    public GameObject loginBox;
    public GameObject registerBox;

    [Header("弹窗提示 UI")]
    public GameObject popupPanel;
    public Text popupText;
    public Button btnPopupConfirm;

    [Header("登录界面 UI")]
    public InputField loginUsernameInput;
    public InputField loginPasswordInput;
    public Button btnLogin;
    public Button btnGoToRegister;

    [Header("注册界面 UI")]
    public InputField regUsernameInput;
    public InputField regPasswordInput;
    public InputField regConfirmPasswordInput;
    public Button btnConfirmRegister;
    public Button btnBackToLogin;

    [Header("退出游戏")]
    public Button btnExitGame;   // 退出游戏按钮



    // ==========================================
    //
    // ==========================================
    [Header("临时兼容旧代码（即将废弃）")]
    public string currentUsername = "";
    public PlayerData currentPlayer = new PlayerData(); // 给个空壳，防止报 Null 错误
    public void SavePlayerData()
    {
        // 留个空壳方法，啥也不干，专门用来堵住 GameManager 
    }
    // ==========================================

    void Awake()
    {
        Instance = this;
        // 已经删除了 DontDestroyOnLoad，完美！
    }

    void Start()
    {
        DatabaseManager.InitializeDatabase();

        popupPanel.SetActive(false);
        ShowLoginBox();

        btnPopupConfirm.onClick.AddListener(ClosePopup);
        btnLogin.onClick.AddListener(OnLoginClicked);
        btnGoToRegister.onClick.AddListener(ShowRegisterBox);
        btnConfirmRegister.onClick.AddListener(OnRegisterClicked);
        btnBackToLogin.onClick.AddListener(ShowLoginBox);
        btnExitGame.onClick.AddListener(OnExitGameClicked);
    }

    private void OnExitGameClicked()
    {
        // 如果在 Unity 编辑器里运行，就用编辑器的停止播放
    #if UNITY_EDITOR
           UnityEditor.EditorApplication.isPlaying = false;
    #else
        // 真机/打包后才能真正退出应用
        Application.Quit();
    #endif
    }


    private void ShowPopup(string message)
    {
        popupText.text = message;
        popupPanel.SetActive(true);
        popupPanel.transform.SetAsLastSibling();
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
        loginPasswordInput.text = "";
        regPasswordInput.text = "";
        regConfirmPasswordInput.text = "";
        regUsernameInput.text = "";
    }

    private void ShowLoginBox()
    {
        loginBox.SetActive(true);
        registerBox.SetActive(false);
        loginUsernameInput.text = "";
        loginPasswordInput.text = "";
    }

    private void ShowRegisterBox()
    {
        loginBox.SetActive(false);
        registerBox.SetActive(true);
        regUsernameInput.text = "";
        regPasswordInput.text = "";
        regConfirmPasswordInput.text = "";
    }

    private void OnRegisterClicked()
    {
        string regName = regUsernameInput.text.Trim();
        string regPwd = regPasswordInput.text;
        string regConfirmPwd = regConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(regName) || string.IsNullOrEmpty(regPwd))
        {
            ShowPopup("注册失败：账号或密码不能为空！"); return;
        }

        if (regPwd != regConfirmPwd)
        {
            ShowPopup("注册失败：两次输入的密码不一致！"); return;
        }

        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM UserTable WHERE username = @user";
                checkCmd.Parameters.AddWithValue("@user", regName);
                long count = (long)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    ShowPopup("注册失败：该账号已被注册！");
                    return;
                }
            }

            using (var insertCmd = conn.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO UserTable (username, totalScore) VALUES (@user, 0)";
                insertCmd.Parameters.AddWithValue("@user", regName);
                insertCmd.ExecuteNonQuery();
            }
        }

        ShowPopup("注册成功！请使用新账号登录。");
        ShowLoginBox();
    }

    private void OnLoginClicked()
    {
        string loginName = loginUsernameInput.text.Trim();
        string loginPwd = loginPasswordInput.text;

        if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(loginPwd))
        {
            ShowPopup("登录失败：账号或密码不能为空！"); return;
        }

        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM UserTable WHERE username = @user";
                cmd.Parameters.AddWithValue("@user", loginName);
                long count = (long)cmd.ExecuteScalar();

                if (count == 0)
                {
                    ShowPopup("登录失败：账号不存在，请先注册！");
                    return;
                }
            }
        }

        GameSession.CurrentUsername = loginName;
        Debug.Log($"🔑 【登录成功】玩家 {loginName} 获取了跨场景通行证！");

        SceneManager.LoadScene("GameScene");
    }
}