using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float _bulletSpeed = 5f;
    private Vector2 _direction;
    private Rigidbody2D _rigidbody;

    public Vector2 Direction { set => _direction = value.normalized; }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.AddForce(_direction * _bulletSpeed, ForceMode2D.Force);
    }

    private void Update()
    {
        //transform.Translate(_direction * Time.deltaTime * _bulletSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("BULLET HIT " + LayerMask.LayerToName(collision.gameObject.layer));
        // hit the player - destroy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // player lose
            Destroy(gameObject);
        }
        // hit the enemy - destroy
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // enemy lose
            Destroy(gameObject);
        }
        // hit the obstacle - ricoshet
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Debug.Log(_direction + " " + collision.GetContact(0).normal);
            _direction = Vector2.Reflect(_direction, collision.GetContact(0).normal);
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0;
            _rigidbody.AddForce(_direction * _bulletSpeed, ForceMode2D.Force);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("BulletDestroyer"))
        {
            // bullet disappears
            Destroy(gameObject);
        }
    }
}
