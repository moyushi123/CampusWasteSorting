using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.InputSystem;


// ==========================================
// 数据结构：本地排行榜
// ==========================================
[System.Serializable]
public class LeaderboardEntry
{
    public string username;
    public int score;
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 容器总控")]
    public GameObject mainMenuUI;
    public GameObject gameplayUI;
    public GameObject subPanelsUI;
    public GameObject settlementUI;

    [Header("主菜单按钮")]
    public Button btnChallengeMode;
    public Button btnFreeMode;
    public Button btnRules;
    public Button btnWrongBook;
    public Button btnLeaderboard;

    [Header("游玩界面 UI")]
    public Text textTimer;
    public Text textScore;
    public Button btnExitFreeMode;

    [Header("子界面与弹窗")]
    public GameObject panelRules;
    public Button btnCloseRules;
    public GameObject panelWrongBook;
    public Text textWrongList;
    public Button btnCloseWrongBook;
    public GameObject panelLeaderboard;
    public Text textRankList;
    public Button btnCloseLeaderboard;

    [Header("结算界面 UI")]
    public Text textGameOverTitle;
    public Text textFinalScore;
    public Text textHighscoreTip;
    public Button btnBackToMainMenu;

    [Header("玩家控制脚本 (核心魔法)")]
    [Tooltip("把玩家身上的移动(Movement)和视角(Camera)脚本拖进来。菜单时自动禁用，游戏时启用，实现完美待机！")]
    public Behaviour[] playerControlScripts;

    // --- 运行时状态 ---
    private bool isGameActive = false;
    private bool isChallengeMode = false;
    private float timeLeft = 180f;
    public int currentMatchScore = 0; // 当局得分

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // 1. 初始化 UI 状态：只显示主菜单
        mainMenuUI.SetActive(true);
        gameplayUI.SetActive(false);
        subPanelsUI.SetActive(false);
        settlementUI.SetActive(false);

        // 2. 锁定并显示鼠标，禁用玩家控制（此时角色会在背景播放待机动画）
        SetPlayerControl(false);

        // 3. 绑定主菜单事件
        btnChallengeMode.onClick.AddListener(StartChallengeMode);
        btnFreeMode.onClick.AddListener(StartFreeMode);
        btnRules.onClick.AddListener(() => ShowSubPanel(panelRules));
        btnWrongBook.onClick.AddListener(ShowWrongBookData);
        btnLeaderboard.onClick.AddListener(ShowLeaderboardData);

        // 4. 绑定关闭弹窗事件
        btnCloseRules.onClick.AddListener(CloseAllSubPanels);
        btnCloseWrongBook.onClick.AddListener(CloseAllSubPanels);
        btnCloseLeaderboard.onClick.AddListener(CloseAllSubPanels);

        // 5. 绑定游玩与结算事件
        btnExitFreeMode.onClick.AddListener(EndMatch);
        btnBackToMainMenu.onClick.AddListener(RestartScene);
    }

    void Update()
    {
        // 仅在游戏进行中时才执行检测
        if (isGameActive)
        {
            // 1. 挑战模式的倒计时逻辑
            if (isChallengeMode)
            {
                timeLeft -= Time.deltaTime;
                textTimer.text = "时间: " + Mathf.CeilToInt(timeLeft).ToString() + "s";

                if (timeLeft <= 0)
                {
                    timeLeft = 0;
                    EndMatch(); // 时间到，结束游戏
                }
            }

            // 2. ⬅️ 新增：检测 ESC 键主动结束游戏
            // 无论是挑战模式想提前放弃，还是游览模式想结束逛街，按下 ESC 直接结算
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EndMatch();
            }
        }
    }

    // ==========================================
    // 游戏流程控制
    // ==========================================
    private void StartChallengeMode()
    {
        isChallengeMode = true;
        timeLeft = 180f;
        textTimer.text = "时间: 180s";
        btnExitFreeMode.gameObject.SetActive(false); // 挑战模式不能主动退出，必须等时间到
        BeginGameplay();
    }

    private void StartFreeMode()
    {
        isChallengeMode = false;
        textTimer.text = "模式: 自由游览";
        btnExitFreeMode.gameObject.SetActive(true); // 自由模式可以随时点左上角退出
        BeginGameplay();
    }

    private void BeginGameplay()
    {
        currentMatchScore = 0;
        UpdateScoreUI();

        mainMenuUI.SetActive(false);
        gameplayUI.SetActive(true);

        SetPlayerControl(true); // 恢复玩家控制，鼠标隐藏锁定
        isGameActive = true;
    }

    private void EndMatch()
    {
        isGameActive = false;
        SetPlayerControl(false); // 再次剥夺控制权，呼出鼠标

        gameplayUI.SetActive(false);
        settlementUI.SetActive(true);

        // --- 核心数据结算 ---
        textGameOverTitle.text = isChallengeMode ? "挑战结束！" : "游览结束！";
        textFinalScore.text = "本次得分: " + currentMatchScore;

        if (DataManager.Instance != null && DataManager.Instance.currentPlayer != null)
        {
            // 1. 累加到历史总分并保存
            DataManager.Instance.currentPlayer.totalScore += currentMatchScore;
            DataManager.Instance.SavePlayerData();

            // 2. 如果是挑战模式，尝试录入排行榜
            if (isChallengeMode)
            {
                SaveToLeaderboard(DataManager.Instance.currentUsername, currentMatchScore);
                textHighscoreTip.text = "分数已记录至本地，并在总分中累加！";
            }
            else
            {
                textHighscoreTip.text = "自由模式不计入排行榜，但分数已累计！";
            }
        }
    }

    private void RestartScene()
    {
        // 核心魔法：重新加载当前场景，所有的垃圾、人物位置都会完美复原！
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ==========================================
    // 子面板逻辑与数据读取
    // ==========================================
    private void ShowSubPanel(GameObject targetPanel)
    {
        subPanelsUI.SetActive(true);
        panelRules.SetActive(false);
        panelWrongBook.SetActive(false);
        panelLeaderboard.SetActive(false);

        targetPanel.SetActive(true);
    }

    private void CloseAllSubPanels()
    {
        subPanelsUI.SetActive(false);
    }

    private void ShowWrongBookData()
    {
        ShowSubPanel(panelWrongBook);
        textWrongList.text = "【我的错题记录】\n\n";

        if (DataManager.Instance != null && DataManager.Instance.currentPlayer != null)
        {
            var wrongBook = DataManager.Instance.currentPlayer.wrongBook;
            if (wrongBook.Count == 0)
            {
                textWrongList.text += "太棒了！你目前没有任何错题记录。";
            }
            else
            {
                foreach (var item in wrongBook)
                {
                    textWrongList.text += $"- 误判【{item.trashName}】，它应属于【{item.correctCategory}】 (错 {item.errorCount} 次)\n";
                }
            }
        }
        else
        {
            textWrongList.text += "未获取到玩家数据...";
        }
    }

    // ==========================================
    // 排行榜读写逻辑
    // ==========================================
    private string GetLeaderboardPath()
    {
        return Path.Combine(Application.persistentDataPath, "GlobalLeaderboard.json");
    }

    private void SaveToLeaderboard(string username, int score)
    {
        string path = GetLeaderboardPath();
        LeaderboardData data = new LeaderboardData();

        if (File.Exists(path))
        {
            data = JsonUtility.FromJson<LeaderboardData>(File.ReadAllText(path));
        }

        // 添加新记录
        data.entries.Add(new LeaderboardEntry { username = username, score = score });

        // 按照分数降序排列，只保留前 5 名
        data.entries = data.entries.OrderByDescending(e => e.score).Take(5).ToList();

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    private void ShowLeaderboardData()
    {
        ShowSubPanel(panelLeaderboard);
        textRankList.text = "【挑战模式前五名】\n\n";

        string path = GetLeaderboardPath();
        if (File.Exists(path))
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(File.ReadAllText(path));
            for (int i = 0; i < data.entries.Count; i++)
            {
                textRankList.text += $"第 {i + 1} 名: {data.entries[i].username} - {data.entries[i].score} 分\n";
            }
        }
        else
        {
            textRankList.text += "暂无记录，快来创造第一个高分吧！";
        }
    }

    // ==========================================
    // 辅助工具方法
    // ==========================================
    public void AddScore(int points)
    {
        currentMatchScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        textScore.text = "当前得分: " + currentMatchScore;
    }

    private void SetPlayerControl(bool canControl)
    {
        // 遍历禁用/启用角色控制脚本
        foreach (var script in playerControlScripts)
        {
            if (script != null) script.enabled = canControl;
        }

        // 控制鼠标显示与锁定
        Cursor.visible = !canControl;
        Cursor.lockState = canControl ? CursorLockMode.Locked : CursorLockMode.None;
    }
}