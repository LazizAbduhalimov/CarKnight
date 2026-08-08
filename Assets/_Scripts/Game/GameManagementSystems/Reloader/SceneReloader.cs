using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    private float _missInputTime = 0.5f;
    private bool _missInputTimePassed;

    private void OnEnable()
    {
        Tween.Delay(_missInputTime, () => _missInputTimePassed = true, useUnscaledTime: true);
    }

    private void OnDisable()
    {
        _missInputTimePassed = false;    
    }

    public void ReloadCurrentScene()
    {
        if (!_missInputTimePassed) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}