using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tale_Of_Two_Wastelands_Installer
{
    class BSAOpt
    {
        static private void Run(string inArg, string outArg, string sysArch)
        {
            System.Diagnostics.Process bsaOpt = new System.Diagnostics.Process();

            bsaOpt.StartInfo.FileName = "resources\\BSAOpt\\BSAOpt x" + sysArch + ".exe";
            bsaOpt.StartInfo.Arguments = " -deployment -game fo -compress 10 -criticals \"" + inArg + "\" \"" + outArg + "\"";
            bsaOpt.StartInfo.UseShellExecute = false;
            bsaOpt.StartInfo.CreateNoWindow = true;
            bsaOpt.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            bsaOpt.StartInfo.RedirectStandardOutput = true;
            bsaOpt.Start();
            string log = bsaOpt.StandardOutput.ReadToEnd();
            bsaOpt.Close();
        }

        static public void BuildBSA(string inDirectory, string outFile, string sysArch)
        {
            Run(inDirectory, outFile, sysArch);
            Directory.Delete(inDirectory, true);
            Directory.CreateDirectory(inDirectory);
        }

        static public void ExtractBSA(string inBSA, string outDir, string sysArch)
        {
            Run(inBSA, outDir, sysArch);
        }
    }
}
