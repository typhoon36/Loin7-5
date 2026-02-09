using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUITest : MonoBehaviour
{
    [Header("인벤토리 참조")]
    public Inventory inventory;

    [Header("UI 버튼 (Inspector에서 연결)")]
    public Button btnAddPotion;
    public Button btnAddSword;
    public Button btnRemoveFirst;
    public Button btnClear;

    [Header("인벤토리 표시 텍스트")]
    public Text txtDisplay;  // UnityEngine.UI.Text 사용

    // 아이템 추가 시 번호를 붙이기 위한 카운터
    int potionCount = 0;
    int swordCount = 0;

    void Start()
    {
        // inventory가 비어있으면 같은 오브젝트에서 찾기
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        // 버튼 이벤트 연결
        if (btnAddPotion != null)
            btnAddPotion.onClick.AddListener(OnAddPotion);
        if (btnAddSword != null)
            btnAddSword.onClick.AddListener(OnAddSword);
        if (btnRemoveFirst != null)
            btnRemoveFirst.onClick.AddListener(OnRemoveFirst);
        if (btnClear != null)
            btnClear.onClick.AddListener(OnClear);

        RefreshDisplay();
    }

    // --- 버튼 콜백 ---

    void OnAddPotion()
    {
        potionCount++;
        inventory.AddItem(new Item("potion", $"포션 #{potionCount}"));
        LogCapacity();
        RefreshDisplay();
    }

    void OnAddSword()
    {
        swordCount++;
        inventory.AddItem(new Item("sword", $"검 #{swordCount}"));
        LogCapacity();
        RefreshDisplay();
    }

    void OnRemoveFirst()
    {
        if (inventory.items.Count == 0)
        {
            Debug.LogWarning("인벤토리가 비어있습니다!");
            return;
        }
        inventory.RemoveAt(0);
        RefreshDisplay();
    }

    void OnClear()
    {
        inventory.items.Clear();
        Debug.Log("인벤토리 전체 삭제");
        RefreshDisplay();
    }

    // --- 화면 갱신 ---

    void RefreshDisplay()
    {
        if (txtDisplay == null) return;

        var items = inventory.items;
        string text = $"=== 인벤토리 (Count: {items.Count} / Capacity: {items.Capacity}) ===\n\n";

        if (items.Count == 0)
        {
            text += "(비어 있음)";
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                text += $"[{i}] {items[i].displayName}  (id: {items[i].id})\n";
            }
        }

        txtDisplay.text = text;
    }

    // Capacity 변화를 콘솔에 출력 (List의 동적 배열 확장 관찰용)
    void LogCapacity()
    {
        Debug.Log($"  → List.Count = {inventory.items.Count}, List.Capacity = {inventory.items.Capacity}");
    }

}
