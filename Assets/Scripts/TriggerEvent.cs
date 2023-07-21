using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    public UISystem UISystem;

    public void OnToggleChange(bool value)
    {
        Debug.Log($"TOGGLE {value}");
        UISystem.OnToggleChange(value);
    }

    public void OnDropDownChange(int index)
    {
        Debug.Log($"index {index}");
        UISystem.OnDropDownChanged(index);
    }
}
