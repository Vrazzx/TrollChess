using UnityEngine;
using UnityEngine.EventSystems;

public class BenchUnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public UnitData unitData;
    private GameObject dragPreview;
    private GameObject highlightPreview; // ← новое поле
    private Camera mainCamera;
    public GameObject iconGO; // ← добавьте это

    // Ссылка на префаб подсветки (назначьте в инспекторе)
    public GameObject highlightPrefab;

    void Start()
    {
        mainCamera = Camera.main;
        iconGO = gameObject;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Создаём превью юнита
        dragPreview = new GameObject("DragPreview");
        var sr = dragPreview.AddComponent<SpriteRenderer>();
        sr.color = Color.cyan;
        sr.sortingOrder = 20;

        // Создаём подсветку
        if (highlightPrefab != null)
        {
            highlightPreview = Instantiate(highlightPrefab);
            highlightPreview.SetActive(false); // скрыта по умолчанию
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedUnit = eventData.pointerDrag?.GetComponent<Unit>();
        if (draggedUnit == null || !draggedUnit.IsPlayer) return;

        // Удаляем с поля
        UnitManager.Instance.OnUnitDied(draggedUnit);

        // Добавляем в инвентарь
        GameManager.Instance.benchUnits.Add(draggedUnit.data); // ✅ важно!

        // Обновляем UI
        UIManager.Instance?.RefreshBench();

        // Уничтожаем объект юнита
        Destroy(eventData.pointerDrag);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(dragPreview);
        if (highlightPreview != null)
            Destroy(highlightPreview);

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Node targetNode = FindClosestFreePlayerNode(mousePos);
        if (targetNode != null)
        {
            var unit = UnitManager.Instance.SpawnUnit(unitData, targetNode, isPlayer: true);
            if (unit != null)
            {
                unit.originalNode = targetNode;
            }
            GameManager.Instance.benchUnits.Remove(unitData);
            Destroy(gameObject);
        }
    }

    private void UpdateHighlight(Vector3 mouseWorldPos)
    {
        if (highlightPreview == null) return;

        // Ищем ближайшую клетку
        Node closestNode = FindClosestPlayerNode(mouseWorldPos);
        if (closestNode != null)
        {
            highlightPreview.SetActive(true);
            highlightPreview.transform.position = new Vector3(closestNode.worldPosition.x, closestNode.worldPosition.y, -1f);

            // Цвет: зелёный = можно разместить, красный = нельзя
            bool canPlace = GridManager.Instance.IsNodeInPlayerZone(closestNode) && !closestNode.IsOccupied;
            Color highlightColor = canPlace ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);

            var sr = highlightPreview.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = highlightColor;
        }
        else
        {
            highlightPreview.SetActive(false);
        }
    }

    // Находит ближайшую клетку в зоне игрока (даже занятую)
    private Node FindClosestPlayerNode(Vector3 mouseWorldPos)
    {
        var allNodes = GridManager.Instance.AllNodes;
        if (allNodes == null) return null;

        Node closest = null;
        float minDist = float.MaxValue;

        foreach (var node in allNodes)
        {
            if (!GridManager.Instance.IsNodeInPlayerZone(node)) continue;

            float dist = Vector2.Distance(mouseWorldPos, node.worldPosition);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return minDist < 2.0f ? closest : null;
    }

    // Находит ближайшую СВОБОДНУЮ клетку
    private Node FindClosestFreePlayerNode(Vector3 mouseWorldPos)
    {
        var allNodes = GridManager.Instance.AllNodes;
        if (allNodes == null) return null;

        Node closest = null;
        float minDist = float.MaxValue;

        foreach (var node in allNodes)
        {
            if (!GridManager.Instance.IsNodeInPlayerZone(node)) continue;
            if (node.IsOccupied) continue;

            float dist = Vector2.Distance(mouseWorldPos, node.worldPosition);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return minDist < 2.0f ? closest : null;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // Позиция курсора в мире
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragPreview.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        // Обновляем подсветку
        UpdateHighlight(mousePos);
    }
}
