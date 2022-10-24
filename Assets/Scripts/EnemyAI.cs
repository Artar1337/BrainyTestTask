using System.Collections;
using UnityEngine;
using Pathfinding;

/// <summary>
/// Контролирует ИИ врага, передвижение (пакет AstarPath), стрельба
/// </summary>
public class EnemyAI : MonoBehaviour
{
    private const float RAYWIDTH = 0.1f;
    private const float RICOCHETCOOLDOWN = 1f;
    private const float DODGECOOLDOWN = 1.8f;
    private const float DEG360 = 360f;
    private const float DEG90 = 90f;
    private const float DODGESPEED = 4f;
    private const float DODGEABS = 0.05f;
    /// <summary>
    /// Шанс проверки возможности рикошета (от 0 до MAXCHANCE)
    /// </summary>
    private const int RICOCHETCHANCE = 2;
    private const int MAXCHANCE = 100;
    /// <summary>
    /// Максимально возможное кол-во отскоков для подсчета рикошета
    /// </summary>
    private const int MAXRECURSIONSTEP = 20;
    private const string ENEMYLAYER = "Enemy";
    private const string OBSTACLELAYER = "Obstacle";
    private const string PLAYERLAYER = "Player";

    [SerializeField]
    private LayerMask bulletCheckerMask;
    /// <summary>
    /// Максимальное количество пускаемых лучей для подсчета возможности рикошета
    /// </summary>
    [SerializeField]
    private int ricochetRayCheckerCount = 90;
    [SerializeField]
    private LayerMask recognizableLayers;
    [SerializeField]
    private float cooldown = 0.7f;

    private Shooting shooting;
    private float shootCooldown = 0f;
    private float dodgeCooldown = 0f;
    private AIPath aiPath;
    
    private void Start()
    {
        shooting = GetComponent<Shooting>();
        GameController.instance.GetComponent<AstarPath>().Scan();
        shootCooldown = cooldown;
        aiPath = GetComponent<AIPath>();
        GetComponent<Seeker>().StartPath(transform.position, 
            GetComponent<AIDestinationSetter>().target.transform.position);
    }

    /// <summary>
    /// Обработка стрельбы, уворота от пуль
    /// </summary>
    private void FixedUpdate()
    {
        if (dodgeCooldown <= 0f)
        {
            BulletEvadingCheck();
        }
        else
        {
            dodgeCooldown -= Time.fixedDeltaTime;
        }
        
        if (shootCooldown > 0f)
        {
            shootCooldown -= Time.fixedDeltaTime;
            return;
        }
        
        // стреляем прямым выстрелом, если можно
        CheckDirectShot();

        //если выпал шанс - начинаем стрелять рикошетом
        if (Random.Range(0, MAXCHANCE) < RICOCHETCHANCE)
        {
            CheckRicochetShot();
        }
    }

    /// <summary>
    /// Уклонение от пуль
    /// </summary>
    private void BulletEvadingCheck()
    {
        RaycastHit2D hit;
        Projectile bullet;
        // для каждого снаряда проверяем - если пуля касается врага (прямое попадание), то доджим её
        for (int i = 0; i < shooting.Bullets.childCount; i++)
        {
            bullet = shooting.Bullets.GetChild(i).GetComponent<Projectile>();
            hit = Physics2D.Raycast(bullet.transform.position, bullet.Direction, float.MaxValue, bulletCheckerMask);
            if (hit.collider != null)
            {
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer(ENEMYLAYER))
                {
                    dodgeCooldown = DODGECOOLDOWN;
                    StartCoroutine(EvadeBulletCoroutine(Mathf.Abs(bullet.Direction.x) > Mathf.Abs(bullet.Direction.y)));
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Проверка возможности рикошета для ricochetRayCheckerCount лучей, пущенных по дуге окружности DEG360
    /// </summary>
    private void CheckRicochetShot()
    {
        float deltaDegree = DEG360 / ricochetRayCheckerCount;
        RaycastHit2D hit;
        Vector3 initialRotation = transform.eulerAngles;
        for(int i = 0; i < ricochetRayCheckerCount; i++)
        {
            transform.eulerAngles = new Vector3(0, 0, i * deltaDegree - DEG90);
            hit = Physics2D.Raycast(shooting.Gun.position, shooting.Gun.up, float.MaxValue, recognizableLayers);
            if (ReflectRay(hit, shooting.Gun.up))
            {
                shootCooldown = RICOCHETCOOLDOWN; 
                StartCoroutine(RotateAndShootCoroutine(transform.eulerAngles, initialRotation));
                transform.eulerAngles = initialRotation;
                return;
            }
        }
        transform.eulerAngles = initialRotation;
    }

    /// <summary>
    /// Корутина для уклонения от пули
    /// </summary>
    /// <param name="evadingOnX">Уклонение происходит по оси X?</param>
    private IEnumerator EvadeBulletCoroutine(bool evadingOnX)
    {
        aiPath.enabled = false;
        float step = 1f;
        Vector2 start = transform.position, 
            endRight = new Vector2(start.x + step, start.y), 
            endLeft = new Vector2(start.x - step, start.y), end;
        if (evadingOnX)
        {
            endRight = new Vector2(start.x, start.y + step);
            endLeft = new Vector2(start.x, start.y - step);
        }
        bool canSpawnOnRight = !Physics2D.CircleCast(endRight, step, transform.forward, step, LayerMask.NameToLayer(OBSTACLELAYER));
        bool canSpawnOnLeft = !Physics2D.CircleCast(endLeft, step, transform.forward, step, LayerMask.NameToLayer(OBSTACLELAYER));

        if(canSpawnOnLeft && canSpawnOnRight)
        {
            end = Random.Range(0, 2) > 0 ? endRight : endLeft;
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

        while (Mathf.Abs(transform.position.x - end.x) + Mathf.Abs(transform.position.y - end.y) > DODGEABS)
        {
            transform.position = Vector2.MoveTowards(transform.position, end, Time.deltaTime * DODGESPEED);
            yield return null;
        }

        yield return null;
        aiPath.enabled = true;
    }

    /// <summary>
    /// Корутина для поворота и последующего выстрела
    /// </summary>
    /// <param name="shootDirection">Направление, куда надо развернуться</param>
    /// <param name="initialRotation">Изначальный поворот</param>
    private IEnumerator RotateAndShootCoroutine(Vector3 shootDirection, Vector3 initialRotation)
    {
        aiPath.enabled = false;
        float time = 0;
        Quaternion start = new Quaternion(), end = new Quaternion();
        start.eulerAngles = initialRotation;
        end.eulerAngles = shootDirection;
        yield return null;

        while (Mathf.Abs(transform.eulerAngles.z - shootDirection.z) > DODGEABS * 2)
        {
            transform.rotation = Quaternion.Lerp(start, end, time * DODGESPEED);
            time += Time.deltaTime;
            yield return null;
        }

        Shoot();
        yield return null;
        aiPath.enabled = true;
    }

    /// <summary>
    /// Подсчет результата выстрела после n-ного рикошета
    /// </summary>
    /// <param name="hit">Объект, которого пуля коснулась последним</param>
    /// <param name="direction">Направление полёта пули</param>
    /// <param name="recursionStep">Текущий шаг рекурсии</param>
    /// <returns>true, если пуля попадёт по игроку (иначе false)</returns>
    private bool ReflectRay(RaycastHit2D hit, Vector2 direction, int recursionStep = 0)
    {
        if(hit.collider == null || recursionStep > MAXRECURSIONSTEP)
        {
            return false;
        }
        // выстрел по препятствию - отражение луча
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer(OBSTACLELAYER))
        {
            direction = Vector2.Reflect(direction, hit.normal);
            hit = Physics2D.Raycast(hit.point, direction, 20f, recognizableLayers);
            return ReflectRay(hit, direction, recursionStep + 1);
        }
        // выстрел по игроку - рикошетом возможно попасть
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer(PLAYERLAYER))
        {
            return true;
        }
        // выстрел по самому себе, по границе уничтожения пули и т.д. - такая последовательность рикошетов не валидна
        return false;
    }

    /// <summary>
    /// Проверка возможности попасть в игрока прямым попаданием пули (всего пускается 3 луча в направлении взгляда)
    /// </summary>
    private void CheckDirectShot()
    {
        ShootIfOnTarget(Physics2D.Raycast(shooting.Gun.position, shooting.Gun.up, float.MaxValue, recognizableLayers));
        if (shootCooldown > 0f)
        {
            return;
        }
        ShootIfOnTarget(Physics2D.Raycast(new Vector2(shooting.Gun.position.x + RAYWIDTH, shooting.Gun.position.y),
            shooting.Gun.up, float.MaxValue, recognizableLayers));
        if (shootCooldown > 0f)
        {
            return;
        }
        ShootIfOnTarget(Physics2D.Raycast(new Vector2(shooting.Gun.position.x - RAYWIDTH, shooting.Gun.position.y),
            shooting.Gun.up, float.MaxValue, recognizableLayers));
    }

    /// <summary>
    /// Выстрелить из оружия
    /// </summary>
    private void Shoot()
    {
        shooting.Shoot();
        shootCooldown = cooldown;
    }

    /// <summary>
    /// Вызывает Shoot(), если пуля потенциально может попасть в игрока
    /// </summary>
    /// <param name="hit">Объект, которого коснется пуля</param>
    private void ShootIfOnTarget(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer(PLAYERLAYER))
            {
                Shoot();
            }
        }
    }
}
