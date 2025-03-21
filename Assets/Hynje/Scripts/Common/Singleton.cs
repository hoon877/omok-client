using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static bool _isInitialized = false;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            // 씬 전환시 호출되는 액션 메서드 할당
            if (!_isInitialized)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                _isInitialized = true;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected abstract void OnSceneLoaded(Scene scene, LoadSceneMode mode);
}
