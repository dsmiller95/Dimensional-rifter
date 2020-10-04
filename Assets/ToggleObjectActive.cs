using UnityEngine;

public class ToggleObjectActive : MonoBehaviour
{

    public void ToggleActive(GameObject gameObject)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
