using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class EnemySorterUITest : MonoBehaviour
{
    [Header("EnemySorter 참조")]
    public EnemySorter sorter;

    [Header("생성 버튼")]
    public Button btnSpawn10;
    public Button btnSpawn50;
    public Button btnClear;

    [Header("정렬 버튼")]
    public Button btnSortDist;
    public Button btnSortName;
    public Button btnShuffle;

    [Header("탐색 버튼")]
    public Button btnLinearSearch;
    public Button btnBinarySearch;

    [Header("결과 표시")]
    public Text txtResult;

    [Header("생성 범위")]
    public float spawnRange = 7f;

    // 로그
    List<string> logs = new List<string>();
    // 현재 정렬 기준
    string currentSort = "없음";
    // 탐색 대상 (가장 가까운 적)
    Transform searchTarget;

    void Start()
    {
        if (sorter == null)
            sorter = GetComponent<EnemySorter>();

        if (btnSpawn10 != null) btnSpawn10.onClick.AddListener(() => SpawnEnemies(10));
        if (btnSpawn50 != null) btnSpawn50.onClick.AddListener(() => SpawnEnemies(50));
        if (btnClear != null) btnClear.onClick.AddListener(ClearEnemies);
        if (btnSortDist != null) btnSortDist.onClick.AddListener(SortByDistance);
        if (btnSortName != null) btnSortName.onClick.AddListener(SortByName);
        if (btnShuffle != null) btnShuffle.onClick.AddListener(Shuffle);
        if (btnLinearSearch != null) btnLinearSearch.onClick.AddListener(LinearSearch);
        if (btnBinarySearch != null) btnBinarySearch.onClick.AddListener(BinarySearch);

        RefreshDisplay();
    }

    // --- 적 생성 ---
    void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Enemy_{sorter.enemies.Count:D3}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSprite();
            sr.color = Color.red;
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            // 랜덤 위치 (플레이어 주변)
            Vector2 pos = Random.insideUnitCircle * spawnRange;
            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            sorter.enemies.Add(go.transform);
        }

        // 첫 번째 적을 탐색 대상으로 설정
        if (searchTarget == null && sorter.enemies.Count > 0)
            searchTarget = sorter.enemies[0];

        currentSort = "없음";
        AddLog($"적 {count}마리 생성! (총 {sorter.enemies.Count}마리)");
        RefreshDisplay();
    }

    // --- 전체 삭제 ---
    void ClearEnemies()
    {
        foreach (var e in sorter.enemies)
        {
            if (e != null) Destroy(e.gameObject);
        }
        sorter.enemies.Clear();
        searchTarget = null;
        currentSort = "없음";
        AddLog("적 전체 삭제!");
        RefreshDisplay();
    }

    // --- 거리순 정렬 ---
    void SortByDistance()
    {
        if (sorter.enemies.Count == 0) { AddLog("적이 없습니다!"); RefreshDisplay(); return; }

        var sw = Stopwatch.StartNew();
        sorter.SortByDistance();
        sw.Stop();

        // 가장 가까운 적을 탐색 대상으로
        searchTarget = sorter.enemies[0];
        currentSort = "거리순";

        // 시각적 번호 표시
        UpdateEnemyLabels();

        AddLog($"거리순 정렬 완료! ({sorter.enemies.Count}개, {sw.Elapsed.TotalMilliseconds:F3}ms)");
        AddLog($"  가장 가까운 적: {sorter.enemies[0].name} (거리: {GetDist(sorter.enemies[0]):F1})");
        RefreshDisplay();
    }

    // --- 이름순 정렬 ---
    void SortByName()
    {
        if (sorter.enemies.Count == 0) { AddLog("적이 없습니다!"); RefreshDisplay(); return; }

        var sw = Stopwatch.StartNew();
        sorter.enemies.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        sw.Stop();

        currentSort = "이름순";
        UpdateEnemyLabels();

        AddLog($"이름순 정렬 완료! ({sorter.enemies.Count}개, {sw.Elapsed.TotalMilliseconds:F3}ms)");
        RefreshDisplay();
    }

    // --- 셔플 ---
    void Shuffle()
    {
        if (sorter.enemies.Count == 0) { AddLog("적이 없습니다!"); RefreshDisplay(); return; }

        // Fisher-Yates 셔플
        for (int i = sorter.enemies.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = sorter.enemies[i];
            sorter.enemies[i] = sorter.enemies[j];
            sorter.enemies[j] = tmp;
        }

        currentSort = "없음 (셔플됨)";
        AddLog("목록을 랜덤으로 섞었습니다! (이진탐색 사용 불가)");
        RefreshDisplay();
    }

    // --- 선형 탐색 ---
    void LinearSearch()
    {
        if (sorter.enemies.Count == 0 || searchTarget == null)
        {
            AddLog("적이 없습니다!"); RefreshDisplay(); return;
        }

        float targetDist = GetSqrDist(searchTarget);
        int compareCount = 0;
        int foundIndex = -1;

        for (int i = 0; i < sorter.enemies.Count; i++)
        {
            compareCount++;
            float dist = GetSqrDist(sorter.enemies[i]);
            if (Mathf.Approximately(dist, targetDist))
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex >= 0)
            AddLog($"선형 탐색: {compareCount}번 비교 → [{foundIndex}] {searchTarget.name} 발견! (거리: {GetDist(searchTarget):F1})");
        else
            AddLog($"선형 탐색: {compareCount}번 비교 → 못 찾음!");

        RefreshDisplay();
    }

    // --- 이진 탐색 ---
    void BinarySearch()
    {
        if (sorter.enemies.Count == 0 || searchTarget == null)
        {
            AddLog("적이 없습니다!"); RefreshDisplay(); return;
        }

        if (currentSort != "거리순")
        {
            AddLog("이진 탐색 실패! 거리순 정렬이 필요합니다!");
            AddLog("  → 이진탐색은 반드시 정렬된 데이터에서만 사용 가능");
            RefreshDisplay();
            return;
        }

        float targetDist = GetSqrDist(searchTarget);
        int compareCount = 0;
        int low = 0, high = sorter.enemies.Count - 1;
        int foundIndex = -1;

        while (low <= high)
        {
            compareCount++;
            int mid = (low + high) / 2;
            float midDist = GetSqrDist(sorter.enemies[mid]);

            if (Mathf.Approximately(midDist, targetDist))
            {
                foundIndex = mid;
                break;
            }
            if (midDist < targetDist) low = mid + 1;
            else high = mid - 1;
        }

        if (foundIndex >= 0)
            AddLog($"이진 탐색: {compareCount}번 비교 → [{foundIndex}] {searchTarget.name} 발견! (거리: {GetDist(searchTarget):F1})");
        else
            AddLog($"이진 탐색: {compareCount}번 비교 → 못 찾음 (부동소수점 오차 가능)");

        RefreshDisplay();
    }

    // --- 시각적 번호 표시 ---
    void UpdateEnemyLabels()
    {
        for (int i = 0; i < sorter.enemies.Count; i++)
        {
            var sr = sorter.enemies[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // 가까운 적일수록 밝은 빨강, 먼 적일수록 어두운 빨강
                float t = (float)i / Mathf.Max(1, sorter.enemies.Count - 1);
                sr.color = Color.Lerp(Color.yellow, Color.red, t);
            }
        }
    }

    // --- 거리 유틸 ---
    float GetSqrDist(Transform t)
    {
        return (t.position - sorter.player.position).sqrMagnitude;
    }

    float GetDist(Transform t)
    {
        return Vector3.Distance(t.position, sorter.player.position);
    }

    // --- 간단한 스프라이트 생성 ---
    Sprite MakeSprite()
    {
        var tex = new Texture2D(4, 4);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    // --- 로그 관리 ---
    void AddLog(string msg)
    {
        logs.Add(msg);
        if (logs.Count > 6) logs.RemoveAt(0);
        Debug.Log($"[EnemySorter] {msg}");
    }

    // --- 화면 갱신 ---
    void RefreshDisplay()
    {
        if (txtResult == null) return;

        string text = $"=== 적 목록 ({currentSort}) ===\n";
        text += $"총 {sorter.enemies.Count}마리";
        if (searchTarget != null)
            text += $"  |  탐색 대상: {searchTarget.name}";
        text += "\n\n";

        // 목록 (최대 15개)
        int shown = 0;
        for (int i = 0; i < sorter.enemies.Count && shown < 15; i++)
        {
            var e = sorter.enemies[i];
            if (e == null) continue;
            float dist = GetDist(e);
            string marker = (e == searchTarget) ? " ← 탐색 대상" : "";
            text += $"  [{i + 1}] {e.name}  거리: {dist:F1}{marker}\n";
            shown++;
        }
        if (sorter.enemies.Count > 15)
            text += $"  ... 외 {sorter.enemies.Count - 15}마리\n";

        // 로그
        if (logs.Count > 0)
        {
            text += "\n--- 최근 동작 ---\n";
            foreach (var log in logs)
                text += $"  {log}\n";
        }

        txtResult.text = text;
    }
}
