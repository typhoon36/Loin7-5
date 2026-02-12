using System.Collections.Generic;

using UnityEngine;



// EnemySorter: 플레이어와의 거리를 기준으로 적들을 정렬하고 이진탐색 예제를 제공

public class EnemySorter : MonoBehaviour

{

    // 플레이어 Transform 할당

    public Transform player;

    // 적들 목록 (Editor에서 드래그하여 채움)

    public List<Transform> enemies = new List<Transform>();



    // 거리 기준 정렬 (sqrMagnitude 사용으로 루트 연산 생략)

    public void SortByDistance()

    {

        enemies.Sort((a, b) =>

        {

            float da = (a.position - player.position).sqrMagnitude;

            float db = (b.position - player.position).sqrMagnitude;

            return da.CompareTo(db);

        });

    }



    // 이진 탐색: 정렬된 리스트에서 대상의 인덱스를 찾음

    // 주의: 동일 거리 값이 여러 개일 수 있으므로 정확한 매칭 로직은 상황에 맞게 조정 필요

    public int BinarySearchByDistance(Transform target)

    {

        // enemies가 이미 거리 기준으로 정렬되어 있어야 함

        int low = 0, high = enemies.Count - 1;

        float targetDist = (target.position - player.position).sqrMagnitude;

        while (low <= high)

        {

            int mid = (low + high) / 2;

            float midDist = (enemies[mid].position - player.position).sqrMagnitude;

            if (Mathf.Approximately(midDist, targetDist)) return mid;

            if (midDist < targetDist) low = mid + 1; else high = mid - 1;

        }

        return -1;

    }

}

