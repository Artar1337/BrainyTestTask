using UnityEngine;

// script controlls spawning bullets for player and for enemy

public class Shooting : MonoBehaviour
{
    [SerializeField]
    private GameObject _bullet;
    [SerializeField]
    private bool _isPlayer = true;

    private Transform _gun, _bullets;

    public Transform Gun { get => _gun; }

    // Start is called before the first frame update
    void Start()
    {
        _gun = transform.Find("Gun");
        _bullets = GameObject.Find("Bullets").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlayer && Input.GetButtonDown("Fire1"))
        {
            CombinationReader.instance.CheckIfActionWasCommited(EActions.Shot);
            Shoot(true);
        }
    }

    public void Shoot(bool isPlayerOwner = false)
    {
        Projectile projectile = Instantiate(_bullet, _gun.position, _gun.rotation, _bullets).GetComponent<Projectile>();
        projectile.Direction = _gun.up;
        projectile.IsPlayerOwner = isPlayerOwner;
    }
}
