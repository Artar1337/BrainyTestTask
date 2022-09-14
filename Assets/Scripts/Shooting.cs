using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField]
    private GameObject _bullet;
    [SerializeField]
    private bool _isPlayer = true;

    private Transform _gun;

    // Start is called before the first frame update
    void Start()
    {
        _gun = transform.Find("Gun");
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlayer && Input.GetButtonDown("Fire1"))
        {
            GameObject projectile = Instantiate(_bullet, _gun.position, _gun.rotation);
            projectile.GetComponent<Projectile>().Direction = _gun.up;
        }
    }
}
