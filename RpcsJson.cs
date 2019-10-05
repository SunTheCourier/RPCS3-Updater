using System;

namespace RPCS3_Updater
{
    public class RpcsJson
    {
        public int return_code;
        public RpcsBuilds latest_build;
    }
    public class RpcsBuild
    {
        public Uri download;
        //public ulong size;
        //public string checksum;
    }

    public class RpcsBuilds
    {
        public ulong pr;
        //public DateTime datetime;
        public string version;
        public RpcsBuild windows;
        //public RpcsBuild linux;
    }
}
