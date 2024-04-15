using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Texture;
using SPICA.Formats.Generic.COLLADA;
using SPICA.Formats.Generic.StudioMdl;
using SPICA.Formats.GFLX;
using SPICA.Formats.GFLX.TR;
using SPICA.WinForms.Formats;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SPICA.WinForms
{
    public partial class FrmExport : Form
    {
        public FrmExport()
        {
            InitializeComponent();
        }

        private void FrmExport_Load(object sender, EventArgs e)
        {
            CmbFormat.SelectedIndex = 0;
        }

        private void BtnBrowseIn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog Browser = new FolderBrowserDialog())
            {
                if (Browser.ShowDialog() == DialogResult.OK) TxtInputFolder.Text = Browser.SelectedPath;
            }
        }

        private void BtnBrowseOut_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog Browser = new FolderBrowserDialog())
            {
                if (Browser.ShowDialog() == DialogResult.OK) TxtOutFolder.Text = Browser.SelectedPath;
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(TxtInputFolder.Text))
            {
                MessageBox.Show(
                    "Input folder not found!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!Directory.Exists(TxtOutFolder.Text))
            {
                MessageBox.Show(
                    "Output folder not found!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string[] Files = Directory.GetFiles(TxtInputFolder.Text) ;
            
            bool ExportModels = ChkExportModels.Checked;
            bool ExportSkeletons = ChkExportSkeletons.Checked;
            bool ExportAnims = ChkExportAnimations.Checked;
            bool ExportTexs = ChkExportTextures.Checked;
            bool PrefixNames = ChkPrefixNames.Checked;

            int Format = CmbFormat.SelectedIndex;

            int FileLen = Files.Length;

            BtnConvert.Enabled = false;
            
            for (int i = 1; i < FileLen; i += 9) {
                
                string BaseName = TxtOutFolder.Text + Path.DirectorySeparatorChar;
                var filename = Path.Combine(TxtInputFolder.Text, $"dec_{i.ToString("00000")}.bin");

                H3D Data = FormatIdentifier.IdentifyAndOpen(filename);
                
                if (Data == null) continue;

                BaseName += $"{i:d8}_{Data.Models[0].Name}{Path.DirectorySeparatorChar}";

                Directory.CreateDirectory(BaseName);

                for (int j = 0; j < 8; j++)
                {
                    var fileidx = i + j;
                    filename = Path.Combine(TxtInputFolder.Text, $"dec_{fileidx.ToString("00000")}.bin");
                    H3D Extra = FormatIdentifier.IdentifyAndOpen(filename, Data.Models[0].Skeleton);
                    if ( Extra == null ) continue;
                    Data.Merge(Extra);
                }

                if (ExportModels)
                {
                    for (int Index = 0; Index < Data.Models.Count; Index++)
                    {
                        string FileName = BaseName + Data.Models[Index].Name;

                        switch (Format)
                        {
                            case 0: new DAE(Data, Index).Save(FileName + ".dae"); break;
                            case 1:
                            case 2:
                                new SMD(Data, Index).Save(FileName + ".smd"); break;
                        }
                    }
                }

                if (ExportAnims)
                {
                    for (int Index = 0; Index < Data.SkeletalAnimations.Count; Index++)
                    {
                        string FileName = BaseName + Data.SkeletalAnimations[Index].Name;

                        switch (Format)
                        {
                            case 0: new DAE(Data, 0, Index).Save(FileName + ".dae"); break;
                            case 1: new SMD(Data, 0, Index).Save(FileName + ".smd"); break;
                            case 2: 
                                new TRLXANM(Data, 0, Index).Save(FileName + ".tranm");
                                new TRLXANM(Data, 0, Index).Save(FileName + ".gfbanm");
                                break;
                        }
                    }
                }

                if (ExportSkeletons)
                {
                    for (int Index = 0; Index < Data.Models.Count; Index++)
                    {
                        string FileName = BaseName + Data.Models[Index].Name;
                        new TRLXSKL(Data, Index).Save(FileName + ".trskl"); break;
                    }
                }

                if (ExportTexs)
                {
                    foreach (H3DTexture Tex in Data.Textures)
                    {
                        Tex.ToBitmap().Save(BaseName + Tex.Name + ".png");
                    }
                }

                ProgressConv.Value = (i/FileLen) * 100;

                Application.DoEvents();
            }

            BtnConvert.Enabled = true;
        }
    }
}
