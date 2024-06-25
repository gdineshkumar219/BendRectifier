using BendRectifierLib.Geometry;
using System.Drawing;

namespace BendRectifierLib.Reader;
using static BendRectifierLib.EGeoState;
#region GeoReader --------------------------------------------------------------------------------
public class GeoReader (string fileName) {
   #region Methods --------------------------------------------------------------------------------
   /// <summary> Reads GEO file and extract vertices and bend lines </summary>
   /// <returns> Tuple containing vertices and bend lines</returns>
   public (List<CADPoint> Vertices, List<BendLine> Bends) Read () {
      List<CADPoint> vertices = [];
      var bendLines = new List<BendLine> ();
      (double BendAngle, double BendRadius, double BendFactor) bendData = default;
      EGeoState state = INITIAL;
      using StreamReader reader = new (mFileName);
      while (reader.ReadLine () is string line) {
         line = line.Trim ();
         switch (state) {
            case INITIAL:
               if (line == "#~31") state = POINTS;
               else if (line == "#~331") state = LINES;
               else if (line == "#~37") {
                  state = BENDPARAMETER;
                  bendData = ProcessBendData (reader);
               } else if (line == "#~371") {
                  state = BENDLINE;
                  (int, int) bendVertices = default;
                  bendVertices = ProcessBendVertices (reader);
                  if (bendVertices != default) {
                     CADPoint st = new (), endPt = new ();
                     (var i1, var i2) = (bendVertices.Item1, bendVertices.Item2);
                     if (sPoints.Count > 0)
                        (st, endPt) = (sPoints[i1 - 1], sPoints[i2 - 1]);
                     bendLines.Add (new BendLine (st, endPt, bendData.BendFactor, bendData.BendRadius, bendData.BendAngle));
                     bendVertices = default;
                  }
               }
               break;
            case POINTS:
               if (line.StartsWith ('P'))
                  ProcessPoints (reader);
               else if (line.StartsWith ("##~~"))
                  state = INITIAL;
               break;
            case LINES:
               while (line != null && line != "##~~") {
                  if (line.StartsWith ("LIN")) ProcessOuterContour (reader, vertices);
                  line = reader.ReadLine () ?? "";
               }
               state = INITIAL;
               break;
            default:
               state = INITIAL;
               break;
         }
      }
      vertices = vertices.Distinct ().ToList ();
      return (vertices, bendLines);
   }
   #endregion

   #region Implementation -------------------------------------------------------------------------
   /// <summary> To extract bend vertices </summary>
   /// <returns> Bend attributes in a list</returns>
   static (int, int) ProcessBendVertices (StreamReader reader) {
      if (reader.ReadLine () == "LIN") reader.ReadLine ();
      if (reader.ReadLine () is string line && line != "|~") return GetVertices (line);
      return default;
   }

   /// <summary> Helper method for parsing attributes </summary>
   static (int, int) GetVertices (string? line) {
      var parts = line?.Split (' ');
      if (parts?.Length == 2 &&
         int.TryParse (parts[0], out int val1) &&
         int.TryParse (parts[1], out int val2))
         return (val1, val2);
      throw new FormatException ("The line does not contain valid vertex data.");
   }

   /// <summary> To extract bend line attributes </summary>
   /// <returns> Tuple containing bend radius, bend angle and bend factor </returns>
   static (double, double, double) ProcessBendData (StreamReader reader) {
      double bendAngle, bendRadius, bendFactor;
      reader.ReadLine ();
      bendAngle = ParseBendAttributes (reader.ReadLine () ?? "");
      bendRadius = ParseBendAttributes (reader.ReadLine () ?? "");
      bendFactor = Math.Abs (ParseBendAttributes (reader.ReadLine () ?? ""));
      return (bendAngle, bendRadius, bendFactor);
   }

   /// <summary> To get all the points of the model </summary>
   static void ProcessPoints (StreamReader reader) {
      reader.ReadLine ();
      if (reader.ReadLine () is string line) {
         string[] parts = line.Split (' ');
         if (parts.Length == 3 &&
            double.TryParse (parts[0], out double x) &&
            double.TryParse (parts[1], out double y))
            sPoints.Add (new CADPoint (x, y));
      }
   }

   /// <summary> To process outer contour points </summary>
   static void ProcessOuterContour (StreamReader reader, List<CADPoint> vertices) {
      reader.ReadLine (); // Skip the first line
      if (reader.ReadLine () is string line) {
         string[] parts = line.Split (' ');
         if (parts.Length == 2 &&
             int.TryParse (parts[0], out int val1) &&
             int.TryParse (parts[1], out int val2)) {
            vertices.Add (sPoints[val1 - 1]);
            vertices.Add (sPoints[val2 - 1]);
         }
      }
   }

   /// <summary> To parse bend line attributes such as bend radius, bend angle , bend factor,etc </summary>
   /// <param name="line"></param>
   /// <returns> Parsed value of attributes </returns>
   static double ParseBendAttributes (string line) => double.TryParse (line.Split (' ')[0], out double value) ? value : 0.0;
   #endregion

   #region Private Data ---------------------------------------------------------------------------
   static readonly List<CADPoint> sPoints = [];
   readonly string mFileName = fileName;
   #endregion
}
#endregion
