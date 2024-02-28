using System;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocalFileController : MonoBehaviour
{
    [SerializeField] private Transform localFilePanel;
    [SerializeField] private Transform btnPrefab;

    List<string> pathList = new List<string>();
    string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    TMP_Text folderName; Button returnBtn; Transform fileContent;

    void Start()
    {
        ConnectTransform();
    }
    void ConnectTransform()
    {
        folderName = FindTransform.FindChild(localFilePanel, "FolderName").GetComponent<TMP_Text>();
        fileContent = FindTransform.FindChild(localFilePanel, "Content");
        returnBtn = FindTransform.FindChild(localFilePanel, "ReturnButton").GetComponent<Button>();
        returnBtn.onClick.AddListener(OnClick_ReturnButton);

        LoadLocalFolder(defaultPath, false);
    }
    void OnClick_ReturnButton()
    {
        if (pathList.Count <= 1) return;
        pathList.RemoveAt(pathList.Count - 1);
        LoadLocalFolder(pathList[pathList.Count - 1], true);
    }
    void LoadLocalFolder(string path, bool isReturn)//폴더에 들어가거나, 뒤로가기 버튼을 누를때 실행된다.
    {
        ClearView();
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        if (!directoryInfo.Exists) { Debug.LogError("Directory not found"); return; }

        folderName.text = directoryInfo.Name;
        if (!isReturn) pathList.Add(directoryInfo.FullName);

        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        foreach (FileSystemInfo fsi in fileSystemInfos) InstantiateButton(fsi);
    }
    void InstantiateButton(FileSystemInfo fsi)
    {
        if (fsi is FileInfo file)
        {
            if (file.Extension != ".glb") return;

            Transform btn = Instantiate(btnPrefab, fileContent);
            if (file.Name.Length <= 10) btn.GetComponentInChildren<TMP_Text>().text = file.Name;
            else btn.GetComponentInChildren<TMP_Text>().text = file.Name.Substring(0, 5) + "..." + file.Name.Substring(5, 4);
            btn.GetComponentInChildren<TMP_Text>().text = file.Name;
            btn.GetComponent<Button>().onClick.AddListener(() => GLTFLoader.Instance.LoadLocal(file.FullName));
        }
        else if (fsi is DirectoryInfo directory)
        {
            Transform btn = Instantiate(btnPrefab, fileContent);
            if (directory.Name.Length <= 10) btn.GetComponentInChildren<TMP_Text>().text = directory.Name;
            else btn.GetComponentInChildren<TMP_Text>().text = directory.Name.Substring(0, 5) + "..." + directory.Name.Substring(5, 4);
            btn.GetComponent<UnityEngine.UI.Image>().color = new Color(1.0f, 1.0f, 0.0f);
            btn.GetComponent<Button>().onClick.AddListener(() => LoadLocalFolder(directory.FullName, false));
        }
    }
    void ClearView()
    {
        foreach (Transform btn in fileContent) Destroy(btn.gameObject);
    }
}
public class DirectoryFileController
{
    public static void IsExistFolder(string path)
    {
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }
    public static void EmptyFolder(string folderPath, bool backUp)
    {
        string[] files = Directory.GetFiles(folderPath);
        if (!backUp)
        {
            foreach (string file in files) File.Delete(file); return;
        }
        string backPath = Path.Combine(folderPath, "BackUp");
        foreach (string file in files)
        {
            string newPath = Path.Combine(backPath, Path.GetFileName(file));
            if (File.Exists(newPath)) File.Delete(newPath);
            File.Move(file, newPath);
        }
    }
    public static void CopyFile(string targetPath, string wantPath)
    {
        Debug.Log(targetPath);
        Debug.Log(wantPath);
        string bakedPath = Path.Combine(DataController.defaultPath, "LightData","bakedData");
        if (!File.Exists(targetPath)) return;
        if (targetPath == wantPath) return;

        if ((bakedPath != wantPath) && File.Exists(wantPath))
        {
            string directory = Path.GetDirectoryName(wantPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(wantPath);
            string fileExtension = Path.GetExtension(wantPath);

            int counter = 1;
            string newFileName = $"{fileNameWithoutExtension}({counter}){fileExtension}";
            string newFilePath = Path.Combine(directory, newFileName);

            while (File.Exists(newFilePath))
            {
                counter++;
                newFileName = $"{fileNameWithoutExtension}({counter}){fileExtension}";
                newFilePath = Path.Combine(directory, newFileName);
            }

            wantPath = newFilePath;
        }

        File.Copy(targetPath, wantPath, true);
    }
}