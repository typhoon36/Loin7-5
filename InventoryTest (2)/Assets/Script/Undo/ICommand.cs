using UnityEngine;

// ICommand: 커맨드 패턴 인터페이스

//-Execute(): 명령 실행

//-Undo(): 실행 취소


public interface ICommand 
{

    void Execute();
    void Undo();

}
