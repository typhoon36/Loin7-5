using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // 인벤토리 내부 저장소 (List<T> 사용)

    public List<Item> items = new List<Item>();



    // 아이템 추가: List.Add는 평균 O(1) (capacity 확장 시 복사가 발생할 수 있음)

    public void AddItem(Item item)

    {

        items.Add(item);

        Debug.Log($"Added: {item.displayName} (count={items.Count})");

    }



    // 인덱스 기준 제거: RemoveAt은 중간 요소 제거 시 O(n)

    public void RemoveAt(int index)

    {

        if (index < 0 || index >= items.Count) return;

        var it = items[index];

        items.RemoveAt(index);

        Debug.Log($"Removed: {it.displayName} (count={items.Count})");

    }



    // 특정 id를 가진 아이템의 인덱스 찾기 (선형 탐색)

    public int IndexOf(string id) => items.FindIndex(i => i.id == id);
}
