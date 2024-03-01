using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using System.IO.Compression;
using GLTFast;
public class test : MonoBehaviour
{
    // Start is called before the first frame update
    public List<byte[]> bytesList = new List<byte[]>();
    string des = Path.Combine(DataController.defaultPath, "testDeCom");
    void Start()
    {
        string path = Path.Combine(DataController.defaultPath);
        string[] files = Directory.GetFiles(path);
        
        LoadBytesListFromFile();
        foreach (byte[] bytes in bytesList)
        {
            Debug.Log("yeah");
            testWeb(bytes);
        }
    }
    public async void testWeb(byte[] dataFromWeb)
    {
        Debug.Log("ho");
        // GltfImport 인스턴스 생성
        var gltf = new GltfImport();

        // glTF 바이너리 로드
        bool success = await gltf.LoadGltfBinary(dataFromWeb, null);
        Debug.Log("wait?");
        if (success)
        {
            // 로드된 glTF의 메인 씬을 인스턴스화
            await gltf.InstantiateMainSceneAsync(transform);
        }
        else
        {
            Debug.LogError("glTF 로드 실패!");
        }
    }

    
    
    public void testBytes()
    {
        string path = Path.Combine(DataController.defaultPath, "SampleText.glb");
        string dest = Path.Combine(DataController.defaultPath, "test");
        byte[] bytes = File.ReadAllBytes(path);
        File.WriteAllBytes(dest, bytes);
    }
    void DecompressFile(string compressedFile, string restoredFile)
    {
        // 압축된 파일을 열고 읽기 전용으로 스트림을 생성합니다.
        using (FileStream compressedFileStream = File.OpenRead(compressedFile))
        {
            // 복원된 파일을 생성하고 쓰기 전용으로 스트림을 생성합니다.
            using (FileStream restoredFileStream = File.Create(restoredFile))
            {
                // GZipStream을 사용하여 파일을 압축 해제합니다.
                using (GZipStream decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                {
                    // 압축 해제된 데이터를 복원된 파일에 씁니다.
                    decompressionStream.CopyTo(restoredFileStream);
                }
            }
        }
    }
    void SaveBytesListToFile()
    {
        // Check if the bytes list is empty
        if (bytesList.Count == 0)
        {
            Debug.LogWarning("Byte list is empty. Nothing to save.");
            return;
        }

        // Create a FileStream to write to the file
        using (BinaryWriter writer = new BinaryWriter(File.Open(des, FileMode.Create)))
        {
            // Write the number of byte arrays in the list
            writer.Write(bytesList.Count);

            // Iterate through the byte arrays in the list
            foreach (byte[] byteArray in bytesList)
            {
                // Write the length of the byte array
                writer.Write(byteArray.Length);
                // Write the byte array itself
                writer.Write(byteArray);
            }
        }

        Debug.Log($"Byte list saved Count: {bytesList.Count}");
    }

    void LoadBytesListFromFile()
    {
        // Check if the file exists
        if (!File.Exists(des))
        {
            Debug.LogWarning("File does not exist: " + des);
            return;
        }

        // Clear the existing byte list
        bytesList.Clear();

        // Create a FileStream to read from the file
        using (BinaryReader reader = new BinaryReader(File.Open(des, FileMode.Open)))
        {
            // Read the number of byte arrays in the file
            int count = reader.ReadInt32();

            // Iterate through the byte arrays in the file
            for (int i = 0; i < count; i++)
            {
                // Read the length of the byte array
                int length = reader.ReadInt32();
                // Read the byte array itself
                byte[] byteArray = reader.ReadBytes(length);
                // Add the byte array to the list
                bytesList.Add(byteArray);
            }
        }
        Debug.Log($"Byte list loaded Count: {bytesList.Count}");
        
    }
    void CompressFile(string inputFile, string compressedFile)
    {
        // 원본 파일을 열고 읽기 전용으로 스트림을 생성합니다.
        using (FileStream originalFileStream = File.OpenRead(inputFile))
        {
            // 압축 파일을 생성하고 쓰기 전용으로 스트림을 생성합니다.
            using (FileStream compressedFileStream = File.Create(compressedFile))
            {
                // GZipStream을 사용하여 파일을 압축합니다.
                using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    // 원본 파일에서 읽어와 압축 스트림에 쓰기를 수행합니다.
                    originalFileStream.CopyTo(compressionStream);
                }
            }
        }
    }
}