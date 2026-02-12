
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreeUITest : MonoBehaviour
{
    [Header("트리 루트 (씬의 최상위 오브젝트)")]
    public Transform treeRoot;

    [Header("순회 버튼")]
    public Button btnDfsPreOrder;
    public Button btnDfsPostOrder;
    public Button btnBfs;
    public Button btnReset;

    [Header("결과 표시")]
    public Text txtResult;

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;  // 방문 시 색상
    public float highlightDelay = 0.4f;          // 방문 간격(초)

    // 원래 색상 저장
    Dictionary<Transform, Color> originalColors = new Dictionary<Transform, Color>();
    Coroutine currentTraversal;

    void Start()
    {
        // 원래 색상 저장
        if (treeRoot != null)
            SaveOriginalColors(treeRoot);

        if (btnDfsPreOrder != null)
            btnDfsPreOrder.onClick.AddListener(() => StartTraversal("DFS_Pre"));
        if (btnDfsPostOrder != null)
            btnDfsPostOrder.onClick.AddListener(() => StartTraversal("DFS_Post"));
        if (btnBfs != null)
            btnBfs.onClick.AddListener(() => StartTraversal("BFS"));
        if (btnReset != null)
            btnReset.onClick.AddListener(ResetColors);

        if (txtResult != null)
            txtResult.text = "버튼을 눌러 순회를 시작하세요.";
    }

    void SaveOriginalColors(Transform node)
    {
        var sr = node.GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColors[node] = sr.color;
        foreach (Transform child in node)
            SaveOriginalColors(child);
    }

    void ResetColors()
    {
        if (currentTraversal != null)
            StopCoroutine(currentTraversal);

        foreach (var pair in originalColors)
        {
            var sr = pair.Key.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = pair.Value;
        }

        if (txtResult != null)
            txtResult.text = "초기화 완료. 버튼을 눌러 순회를 시작하세요.";
    }

    // --- 순회 시작 ---
    void StartTraversal(string mode)
    {
        if (treeRoot == null) return;
        ResetColors();

        // 순회 결과 수집
        List<(Transform node, int depth)> visitOrder = new List<(Transform, int)>();

        switch (mode)
        {
            case "DFS_Pre":
                DfsPreOrder(treeRoot, 0, visitOrder);
                break;
            case "DFS_Post":
                DfsPostOrder(treeRoot, 0, visitOrder);
                break;
            case "BFS":
                BfsTraversal(treeRoot, visitOrder);
                break;
        }

        // 결과 텍스트 생성
        string label = mode == "DFS_Pre" ? "DFS 전위 (Pre-order)" :
                       mode == "DFS_Post" ? "DFS 후위 (Post-order)" :
                       "BFS 레벨 순서 (Breadth-first)";

        string resultText = $"=== {label} ===\n\n";
        for (int i = 0; i < visitOrder.Count; i++)
        {
            var (node, depth) = visitOrder[i];
            string indent = new string(' ', depth * 2);
            resultText += $"  [{i + 1}] {indent}{node.name} (깊이 {depth})\n";
        }

        string structure = mode == "DFS_Pre" ? "스택/재귀 (부모→자식)" :
                           mode == "DFS_Post" ? "스택/재귀 (자식→부모)" :
                           "큐 (레벨 단위)";
        resultText += $"\n사용 자료구조: {structure}";
        resultText += $"\n총 노드 수: {visitOrder.Count}개";

        if (txtResult != null)
            txtResult.text = resultText;

        // 하이라이트 애니메이션
        currentTraversal = StartCoroutine(HighlightSequence(visitOrder));

        // 콘솔 로그
        string names = "";
        foreach (var (node, _) in visitOrder)
            names += node.name + " → ";
        Debug.Log($"[{label}] {names.TrimEnd(' ', '→')}");
    }

    // --- DFS 전위 순회: 방문 → 자식들 ---
    void DfsPreOrder(Transform node, int depth, List<(Transform, int)> result)
    {
        result.Add((node, depth));       // 먼저 방문
        foreach (Transform child in node)
            DfsPreOrder(child, depth + 1, result);
    }

    // --- DFS 후위 순회: 자식들 → 방문 ---
    void DfsPostOrder(Transform node, int depth, List<(Transform, int)> result)
    {
        foreach (Transform child in node)
            DfsPostOrder(child, depth + 1, result);
        result.Add((node, depth));       // 자식 다 방문 후 방문
    }

    // --- BFS 레벨 순서 순회: 큐 사용 ---
    void BfsTraversal(Transform root, List<(Transform, int)> result)
    {
        Queue<(Transform node, int depth)> queue = new Queue<(Transform, int)>();
        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            var (node, depth) = queue.Dequeue();
            result.Add((node, depth));

            foreach (Transform child in node)
                queue.Enqueue((child, depth + 1));
        }
    }

    // --- 하이라이트 애니메이션 ---
    System.Collections.IEnumerator HighlightSequence(List<(Transform node, int depth)> order)
    {
        foreach (var (node, _) in order)
        {
            var sr = node.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = highlightColor;

            yield return new WaitForSeconds(highlightDelay);
        }
    }
}
