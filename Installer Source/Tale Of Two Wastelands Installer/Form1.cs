using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace Tale_Of_Two_Wastelands_Installer
{
    public partial class frm_Main : Form
    {
        string dirTemp;
        string dirAssets = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\resources\\";
        string dirFO3;
        string dirFNV;
        string dirTTW;
        string dirTTWMain;
        string dirTTWOptional;
        string sysArch;
        StreamWriter logFile;
        StreamReader readFile;
        Dictionary<string, string> CheckSums = new Dictionary<string,string>();
        RegistryKey bethKey, f03Key, fnvKey, ttwKey;
        DialogResult dlgResult;

        public frm_Main()
        {
            InitializeComponent();
        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            //Create TTW log directory
            if (!Directory.Exists(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\my games\\TaleOfTwoWastelands"))
            {
                Directory.CreateDirectory(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\my games\\TaleOfTwoWastelands");
            } 

            //Create and open TTW log file
            File.WriteAllText(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\my games\\TaleOfTwoWastelands\\Install Log " + System.DateTime.Now.ToString("MM_dd_yyyy - HH_mm_ss") + ".txt", "");
            logFile = File.AppendText(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\my games\\TaleOfTwoWastelands\\Install Log " + System.DateTime.Now.ToString("MM_dd_yyyy - HH_mm_ss") + ".txt");
            logFile.AutoFlush = true;

            //read processor type
            sysArch = Registry.GetValue("HKey_Local_Machine\\Hardware\\Description\\System\\CentralProcessor\\0\\", "Identifier", "").ToString();
            WriteLog("System Architecture: " + sysArch);

            //determine software reg path (depends on architecture) 
            if (sysArch.Contains("86")) //32-bit
            {
                sysArch = "32";
                WriteLog("\t32-bit architecture found.");
                bethKey = Registry.LocalMachine.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            else if (sysArch.Contains("64")) //64-bit
            {
                sysArch = "64";
                WriteLog("\t64-bit architecture found.");
                bethKey = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            else //Unknown -> Exit
            {
                MessageBox.Show("Could not determine your system architecture, closing for now, please report this bug.\r\n" + sysArch);
                WriteLog("\tCould not determine system architecture.");
                this.Close();
            }

            //create or retrieve BethSoft path
            bethKey = bethKey.CreateSubKey("Bethesda Softworks", RegistryKeyPermissionCheck.ReadWriteSubTree);

            //create or retrieve FO3 path
            f03Key = bethKey.CreateSubKey("Fallout3");
            dirFO3 = f03Key.GetValue("Installed Path", "").ToString();
            txt_FO3Location.Text = dirFO3;

            //create or retrieve FNV path
            fnvKey = bethKey.CreateSubKey("FalloutNV");
            dirFNV = fnvKey.GetValue("Installed Path", "").ToString();
            txt_FNVLocation.Text = dirFNV;

            //create or retrieve TTW path
            ttwKey = bethKey.CreateSubKey("TaleOfTwoWastelands");
            dirTTW = ttwKey.GetValue("Installed Path", "").ToString();
            txt_TTWLocation.Text = dirTTW;

            CheckSums = BuildChecksumDictionary(dirAssets + "\\TTW Data\\TTW Patches\\TTW_Checksums.txt");
        }
        
        private void frm_Main_Shown(object sender, EventArgs e)
        {
            //bgw_Install.ReportProgress(0,"NMM: " + Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\NexusModManager\\FalloutNV","Mods",""));
            WriteLog("Looking for Fallout3.exe");
            if (File.Exists(dirFO3 + "\\Fallout3.exe"))
            {
                txt_FO3Location.Text = dirFO3;
                WriteLog("\tFound.");
            }
            else
            {
                WriteLog("\tNot found, prompting user.");
                dlg_FindGame.FilterIndex = 1;
                dlg_FindGame.Title = "Fallout 3";
                MessageBox.Show("Could not automatically find Fallout 3 location, please manually indicate its location.");
                do
                {
                    dlgResult = dlg_FindGame.ShowDialog();
                    if (dlgResult == DialogResult.OK)
                    {
                        dirFO3 = Path.GetDirectoryName(dlg_FindGame.FileName) + "\\";
                        txt_FO3Location.Text = dirFO3;
                        WriteLog("User selected: " + dirFO3);
                        f03Key.SetValue("Installed Path", dirFO3, RegistryValueKind.String);
                    }
                    else
                    {
                        if (MessageBox.Show("You cannot continue without indicating the location of Fallout3.exe.", "Error", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            break;
                        }
                    }
                }
                while (dlgResult != DialogResult.OK);
            }

            WriteLog("Looking for FalloutNV.exe");
            if (File.Exists(dirFNV + "\\FalloutNV.exe"))
            {
                txt_FNVLocation.Text = dirFNV;
                WriteLog("\tFound.");
            }
            else
            {
                WriteLog("\tNot found, prompting user.");
                dlg_FindGame.FilterIndex = 2;
                dlg_FindGame.Title = "Fallout New Vegas";
                MessageBox.Show("Could not automatically find Fallout New Vegas location, please manually indicate its location.");
                do
                {
                    dlgResult = dlg_FindGame.ShowDialog();
                    if (dlgResult == DialogResult.OK)
                    {
                        dirFNV = Path.GetDirectoryName(dlg_FindGame.FileName) + "\\";
                        txt_FNVLocation.Text = dirFNV;
                        WriteLog("User selected: " + dirFNV);
                        fnvKey.SetValue("Installed Path", dirFNV, RegistryValueKind.String);
                    }
                    else
                    {
                        if (MessageBox.Show("You cannot continue without indicating the location of FalloutNV.exe.", "Error", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            break;
                        }
                    }
                }
                while (dlgResult != DialogResult.OK);
            }

            WriteLog("Looking for Tale of Two Wastelands");
            if (dirTTW != null && dirTTW != "\\")
            {
                WriteLog("\tDefault path found.");
            }
            else
            {
                WriteLog("\tNo path indicated, prompting user.");
                MessageBox.Show("Please indicate where you would like the Tale Of Two Wastelands FOMOD to be saved.");
                do
                {
                    dlgResult = dlg_SaveTTW.ShowDialog();
                    this.Focus();
                    if (dlgResult == DialogResult.OK)
                    {
                        dirTTW = Path.GetDirectoryName(dlg_SaveTTW.FileName) + "\\";
                        txt_TTWLocation.Text = dirTTW;
                        WriteLog("User selected: " + dirTTW);
                        ttwKey.SetValue("Installed Path", dirTTW, RegistryValueKind.String);
                    }
                    else
                    {
                        if (MessageBox.Show("You cannot continue without an install location for Tale of Two Wastelands.", "Error", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            break;
                        }
                    }
                }
                while (dlgResult != DialogResult.OK);
            }
        }

        private void btn_FO3Browse_Click(object sender, EventArgs e)
        {
            dlg_FindGame.FilterIndex = 1;
            dlg_FindGame.Title = "Fallout 3";
            dlgResult = dlg_FindGame.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                dirFO3 = Path.GetDirectoryName(dlg_FindGame.FileName) + "\\";
                txt_FO3Location.Text = dirFO3;
                WriteLog("User manually changed Fallout 3 directory to: " + dirFO3);
            }
        }

        private void btn_FNVBrowse_Click(object sender, EventArgs e)
        {
            dlg_FindGame.FilterIndex = 2;
            dlg_FindGame.Title = "Fallout New Vegas";
            dlgResult = dlg_FindGame.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                dirFNV = Path.GetDirectoryName(dlg_FindGame.FileName) + "\\";
                txt_FNVLocation.Text = dirFNV;
                WriteLog("User manually changed Fallout New Vegas directory to: " + dirFNV);
            }
        }

        private void btn_TTWBrowse_Click(object sender, EventArgs e)
        {
            dlgResult = dlg_SaveTTW.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                dirTTW = Path.GetDirectoryName(dlg_SaveTTW.FileName) + "\\";
                txt_TTWLocation.Text = dirTTW;
                WriteLog("User manually changed Tale of Two Wastelands directory to: " + dirTTW);
                ttwKey.SetValue("Installed Path", dirTTW, RegistryValueKind.String);
            }
        }

        private void btn_Install_Click(object sender, EventArgs e)
        {
            btn_Install.Enabled = false;
            bgw_Install.RunWorkerAsync();
        }

        private void bgw_Install_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                dirTemp = dirTTW + "TempExtract\\";
                dirTTWMain = dirTTW + "Main Files\\";
                dirTTWOptional = dirTTW + "Optional Files\\";

                DialogResult buildResult;

                if (CheckFiles())
                {
                    WriteLog("All files found.");
                    bgw_Install.ReportProgress(0, "All files found. Proceeding with installation.");
                }
                else
                {
                    WriteLog("Missing files detected. Aborting install.");
                    bgw_Install.ReportProgress(0, "The above files were not found. Make sure your Fallout 3 location is accurate and try again.\nInstallation failed.");
                    return;
                }

                WriteLog("Creating FOMOD foundation.");
                CopyFolder(dirAssets + "TTW Data\\TTW Files\\", dirTTW);

                do
                {
                    buildResult = BuildBSA("Fallout - Meshes", "Fallout3 - Meshes");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Fallout - Misc", "Fallout3 - Misc");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Fallout - Sound", "Fallout3 - Sound");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Fallout - Textures", "Fallout3 - Textures");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Fallout - MenuVoices", "Fallout3 - MenuVoices");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Fallout - Voices", "Fallout3 - Voices");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Anchorage - Main", "Anchorage - Main");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Anchorage - Sounds", "Anchorage - Sounds");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("ThePitt - Main", "ThePitt - Main");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("ThePitt - Sounds", "ThePitt - Sounds");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("BrokenSteel - Main", "BrokenSteel - Main");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("BrokenSteel - Sounds", "BrokenSteel - Sounds");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("PointLookout - Main", "PointLookout - Main");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("PointLookout - Sounds", "PointLookout - Sounds");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Zeta - Main", "Zeta - Main");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                do
                {
                    buildResult = BuildBSA("Zeta - Sounds", "Zeta - Sounds");
                }
                while (buildResult == System.Windows.Forms.DialogResult.Retry);
                if (buildResult == System.Windows.Forms.DialogResult.Abort)
                    return;

                if (!File.Exists(dirTTWOptional + "Fallout3 Sound Effects\\TaleOfTwoWastelands - SFX.bsa"))
                {

                    bgw_Install.ReportProgress(0, "Building optional TaleOfTwoWastelands - SFX.bsa...");

                    BSAOpt.ExtractBSA(dirFO3 + "Data\\Fallout - Sound.bsa\\sound\\songs\\", dirTTWMain + "sound\\songs\\", sysArch);

                    BSAOpt.ExtractBSA(dirFO3 + "Data\\Fallout - Sound.bsa\\sound\\fx\\ui\\", dirTemp + "sound\\fx\\ui\\", sysArch);

                    using (StreamReader copyFile = new StreamReader(dirAssets + "TTW Data\\TTW_SFXCopy.txt"))
                    {
                        string line;

                        WriteLog("Building TaleOfTwoWastelands - SFX.bsa.");
                        do
                        {
                            line = copyFile.ReadLine();
                            if (!Directory.Exists(Path.GetDirectoryName(dirTemp + "SFXBSA\\" + line)))
                                Directory.CreateDirectory(Path.GetDirectoryName(dirTemp + "SFXBSA\\" + line));
                            File.Move(dirTemp + line, dirTemp + "SFXBSA\\" + line);
                        }
                        while (!copyFile.EndOfStream);

                        BSAOpt.BuildBSA(dirTemp + "SFXBSA", dirTTWOptional + "Fallout3 Sound Effects\\TaleOfTwoWastelands - SFX.bsa", sysArch);

                        Directory.Delete(dirTemp, true);
                        Directory.CreateDirectory(dirTemp);
                        bgw_Install.ReportProgress(0, "Done\n");
                    }

                }

                if (!File.Exists(dirTTWOptional + "Fallout3 Player Voice\\TaleOfTwoWastelands - PlayerVoice.bsa"))
                {

                    BSAOpt.ExtractBSA(dirFO3 + "Data\\Fallout - Voices.bsa\\sound\\voice\\fallout3.esm\\playervoicemale\\", dirTemp + "PlayerVoice\\sound\\voice\\falloutnv.esm\\playervoicemale\\", sysArch);
                    BSAOpt.ExtractBSA(dirFO3 + "Data\\Fallout - Voices.bsa\\sound\\voice\\fallout3.esm\\playervoicefemale\\", dirTemp + "PlayerVoice\\sound\\voice\\falloutnv.esm\\playervoicefemale\\", sysArch);

                    BSAOpt.BuildBSA(dirTemp + "PlayerVoice\\", dirTTWOptional + "Fallout3 Player Voice\\TaleOfTwoWastelands - PlayerVoice.bsa", sysArch);

                }

                if (Directory.Exists(dirTemp))
                    Directory.Delete(dirTemp, true);

                if (!File.Exists(dirTTWMain + "TaleOfTwoWastelands.bsa"))
                    File.Copy(dirAssets + "TTW Data\\TaleOfTwoWastelands.bsa", dirTTWMain + "TaleOfTwoWastelands.bsa");

                if (!PatchFile("Fallout3.esm"))
                    return;
                if (!PatchFile("Anchorage.esm"))
                    return;
                if (!PatchFile("ThePitt.esm"))
                    return;
                if (!PatchFile("BrokenSteel.esm"))
                    return;
                if (!PatchFile("PointLookout.esm"))
                    return;
                if (!PatchFile("Zeta.esm"))
                    return;

                WriteLog("Copying Fallout3 Music");
                bgw_Install.ReportProgress(0, "Copying Fallout3 music files...");
                using (StreamReader copyFile = new StreamReader(dirAssets + "TTW Data\\FO3_MusicCopy.txt"))
                {
                    string line;
                    do
                    {
                        line = copyFile.ReadLine();
                        Directory.CreateDirectory(Path.GetDirectoryName(dirTTWMain + line));
                        if (File.Exists(dirFO3 + "Data\\" + line))
                            try
                            {
                                File.Copy(dirFO3 + "Data\\" + line, dirTTWMain + line, true);
                            }
                            catch (System.UnauthorizedAccessException error)
                            {
                                WriteLog("ERROR: " + line + " did not copy successfully due to: Unauthorized Access Exception " + error.Source + ".");
                            }
                        else
                            WriteLog("File Not Found:\t" + dirFO3 + "Data\\" + line);
                    }
                    while (!copyFile.EndOfStream);
                }
                WriteLog("Done.");
                bgw_Install.ReportProgress(0, "Done.");

                WriteLog("Copying Fallout3 Video");
                bgw_Install.ReportProgress(0, "Copying Fallout3 video files...");
                using (StreamReader copyFile = new StreamReader(dirAssets + "TTW Data\\FO3_VideoCopy.txt"))
                {
                    string line;
                    do
                    {
                        line = copyFile.ReadLine();
                        Directory.CreateDirectory(Path.GetDirectoryName(dirTTWMain + line));
                        if (File.Exists(dirFO3 + "Data\\" + line))
                            try
                            {
                                File.Copy(dirFO3 + "Data\\" + line, dirTTWMain + line, true);
                            }
                            catch (System.UnauthorizedAccessException error)
                            {
                                WriteLog("ERROR: " + line + " did not copy successfully due to: Unauthorized Access Exception " + error.Source + ".");
                            }
                        else
                            WriteLog("File Not Found:\t" + dirFO3 + "Data\\" + line);
                    }
                    while (!copyFile.EndOfStream);
                }
                WriteLog("Done.");
                bgw_Install.ReportProgress(0, "Done.");

                if (MessageBox.Show("Tale of Two Wastelands is easiest to install via a mod manager (such as Nexus Mod Manager). Manual installation is possible but not suggested.\n\nWould like the installer to automatically build FOMODs?", "Build FOMODs?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    WriteLog("Building FOMODs.");
                    bgw_Install.ReportProgress(0, "Building FOMODs...\n\tThis can take some time.");
                    BuildFOMOD(dirTTWMain, dirTTW + "TaleOfTwoWastelands_Main.fomod");
                    BuildFOMOD(dirTTWOptional, dirTTW + "TaleOfTwoWastelands_Options.fomod");
                    WriteLog("Done.");
                    bgw_Install.ReportProgress(0, "FOMODs built.");
                }

                bgw_Install.ReportProgress(0, "Install completed successfully.");
                MessageBox.Show("Tale of Two Wastelands has been installed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unhandled exception has occurred:\n" + ex.Message, "Exception");
                WriteLog(ex.Message);
                bgw_Install.ReportProgress(0, ex.Message);
            }
        }

        private void bgw_Install_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            txt_Progress.AppendText("[" + System.DateTime.Now.ToString() + "]\t" + e.UserState.ToString() + "\n");
        }

        private void bgw_Install_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btn_Install.Enabled = true;
        }

        public void WriteLog(string log)
        {
            logFile.WriteLine("[" + System.DateTime.Now.ToString() + "]\t" + log);
        }
        
        public string GetChecksum(string fileIn)
        {
            MD5 fileHash = MD5.Create();
            using (FileStream checkFile = new FileStream(fileIn, FileMode.Open))
            {
                string hash = BitConverter.ToString(fileHash.ComputeHash(checkFile)).Replace("-", "");
                return hash;
            }
        }

        public Dictionary<string, string> BuildChecksumDictionary(string listFile)
        {
            string[] s;
            Dictionary<string, string> dictList = new Dictionary<string, string>();
            WriteLog("Building checksum dictionary using " + listFile);
            readFile = new StreamReader(listFile);

            do
            {
                s = readFile.ReadLine().Split(',');
                if (s.Count<String>() > 2)
                {
                    WriteLog("Invalid checksum data found: \n\r" + s);
                    continue;
                }

                WriteLog("\t" + s[0] + " = " + s[1]);
                dictList.Add(s[0], s[1]);

            }
            while (!readFile.EndOfStream);
            WriteLog("Done.");
            return dictList;
        }

        public bool ApplyPatch(string inFile, string patchFile, string outFile)
        {
            System.Diagnostics.Process xDiff = new System.Diagnostics.Process();

            xDiff.StartInfo.FileName = dirAssets + "\\xdelta3_x32.exe";
            xDiff.StartInfo.Arguments = " -d -s \"" + inFile + "\" \"" + patchFile + "\" \"" + outFile;
            xDiff.Start();
            xDiff.WaitForExit();

            string hash;
            CheckSums.TryGetValue(Path.GetFileName(inFile),out hash);

            if (GetChecksum(outFile) == hash)
                return true;
            else
                return false;
        }

        public bool PatchFile(string filePatch, bool bSearchFO3 = true)
        {
            string newChecksum;
            string curChecksum;

            WriteLog("Patching " + filePatch + "...");
            bgw_Install.ReportProgress(0, "Patching " + filePatch + "...");

            if (File.Exists(dirTTWMain + filePatch))
            {
                CheckSums.TryGetValue(filePatch, out newChecksum);
                curChecksum = GetChecksum(dirTTWMain + filePatch);
                if (curChecksum == newChecksum)
                {
                    bgw_Install.ReportProgress(0, filePatch + " is up to date.");
                    WriteLog(dirTTWMain + filePatch + " is already up to date.");
                    return true;
                }
                else if (File.Exists(dirAssets + "TTW Data\\TTW Patches\\" + filePatch + "." + curChecksum + "." + newChecksum + ".diff"))
                {
                    WriteLog("\tApplying patch " + dirAssets + "TTW Data\\TTW Patches\\" + filePatch + "." + curChecksum + "." + newChecksum + ".diff");
                    if (ApplyPatch(dirTTWMain + filePatch, dirAssets + "TTW Data\\TTW Patches\\" + filePatch + "." + curChecksum + "." + newChecksum + ".diff", dirTTWMain + filePatch + ".new"))
                    {
                        File.Delete(dirTTWMain + filePatch);
                        File.Move(dirTTWMain + filePatch + ".new", dirTTWMain + filePatch);
                        WriteLog("Patch successful.");
                        bgw_Install.ReportProgress(0, "Patch successful.");
                        return true;
                    }
                    else
                        WriteLog("Patch failed.");
                }
                else
                {
                    WriteLog("No patch exists for " + dirTTWMain + filePatch + ", deleting.");
                    File.Delete(dirTTWMain + filePatch);
                }
            }
            else
                WriteLog("\t" + filePatch + " not found in " + dirTTWMain);

            if (File.Exists(dirFO3 + "Data\\" + filePatch) && bSearchFO3)
            {
                WriteLog("\tChecking " + dirFO3);

                CheckSums.TryGetValue(filePatch, out newChecksum);
                curChecksum = GetChecksum(dirFO3 + "Data\\" + filePatch);
                if (curChecksum == newChecksum)
                {
                    bgw_Install.ReportProgress(0, filePatch + " is up to date.");
                    WriteLog(dirFO3 + "Data\\" + filePatch + " is already up to date. Moving to " + dirTTWMain);
                    File.Copy(dirFO3 + "Data\\" + filePatch, dirTTWMain + filePatch);
                    return true;
                }
                else if (File.Exists(dirAssets + "TTW Data\\TTW Patches\\" + filePatch + "." + curChecksum + "." + newChecksum + ".diff"))
                {
                    WriteLog("\tApplying patch " + dirAssets + "TTW Data\\TTW Patches\\" + filePatch + "." + curChecksum + "." + newChecksum + ".diff");
                    if (ApplyPatch(dirFO3 + "Data\\" + filePatch, dirAssets + "TTW Data\\TTW Patches\\" + filePatch + "." + curChecksum + "." + newChecksum + ".diff", dirTTWMain + filePatch))
                    {
                        WriteLog("Patch successful.");
                        bgw_Install.ReportProgress(0, "Patch successful.");
                        return true;
                    }
                    else
                    {
                        WriteLog("Patch failed.");
                        bgw_Install.ReportProgress(0, "Patch failed for an unknown reason, try to install again. If this problem persists, please report it.");
                        return false;
                    }
                }
                else
                {
                    WriteLog("No patch for this version of " + dirFO3 + "Data\\" + filePatch + " Exists. Install aborted.");
                    bgw_Install.ReportProgress(0, "Your version of " + filePatch + " cannot be patched.\n" +
                        "\tCurrently Tale of Two Wastelands only works on legal, fully patched versions of Fallout3.\n" +
                        "Install aborted.");
                    return false;
                }
            }
            else if (bSearchFO3)
            {
                WriteLog(filePatch + " could not be found. Install aborted.");
                bgw_Install.ReportProgress(0, "The installer could not find " + filePatch + " in " + dirTTWMain + " or " + dirFO3 + "Data\\" +
                    "\t Make sure you have selected the proper paths.\n" +
                    "\nInstall aborted.");
                return false;
            }
            else
            {
                WriteLog(dirTTWMain + filePatch + " cannot be patched. Install aborted.");
                bgw_Install.ReportProgress(0, "Your version of " + filePatch + " cannot be patched. This is abnormal.");
                return false;
            }
        }

        public bool CheckFiles()
        {
            bool bFileCheck = true;

            WriteLog("Checking for required files.");
            bgw_Install.ReportProgress(0, "Checking for required files...");

            if (!(File.Exists(dirTTWMain + "Fallout3.esm")) && !(File.Exists(dirFO3 + "Data\\Fallout3.esm")))
            {
                WriteLog("Fallout3.esm could not be found.");
                bgw_Install.ReportProgress(0, "\tFallout3.esm could not be found.");
                bFileCheck = false;
            }

            if (!(File.Exists(dirTTWMain + "Anchorage.esm")) && !(File.Exists(dirFO3 + "Data\\Anchorage.esm")))
            {
                WriteLog("Anchorage.esm could not be found.");
                bgw_Install.ReportProgress(0, "\tAnchorage.esm could not be found.");
                bFileCheck = false;
            }

            if (!(File.Exists(dirTTWMain + "ThePitt.esm")) && !(File.Exists(dirFO3 + "Data\\ThePitt.esm")))
            {
                WriteLog("ThePitt.esm could not be found.");
                bgw_Install.ReportProgress(0, "\tThePitt.esm could not be found.");
                bFileCheck = false;
            }

            if (!(File.Exists(dirTTWMain + "BrokenSteel.esm")) && !(File.Exists(dirFO3 + "Data\\BrokenSteel.esm")))
            {
                WriteLog("BrokenSteel.esm could not be found.");
                bgw_Install.ReportProgress(0, "\tBrokenSteel.esm could not be found.");
                bFileCheck = false;
            }

            if (!(File.Exists(dirTTWMain + "PointLookout.esm")) && !(File.Exists(dirFO3 + "Data\\PointLookout.esm")))
            {
                WriteLog("PointLookout.esm could not be found.");
                bgw_Install.ReportProgress(0, "\tPointLookout.esm could not be found.");
                bFileCheck = false;
            }

            if (!(File.Exists(dirTTWMain + "Zeta.esm")) && !(File.Exists(dirFO3 + "Data\\Zeta.esm")))
            {
                WriteLog("Zeta.esm could not be found.");
                bgw_Install.ReportProgress(0, "\tZeta.esm could not be found.");
                bFileCheck = false;
            }

            if (!File.Exists(dirTTWMain + "Fallout3 - Main.bsa"))
            {
                if (!File.Exists(dirFO3 + "Data\\Fallout - Meshes.bsa"))
                {
                    WriteLog("Fallout - Meshes.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tFallout - Meshes.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\Fallout - Misc.bsa"))
                {
                    WriteLog("Fallout - Misc.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tFallout - Misc.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\Fallout - Textures.bsa"))
                {
                    WriteLog("Fallout - Textures.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tFallout - Textures.bsa could not be found.");
                    bFileCheck = false;
                }
            }

            if (!File.Exists(dirTTWMain + "Fallout3 - Sounds.bsa"))
            {
                if (!File.Exists(dirFO3 + "Data\\Fallout - MenuVoices.bsa"))
                {
                    WriteLog("Fallout - MenuVoices.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tFallout - MenuVoices.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\Fallout - Sound.bsa"))
                {
                    WriteLog("Fallout - Sound.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tFallout - Sound.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\Fallout - Voices.bsa"))
                {
                    WriteLog("Fallout - Voices.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tFallout - Voices.bsa could not be found.");
                    bFileCheck = false;
                }
            }

            if (!File.Exists(dirTTWMain + "Fallout3 - DLC.bsa"))
            {
                if (!File.Exists(dirFO3 + "Data\\Anchorage - Main.bsa"))
                {
                    WriteLog("Anchorage - Main.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tAnchorage - Main.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\Anchorage - Sounds.bsa"))
                {
                    WriteLog("Anchorage - Sounds.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tAnchorage - Sounds.bsa could not be found.");
                    bFileCheck = false;
                }

                if (!File.Exists(dirFO3 + "Data\\ThePitt - Main.bsa"))
                {
                    WriteLog("ThePitt - Main.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tThePitt - Main.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\ThePitt - Sounds.bsa"))
                {
                    WriteLog("ThePitt - Sounds.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tThePitt - Sounds.bsa could not be found.");
                    bFileCheck = false;
                }

                if (!File.Exists(dirFO3 + "Data\\BrokenSteel - Main.bsa"))
                {
                    WriteLog("BrokenSteel - Main.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tBrokenSteel - Main.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\BrokenSteel - Sounds.bsa"))
                {
                    WriteLog("BrokenSteel - Sounds.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tBrokenSteel - Sounds.bsa could not be found.");
                    bFileCheck = false;
                }

                if (!File.Exists(dirFO3 + "Data\\PointLookout - Main.bsa"))
                {
                    WriteLog("PointLookout - Main.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tPointLookout - Main.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\PointLookout - Sounds.bsa"))
                {
                    WriteLog("PointLookout - Sounds.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tPointLookout - Sounds.bsa could not be found.");
                    bFileCheck = false;
                }

                if (!File.Exists(dirFO3 + "Data\\Zeta - Main.bsa"))
                {
                    WriteLog("Zeta - Main.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tZeta - Main.bsa could not be found.");
                    bFileCheck = false;
                }
                if (!File.Exists(dirFO3 + "Data\\Zeta - Sounds.bsa"))
                {
                    WriteLog("Zeta - Sounds.bsa could not be found.");
                    bgw_Install.ReportProgress(0, "\tZeta - Sounds.bsa could not be found.");
                    bFileCheck = false;
                }
            }
            return bFileCheck;
        }

        public void DeleteList(string cleanFolder, string listFile)
        {
            WriteLog("Cleaning (" + Path.GetFileName(listFile) + ")...");
            StreamReader streamList = new StreamReader(listFile);
            string line;

            do
            {
                line = streamList.ReadLine();
                if (File.Exists(cleanFolder + line))
                {
                    File.Delete(cleanFolder + line);
                }
                else
                {
                    WriteLog("File Not Found:\t" + cleanFolder + line);
                }

            }
            while (!streamList.EndOfStream);
        }

        public void BuildFOMOD(string inDir, string outFile)
        {
            System.Diagnostics.Process zip = new System.Diagnostics.Process();

            zip.StartInfo.FileName = "\"" + dirAssets + "7Zip\\7za.exe\"";
            zip.StartInfo.Arguments = " a -mx0 -tzip \"" + outFile + "\" \"" + inDir + "\"";
            zip.StartInfo.UseShellExecute = false;
            zip.StartInfo.RedirectStandardOutput = true;
            zip.Start();
            string log = zip.StandardOutput.ReadToEnd();
            zip.Close();
        }

        public void CopyFolder(string inFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            foreach (string folder in Directory.EnumerateDirectories(inFolder, "*", SearchOption.AllDirectories).ToArray<String>())
            {
                Directory.CreateDirectory(destFolder + folder.Replace(inFolder,""));
            }
            foreach (string file in Directory.EnumerateFiles(inFolder, "*", SearchOption.AllDirectories).ToArray<string>())
            {
                try
                {
                    File.Copy(file, destFolder + file.Replace(inFolder, ""),true);
                }
                catch (System.UnauthorizedAccessException error)
                {
                    WriteLog("ERROR: " + file.Replace(inFolder, "") + " did not copy successfully due to: Unauthorized Access Exception " + error.Source + ".");
                }
            }
        }

        public DialogResult BuildBSA(string inBSA, string outBSA)
        {
            List<string> errors = new List<string>();
            string errorString = "\t";

            if (File.Exists(dirTTWMain + outBSA + ".bsa"))
            {
                switch (MessageBox.Show( outBSA + ".bsa already exists. Rebuild?", "File Already Exists", MessageBoxButtons.YesNo))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        File.Delete(dirTTWMain + outBSA + ".bsa");
                        WriteLog("Rebuilding " + outBSA);
                        bgw_Install.ReportProgress(0, "Rebuilding " + outBSA);
                        break;
                    case System.Windows.Forms.DialogResult.No:
                        WriteLog(outBSA + " has already been built. Skipping.");
                        bgw_Install.ReportProgress(0, outBSA + " has already been built. Skipping.");
                        return DialogResult.No;
                }
            }
            else
            {
                WriteLog("Building " + outBSA);
                bgw_Install.ReportProgress(0, "Building " + outBSA);
            }

            errors = BSADiff.PatchBSA(dirFO3 + "Data\\" + inBSA + ".bsa", dirTTWMain + outBSA + ".bsa", dirTemp, dirAssets + "TTW Data\\TTW Patches\\" + outBSA, sysArch);

            foreach (string error in errors)
            {
                errorString += error + "\n\t";
                WriteLog(error);
                bgw_Install.ReportProgress(0, error);
            }
            if (errors.Count > 0)
            {
                switch (MessageBox.Show("The following errors have occurred:" + errorString, "Error Log", MessageBoxButtons.AbortRetryIgnore))
                {
                    case System.Windows.Forms.DialogResult.Abort:   //Quit install
                        WriteLog("Install Aborted.");
                        bgw_Install.ReportProgress(0, "Install Aborted.");
                        return System.Windows.Forms.DialogResult.Abort;
                    case System.Windows.Forms.DialogResult.Retry:   //Start over from scratch
                        WriteLog("Retrying build.");
                        bgw_Install.ReportProgress(0, "Retrying build.");
                        return System.Windows.Forms.DialogResult.Retry;
                    case System.Windows.Forms.DialogResult.Ignore:  //Ignore errors and move on
                        WriteLog("Ignoring errors.");
                        bgw_Install.ReportProgress(0, "Ignoring errors.");
                        return System.Windows.Forms.DialogResult.Ignore;
                }
            }
            
            WriteLog("Build successful.");
            bgw_Install.ReportProgress(0, "Build successful.");
            return System.Windows.Forms.DialogResult.OK;
        }
    }
}
