using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class BotAIController2D : MonoBehaviour
{
    [Header("Grid")]
    public GridScript grid;

    [Header("Enemy")]
    public Transform enemy;

    [Header("Movement")]
    public float speed = 5f;
    public float waypointRadius = 0.4f;

    [Header("Pathfinding")]
    public int safeThreshold = 950;
    public int recentCellsMax = 6;

    enum Direction { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest }

    Rigidbody2D rb;
    List<Vector2Int> path = new List<Vector2Int>();
    int waypointIndex = 0;
    Queue<Vector2Int> recentCells = new Queue<Vector2Int>();

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        if (grid == null || enemy == null) return;

        // 1. Get current cell
        Vector2Int currentCell = WorldToCell(transform.position);

        // 2. Find the safest cell on the whole grid
        Vector2Int goalCell = FindLowestCell();

        // 3. Build path
        path = BuildPath(currentCell, goalCell);
        waypointIndex = 0;

        // 4. If no path, stop
        if (path == null || path.Count == 0)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        while (waypointIndex < path.Count - 1 &&
               Vector2.Distance(transform.position, CellToWorld(path[waypointIndex])) < waypointRadius)
        {
            waypointIndex++;
        }

        // 7. Move toward current waypoint
        Vector2 target = CellToWorld(path[waypointIndex]);
        float dist = Vector2.Distance(transform.position, target);

        if (dist <= waypointRadius && waypointIndex == path.Count - 1)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.velocity = dir * speed;

        // 8. Track last visited cells
        recentCells.Enqueue(currentCell);
        if (recentCells.Count > recentCellsMax)
            recentCells.Dequeue();
    }

    Vector2Int FindLowestCell()
    {
        int lowestVal = int.MaxValue;
        int bestX = CONSTANTS.minX;
        int bestY = CONSTANTS.minY;

        for (int x = CONSTANTS.minX; x <= CONSTANTS.maxX; x++)
        {
            for (int y = CONSTANTS.minY; y <= CONSTANTS.maxY; y++)
            {
                int val = grid.getGrid(x, y);
                if (val < lowestVal)
                {
                    lowestVal = val;
                    bestX = x;
                    bestY = y;
                }
            }
        }

        return new Vector2Int(bestX, bestY);
    }

    List<Vector2Int> BuildPath(Vector2Int startCell, Vector2Int goalCell)
    {
        var path = new List<Vector2Int>();
        Vector2Int current = startCell;

        int maxSteps = CONSTANTS.maxX * CONSTANTS.maxY;

        for (int step = 0; step < maxSteps; step++)
        {
            if (current == goalCell) break;

            float relX = goalCell.x - current.x;
            float relY = goalCell.y - current.y;
            float angle = Mathf.Atan2(relY, relX) * Mathf.Rad2Deg;
            Direction dir = GetDirection(angle);

            List<Vector2Int> neighbors = GetSortedNeighbors(current, dir);

            // filter out recently visited cells
            for (int i = 0; i < neighbors.Count; i++)
                if (recentCells.Contains(neighbors[i]))
                    neighbors[i] = new Vector2Int(-1, -1);

            Vector2Int best = new Vector2Int(-1, -1);

            best = BestInRange(neighbors, 3, 5);
            if (best.x == -1) best = BestInRange(neighbors, 2, 6);
            if (best.x == -1) best = BestInRange(neighbors, 0, 7);
            if (best.x == -1) best = LowestInRange(neighbors, 0, 7);
            if (best.x == -1) break;

            path.Add(best);
            current = best;
        }

        return path;
    }

    Vector2Int BestInRange(List<Vector2Int> neighbors, int from, int to)
    {
        int lowestVal = safeThreshold;
        Vector2Int best = new Vector2Int(-1, -1);

        for (int i = from; i <= to; i++)
        {
            Vector2Int n = neighbors[i];
            if (n.x == -1) continue; // if part of previous queue dont consider point

            int val = grid.getGrid(n.x, n.y);
            if (val < lowestVal)
            {
                lowestVal = val;
                best = n;
            }
        }

        return best;
    }

    Vector2Int LowestInRange(List<Vector2Int> neighbors, int from, int to)
    {
        int lowestVal = int.MaxValue;
        Vector2Int best = new Vector2Int(-1, -1);

        for (int i = from; i <= to; i++)
        {
            Vector2Int n = neighbors[i];
            if (n.x == -1) continue;

            int val = grid.getGrid(n.x, n.y);
            if (val < lowestVal)
            {
                lowestVal = val;
                best = n;
            }
        }

        return best;
    }

    Direction GetDirection(float angle)
    {
        if (angle > 157.5f || angle < -157.5f) return Direction.West;
        if (angle > 112.5f) return Direction.NorthWest;
        if (angle > 67.5f) return Direction.North;
        if (angle > 22.5f) return Direction.NorthEast;
        if (angle > -22.5f) return Direction.East;
        if (angle > -67.5f) return Direction.SouthEast;
        if (angle > -112.5f) return Direction.South;
        return Direction.SouthWest;
    }

    Vector2Int WorldToCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.Clamp(Mathf.RoundToInt(worldPos.x), CONSTANTS.minX, CONSTANTS.maxX),
            Mathf.Clamp(Mathf.RoundToInt(worldPos.y), CONSTANTS.minY, CONSTANTS.maxY)
        );
    }

    Vector2 CellToWorld(Vector2Int cell)
    {
        return new Vector2(cell.x, cell.y);
    }

    List<Vector2Int> GetSortedNeighbors(Vector2Int cell, Direction escapeDir)
    {
        Vector2Int N = new Vector2Int(0, 1);
        Vector2Int NE = new Vector2Int(1, 1);
        Vector2Int E = new Vector2Int(1, 0);
        Vector2Int SE = new Vector2Int(1, -1);
        Vector2Int S = new Vector2Int(0, -1);
        Vector2Int SW = new Vector2Int(-1, -1);
        Vector2Int W = new Vector2Int(-1, 0);
        Vector2Int NW = new Vector2Int(-1, 1);

        Vector2Int[] dirs;

        switch (escapeDir) // based on escape direction, sort a list to ensure that the escape direction is the 4th index of the list
        {
            case Direction.North: dirs = new[] { S, SW, W, NW, N, NE, E, SE }; break;
            case Direction.NorthEast: dirs = new[] { SW, W, NW, N, NE, E, SE, S }; break;
            case Direction.East: dirs = new[] { W, NW, N, NE, E, SE, S, SW }; break;
            case Direction.SouthEast: dirs = new[] { NW, N, NE, E, SE, S, SW, W }; break;
            case Direction.South: dirs = new[] { N, NE, E, SE, S, SW, W, NW }; break;
            case Direction.SouthWest: dirs = new[] { NE, E, SE, S, SW, W, NW, N }; break;
            case Direction.West: dirs = new[] { E, SE, S, SW, W, NW, N, NE }; break;
            case Direction.NorthWest: dirs = new[] { SE, S, SW, W, NW, N, NE, E }; break;
            default: dirs = new[] { S, SW, W, NW, N, NE, E, SE }; break;
        }

        var neighbors = new List<Vector2Int>();
        foreach (var d in dirs)
        {
            Vector2Int n = cell + d;
            if (n.x >= CONSTANTS.minX && n.x <= CONSTANTS.maxX &&
                n.y >= CONSTANTS.minY && n.y <= CONSTANTS.maxY)
                neighbors.Add(n);
            else
                neighbors.Add(new Vector2Int(-1, -1));
        }

        return neighbors;
    }
}