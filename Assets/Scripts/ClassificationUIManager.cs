using UnityEngine;
using UnityEngine.UI;

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
    public Button btnConfirm; // ⬅️ 新增：确认按钮的引用

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

        // 游戏一开始，先隐藏确认按钮
        btnConfirm.gameObject.SetActive(false);

        // 绑定四个分类按钮的点击事件
        btnRecyclable.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.可回收物));
        btnHazardous.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.有害垃圾));
        btnFood.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.厨余垃圾));
        btnOther.onClick.AddListener(() => OnCategoryButtonClicked(TrashCategory.其他垃圾));

        // ⬅️ 新增：绑定确认按钮的点击事件
        btnConfirm.onClick.AddListener(ClosePanel);
    }

    public void OpenClassificationPanel(string trashName, TrashCategory correctCategory)
    {
        currentTrashName = trashName;
        currentCorrectCategory = correctCategory;

        trashNameTitle.text = "请对【" + currentTrashName + "】进行分类：";
        feedbackText.text = "";

        // 每次打开面板时，确保确认按钮是隐藏的
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
        }
        else
        {
            feedbackText.color = Color.red;
            feedbackText.text = "❌ 判定失败！【" + currentTrashName + "】不属于【" + chosenCategory.ToString() + "】哦！\n已记录至错题本，扣除 10 分。";
        }

        // ⬅️ 修改点：不再使用协程延迟，而是直接显示“确定”按钮让玩家点击
        btnConfirm.gameObject.SetActive(true);
    }

    // ⬅️ 新增：点击确定按钮后执行的方法
    private void ClosePanel()
    {
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