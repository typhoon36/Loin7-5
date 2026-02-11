using UnityEngine;

using UnityEngine.Tilemaps;



// TilemapToAStar: Tilemap의 타일 정보를 AStarGrid의 walkable 배열로 변환

// - tilemap: 장애물이 놓인 Tilemap (HasTile(cell)로 판단)

// - grid: 타일 좌표(0,0)가 매핑될 AStarGrid

// - tilemapOrigin: 타일맵 셀 좌표에서 그리드(0,0)에 대응되는 cell 좌표

// 예: tilemapOrigin = new Vector3Int(-5, -3, 0) 이면 grid (0,0) -> tilemap cell (-5,-3)

public class TilemapToAStar : MonoBehaviour
{

    public Tilemap tilemap;

    public AStarGrid grid;

    public Vector3Int tilemapOrigin = Vector3Int.zero;



    // 자동으로 Start 시 변환

    void Start()

    {

        if (tilemap == null || grid == null) return;

        PopulateGridFromTilemap();

    }



    // grid의 (x,y) 좌표를 tilemap의 cell로 변환

    Vector3Int GridToCell(int x, int y)

    {

        return new Vector3Int(tilemapOrigin.x + x, tilemapOrigin.y + y, tilemapOrigin.z);

    }



    // tilemap의 cell -> grid 좌표(역변환). grid 좌표 범위를 벗어나면 false를 반환

    bool CellToGrid(Vector3Int cell, out Vector2Int gridPos)

    {

        int gx = cell.x - tilemapOrigin.x;

        int gy = cell.y - tilemapOrigin.y;

        if (gx < 0 || gx >= grid.width || gy < 0 || gy >= grid.height)

        {

            gridPos = Vector2Int.zero;

            return false;

        }

        gridPos = new Vector2Int(gx, gy);

        return true;

    }



    // 그리드에 타일맵 정보를 채움

    public void PopulateGridFromTilemap()

    {

        for (int x = 0; x < grid.width; x++)

        {

            for (int y = 0; y < grid.height; y++)

            {

                var cell = GridToCell(x, y);

                bool hasTile = tilemap.HasTile(cell);

                // 프로젝트에 따라 타일이 있으면 통과 불가로 처리

                grid.walkable[x, y] = !hasTile;

            }

        }

    }



    // 필요 시 개별 셀 업데이트

    public void UpdateCellFromTilemapCell(Vector3Int cell)

    {

        if (CellToGrid(cell, out var gp))

        {

            grid.walkable[gp.x, gp.y] = !tilemap.HasTile(cell);

        }

    }

}

