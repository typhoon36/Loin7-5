using UnityEngine;

public class Item 
{
    //아이템의 고유아이디
    public string id;
    //표시명(인벤토리 UI에 보여줄 문자열)
    public string displayName;

    public Item(string id, string name)
    {

        this.id = id;

        this.displayName = name;

    }
}
