using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 使用新输入系统

public class TrashPickup : MonoBehaviour
{
    private bool canPickup = false;
    private Text promptText; // 借用你的开门提示文字

    void Start()
    {
        // ⚠️ 极其重要的一步：让生成的垃圾自己去场景里找到那个 UI 文本
        // 请务必把下面的 "这里填你UI物体的名字" 替换成你在 Hierarchy 里那个真实存在的 Text 物体名字！
        GameObject uiObj = GameObject.Find("Text");

        if (uiObj != null)
        {
            promptText = uiObj.GetComponent<Text>();
        }
        else
        {
            Debug.LogWarning("没找到UI提示物体，请检查名字是否拼写正确！");
        }
    }

    void Update()
    {
        // 如果玩家在范围内 + 按下了 F 键
        if (canPickup && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 1. 隐藏提示文字
            if (promptText != null) promptText.text = "";

            // 2. 依然用控制台打印，确保逻辑跑通
            Debug.Log("成功拾取！等以后再做计分板 UI");

            // 3. 销毁垃圾模型
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 当玩家走进垃圾的感应圈
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            if (promptText != null) promptText.text = "按 [F] 拾取垃圾";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 当玩家离开感应圈
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            if (promptText != null) promptText.text = ""; // 离开时清空屏幕文字
        }
    }
}