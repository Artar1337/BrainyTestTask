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
    private Vector3 _defaultPlayerPos, _defaultEnemyPos;
    private Text _score;

    public string Score { get => _playerScore + " : " + _computerScore; }

    // Start is called before the first frame update
    void Start()
    {
        _bullets = GameObject.Find("Bullets").transform;
        _player = GameObject.Find("Player").transform;
        _enemy = GameObject.Find("Enemy").transform;
        _score = GameObject.Find("Main Canvas").transform.Find("Score").GetComponent<Text>();
        _defaultPlayerPos = _player.position;
        _defaultEnemyPos = _enemy.position;
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
