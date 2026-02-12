using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HashDictUITest : MonoBehaviour
{
    [Header("입력 필드")]
    public InputField inputKey;
    public InputField inputValue;

    [Header("버튼")]
    public Button btnAdd;
    public Button btnGet;
    public Button btnRemove;
    public Button btnClear;
    public Button btnHashCode;
    public Button btnAddRandom;

    [Header("결과 표시")]
    public Text txtResult;

    // 내부 Dictionary (해시테이블)
    Dictionary<string, string> dict = new Dictionary<string, string>();

    // 동작 로그
    List<string> logs = new List<string>();
    int hitCount = 0;
    int missCount = 0;

    void Start()
    {
        if (btnAdd != null) btnAdd.onClick.AddListener(OnAdd);
        if (btnGet != null) btnGet.onClick.AddListener(OnGet);
        if (btnRemove != null) btnRemove.onClick.AddListener(OnRemove);
        if (btnClear != null) btnClear.onClick.AddListener(OnClear);
        if (btnHashCode != null) btnHashCode.onClick.AddListener(OnShowHashCode);
        if (btnAddRandom != null) btnAddRandom.onClick.AddListener(OnAddRandom);

        RefreshDisplay();
    }

    // --- 추가/수정 ---
    void OnAdd()
    {
        string key = GetKey();
        string value = GetValue();
        if (string.IsNullOrEmpty(key))
        {
            AddLog("키를 입력하세요!");
            RefreshDisplay();
            return;
        }

        bool existed = dict.ContainsKey(key);
        dict[key] = value;

        if (existed)
            AddLog($"\"{key}\" 값 수정: \"{value}\" (기존 값 덮어쓰기)");
        else
            AddLog($"\"{key}\" 추가: \"{value}\" (Hash: {key.GetHashCode()})");

        RefreshDisplay();
    }

    // --- 조회 ---
    void OnGet()
    {
        string key = GetKey();
        if (string.IsNullOrEmpty(key))
        {
            AddLog("키를 입력하세요!");
            RefreshDisplay();
            return;
        }

        if (dict.TryGetValue(key, out string value))
        {
            hitCount++;
            AddLog($"\"{key}\" 조회: 캐시 히트! 값=\"{value}\" [O(1)]");
        }
        else
        {
            missCount++;
            AddLog($"\"{key}\" 조회: 캐시 미스! 해당 키 없음");
        }

        RefreshDisplay();
    }

    // --- 삭제 ---
    void OnRemove()
    {
        string key = GetKey();
        if (string.IsNullOrEmpty(key))
        {
            AddLog("키를 입력하세요!");
            RefreshDisplay();
            return;
        }

        if (dict.Remove(key))
            AddLog($"\"{key}\" 삭제 완료");
        else
            AddLog($"\"{key}\" 삭제 실패: 해당 키 없음");

        RefreshDisplay();
    }

    // --- 전체 삭제 ---
    void OnClear()
    {
        int count = dict.Count;
        dict.Clear();
        hitCount = 0;
        missCount = 0;
        AddLog($"전체 삭제! ({count}개 항목 제거)");
        RefreshDisplay();
    }

    // --- 해시코드 보기 ---
    void OnShowHashCode()
    {
        string key = GetKey();
        if (string.IsNullOrEmpty(key))
        {
            AddLog("키를 입력하세요!");
            RefreshDisplay();
            return;
        }

        int hash = key.GetHashCode();
        string lower = key.ToLower();
        int hashLower = lower.GetHashCode();

        string msg = $"GetHashCode 결과:\n";
        msg += $"  \"{key}\" → {hash}\n";
        if (key != lower)
            msg += $"  \"{lower}\" (소문자) → {hashLower}\n";
        msg += $"  같은 문자열 다시: \"{key}\" → {key.GetHashCode()} (동일!)\n";

        // 버킷 인덱스 시뮬레이션
        int bucketCount = 8;
        int bucketIndex = (hash & 0x7FFFFFFF) % bucketCount;
        msg += $"  버킷 인덱스 (버킷 {bucketCount}개 기준): {bucketIndex}번";

        AddLog(msg);
        RefreshDisplay();
    }

    // --- 랜덤 100개 추가 ---
    void OnAddRandom()
    {
        string[] itemTypes = { "Sword", "Shield", "Potion", "Armor", "Ring", "Staff", "Bow", "Helmet", "Boots", "Gloves" };
        int addedCount = 0;

        for (int i = 0; i < 100; i++)
        {
            string itemName = itemTypes[Random.Range(0, itemTypes.Length)];
            string key = $"{itemName}_{i:D3}";
            string value = $"{itemName} Lv.{Random.Range(1, 100)}";
            dict[key] = value;
            addedCount++;
        }

        AddLog($"랜덤 {addedCount}개 추가! 총 {dict.Count}개 — 그래도 조회는 O(1)!");
        RefreshDisplay();
    }

    // --- 로그 관리 ---
    void AddLog(string msg)
    {
        logs.Add(msg);
        if (logs.Count > 5) logs.RemoveAt(0);
        Debug.Log($"[Dictionary] {msg}");
    }

    // --- 화면 갱신 ---
    void RefreshDisplay()
    {
        if (txtResult == null) return;

        string text = "=== Dictionary(해시테이블) 상태 ===\n";
        text += $"항목 수: {dict.Count}개";
        text += $"  |  히트: {hitCount}  미스: {missCount}\n\n";

        // 항목 목록 (최대 15개만 표시)
        int shown = 0;
        foreach (var pair in dict)
        {
            if (shown >= 15)
            {
                text += $"  ... 외 {dict.Count - 15}개\n";
                break;
            }
            int hash = pair.Key.GetHashCode();
            text += $"  [{pair.Key}] = \"{pair.Value}\"  (Hash: {hash})\n";
            shown++;
        }

        // 로그
        if (logs.Count > 0)
        {
            text += "\n--- 최근 동작 ---\n";
            foreach (var log in logs)
                text += $"  {log}\n";
        }

        txtResult.text = text;
    }

    // --- 유틸 ---
    string GetKey()
    {
        return inputKey != null ? inputKey.text.Trim() : "";
    }

    string GetValue()
    {
        return inputValue != null ? inputValue.text.Trim() : "값 없음";
    }
}
