using UnityEngine;
using UnityEngine.UI;

public class UndoUITest : MonoBehaviour
{
    [Header("이동 대상 (2D 스프라이트)")]
    public Transform target;         // 이동시킬 오브젝트 (예: Square 스프라이트)

    [Header("Undo 매니저")]
    public UndoManager undoManager;  // 같은 오브젝트에 있으면 자동 연결

    [Header("방향 이동 버튼 (4방향)")]
    public Button btnMoveUp;
    public Button btnMoveDown;
    public Button btnMoveLeft;
    public Button btnMoveRight;

    [Header("Undo/Redo 버튼")]
    public Button btnUndo;
    public Button btnRedo;

    [Header("상태 표시")]
    public Text txtStatus;         

    [Header("설정")]
    public float moveDistance = 1f;   // 한 번에 이동할 거리

    // 명령 히스토리 (UI 표시용)
    System.Collections.Generic.List<string> history = new System.Collections.Generic.List<string>();
    int historyIndex = -1;

    void Start()
    {
        if (undoManager == null)
            undoManager = GetComponent<UndoManager>();

        // 버튼 이벤트 연결
        if (btnMoveUp != null)
            btnMoveUp.onClick.AddListener(() => DoMove(Vector3.up * moveDistance, "↑ 위"));
        if (btnMoveDown != null)
            btnMoveDown.onClick.AddListener(() => DoMove(Vector3.down * moveDistance, "↓ 아래"));
        if (btnMoveLeft != null)
            btnMoveLeft.onClick.AddListener(() => DoMove(Vector3.left * moveDistance, "← 왼쪽"));
        if (btnMoveRight != null)
            btnMoveRight.onClick.AddListener(() => DoMove(Vector3.right * moveDistance, "→ 오른쪽"));

        if (btnUndo != null)
            btnUndo.onClick.AddListener(OnUndo);
        if (btnRedo != null)
            btnRedo.onClick.AddListener(OnRedo);

        RefreshDisplay();
    }

    // --- 이동 명령 실행 ---
    void DoMove(Vector3 offset, string direction)
    {
        if (target == null) return;

        Vector3 from = target.position;
        Vector3 to = from + offset;
        var cmd = new MoveCommand(target, from, to);
        undoManager.Do(cmd);

        // 히스토리 기록 (undo 이후 새 명령이면 분기 제거)
        if (historyIndex < history.Count - 1)
            history.RemoveRange(historyIndex + 1, history.Count - historyIndex - 1);

        history.Add($"{direction}  ({from.x:F0},{from.y:F0}) → ({to.x:F0},{to.y:F0})");
        historyIndex = history.Count - 1;

        Debug.Log($"[Do] {direction}: ({from.x:F0},{from.y:F0}) → ({to.x:F0},{to.y:F0})");
        RefreshDisplay();
    }

    void OnUndo()
    {
        if (historyIndex < 0)
        {
            Debug.LogWarning("Undo할 명령이 없습니다!");
            return;
        }
        undoManager.Undo();
        Debug.Log($"[Undo] {history[historyIndex]}");
        historyIndex--;
        RefreshDisplay();
    }

    void OnRedo()
    {
        if (historyIndex >= history.Count - 1)
        {
            Debug.LogWarning("Redo할 명령이 없습니다!");
            return;
        }
        undoManager.Redo();
        historyIndex++;
        Debug.Log($"[Redo] {history[historyIndex]}");
        RefreshDisplay();
    }

    // --- 상태 표시 갱신 ---
    void RefreshDisplay()
    {
        if (txtStatus == null) return;

        string text = "=== Undo/Redo 스택 상태 ===\n\n";

        if (target != null)
            text += $"스퀘어 위치: ({target.position.x:F0}, {target.position.y:F0})\n\n";

        text += "--- 명령 히스토리 ---\n";
        if (history.Count == 0)
        {
            text += "(비어 있음)\n";
        }
        else
        {
            for (int i = 0; i < history.Count; i++)
            {
                string marker;
                if (i == historyIndex)
                    marker = "  ◀ 현재";
                else if (i > historyIndex)
                    marker = "  (redo 가능)";
                else
                    marker = "";

                text += $"  [{i + 1}] {history[i]}{marker}\n";
            }
        }

        text += $"\nUndo 가능: {historyIndex + 1}개";
        text += $"\nRedo 가능: {history.Count - historyIndex - 1}개";

        txtStatus.text = text;
    }
}
