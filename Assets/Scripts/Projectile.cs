using UnityEngine;

// bullet scripts, controlls actions oncollisionenter

public class Projectile : MonoBehaviour
{
    public float _bulletSpeed = 5f;
    private Vector2 _direction;
    private Rigidbody2D _rigidbody;
    private bool _isPlayerOwner = false;

    public Vector2 Direction { set => _direction = value.normalized; get => _direction; }
    public bool IsPlayerOwner { set => _isPlayerOwner = value; }

    // add starting force 
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.AddForce(_direction * _bulletSpeed, ForceMode2D.Force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // hit the player - destroy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // player lose
            Destroy(gameObject);
            GameController.instance.RoundOver(false);
        }
        // hit the enemy - destroy
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // enemy lose
            Destroy(gameObject);
            GameController.instance.RoundOver(true);
        }
        // hit the obstacle - ricochet
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            _direction = Vector2.Reflect(_direction, collision.GetContact(0).normal);
            if (_rigidbody == null)
                return;
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0;
            _rigidbody.AddForce(_direction * _bulletSpeed, ForceMode2D.Force);
            if(_isPlayerOwner)
                CombinationReader.instance.CheckIfActionWasCommited(EActions.ProjectileBounce);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("BulletDestroyer"))
        {
            // bullet disappears
            Destroy(gameObject);
        }
    }
}
