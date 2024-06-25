using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using BendRectifierLib.Reader;
using BendRectifierLib.Geometry;
using BendRectifierLib.Writer;
using System.Collections.Generic;

namespace BendRectifier;
public class FileHandler {

   #region Properties------------------------------------------------------------------------------
   public string Title { get; set; }
   public bool IsModified { get; set; } = false;

   static string FilePath => mFilePath;
   #endregion

   #region Methods
   public void OpenFile () {
      var openFileDialog = new OpenFileDialog {
         Filter = "DXF files (*.dxf)|*.dxf|Geo files (*.geo)|*.geo"
      };
      if (openFileDialog.ShowDialog () == true) {
         mFilePath = openFileDialog.FileName;
         string extension = Path.GetExtension (mFilePath).ToLower ();
         Title = Path.GetFileName (mFilePath);
         Application.Current.MainWindow.Title = Title;
         switch (extension) {
            case ".dxf":
               LoadFromDXF (mFilePath);
               break;
            case ".geo":
               LoadFromGeo (mFilePath);
               break;
            default:
               MessageBox.Show ("Unsupported file format.");
               break;
         }
      }
   }

   void LoadFromDXF (string filePath) {
      if (File.Exists (filePath)) filePath = filePath;
      var reader = new DXFReader (filePath);
      var data = reader.Read ();
      var vertices = data.Vertices;
      var bends = data.Bends;
      Pline pline = new (vertices) { Closed = data.Polygon };
      DrawingSurface.Drawing.Plines = Pline.AllPlines;
      foreach (var b in bends) {
         CADPoint st = new (b.StartP.X, b.StartP.Y), end = new (b.EndP.X, b.EndP.Y);
         Line line = new (st, end);
         Pline.BendLines.Add (line);
      }
      DrawingSurface.Drawing.AddPline (pline);
   }

   static void LoadFromGeo (string filePath) {
      //Pline.Reset ();
      var reader = new GeoReader (filePath);
      var data = reader.Read ();
      var vertices = data.Vertices;
      var bends = data.Bends;
      Pline pline = new (vertices) { Closed = true };
      DrawingSurface.Drawing.Plines = Pline.AllPlines;
      foreach (var b in bends) {
         CADPoint st = new (b.StartP.X, b.StartP.Y), end = new (b.EndP.X, b.EndP.Y);
         Line line = new (st, end);
         Pline.BendLines.Add (line);
      }
      DrawingSurface.Drawing.AddPline (pline);
   }
  

   private void WriteGeo (List<CADPoint> pts, List<BendLine> bendLines) {
      var writer = new GeoWriter (FilePath);
      writer.OverWrite (pts, bendLines);
   }

   private void WriteDxf (List<CADPoint> pts,List<BendLine>bendLines) {
      var writer = new DXFWriter (FilePath);
      writer.OverWrite (pts,bendLines);
   }

   public void SaveFile (List<CADPoint>pts, List<BendLine> bendLines) {
      var saveFileDialog = new SaveFileDialog {
         Filter = "DXF files (*.dxf)|*.dxf|geo files (*.geo)|*.geo",
      };
      if (saveFileDialog.ShowDialog () == true) {
         string extension = Path.GetExtension (FilePath).ToLower ();
         switch (extension) {
            case ".txt":
               WriteDxf (pts,bendLines);
               break;
            case ".bin":
               WriteGeo (pts,bendLines);
               break;
            default:
               MessageBox.Show ("Unsupported file format.");
               break;
         }
         IsModified = false;
         UpdateMainWindowTitle (Path.GetFileName (FilePath));
      }
   }

   void UpdateMainWindowTitle (string fileName) {
      string title = string.IsNullOrEmpty (fileName) ? "Untitled" : fileName;
      if (IsModified)
         title += "*";
      Application.Current.MainWindow.Title = title;
   }
   #endregion
   #region Private fields -------------------------------------------------------------------------
   Drawing drawing = new ();
   static string mFilePath;
   #endregion
}
