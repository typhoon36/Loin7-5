using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCache : MonoBehaviour
{
    // 내부 캐시 사전

    Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();



    // Get: 키가 있으면 캐시에서 반환, 없으면 loader로 로드하여 캐시에 저장 후 반환

    // loader는 Resources.Load 또는 Addressables 로더 콜백을 전달

    public GameObject Get(string key, Func<GameObject> loader)
    {

        if (cache.TryGetValue(key, out var go)) return go;

        go = loader();

        cache[key] = go;

        return go;

    }



    // TryGet: 캐시에 존재하는지 확인

    public bool TryGet(string key, out GameObject go) => cache.TryGetValue(key, out go);

}

