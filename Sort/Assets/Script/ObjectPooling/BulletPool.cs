using System.Collections.Generic;

using UnityEngine;



// BulletPool: 간단한 객체 풀 구현 예제

// 목적: 런타임 동안 Instantiate/Destroy를 반복 호출하지 않고 재사용하여 성능 개선

public class BulletPool : MonoBehaviour

{

    // 풀에서 생성할 프리팹

    public GameObject bulletPrefab;

    // 초기 풀 크기

    public int initialSize = 20;

    // 내부 큐(선입선출)로 객체 관리

    Queue<GameObject> pool = new Queue<GameObject>();



    void Start()

    {

        // 초기 풀 채우기(비활성 상태)

        for (int i = 0; i < initialSize; i++)

        {

            var b = Instantiate(bulletPrefab, transform);

            b.SetActive(false);

            pool.Enqueue(b);

        }

    }



    // 풀에서 하나를 얻는다. 풀이 비어있으면 새로 만든다.

    public GameObject GetBullet()

    {

        GameObject b;

        if (pool.Count > 0) b = pool.Dequeue();

        else b = Instantiate(bulletPrefab, transform);

        b.SetActive(true);

        return b;

    }



    // 사용이 끝난 오브젝트는 풀로 반환

    public void ReturnBullet(GameObject b)

    {

        b.SetActive(false);

        pool.Enqueue(b);

    }

}

