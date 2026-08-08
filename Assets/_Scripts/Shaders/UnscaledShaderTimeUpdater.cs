using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class UnscaledShaderTimeUpdater : MonoBehaviour
{
    private static readonly int UnscaledTimeId = Shader.PropertyToID("_UnscaledTime");
    private static UnscaledShaderTimeUpdater _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (_instance != null) return;

        var existing = FindObjectOfType<UnscaledShaderTimeUpdater>();
        if (existing != null)
        {
            _instance = existing;
            return;
        }

        var go = new GameObject(nameof(UnscaledShaderTimeUpdater));
        _instance = go.AddComponent<UnscaledShaderTimeUpdater>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        Shader.SetGlobalFloat(UnscaledTimeId, Time.unscaledTime);
    }
}
