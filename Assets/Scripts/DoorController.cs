using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour
{
    [Header("把 Legacy Text 拖到这里")]
    public Text uiText;

    private Animator anim;
    private bool isPlayerNearby = false;
    private bool isOpen = false; // 记录门当前是不是开着的

    void Start()
    {
        anim = GetComponent<Animator>();
        if (uiText != null) uiText.text = "";
    }

    void Update()
    {
        // 只要人在附近 + 按了F (去掉了 !isOpen 的限制，允许反复按)
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isOpen)
            {
                // === 如果门是开着的，就执行关门 ===
                anim.Play("DoorClose"); // 播放关门动画
                isOpen = false;         // 标记状态为“关”
                UpdateUI();             // 更新提示文字
            }
            else
            {
                // === 如果门是关着的，就执行开门 ===
                anim.Play("DoorOpen");  // 播放开门动画
                isOpen = true;          // 标记状态为“开”
                UpdateUI();             // 更新提示文字
            }
        }
    }

    // 一个小函数，用来根据门的状态自动切换提示文字
    void UpdateUI()
    {
        if (uiText != null && isPlayerNearby)
        {
            if (isOpen)
                uiText.text = "按 [F] 关门";
            else
                uiText.text = "按 [F] 开门";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            UpdateUI(); // 走进感应区时，立刻显示正确的提示
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (uiText != null) uiText.text = ""; // 离开时清空文字
        }
    }
}