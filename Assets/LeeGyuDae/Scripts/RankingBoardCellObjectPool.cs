using System.Collections.Generic;
using UnityEngine;

namespace LeeGyuDae
{
    public class RankingBoardCellObjectPool : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private GameObject rankingBoardCellPrefab;

        [SerializeField] private Transform contentTransform;

        private Queue<GameObject> _pool;
        public static RankingBoardCellObjectPool Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _pool = new Queue<GameObject>();
        }

        /// <summary>
        /// Object Pool에 새로운 오브젝트 생성 메서드
        /// </summary>
        private void CreateNewObject()
        {
            GameObject newObject = Instantiate(rankingBoardCellPrefab, contentTransform);
            newObject.SetActive(false);
            _pool.Enqueue(newObject);
        }

        /// <summary>
        /// 오브젝트 풀에 있는 오브젝트 반환 메서드
        /// </summary>
        /// <returns>오브젝트 풀에 있는 오브젝트</returns>
        public GameObject GetObject()
        {
            if (_pool.Count == 0) CreateNewObject();

            GameObject dequeObject = _pool.Dequeue();
            dequeObject.SetActive(true);
            return dequeObject;
        }

        /// <summary>
        /// 사용한 오브젝트를 오브젝트 풀로 되돌려 주는 메서드
        /// </summary>
        /// <param name="returnObject">반환할 오브젝트</param>
        public void ReturnObject(GameObject returnObject)
        {
            returnObject.SetActive(false);
            _pool.Enqueue(returnObject);
        }
    }
}