using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Microsoft.Win32;
using RetinaImageProcessor.UI.Logic.Shared.Processing;
using RetinaImageProcessor.UI.Logic.Shared.View;

namespace RetinaImageProcessor.UI.Logic.MatchedFilters.View
{
	// Token: 0x02000013 RID: 19
	public partial class MatchedFiltersView : UserControl, IStatefulView, INotifyPropertyChanged, IComponentConnector
	{
		// Token: 0x060000B2 RID: 178 RVA: 0x00007BAC File Offset: 0x00005DAC
		private void LoadImageFile_Click(object sender, RoutedEventArgs e)
		{

			DirectoryInfo d = new DirectoryInfo(@"x:\GitHub\Biometria\Aplikacja\img\");//Assuming Test is your Folder
			FileInfo[] Files = d.GetFiles("*.*"); //Getting Text files
			foreach(FileInfo file in Files )
			{

				//OpenFileDialog fileDialog = new OpenFileDialog();
				//fileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif)|*.png;*.jpg;*.jpeg;*.tif;*.tiff;*.gif|All files (*.*)|*.*";
				//bool? result = fileDialog.ShowDialog();
				//bool flag = result != null && result.Value;
				//if (flag)
				//{
					this.ProcessedFileLabel = file.Name;
					this.imageName = file.Name;
					this.path = file.DirectoryName;//Path.GetDirectoryName(fileDialog.FileName);
					this.ClearStates();
					this.ClearView();
					this.workingBitmap = AForge.Imaging.Image.FromFile(file.FullName);
					this.workingBitmap = MatchedFiltersView.ResizeImage(this.workingBitmap);
					this.CurrentImage.Source = MatchedFiltersView.getImageSourceFromBitmap(this.workingBitmap);
					this.IsMaskHandpicked = false;
					this.LastOperation = StateOperations.LoadImage;
					this.ShowNextImage();
				//}
				AllUpToExtractMinutiae_Click(sender, e);
			}
		}
	}
}
