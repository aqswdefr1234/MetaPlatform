using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button menuBtn;

    Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();
    void Start()
    {
        menuBtn.onClick.AddListener(OnClick_MenuButton);
        AddPanelList();
        SetMenuChildButton();
    }
    void AddPanelList()
    {
        foreach(Transform panel in canvas)
        {
            if (panel.tag != "Panel") continue;
            panelDict[panel.name] = panel.gameObject;
        }
    }
    void SetMenuChildButton()
    {
        Transform menuTrans = menuPanel.transform;

        Button roomBtn = FindTransform.FindChild(menuTrans, "RoomButton").GetComponent<Button>();
        roomBtn.onClick.AddListener(() => ActivatePanel("RoomPanel"));
        Button localFileBtn = FindTransform.FindChild(menuTrans, "LocalFileButton").GetComponent<Button>();
        localFileBtn.onClick.AddListener(() => ActivatePanel("LoadLocalFilePanel"));
        Button modifyBtn = FindTransform.FindChild(menuTrans, "ModifyObjectButton").GetComponent<Button>();
        modifyBtn.onClick.AddListener(() => ActivatePanel("ObjectsPanel"));
    }
    void OnClick_MenuButton()
    {
        if (menuPanel.activeSelf) menuPanel.SetActive(false);
        else menuPanel.SetActive(true);
    }
    void ActivatePanel(string panelName)
    {
        foreach (GameObject panel in panelDict.Values) panel.SetActive(false);
        panelDict[panelName].SetActive(true);
        menuPanel.SetActive(false);
    }
}
