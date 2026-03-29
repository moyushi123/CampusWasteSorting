using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using System.Data;
using Mono.Data.Sqlite;

public enum TrashCategory
{
    可回收物,
    有害垃圾,
    厨余垃圾,
    其他垃圾
}

public class ClassificationUIManager : MonoBehaviour
{
    public static ClassificationUIManager Instance;

    [Header("玩家输入控制")]
    public StarterAssetsInputs playerInputs;

    [Header("UI 面板引用")]
    public GameObject classificationPanel;
    public Text trashNameTitle;
    public Text feedbackText;

    [Header("分类按钮引用")]
    public Button btnRecyclable;
    public Button btnHazardous;
    public Button btnFood;
    public Button btnOther;

    [Header("确认按钮")]
    public Button btnConfirm;

    private string currentTrashName;
    private TrashCategory currentCorrectCategory;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        classificationPanel.SetActive(false);
        feedbackText.text = "";

        btnConfirm.gameObject.SetActive(false);

        btnRecyclable.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.可回收物));
        btnHazardous.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.有害垃圾));
        btnFood.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.厨余垃圾));
        btnOther.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.其他垃圾));

        btnConfirm.onClick.AddListener(ClosePanel);
    }

    public void OpenClassificationPanel(string trashName, TrashCategory correctCategory)
    {
        if (playerInputs != null)
        {
            playerInputs.cursorInputForLook = false;
        }

        currentTrashName = trashName;
        currentCorrectCategory = correctCategory;

        trashNameTitle.text = "请对【" + currentTrashName + "】进行分类：";
        feedbackText.text = "";

        btnConfirm.gameObject.SetActive(false);

        Time.timeScale = 0f;
        classificationPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnCategoryButtonClicked(TrashCategory chosenCategory)
    {
        SetButtonsInteractable(false);

        if (chosenCategory == currentCorrectCategory)
        {
            feedbackText.color = Color.green;
            feedbackText.text = "✅ 判定成功！加 10 分！";

            if (SystemLogger.Instance != null)
            {
                SystemLogger.Instance.AddLog($"[玩家] 成功将 [{currentTrashName}] 分入 {currentCorrectCategory}，得分 +10");
            }

            // 通知 GameManager 增加当局得分
            if (GameManager.Instance != null) GameManager.Instance.AddScore(10);

            // 汇报看板：答对了
            if (DashboardManager.Instance != null) DashboardManager.Instance.RecordResult(currentCorrectCategory, true);
        }
        else
        {
            // 汇报看板：答错了
            if (DashboardManager.Instance != null) DashboardManager.Instance.RecordResult(currentCorrectCategory, false);

            feedbackText.color = Color.red;
            feedbackText.text = "❌ 判定失败！【" + currentTrashName + "】不属于【" + chosenCategory.ToString() + "】哦！\n已记录至错题本，扣除 10 分。";

            if (SystemLogger.Instance != null)
            {
                SystemLogger.Instance.AddLog($"[玩家] 警告！错误地将 [{currentTrashName}] 放入 {chosenCategory}，扣除 10 分");
            }

            // 通知 GameManager 扣除当局得分
            if (GameManager.Instance != null) GameManager.Instance.AddScore(-10);

            // 🌟 核心改造：直连 SQLite 处理错题本
            if (GameSession.IsLoggedIn)
            {
                using (var conn = DatabaseManager.GetConnection())
                {
                    conn.Open();

                    // 第一步：查询数据库中是否已有该垃圾的错题记录
                    long count = 0;
                    using (var checkCmd = conn.CreateCommand())
                    {
                        checkCmd.CommandText = "SELECT COUNT(*) FROM WrongBookTable WHERE username = @user AND trashName = @trash";
                        checkCmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                        checkCmd.Parameters.AddWithValue("@trash", currentTrashName);
                        count = (long)checkCmd.ExecuteScalar();
                    }

                    // 第二步：根据查询结果决定是 UPDATE（更新次数）还是 INSERT（新建记录）
                    using (var actionCmd = conn.CreateCommand())
                    {
                        if (count > 0)
                        {
                            // 以前错过，直接让数据库里的 errorCount 字段 + 1
                            actionCmd.CommandText = "UPDATE WrongBookTable SET errorCount = errorCount + 1 WHERE username = @user AND trashName = @trash";
                        }
                        else
                        {
                            // 第一次分错，插入全新数据
                            actionCmd.CommandText = "INSERT INTO WrongBookTable (username, trashName, correctCategory, errorCount) VALUES (@user, @trash, @cat, 1)";
                            actionCmd.Parameters.AddWithValue("@cat", currentCorrectCategory.ToString());
                        }

                        actionCmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                        actionCmd.Parameters.AddWithValue("@trash", currentTrashName);
                        actionCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        btnConfirm.gameObject.SetActive(true);
    }

    private void ClosePanel()
    {
        if (playerInputs != null)
        {
            playerInputs.cursorInputForLook = true;
        }

        Time.timeScale = 1f;
        classificationPanel.SetActive(false);
        SetButtonsInteractable(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetButtonsInteractable(bool state)
    {
        btnRecyclable.interactable = state;
        btnHazardous.interactable = state;
        btnFood.interactable = state;
        btnOther.interactable = state;
    }
}