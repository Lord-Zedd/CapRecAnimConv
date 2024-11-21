using Microsoft.Win32;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace CapRecAnimConv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string _rootDir;
		Dictionary<OutType, string> _extensions = new Dictionary<OutType, string>()
		{
			[OutType.JMA] = ".jma",
			[OutType.JMM] = ".jmm",
			[OutType.JMT] = ".jmt",
			[OutType.JMO] = ".jmo",
			[OutType.JMR] = ".jmr",
			[OutType.JMRX] = ".jmrx",
			[OutType.JMZ] = ".jmz",
			[OutType.JMW] = ".jmw",
			[OutType.TXT_Pos] = ".txt",
			[OutType.TXT_PosRot] = ".txt",
		};

		public MainWindow()
		{
			InitializeComponent();
			_rootDir = Directory.GetCurrentDirectory();
			GetBinList("captures");
		}

		private void GetBinList(string path)
		{
			string animPath = Path.Combine(_rootDir, path);
			if (!Directory.Exists(animPath))
				return;

			foreach (var d in Directory.GetDirectories(animPath))
			{
				TreeViewItem par = new TreeViewItem();
				string parShort = Path.GetFileName(d);
				par.Header = parShort;
				par.Tag = new TreeTag(TreeItemType.Directory, d, parShort);

				foreach (var bin in Directory.GetFiles(d))
				{
					TreeViewItem anim = new TreeViewItem();
					string animShort = Path.GetFileNameWithoutExtension(bin);
					anim.Header = animShort;
					anim.Tag = new TreeTag(TreeItemType.File, bin, animShort);

					par.Items.Add(anim);
				}

				treeFiles.Items.Add(par);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (OutputFormat.SelectedIndex == -1)
				return;

			var selected = treeFiles.SelectedItem as TreeViewItem;
			if (selected == null || (selected.Tag is not TreeTag))
				return;
			var tag = selected.Tag as TreeTag;

			ComboBoxItem selectedPos = PositionMod.SelectedItem as ComboBoxItem;
			if (selectedPos == null || selectedPos.Tag is not PositionModType)
				return;
			PositionModType posMod = (PositionModType)selectedPos.Tag;

			Vector3 posCustom = Vector3.Zero;
			if (posMod == PositionModType.Custom)
			{
				if (!float.TryParse(PositionXCustom.Text, out float customX) ||
					!float.TryParse(PositionYCustom.Text, out float customY) ||
					!float.TryParse(PositionZCustom.Text, out float customZ))
					return;

				posCustom = new Vector3(customX, customY, customZ);
			}

			ComboBoxItem selectedRot = RotationMod.SelectedItem as ComboBoxItem;
			if (selectedRot == null || selectedRot.Tag is not RotationModType)
				return;
			RotationModType rotMod = (RotationModType)selectedRot.Tag;

			ComboBoxItem selectedOut = OutputFormat.SelectedItem as ComboBoxItem;
			if (selectedOut == null || selectedOut.Tag is not OutType)
				return;
			OutType outType = (OutType)selectedOut.Tag;

			if (tag.Type == TreeItemType.Directory)
			{
				OpenFolderDialog ofd = new OpenFolderDialog()
				{
					Title = "Select Output Directory",
				};
				var result = ofd.ShowDialog();
				if (!(bool)result)
					return;

				string folder = Path.GetFileName(ofd.SafeFolderName);

				foreach (TreeViewItem childItem in selected.Items)
				{
					if (childItem == null || childItem.Tag is not TreeTag)
						continue;
					TreeTag childTag = (TreeTag)childItem.Tag;

                    string outpath = Path.Combine(folder, childTag.Name + _extensions[outType]);

					HandleDump(outpath, childTag.Path, posMod, rotMod, posCustom, outType);
				}
			}
			else
			{
				SaveFileDialog sfd = new SaveFileDialog
				{
					RestoreDirectory = true,
					Title = "Save Animation",
					FilterIndex = OutputFormat.SelectedIndex + 1,
					Filter = "JMA (*.jma)|*.jma;|JMM (*.jmm)|*.jmm;|JMT (*.jmt)|*.jmt;|JMO (*.jmo)|*.jmo;|JMR (*.jmr)|*.jmr;|" +
					"JMRX (*.jmrx)|*.jmrx;|JMZ (*.jmz)|*.jmz;|JMW (*.jmw)|*.jmw;|Text File (Pos) (*.txt)|*.txt;|Text File (Pos+Rot) (*.txt)|*.txt;",
					FileName = tag.Name
				};
				if (!(bool)sfd.ShowDialog())
					return;

				HandleDump(sfd.FileName, tag.Path, posMod, rotMod, posCustom, outType);
			}
		}

		private bool HandleDump(string outPath, string binPath, PositionModType posMod, RotationModType rotMod, Vector3 customPos, OutType type)
		{
			CapturedAnimation bin = new CapturedAnimation(binPath);

			if (type == OutType.TXT_Pos)
				return bin.WriteTXT(outPath);
			else if (type == OutType.TXT_PosRot)
				return bin.WriteTXT(outPath, true);
			else
			{
				bin.ApplyMods(posMod, rotMod, customPos);
				return bin.WriteJMA(outPath);
			}
		}
	}
}