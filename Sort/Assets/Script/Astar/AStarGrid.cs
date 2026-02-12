using System;
using System.Collections.Generic;
using UnityEngine;

// 개선된 A* 그리드 구현
// - 우선순위 큐(힙)을 사용하여 Open Set 성능 개선
// - 대각선 이동 옵션과 여러 휴리스틱 지원
public class AStarGrid : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public bool[,] walkable;

    public bool allowDiagonal = false;
    public enum HeuristicType { Manhattan, Euclidean, Octile }
    public HeuristicType heuristic = HeuristicType.Manhattan;

    // 대각선 이동 비용 (기본 sqrt(2))
    public float diagonalCost = 1.41421356f;

    void Awake()
    {
        walkable = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                walkable[x, y] = true;
    }

    class Node
    {
        public int x, y;
        public float g; // 시작부터의 비용
        public float h; // 휴리스틱 추정값
        public float f => g + h;
        public Vector2Int parent;
    }

    // 간단한 최소 힙 기반 우선순위 큐
    class PriorityQueue<T>
    {
        List<T> data = new List<T>();
        Comparison<T> cmp;
        public PriorityQueue(Comparison<T> cmp) { this.cmp = cmp; }
        public int Count => data.Count;
        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (cmp(data[ci], data[pi]) >= 0) break;
                var tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }
        public T Dequeue()
        {
            int li = data.Count - 1;
            var front = data[0];
            data[0] = data[li];
            data.RemoveAt(li);
            --li;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && cmp(data[rc], data[ci]) < 0) ci = rc;
                if (cmp(data[pi], data[ci]) <= 0) break;
                var tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
                pi = ci;
            }
            return front;
        }
    }

    float Heuristic(int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x1 - x2);
        int dy = Math.Abs(y1 - y2);
        switch (heuristic)
        {
            case HeuristicType.Euclidean:
                return Mathf.Sqrt(dx * dx + dy * dy);
            case HeuristicType.Octile:
                // octile: 대각선 비용과 직선 비용 혼합
                int min = Math.Min(dx, dy);
                int max = Math.Max(dx, dy);
                return min * diagonalCost + (max - min) * 1f;
            case HeuristicType.Manhattan:
            default:
                return dx + dy;
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (start.x < 0 || start.x >= width || start.y < 0 || start.y >= height) return null;
        if (goal.x < 0 || goal.x >= width || goal.y < 0 || goal.y >= height) return null;
        if (!walkable[start.x, start.y] || !walkable[goal.x, goal.y]) return null;

        // gScore[x,y]: 시작점에서 (x,y)까지의 현재까지 알려진 최소 비용
        float[,] gScore = new float[width, height];
        for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) gScore[x, y] = float.PositiveInfinity;
        Vector2Int[,] parent = new Vector2Int[width, height];
        // ★ Closed List (닫힌 목록): 이미 최적 경로가 확정된 노드
        //   - true로 표시된 노드는 다시 탐색하지 않음
        bool[,] visited = new bool[width, height];

        var cmp = new Comparison<Node>((a, b) => a.f.CompareTo(b.f));
        // ★ Open List (열린 목록): 탐색 후보 노드들을 f(n) 기준 우선순위 큐로 관리
        //   - f(n)이 가장 작은 노드를 먼저 꺼내서 탐색
        var open = new PriorityQueue<Node>(cmp);

        gScore[start.x, start.y] = 0f;
        parent[start.x, start.y] = new Vector2Int(-1, -1); // 시작 노드의 부모를 -1로 설정하여 경로 복원 종료 조건 보장
        var startNode = new Node() { x = start.x, y = start.y, g = 0f, h = Heuristic(start.x, start.y, goal.x, goal.y), parent = new Vector2Int(-1, -1) };
        open.Enqueue(startNode);

        var directions = new List<(int x, int y, float cost)>()
        {
            (1,0,1f), (-1,0,1f), (0,1,1f), (0,-1,1f)
        };
        if (allowDiagonal)
        {
            directions.Add((1, 1, diagonalCost)); directions.Add((1, -1, diagonalCost)); directions.Add((-1, 1, diagonalCost)); directions.Add((-1, -1, diagonalCost));
        }

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            // stale entry check: 이미 더 좋은 경로로 갱신된 노드는 건너뜀
            if (current.g > gScore[current.x, current.y]) continue;

            if (current.x == goal.x && current.y == goal.y)
            {
                // reconstruct path
                var path = new List<Vector2Int>();
                Vector2Int cur = new Vector2Int(current.x, current.y);
                while (cur.x >= 0)
                {
                    path.Add(cur);
                    var p = parent[cur.x, cur.y];
                    cur = p;
                }
                path.Reverse();
                return path;
            }

            // 현재 노드를 Closed List에 추가 (탐색 완료 표시)
            visited[current.x, current.y] = true;

            foreach (var d in directions)
            {
                int nx = current.x + d.x;
                int ny = current.y + d.y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (!walkable[nx, ny]) continue;

                // corner cutting check: if moving diagonally, optionally prevent passing through diagonal corners
                if (allowDiagonal && Math.Abs(d.x) == 1 && Math.Abs(d.y) == 1)
                {
                    // prevent cutting between two blocked orthogonals
                    if (!walkable[current.x + d.x, current.y] || !walkable[current.x, current.y + d.y]) continue;
                }

                float tentativeG = gScore[current.x, current.y] + d.cost;
                if (tentativeG < gScore[nx, ny])
                {
                    gScore[nx, ny] = tentativeG;
                    parent[nx, ny] = new Vector2Int(current.x, current.y);
                    float h = Heuristic(nx, ny, goal.x, goal.y);
                    var neighborNode = new Node() { x = nx, y = ny, g = tentativeG, h = h, parent = new Vector2Int(current.x, current.y) };
                    open.Enqueue(neighborNode);
                }
            }
        }

        return null; // no path
    }

    // Visualization
    [Header("Gizmos")] public bool drawGrid = true;
    public bool drawWalkable = true;
    public bool drawBlocked = true;
    public bool drawLastPath = true;
    public float cellSize = 1f;

    // 마지막으로 계산된 경로를 저장할 수 있도록 공개
    public List<Vector2Int> lastPath = null;

    // 외부에서 경로를 전달해 시각화 가능
    public void SetLastPath(List<Vector2Int> path)
    {
        lastPath = path;
    }

    Vector3 CellCenterWorld(int x, int y)
    {
        // 그리드 (0,0)을 월드상의 transform.position에 매핑하고 각 셀은 cellSize 크기
        return transform.position + new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0f);
    }

    void OnDrawGizmos()
    {
        if (!drawGrid || walkable == null) return;

        // Draw cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var center = CellCenterWorld(x, y);
                if (walkable != null)
                {
                    if (!walkable[x, y] && drawBlocked)
                    {
                        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
                        Gizmos.DrawCube(center, Vector3.one * cellSize);
                    }
                    else if (walkable[x, y] && drawWalkable)
                    {
                        Gizmos.color = new Color(0f, 1f, 0f, 0.05f);
                        Gizmos.DrawCube(center, Vector3.one * cellSize);
                    }
                }
                // optional grid lines
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(center, Vector3.one * cellSize);
            }
        }

        // Draw last path
        if (drawLastPath && lastPath != null && lastPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < lastPath.Count; i++)
            {
                var p = lastPath[i];
                var w = CellCenterWorld(p.x, p.y);
                Gizmos.DrawSphere(w, cellSize * 0.15f);
                if (i > 0)
                {
                    var prev = lastPath[i - 1];
                    var wp = CellCenterWorld(prev.x, prev.y);
                    Gizmos.DrawLine(wp, w);
                }
            }
        }
    }
}
