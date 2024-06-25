using BendRectifierLib.Geometry;
using System.Windows.Media;
using System.Windows;

namespace BendRectifier;
public class Transforms {
   public static Matrix ComputeZoomExtentsProjXfm (double viewWidth, double viewHeight, Bounding b) {
      var viewMargin = 0;
      // Compute the scaling, to fit specified drawing extents into the view space
      double scaleX = (viewWidth - 2 * viewMargin) / b.Width, scaleY = (viewHeight - 2 * viewMargin) / b.Height;
      double scale = Math.Min (scaleX, scaleY);
      var scaleMatrix = Matrix.Identity; scaleMatrix.Scale (scale, -scale);
      // translation...
      CADPoint p = new (b.Mid.X, b.Mid.Y);
      Point projectedMidPt = scaleMatrix.Transform (new Point (p.X, p.Y));
      CADPoint viewMidPt = new (viewWidth / 2, viewHeight / 2);
      var translateMatrix = Matrix.Identity; translateMatrix.Translate (viewMidPt.X - projectedMidPt.X, viewMidPt.Y - projectedMidPt.Y);
      // Final zoom extents matrix, is a product of scale and translate matrices
      scaleMatrix.Append (translateMatrix);
      return scaleMatrix;
   }
}
