using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 必须引用这个，因为你用了 Starter Assets

public class DoorController : MonoBehaviour
{
    [Header("把 Legacy Text 拖到这里")]
    public Text uiText;

    private Animator anim;
    private bool isPlayerNearby = false;
    private bool isOpen = false;

    void Start()
    {
        anim = GetComponent<Animator>(); // 自动找到门身上的动画组件
        if (uiText != null) uiText.text = ""; // 游戏开始先清空提示
    }

    void Update()
    {
        // 条件：人在附近 + 门没开 + 按了F键
        if (isPlayerNearby && !isOpen && Keyboard.current.fKey.wasPressedThisFrame)
        {
            // 核心命令：直接强行播放名为 "DoorOpen" 的状态！
            anim.Play("DoorOpen");

            isOpen = true; // 标记门开了，以后再按F就不理你了
            if (uiText != null) uiText.text = ""; // 隐藏提示字
        }
    }

    // 走进感应区
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = true;
            if (uiText != null) uiText.text = "按 [F] 开门";
        }
    }

    // 离开感应区
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (uiText != null) uiText.text = "";
        }
    }
}