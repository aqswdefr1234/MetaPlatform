using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeLightController : MonoBehaviour
{
    [SerializeField] private Transform lightsTrans;
    [SerializeField] private GameObject pointLightPrefab;
    [SerializeField] private GameObject spotLightPrefab;

    public void CreatePointLight() 
    {
        if (lightsTrans.childCount > 29) return;
        Instantiate(pointLightPrefab, lightsTrans).transform.name = $"Point{lightsTrans.childCount}";
    }
    public void CreateSpotLight()
    {
        if (lightsTrans.childCount > 29) return;
        Instantiate(spotLightPrefab, lightsTrans).transform.name = $"Spot{lightsTrans.childCount}"; ;
    }
}
