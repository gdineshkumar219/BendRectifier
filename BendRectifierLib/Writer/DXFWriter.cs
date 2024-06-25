using BendRectifierLib.Geometry;
using System.Drawing;
using System.Text;

namespace BendRectifierLib.Writer;
#region DXFWriter ---------------------------------------------------------------------------------
public class DXFWriter (string filename) {

   #region Methods -------------------------------------------------------------------------------
   /// <summary> To overwrite the update points </summary>
   /// <param name="newPoints"></param>
   /// <param name="bendLines"></param>
   public void OverWrite (List<CADPoint> newPoints, List<BendLine> bendLines) {
      // Ensure the points are sorted in an anticlockwise direction and distinct
      if (newPoints.Count > 2) newPoints = CADPoint.SortPointsAnticlockwise (newPoints.Distinct ()).ToList ();
      // Read all lines from the input file
      var fileLines = File.ReadAllLines (mFilePath);
      StringBuilder updatedContent = new ();
      int bendIndex = 0;
      for (int i = 0; i < fileLines.Length; i++) {
         string currentLine = fileLines[i].Trim ();
         switch (currentLine) {
            case "VERTEX":
               // Skip the original VERTEX section until SEQEND is encountered
               while (i < fileLines.Length && fileLines[i].Trim () != "SEQEND") i++;
               // Add the new points in the required format
               foreach (var point in newPoints)
                  updatedContent.AppendLine ($"VERTEX\n\t8\n0\n\t10\n{point.X.ToString ()}\n\t20\n{point.Y.ToString ()}\n\t0");
               updatedContent.AppendLine ("SEQEND");
               break;
            case "LINE":
               updatedContent.AppendLine (fileLines[i]);
               // Skip through the LINE section until 'Bend' keyword is encountered
               while (i < fileLines.Length && fileLines[i].Trim () != "Bend")
                  updatedContent.AppendLine (fileLines[++i]);
               // Process the bend lines
               while (i < fileLines.Length) {
                  currentLine = fileLines[++i].Trim ();
                  if (currentLine == "1001") break;
                  updatedContent.AppendLine (fileLines[i]);
                  switch (currentLine) {
                     case "10":
                        if (bendIndex < bendLines.Count) {
                           updatedContent.AppendLine (bendLines[bendIndex].StartP.X.ToString ());
                           i++;
                        }
                        break;
                     case "20":
                        if (bendIndex < bendLines.Count) {
                           updatedContent.AppendLine (bendLines[bendIndex].StartP.Y.ToString ());
                           i++;
                        }
                        break;
                     case "11":
                        if (bendIndex < bendLines.Count) {
                           updatedContent.AppendLine (bendLines[bendIndex].EndP.X.ToString ());
                           i++;
                        }
                        break;
                     case "21":
                        if (bendIndex < bendLines.Count) {
                           updatedContent.AppendLine (bendLines[bendIndex].EndP.Y.ToString ());
                           bendIndex++;
                           i++;
                        }
                        break;
                  }
               }
               updatedContent.AppendLine ("\t1001");
               break;
            default:
               updatedContent.AppendLine (fileLines[i]);
               break;
         }
      }
      // Ensure directory and filename are properly handled
      string directory = Path.GetDirectoryName (mFilePath) ?? string.Empty;
      string originalFileName = Path.GetFileNameWithoutExtension (mFilePath);
      string extension = Path.GetExtension (mFilePath);
      string newFileName = Path.Combine (directory, $"modified_{originalFileName}{extension}");
      // Write the updated content to a new file
      File.WriteAllText (newFileName, updatedContent.ToString ());
   }
   #endregion

   #region Private Data --------------------------------------------------------------------------
   readonly string mFilePath = filename;
   #endregion
}
#endregion
