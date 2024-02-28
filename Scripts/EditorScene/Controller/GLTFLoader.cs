using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using UnityEngine;

public class GLTFLoader : MonoBehaviour
{
    [SerializeField] private Transform gltfPrefab;
    [SerializeField] private List<Shader> requiredShader = new List<Shader>();//빌드에 urp쉐이더를 포함시키기 위해 할당. 직접 사용은 x
    public static GLTFLoader Instance = null;
    Transform gltfGround;
    void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }
    void Start()
    {
        gltfGround = FindTransform.FindChild(GameObject.Find("LoadedObjects").transform, "GLTFs");
    }
    public void LoadLocal(string path)
    {
        Debug.Log(path);
        Transform gltf = Instantiate(gltfPrefab, gltfGround);
        gltf.gameObject.AddComponent<GLTFast.GltfAsset>().Url = path;
    }
   
}
