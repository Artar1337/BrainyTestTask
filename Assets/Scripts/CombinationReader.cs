using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Возможные действия в комбо
/// </summary>
public enum EActions
{
    None,
    Shot,
    SideWalk,
    Walk,
    ProjectileBounce,
    Rotation
}

/// <summary>
/// Парсинг и вывод всех комбо
/// </summary>
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

    private const string SEPARATOR = ", ";
    private const string NEWLINESEPARATOR = "\n ";
    private const float LOGREMOVETIME = 3f;

    [SerializeField]
    private float walkCooldown = 3f;
    [SerializeField]
    private float rotationCooldown = 1.8f;
    [SerializeField]
    private TMP_Text log;

    /// <summary>
    /// Комбинации действий, распознаваемые игрой (ДОЛЖНО быть хотя бы 2 действия)
    /// </summary>
    private List<List<EActions>> combinations = new List<List<EActions>>() { 
        new List<EActions>(){ EActions.Walk, EActions.Shot, EActions.ProjectileBounce },
        new List<EActions>(){ EActions.Rotation, EActions.Shot },
        new List<EActions>(){ EActions.Walk, EActions.Rotation, EActions.Shot },
        new List<EActions>(){ EActions.Walk, EActions.Rotation, EActions.Shot, EActions.ProjectileBounce },
        new List<EActions>(){ EActions.Shot, EActions.Shot },
        new List<EActions>(){ EActions.SideWalk, EActions.Shot }
    };
    /// <summary>
    /// Key - индекс комбинации, Value - индекс действия в комбинации
    /// </summary>
    private Dictionary<int, int> valid = new Dictionary<int, int>();
    private Movement movement;
    private float currentWalkCooldown = 0;
    private float currentSideCooldown = 0; 
    private float currentRotationCooldown = 0;

    void Start()
    {
        movement = GetComponent<Movement>();
    }

    /// <summary>
    /// Апдейт таймера для кулдаунов
    /// </summary>
    private void FixedUpdate()
    {
        if (currentWalkCooldown > 0f)
        {
            currentWalkCooldown -= Time.fixedDeltaTime;
        } 
        if (currentSideCooldown > 0f)
        {
            currentSideCooldown -= Time.fixedDeltaTime;
        }
        if (currentRotationCooldown > 0f)
        {
            currentRotationCooldown -= Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Совершает действие (проверка соответствия всем комбо)
    /// </summary>
    /// <param name="action">Действие для совершения</param>
    private void CommitAction(EActions action)
    {
        if(valid.Count > 0)
        {
            var newValid = new Dictionary<int,int>();
            foreach(var pair in valid)
            {
                // если action - следующее действие в комбинациях - переназначаем value
                if(combinations[pair.Key][pair.Value + 1] == action)
                {
                    newValid.Add(pair.Key, pair.Value + 1);
                }
            }

            valid.Clear();
            foreach (var pair in newValid)
            {
                valid.Add(pair.Key, pair.Value);
            }

            List<int> clearList = new List<int>();
            foreach (var pair in valid)
            {
                // индекс максимально возможный => это комбо
                if (valid[pair.Key] == combinations[pair.Key].Count - 1)
                {
                    OutputCombo(combinations[pair.Key]);
                    clearList.Add(pair.Key);
                }
            }

            foreach(var element in clearList)
            {
                valid.Remove(element);
            }
        }

        // реинициализация словаря valid
        int index = 0;
        foreach (var list in combinations)
        {
            if(list[0] == action && !valid.ContainsKey(index))
            {
                valid.Add(index, 0);
            }
            index++;
        }
    }

    /// <summary>
    /// Перевод комбо в строку
    /// </summary>
    /// <param name="combos">Комбо для конвертации в строку</param>
    /// <returns>List<EActions> в формате string</returns>
    private string GetCombo(List<EActions> combos)
    {
        StringBuilder builder = new StringBuilder();
        foreach(var combo in combos)
        {
            builder.Append(combo.ToString());
            builder.Append(SEPARATOR);
        }
        builder.Remove(builder.Length - SEPARATOR.Length, SEPARATOR.Length);
        return builder.ToString();
    }

    /// <summary>
    /// Вывод комбо в лог
    /// </summary>
    /// <param name="combos">Комбо для вывода</param>
    private void OutputCombo(List<EActions> combos)
    {
        log.text = log.text + GetCombo(combos) + NEWLINESEPARATOR;
        Invoke(nameof(RemoveOneLineFromLog), LOGREMOVETIME);
    }

    /// <summary>
    /// Убирает первую строку из лога (удаляет часть строки до NEWLINEEPARATOR)
    /// </summary>
    public void RemoveOneLineFromLog()
    {
        int index = 0;
        foreach(char c in log.text)
        {
            if (c == NEWLINESEPARATOR[0])
            {
                log.text = log.text.Substring(index + 1);
                return;
            }
            index++;
        }
    }

    /// <summary>
    /// Проверка, что действие было совершено
    /// </summary>
    /// <param name="action">Действие для проверки</param>
    public void CheckIfActionWasCommited(EActions action)
    {
        switch (action)
        {
            case EActions.Walk:  
                // Проверка velocity (W, S, Yaxis)
                if (!movement.CheckYVelocity())
                {
                    currentWalkCooldown = 0f;
                    return;
                }  
                if (currentWalkCooldown > 0f)
                {
                    return;
                }
                currentWalkCooldown = walkCooldown;
                break;
            case EActions.SideWalk:
                // Проверка velocity (A, D, Xaxis)
                if (!movement.CheckXVelocity())
                {
                    currentSideCooldown = 0f;
                    return;
                }
                if (currentSideCooldown > 0f)
                {
                    return;
                } 
                currentSideCooldown = walkCooldown;
                break;
            case EActions.Rotation:
                // Проверка delta угла поворота
                if (!movement.CheckRotation())
                {
                    currentRotationCooldown = 0f;
                    return;
                }
                if (currentRotationCooldown > 0f)
                {
                    return;
                } 
                currentRotationCooldown = rotationCooldown;
                break;
            default:
                // игнор ('None', 'Shot', 'Bounce' и др.) 
                return;
        }
        CommitAction(action);
    }
}
