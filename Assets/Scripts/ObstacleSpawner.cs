using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField]
    private int _obstacleCount = 15;
    [SerializeField]
    private GameObject[] _obstacles;
    [SerializeField]
    private float _XBorder = 8.888f, _YBorder = 5f;

    // Start is called before the first frame update
    void Start()
    {
        float x, y;
        int index;
        Quaternion rotation = new Quaternion();
        for(int i = 0; i < _obstacleCount; i++)
        {
            x = Resources.instance.GetRandomFloat(-_XBorder, _XBorder);
            y = Resources.instance.GetRandomFloat(-_YBorder, _YBorder);
            rotation.eulerAngles = new Vector3(0, 0, Resources.instance.GetRandomFloat(-90f, 90f));
            index = Resources.instance.Rng.Next(0, _obstacles.Length);

            if (!Physics2D.BoxCast(new Vector2(x, y),
                _obstacles[index].transform.localScale,
                rotation.eulerAngles.z, transform.forward))
            {
                Instantiate(_obstacles[index],
                    new Vector3(x, y), rotation);
            }
            else
                i--;
        }
    }

}
