using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
public class DataController : MonoBehaviour
{
    [SerializeField] private Transform foundationPrefab;
    [SerializeField] private Transform pointPrefab;
    [SerializeField] private Transform spotPrefab;
    [SerializeField] private GameObject waitPanel;

    public static string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MetaShopData");
    Transform loadedObjects, gltfGround, lightGround;
    ObjectParser objectParser;
    void Start()
    {
        loadedObjects = FindTransform.FindSceneRoot("LoadedObjects");
        gltfGround = FindTransform.FindChild(loadedObjects, "GLTFs");
        lightGround = FindTransform.FindChild(loadedObjects, "Lights");
        objectParser = ObjectParser.Instance;
        CreateFolder();
    }
    void CreateFolder()
    {
        string createPath = Path.Combine(defaultPath, "LightData", "RoomData");
        DirectoryFileController.IsExistFolder(createPath);

        createPath = Path.Combine(defaultPath, "BackUp");
        DirectoryFileController.IsExistFolder(createPath);
    }
    public void SaveScene()
    {
        try
        {
            waitPanel.SetActive(true);
            DirectoryFileController.EmptyFolder(defaultPath, true);
            ChangeSameName();
            SaveLights();
            SaveObjects();
            waitPanel.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            waitPanel.SetActive(false);
        }
        
    }
    public void LoadScene()
    {
        RemoveInstantiate();
        LoadLights();
        LoadObjects();
    }
    void ChangeSameName()
    {
        List<string> nameList = new List<string>();
        string newName = "";
        foreach (Transform gltf in gltfGround)
        {
            int num = 1;
            newName = gltf.name;
            while (nameList.Contains(newName))
            {
                newName = $"{newName}({num})";
                num++;
            }
            gltf.name = newName;
            nameList.Add(newName);
        }
    }
    void SaveObjects()
    {
        foreach (Transform child in gltfGround) SaveJson(child);
    }
    void SaveJson(Transform child)
    {
        ObjectParser parser = ObjectParser.Instance;
        parser.EncodingObject(child);
        string json = JsonUtility.ToJson(parser);
        string fileName = child.name;
        string path = Path.Combine(defaultPath, fileName);
        File.WriteAllText(path, json);
    }
    void SaveLights()
    {
        string lightFolder = Path.Combine(defaultPath, "LightData");
        SaveBakedLightMap(lightFolder);
        SaveRealLight(lightFolder);
    }
    void SaveBakedLightMap(string lightFolder)
    {
        string bakedPath = Path.Combine(lightFolder, "bakedData");
        DirectoryFileController.CopyFile(RoomController.currentPath, bakedPath);
        transform.GetComponent<RoomController>().InitializeBakedData();
    }
    void SaveRealLight(string lightFolder)
    {
        string lightTransPath = Path.Combine(lightFolder, "LightTransData");
        List<Light> lightList = new List<Light>();
        FillCollection.FillComponent<Light>(lightGround, lightList);
        LightsParser lightsData = new LightsParser(lightList);
        string json = JsonUtility.ToJson(lightsData);
        File.WriteAllText(lightTransPath, json);
    }
    
    void RemoveInstantiate()
    {
        foreach (Transform child in gltfGround) Destroy(child.gameObject);
        foreach (Transform child in lightGround) Destroy(child.gameObject);
    }
    void LoadObjects()
    {
        string[] files = Directory.GetFiles(defaultPath);
        for (int i = 0; i < files.Length; i++)
        {
            string json = File.ReadAllText(files[i]);
            string targetName = Path.GetFileName(files[i]);
            LoadJson(targetName, json);
        }
    }
    public void LoadJson(string fileName, string json)
    {
        ObjectParser parser = ObjectParser.Instance;
        Transform foundation = Instantiate(foundationPrefab, gltfGround);
        foundation.name = fileName;
        parser = JsonUtility.FromJson<ObjectParser>(json);
        parser.DecodingObject(parser, foundation);
    }
    void LoadLights()
    {
        string lightFolder = Path.Combine(defaultPath, "LightData");
        LoadBakedLightMap(lightFolder);
        LoadRealLight(lightFolder);
    }
    void LoadBakedLightMap(string lightFolder)
    {
        string bakedPath = Path.Combine(lightFolder, "bakedData");
        if (!File.Exists(bakedPath)) return;
        RoomController.currentPath = bakedPath;
        transform.GetComponent<RoomController>().LoadBakedMap(bakedPath, "bakedData");
    }
    void LoadRealLight(string lightFolder)
    {
        string lightTransPath = Path.Combine(lightFolder, "LightTransData");
        if (!File.Exists(lightTransPath)) return;
        
        string json = File.ReadAllText(lightTransPath);
        LoadRealLightJson(json);
    }
    public void LoadRealLightJson(string json)
    {
        LightsParser lightsData = JsonUtility.FromJson<LightsParser>(json);

        int count = lightsData.lightType.Count;
        for (int i = 0; i < count; i++)
        {
            Transform newLight = null;
            if (lightsData.lightType[i] == "Spot") newLight = Instantiate(spotPrefab, lightGround);
            else if (lightsData.lightType[i] == "Point") newLight = Instantiate(pointPrefab, lightGround);

            if (newLight == null) continue;
            newLight.position = lightsData.posList[i];
            newLight.eulerAngles = lightsData.rotList[i];

            Light light = newLight.GetComponent<Light>();
            light.color = lightsData.colorList[i];
            light.intensity = lightsData.intensityList[i];
        }
    }
}
[System.Serializable]
public class ObjectParser
{
    public static ObjectParser instance = null;
    public static ObjectParser Instance
    {
        get
        {
            if (instance == null) instance = new ObjectParser();
            return instance;
        }
        set { instance = value; }
    }
    public List<GltfParser> gltfList = new List<GltfParser>();
    public List<int> orderList = new List<int>();
    public Vector3 pos = new Vector3();
    public Vector3 rot = new Vector3();
    public Vector3 size = new Vector3();

    public void EncodingObject(Transform foundation)
    {
        Clear();
        Encoding(foundation);
    }
    void Encoding(Transform foundation)
    {
        (pos, rot, size) = ConvertData.TransToVec(foundation);
        List<(int, Transform)> hierarchy = new List<(int, Transform)>();
        Transform target = foundation.GetChild(0);
        FillCollection.FillTransformHierarchy(target, 0, hierarchy);
        foreach ((int, Transform) item in hierarchy)
        {
            GltfParser gltf = new GltfParser();
            gltf.EncodingGltf(item.Item2);
            gltfList.Add(gltf);
            orderList.Add(item.Item1);
        }
    }
    public void DecodingObject(ObjectParser data, Transform foundation)
    {
        CreateObject(data.gltfList.ToArray(), data.orderList.ToArray(), foundation);
    }
    void CutList(int index, List<GameObject> list)
    {
        int count = list.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (index == list.Count - 1) return;
            list.RemoveAt(i);
        }
    }
    void CreateObject(GltfParser[] gltfArr, int[] orderArr, Transform foundation)
    {
        List<GameObject> orderList = new List<GameObject>();

        for (int i = 0; i < orderArr.Length; i++)
        {
            GltfParser gltf = new GltfParser();
            if (i == 0)
            {
                GameObject parent = new GameObject();
                parent.transform.SetParent(foundation);
                gltf.DecodingGltf(gltfArr[i], parent.transform);
                orderList.Add(parent);
                continue;
            }
            if (orderArr[i] > orderArr[i - 1])//이전 오브젝트의 자식 오브젝트 인 경우
            {
                GameObject newObject = new GameObject();
                newObject.transform.SetParent(orderList[orderArr[i - 1]].transform);
                gltf.DecodingGltf(gltfArr[i], newObject.transform);
                orderList.Add(newObject);
            }
            else
            {
                int parentIndex = orderArr[i] - 1;
                CutList(parentIndex, orderList);

                GameObject newObject = new GameObject();
                newObject.transform.SetParent(orderList[parentIndex].transform);
                gltf.DecodingGltf(gltfArr[i], newObject.transform);
                orderList.Add(newObject);
            }
        }
        ConvertData.VecToTrans(foundation, (pos, rot, size));
    }
    void Clear()
    {
        gltfList.Clear();
        orderList.Clear();
    }
}
[System.Serializable]
public class GltfParser
{
    public Vector3 pos = new Vector3();
    public Vector3 rot = new Vector3();
    public Vector3 size = new Vector3();
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<TrianglesData> triangles = new List<TrianglesData>();
    public void EncodingGltf(Transform target)
    {
        //Clear();

        (pos, rot, size) = ConvertData.TransToVec(target);
        if (target.GetComponent<MeshFilter>() == null) return;
        Mesh mesh = target.GetComponent<MeshFilter>().mesh;
        Material[] materials = target.GetComponent<MeshRenderer>().materials;
        vertices.AddRange(mesh.vertices);
        normals.AddRange(mesh.normals);
        uvs.AddRange(mesh.uv);

        int subMeshCount = mesh.subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
        {
            TrianglesData triangle = new TrianglesData(mesh.GetTriangles(i), materials[i]);
            triangles.Add(triangle);
        }
    }
    public void DecodingGltf(GltfParser data, Transform newTrans)
    {
        //Clear();

        if (data.vertices.Count == 0)
        {
            ConvertData.VecToTrans(newTrans, (data.pos, data.rot, data.size));
            return;
        }

        Mesh mesh = newTrans.gameObject.AddComponent<MeshFilter>().mesh;
        List<Material> materials = new List<Material>();
        mesh.vertices = data.vertices.ToArray();
        mesh.normals = data.normals.ToArray();
        mesh.uv = data.uvs.ToArray();
       
        mesh.subMeshCount = data.triangles.Count;//미리 설정해야 에러안나옴.
        for (int i = 0; i < data.triangles.Count; i++)
        {
            mesh.SetTriangles(data.triangles[i].subTriangles, i);

            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = data.triangles[i].color;
            materials.Add(material);
        }
        newTrans.gameObject.AddComponent<MeshRenderer>().materials = materials.ToArray();
        ConvertData.VecToTrans(newTrans, (data.pos, data.rot, data.size));
    }
    void Clear()
    {
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        triangles.Clear();
    }
}
[System.Serializable]
public class TrianglesData
{
    public Color32 color = new Color32(255, 255, 255, 255);
    public List<int> subTriangles = new List<int>();//머터리얼 여러개 있을 시 부분별로 할당해주기 위해
    public TrianglesData(int[] triangles, Material material)
    {
        subTriangles.AddRange(triangles);
        color = material.color;
    }
}
[System.Serializable]
public class LightsParser
{
    public List<string> lightType = new List<string>();
    public List<Vector3> posList = new List<Vector3>();
    public List<Vector3> rotList = new List<Vector3>();
    public List<Color32> colorList = new List<Color32>();
    public List<int> intensityList = new List<int>();
    public LightsParser(List<Light> lightList)
    {
        foreach (Light light in lightList)
        {
            if (light == null) continue;
            lightType.Add(light.type.ToString());
            posList.Add(light.transform.position);
            rotList.Add(light.transform.eulerAngles);
            colorList.Add(light.color);
            intensityList.Add((int)light.intensity);
        }
    }
}