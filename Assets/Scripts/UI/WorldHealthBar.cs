using UnityEngine;
using System.Collections.Generic;

/// <summary>Draws a small world-space health bar above a sprite without a Canvas.</summary>
public class WorldHealthBar : MonoBehaviour
{
    private const float BarWidth = 1.05f;
    private const float BarHeight = 0.075f;
    private const float VerticalPadding = 0.16f;

    private static Sprite solidSprite;
    private static readonly List<WorldHealthBar> activeBars = new List<WorldHealthBar>();
    private static bool allBarsVisible = true;
    private SpriteRenderer anchorRenderer;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;
    private float healthRatio = 1f;
    private float verticalOffset = VerticalPadding;
    private bool shieldStyle;
    private bool visibilityRequested = true;

    private void Awake()
    {
        anchorRenderer = GetComponent<SpriteRenderer>();
        if (anchorRenderer == null)
            anchorRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (anchorRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer found for health bar on {name}.", this);
            enabled = false;
            return;
        }

        EnsureSprite();
        CreateBarPart("HealthBarBackground", out backgroundRenderer);
        CreateBarPart("HealthBarFill", out fillRenderer);
        backgroundRenderer.color = new Color(0f, 0f, 0f, 0.85f);
        SetHealth(1f, 1f);
    }

    private void OnEnable()
    {
        if (!activeBars.Contains(this)) activeBars.Add(this);
        ApplyVisibility();
    }

    private void OnDisable()
    {
        activeBars.Remove(this);
    }

    public static void SetAllVisible(bool visible)
    {
        allBarsVisible = visible;
        for (int i = activeBars.Count - 1; i >= 0; i--)
        {
            if (activeBars[i] == null)
                activeBars.RemoveAt(i);
            else
                activeBars[i].ApplyVisibility();
        }
    }

    public static bool AreAllVisible => allBarsVisible;

    public void SetHealth(float currentHealth, float maxHealth)
    {
        healthRatio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;
        if (fillRenderer == null) return;

        UpdateFillColor();
        UpdateFillScale();
    }

    public void SetVerticalOffset(float offset)
    {
        verticalOffset = offset;
    }

    public void SetShieldStyle()
    {
        shieldStyle = true;
        UpdateFillColor();
    }

    public void SetVisible(bool visible)
    {
        visibilityRequested = visible;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        bool visible = visibilityRequested && allBarsVisible;
        if (backgroundRenderer != null) backgroundRenderer.enabled = visible;
        if (fillRenderer != null) fillRenderer.enabled = visible;
    }

    private void LateUpdate()
    {
        if (anchorRenderer == null) return;

        float scaleY = Mathf.Max(Mathf.Abs(transform.lossyScale.y), 0.0001f);
        float localY = (anchorRenderer.bounds.max.y + verticalOffset - transform.position.y) / scaleY;
        backgroundRenderer.transform.localPosition = new Vector3(0f, localY, -0.1f);
        fillRenderer.transform.localPosition = new Vector3(0f, localY, -0.11f);
        UpdateFillScale();
    }

    private void CreateBarPart(string objectName, out SpriteRenderer spriteRenderer)
    {
        GameObject barPart = new GameObject(objectName);
        barPart.transform.SetParent(transform, false);
        spriteRenderer = barPart.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = solidSprite;
        spriteRenderer.sortingLayerID = anchorRenderer.sortingLayerID;
        spriteRenderer.sortingOrder = Mathf.Max(anchorRenderer.sortingOrder + 10, 20);
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
    }

    private void UpdateFillScale()
    {
        if (fillRenderer == null) return;

        float scaleX = 1f / Mathf.Max(Mathf.Abs(transform.lossyScale.x), 0.0001f);
        float scaleY = 1f / Mathf.Max(Mathf.Abs(transform.lossyScale.y), 0.0001f);
        float fillWidth = BarWidth * healthRatio;

        backgroundRenderer.transform.localScale = new Vector3(BarWidth * 1.08f * scaleX, BarHeight * 1.5f * scaleY, 1f);
        fillRenderer.transform.localScale = new Vector3(fillWidth * scaleX, BarHeight * scaleY, 1f);
        fillRenderer.transform.localPosition = new Vector3(
            -(BarWidth - fillWidth) * 0.5f * scaleX,
            fillRenderer.transform.localPosition.y,
            -0.11f);
    }

    private void UpdateFillColor()
    {
        if (fillRenderer == null) return;
        if (shieldStyle)
        {
            fillRenderer.color = healthRatio > 0.3f
                ? new Color(0.2f, 0.75f, 1f)
                : new Color(0.55f, 0.35f, 1f);
            return;
        }

        fillRenderer.color = healthRatio > 0.6f
            ? new Color(0.2f, 0.9f, 0.2f)
            : healthRatio > 0.3f
                ? new Color(1f, 0.8f, 0.1f)
                : new Color(1f, 0.2f, 0.15f);
    }

    private static void EnsureSprite()
    {
        if (solidSprite != null) return;

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "WorldHealthBarPixel",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        solidSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        solidSprite.name = "WorldHealthBarSolidSprite";
    }
}
