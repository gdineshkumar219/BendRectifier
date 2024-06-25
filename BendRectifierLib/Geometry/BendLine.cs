using System.Drawing;

namespace BendRectifierLib.Geometry;
#region Record BendLine -------------------------------------------------------------------------
/// <summary> Represents a bend line of 2D sheet metal part </summary>
public record BendLine (CADPoint StartP, CADPoint EndP, double BendDeduction, double BendRadius, double BendAngle) {
   #region Properties ---------------------------------------------------------------------------
   public double Slope => mSlope == double.MaxValue ? mSlope = Math.Abs (Math.Round ((EndP.Y - StartP.Y) / (EndP.X - StartP.X), 1)) : mSlope;
   double mSlope = double.MaxValue;
   #endregion

   #region Methods ------------------------------------------------------------------------------
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

   /// <summary>
   /// Returns false if point does not lie on the same (infinitely extended line). IntPt = Invalid;
   /// Returns true if:
   /// 1. Point is Co-linear. IntPt = Invalid;
   /// 2. Point lies on extended version of line (or on line). IntPt = Point of intersection;
   /// </summary>
   public bool IntersectsAt (CADPoint x, CADPoint y, out CADPoint intPt) {
      intPt = CADPoint.Invalid;
      double a1 = StartP.Y - EndP.Y, b1 = EndP.X - StartP.X; if (a1 == 0 && b1 == 0) return false;
      double c1 = StartP.X * (EndP.Y - StartP.Y) + StartP.Y * (StartP.X - EndP.X);
      double a2 = x.Y - y.Y, b2 = y.X - x.X; if (a2 == 0 && b2 == 0) return false;
      double c2 = x.X * (y.Y - x.Y) + x.Y * (x.X - y.X);
      double denom = a1 * b2 - a2 * b1; if (denom == 0) return IsColinearSegment (x, y);
      intPt = new CADPoint ((b1 * c2 - b2 * c1) / denom, (c1 * a2 - c2 * a1) / denom);
      return true;
   }

   public bool IsColinearSegment (CADPoint x, CADPoint y) =>
            Slope == new Line (x, y).Slope && (x.LiesOn (new Line (StartP, EndP)) || y.LiesOn (new Line (StartP, EndP)));
   #endregion
}
#endregion
