using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TrashPickup : MonoBehaviour
{
    [Header("垃圾属性配置")]
    public string trashName = "香蕉皮"; // 在 Inspector 中配置垃圾名称
    public TrashCategory correctCategory = TrashCategory.厨余垃圾; // 在 Inspector 中选择正确分类

    private bool canPickup = false;
    private Text promptText;

    void Start()
    {
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
        // 如果玩家在范围内 + 按下了 F 键 + 当前时间没有被冻结（防止在UI弹出的情况下继续按F）
        if (canPickup && Keyboard.current.fKey.wasPressedThisFrame && Time.timeScale > 0f)
        {
            if (promptText != null) promptText.text = "";

            // 核心交互：呼叫 UI 管理器弹出面板，并把自己的名字和正确分类传过去
            if (ClassificationUIManager.Instance != null)
            {
                ClassificationUIManager.Instance.OpenClassificationPanel(trashName, correctCategory);
            }
            else
            {
                Debug.LogError("场景中没有找到 ClassificationUIManager！");
            }

            // 将自己销毁（因为已经捡起来开始判定了）
            //Destroy(gameObject);

            // ⬅️ 新架构：不再销毁，而是直接隐藏，实现“秒速回收回池子”
            gameObject.SetActive(false);
            canPickup = false; // 保险起见，重置一下它的交互状态
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            if (promptText != null) promptText.text = "按 [F] 拾取 " + trashName;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            if (promptText != null) promptText.text = "";
        }
    }
}