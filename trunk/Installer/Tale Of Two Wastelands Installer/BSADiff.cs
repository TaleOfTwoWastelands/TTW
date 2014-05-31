using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Tale_Of_Two_Wastelands_Installer
{
    class BSADiff
    {
        public static void CompareBSAs(string oldBSA, string newBSA, string oldDir, string newDir, string difDir, string sysArch = "32")
        {
            string oldChk;
            string newChk;

            string oldFile;
            string newFile;

            string tempChecksum;
            string patchFile;

            SortedDictionary<string, string> oldChkDict = new SortedDictionary<string, string>();
            SortedDictionary<string, string> newChkDict = new SortedDictionary<string, string>();

            SortedDictionary<string, string> oldFileDict = new SortedDictionary<string, string>();
            SortedDictionary<string, string> newFileDict = new SortedDictionary<string, string>();

            SortedDictionary<string, string> renameDict = new SortedDictionary<string, string>();

            BSAOpt.ExtractBSA(oldBSA, oldDir, sysArch);
            BSAOpt.ExtractBSA(newBSA, newDir, sysArch);

            foreach (string folder in Directory.EnumerateDirectories(oldDir, "*", SearchOption.AllDirectories))
            {
                foreach (string file in Directory.EnumerateFiles(folder))
                {
                    tempChecksum = GetChecksum(file);
                    oldChkDict.Add(file.Replace(oldDir, ""), tempChecksum);
                    try
                    {
                        oldFileDict.Add(tempChecksum, file.Replace(oldDir, ""));
                    }
                    catch (System.ArgumentException ex)
                    {
                        //do nothing
                    }
                }
            }
            foreach (string folder in Directory.EnumerateDirectories(newDir, "*", SearchOption.AllDirectories))
            {
                foreach (string file in Directory.EnumerateFiles(folder))
                {
                    tempChecksum = GetChecksum(file);
                    newChkDict.Add(file.Replace(newDir, ""), GetChecksum(file));
                    try
                    {
                        newFileDict.Add(tempChecksum, file.Replace(newDir, ""));
                    }
                    catch (System.ArgumentException ex)
                    {
                        //do nothing
                    }
                }
            }

            foreach (KeyValuePair<string, string> entry in newChkDict)
            {
                newFile = entry.Key;
                newChk = entry.Value;
                if (oldChkDict.TryGetValue(newFile, out oldChk))
                {
                    //the file existed in the old BSA
                    if (newChk == oldChk)
                    {
                        //the files have not changed
                        continue;
                    }
                    else
                    {
                        //the files have changed
                        patchFile = difDir + newFile + "." + oldChk + "." + newChk + ".diff";
                        Directory.CreateDirectory(Path.GetDirectoryName(patchFile));
                        using (FileStream output = new FileStream(patchFile, FileMode.Create))
                            BinaryPatchUtility.Create(File.ReadAllBytes(oldDir + newFile), File.ReadAllBytes(newDir + newFile), output);
                    }
                }
                else if (oldFileDict.TryGetValue(newChk, out oldFile))
                {
                    //the file did exist, but was under a different name
                    renameDict.Add(newFile, oldFile);
                }
                else
                {
                    //the file is brand spanking new and should probably go in TTW.bsa
                    MessageBox.Show(newFile + " is a new file and a method of generating it cannot be automatically created. Consider putting it in TTW.bsa.");
                }
            }

            if (renameDict.Count > 0)
            {
                using (Stream stream = File.Open(difDir + "RenameFiles.dict", FileMode.Create))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(stream, renameDict);
                }
            }
            if (newChkDict.Count > 0)
            {
                using (Stream stream = File.Open(difDir + "CheckSums.dict", FileMode.Create))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(stream, newChkDict);
                }
            }
        }

        public static List<string> PatchBSA(string oldBSA, string newBSA, string BSADir, string patchDir, string sysArch = "32")
        {
            List<string> errorLog = new List<string>();

            SortedDictionary<string, string> renameDict = new SortedDictionary<string, string>();
            SortedDictionary<string, string> newChkDict = new SortedDictionary<string, string>();
            SortedDictionary<string, string> oldChkDict = new SortedDictionary<string, string>();

            BSAOpt.ExtractBSA(oldBSA, BSADir, sysArch);

            if (File.Exists(patchDir + "\\RenameFiles.dict"))
            {
                using (FileStream stream = new FileStream(patchDir + "\\RenameFiles.dict", FileMode.Open))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    renameDict = (SortedDictionary<string, string>)bFormatter.Deserialize(stream);
                }
            }
            if (File.Exists(patchDir + "\\CheckSums.dict"))
            {
                using (FileStream stream = new FileStream(patchDir + "\\CheckSums.dict", FileMode.Open))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    newChkDict = (SortedDictionary<string, string>)bFormatter.Deserialize(stream);
                }
            }
            else
            {
                errorLog.Add("No Checksum dictionary is available for: " + oldBSA);
                return errorLog;
            }

            foreach (KeyValuePair<string, string> entry in renameDict)
            {
                string oldFile = entry.Value;
                string newFile = entry.Key;

                if (File.Exists(BSADir + oldFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(BSADir + newFile));
                    File.Copy(BSADir + oldFile, BSADir + newFile);
                }
                else
                {
                    errorLog.Add("File not found: " + oldFile);
                    errorLog.Add("\tCannot create: " + newFile);
                }
            }

            foreach (string folder in Directory.EnumerateDirectories(BSADir, "*", SearchOption.AllDirectories))
            {
                foreach (string file in Directory.EnumerateFiles(folder))
                {
                    oldChkDict.Add(file.Replace(BSADir, ""), GetChecksum(file));
                }
            }

            foreach (KeyValuePair<string, string> entry in newChkDict)
            {
                string oldChk;
                string newChk = entry.Value;
                string file = entry.Key;

                if (oldChkDict.TryGetValue(file, out oldChk))
                {
                    //file exists
                    if (oldChk == newChk)
                    {
                        //file is up to date
                        continue;
                    }
                    else
                    {
                        //file exists but is not up to date
                        if (File.Exists(patchDir + "\\" + file + "." + oldChk + "." + newChk + ".diff"))
                        {
                            //a patch exists for the file
                            using (FileStream input = new FileStream(BSADir + file, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (FileStream output = new FileStream(BSADir + file + ".tmp", FileMode.Create))
                                BinaryPatchUtility.Apply(input, () => new FileStream(patchDir + "\\" + file + "." + oldChk + "." + newChk + ".diff", FileMode.Open, FileAccess.Read, FileShare.Read), output);

                            oldChk = GetChecksum(BSADir + file + ".tmp");

                            File.Delete(BSADir + file);
                            File.Move(BSADir + file + ".tmp", BSADir + file);
                        }
                        else
                        {
                            //no patch exists for the file
                            errorLog.Add("File is of an unexpected version: " + file + " - " + oldChk);
                            errorLog.Add("\tThis file cannot be patched. Errors may occur.");
                        }
                    }
                }
                else
                {
                    //file not found
                    errorLog.Add("File not found: " + file);
                }
            }

            foreach (KeyValuePair<string, string> entry in oldChkDict)
            {
                string oldChk = entry.Value;
                string newChk;
                string file = entry.Key;

                if (!newChkDict.TryGetValue(file, out newChk))
                {
                    File.Delete(BSADir + file);
                }
            }

            BSAOpt.BuildBSA(BSADir, newBSA, sysArch);

            return errorLog;
        }

        public static string GetChecksum(string fileIn)
        {
            MD5 fileHash = MD5.Create();
            using (FileStream checkFile = new FileStream(fileIn, FileMode.Open))
            {
                string hash = BitConverter.ToString(fileHash.ComputeHash(checkFile)).Replace("-", "");
                return hash;
            }
        }
    }
}