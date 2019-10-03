using System.IO;

namespace RPCS3_Updater
{
    public static class Utils
    {
        public static FileInfo GetFile(this DirectoryInfo obj, string filename) => new FileInfo($"{obj.FullName}{Path.DirectorySeparatorChar}{filename}");
    }
}
