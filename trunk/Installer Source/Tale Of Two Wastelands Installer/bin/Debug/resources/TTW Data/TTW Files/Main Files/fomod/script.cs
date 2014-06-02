using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using fomm.Scripting;

class Script : FalloutNewVegasBaseScript
{
    static bool bInstall = true;
	
	public static bool PluginExists(String pluginName)
	{
		string[] loadOrder = GetAllPlugins();
		for (int i = 0; i < loadOrder.Length; ++i)
		{
			if (loadOrder[i].Equals(pluginName, StringComparison.InvariantCultureIgnoreCase))
				 return true;
		}
		return false;
	}
	
	public static int GetPluginIndex(String pluginName)
	{
		string[] loadOrder = GetAllPlugins();
		for (int i = 0; i < loadOrder.Length; ++i)
		{
			if (loadOrder[i].Equals(pluginName, StringComparison.InvariantCultureIgnoreCase))
				return i;
		}
		return -1;
	}
	
	public static void PlaceAtIndex(String pluginToMove, int index)
	{	
			int[] pluginIdxArray = { GetPluginIndex(pluginToMove) };
			SetLoadOrder(pluginIdxArray, index);
	}
	
	public static void PlaceAtPlugin(String targetPlugin, String pluginToMove, int offset)
	{
		int targetPluginIndex = GetPluginIndex(targetPlugin);
		int pluginToMoveIndex = GetPluginIndex(pluginToMove);
		if (targetPluginIndex != -1 && pluginToMoveIndex != -1)
		{
			int[] pluginIdxArray = { pluginToMoveIndex };
			SetLoadOrder(pluginIdxArray, targetPluginIndex + offset);
		}
	}
	
	public static void PlaceAfterPlugin(String targetPlugin, String pluginToMove)
	{
		PlaceAtPlugin(targetPlugin, pluginToMove, 1);
	}
	
	public static void PlaceBeforePlugin(String targetPlugin, String pluginToMove)
	{
		PlaceAtPlugin(targetPlugin, pluginToMove, 0);
	}

    public static bool OnActivate()
    {		
		if (!PluginExists("FalloutNV.esm"))
		{
			MessageBox("Could not find FalloutNV.esm. Did you HONESTLY think this would work without FalloutNV.esm?");
			bInstall = false;
		}
		if (!PluginExists("DeadMoney.esm"))
		{
			MessageBox("Could not find DeadMoney.esm.");
			bInstall = false;
		}
		if (!PluginExists("HonestHearts.esm"))
		{
			MessageBox("Could not find HonestHearts.esm.");
			bInstall = false;
		}
		if (!PluginExists("OldWorldBlues.esm"))
		{
			MessageBox("Could not find OldWorldBlues.esm.");
			bInstall = false;
		}
		if (!PluginExists("LonesomeRoad.esm"))
		{
			MessageBox("Could not find LonesomeRoad.esm.");
			bInstall = false;
		}
		if (!PluginExists("GunRunnersArsenal.esm"))
		{
			MessageBox("Could not find GunRunnersArsenal.esm.");
			bInstall = false;
		}
		
		if (!bInstall)
			return bInstall;
		
		if (bInstall)
		{
			PerformBasicInstall();
			
			if (GetFalloutIniString("General", "SCharGenQuest") == "00102037")
				EditFalloutINI("General", "SCharGenQuest", "001FFFF8", true);
			else if (GetFalloutIniString("General","SCharGenQuest") != "001FFFF8")
				MessageBox("There was an error when trying to change your CharGen quest:\n\tSCharGenQuest = " + GetFalloutIniString("General","SCharGenQuest") + "\n\nThis can happen if you are using another mod that changes the start quest.");
				
			if (GetFalloutIniString("General", "sIntroMovie") == "Fallout INTRO Vsk.bik")
				EditFalloutINI("General", "sIntroMovie", "", true);
			else if (GetFalloutIniString("General","sIntroMovie") != "")
				MessageBox("There was an error when trying to change your intro movie:\n\tsIntroMovie = " + GetFalloutIniString("General","sIntroMovie") + "\n\nThis can happen if you are using another mod that changes the intro movie.");
				
			if (GetFalloutIniString("General", "bLoadFaceGenHeadEGTFiles") == "0")
				EditFalloutINI("General", "bLoadFaceGenHeadEGTFiles", "1", true);
			else if (GetFalloutIniString("General","bLoadFaceGenHeadEGTFiles") != "1")
				MessageBox("There was an error when trying to apply the whiteface bug fix:\n\tbLoadFaceGenHeadEGTFiles = " + GetFalloutIniString("General","bLoadFaceGenHeadEGTFiles") + "\n\nYou shouldn't be seeing this. This should only ever be set to 1 or 0. Honestly.\n\n:(");			

			SetPluginActivation("FalloutNV.esm", true);
			PlaceAtIndex("FalloutNV.esm",0);
			
			SetPluginActivation("DeadMoney.esm", true);
			PlaceAfterPlugin("FalloutNV.esm","DeadMoney.esm");
			
			SetPluginActivation("HonestHearts.esm", true);
			PlaceAfterPlugin("DeadMoney.esm","HonestHearts.esm");
			
			SetPluginActivation("OldWorldBlues.esm", true);
			PlaceAfterPlugin("HonestHearts.esm","OldWorldBlues.esm");
			
			SetPluginActivation("LonesomeRoad.esm", true);
			PlaceAfterPlugin("OldWorldBlues.esm","LonesomeRoad.esm");
			
			SetPluginActivation("GunRunnersArsenal.esm", true);
			PlaceAfterPlugin("LonesomeRoad.esm","GunRunnersArsenal.esm");
			
			SetPluginActivation("Fallout3.esm", true);
			PlaceAfterPlugin("GunRunnersArsenal.esm","Fallout3.esm");
			
			SetPluginActivation("Anchorage.esm", true);
			PlaceAfterPlugin("Fallout3.esm","Anchorage.esm");
			
			SetPluginActivation("ThePitt.esm", true);
			PlaceAfterPlugin("Anchorage.esm","ThePitt.esm");
			
			SetPluginActivation("BrokenSteel.esm", true);
			PlaceAfterPlugin("ThePitt.esm","BrokenSteel.esm");
			
			SetPluginActivation("PointLookout.esm", true);
			PlaceAfterPlugin("BrokenSteel.esm","PointLookout.esm");
			
			SetPluginActivation("Zeta.esm", true);
			PlaceAfterPlugin("PointLookout.esm","Zeta.esm");
			
			SetPluginActivation("TaleOfTwoWastelands.esm", true);
			PlaceAfterPlugin("Zeta.esm","TaleOfTwoWastelands.esm");
			
			if (PluginExists("CaravanPack.esm"))
				PlaceBeforePlugin("Fallout3.esm","CaravanPack.esm");
			
			if (PluginExists("ClassicPack.esm"))
				PlaceBeforePlugin("Fallout3.esm","ClassicPack.esm");
			
			if (PluginExists("MercenaryPack.esm"))
				PlaceBeforePlugin("Fallout3.esm","MercenaryPack.esm");
			
			if (PluginExists("TribalPack.esm"))
				PlaceBeforePlugin("Fallout3.esm","TribalPack.esm");
		}

        return bInstall;
    }
}