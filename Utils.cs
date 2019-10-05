using System.IO;

namespace RPCS3_Updater
{
    public static class Utils
    {
        public static FileInfo GetFile(this DirectoryInfo obj, string filename) => new FileInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{filename}");

        public static string GetCommit(this RpcsBuild obj)
        {
            foreach (string seg in obj.download.Segments)
            {
                if (seg.Length > 5 && seg.Substring(0, 5) == "build")
                    return seg.Substring(6, 8);
            }
            return null;
        }

    }
}
