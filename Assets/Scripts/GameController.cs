using UnityEngine;
using TMPro;

/// <summary>
/// Игровой контроллер (старт игры, выигрыш/проигрыш)
/// </summary>
public class GameController : MonoBehaviour
{
    #region Singleton
    public static GameController instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Game controller instance error!");
            return;
        }
        instance = this;
    }
    #endregion

    private const string BULLET = "Bullet";

    [SerializeField]
    private Transform bullets;
    [SerializeField]
    private Transform player;
    [SerializeField]
    private Transform enemy;
    [SerializeField]
    private GameObject centerBox;
    [SerializeField]
    private GameObject constField;
    [SerializeField]
    private TMP_Text score;
    [SerializeField]
    private GameObject startFixed;
    [SerializeField]
    private GameObject startRandom;

    private int playerScore = 0;
    private int computerScore = 0;
    private Vector3 defaultPlayerPos; 
    private Vector3 defaultEnemyPos;
    
    public string Score 
    { 
        get => "<color=green>" + playerScore + "</color><b>:</b><color=red>" + computerScore + "</color>"; 
    }

    void Start()
    {
        int bulletLayer = LayerMask.NameToLayer(BULLET);
        Physics2D.IgnoreLayerCollision(bulletLayer, bulletLayer);
        player.gameObject.SetActive(false);
        enemy.gameObject.SetActive(false);
        centerBox.gameObject.SetActive(false);
        constField.gameObject.SetActive(false);
        defaultPlayerPos = player.position;
        defaultEnemyPos = enemy.position;
    }

    /// <summary>
    /// Старт игры
    /// </summary>
    /// <param name="withRandomField">Сгенерировать рандомное поле?</param>
    public void StartGame(bool withRandomField)
    {
        centerBox.SetActive(true);
        if (withRandomField)
        {
            GetComponent<ObstacleSpawner>().enabled = true;
        }
        else
        {
            constField.SetActive(true);
        }
        player.gameObject.SetActive(true);
        enemy.gameObject.SetActive(true);
        startRandom.SetActive(false);
        startFixed.SetActive(false);
    }

    /// <summary>
    /// Парсинг инпута для выхода на кнопку ESC
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// Завершение раунда
    /// </summary>
    /// <param name="playerWon">Выиграл ли игрок?</param>
    public void RoundOver(bool playerWon)
    {
        if (playerWon)
        {
            playerScore++;
        }
        else
        {
            computerScore++;
        }
            
        score.text = Score;
        foreach(Transform child in bullets)
        {
            Destroy(child.gameObject);
        }

        enemy.GetComponent<EnemyAI>().StopAllCoroutines();
        enemy.GetComponent<Pathfinding.AIPath>().enabled = true;
        player.position = defaultPlayerPos;
        enemy.position = defaultEnemyPos;
        Rigidbody2D rigidbody = player.GetComponent<Rigidbody2D>();
        rigidbody.velocity = Vector2.zero;
        rigidbody.angularVelocity = 0;
    }
}
