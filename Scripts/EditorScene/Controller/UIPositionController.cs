using System;
using UnityEngine;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
public class UIPositionController : MonoBehaviour
{
    [Header("Unit width : 0 ~ 20, height : 0 ~ 11")]
    [SerializeField] private UIPosition[] uiArr;
    
    int widthUnit, heightUnit;
    void Start()
    {
        widthUnit = (int)Math.Round(Screen.width / 20f);
        heightUnit = (int)Math.Round(Screen.height / 11f);

        foreach (UIPosition ui in uiArr)
        {
            ui.target.position = new Vector3(ui.widthInt * widthUnit, ui.heightInt * heightUnit, 0);
        }
    }
}
[System.Serializable]
public class UIPosition
{
    public Transform target;
    public int widthInt;
    public int heightInt;
}