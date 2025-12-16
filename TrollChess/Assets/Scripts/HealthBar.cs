using UnityEngine;

[RequireComponent(typeof(Unit))]
public class HealthBar : MonoBehaviour // ← MonoBehavior обязательно!
{
    private Unit unit;
    private GameObject barGO;
    private SpriteRenderer barRenderer;
    private float maxHealth;

    public void Init(int maxHealth)
    {
        this.maxHealth = maxHealth;
        unit = GetComponent<Unit>();

        barGO = new GameObject("HealthBar");
        barGO.transform.parent = transform;
        barGO.transform.localPosition = Vector3.up * 0.5f;
        barGO.transform.localScale = new Vector3(0.8f, 0.1f, 1f);

        barRenderer = barGO.AddComponent<SpriteRenderer>();
        barRenderer.color = Color.green;
        barRenderer.sortingOrder = 15;
        // Создаём белый спрайт для полоски
        Texture2D whiteTex = Texture2D.whiteTexture;
        Sprite barSprite = Sprite.Create(whiteTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        barRenderer.sprite = barSprite;
    }

    void Update()
    {
        if (unit == null || barRenderer == null) return;

        float healthPercent = (float)unit.stats.currentHealth / maxHealth;
        barRenderer.transform.localScale = new Vector3(healthPercent, 0.1f, 1f);

        if (healthPercent > 0.6f)
            barRenderer.color = Color.green;
        else if (healthPercent > 0.3f)
            barRenderer.color = Color.yellow;
        else
            barRenderer.color = Color.red;
    }
}
