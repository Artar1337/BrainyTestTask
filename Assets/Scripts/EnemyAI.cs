using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private Seeker _seeker;
    private Shooting _shooting;
    [SerializeField]
    private LayerMask _recognizableLayers;
    [SerializeField]
    private float _cooldown = 0.7f, _rayWidth = 0.1f;
    private float _currentCooldown;
    
    private void Start()
    {
        _seeker = GetComponent<Seeker>();
        _shooting = GetComponent<Shooting>();
        GameController.instance.GetComponent<AstarPath>().Scan();
        _currentCooldown = _cooldown;
        _seeker.StartPath(transform.position, 
            GetComponent<AIDestinationSetter>().target.transform.position);
    }

    private void FixedUpdate()
    {
        // shooting cooldown
        if (_currentCooldown > 0f)
        {
            _currentCooldown -= Time.fixedDeltaTime;
            return;
        }

        // pointing a ray into a player
        ShootIfOnTarget(Physics2D.Raycast(_shooting.Gun.position, _shooting.Gun.up, 20f, _recognizableLayers));

        // and 2 more rays to higher chanse of getting a hit
        if (_currentCooldown > 0f)
            return;
        ShootIfOnTarget(Physics2D.Raycast(
            new Vector2(_shooting.Gun.position.x + _rayWidth, _shooting.Gun.position.y),
            _shooting.Gun.up, 20f, _recognizableLayers));
        if (_currentCooldown > 0f)
            return;
        ShootIfOnTarget(Physics2D.Raycast(
            new Vector2(_shooting.Gun.position.x - _rayWidth, _shooting.Gun.position.y),
            _shooting.Gun.up, 20f, _recognizableLayers));
    }

    private void ShootIfOnTarget(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            //hits the player - time to shoot
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                _shooting.Shoot();
                _currentCooldown = _cooldown;
            }
        }
    }
}
