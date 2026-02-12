using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
public class AStarUITest : MonoBehaviour
{
    [Header("A* 그리드")]
    public AStarGrid grid;

    [Header("타일맵 참조")]
    public Tilemap wallTilemap;      // 벽 타일맵 (장애물 배치용)
    public Tilemap pathTilemap;      // 경로 시각화용 타일맵

    [Header("타일 에셋 (Project 창에서 드래그)")]
    public TileBase wallTile;        // 벽 타일
    public TileBase pathTile;        // 경로 표시 타일
    public TileBase startTile;       // 시작점 표시 타일 (없으면 pathTile 사용)
    public TileBase goalTile;        // 도착점 표시 타일 (없으면 pathTile 사용)

    [Header("모드 버튼")]
    public Button btnModeWall;
    public Button btnModeStart;
    public Button btnModeGoal;
    public Button btnFindPath;
    public Button btnClearWalls;
    public Button btnToggleDiagonal;

    [Header("상태 표시")]
    public Text txtStatus;

    [Header("그리드-타일맵 좌표 오프셋")]
    public Vector3Int tilemapOrigin = new Vector3Int(-5, -4, 0);

    enum EditMode { Wall, Start, Goal }
    EditMode currentMode = EditMode.Wall;

    Vector2Int startPos = new Vector2Int(0, 0);
    Vector2Int goalPos;

    void Start()
    {
        if (grid == null)
            grid = GetComponent<AStarGrid>();

        goalPos = new Vector2Int(grid.width - 1, grid.height - 1);

        // 버튼 연결
        if (btnModeWall != null)
            btnModeWall.onClick.AddListener(() => SetMode(EditMode.Wall));
        if (btnModeStart != null)
            btnModeStart.onClick.AddListener(() => SetMode(EditMode.Start));
        if (btnModeGoal != null)
            btnModeGoal.onClick.AddListener(() => SetMode(EditMode.Goal));
        if (btnFindPath != null)
            btnFindPath.onClick.AddListener(FindPath);
        if (btnClearWalls != null)
            btnClearWalls.onClick.AddListener(ClearWalls);
        if (btnToggleDiagonal != null)
            btnToggleDiagonal.onClick.AddListener(ToggleDiagonal);

        // 시작/도착 마커 표시
        RefreshMarkers();
        UpdateStatus("벽 배치 모드: 타일맵 셀을 클릭하여 벽을 배치하세요.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();

        // 드래그로 연속 벽 배치
        if (Input.GetMouseButton(0) && currentMode == EditMode.Wall)
            HandleClick();
    }

    // --- 클릭 처리 ---
    void HandleClick()
    {
        // UI 클릭 무시
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        // 마우스 월드 좌표 → 타일맵 셀 좌표 → 그리드 좌표
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = wallTilemap.WorldToCell(mouseWorld);

        // 그리드 좌표로 변환
        int gx = cellPos.x - tilemapOrigin.x;
        int gy = cellPos.y - tilemapOrigin.y;

        if (gx < 0 || gx >= grid.width || gy < 0 || gy >= grid.height) return;

        switch (currentMode)
        {
            case EditMode.Wall:
                if (gx == startPos.x && gy == startPos.y) return;
                if (gx == goalPos.x && gy == goalPos.y) return;

                // 벽 토글: 타일이 있으면 제거, 없으면 배치
                bool hasWall = wallTilemap.HasTile(cellPos);
                if (hasWall)
                {
                    wallTilemap.SetTile(cellPos, null);
                    grid.walkable[gx, gy] = true;
                }
                else
                {
                    wallTilemap.SetTile(cellPos, wallTile);
                    grid.walkable[gx, gy] = false;
                }
                ClearPathTilemap();
                break;

            case EditMode.Start:
                if (!grid.walkable[gx, gy]) return;
                startPos = new Vector2Int(gx, gy);
                ClearPathTilemap();
                RefreshMarkers();
                UpdateStatus($"시작점 설정: ({gx}, {gy})");
                break;

            case EditMode.Goal:
                if (!grid.walkable[gx, gy]) return;
                goalPos = new Vector2Int(gx, gy);
                ClearPathTilemap();
                RefreshMarkers();
                UpdateStatus($"도착점 설정: ({gx}, {gy})");
                break;
        }
    }

    // --- 모드 변경 ---
    void SetMode(EditMode mode)
    {
        currentMode = mode;
        string modeText = mode == EditMode.Wall ? "벽 배치 모드: 셀 클릭으로 벽 토글" :
                          mode == EditMode.Start ? "시작점 모드: 셀 클릭으로 시작점 설정" :
                          "도착점 모드: 셀 클릭으로 도착점 설정";
        UpdateStatus(modeText);
    }

    // --- 경로 찾기 ---
    void FindPath()
    {
        ClearPathTilemap();

        var path = grid.FindPath(startPos, goalPos);
        grid.SetLastPath(path);

        if (path != null)
        {
            // 경로를 PathTilemap에 타일로 표시
            foreach (var p in path)
            {
                Vector3Int cellPos = GridToCell(p.x, p.y);
                pathTilemap.SetTile(cellPos, pathTile);
            }

            // 시작/도착 마커 다시 표시 (경로 위에 덮어쓰기)
            RefreshMarkers();

            string pathStr = "";
            foreach (var p in path)
                pathStr += $"({p.x},{p.y}) → ";

            UpdateStatus(
                $"경로 발견! 길이: {path.Count}칸\n" +
                $"휴리스틱: {grid.heuristic} | 대각선: {(grid.allowDiagonal ? "ON" : "OFF")}\n" +
                $"경로: {pathStr.TrimEnd(' ', '→')}"
            );
            Debug.Log($"[A*] 경로 발견: {path.Count}칸");
        }
        else
        {
            UpdateStatus("경로 없음! 벽으로 막혀있거나 도달 불가능합니다.");
            Debug.LogWarning("[A*] 경로를 찾을 수 없습니다.");
        }
    }

    // --- 경로 타일맵 지우기 ---
    void ClearPathTilemap()
    {
        if (pathTilemap != null)
            pathTilemap.ClearAllTiles();
        grid.SetLastPath(null);
        RefreshMarkers();
    }

    // --- 벽 전부 초기화 ---
    void ClearWalls()
    {
        if (wallTilemap != null)
            wallTilemap.ClearAllTiles();

        for (int x = 0; x < grid.width; x++)
            for (int y = 0; y < grid.height; y++)
                grid.walkable[x, y] = true;

        ClearPathTilemap();
        UpdateStatus("벽 초기화 완료.");
    }

    // --- 대각선 토글 ---
    void ToggleDiagonal()
    {
        grid.allowDiagonal = !grid.allowDiagonal;

        if (btnToggleDiagonal != null)
        {
            var txt = btnToggleDiagonal.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = grid.allowDiagonal ? "대각선 ON" : "대각선 OFF";
        }

        ClearPathTilemap();
        UpdateStatus($"대각선 이동: {(grid.allowDiagonal ? "ON (8방향)" : "OFF (4방향)")}");
    }

    // --- 시작/도착 마커 표시 ---
    void RefreshMarkers()
    {
        if (pathTilemap == null) return;

        // 시작점 마커
        var sTile = startTile != null ? startTile : pathTile;
        Vector3Int startCell = GridToCell(startPos.x, startPos.y);
        pathTilemap.SetTile(startCell, sTile);
        // 시작점 색상을 초록색으로
        pathTilemap.SetTileFlags(startCell, TileFlags.None);
        pathTilemap.SetColor(startCell, Color.green);

        // 도착점 마커
        var gTile = goalTile != null ? goalTile : pathTile;
        Vector3Int goalCell = GridToCell(goalPos.x, goalPos.y);
        pathTilemap.SetTile(goalCell, gTile);
        pathTilemap.SetTileFlags(goalCell, TileFlags.None);
        pathTilemap.SetColor(goalCell, Color.blue);
    }

    // --- 그리드 좌표 → 타일맵 셀 좌표 ---
    Vector3Int GridToCell(int gx, int gy)
    {
        return new Vector3Int(tilemapOrigin.x + gx, tilemapOrigin.y + gy, 0);
    }

    // --- 상태 텍스트 ---
    void UpdateStatus(string msg)
    {
        if (txtStatus != null)
            txtStatus.text = msg;
    }
}
