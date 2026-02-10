using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class TreeUITest : MonoBehaviour
{
    // 순회 방식 (사용자가 버튼으로 선택)
    // - DFS_Pre: 부모를 먼저 보고 자식들을 차례로 봅니다.
    // - DFS_Post: 자식들을 모두 본 다음 부모를 봅니다.
    // - BFS: 같은 높이(레벨)에 있는 것들을 먼저 보고 다음 레벨로 내려갑니다.

    enum TraversalMode { DFS_Pre, DFS_Post, BFS }

    [Header("트리 루트 (씬의 최상위 오브젝트)")]
                    [SerializeField] Transform treeRoot; // 순회를 시작할 최상위 오브젝트

    [Header("순회 버튼")]
    [SerializeField] Button btnDfsPreOrder;   // '부모 먼저' 순회 버튼
    [SerializeField] Button btnDfsPostOrder;  // '자식 먼저' 순회 버튼
    [SerializeField] Button btnBfs;           // 레벨 순서(BFS) 버튼
    [SerializeField] Button btnReset;         // 색상 원복 버튼

    [Header("결과 표시")]
    [SerializeField] Text txtResult; // 순회 결과를 보여줄 텍스트 박스

    [Header("하이라이트 설정")]
    [SerializeField] Color highlightColor = Color.yellow; // 방문할 때 잠시 바꿀 색
    [SerializeField] float highlightDelay = 0.4f;        // 각 노드별 대기 시간(초)
    [SerializeField] bool revertAfterVisit = true;       // 방문 후 원래 색으로 되돌릴지 여부

    // 성능을 위해 각 Transform에 붙은 SpriteRenderer를 미리 저장합니다.
    Dictionary<Transform, SpriteRenderer> rendererCache = new Dictionary<Transform, SpriteRenderer>();

    // 각 오브젝트의 원래 색을 저장해 두었다가 필요할 때 복원합니다.
    Dictionary<Transform, Color> originalColors = new Dictionary<Transform, Color>();

    // 현재 강조(하이라이트) 동작을 나타내는 코루틴 참조(동시 실행 방지용)
    Coroutine currentTraversal;

    void Start()
    {
        // 씬의 트리를 한번 훑어서 렌더러와 색 정보를 저장해 둡니다.
        CacheRenderersAndColors();

        // 버튼에 클릭 이벤트 연결 (버튼이 없는 경우 안전하게 건너뜀)
        if (btnDfsPreOrder != null)
            btnDfsPreOrder.onClick.AddListener(() => StartTraversal(TraversalMode.DFS_Pre));
        if (btnDfsPostOrder != null)
            btnDfsPostOrder.onClick.AddListener(() => StartTraversal(TraversalMode.DFS_Post));
        if (btnBfs != null)
            btnBfs.onClick.AddListener(() => StartTraversal(TraversalMode.BFS));
        if (btnReset != null)
            btnReset.onClick.AddListener(ResetColors);

        if (txtResult != null)
            txtResult.text = "버튼을 눌러 순회를 시작하세요.";
    }

    /// <summary>
    /// 씬 안의 트리를 한 번 훑으면서, 화면에 보이는(또는 색을 바꿀 수 있는)
    /// SpriteRenderer를 찾아서 캐시하고 원래 색을 기록해 둡니다.
    /// </summary>
    void CacheRenderersAndColors()
    {
        rendererCache.Clear();
        originalColors.Clear();

        if (treeRoot == null) return;

        var stack = new Stack<Transform>();
        stack.Push(treeRoot);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                rendererCache[t] = sr;
                originalColors[t] = sr.color;
            }

            // 자식들을 추가해서 트리 전체를 확인합니다.
            foreach (Transform child in t)
                stack.Push(child);
        }
    }

    /// <summary>
    /// 현재 진행 중인 강조를 멈추고, 저장해둔 원래 색으로 모두 되돌립니다.
    /// 사용자가 '초기화' 버튼을 누르거나 새 순회를 시작할 때 호출됩니다.
    /// </summary>
    void ResetColors()
    {
        CancelCurrentTraversal();

        foreach (var kv in rendererCache)
        {
            if (kv.Value != null && originalColors.TryGetValue(kv.Key, out var col))
                kv.Value.color = col;
        }

        if (txtResult != null)
            txtResult.text = "초기화 완료. 버튼을 눌러 순회를 시작하세요.";
    }

    /// <summary>
    /// 현재 실행 중인 하이라이트 코루틴이 있으면 안전하게 중지합니다.
    /// </summary>
    void CancelCurrentTraversal()
    {
        if (currentTraversal != null)
        {
            StopCoroutine(currentTraversal);
            currentTraversal = null;
        }
    }

    /// <summary>
    /// 사용자가 선택한 방식으로 트리를 훑고(순회),
    /// 그 결과를 텍스트로 만들어 보여준 뒤 하이라이트를 시작합니다.
    /// </summary>
    void StartTraversal(TraversalMode mode)
    {
        if (treeRoot == null) return;

        // 이전 상태를 초기화(색 복원 및 기존 코루틴 취소)
        ResetColors();

        // 방문 순서와 각 노드의 깊이 정보를 모아둡니다.
        var visitOrder = new List<(Transform node, int depth)>();

        switch (mode)
        {
            case TraversalMode.DFS_Pre:
                DfsPreOrder(treeRoot, 0, visitOrder);
                break;
            case TraversalMode.DFS_Post:
                DfsPostOrder(treeRoot, 0, visitOrder);
                break;
            case TraversalMode.BFS:
                BfsTraversal(treeRoot, visitOrder);
                break;
        }

        BuildResultText(mode, visitOrder);

        // 중복 실행을 막고 새 하이라이트를 시작합니다.
        CancelCurrentTraversal();
        currentTraversal = StartCoroutine(HighlightSequence(visitOrder));
    }

    /// <summary>
    /// 방문 순서와 간단한 통계(총 노드 수, 사용 자료구조 등)를
    /// 사람 읽기 쉬운 문장으로 만들어 화면에 표시합니다.
    /// 또한 콘솔에는 방문한 노드 이름을 간단히 출력합니다.
    /// </summary>
    void BuildResultText(TraversalMode mode, List<(Transform node, int depth)> visitOrder)
    {
        string label = mode == TraversalMode.DFS_Pre ? "DFS 전위 (Pre-order)" :
                       mode == TraversalMode.DFS_Post ? "DFS 후위 (Post-order)" :
                       "BFS 레벨 순서 (Breadth-first)";

        var sb = new StringBuilder();
        sb.AppendLine($"=== {label} ===");
        sb.AppendLine();

        // 깊이에 따라 들여쓰기해서 트리 모양을 보기 쉽게 표시합니다.
        for (int i = 0; i < visitOrder.Count; i++)
        {
            var (node, depth) = visitOrder[i];
            sb.Append(' ', depth * 2);
            sb.AppendLine($"[{i + 1}] {node.name} (깊이 {depth})");
        }

        string structure = mode == TraversalMode.DFS_Pre ? "스택/재귀 (부모→자식)" :
                           mode == TraversalMode.DFS_Post ? "스택/재귀 (자식→부모)" :
                           "큐 (레벨 단위)";

        sb.AppendLine();
        sb.AppendLine($"사용 자료구조: {structure}");
        sb.AppendLine($"총 노드 수: {visitOrder.Count}개");

        if (txtResult != null)
            txtResult.text = sb.ToString();

        // 콘솔 로그: 방문된 노드 이름을 화살표로 연결해 출력
        var names = new StringBuilder();
        foreach (var (node, _) in visitOrder)
        {
            if (names.Length > 0) names.Append(" → ");
            names.Append(node.name);
        }
        Debug.Log($"[{label}] {names}");
    }

    /// <summary>
    /// 전위 순회(부모 먼저 보기)
    /// - 현재 노드를 기록한 뒤 자식들을 차례로 순회합니다.
    /// </summary>
    void DfsPreOrder(Transform node, int depth, List<(Transform, int)> result)
    {
        result.Add((node, depth));
        foreach (Transform child in node)
            DfsPreOrder(child, depth + 1, result);
    }

    /// <summary>
    /// 후위 순회(자식 먼저 보기)
    /// - 자식들을 모두 기록한 다음에 부모를 기록합니다.
    /// </summary>
    void DfsPostOrder(Transform node, int depth, List<(Transform, int)> result)
    {
        foreach (Transform child in node)
            DfsPostOrder(child, depth + 1, result);
        result.Add((node, depth));
    }

    /// <summary>
    /// 너비 우선(BFS)
    /// - 같은 높이에 있는 것들을 먼저 보고, 다음 높이로 내려가며 순회합니다.
    /// </summary>
    void BfsTraversal(Transform root, List<(Transform, int)> result)
    {
        var queue = new Queue<(Transform node, int depth)>();
        queue.Enqueue((root, 0));
        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            result.Add((node, depth));
            foreach (Transform child in node)
                queue.Enqueue((child, depth + 1));
        }
    }

    /// <summary>
    /// 모아둔 방문 순서대로 각 노드를 잠시 다른 색으로 바꿔서 강조합니다.
    /// - 화면에 보이는 노드는 지정한 색으로 바뀌며, 설정에 따라 원래 색으로 되돌립니다.
    /// - 화면에 보이지 않거나 렌더러가 없으면 짧게 건너뜁니다.
    /// </summary>
    IEnumerator HighlightSequence(List<(Transform node, int depth)> order)
    {
        foreach (var (node, _) in order)
        {
            if (node == null) continue;

            if (rendererCache.TryGetValue(node, out var sr) && sr != null)
            {
                var before = originalColors.TryGetValue(node, out var orig) ? orig : sr.color;
                sr.color = highlightColor;

                yield return new WaitForSeconds(highlightDelay);

                if (revertAfterVisit)
                    sr.color = before;
            }
            else
            {
                // 렌더러가 없으면 빠르게 건너뜁니다.
                yield return new WaitForSeconds(highlightDelay / 2f);
            }
        }

        // 완료 표시: 참조를 해제합니다.
        currentTraversal = null;
    }
}
