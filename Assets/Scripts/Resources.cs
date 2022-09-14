using UnityEngine;

// resources, can be used for sounds, strings and other objects storage 

public class Resources : MonoBehaviour
{
    #region Singleton
    public static Resources instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Resources instance error!");
            return;
        }
        instance = this;
    }
    #endregion

    private System.Random _rng;

    public System.Random Rng { get => _rng; }

    public float GetRandomFloat(float min, float max)
    {
        float range = max - min;
        float sample = (float)Rng.NextDouble();
        return (sample * range) + min;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rng = new System.Random();

        int bulletLayer = LayerMask.NameToLayer("Bullet");
        Physics2D.IgnoreLayerCollision(bulletLayer, bulletLayer);
    }
}
