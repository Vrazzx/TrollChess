using UnityEngine;
using UnityEngine.EventSystems;

public class BenchDropHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var draggedUnit = eventData.pointerDrag?.GetComponent<Unit>();
        if (draggedUnit == null || !draggedUnit.IsPlayer) return;

        // Удаляем с поля
        UnitManager.Instance.OnUnitDied(draggedUnit);

        // Удаляем старую иконку (если есть)
        if (draggedUnit.benchIcon != null)
        {
            Destroy(draggedUnit.benchIcon);
            draggedUnit.benchIcon = null;
        }

        // Добавляем в инвентарь
        GameManager.Instance.benchUnits.Add(draggedUnit.data);

        // Создаём новую иконку
        UIManager.Instance?.AddUnitToBench(draggedUnit.data);

        // Уничтожаем объект юнита
        Destroy(eventData.pointerDrag);
    }
}
