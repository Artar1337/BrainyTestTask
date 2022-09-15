using System.Collections.Generic;
using System.Text;
using UnityEngine;

// parsing and outputting all combo-actions

// possible actions
public enum EActions
{
    None,
    Shot,
    SideWalk,
    Walk,
    ProjectileBounce,
    Rotation
}

[RequireComponent(typeof(Movement))]
public class CombinationReader : MonoBehaviour
{

    #region Singleton
    public static CombinationReader instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Combo reader instance error!");
            return;
        }
        instance = this;
    }
    #endregion

    // some actions to read (MUST be at least two actions!)
    private List<List<EActions>> _combinations = new List<List<EActions>>() { 
        new List<EActions>(){ EActions.Walk, EActions.Shot, EActions.ProjectileBounce },
        new List<EActions>(){ EActions.Rotation, EActions.Shot },
        new List<EActions>(){ EActions.Walk, EActions.Rotation, EActions.Shot },
        new List<EActions>(){ EActions.Walk, EActions.Rotation, EActions.Shot, EActions.ProjectileBounce },
        new List<EActions>(){ EActions.Shot, EActions.Shot },
        new List<EActions>(){ EActions.SideWalk, EActions.Shot }
    };
    // key - list index, value - actionindex
    private Dictionary<int, int> _valid = new Dictionary<int, int>();
    // visual representation
    private TMPro.TMP_Text _log;
    private Movement _movement;
    [SerializeField]
    private float _walkCooldown = 3f, _rotationCooldown = 1.8f;
    private float _currentWalkCooldown = -1f, _currentSideCooldown = -1f, _currentRotationCooldown = -1f;

    // Start is called before the first frame update
    void Start()
    {
        _log = GameObject.Find("Main Canvas").transform.Find("Log").GetComponent<TMPro.TMP_Text>();
        _movement = GetComponent<Movement>();
    }

    private void FixedUpdate()
    {
        if (_currentWalkCooldown > 0f)
            _currentWalkCooldown -= Time.fixedDeltaTime;
        if (_currentSideCooldown > 0f)
            _currentSideCooldown -= Time.fixedDeltaTime;
        if (_currentRotationCooldown > 0f)
            _currentRotationCooldown -= Time.fixedDeltaTime;
    }

    private void CommitAction(EActions action)
    {
        Debug.Log("Action: " + action.ToString());
        // got some valid actions - checking them first
        if(_valid.Count > 0)
        {
            var newValid = new Dictionary<int,int>();
            foreach(var pair in _valid)
            {
                // if action is next action in _combinations - reassign value
                if(_combinations[pair.Key][pair.Value + 1] == action)
                {
                    newValid.Add(pair.Key, pair.Value + 1);
                }
                // else - combo does not match, ignore
            }

            _valid.Clear();
            foreach (var pair in newValid)
            {
                _valid.Add(pair.Key, pair.Value);
            }

            List<int> clearList = new List<int>();
            foreach (var pair in _valid)
            {
                // got to a last index - combo detected!
                if (_valid[pair.Key] == _combinations[pair.Key].Count - 1)
                {
                    // outputting a string
                    OutputCombo(_combinations[pair.Key]);
                    // clearing a dictionary
                    clearList.Add(pair.Key);
                }
            }

            foreach(var element in clearList)
            {
                _valid.Remove(element);
            }
        }

        // searching in every combination and initialize _valid
        int index = 0;
        foreach (var list in _combinations)
        {
            // found first element, init _valid
            if(list[0] == action && !_valid.ContainsKey(index))
            {
                _valid.Add(index, 0);
            }
            index++;
        }
    }

    // translate list of actions to string
    private string GetCombo(List<EActions> combos)
    {
        StringBuilder builder = new StringBuilder();
        foreach(var combo in combos)
        {
            builder.Append(combo.ToString());
            builder.Append(", ");
        }
        builder.Remove(builder.Length - 2, 2);
        return builder.ToString();
    }

    private void OutputCombo(List<EActions> combos)
    {
        _log.text = _log.text + GetCombo(combos) + "\n ";
        Invoke(nameof(RemoveOneLineFromLog), 3f);
    }

    public void RemoveOneLineFromLog()
    {
        int index = 0;
        bool found = false;
        // searching for first '\n'
        foreach(char c in _log.text)
        {
            if (c == '\n')
            {
                found = true;
                break;
            }
            index++;
        }
        // reassigning the text to substring after first '\n'
        if (found)
        {
            _log.text = _log.text.Substring(index + 1);
        }
    }

    public void CheckIfActionWasCommited(EActions action)
    {
        switch (action)
        {
            case EActions.Walk:  
                // check if player's velocity is high enough (W, S, Yaxis)
                if (!_movement.CheckYVelocity())
                {
                    _currentWalkCooldown = -0.1f;
                    return;
                }  
                if (_currentWalkCooldown > 0f)
                    return;
                _currentWalkCooldown = _walkCooldown;
                break;
            case EActions.SideWalk:
                // check if player's velocity is high enough (A, D, Xaxis)
                if (!_movement.CheckXVelocity())
                {
                    _currentSideCooldown = -0.1f;
                    return;
                }
                if (_currentSideCooldown > 0f)
                    return;
                _currentSideCooldown = _walkCooldown;
                break;
            case EActions.Rotation:
                // check player's angle rotation change - if high enough, then commit
                if (!_movement.CheckRotation())
                {
                    _currentRotationCooldown = -0.1f;
                    return;
                }
                if (_currentRotationCooldown > 0f)
                    return;
                _currentRotationCooldown = _rotationCooldown;
                break;
            case EActions.Shot:
                // does not need to be checked - just summon when shoot button is pressed
                break;
            case EActions.ProjectileBounce:
                // does not need to be checked - just summon when player's projectile is bouncing
                break;
            default:
                // other cases ('None' included) - ignore
                return;
        }
        CommitAction(action);
    }
}
