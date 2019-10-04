using Newtonsoft.Json;
using SevenZipNET;
using System;
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
            
            using (WebClient client = new WebClient())
            {
                string rawJson = client.DownloadString("https://update.rpcs3.net/?api=v1&c=");
                RpcsJson updateJson = JsonConvert.DeserializeObject<RpcsJson>(rawJson);
                if (updateJson.return_code != 0)
                {
                    MessageBox.Show($"API returned non-zero code: {updateJson.return_code}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                FileInfo index = runningLocation.Parent.GetFile("index.json");
                if (!index.Exists)
                {
                    if (MessageBox.Show("Would you like to download the latest RPCS3 version to the parent directory of where this application is running? (selecting no will assume that you have installed the latest version already!)", "Install", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    {
                        File.WriteAllText(index.FullName, rawJson);
                        return;
                    }
                }
                else
                {
                    RpcsJson indexJson = JsonConvert.DeserializeObject<RpcsJson>(File.ReadAllText(index.FullName));
                    if (updateJson.latest_build.version == indexJson.latest_build.version)
                    {
                        MessageBox.Show("RPCS3 is up to date!", "RPCS-Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                Console.WriteLine("Updating RPCS3...\nProgram will automatically exit when it is done!");
                string tempPath = Path.GetTempFileName();
                client.DownloadFile(updateJson.latest_build.windows.download, tempPath);
                SevenZipBase.Path7za = zipLib.FullName;
                SevenZipExtractor extractor = new SevenZipExtractor(tempPath);
                extractor.ExtractAll(runningLocation.Parent.FullName, true);
                File.WriteAllText(index.FullName, rawJson);
                Console.WriteLine("Done!");
            }
        }
    }
}
