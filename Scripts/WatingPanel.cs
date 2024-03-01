using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WatingPanel : MonoBehaviour
{
    [SerializeField] private Transform icon;
    private bool isRotating = true;

    private async void OnEnable()
    {
        isRotating = true;
        await RotateIconAsync();
    }

    private void OnDisable()
    {
        isRotating = false;
    }

    private async Task RotateIconAsync()
    {
        while (isRotating)
        {
            Debug.Log("ºù±Ûºù±Û");
            icon.Rotate(0f, 0f, -1f);
            await Task.Delay(10); // È¸Àü °£°Ý Á¶Á¤
        }
    }
}
