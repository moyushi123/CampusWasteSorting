using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class SystemLogger : MonoBehaviour
{
    public static SystemLogger Instance; // 单例模式，随叫随到

    [Header("UI 绑定")]
    public Text logTextUI; // 拖入刚才建的 LogText

    [Header("设置")]
    public int maxLogLines = 10; // 屏幕上最多显示几行，防止卡顿

    private Queue<string> logQueue = new Queue<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 任何脚本只要调用这个方法，就能在屏幕上打日志！
    public void AddLog(string message)
    {
        // 自动加上当前的时间戳 [14:30:22]
        string timeStamp = DateTime.Now.ToString("HH:mm:ss");
        string finalMessage = $"[{timeStamp}] {message}";

        logQueue.Enqueue(finalMessage);

        // 如果超过最大行数，就把最老的那条删掉
        if (logQueue.Count > maxLogLines)
        {
            logQueue.Dequeue();
        }

        // 更新 UI 面板
        UpdateUIText();
    }

    private void UpdateUIText()
    {
        if (logTextUI != null)
        {
            logTextUI.text = string.Join("\n", logQueue);
        }
    }
}