using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCacheUITest : MonoBehaviour
{
    [Header("ResourceCache 참조")]
    public ResourceCache resourceCache;

    [Header("프리팹 (없으면 자동 생성)")]
    public GameObject swordPrefab;
    public GameObject shieldPrefab;
    public GameObject potionPrefab;

    [Header("버튼")]
    public Button btnLoadSword;
    public Button btnLoadShield;
    public Button btnLoadPotion;
    public Button btnClearCache;
    public Button btnRepeatLoad;

    [Header("결과 표시")]
    public Text txtResult;

    // 통계 추적
    int hitCount = 0;
    int missCount = 0;
    int loaderCallCount = 0;
    List<string> logs = new List<string>();

    // 캐시된 키 목록 (표시용)
    List<string> cachedKeys = new List<string>();

    // 스폰 위치
    float spawnX = -3f;

    void Start()
    {
        if (resourceCache == null)
            resourceCache = GetComponent<ResourceCache>();

        if (btnLoadSword != null)
            btnLoadSword.onClick.AddListener(() => LoadResource("Sword"));
        if (btnLoadShield != null)
            btnLoadShield.onClick.AddListener(() => LoadResource("Shield"));
        if (btnLoadPotion != null)
            btnLoadPotion.onClick.AddListener(() => LoadResource("Potion"));
        if (btnClearCache != null)
            btnClearCache.onClick.AddListener(ClearCache);
        if (btnRepeatLoad != null)
            btnRepeatLoad.onClick.AddListener(RepeatLoad);

        RefreshDisplay();
    }

    // --- 리소스 로드 ---
    void LoadResource(string key)
    {
        bool wasCached = resourceCache.TryGet(key, out _);

        // ResourceCache.Get 호출 — loader는 캐시 미스일 때만 실행됨
        var obj = resourceCache.Get(key, () =>
        {
            // ★ 이 loader는 캐시에 없을 때만 호출됩니다!
            loaderCallCount++;
            Debug.Log($"[Loader 호출] \"{key}\" — 새로 생성 (비용 큰 작업!)");
            return CreatePrefab(key);
        });

        if (wasCached)
        {
            hitCount++;
            AddLog($"HIT  \"{key}\" → 캐시 히트! loader 호출 안 함 [O(1)]");
        }
        else
        {
            missCount++;
            if (!cachedKeys.Contains(key))
                cachedKeys.Add(key);
            AddLog($"MISS \"{key}\" → 캐시 미스! loader 호출됨 (#{loaderCallCount})");
        }

        RefreshDisplay();
    }

    // --- 프리팹 생성 (loader 역할) ---
    GameObject CreatePrefab(string key)
    {
        GameObject prefab = null;
        switch (key)
        {
            case "Sword": prefab = swordPrefab; break;
            case "Shield": prefab = shieldPrefab; break;
            case "Potion": prefab = potionPrefab; break;
        }

        GameObject obj;
        if (prefab != null)
        {
            obj = Instantiate(prefab);
        }
        else
        {
            // 프리팹이 없으면 스프라이트를 직접 생성
            obj = new GameObject(key);
            var sr = obj.AddComponent<SpriteRenderer>();

            // 기본 Square 스프라이트 사용
            sr.sprite = MakeSprite();

            switch (key)
            {
                case "Sword": sr.color = Color.red; break;
                case "Shield": sr.color = Color.blue; break;
                case "Potion": sr.color = Color.green; break;
                default: sr.color = Color.white; break;
            }

            obj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        }

        // 스폰 위치 배치
        obj.transform.position = new Vector3(spawnX, -2f, 0f);
        spawnX += 1.5f;

        obj.name = $"[Cached] {key}";
        return obj;
    }

    // --- 간단한 흰색 스프라이트 생성 ---
    Sprite MakeSprite()
    {
        var tex = new Texture2D(4, 4);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
    }

    // --- 10번 반복 로드 ---
    void RepeatLoad()
    {
        string[] keys = { "Sword", "Shield", "Potion", "Sword", "Shield",
                          "Potion", "Sword", "Sword", "Shield", "Potion" };

        int prevLoaderCount = loaderCallCount;

        foreach (var key in keys)
        {
            bool wasCached = resourceCache.TryGet(key, out _);

            resourceCache.Get(key, () =>
            {
                loaderCallCount++;
                return CreatePrefab(key);
            });

            if (wasCached)
                hitCount++;
            else
            {
                missCount++;
                if (!cachedKeys.Contains(key))
                    cachedKeys.Add(key);
            }
        }

        int newCalls = loaderCallCount - prevLoaderCount;
        AddLog($"10번 반복 로드 완료!");
        AddLog($"  → loader 호출: {newCalls}회 (캐시 덕분에 {10 - newCalls}회 절약!)");

        RefreshDisplay();
    }

    // --- 캐시 전체 삭제 ---
    void ClearCache()
    {
        // ResourceCache에 Clear가 없으므로 새 컴포넌트로 교체
        Destroy(resourceCache);
        resourceCache = gameObject.AddComponent<ResourceCache>();

        // 생성된 오브젝트도 삭제
        var cached = GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var sr in cached)
        {
            if (sr.gameObject.name.StartsWith("[Cached]"))
                Destroy(sr.gameObject);
        }

        spawnX = -3f;
        cachedKeys.Clear();
        hitCount = 0;
        missCount = 0;
        loaderCallCount = 0;

        AddLog("캐시 전체 삭제! 다시 로드하면 loader가 호출됩니다.");
        RefreshDisplay();
    }

    // --- 로그 관리 ---
    void AddLog(string msg)
    {
        logs.Add(msg);
        if (logs.Count > 6) logs.RemoveAt(0);
        Debug.Log($"[ResourceCache] {msg}");
    }

    // --- 화면 갱신 ---
    void RefreshDisplay()
    {
        if (txtResult == null) return;

        int total = hitCount + missCount;
        float hitRate = total > 0 ? (float)hitCount / total * 100f : 0f;

        string text = "=== ResourceCache 상태 ===\n";
        text += $"캐시 항목: {cachedKeys.Count}개";
        text += $"  |  히트: {hitCount}  미스: {missCount}";
        text += $"  |  히트율: {hitRate:F0}%\n";
        text += $"Loader 호출: {loaderCallCount}회";
        if (total > loaderCallCount && loaderCallCount > 0)
            text += $"  (캐시 덕분에 {total - loaderCallCount}회 절약!)";
        text += "\n\n";

        // 캐시 항목 목록
        foreach (var key in cachedKeys)
        {
            text += $"  [{key}] → 캐시됨 (Hash: {key.GetHashCode()})\n";
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
}
