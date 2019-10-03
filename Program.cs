using Newtonsoft.Json;
using SevenZipNET;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace RPCS3_Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo runningLocation = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            FileInfo zipLib = runningLocation.GetFile("7za.exe");
            if (!zipLib.Exists)
            {
                MessageBox.Show($"7za.exe does not exist!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FileInfo index = runningLocation.Parent.GetFile("index.json");
            using (WebClient client = new WebClient())
            {
                string rawJson = client.DownloadString("https://update.rpcs3.net/?api=v1&c=");
                RpcsJson json = JsonConvert.DeserializeObject<RpcsJson>(rawJson);
                if (json.return_code != 0)
                {
                    MessageBox.Show($"API returned code {json.return_code}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!index.Exists)
                {
                    if (MessageBox.Show("Would you like to download the latest RPCS3 version to the parent directory of where this application is running? (selecting no will assume that you have it installed the latest version already!)", "Install", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    {
                        File.WriteAllText(index.FullName, rawJson);
                        return;
                    }
                }
                else
                {
                    RpcsJson indexJson = JsonConvert.DeserializeObject<RpcsJson>(File.ReadAllText(index.FullName));
                    if (json.latest_build.version == indexJson.latest_build.version)
                    {
                        MessageBox.Show("RPCS3 is up to date!", "RPCS-Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                string tempPath = Path.GetTempFileName();
                client.DownloadFile(json.latest_build.windows.download, tempPath);
                SevenZipBase.Path7za = zipLib.FullName;
                SevenZipExtractor extractor = new SevenZipExtractor(tempPath);
                extractor.ExtractAll(runningLocation.Parent.FullName, true);
                File.WriteAllText(index.FullName, rawJson);
            }
        }
    }
}
