using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

public class GridManager : Manager<GridManager>
{
    public Tilemap grid;

    public Canvas canvas; // ← ДОБАВЛЕНО: ссылка на Canvas
    public GameObject gridCellPrefab; // ← ДОБАВЛЕНО: префаб клетки UI

    private bool uiGridCreated = false; // ← ДОБАВЛЕНО: флаг, чтобы не создавать дважды
    private Dictionary<Vector3, Node> positionToNode = new Dictionary<Vector3, Node>();
    public List<Node> AllNodes => graph?.nodes;


    Graph graph;
    // Dictionary<Team, int> startPositionsPerTeam;

    // public Node GetFreeNode(Team forTeam)
    // {
    //     int startIndex = startPositionsPerTeam[forTeam];
    //     int currentIndex = startIndex;

    //     while (graph.nodes[currentIndex].IsOccupied)
    //     {
    //         if (startIndex == 0)
    //         {
    //             currentIndex++;
    //             if (currentIndex == graph.nodes.Count)
    //             {
    //                 return null;
    //             }
    //         }
    //         else
    //         {
    //             currentIndex--;
    //             if (currentIndex == -1)
    //             {
    //                 return null;
    //             }
    //         }
    //     }

    //     return graph.nodes[currentIndex];
    // }

    private void Awake()
    {
        base.Awake();
        InitializeGraph();
        // startPositionsPerTeam = new Dictionary<Team, int>();
        // startPositionsPerTeam.Add(Team.Team1, 0);
        // startPositionsPerTeam.Add(Team.Team2, graph.nodes.Count - 1);

    }

    private void InitializeGraph()
    {
        graph = new Graph();
        positionToNode.Clear();

        for (int x = grid.cellBounds.xMin; x < grid.cellBounds.xMax; x++)
        {
            for (int y = grid.cellBounds.yMin; y < grid.cellBounds.yMax; y++)
            {
                Vector3Int localPos = new Vector3Int(x, y, 0);
                if (grid.HasTile(localPos))
                {
                    Vector3 worldPos = grid.CellToWorld(localPos);
                    graph.AddNode(worldPos);
                    // Кэшируем позицию → узел для быстрого поиска
                    positionToNode[worldPos] = graph.nodes[graph.nodes.Count - 1];
                }
            }
        }

        // Создаём связи между соседями (гекс-сетка: до 6 соседей)
        foreach (Node node in graph.nodes)
        {
            foreach (Node other in graph.nodes)
            {
                if (node != other && Vector3.Distance(node.worldPosition, other.worldPosition) < 1.1f)
                {
                    graph.AddEdge(node, other);
                }
            }
        }
    }

    /// <summary>
    /// Возвращает узел по мировой позиции (с точностью до 0.01f)
    /// </summary>
    public Node GetNodeAtPosition(Vector3 worldPosition)
    {
        foreach (var kvp in positionToNode)
        {
            if (Vector3.Distance(kvp.Key, worldPosition) < 0.01f)
                return kvp.Value;
        }
        return null;
    }

    /// <summary>
    /// Освобождает узел (используется при смерти юнита или удалении)
    /// </summary>
    public void ReleaseNode(Node node)
    {
        if (node != null)
            node.SetOccupied(false);
    }

    /// <summary>
    /// Пытается занять узел для юнита
    /// </summary>
    public bool TryOccupyNode(Node node)
    {
        if (node == null || node.IsOccupied) return false;
        node.SetOccupied(true);
        return true;
    }

    /// <summary>
    /// Проверяет, принадлежит ли узел нижней (дружественной) половине доски
    /// </summary>
    public bool IsNodeInPlayerZone(Node node)
    {
        if (graph.nodes.Count == 0) return false;
        float midY = (graph.nodes[0].worldPosition.y + graph.nodes[^1].worldPosition.y) / 2f;
        return node.worldPosition.y <= midY;
    }
    private void OnDrawGizmos()
    {
        if (graph == null)
            return;

        var allEdges = graph.edges;
        if (allEdges == null)
            return;

        foreach (Edge e in allEdges)
        {
            Debug.DrawLine(e.from.worldPosition, e.to.worldPosition, Color.black, 100);
        }

        var allNodes = graph.nodes;
        if (allNodes == null)
            return;

        foreach (Node n in allNodes)
        {
            Gizmos.color = n.IsOccupied ? Color.red : Color.green;
            Gizmos.DrawSphere(n.worldPosition, 0.1f);

        }
    }
    
    public void CreateGridUI()
    {
        if (uiGridCreated) return;
        uiGridCreated = true;

        foreach (var node in AllNodes)
        {
            var cell = Instantiate(gridCellPrefab, canvas.transform);
            cell.GetComponent<GridCellDropHandler>().node = node;
            cell.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(node.worldPosition);
        }
    }
}