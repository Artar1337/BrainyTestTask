using UnityEngine;

/// <summary>
/// Спавнит препятствия при старте игры случайным образом
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField]
    private int _obstacleCount = 15;
    [SerializeField]
    private GameObject[] _obstacles;
    [SerializeField]
    private float _XBorder = 8.888f, _YBorder = 5f;

    /// <summary>
    /// Спавнит препятствия
    /// </summary>
    void Start()
    {
        float x, y;
        int index;
        Quaternion rotation = new Quaternion();
        for(int i = 0; i < _obstacleCount; i++)
        {
            x = Random.Range(-_XBorder, _XBorder);
            y = Random.Range(-_YBorder, _YBorder);
            rotation.eulerAngles = new Vector3(0, 0, Random.Range(-90f, 90f));
            index = Random.Range(0, _obstacles.Length);

            if (!Physics2D.BoxCast(new Vector2(x, y),
                _obstacles[index].transform.localScale,
                rotation.eulerAngles.z, transform.forward))
            {
                Instantiate(_obstacles[index],
                    new Vector3(x, y), rotation);
            }
            else
            {
                i--;
            }    
        }
    }
}
