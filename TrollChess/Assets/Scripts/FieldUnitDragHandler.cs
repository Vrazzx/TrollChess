using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Unit))]
public class FieldUnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Unit unit;
    private GameObject dragPreview;
    private GameObject highlightPreview;
    private Camera mainCamera;
    private Node originalNode; // запоминаем исходную позицию на время перетаскивания

    void Start()
    {
        unit = GetComponent<Unit>();
        mainCamera = Camera.main;

        // Убедимся, что у юнита есть originalNode
        if (unit.originalNode == null)
        {
            unit.originalNode = unit.currentNode;
        }
    }

    // Можно ли перемещать юнита?
    private bool CanMove()
    {
        return GameManager.Instance?.currentPhase == GamePhase.Preparation;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CanMove()) return;

        // Можно добавить визуальную обратную связь (например, увеличение)
        Debug.Log($"Selected unit: {unit.data.unitName}");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanMove()) return;

        // Сохраняем текущий узел
        originalNode = unit.currentNode;

        // Освобождаем текущий узел (чтобы другие юниты могли его занять)
        if (originalNode != null)
        {
            originalNode.SetOccupied(false);
        }

        // Создаём превью
        dragPreview = new GameObject("%FieldDragPreview");
        var sr = dragPreview.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.color = unit.IsPlayer ? Color.cyan : Color.red;
        sr.sortingOrder = 20;

        // Подсветка
        if (UIManager.Instance?.highlightCirclePrefab != null)
        {
            highlightPreview = Instantiate(UIManager.Instance.highlightCirclePrefab);
            highlightPreview.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanMove()) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragPreview.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        // Обновляем подсветку
        UpdateHighlight(mousePos);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanMove()) return;

        Destroy(dragPreview);
        if (highlightPreview != null)
            Destroy(highlightPreview);

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Node targetNode = FindClosestFreePlayerNode(mousePos);

        if (targetNode != null)
        {
            // Занимаем новую позицию
            targetNode.SetOccupied(true);
            unit.currentNode = targetNode;
            unit.originalNode = targetNode; // обновляем исходную позицию
            transform.position = targetNode.worldPosition;
        }
        else
        {
            // Возвращаем на исходную позицию
            if (originalNode != null)
            {
                originalNode.SetOccupied(true);
                unit.currentNode = originalNode;
                transform.position = originalNode.worldPosition;
            }
        }
    }

    private void UpdateHighlight(Vector3 mouseWorldPos)
    {
        if (highlightPreview == null) return;

        Node closestNode = FindClosestPlayerNode(mouseWorldPos);
        if (closestNode != null)
        {
            highlightPreview.SetActive(true);
            highlightPreview.transform.position = new Vector3(closestNode.worldPosition.x, closestNode.worldPosition.y, -1f);

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
}
