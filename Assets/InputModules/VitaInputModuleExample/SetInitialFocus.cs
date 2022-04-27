using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SetInitialFocus : MonoBehaviour
{
    public GameObject initialOption;

    void Start ()
    {
        EventSystem.current.SetSelectedGameObject(initialOption);
    }
    
    void Update ()
    {
    }
}
