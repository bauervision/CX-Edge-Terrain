using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{

    // create the local instance of this class
    private static Tooltip instance;
    public Camera uiCamera;
    public Text tooltipText;
    public RectTransform bgRect;

    private int xOffset = 90;
    private int yOffset = 10;

    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        instance.gameObject.SetActive(false);
    }

    public static void ShowToolTip(string tooltipString)
    {
        instance.gameObject.SetActive(true);// use the local instance
        instance.tooltipText.text = tooltipString;
        float textPadding = 8f;
        Vector2 bgSize = new Vector2(instance.tooltipText.preferredWidth + textPadding * 2f, instance.tooltipText.preferredHeight + textPadding * 2f);
        instance.bgRect.sizeDelta = bgSize;

    }



    public static void HideToolTip()
    {
        instance.gameObject.SetActive(false);

    }

    private void Update()
    {
        Vector2 localPoint;

        Vector3 mouseOffset = new Vector3(Input.mousePosition.x + xOffset, Input.mousePosition.y - yOffset, Input.mousePosition.z);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), mouseOffset, uiCamera, out localPoint);
        transform.localPosition = localPoint;
    }
}
