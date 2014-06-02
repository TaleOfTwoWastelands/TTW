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
    string currentOption;
    static bool bInstall = true;

    static Form frm_Install;
    static TableLayoutPanel tableLayoutPanel1;
    static TableLayoutPanel tableLayoutPanel2;
    static PictureBox pic_Optional;
    static RichTextBox txt_Optional;
    static CheckedListBox lst_Optional;
    static Button btn_Install;

    static void InitializeComponents()
    {
        tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
        pic_Optional = new System.Windows.Forms.PictureBox();
        txt_Optional = new System.Windows.Forms.RichTextBox();
        lst_Optional = new System.Windows.Forms.CheckedListBox();
        btn_Install = new System.Windows.Forms.Button();
        frm_Install = new Form();

        frm_Install = CreateCustomForm();

        tableLayoutPanel1.SuspendLayout();
        tableLayoutPanel2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(pic_Optional)).BeginInit();
        frm_Install.SuspendLayout();
        // 
        // tableLayoutPanel1
        // 
        tableLayoutPanel1.AutoSize = true;
        tableLayoutPanel1.ColumnCount = 2;
        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 0);
        tableLayoutPanel1.Controls.Add(lst_Optional, 0, 0);
        tableLayoutPanel1.Controls.Add(btn_Install, 0, 1);
        tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 2;
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        tableLayoutPanel1.Size = new System.Drawing.Size(784, 562);
        tableLayoutPanel1.TabIndex = 0;
        // 
        // tableLayoutPanel2
        // 
        tableLayoutPanel2.AutoSize = true;
        tableLayoutPanel2.ColumnCount = 1;
        tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 543F));
        tableLayoutPanel2.Controls.Add(pic_Optional, 0, 0);
        tableLayoutPanel2.Controls.Add(txt_Optional, 0, 1);
        tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanel2.Location = new System.Drawing.Point(243, 3);
        tableLayoutPanel2.Name = "tableLayoutPanel2";
        tableLayoutPanel2.RowCount = 2;
        tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 138F));
        tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
        tableLayoutPanel2.Size = new System.Drawing.Size(538, 516);
        tableLayoutPanel2.TabIndex = 1;
        // 
        // pic_Optional
        // 
        pic_Optional.Dock = System.Windows.Forms.DockStyle.Fill;
        pic_Optional.ImageLocation = "";
        pic_Optional.Location = new System.Drawing.Point(3, 3);
        pic_Optional.Name = "pic_Optional";
        pic_Optional.Size = new System.Drawing.Size(537, 132);
        pic_Optional.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        pic_Optional.TabIndex = 0;
        pic_Optional.TabStop = false;
        // 
        // txt_Optional
        // 
        txt_Optional.Dock = System.Windows.Forms.DockStyle.Fill;
        txt_Optional.Location = new System.Drawing.Point(3, 141);
        txt_Optional.Name = "txt_Optional";
        txt_Optional.ReadOnly = true;
        txt_Optional.Size = new System.Drawing.Size(537, 415);
        txt_Optional.TabIndex = 1;
        txt_Optional.Text = "";
        // 
        // lst_Optional
        // 
        lst_Optional.Dock = System.Windows.Forms.DockStyle.Fill;
        lst_Optional.FormattingEnabled = true;
        lst_Optional.Location = new System.Drawing.Point(3, 3);
        lst_Optional.Name = "lst_Optional";
        lst_Optional.Size = new System.Drawing.Size(234, 516);
        lst_Optional.TabIndex = 2;
        lst_Optional.ThreeDCheckBoxes = true;
        // 
        // btn_Install
        // 
        btn_Install.Anchor = System.Windows.Forms.AnchorStyles.None;
        tableLayoutPanel1.SetColumnSpan(btn_Install, 2);
        btn_Install.Location = new System.Drawing.Point(354, 530);
        btn_Install.Name = "btn_Install";
        btn_Install.Size = new System.Drawing.Size(75, 23);
        btn_Install.TabIndex = 3;
        btn_Install.Text = "Install";
        btn_Install.UseVisualStyleBackColor = true;
        // 
        // frm_Install
        // 
        frm_Install.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        frm_Install.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        frm_Install.ClientSize = new System.Drawing.Size(784, 562);
        frm_Install.Controls.Add(tableLayoutPanel1);
        frm_Install.Name = "frm_Install";
        frm_Install.Text = "Tale of Two Wastelands";
        tableLayoutPanel1.ResumeLayout(false);
        tableLayoutPanel1.PerformLayout();
        tableLayoutPanel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(pic_Optional)).EndInit();
        frm_Install.ResumeLayout(false);
        frm_Install.PerformLayout();

        lst_Optional.SelectedIndexChanged += delegate(object sender, EventArgs e)
        {
            string currentOption = lst_Optional.SelectedItem.ToString();
            pic_Optional.Image = GetImageFromFomod("Optional Files/" + currentOption + "/Image.bmp");
            txt_Optional.Lines = GetStringsFromFile("Optional Files/" + currentOption + "/Description.txt");
        };
        btn_Install.Click += delegate(object sender, EventArgs e)
        {
			bInstall = true;	
			frm_Install.Close();
        };
    }
	
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


    static string[] GetStringsFromFile(string source)
    {
        byte[] data = GetFileFromFomod(source);

        if (data == null)
            return null;

        Stream listStream = new MemoryStream(data);
        if (listStream == null)
            return null;

        TextReader tr = new StreamReader(listStream);

        List<string> fileList = new List<string>();
        string line;
        while ((line = tr.ReadLine()) != null)
        {
            fileList.Add(line);
        }
        tr.Close();

        return fileList.ToArray();
    }

    static Image GetImageFromFomod(string filename)
    {
        byte[] data = GetFileFromFomod(filename);

        if (data == null)
            return null;

        MemoryStream stream = new MemoryStream(data);
        Image image = Image.FromStream(stream);
        stream.Close();

        return image;
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
        InitializeComponents();
		
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
		if (!PluginExists("TaleOfTwoWastelands.esm"))
		{
			MessageBox("The main TTW FOMOD must be installed first.");
			bInstall = false;
		}
		
		if (!bInstall)
			return bInstall;
			
		bInstall = false;

        string[] optionalFiles = GetStringsFromFile("FolderList.txt");
        foreach (string folder in optionalFiles)
        {
            lst_Optional.Items.Add(folder);
        }

        frm_Install.ShowDialog();
		
		if (bInstall)
		{
			foreach(object itemChecked in lst_Optional.CheckedItems)
            {	int offset = 1;
                foreach (string sFile in GetStringsFromFile(itemChecked.ToString() + "/FileList.txt"))
                {
                    CopyDataFile(itemChecked.ToString() + "/" + sFile, sFile);
					if (sFile.ToLower().IndexOf(".es") == sFile.Length - 4)
					{
						PlaceAtPlugin("TaleOfTwoWastelands.esm",sFile,offset);
						offset ++;
					}
                }
            }
		}

        return bInstall;
    }
}