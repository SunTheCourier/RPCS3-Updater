using Newtonsoft.Json;
using SevenZipNET;
using System;
using System.Diagnostics;
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
                MessageBox.Show($"7za.exe does not exist!", "RPCS-Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (WebClient client = new WebClient())
            {
                string rawJson = client.DownloadString("https://update.rpcs3.net/?api=v1&c=");
                RpcsJson updateJson = JsonConvert.DeserializeObject<RpcsJson>(rawJson);
                if (updateJson.return_code != 0)
                {
                    MessageBox.Show($"API returned non-zero return code: {updateJson.return_code}", "RPCS-Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                FileInfo index = runningLocation.Parent.GetFile("index.json");
                string updateInfo = $"Installing RPCS3 version: {updateJson.latest_build.version}, commit: {updateJson.latest_build.windows.GetCommit()}, PR: #{updateJson.latest_build.pr}";
                if (!index.Exists)
                {
                    if (MessageBox.Show("Would you like to download the latest RPCS3 version to the parent directory of where this application is located? (selecting no will assume that you have downloaded the latest version already!)", "RPCS-Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
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
                    updateInfo += $", previously installed version: {indexJson.latest_build.version}, Commit: {indexJson.latest_build.windows.GetCommit()}, PR: #{indexJson.latest_build.pr}";
                }
                updateInfo += "...";
                Console.WriteLine(updateInfo);
                string tempPath = Path.GetTempFileName();
                client.DownloadFile(updateJson.latest_build.windows.download, tempPath);
                SevenZipBase.Path7za = zipLib.FullName;
                SevenZipExtractor extractor = new SevenZipExtractor(tempPath);
                extractor.ExtractAll(runningLocation.Parent.FullName, true);
                File.WriteAllText(index.FullName, rawJson);
                if (MessageBox.Show("RPCS3 has been installed, would you like to launch it?", "RPCS-Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = runningLocation.Parent.GetFile("rpcs3.exe").FullName;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.WorkingDirectory = runningLocation.Parent.FullName;
                        process.Start();
                    }
                }
            }
        }
    }
}
