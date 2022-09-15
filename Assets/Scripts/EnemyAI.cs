using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Pathfinding;

// simple enemy AI, controlls moving (AstarPath package used), shooting

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private LayerMask _bulletCheckerMask;
    [SerializeField]
    private int _ricochetRayCheckerCount = 90;
    [SerializeField]
    private LayerMask _recognizableLayers;
    [SerializeField]
    private float _cooldown = 0.7f, _rayWidth = 0.1f;
    private Shooting _shooting;
    private float _currentCooldown, _dodgeCooldown = 0f;
    private AIPath _aiPath;
    
    private void Start()
    {
        _shooting = GetComponent<Shooting>();
        GameController.instance.GetComponent<AstarPath>().Scan();
        _currentCooldown = _cooldown;
        _aiPath = GetComponent<AIPath>();
        GetComponent<Seeker>().StartPath(transform.position, 
            GetComponent<AIDestinationSetter>().target.transform.position);
    }

    private void FixedUpdate()
    {
        if (_dodgeCooldown <= 0f)
        {
            // bullet dodge check
            BulletEvadingCheck();
        }
        else
        {
            _dodgeCooldown -= Time.fixedDeltaTime;
        }
        
        // shooting cooldown
        if (_currentCooldown > 0f)
        {
            _currentCooldown -= Time.fixedDeltaTime;
            return;
        }
        
        // check if can shoot directly and shoot if can
        CheckDirectShot();

        // check random tick - if 2% chance dropped - then make ricochet check
        if (Resources.instance.Rng.Next(0, 100) > 97)
            CheckRicochetShot();
    }

    private void BulletEvadingCheck()
    {
        RaycastHit2D hit;
        Projectile bullet;
        for (int i = 0; i < _shooting.Bullets.childCount; i++)
        {
            bullet = _shooting.Bullets.GetChild(i).GetComponent<Projectile>();
            // check if bullet hits the enemy
            hit = Physics2D.Raycast(bullet.transform.position, bullet.Direction, 1000000f, _bulletCheckerMask);
            // if bullet hits enemy - time to dodge!
            if (hit.collider != null)
            {
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    _dodgeCooldown = 1.8f;
                    StartCoroutine(EvadeBulletCoroutine(Mathf.Abs(bullet.Direction.x) > Mathf.Abs(bullet.Direction.y)));
                    return;
                }
            }
        }
    }

    private void CheckRicochetShot()
    {
        float deltaDegree = 360f / _ricochetRayCheckerCount;
        RaycastHit2D hit;
        Vector3 initialRotation = transform.eulerAngles;
        for(int i = 0; i < _ricochetRayCheckerCount; i++)
        {
            transform.eulerAngles = new Vector3(0, 0, i * deltaDegree - 90f);
            hit = Physics2D.Raycast(_shooting.Gun.position, _shooting.Gun.up, 20f, _recognizableLayers);
            // if ray hits player - shoot
            if (ReflectRay(hit, _shooting.Gun.up))
            {
                 _currentCooldown = 1f; 
                StartCoroutine(RotateAndShootCoroutine(transform.eulerAngles, initialRotation));
                transform.eulerAngles = initialRotation;
                return;
            }
        }
        transform.eulerAngles = initialRotation;
    }

    private IEnumerator EvadeBulletCoroutine(bool evadingOnX)
    {
        _aiPath.enabled = false;
        float time = 0, step = 1f;
        Vector2 start = transform.position, 
            endRight = new Vector2(start.x + step, start.y), 
            endLeft = new Vector2(start.x - step, start.y), end;
        if (evadingOnX)
        {
            endRight = new Vector2(start.x, start.y + step);
            endLeft = new Vector2(start.x, start.y - step);
        }
        bool canSpawnOnRight = !Physics2D.CircleCast(endRight, 1f, transform.forward, 1f, LayerMask.NameToLayer("Obstacle"));
        bool canSpawnOnLeft = !Physics2D.CircleCast(endLeft, 1f, transform.forward, 1f, LayerMask.NameToLayer("Obstacle"));

        if(canSpawnOnLeft && canSpawnOnRight)
        {
            end = Resources.instance.Rng.Next(0, 2) > 0 ? endRight : endLeft;
        }
        else if (canSpawnOnLeft)
        {
            end = endLeft;
        }
        else if (canSpawnOnRight)
        {
            end = endRight;
        }
        else
        {
            yield break;
        }

        while (Mathf.Abs(transform.position.x - end.x) + Mathf.Abs(transform.position.y - end.y) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, end, 0.01f);
            time += Time.deltaTime;
            yield return null;
        }

        yield return null;
        _aiPath.enabled = true;
    }

    private IEnumerator RotateAndShootCoroutine(Vector3 shootDirection, Vector3 initialRotation)
    {
        _aiPath.enabled = false;
        float time = 0;
        Quaternion start = new Quaternion(), end = new Quaternion();
        start.eulerAngles = initialRotation;
        end.eulerAngles = shootDirection;
        yield return null;

        while (Mathf.Abs(transform.eulerAngles.z - shootDirection.z) > 0.1f)
        {
            transform.rotation = Quaternion.Lerp(start, end, time * 4f);
            time += Time.deltaTime;
            yield return null;
        }

        Shoot();
        yield return null;
        _aiPath.enabled = true;
    }

    private bool ReflectRay(RaycastHit2D hit, Vector2 direction, int recursionStep = 0)
    {
        if(hit.collider == null || recursionStep > 20)
        {
            return false;
        }

        // hit the obstacle - reflect ray
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            direction = Vector2.Reflect(direction, hit.normal);
            hit = Physics2D.Raycast(hit.point, direction, 20f, _recognizableLayers);
            return ReflectRay(hit, direction, recursionStep + 1);
        }
        // hit the player - ray is good
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            return true;
        }
        // hit self - no point to shoot in that direction
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            return false;
        }
        // hit bullet border - ray is useless
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("BulletDestroyer"))
        {
            return false;
        }
        return false;
    }

    private void CheckDirectShot()
    {
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

    private void Shoot()
    {
        _shooting.Shoot();
        _currentCooldown = _cooldown;
    }

    // if ray collides player - shoot
    private void ShootIfOnTarget(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            //hits the player - time to shoot
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Shoot();
            }
        }
    }
}
