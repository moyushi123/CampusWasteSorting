using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;

public class DashboardManager : MonoBehaviour
{
    public static DashboardManager Instance;

    [Header("UI 进度条引用")]
    public Image barRecyclable;
    public Image barHazardous;
    public Image barFood;
    public Image barResidual;

    [Header("UI 百分比文字引用")]
    public Text textRecyclable;
    public Text textHazardous;
    public Text textFood;
    public Text textResidual;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 游戏一开始，直接从数据库把该玩家的历史正确率全拉出来显示！
        RefreshAllDashboards();
    }

    // ==========================================
    // 核心写入：每次答题结束，记录结果并立刻存入 SQLite
    // ==========================================
    public void RecordResult(TrashCategory category, bool isCorrect)
    {
        if (!GameSession.IsLoggedIn) return; // 如果没登录，不记录

        string catName = category.ToString();
        int addCorrect = isCorrect ? 1 : 0;
        int addError = isCorrect ? 0 : 1;

        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();

            // 1. 先检查该玩家在这个分类下是否已经有数据了
            long count = 0;
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM CategoryStatsTable WHERE username = @user AND category = @cat";
                checkCmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                checkCmd.Parameters.AddWithValue("@cat", catName);
                count = (long)checkCmd.ExecuteScalar();
            }

            using (var actionCmd = conn.CreateCommand())
            {
                if (count > 0)
                {
                    // 2. 如果有，直接 UPDATE 累加次数
                    actionCmd.CommandText = "UPDATE CategoryStatsTable SET correctCount = correctCount + @c, errorCount = errorCount + @e WHERE username = @user AND category = @cat";
                }
                else
                {
                    // 3. 如果没有，INSERT 一条新记录
                    actionCmd.CommandText = "INSERT INTO CategoryStatsTable (username, category, correctCount, errorCount) VALUES (@user, @cat, @c, @e)";
                }

                actionCmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);
                actionCmd.Parameters.AddWithValue("@cat", catName);
                actionCmd.Parameters.AddWithValue("@c", addCorrect);
                actionCmd.Parameters.AddWithValue("@e", addError);
                actionCmd.ExecuteNonQuery();
            }
        }

        // 数据写入完毕，通知 UI 立刻刷新！
        RefreshAllDashboards();
    }

    // ==========================================
    // 核心读取：从 SQLite 拉取真实数据并刷新 UI
    // ==========================================
    private void RefreshAllDashboards()
    {
        if (!GameSession.IsLoggedIn) return;

        // 每次刷新前，先全部归零（防止查不到数据的分类显示上次残留）
        UpdateSingleBar(barRecyclable, textRecyclable, 0, 0);
        UpdateSingleBar(barHazardous, textHazardous, 0, 0);
        UpdateSingleBar(barFood, textFood, 0, 0);
        UpdateSingleBar(barResidual, textResidual, 0, 0);

        using (var conn = DatabaseManager.GetConnection())
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT category, correctCount, errorCount FROM CategoryStatsTable WHERE username = @user";
                cmd.Parameters.AddWithValue("@user", GameSession.CurrentUsername);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string catName = reader.GetString(0);
                        int correct = reader.GetInt32(1);
                        int error = reader.GetInt32(2);

                        // 根据数据库读出的名字，分配给对应的 UI 进度条
                        if (catName == TrashCategory.可回收物.ToString())
                            UpdateSingleBar(barRecyclable, textRecyclable, correct, error);
                        else if (catName == TrashCategory.有害垃圾.ToString())
                            UpdateSingleBar(barHazardous, textHazardous, correct, error);
                        else if (catName == TrashCategory.厨余垃圾.ToString())
                            UpdateSingleBar(barFood, textFood, correct, error);
                        else if (catName == TrashCategory.其他垃圾.ToString())
                            UpdateSingleBar(barResidual, textResidual, correct, error);
                    }
                }
            }
        }
    }

    // ==========================================
    // UI 计算与渲染逻辑（原汁原味保留你的出色算法）
    // ==========================================
    private void UpdateSingleBar(Image bar, Text percentText, int correctCount, int errorCount)
    {
        if (bar == null) return;

        int totalAttempts = correctCount + errorCount;

        if (totalAttempts == 0)
        {
            bar.fillAmount = 0f;
            if (percentText != null) percentText.text = "0%";
            return;
        }

        float accuracyRate = (float)correctCount / totalAttempts;
        bar.fillAmount = accuracyRate;

        if (percentText != null)
        {
            int percent = Mathf.RoundToInt(accuracyRate * 100);
            percentText.text = percent + "%";
        }
    }
}