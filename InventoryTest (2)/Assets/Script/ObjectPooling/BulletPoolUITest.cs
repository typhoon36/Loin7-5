using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class BulletPoolUITest : MonoBehaviour
{
    [Header("풀 참조")]
    public BulletPool pool;

    [Header("발사 위치")]
    public Transform spawnPoint;     // 총알이 생성될 위치 (예: Spawner)

    [Header("UI 버튼")]
    public Button btnShootOne;       // 한 발 발사
    public Button btnAutoToggle;     // 연사 시작/정지 토글
    public Button btnReturnAll;      // 활성 총알 전부 회수

    [Header("상태 표시")]
    public Text txtStatus;

    [Header("연사 설정")]
    public float autoFireInterval = 0.1f;  // 연사 간격(초)

    // 내부 상태
    bool autoFiring = false;
    float autoFireTimer = 0f;
    int totalCreated = 0;                  // 총 생성된 오브젝트 수 추적

    // 활성 총알 추적 (전부 회수용)
    List<GameObject> activeBullets = new List<GameObject>();

    void Start()
    {
        if (pool == null)
            pool = GetComponent<BulletPool>();

        totalCreated = pool.initialSize;

        if (btnShootOne != null)
            btnShootOne.onClick.AddListener(ShootOne);
        if (btnAutoToggle != null)
            btnAutoToggle.onClick.AddListener(ToggleAutoFire);
        if (btnReturnAll != null)
            btnReturnAll.onClick.AddListener(ReturnAll);

        RefreshDisplay();
    }

    void Update()
    {
        // 연사 모드
        if (autoFiring)
        {
            autoFireTimer += Time.deltaTime;
            if (autoFireTimer >= autoFireInterval)
            {
                autoFireTimer = 0f;
                ShootOne();
            }
        }

        // 비활성화된 총알을 추적 리스트에서 제거
        activeBullets.RemoveAll(b => b == null || !b.activeInHierarchy);

        RefreshDisplay();
    }

    // --- 한 발 발사 ---
    void ShootOne()
    {
        if (pool == null || spawnPoint == null) return;

        // 풀에서 총알 가져오기
        var bullet = pool.GetBullet();
        bullet.transform.position = spawnPoint.position;
        bullet.transform.rotation = Quaternion.identity;

        // Bullet2D에 풀 참조 전달
        var b2d = bullet.GetComponent<Bullet2D>();
        if (b2d != null)
            b2d.SetPool(pool);

        // 활성 총알 추적
        if (!activeBullets.Contains(bullet))
            activeBullets.Add(bullet);

        // 풀이 비어서 새로 생성된 경우 카운트 갱신
        int currentTotal = pool.transform.childCount;
        if (currentTotal > totalCreated)
        {
            Debug.Log($"[풀 확장] 신규 Instantiate 발생! 총 생성: {currentTotal} (기존 {totalCreated})");
            totalCreated = currentTotal;
        }
    }

    // --- 연사 토글 ---
    void ToggleAutoFire()
    {
        autoFiring = !autoFiring;
        autoFireTimer = 0f;

        // 버튼 텍스트 변경
        if (btnAutoToggle != null)
        {
            var txt = btnAutoToggle.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = autoFiring ? "연사 정지" : "연사 시작";
        }

        Debug.Log(autoFiring ? "[연사 시작]" : "[연사 정지]");
    }

    // --- 전부 회수 ---
    void ReturnAll()
    {
        int count = 0;
        foreach (var bullet in activeBullets)
        {
            if (bullet != null && bullet.activeInHierarchy)
            {
                pool.ReturnBullet(bullet);
                count++;
            }
        }
        activeBullets.Clear();
        Debug.Log($"[전부 회수] {count}개 총알 반환");
    }

    // --- 상태 표시 ---
    void RefreshDisplay()
    {
        if (txtStatus == null) return;

        int activeCount = activeBullets.Count;
        int totalInPool = pool.transform.childCount;
        int waitingCount = totalInPool - activeCount;

        string text = "=== 객체 풀 상태 (Queue) ===\n";
        text += $"대기(풀): {waitingCount}개  |  활성(사용중): {activeCount}개  |  총 생성: {totalInPool}개\n";
        text += $"연사: {(autoFiring ? "ON" : "OFF")}  |  초기 풀 크기: {pool.initialSize}";

        if (totalInPool > pool.initialSize)
            text += $"\n※ 풀 확장 발생! (초기 {pool.initialSize} → 현재 {totalInPool})";

        txtStatus.text = text;
    }
}
