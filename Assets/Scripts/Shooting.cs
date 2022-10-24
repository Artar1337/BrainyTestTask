using UnityEngine;

/// <summary>
/// Контролирует спавн пуль
/// </summary>
public class Shooting : MonoBehaviour
{
    private const string SHOOTAXIS = "Fire1";

    [SerializeField]
    private GameObject bullet;
    [SerializeField]
    private bool isPlayer = true;
    [SerializeField]
    private Transform gun;
    [SerializeField]
    private Transform bullets;

    public Transform Gun { get => gun; }
    public Transform Bullets { get => bullets; }

    /// <summary>
    /// Проверка input для спавна пуль игроком
    /// </summary>
    void Update()
    {
        if (isPlayer && Input.GetButtonDown(SHOOTAXIS))
        {
            CombinationReader.instance.CheckIfActionWasCommited(EActions.Shot);
            Shoot(true);
        }
    }

    /// <summary>
    /// Спавнит пулю
    /// </summary>
    /// <param name="isPlayerOwner">Игрок - владелец пули?</param>
    public void Shoot(bool isPlayerOwner = false)
    {
        Projectile projectile = Instantiate(bullet, gun.position, gun.rotation, bullets).GetComponent<Projectile>();
        projectile.Direction = gun.up;
        projectile.IsPlayerOwner = isPlayerOwner;
    }
}
