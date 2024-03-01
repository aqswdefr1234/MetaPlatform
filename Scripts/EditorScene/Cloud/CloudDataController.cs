using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections;
using LightmapAnalysis;

public class CloudDataController : MonoBehaviour
{
    Transform loadedObjects, gltfGround, lightGround;
    public Transform test;
    void Start()
    {
        SetTransform();
    }
    void SetTransform()
    {
        loadedObjects = FindTransform.FindSceneRoot("LoadedObjects");
        gltfGround = FindTransform.FindChild(loadedObjects, "GLTFs");
        lightGround = FindTransform.FindChild(loadedObjects, "Lights");
    }
    IEnumerator AsyncTasking(Task task, Action successAction)
    {
        while (true)
        {
            if (task.IsCompleted)
            {
                successAction();
                break;
            }
            yield return null;
        }
    }
    public void SaveOneFile()
    {
        Task task = Task.Run(SaveCompressFile);
        StartCoroutine(AsyncTasking(task, () => { }));
    }
    public void LoadOneFile()
    {
        OverallData allData = null;
        Task task = Task.Run(() =>
        {
            allData = LoadAllData();
        });
        StartCoroutine(AsyncTasking(task, () => LoadJson(allData)));
    }
    public void SaveCompressFile()
    {
        string savedFolder = Path.Combine(DataController.defaultPath, "CloudFolder");
        string savedName = Path.Combine(savedFolder, "storagefile");
        DirectoryFileController.IsExistFolder(savedFolder);

        string json = SerializeAllData();
        byte[] gzipBytes = SaveCompressedJsonToFile(json);
        File.WriteAllBytes(savedName, gzipBytes);
    }
    public OverallData LoadAllData()
    {
        string loadFile = Path.Combine(DataController.defaultPath, "CloudFolder", "storagefile");
        byte[] loadBytes = File.ReadAllBytes(loadFile);
        string json = LoadCompressedJsonFromFile(loadBytes);
        OverallData allData = JsonUtility.FromJson<OverallData>(json);
        return allData;
    }
    public void LoadJson(OverallData allData)
    {
        int length = allData.objectNameArr.Length;
        for (int i = 0; i < length; i++)
        {
            transform.GetComponent<DataController>().LoadJson(allData.objectNameArr[i], allData.objectDataList[i]);
        }
        transform.GetComponent<DataController>().LoadRealLightJson(allData.lightTransData);
        transform.GetComponent<RoomController>().LoadJson("Web:Current", allData.bakedData);
    }
    string SerializeAllData()
    {
        string objectFolder = DataController.defaultPath;
        string bakedFolder = Path.Combine(objectFolder, "LightData", "bakedData");
        string lightTransFolder = Path.Combine(objectFolder, "LightData", "LightTransData");

        (string[] nameArr, string[] dataArr) = ReadLocalObject(objectFolder);
        string bakedData = File.ReadAllText(bakedFolder);
        string transData = File.ReadAllText(lightTransFolder);

        OverallData allData = new OverallData(nameArr, dataArr, bakedData, transData);
        return JsonUtility.ToJson(allData);
    }
    (string[], string[]) ReadLocalObject(string folderPath)
    {
        string[] pathArr = Directory.GetFiles(folderPath);
        string[] nameArr = new string[pathArr.Length];
        string[] dataArr = new string[pathArr.Length];
        for(int i = 0; i < pathArr.Length; i++)
        {
            nameArr[i] = Path.GetFileName(pathArr[i]);
            dataArr[i] = File.ReadAllText(pathArr[i]);
        }
        return (nameArr, dataArr);
    }
    private byte[] SaveCompressedJsonToFile(string jsonData)
    {
        byte[] compressedData;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                using (StreamWriter writer = new StreamWriter(gzipStream))
                {
                    writer.Write(jsonData);
                }
            }
            compressedData = memoryStream.ToArray();//압축된 byte[] 데이터
        }
        return compressedData;

    }
    private string LoadCompressedJsonFromFile(byte[] zipData)
    {
        byte[] compressedData = zipData;

        using (MemoryStream memoryStream = new MemoryStream(compressedData))
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                using (StreamReader reader = new StreamReader(gzipStream))//StreamReader : 특정 인코딩의 바이트 스트림에서 문자를 읽는 TextReader 를 구현.
                {
                    string jsonData = reader.ReadToEnd();
                    return jsonData;
                }
            }
        }
    }
}
[System.Serializable]
public class OverallData
{
    public string[] objectNameArr = null;
    public string[] objectDataList = null;
    public string bakedData = "";
    public string lightTransData = "";

    public OverallData(string[] nameArr, string[] objectFileArr, string bakedFile, string lightTransFile)
    {
        objectNameArr = nameArr;
        objectDataList = objectFileArr;
        bakedData = bakedFile;
        lightTransData = lightTransFile;
    }
}