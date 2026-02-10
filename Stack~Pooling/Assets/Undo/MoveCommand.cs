using UnityEngine;

// MoveCommand: Transform의 위치를 바꾸는 간단한 커맨드 예제
public class MoveCommand : ICommand
{

    Transform t;

    Vector3 before, after;



    public MoveCommand(Transform t, Vector3 from, Vector3 to)
    {

        this.t = t;

        before = from;

        after = to;

    }



    public void Execute()
    {

        if (t) t.position = after;

    }



    public void Undo()
    {

        if (t) t.position = before;

    }


}
