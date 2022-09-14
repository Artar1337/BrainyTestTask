using UnityEngine;

public class ButtonsHandler : MonoBehaviour
{
    public void StartGame(bool withRandomField)
    {
        GameController.instance.StartGame(withRandomField);
    }
}
