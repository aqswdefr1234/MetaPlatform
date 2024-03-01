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
        // FirebaseApp의 인스턴스를 가져옵니다.
        FirebaseApp app = FirebaseApp.DefaultInstance;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            if (task.Result == DependencyStatus.Available)
            {
                // FirebaseApp이 성공적으로 초기화된 경우
                Debug.Log("FirebaseApp initialized successfully!");
                storage = FirebaseStorage.DefaultInstance;
            }
            else
            {
                // FirebaseApp 초기화 중 오류가 발생한 경우
                Debug.LogError($"Failed to initialize FirebaseApp: {task.Result}");
            }
        });
        // FirebaseStorage의 인스턴스를 생성합니다.
        
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
    // .zip 파일 업로드 함수
    public async Task UploadZipFile(string localFilePath, string remoteFileName)
    {
        // Firebase Storage의 참조를 얻어옵니다.
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://metaverseshop-72b74.appspot.com");

        // 로컬 파일로부터 바이트 배열을 읽어옵니다.
        byte[] data = System.IO.File.ReadAllBytes(localFilePath);

        // .zip 파일을 업로드합니다.
        StorageReference zipFileRef = storageRef.Child(remoteFileName);
        await zipFileRef.PutBytesAsync(data);
        Debug.Log("Zip file uploaded successfully!");
    }

    // .zip 파일 다운로드 함수
    public async Task DownloadZipFile(string remoteFilePath, string localFilePath)
    {
        // Firebase Storage의 참조를 얻어옵니다.
        StorageReference storageRef = storage.GetReferenceFromUrl("gs://metaverseshop-72b74.appspot.com");

        // 저장된 .zip 파일을 다운로드합니다.
        StorageReference zipFileRef = storageRef.Child(remoteFilePath);
        await zipFileRef.GetBytesAsync(1024 * 1024 * 1000) // 최대 10MB 크기로 설정 (원하는 크기로 변경 가능)
            .ContinueWith((Task<byte[]> task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Failed to download zip file!{task.Exception.ToString()}");
                    return;
                }

                // 다운로드된 바이트 배열을 로컬 파일로 저장합니다.
                File.WriteAllBytes(localFilePath, task.Result);
                Debug.Log("Zip file downloaded successfully!");
            });
    }
}
