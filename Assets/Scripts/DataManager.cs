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

    [Header("弹窗提示 UI (新增)")]
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

    [Header("运行时核心数据")]
    public string currentUsername = "";
    public PlayerData currentPlayer;

    private string saveDirectory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        saveDirectory = Application.persistentDataPath;

        // 默认隐藏弹窗，显示登录面板
        popupPanel.SetActive(false);
        ShowLoginBox();

        // 绑定弹窗的确定按钮事件
        btnPopupConfirm.onClick.AddListener(ClosePopup);

        // 绑定其他按钮事件
        btnLogin.onClick.AddListener(OnLoginClicked);
        btnGoToRegister.onClick.AddListener(ShowRegisterBox);
        btnConfirmRegister.onClick.AddListener(OnRegisterClicked);
        btnBackToLogin.onClick.AddListener(ShowLoginBox);
    }

    // ==========================================
    // 新增：通用弹窗控制逻辑
    // ==========================================
    private void ShowPopup(string message)
    {
        popupText.text = message;
        popupPanel.SetActive(true);
        // 确保弹窗在所有 UI 的最上层显示
        popupPanel.transform.SetAsLastSibling();
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    // --- 界面切换逻辑 ---
    private void ShowLoginBox()
    {
        loginBox.SetActive(true);
        registerBox.SetActive(false);
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

    // --- 注册逻辑 ---
    private void OnRegisterClicked()
    {
        string regName = regUsernameInput.text.Trim();
        string regPwd = regPasswordInput.text;
        string regConfirmPwd = regConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(regName) || string.IsNullOrEmpty(regPwd))
        {
            ShowPopup("注册失败：账号或密码不能为空！");
            return;
        }

        if (regPwd != regConfirmPwd)
        {
            ShowPopup("注册失败：两次输入的密码不一致！");
            return;
        }

        string savePath = Path.Combine(saveDirectory, regName + "_Data.json");

        if (File.Exists(savePath))
        {
            ShowPopup("注册失败：该账号已被注册，请换一个账号！");
            return;
        }

        PlayerData newPlayer = new PlayerData();
        newPlayer.username = regName;
        newPlayer.password = regPwd;
        newPlayer.totalScore = 0;
        newPlayer.wrongBook = new List<WrongItemData>();

        string json = JsonUtility.ToJson(newPlayer, true);
        File.WriteAllText(savePath, json);

        // 注册成功提示，并切回登录界面
        ShowPopup("注册成功！请使用新账号登录。");
        ShowLoginBox();
    }

    // --- 登录逻辑 ---
    private void OnLoginClicked()
    {
        string loginName = loginUsernameInput.text.Trim();
        string loginPwd = loginPasswordInput.text;

        if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(loginPwd))
        {
            ShowPopup("登录失败：账号或密码不能为空！");
            return;
        }

        string savePath = Path.Combine(saveDirectory, loginName + "_Data.json");

        if (!File.Exists(savePath))
        {
            ShowPopup("登录失败：账号不存在，请先注册！");
            return;
        }

        string json = File.ReadAllText(savePath);
        PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);

        if (loadedData.password != loginPwd)
        {
            ShowPopup("登录失败：密码错误！");
            return;
        }

        currentUsername = loginName;
        currentPlayer = loadedData;

        // 登录成功时，直接跳转场景体验更流畅，不需要玩家再多点一次“确定”
        SceneManager.LoadScene("GameScene");
    }


    // ==========================================
    // 新增：全局存档方法
    // ==========================================
    public void SavePlayerData()
    {
        // 安全校验：如果没有登录玩家，就不执行保存
        if (currentPlayer == null || string.IsNullOrEmpty(currentUsername)) return;

        string savePath = Path.Combine(saveDirectory, currentUsername + "_Data.json");

        // 将当前内存里的 currentPlayer 最新状态转换成 JSON 并覆盖写入本地
        string json = JsonUtility.ToJson(currentPlayer, true);
        File.WriteAllText(savePath, json);

        Debug.Log("【数据持久化】玩家 [" + currentUsername + "] 的分数与错题已更新至本地 JSON！");
    }



}