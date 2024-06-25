using BendRectifierLib.Geometry;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BendRectifier;
public class DrawingSurface : Canvas {
   public DrawingSurface() {
      MouseWheel += OnMouseWheel;
      MouseRightButtonDown += OnMouseRightDown;

   }
   #region Implementation -----------------------------------------------------------------------------------
   protected override void OnRender (DrawingContext dc) {
      base.OnRender (dc);
      var dwgCom = DrawingCommands.GetInstance;
      (dwgCom.DC, dwgCom.Xfm, dwgCom.Brush) = (dc, Xfm, Brushes.Red);
      mPen = new (Brushes.Red, 2);
      EntityBuilder?.Draw (dwgCom);
      mPen = new (Brushes.Black, 2);
      Drawing?.Draw (dwgCom);
   }
   void OnMouseWheel (object sender, MouseWheelEventArgs e) {
      double zoomFactor = 1.05;
      if (e.Delta > 0) zoomFactor = 1 / zoomFactor;
      var pt1 = mInvProjXfm.Transform (e.GetPosition (this));
      var ptDraw = new CADPoint (pt1.X, pt1.Y); // mouse point in drawing space
      var cA = mInvProjXfm.Transform (new Point (mViewMargin, mViewMargin));
      var cornerA = new CADPoint (cA.X, cA.Y);
      var cB = mInvProjXfm.Transform (new Point (ActualWidth - mViewMargin, ActualHeight - mViewMargin));
      var cornerB = new CADPoint (cB.X, cB.Y);
      var b = new Bounding (cornerA, cornerB);
      b = b.Inflated (ptDraw, zoomFactor);
      mProjXfm = Transforms.ComputeZoomExtentsProjXfm (ActualWidth, ActualHeight, b);
      mInvProjXfm = mProjXfm; mInvProjXfm.Invert ();
      Xfm = mProjXfm;
      InvalidateVisual ();
   }
   void OnMouseRightDown (object sender, MouseButtonEventArgs e) {
      mProjXfm = Transforms.ComputeZoomExtentsProjXfm (ActualWidth, ActualHeight, Drawing.Bound);
      mInvProjXfm = mProjXfm;  mInvProjXfm.Invert ();
      Xfm = mProjXfm;
      InvalidateVisual ();
   }
   #endregion

   #region Properties ---------------------------------------------------------------------------------------
   static public Drawing Drawing = new ();
   public Widget EntityBuilder;
   public Matrix Xfm, mProjXfm = Matrix.Identity, mInvProjXfm = Matrix.Identity;

   public static Pen Pen => mPen;
   double mViewMargin = 0;
   static Pen mPen;
   #endregion
}
public class DrawingCommands {

   #region Constructors -------------------------------------------------------------------------------------
   private DrawingCommands () { }
   #endregion

   #region Properties ---------------------------------------------------------------------------------------
   public static DrawingCommands GetInstance { get { mDrawingCommands ??= new DrawingCommands (); return mDrawingCommands; } }
   public DrawingContext DC { set => mDc = value; }
   public Matrix Xfm { set => mXfm = value; }

   public Brush Brush { get; set; }
   #endregion

   #region Implementation -----------------------------------------------------------------------------------
   public void DrawLines (IEnumerable<Point> dwgPts) {
      var itr = dwgPts.GetEnumerator ();
      if (!itr.MoveNext ()) return;
      var prevPt = itr.Current;
      while (itr.MoveNext ()) {
         DrawLine (prevPt, itr.Current);
         prevPt = itr.Current;
      }
   }

   public void DrawLine (Point startPt, Point endPt) {
      var pen = DrawingSurface.Pen;
      mDc.DrawLine (pen, mXfm.Transform (startPt), mXfm.Transform (endPt));
   }

   public void DrawBendLine (Point startPt, Point endPt) {
      var pen = new Pen (Brushes.ForestGreen, 2) {
         DashStyle = DashStyles.DashDot
      };
      mDc.DrawLine (pen, mXfm.Transform (startPt), mXfm.Transform (endPt));
   }



   public void DrawPolyline (IEnumerable<Point> dwgPts, bool closed) {
      // Ensure points is not null and has at least 2 points
      if (dwgPts == null || !dwgPts.Any ()) {
         throw new ArgumentException ("Points collection cannot be null or empty");
      }
      if (dwgPts.Count () > 1) {
         if (closed)
            DrawPolygon (dwgPts);
         else {
            DrawLines (dwgPts);
         }
      }
   }

   public void DrawPolygon (IEnumerable<Point> dwgPts) {
      DrawLines (dwgPts);
      DrawLine (dwgPts.ToList ()[^1], dwgPts.First ());
   }
   #endregion

   #region Private ------------------------------------------------------------------------------------------
   Matrix mXfm;

   DrawingContext mDc;
   static DrawingCommands mDrawingCommands;
   #endregion
}

public interface IDrawable {
   public Action RedrawReq { get; set; }
   public abstract void Draw (DrawingCommands drawingCommands);
}

public class Drawing : IDrawable {

   #region Implementation ----------------------------------------------------------------------------------
   public void AddPline (Pline pline) {
      mPlines.Add (pline);
      Bound = new Bounding (mPlines.Select (pline => pline.Bound));
      //RedrawReq ();
   }

   public virtual void Draw (DrawingCommands drawingCommands) {
      foreach (var pline in Pline.AllPlines)
         drawingCommands.DrawPolyline (pline.Points/*.GetPoints()*/.Select (x => new Point (x.X, x.Y)), pline.Closed);
         foreach (var b in Pline.BendLines) {
            CADPoint st = b.StartP, end = b.EndP;
            drawingCommands.DrawBendLine (new Point (st.X, st.Y), new Point (end.X, end.Y));
         }
   }
   public void Clear () => mPlines.Clear ();
   #endregion

   #region Properties --------------------------------------------------------------------------------------
   public Bounding Bound { get; set; }
   public Action RedrawReq { get; set; }
   public int Count => mPlines.Count;

   public List<Pline> Plines {
      get => mPlines;
      set => mPlines = value;
   }
   #endregion

   #region Private -----------------------------------------------------------------------------------------
   List<Pline> mPlines = new ();
   #endregion
}
