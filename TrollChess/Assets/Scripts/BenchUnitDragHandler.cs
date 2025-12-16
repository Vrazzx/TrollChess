using UnityEngine;
using UnityEngine.EventSystems;

public class BenchUnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public UnitData unitData;
    private GameObject dragPreview;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BenchUnitDragHandler.OnBeginDrag called");

        // Создаём превью
        dragPreview = new GameObject("DragPreview");
        var sr = dragPreview.AddComponent<SpriteRenderer>();
        sr.color = Color.cyan;
        sr.sortingOrder = 20;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        dragPreview.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(dragPreview);

        // Луч из камеры
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.zero, Mathf.Infinity);

        if (hit.collider != null)
        {
            // Найдём ближайший узел
            var allNodes = GridManager.Instance.AllNodes;
            Node closest = null;
            float minDist = float.MaxValue;
            foreach (var node in allNodes)
            {
                if (node.IsOccupied) continue;
                if (!GridManager.Instance.IsNodeInPlayerZone(node)) continue;

                float dist = Vector2.Distance(ray.origin, node.worldPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = node;
                }
            }

            if (closest != null)
            {
                UnitManager.Instance.SpawnUnit(unitData, closest, isPlayer: true);
                GameManager.Instance.benchUnits.Remove(unitData);
                Destroy(gameObject); // удаляем с Bench
                return;
            }
        }

        // Если не получилось — ничего не делаем (остаётся на Bench)
    }
}