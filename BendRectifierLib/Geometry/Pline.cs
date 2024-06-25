using BendRectifierLib.Geometry;

namespace BendRectifier;
#region Pline -------------------------------------------------------------------------------------
public class Pline {
   #region Constructors ---------------------------------------------------------------------------
   public Pline (IEnumerable<CADPoint> pts) {
      mPoints = pts.ToList ();
      Bound = new Bounding (pts);
   }

   public Pline () {
     mPoints = new List<CADPoint> ();
      //Bound = new Bounding (mPoints);
   }
   #endregion

   #region Properties -----------------------------------------------------------------------------
   public bool Closed { get; set; }
   public List<CADPoint> Points => mPoints;
   public static List<Pline> AllPlines => mPlines;
   public static List<Line> BendLines => mBendLines;

   public EEntityType Name { get; set; }
   public Bounding Bound { get; }
   #endregion

   #region Implementation -------------------------------------------------------------------------
   #region Methods --------------------------------------------------------------------------------
   public static Pline CreateLine (CADPoint startPt, CADPoint endPt) {
      return new Pline (Enum (startPt, endPt));
      static IEnumerable<CADPoint> Enum (CADPoint a, CADPoint b) {
         yield return a;
         yield return b;
      }
   }

   public static Pline CreateRectangle (CADPoint corner1, CADPoint corner2, CADPoint corner3, CADPoint corner4) {
      return new Pline (Enum (corner1, corner2, corner3, corner4));
      static IEnumerable<CADPoint> Enum (CADPoint a, CADPoint b, CADPoint c, CADPoint d) {
         yield return a;
         yield return b;
         yield return c;
         yield return d;
      }
   }

   public static Pline CreatePolyLine (IEnumerable<CADPoint> pts) => new Pline (pts);

   public void Open () => Closed = false;

   public void Close () => Closed = true;

   public void SaveText (TextWriter tw) {
      tw.WriteLine (Name.ToString ());
      if (Name == EEntityType.POLYLINE) {
         tw.Flush ();
         
      }
      foreach (var pt in mPoints)
         tw.WriteLine ($"{pt.X},{pt.Y}");
      tw.WriteLine ("\n");
      //mPoints.Clear ();
   }

   public void SaveBinary (BinaryWriter bw) {
      //foreach( var p in AllPlines) {
      //   foreach(var pt in p.Points) {
      //      bw.Write (pt.X);
      //      bw.Write (pt.Y);
      //   }
      //   bw.Write ('\n');
      //}
      bw.Write (Name.ToString());
      foreach (var pt in mPoints) {
         bw.Write (pt.X);
         bw.Write (pt.Y);
      }
      bw.Write ("\n");
   }

   public static Pline CreateShape (EEntityType type, List<CADPoint> points) {
      return type switch {
         EEntityType.LINE => CreateLine (points[0], points[1]),
         EEntityType.RECTANGLE => CreateRectangle (points[0], points[1], points[2], points[3]),
         EEntityType.POLYLINE => CreatePolyLine (points),
         _ => throw new ArgumentException ("Invalid shape type"),
      };
   }

   public void LoadText (StreamReader sr) {
      mPoints.Clear ();
      string? line = sr.ReadLine ();
      while ((line) != null) {
         string[] parts = line.Split (',');
         if (parts.Length == 2 &&
             double.TryParse (parts[0], out double x) &&
             double.TryParse (parts[1], out double y)) 
            mPoints.Add (new CADPoint (x, y));
         if (Points.Count > 1 && (line.All (char.IsAsciiLetter) || line.All (char.IsWhiteSpace))) {
            mPlines.Add (new Pline (Points));
            mPoints.Clear ();
         }
         line = sr.ReadLine ();
      }
   }
   public void LoadBinary (BinaryReader br) {
      string? name = null;
      while (br.BaseStream.Position != br.BaseStream.Length) {
         if(mPoints.Count<=0) name = br.ReadString ();
         try {
            double x = br.ReadDouble ();
            double y = br.ReadDouble ();
            mPoints.Add (new CADPoint (x, y));
         } catch (EndOfStreamException) {
            break;
         }
            if (mPoints.Count > 1 && (br.ReadChar () == '\n'|| name is not null)) {
               mPlines.Add (new Pline (mPoints));
               mPoints.Clear ();
               name = null;
               br.ReadChar ();
         }
      }
   }

   // Helper method to read a line from the binary stream

   //public void LoadBinary (BinaryReader br) {
   //  // br.ReadString ();
   //   int cnt = br.ReadInt32 ();
   //   while (br.PeekChar () ==1) { 
   //   //for (int i = 0; i < cnt; i++) {
   //      mPoints.Add(new CADPoint(br.ReadDouble (), br.ReadDouble()));
   //      if (br.ReadChar () == '\n') {
   //         mPlines.Add (new Pline (Points));
   //         mPoints.Clear ();
   //         //}
   //      }

   //   }

   //   //Name = Enum.Parse<EEntityType> (br.ReadString ());
   //   // mPoints.Clear ();
   //   //var pointStr = br.Read();

   //   //while (pointStr!=null) {
   //   //   //if (pointStr == "\n")
   //   //   //   break;

   //   //   string[] parts = pointStr.Split (',');
   //   //   if (parts.Length == 2 &&
   //   //       double.TryParse (parts[0], out double x) &&
   //   //       double.TryParse (parts[1], out double y)) 
   //   //      mPoints.Add (new CADPoint (x, y));
   //   //   if(Points.Count > 1 && br.PeekChar () == 0 || pointStr == "\n") {
   //   //      mPlines.Add(new Pline (Points));
   //   //      mPoints.Clear ();
   //   //   }
   //   //   br.Read ().ToString();

   //   //}
   //   //Closed = br.ReadInt32 () == 1;
   //}


   //public void  LoadBinary (BinaryReader br) {
   //   //Name = br.Read ();
   //   int count = br.ReadInt32 ();
   //   mPoints.Clear ();
   //   for (int i = 0; i < count; i++) {
   //      if(double.TryParse (br.Read (), out double x) &&
   //          double.TryParse (br.Read (), out double y)) 
   //      mPoints.Add (new CADPoint (x, y));
   //   }
   //   Closed = br.ReadInt32 () == 1;
   //   return this;
   //}
   public static void Reset () {
      AllPlines.Clear ();
      BendLines.Clear ();
   }

   public void LoadDXF (TextReader tr) {
      mPoints.Clear ();
      string? line;
      bool inPolyline = false;

      while ((line = tr.ReadLine ()) != null) {
         if (line.Trim () == "POLYLINE") {
            inPolyline = true;
         } else if (inPolyline && line.Trim () == "VERTEX") {
            double x = 0.0, y = 0.0; bool isX = false, isY = false;
            while ((line = tr.ReadLine ()) != null && line.Trim()!= "SEQEND") {
               if (line.Trim () == "10") {
                  line = tr.ReadLine ();
                  isX =double.TryParse (line.Trim (), out x);
               } else if (line.Trim () == "20") {
                  line = tr.ReadLine ();
                  isY = double.TryParse (line.Trim (), out y);
               }
               if (isX && isY) {
                  mPoints.Add (new CADPoint (x, y));
                  isX = isY = false;
               }
              }
            while ((line = tr.ReadLine ()) != null && line.Trim () == "BEND") {
               if (line.Trim () == "10") {
                  line = tr.ReadLine ();
                  isX = double.TryParse (line.Trim (), out x);
               } else if (line.Trim () == "20") {
                  line = tr.ReadLine ();
                  isY = double.TryParse (line.Trim (), out y);
               }
               if (isX && isY) {
                  mBendPoints.Add (new CADPoint (x, y));
                  isX = isY = false;
               }
               if (mBendPoints.Count == 2) {
                  AllPlines.Add(new Pline(mBendPoints));
                  mBendPoints.Clear ();
               }
            }

         }
      }
      AllPlines.Add(new Pline (mPoints.ToArray()));
   }
   #endregion
   #endregion

   #region Private Fields -------------------------------------------------------------------------
   readonly List<CADPoint> mPoints,mBendPoints;
   static List<Pline> mPlines = new ();
   static List<Line> mBendLines = [];
   #endregion
}
#endregion


public enum EEntityType {
   LINE, RECTANGLE, POLYLINE
}
