using System.Windows;
using System.Windows.Input;
namespace BendRectifier;
class PanWidget { // Works in screen space
   #region Constructors ----------------------------------------------------------------
   public PanWidget (DrawingSurface dwgSurface, Action<Vector> onPan) {
      mOnPan = onPan;
      dwgSurface.MouseDown += (sender, e) => {
         if (e.ChangedButton == MouseButton.Middle) PanStart (e.GetPosition (dwgSurface));
      };
      dwgSurface.MouseUp += (sender, e) => {
         if (IsPanning) PanEnd (e.GetPosition (dwgSurface));
      };
      dwgSurface.MouseMove += (sender, e) => {
         if (IsPanning) PanMove (e.GetPosition (dwgSurface));
      };
      dwgSurface.MouseLeave += (sender, e) => {
         if (IsPanning) PanCancel ();
      };
   }
   #endregion

   #region Implementation --------------------------------------------------------------
   bool IsPanning => mPrevPt != null;

   void PanStart (Point pt) => mPrevPt = pt;

   void PanMove (Point pt) {
      mOnPan.Invoke (pt - mPrevPt!.Value);
      mPrevPt = pt;
   }

   void PanEnd (Point? pt) {
      if (pt.HasValue)
         PanMove (pt.Value);
      mPrevPt = null;
   }

   void PanCancel () => PanEnd (null);
   #endregion

   #region Private Data ----------------------------------------------------------------
   Point? mPrevPt;
   readonly Action<Vector> mOnPan;
   #endregion
}
