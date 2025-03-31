using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize;
    [SerializeField] private Transform parent;

    private Queue<GameObject> _pool;
    private static ObjectPool _instance;
    
    public static ObjectPool Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        _pool = new Queue<GameObject>();
    }

    /// <summary>
    /// 오브젝트 풀에 새로운 오브젝트를 생성하는 함수
    /// </summary>
    private void CreateNewObject()
    {
        GameObject newObject = Instantiate(prefab, parent);
        newObject.SetActive(false);
        _pool.Enqueue(newObject);
    }
    /// <summary>
    /// 오브젝트 풀에 있는 오브젝트 얻는 메서드
    /// </summary>
    /// <returns></returns>
    public GameObject GetObject()
    {
        if (_pool.Count == 0)
        {
            CreateNewObject();
        }

        GameObject dequeueObject = _pool.Dequeue();
        dequeueObject.SetActive(true);
        return dequeueObject;
    }
    /// <summary>
    /// 사용한 오브젝트를 오브젝트 풀에 되돌리는 메서드
    /// </summary>
    /// <param name="returnObject">반환할 오브젝트</param>
    public void ReturnObject(GameObject returnObject)
    {
        returnObject.SetActive(false);
        _pool.Enqueue(returnObject);
    }
}
