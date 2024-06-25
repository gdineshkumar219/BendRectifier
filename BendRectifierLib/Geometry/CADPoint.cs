using System.Drawing;

namespace BendRectifierLib.Geometry;
public struct CADPoint {

   #region Constructor ----------------------------------------------------------------------------
   public CADPoint (double x, double y) {
      X = x;
      Y = y;
   }
   #endregion

   public readonly bool LiesOn (Line l) {
      CADPoint pt = new (X, Y);
      var (sPt, ePt) = (l.StartP, l.EndP);
      var (d1, d2, d3) = (pt.DistanceTo (sPt), pt.DistanceTo (ePt), sPt.DistanceTo (ePt));
      return d1 + d2 == d3;
   }
   public readonly double DistanceTo (CADPoint p)
        => Math.Sqrt (Math.Pow (p.X - X, 2) + Math.Pow (p.Y - Y, 2));
   public double DX (CADPoint other) => X - other.X;
   public double DY (CADPoint other) => Y - other.Y;
   public double Angle (CADPoint other) => Math.Atan2 (DY (other), DX (other));

   public static CADPoint operator + (CADPoint point, CADPoint vector) => new (point.X + vector.X, point.Y + vector.Y);
   public static CADPoint operator / (CADPoint a, double d) => new (a.X / d, a.Y / d);
   public static CADPoint operator - (CADPoint a, CADPoint b) => new (a.X - b.X, a.Y - b.Y);
   public static bool operator == (CADPoint a, CADPoint b) => a.Equals (b);
   public static bool operator != (CADPoint a, CADPoint b) => !(a == b);

   public static CADPoint Origin => new (0, 0);

   public static CADPoint Invalid = new (double.NaN, double.NaN);
   public static double Distance (CADPoint a, CADPoint b) {
      var dx = a.X - b.X;
      var dy = a.Y - b.Y;
      return Math.Sqrt (dx * dx + dy * dy);
   }
   public override readonly bool Equals (object? obj) {
      if (obj is not CADPoint) return false;
      CADPoint other = (CADPoint)obj;
      return X == other.X && Y == other.Y;
   }

   /// <summary> Sorts a IEnumerable of points in anticlockwise order around their centroid </summary>
   /// <param name="points">The list of points to be sorted </param>
   /// <returns>IEnumerable of points sorted in anticlockwise order </returns>
   public static IEnumerable<CADPoint> SortPointsAnticlockwise (IEnumerable<CADPoint> points) =>
       points.OrderBy (p => Math.Atan2 (p.Y - points.Average (p => p.Y), p.X - points.Average (p => p.X)));

   /// <summary>Returns the _lie_ of this Point2 on the line a-b</summary>
   /// The _lie_ of a Point2 on a finite line a-b is the normalized position of that Point2 along
   /// the line joining a-b. If the Point2 is at a, then its lie is 0. If the Point2 is at b, then
   /// the lie is 1. If the Point2 is exactly at the midpoint, the lie is 0.5 etc. Lies below 0 
   /// or above 1 are possible; this just means the Point2 lies _before_ a, or _beyond_ b.
   public readonly double LieOn (CADPoint a, CADPoint b) {
      double dx = b.X - a.X, dy = b.Y - a.Y;
      return Math.Abs (dx) > Math.Abs (dy)
          ? (dx == 0 ? 0 : (X - a.X) / dx) : (dy == 0 ? 0 : (Y - a.Y) / dy);
   }

   #region Fields ---------------------------------------------------------------------------------
   public double X;
   public double Y;
   #endregion
}
