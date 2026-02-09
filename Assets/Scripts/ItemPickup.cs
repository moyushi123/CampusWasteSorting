using UnityEngine;
using UnityEngine.InputSystem; // 这一行很关键，因为你用了 Starter Assets
using UnityEngine.UI; // <--- 改了这里：引用旧版UI系统

public class ItemPickup : MonoBehaviour
{
    [Header("设置")]
    public string tipText = "按 [F] 捡起垃圾"; // 你想显示的提示文字
    public int scoreValue = 10; // 这个垃圾值多少分

    [Header("UI组件 (把屏幕上的文字拖进来)")]
    public Text uiText; // <--- 改了这里：变成了普通的 Text // 屏幕上的提示文字框

    private bool isPlayerNearby = false; // 记录玩家是否在附近

    void Start()
    {
        // 游戏刚开始时，先隐藏提示文字（如果有的话）
        if (uiText != null) uiText.text = "";
    }

    void Update()
    {
        // 如果玩家在附近 并且 按下了 F 键
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    void PickUp()
    {
        Debug.Log("你捡到了垃圾！"); // 在控制台输出一句话

        // 1. 隐藏提示文字
        if (uiText != null) uiText.text = "";

        // 2. 在这里可以写加分的逻辑（我们之后再写）

        // 3. 销毁这个垃圾物体（让它从世界上消失）
        Destroy(gameObject);
    }

    // 当有人走进感应区
    private void OnTriggerEnter(Collider other)
    {
        // 检查撞到我的是不是玩家（Starter Assets 的标签通常是 Player）
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (uiText != null) uiText.text = tipText; // 显示提示
        }
    }

    // 当有人离开感应区
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (uiText != null) uiText.text = ""; // 隐藏提示
        }
    }
}