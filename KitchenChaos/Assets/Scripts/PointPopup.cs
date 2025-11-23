using UnityEngine;
using TMPro;

public class PointPopup : MonoBehaviour
{
    public float lifetime = 1.5f;
    public float floatSpeed = 60f;
    public Vector2 startOffset = new Vector2(0f, 30f);

    TextMeshProUGUI text;
    RectTransform rect;
    float timer;

    public void Initialize(int points, Vector3 worldPosition, Canvas canvas)
    {
        text = GetComponent<TextMeshProUGUI>();
        if (!text) text = GetComponentInChildren<TextMeshProUGUI>(true);

        rect = GetComponent<RectTransform>();
        if (!rect) rect = gameObject.AddComponent<RectTransform>();

        if (text)
        {
            text.text = $"+{points}";
            var c = text.color; c.a = 1f; text.color = c;
            text.raycastTarget = false;
        }

        Vector2 screenPos = Camera.main ? (Vector2)Camera.main.WorldToScreenPoint(worldPosition)
                                        : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 local;
        bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCam, out local);
        if (!ok) local = Vector2.zero;

        Vector2 half = canvasRect.rect.size * 0.5f;
        local.x = Mathf.Clamp(local.x, -half.x + 10f, half.x - 10f);
        local.y = Mathf.Clamp(local.y, -half.y + 10f, half.y - 10f);

        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = local + startOffset;
        rect.localScale = Vector3.one;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!rect) return;

        rect.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (text && timer > lifetime * 0.5f)
        {
            float t = Mathf.InverseLerp(lifetime * 0.5f, lifetime, timer);
            var c = text.color; c.a = Mathf.Lerp(1f, 0f, t); text.color = c;
        }
    }
}
