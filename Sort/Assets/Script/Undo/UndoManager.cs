using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    Stack<ICommand> undoStack = new Stack<ICommand>(); // 실행된 명령을 저장하는 스택
    Stack<ICommand> redoStack = new Stack<ICommand>(); // 취소된 명령을 저장하는 스택

    // 명령 실행

    public void Do(ICommand cmd)
    {
        cmd.Execute(); // 명령 실행
        undoStack.Push(cmd); // 실행된 명령을 undo 스택에 저장
        redoStack.Clear(); // 새로운 명령이 실행되면 redo 스택은 초기화
    }

    // 실행 취소
    public void Undo()
    {
        if (undoStack.Count == 0) return;

        var cmd = undoStack.Pop();

        cmd.Undo();

        redoStack.Push(cmd); 

    }


    // 다시 실행
    public void Redo()
    {
        if (redoStack.Count == 0) return;

        var cmd = redoStack.Pop(); 

        cmd.Execute();

        undoStack.Push(cmd);


    }
}


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
