using UnityEngine;

/// <summary>
/// Контролирует коллизию и начальное направление пули
/// </summary>
public class Projectile : MonoBehaviour
{
    private const string ENEMYLAYER = "Enemy";
    private const string OBSTACLELAYER = "Obstacle";
    private const string PLAYERLAYER = "Player";
    private const string DESTROYERLAYER = "BulletDestroyer";

    [SerializeField]
    private float BulletSpeed = 500f;

    private Vector2 direction;
    private new Rigidbody2D rigidbody;
    private bool isPlayerOwner = false;

    public Vector2 Direction { set => direction = value.normalized; get => direction; }
    public bool IsPlayerOwner { set => isPlayerOwner = value; }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.AddForce(direction * BulletSpeed, ForceMode2D.Force);
    }

    /// <summary>
    /// Пуля вошла в коллизию
    /// </summary>
    /// <param name="collision">Коллизия</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // касание игрока - проигрыш игрока
        if (collision.gameObject.layer == LayerMask.NameToLayer(PLAYERLAYER))
        {
            Destroy(gameObject);
            GameController.instance.RoundOver(false);
        }
        // касание компьютера - проигрыш компьютера
        else if (collision.gameObject.layer == LayerMask.NameToLayer(ENEMYLAYER))
        {
            Destroy(gameObject);
            GameController.instance.RoundOver(true);
        }
        // касание стены - рикошет
        else if (collision.gameObject.layer == LayerMask.NameToLayer(OBSTACLELAYER))
        {
            direction = Vector2.Reflect(direction, collision.GetContact(0).normal);
            if (rigidbody == null)
            {
                return;
            }
            rigidbody.velocity = Vector2.zero;
            rigidbody.angularVelocity = 0;
            rigidbody.AddForce(direction * BulletSpeed, ForceMode2D.Force);
            if (isPlayerOwner)
            {
                CombinationReader.instance.CheckIfActionWasCommited(EActions.ProjectileBounce);
            } 
        }
        // касание границы - уничтожение пули
        else if(collision.gameObject.layer == LayerMask.NameToLayer(DESTROYERLAYER))
        {
            Destroy(gameObject);
        }
    }
}
