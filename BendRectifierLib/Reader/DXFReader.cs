using BendRectifierLib.Geometry;
using System.Drawing;

namespace BendRectifierLib.Reader;
#region DXFReader ----------------------------------------------------------------------------------
public class DXFReader (string fileName) {

   #region Methods --------------------------------------------------------------------------------
   /// <summary> Reads DXF file </summary>
   /// <returns> Bend data and vertices </returns>
   public (List<CADPoint> Vertices, List<BendLine> Bends, double Thickness,bool Polygon) Read () {
      using StreamReader reader = new (mFileName);
      var state = EDxfState.INITIAL;
      bool polylineClosed = false;
      double bendAngle = 0, thickness = 0, bendRadius = 0, kFactor = 0, bendFactor = 0;
      List<CADPoint> vertices = [];
      (CADPoint, CADPoint) bendPts = default;
      List<BendLine> bendLines = [];
      while (reader.ReadLine () is string line) {
         line = line.Trim ();
         switch (state) {
            case EDxfState.INITIAL:
               if (line == "POLYLINE") {
                  state = EDxfState.POLYLINE;
                  polylineClosed = false;
               } else if (line == "LINE")
                  state = EDxfState.BENDLINE;
               break;
            case EDxfState.POLYLINE:
               if (line == "SEQEND")
                  state = EDxfState.INITIAL;
               else if (line == "VERTEX")
                  ProcessVertices (reader, vertices);
               else if (line == "70" && int.TryParse (reader.ReadLine ()?.Trim (), out int flags))
                  polylineClosed = (flags & 1) == 1;
               else if (line.StartsWith ("Bend"))
                  state = EDxfState.BENDLINE;
               break;
            case EDxfState.BENDLINE:
               bendPts = ProcessBendPoints (line, reader);
               state = EDxfState.BENDPARAMETER;
               break;
            case EDxfState.BENDPARAMETER:
               if (line.StartsWith ("BEND_ANGLE")) bendAngle = ParseBendAttributes (line);
               else if (line.StartsWith ("THICKNESS")) thickness = ParseBendAttributes (line);
               else if (line.StartsWith ("BEND_RADIUS")) bendRadius = ParseBendAttributes (line);
               else if (line.StartsWith ("K_FACTOR")) kFactor = ParseBendAttributes (line);
               else if (line.StartsWith ("BEND_FACTOR")) bendFactor = ParseBendAttributes (line);
               if (line.StartsWith ("BUMP") && bendPts != default) {
                  bendLines.Add (new BendLine (bendPts.Item1, bendPts.Item2, bendFactor, bendRadius, bendAngle));
                  bendPts = default;
                  state = EDxfState.INITIAL;
               }
               break;
         }
      }
      return (vertices, bendLines, thickness, polylineClosed);
   }
   #endregion

   #region Implementation -------------------------------------------------------------------------
   /// <summary> To parse bend parameters </summary>
   /// <param name="line"></param>
   /// <returns> Bend parameter in double</returns>
   static double ParseBendAttributes (string line) => double.TryParse (line.Split (':')[1], out double value) ? value : 0.0;

   /// <summary> To extract outer contour points </summary>
   static void ProcessVertices (StreamReader reader, List<CADPoint> points) {
      bool isX = false, isY = false;
      double x = 0.0, y = 0.0;
      while (reader.ReadLine () is string line && line.Trim () != "SEQEND") {
         line = line.Trim ();
         if (line == "10") isX = double.TryParse (reader.ReadLine ()?.Trim (), out x);
         else if (line == "20") isY = double.TryParse (reader.ReadLine ()?.Trim (), out y);
         if (isX && isY) {
            points.Add (new CADPoint (x, y));
            isX = isY = false;
         }
      }
   }

   /// <summary> To extract bend points </summary>
   static (CADPoint, CADPoint) ProcessBendPoints (string? line, StreamReader reader) {
      (CADPoint, CADPoint) vertex = default;
      bool isX = false, isY = false;
      double x = 0.0, y = 0.0;
      while (line != null && line.Trim () != "1001") {
         line = line.Trim ();
         if (line == "10" || line == "11") isX = double.TryParse (reader.ReadLine ()?.Trim (), out x);
         else if (line == "20" || line == "21") isY = double.TryParse (reader.ReadLine ()?.Trim (), out y);
         if (isX && isY) {
            if (vertex.Item1 == default) vertex.Item1 = new CADPoint (x, y);
            else vertex.Item2 = new CADPoint (x, y);
            x = y = 0.0;
            isX = isY = false;
         }
         if (line.Trim () == "1001") break;
         line = reader.ReadLine ();
      }
      return vertex;
   }
   #endregion

   #region Private Data ---------------------------------------------------------------------------
   readonly string mFileName = fileName;
   #endregion
}
#endregion
