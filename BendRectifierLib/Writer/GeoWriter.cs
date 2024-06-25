using BendRectifierLib.Geometry;
using System.Drawing;
using System.Text;

namespace BendRectifierLib.Writer;
#region GeoWriter ---------------------------------------------------------------------------------
public class GeoWriter (string fileName) {
   #region Methods --------------------------------------------------------------------------------
   public void OverWrite (List<CADPoint> newPoints, List<BendLine> bendLines) {
      var allPts = newPoints.ToList ();
      foreach (var line in bendLines) {
         allPts.Add (line.StartP);
         allPts.Add (line.EndP);
      }
      allPts = allPts.Distinct ().OrderBy (x => x.X).ThenBy (x => x.Y).ToList ();
      if (newPoints.Count > 2) newPoints = CADPoint.SortPointsAnticlockwise (newPoints).ToList ();
      var fileLines = File.ReadAllLines (mFileName);
      int bendIndex = 0;
      var updatedContent = new StringBuilder ();
      for (int i = 0; i < fileLines.Length; i++) {
         string currentLine = fileLines[i].Trim ();
         switch (currentLine) {
            case "#~31":
               while (i < fileLines.Length && fileLines[i].Trim () != "##~~") i++;
               updatedContent.AppendLine ("#~31");
               for (int j = 0; j < allPts.Count; j++)
                  updatedContent.AppendLine ($"P\n{j + 1}\n{allPts[j].X:F9} {allPts[j].Y:F9} {0:F9}\n|~");
               updatedContent.AppendLine ("##~~");
               break;
            case "#~331":
               updatedContent.AppendLine (fileLines[i]);
               int st = 0, end = 0;
               // Skip through the LINE section until 'Bend' keyword is encountered
               while (i < fileLines.Length && fileLines[i].Trim () != "##~~") i++;
               for (int k = 0; k < newPoints.Count; k++) {
                  st = allPts.IndexOf (newPoints[k]);
                  if (k < newPoints.Count - 1) end = allPts.IndexOf (newPoints[k + 1]);
                  updatedContent.AppendLine ($"LIN\n1 0\n{st + 1} {end + 1}\n|~");
               }
               updatedContent.AppendLine ("##~~");
               break;
            case "#~371":
               updatedContent.AppendLine (fileLines[i]);
               // Skip through the LINE section until 'Bend' keyword is encountered
               while (i < fileLines.Length && fileLines[i].Trim () != "##~~") i++;
               updatedContent.AppendLine ("LIN\n4 0");
               int stPos = 0, endPos = 0;
               (stPos, endPos) = (allPts.IndexOf (bendLines[bendIndex].StartP), allPts.IndexOf (bendLines[bendIndex].EndP));
               updatedContent.AppendLine ($"{stPos + 1} {endPos + 1}\n|~\n##~~");
               bendIndex++;
               break;
            default:
               updatedContent.AppendLine (fileLines[i]);
               break;
         }
      }
      string directory = Path.GetDirectoryName (mFileName) ?? string.Empty;
      string originalFileName = Path.GetFileNameWithoutExtension (mFileName);
      string extension = Path.GetExtension (mFileName);
      string newFileName = Path.Combine (directory, $"modified_{originalFileName}{extension}");
      File.WriteAllText (newFileName, updatedContent.ToString ());
   }
   #endregion

   #region Private Data ---------------------------------------------------------------------------
   readonly string mFileName = fileName;
   #endregion
}
#endregion
