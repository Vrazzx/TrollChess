using UnityEngine;
using UnityEngine.EventSystems;

public class GridCellDropHandler : MonoBehaviour, IDropHandler
{
    public Node node; // ссылка на узел сетки

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("GridCellDropHandler.OnDrop called");

        var dragHandler = eventData.pointerDrag?.GetComponent<BenchUnitDragHandler>();
        if (dragHandler != null && node != null && !node.IsOccupied)
        {
            Debug.Log($"Placing unit {dragHandler.unitData.unitName} on cell {node.index}");
            PlaceUnit(dragHandler.unitData);
            // BenchUnitDragHandler сам удалит юнита со скамейки и с себя
        }
    }

    public void PlaceUnit(UnitData unitData)
    {
        if (node == null || node.IsOccupied) return;

        // Спавним юнита на поле
        UnitManager.Instance.SpawnUnit(unitData, node, isPlayer: true);
    }

}
