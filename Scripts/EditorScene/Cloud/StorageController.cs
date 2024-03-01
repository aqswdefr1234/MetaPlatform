using Firebase;
using Firebase.Storage;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class StorageController : MonoBehaviour
{
    FirebaseStorage storage;
    string webFileName = "storagefile";
    private void Start()
    {
        // FirebaseApp�� �ν��Ͻ��� �����ɴϴ�.
        FirebaseApp app = FirebaseApp.DefaultInstance;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            if (task.Result == DependencyStatus.Available)
            {
                // FirebaseApp�� ���������� �ʱ�ȭ�� ���
                Debug.Log("FirebaseApp initialized successfully!");
                storage = FirebaseStorage.DefaultInstance;
            }
            else
            {
                // FirebaseApp �ʱ�ȭ �� ������ �߻��� ���
                Debug.LogError($"Failed to initialize FirebaseApp: {task.Result}");
            }
        });
        // FirebaseStorage�� �ν��Ͻ��� �����մϴ�.
        
    }
    public async void UpLoadTest()
    {
        string localPath = Path.Combine(DataController.defaultPath, "CloudFolder", webFileName);
        await UploadZipFile(localPath, webFileName);
    }
    public async void DownLoadTest()
    {
        string downPath = Path.Combine(DataController.defaultPath, "CloudFolder", "readstoragefile");
        await DownloadZipFile(webFileName, downPath);
    }
    // .zip ���� ���ε� �Լ�
    public async Task UploadZipFile(string localFilePath, string remoteFileName)
    {
        // Firebase Storage�� ������ ���ɴϴ�.
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://metaverseshop-72b74.appspot.com");

        // ���� ���Ϸκ��� ����Ʈ �迭�� �о�ɴϴ�.
        byte[] data = System.IO.File.ReadAllBytes(localFilePath);

        // .zip ������ ���ε��մϴ�.
        StorageReference zipFileRef = storageRef.Child(remoteFileName);
        await zipFileRef.PutBytesAsync(data);
        Debug.Log("Zip file uploaded successfully!");
    }

    // .zip ���� �ٿ�ε� �Լ�
    public async Task DownloadZipFile(string remoteFilePath, string localFilePath)
    {
        // Firebase Storage�� ������ ���ɴϴ�.
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://metaverseshop-72b74.appspot.com");

        // ����� .zip ������ �ٿ�ε��մϴ�.
        StorageReference zipFileRef = storageRef.Child(remoteFilePath);
        await zipFileRef.GetBytesAsync(1024 * 1024 * 1000) // �ִ� 10MB ũ��� ���� (���ϴ� ũ��� ���� ����)
            .ContinueWith((Task<byte[]> task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Failed to download zip file!{task.Exception.ToString()}");
                    return;
                }

                // �ٿ�ε�� ����Ʈ �迭�� ���� ���Ϸ� �����մϴ�.
                File.WriteAllBytes(localFilePath, task.Result);
                Debug.Log("Zip file downloaded successfully!");
            });
    }
}
