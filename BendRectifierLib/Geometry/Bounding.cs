using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BendRectifierLib.Geometry;
public struct Bounding { // Bound in drawing space
   #region Constructors
   public Bounding (CADPoint cornerA, CADPoint cornerB) {
      MinX = Math.Min (cornerA.X, cornerB.X);
      MaxX = Math.Max (cornerA.X, cornerB.X);
      MinY = Math.Min (cornerA.Y, cornerB.Y);
      MaxY = Math.Max (cornerA.Y, cornerB.Y);
   }

   public Bounding (IEnumerable<CADPoint> pts) {
      MinX = pts.Min (p => p.X);
      MaxX = pts.Max (p => p.X);
      MinY = pts.Min (p => p.Y);
      MaxY = pts.Max (p => p.Y);
   }

   public Bounding (IEnumerable<Bounding> bounds) {
      MinX = bounds.Min (b => b.MinX);
      MaxX = bounds.Max (b => b.MaxX);
      MinY = bounds.Min (b => b.MinY);
      MaxY = bounds.Max (b => b.MaxY);
   }

   public Bounding () {
      this = Empty;
   }

   public static readonly Bounding Empty = new () { MinX = double.MaxValue, MinY = double.MaxValue, MaxX = double.MinValue, MaxY = double.MinValue };
   #endregion

   #region Properties
   public double MinX { get; init; }
   public double MaxX { get; init; }
   public double MinY { get; init; }
   public double MaxY { get; init; }
   public double Width => MaxX - MinX;
   public double Height => MaxY - MinY;
   public CADPoint Mid => new ((MaxX + MinX) / 2, (MaxY + MinY) / 2);
   public bool IsEmpty => MinX > MaxX || MinY > MaxY;
   #endregion

   #region Methods
   public Bounding Inflated (CADPoint ptAt, double factor) {
      if (IsEmpty) return this;
      var minX = ptAt.X - (ptAt.X - MinX) * factor;
      var maxX = ptAt.X + (MaxX - ptAt.X) * factor;
      var minY = ptAt.Y - (ptAt.Y - MinY) * factor;
      var maxY = ptAt.Y + (MaxY - ptAt.Y) * factor;
      return new () { MinX = minX, MaxX = maxX, MinY = minY, MaxY = maxY };
   }
   #endregion
}