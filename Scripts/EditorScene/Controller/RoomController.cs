using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using UnityEngine;
using LightmapAnalysis;
using TMPro;

public class RoomController : MonoBehaviour
{
    public static string currentPath = "";

    [SerializeField] private Transform room;
    [SerializeField] private Transform roomPanel;
    [SerializeField] private Transform btnPrefab;

    string roomPath;
    Transform roomContent; Button refreshBtn;
    List<string> pathList = new List<string>();
    LightmapAnalyzer lightmapAnalyzer;

    void Start()
    {
        lightmapAnalyzer = LightmapAnalyzer.DefaultInstance;
        roomPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MetaShopData", "LightData", "RoomData");
        ConnectTransform();
    }
    void ConnectTransform()
    {
        roomContent = FindTransform.FindChild(roomPanel, "Content");
        refreshBtn = FindTransform.FindChild(roomPanel, "RefreshButton").GetComponent<Button>();
        refreshBtn.onClick.AddListener(() => LoadFolder(roomPath));
        LoadFolder(roomPath);
    }
    void LoadFolder(string path)//폴더에 들어가거나, 뒤로가기 버튼을 누를때 실행된다.
    {
        ClearView();
        string[] files = Directory.GetFiles(roomPath);
        InstantiateButton(files);
    }
    void InstantiateButton(string[] files)
    {
        foreach (string file in files)
        {
            Transform btn = Instantiate(btnPrefab, roomContent);
            btn.GetComponentInChildren<TMP_Text>().text = Path.GetFileName(file);
            string fileName = Path.Combine("RoomData", Path.GetFileName(file));
            btn.GetComponent<Button>().onClick.AddListener(() => LoadBakedMap(file, fileName));
        }
    }
    public void LoadBakedMap(string filePath, string fileName)
    {
        if (room.GetComponent<PathToBeLoaded>().paths.Contains(fileName))
            lightmapAnalyzer.ChangeLightmap(room, fileName);

        if (!lightmapAnalyzer.loadedDataDict.ContainsKey(fileName))
        {
            if(!room.GetComponent<PathToBeLoaded>().paths.Contains(fileName))
                room.GetComponent<PathToBeLoaded>().paths.Add(fileName);
            lightmapAnalyzer.Import();
            StartCoroutine(LoadMap(fileName));
        }
        currentPath = filePath;
        Debug.Log(currentPath);
    }
    IEnumerator LoadMap(string fileName)
    {
        WaitForSeconds delay = new WaitForSeconds(0.2f);
        int loopCount = 0;
        while (true)
        {
            if (!CheckSuccess(lightmapAnalyzer.coroutineArr))
            {
                loopCount++;
                if (loopCount > 300) break;
                yield return delay;
                continue;
            }
            lightmapAnalyzer.ChangeLightmap(room, fileName);
            lightmapAnalyzer.coroutineArr = new bool[] { false, false, false };
            break;
        }
    }
    bool CheckSuccess(bool[] arr)
    {
        foreach(bool a in arr)
        {
            if (a == false) return false;
        }
        return true;
    }
    void ClearView()
    {
        foreach (Transform btn in roomContent) Destroy(btn.gameObject);
    }
    public void InitializeBakedData() 
    {
        room.GetComponent<PathToBeLoaded>().paths.Remove("bakedData");
        lightmapAnalyzer.loadedDataDict.Remove("bakedData");
    }
}
