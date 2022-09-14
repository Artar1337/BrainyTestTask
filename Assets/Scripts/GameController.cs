using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // key - player's score, value - computer's score
    private int _playerScore = 0, _computerScore = 0;
    private Transform _bullets, _player, _enemy;
    private GameObject _centerBox, _constField;
    private Vector3 _defaultPlayerPos, _defaultEnemyPos;
    private TMPro.TMP_Text _score;

    public string Score { get => "<color=green>" + _playerScore + "</color><b>:</b><color=red>" + _computerScore + "</color>"; }

    // Start is called before the first frame update
    void Start()
    {
        _bullets = GameObject.Find("Bullets").transform;
        _player = GameObject.Find("Player").transform;
        _player.gameObject.SetActive(false);
        _enemy = GameObject.Find("Enemy").transform;
        _enemy.gameObject.SetActive(false);
        _centerBox = GameObject.Find("CenterBox");
        _centerBox.gameObject.SetActive(false);
        _constField = GameObject.Find("Constant Battlefield");
        _constField.gameObject.SetActive(false);
        _score = GameObject.Find("Main Canvas").transform.Find("Score").GetComponent<TMPro.TMP_Text>();
        _defaultPlayerPos = _player.position;
        _defaultEnemyPos = _enemy.position;
    }

    public void StartGame(bool withRandomField)
    {
        _centerBox.SetActive(true);
        if (withRandomField)
        {
            GetComponent<ObstacleSpawner>().enabled = true;
        }
        else
        {
            _constField.SetActive(true);
        }
        _player.gameObject.SetActive(true);
        _enemy.gameObject.SetActive(true);
        Transform tmp = GameObject.Find("Main Canvas").transform;
        tmp.Find("StartRandom").gameObject.SetActive(false);
        tmp.Find("StartFixed").gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void RoundOver(bool playerWon)
    {
        // set the score
        if (playerWon)
            _playerScore++;
        else
            _computerScore++;
        _score.text = Score;
        // delete all bullets
        foreach(Transform child in _bullets)
        {
            Destroy(child.gameObject);
        }
        //return to base positions
        _player.position = _defaultPlayerPos;
        _enemy.position = _defaultEnemyPos;
        Rigidbody2D rigidbody = _player.GetComponent<Rigidbody2D>();
        rigidbody.velocity = Vector2.zero;
        rigidbody.angularVelocity = 0;
    }

}
