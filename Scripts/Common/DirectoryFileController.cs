using System.IO.Compression;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class DirectoryFileController : MonoBehaviour
{
    public static void IsExistFolder(string path)
    {
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }
    public static (string, FileSystemInfo[]) ReturnDirFile(string folderPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        if (!directoryInfo.Exists) return ("", null);

        string folderName = directoryInfo.Name;
        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        return (folderName, fileSystemInfos);
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
        string bakedPath = Path.Combine(DataController.defaultPath, "LightData", "bakedData");
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
    public static void CompressZIP(string[] filesToCompress, string outputZipFilePath)
    {
        // .zip 파일을 생성
        using (FileStream zipToOpen = new FileStream(outputZipFilePath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                // 각 파일을 .zip 파일에 추가
                foreach (string fileToCompress in filesToCompress)
                {
                    // 파일이 존재하는지 확인
                    if (File.Exists(fileToCompress))
                    {
                        // 파일을 .zip 파일에 추가
                        string fileNameInArchive = Path.GetFileName(fileToCompress);
                        archive.CreateEntryFromFile(fileToCompress, fileNameInArchive);
                    }
                    else
                    {
                        Debug.LogWarning($"File not found: {fileToCompress}");
                    }
                }
            }
        }
    }
}
