using UnityEngine;
using LightmapAnalysis;
using Debug = UnityEngine.Debug;

public class LightController : MonoBehaviour
{
    LightmapAnalyzer lightmapAnalyzer;
    void Start()
    {
        lightmapAnalyzer = LightmapAnalyzer.DefaultInstance;
        Example_Export();
    }
    public void Example_Export()
    {
        lightmapAnalyzer.Export();
    }
    public void Example_Import()
    {
        lightmapAnalyzer.Import();
    }
    public void Example_ChangeLightmap()
    {
        //lightmapAnalyzer.ChangeLightmap(testChangeTrans, testChangeKey);
    }
    public void Example_ImportWeb()
    {
        string keyName = "Set Your Web Json Name";
        string json = "Your Web <BakedLightmapData> Data";
        lightmapAnalyzer.SetDataDictionary("Web:" + keyName, json);
        lightmapAnalyzer.Import();
    }
}
