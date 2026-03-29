using UnityEngine;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;

// 同样不继承 MonoBehaviour，纯静态工具人，绝对不会干扰你的游戏场景！
public static class DatabaseManager
{
    private static string dbPath;

    // 供其他脚本获取数据库连接
    public static SqliteConnection GetConnection()
    {
        if (string.IsNullOrEmpty(dbPath))
        {
            dbPath = "URI=file:" + Path.Combine(Application.persistentDataPath, "CampusEcoDB.sqlite");
        }
        return new SqliteConnection(dbPath);
    }

    // 初始化数据库与建表（只需在游戏刚启动时叫它一次就行）
    public static void InitializeDatabase()
    {
        using (var conn = GetConnection())
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // 1. 创建玩家主表
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UserTable (
                        username TEXT PRIMARY KEY,
                        totalScore INTEGER DEFAULT 0
                    );";
                cmd.ExecuteNonQuery();

                // 2. 创建错题本副表
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS WrongBookTable (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT,
                        trashName TEXT,
                        correctCategory TEXT,
                        errorCount INTEGER DEFAULT 0,
                        FOREIGN KEY(username) REFERENCES UserTable(username)
                    );";
                cmd.ExecuteNonQuery();

                // 3. 创建分类统计看板专用表 (新增)
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS CategoryStatsTable (
                        username TEXT,
                        category TEXT,
                        correctCount INTEGER DEFAULT 0,
                        errorCount INTEGER DEFAULT 0,
                        PRIMARY KEY (username, category)
                    );";
                cmd.ExecuteNonQuery();
            }
        }
        Debug.Log("🗄️ 【数据库管家】 底层表结构初始化完毕！随时待命！");
    }
}