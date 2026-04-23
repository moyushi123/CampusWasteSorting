using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;
using System;
using System.Data;
using Mono.Data.Sqlite;

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

    [Header("音乐控制")]
    public AudioSource bgmSource;
    public AudioClip normalMusic;
    public AudioClip challengeMusic;

    [Header("NPC 控制群组")]
    public NPCPatrol[] allNPCs;

    [Header("当局得分")]
    public int currentMatchScore = 0;
    private bool isGameActive = false;
    private bool isChallengeMode = false;
    private float timeLeft = 180f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        mainMenuUI.SetActive(true);
        gameplayUI.SetActive(false);
        subPanelsUI.SetActive(false);
        settlementUI.SetActive(false);

        SetPlayerControl(false);

        btnChallengeMode.onClick.AddListener(StartChallengeMode);
        btnFreeMode.onClick.AddListener(StartFreeMode);
        btnRules.onClick.AddListener(() => ShowSubPanel(panelRules));
        btnWrongBook.onClick.AddListener(ShowWrongBookData);
        btnLeaderboard.onClick.AddListener(ShowLeaderboardData);

        btnCloseRules.onClick.AddListener(CloseAllSubPanels);
        btnCloseWrongBook.onClick.AddListener(CloseAllSubPanels);
        btnCloseLeaderboard.onClick.AddListener(CloseAllSubPanels);

        btnExitFreeMode.onClick.AddListener(EndMatch);
        btnBackToMainMenu.onClick.AddListener(RestartScene);
    }

    void Update()
    {
        if (!isGameActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (isGameActive)
        {
            if (isChallengeMode)
            {
                timeLeft -= Time.deltaTime;
                textTimer.text = "时间: " + Mathf.CeilToInt(timeLeft).ToString() + "s";

                if (timeLeft <= 0)
                {
                    timeLeft = 0;
                    EndMatch();
                }
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EndMatch();
            }
        }
    }

    private void StartChallengeMode()
    {
        isChallengeMode = true;
        timeLeft = 180f;
        textTimer.text = "时间: 180s";
        btnExitFreeMode.gameObject.SetActive(false);

        if (bgmSource != null && challengeMusic != null)
        {
            bgmSource.clip = challengeMusic;
            bgmSource.Play();
        }

        BeginGameplay();
    }

    private void StartFreeMode()
    {
        isChallengeMode = false;
        textTimer.text = "模式: 自由游览";
        btnExitFreeMode.gameObject.SetActive(true);
        BeginGameplay();
    }

    private void BeginGameplay()
    {
        currentMatchScore = 0;
        UpdateScoreUI();

        mainMenuUI.SetActive(false);
        gameplayUI.SetActive(true);

        SetPlayerControl(true);
        isGameActive = true;

        if (allNPCs != null && allNPCs.Length > 0)
        {
            foreach (var npc in allNPCs)
            {
                if (npc != null) npc.enabled = true;
            }
        }
    }

    private void EndMatch()
    {
        isGameActive = false;

        // 冻结游戏时间
        Time.timeScale = 0f;

        // 剥夺玩家控制权，呼出鼠标
        SetPlayerControl(false);

        // 切换 UI 面板
        gameplayUI.SetActive(false);
        settlementUI.SetActive(true);

        // 停止所有 NPC 的巡逻
        if (allNPCs != null && allNPCs.Length > 0)
        {
            foreach (var npc in allNPCs)
            {
                if (npc != null) npc.enabled = false;
            }
        }

        // 结算面板文字更新
        textGameOverTitle.text = isChallengeMode ? "挑战结束！" : "游览结束！";
        textFinalScore.text = "本次得分: " + currentMatchScore;

        // ==========================================
        // 核心：带有防并发事务控制的数据库写入逻辑
        // ==========================================
        if (GameSession.IsLoggedIn)
        {
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();

                // 【开启事务】如果中途发生异常，数据会自动回滚，保证 ACID 特性
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. 先查出之前的历史总分
                        int currentTotal = 0;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction; // 绑定事务
                            cmd.CommandText = "SELECT totalScore FROM UserTable WHERE username = @user";
                            cmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                currentTotal = Convert.ToInt32(result);
                            }
                        }

                        // 2. 加上本局得分，存回数据库
                        int newTotal = currentTotal + currentMatchScore;
                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = transaction; // 绑定事务
                            updateCmd.CommandText = "UPDATE UserTable SET totalScore = @score WHERE username = @user";
                            updateCmd.Parameters.AddWithValue("@score", newTotal);
                            updateCmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                            updateCmd.ExecuteNonQuery();
                        }

                        // 3. 【提交事务】确认无误，将所有更改写入数据库
                        transaction.Commit();
                        Debug.Log("✅ 事务提交成功：本局分数已安全累加至数据库！");
                    }
                    catch (Exception ex)
                    {
                        // 4. 【回滚事务】一旦发生任何异常，立刻撤销刚才的操作，保护数据库完整性
                        transaction.Rollback();
                        Debug.LogError("❌ 写入失败，事务已回滚！错误信息：" + ex.Message);
                    }
                }
            }

            textHighscoreTip.text = isChallengeMode ? "分数已记录至数据库，并在总分中累加！" : "自由模式分数已累计！";
        }
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

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

    // 🌟 核心改造：从 SQLite 中查询错题本
    private void ShowWrongBookData()
    {
        ShowSubPanel(panelWrongBook);
        textWrongList.text = "【我的错题记录】\n\n";

        if (GameSession.IsLoggedIn)
        {
            bool hasRecord = false;
            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT trashName, correctCategory, errorCount FROM WrongBookTable WHERE username = @user";
                    cmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hasRecord = true;
                            string tName = reader.GetString(0);
                            string cCat = reader.GetString(1);
                            int eCount = reader.GetInt32(2);
                            textWrongList.text += $"- 误判【{tName}】，它应属于【{cCat}】 (错 {eCount} 次)\n";
                        }
                    }
                }
            }

            if (!hasRecord)
            {
                textWrongList.text += "太棒了！你目前没有任何错题记录。";
            }
        }
        else
        {
            textWrongList.text += "未获取到玩家数据...";
        }
    }

    // 🌟 核心改造：利用 SQL 语句实现极其高贵的排行榜排序
    private void ShowLeaderboardData()
    {
        ShowSubPanel(panelLeaderboard);
        textRankList.text = "【数据库总分排行榜 (前五名)】\n\n";

        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // 一句 SQL 直接完成降序排列并只取前 5 个
                cmd.CommandText = "SELECT username, totalScore FROM UserTable ORDER BY totalScore DESC LIMIT 5";

                using (var reader = cmd.ExecuteReader())
                {
                    int rank = 1;
                    bool hasData = false;
                    while (reader.Read())
                    {
                        hasData = true;
                        string uName = reader.GetString(0);
                        int tScore = reader.GetInt32(1);
                        textRankList.text += $"第 {rank} 名: {uName} - {tScore} 分\n";
                        rank++;
                    }

                    if (!hasData)
                    {
                        textRankList.text += "暂无记录，快来创造第一个高分吧！";
                    }
                }
            }
        }
    }

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
        foreach (var script in playerControlScripts)
        {
            if (script != null) script.enabled = canControl;
        }

        Cursor.visible = !canControl;
        Cursor.lockState = canControl ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void ReturnToLogin()
    {
        Time.timeScale = 1f;
        GameSession.Logout(); // 🌟 撕毁通行证
        SceneManager.LoadScene("LoginScene");
    }

    // 🌟 核心改造：从数据库直接拉取数据生成 CSV 报表
    public void ExportReportToDesktop()
    {
        if (!GameSession.IsLoggedIn) return;

        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "校园环保卫士_成绩单_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            string filePath = Path.Combine(desktopPath, fileName);

            using (var conn = DatabaseManager.GetConnection())
            {
                conn.Open();

                // 1. 获取总分
                int totalScore = 0;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT totalScore FROM UserTable WHERE username = @user";
                    cmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value) totalScore = Convert.ToInt32(res);
                }

                // 2. 写入文件
                using (StreamWriter sw = new StreamWriter(filePath, false, new System.Text.UTF8Encoding(true)))
                {
                    sw.WriteLine("玩家账号,总得分,导出时间");
                    sw.WriteLine($"{GameSession.CurrentUsername},{totalScore},{DateTime.Now}");
                    sw.WriteLine("");
                    sw.WriteLine("错题本记录");
                    sw.WriteLine("垃圾名称,正确分类,错误次数");

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT trashName, correctCategory, errorCount FROM WrongBookTable WHERE username = @user";
                        cmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sw.WriteLine($"{reader.GetString(0)},{reader.GetString(1)},{reader.GetInt32(2)}");
                            }
                        }
                    }
                }
            }
            Debug.Log("✅ 成绩单已成功导出至桌面：" + filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("导出失败：" + e.Message);
        }
    }
}