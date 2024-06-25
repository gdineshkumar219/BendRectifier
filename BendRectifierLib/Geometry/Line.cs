using System.Drawing;

namespace BendRectifierLib.Geometry;
#region Class Line-------------------------------------------------------------------------------
/// <summary>Represents a Line with a start point and an end point</summary>
public class Line {
   #region Constructor --------------------------------------------------------------------------
   public Line (CADPoint start, CADPoint end) => (StartP, EndP) = start.X < end.X || start.Y < end.Y ? (start, end) : (end, start);
   #endregion

   #region Properties ---------------------------------------------------------------------------
   public CADPoint StartP;
   public CADPoint EndP;
   public double Slope => mSlope is double.MaxValue ? mSlope = Math.Abs (Math.Round ((EndP.Y - StartP.Y) / (EndP.X - StartP.X), 1)) : mSlope;
   double mSlope = double.MaxValue;
   #endregion

   #region Methods ------------------------------------------------
   public bool IntersectsSegAt (CADPoint p1, CADPoint p2, out CADPoint intPt) {
      if (!IntersectsAt (p1, p2, out intPt)) return false;
      double lie = intPt.LieOn (p1, p2), liedelta = Epsilon / p1.DistanceTo (p2);
      // lie delta - margin of error.
      if (lie < -liedelta || lie > 1 + liedelta) { intPt = CADPoint.Invalid; return false; }
      lie = intPt.LieOn (StartP, EndP); liedelta = Epsilon / StartP.DistanceTo (EndP);
      if (lie < -liedelta || lie > 1 + liedelta) { intPt = CADPoint.Invalid; return false; }
      return true;
   }
   public const double Epsilon = 1e-6;

   public bool IntersectsAt (CADPoint x, CADPoint y, out CADPoint intPt) {
      intPt = CADPoint.Invalid;
      double a1 = StartP.Y - EndP.Y, b1 = EndP.X - StartP.X; if (a1 == 0 && b1 == 0) return false;
      double c1 = StartP.X * (EndP.Y - StartP.Y) + StartP.Y * (StartP.X - EndP.X);
      double a2 = x.Y - y.Y, b2 = y.X - x.X; if (a2 == 0 && b2 == 0) return false;
      double c2 = x.X * (y.Y - x.Y) + x.Y * (x.X - y.X);
      double denom = a1 * b2 - a2 * b1; if (denom == 0) return false;
      intPt = new CADPoint ((b1 * c2 - b2 * c1) / denom, (c1 * a2 - c2 * a1) / denom);
      return true;
   }

   public override int GetHashCode () => base.GetHashCode ();

   public override bool Equals (object? obj) => obj is Line line && this == line;
   #endregion

   #region Operators ----------------------------------------------
   public static bool operator == (Line x, Line y)
     => (x.StartP == y.StartP && x.EndP == y.EndP) || (x.StartP == y.EndP && x.EndP == y.StartP);

   public static bool operator != (Line x, Line y)
      => !(x == y);
   #endregion
}
#endregion
