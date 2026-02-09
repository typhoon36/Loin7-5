using System.Collections.Generic;
using UnityEngine;



// 간단한 게임 아이템 모델

public class Item
{

    // 고유 ID (예: "potion")

    public string id;

    // 표시명(인벤토리 UI에 보여줄 문자열)

    public string displayName;



    public Item(string id, string name)
    {

        this.id = id;

        this.displayName = name;

    }

}
