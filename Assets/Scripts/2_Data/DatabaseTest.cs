using UnityEngine;
using System.IO;
using System.Data;
using Mono.Data.Sqlite;
using System;

public class DatabaseTest : MonoBehaviour
{
    void Start()
    {
        // 1. 获取 Unity 允许我们自由读写的本地持久化路径
        string dbPath = Path.Combine(Application.persistentDataPath, "TestDB.sqlite");
        string connectionString = "URI=file:" + dbPath;

        Debug.Log("📂 【目标数据库路径】目标数据库路径: " + dbPath);

        try
        {
            // 2. 尝试建立连接并打开
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                // 3. 尝试执行一句最基础的 SQL 指令：建一个测试表
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS HelloTable (id INTEGER PRIMARY KEY, message TEXT)";
                    cmd.ExecuteNonQuery();
                }
            }
            // 如果代码能安全走到这里，说明 DLL 环境 100% 完美！
            Debug.Log("✅ 【连接成功】数据库连接完美！建表成功！Unity 引擎环境无懈可击！");
        }
        catch (Exception e)
        {
            // 如果 DLL 没装好，或者路径没有写入权限，这里会爆红
            Debug.LogError("❌ 【连接失败】环境配置有问题，报错信息: " + e.Message);
        }
    }
}