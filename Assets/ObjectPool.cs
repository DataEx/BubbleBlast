using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {


    private Stack<GameObject> pool;
    public GameObject objPrefab;
    public int initialCreateCount = 10;

    private void Awake()
    {
        pool = new Stack<GameObject>(initialCreateCount);
        if (objPrefab == null)
            return;
        for (int i = 0; i < initialCreateCount; i++)
        {
            AddToPool(Instantiate(objPrefab));
        }
    }

    public void AddToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.parent = this.transform;
        pool.Push(obj);
    }

    public GameObject GetObject()
    {
        if(pool.Count > 0)
        {
            GameObject obj = pool.Pop();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return null;
        }
    }

}
