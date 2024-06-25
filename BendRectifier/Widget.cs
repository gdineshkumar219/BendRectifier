using BendRectifierLib.Edit;
using BendRectifierLib.Geometry;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static BendRectifier.Utils;
namespace BendRectifier;

#region Widget ------------------------------------------------------------------------------------
public abstract class Widget : Drawing, INotifyPropertyChanged {
   #region Constructor ------------------------------------------------------------------------------------
   public Widget (DrawingSurface dwgPad) {
      mDwgPad = dwgPad;
      mDrawing = new ();
      mPanWidget = new PanWidget (dwgPad, OnPan);
   }
   protected void OnPropertyChanged (string propertyName) => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));


   #endregion

   #region Implementation ---------------------------------------------------------------------------------

   #region Mouse Events------------------------------------------------------------------------------------
   public void Attach () {
      mDwgPad.MouseLeftButtonDown += OnMouseLeftButtonDown;
      mDwgPad.MouseMove += OnMouseMove;
   }
   public void Detach () {
      mDwgPad.MouseLeftButtonDown -= OnMouseLeftButtonDown;
      mDwgPad.MouseMove -= OnMouseMove;
   }
   void OnMouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
      var pt = mDwgPad.mInvProjXfm.Transform (e.GetPosition (mDwgPad));
      DrawingClicked (pt);
      mDwgPad.InvalidateVisual ();
   }

   void OnMouseMove (object sender, MouseEventArgs e) {
      var pt = mDwgPad.mInvProjXfm.Transform (e.GetPosition (mDwgPad));
      DrawingHover (pt);
      mDwgPad.InvalidateVisual ();
   }

   void OnMouseRightDown (object sender, MouseButtonEventArgs e) {
      mDwgPad.mProjXfm = Transforms.ComputeZoomExtentsProjXfm (mDwgPad.ActualWidth, mDwgPad.ActualHeight, mDrawing.Bound);
      mDwgPad.mInvProjXfm = mDwgPad.mProjXfm; mDwgPad.mInvProjXfm.Invert ();
      mDwgPad.Xfm = mDwgPad.mProjXfm;
      mDwgPad.InvalidateVisual ();
   }

   void OnPan (Vector panDisp) {
      Matrix m = Matrix.Identity; m.Translate (panDisp.X, panDisp.Y);
      mDwgPad.mProjXfm.Append (m);
      mDwgPad.mInvProjXfm = mDwgPad.mProjXfm; mDwgPad.mInvProjXfm.Invert ();
      mDwgPad.Xfm = mDwgPad.mProjXfm;
      mDwgPad.InvalidateVisual ();
   }
   #endregion

   #region Construct Entity -------------------------------------------------------------------------------
   protected abstract Pline PointClicked (Point drawingPt);
   protected virtual void PointHover (Point drawingPt) {
      mHoverPt = drawingPt;
      mDwgPad.InvalidateVisual ();
   }
   protected virtual void DrawingClicked (Point drawingPt) {
      var pline = PointClicked (drawingPt);
      if (pline == null) return;
      DrawingSurface.Drawing?.AddPline (pline);
      mDwgPad?.InvalidateVisual ();
   }
   protected virtual void DrawingHover (Point drawingPt) {
      PointHover (drawingPt);
      mDwgPad.InvalidateVisual ();
   }
   #endregion
   #endregion

   #region Properties -------------------------------------------------------------------------------------
   public Dictionary<string, double> Parameter => mTxtContents;
   public string Prompt => mPrompt;
   public bool IsCompleted;
   #endregion

   #region Private Field ----------------------------------------------------------------------------------
   protected Point? mFirstPt;
   protected Point? mHoverPt;
   protected Drawing mDrawing /*= new ()*/;
   protected DrawingSurface mDwgPad;
   protected Dictionary<string, double> mTxtContents;
   protected string mPrompt;
   readonly PanWidget mPanWidget;
   readonly double mViewMargin = 20;
   protected Pline mPline = new ();


   public event PropertyChangedEventHandler PropertyChanged;

   //public Dictionary<string, double> Parameter { get; internal set; }
   #endregion
}
#endregion

#region LineBuilder -------------------------------------------------------------------------------
public class LineBuilder : Widget {

   #region Constructor ------------------------------------------------------------------------------------
   public LineBuilder (DrawingSurface dwgPad) : base (dwgPad) {
      mTxtContents = new () {
            { "X", 0 }, { "Y", 0 }, { "DX", 0 }, { "DY", 0 }, { "Length", 0 },{ "Angle", 0 }
      };
      mPrompt = "Line: Pick beginning point ";
   }
   #endregion

   #region Properties -----------------------------------------------------------------------------
   public double X { get => Parameter["X"]; set { Parameter["X"] = value; OnPropertyChanged (nameof (X)); } }
   public double Y { get => Parameter["Y"]; set { Parameter["Y"] = value; OnPropertyChanged (nameof (Y)); } }
   public double DX { get => Parameter["DX"]; set { Parameter["DX"] = value; OnPropertyChanged (nameof (DX)); } }
   public double DY { get => Parameter["DY"]; set { Parameter["DY"] = value; OnPropertyChanged (nameof (DY)); } }
   public double Angle { get => Parameter["Angle"]; set { Parameter["Angle"] = value; OnPropertyChanged (nameof (Angle)); } }
   public double Length { get => Parameter["Length"]; set { Parameter["Length"] = value; OnPropertyChanged (nameof (Length)); } }

   //public string Prompt { get => Prompt; set { Prompt = value; OnPropertyChanged (nameof (Prompt)); } }
   #endregion

   #region Implementation ---------------------------------------------------------------------------------
   public void AddInfo () {


   }

   #region Construct Entity -----------------------------------------------------------------------
   protected override Pline PointClicked (Point dwgPt) {
      if (mFirstPt is null) {
         mFirstPt = dwgPt;
         X = RoundOff (dwgPt.X); Y = RoundOff (dwgPt.Y);
         IsCompleted = false;
         mPrompt = "Line: pick end Point";
         return null;
      } else {
         IsCompleted = true;
         var firstPt = mFirstPt.Value;
         var first = new CADPoint (mFirstPt.Value.X, mFirstPt.Value.Y);
         var dwg = new CADPoint (dwgPt.X, dwgPt.Y);
         DX = RoundOff (mFirstPt.Value.X - dwgPt.X);
         DY = RoundOff (mFirstPt.Value.Y - dwgPt.Y);
         Length = RoundOff (CADPoint.Distance (first, dwg));
         Angle = Utils.RadianToDegree (dwg.Angle (first));
         mFirstPt = null;
         mPline = Pline.CreateLine (new CADPoint (firstPt.X, firstPt.Y), new (dwgPt.X, dwgPt.Y));
         mPline.Open ();
         mPline.Name = EEntityType.LINE;
         //Plines.Add (mPline);
         Pline.AllPlines.Add (mPline);
         //Plines.Add (mPline);
         if (mPline.Points.Count > 1) {
            foreach (var pt in mPline.Points) {

               var pt1 = new CADPoint (pt.X, pt.Y);
               var pt2 = new CADPoint (dwgPt.X, dwgPt.Y);
               if (CADPoint.Distance (pt1, pt2) <= 1)
                  mHoverPt = new Point (pt1.X, pt1.Y);
            }
         }
         UndoRedo.PushDrawing ();
         return mPline;
      }
   }
   #endregion


   #endregion

   #region Feedback -------------------------------------------------------------------------------
   public override void Draw (DrawingCommands drawingCommands) {
      if (mFirstPt == null || mHoverPt == null) return;
      drawingCommands.DrawLine (mFirstPt.Value, mHoverPt.Value);
   }
   #endregion

   #region Private --------------------------------------------------------------------------------
   #endregion
}
#endregion

#region RectBuilder -------------------------------------------------------------------------------
public class RectBuilder : Widget {
   #region Constructor ------------------------------------------------------------------------------------
   public RectBuilder (DrawingSurface dwgPad) : base (dwgPad) {
      mTxtContents = new () {
            { "X", 0 }, { "Y", 0 }, { "Width", 0 }, { "Height", 0 }
      };
   }
   #endregion

   #region Properties -----------------------------------------------------------------------------
   public double X { get => Parameter["X"]; set { Parameter["X"] = value; OnPropertyChanged (nameof (X)); } }
   public double Y {
      get => Parameter["Y"]; set { Parameter["Y"] = value; OnPropertyChanged (nameof (Y)); }
   }
   public double Width {
      get => Parameter["Width"]; set { Parameter["Width"] = value; OnPropertyChanged (nameof (Width)); }
   }
   public double Height {
      get => Parameter["Height"]; set { Parameter["Height"] = value; OnPropertyChanged (nameof (Height)); }
   }
   #endregion

   #region Implementation ---------------------------------------------------------------------------------

   #region Construct Entity -------------------------------------------------------------------------------
   protected override Pline PointClicked (Point dwgPt) {
      if (mFirstPt is null) {
         mFirstPt = dwgPt;
         X = RoundOff (dwgPt.X); Y = RoundOff (dwgPt.Y);
         mVertexA = mFirstPt.Value;
         mHoverPt = null;
         return null;
      } else {
         mVertexB = new Point (dwgPt.X, mVertexA.Y);
         mVertexC = new Point (dwgPt.X, dwgPt.Y);
         mVertexD = new Point (mVertexA.X, dwgPt.Y);
         var vA = new CADPoint (mVertexA.X, mVertexA.Y);
         var vB = new CADPoint (mVertexB.X, mVertexB.Y);
         var vC = new CADPoint (mVertexC.X, mVertexC.Y);
         var vD = new CADPoint (mVertexD.X, mVertexD.Y);
         mPline = Pline.CreateRectangle (vA, vB, vC, vD);
         mPline.Name = EEntityType.RECTANGLE;
         mPline.Close ();
         Pline.AllPlines.Add (mPline);
         //Plines.Add (mPline);
         Width = RoundOff (CADPoint.Distance (vA, vB));
         Height = RoundOff (CADPoint.Distance (vB, vC));
         mFirstPt = null;
         UndoRedo.PushDrawing ();
         return mPline;
      }
   }
   protected override void PointHover (Point drawingPt) {
      base.PointHover (drawingPt);
      if (mFirstPt == null) return;
      mVertexB = new Point (drawingPt.X, mVertexA.Y);
      mHoverPt = drawingPt;
      mVertexC = new Point (drawingPt.X, drawingPt.Y);
      mVertexD = new Point (mVertexA.X, drawingPt.Y);
      mDwgPad.InvalidateVisual ();
   }
   #endregion

   #region Feedback ---------------------------------------------------------------------------------------
   public override void Draw (DrawingCommands dwgCmds) {
      if (mFirstPt == null || mHoverPt == null) return;
      dwgCmds.DrawPolygon (new List<Point> { mVertexA, mVertexB, mVertexC, mVertexD });
      mHoverPt = null;
   }
   #endregion

   #endregion

   #region Private Field ----------------------------------------------------------------------------------
   Point mVertexA, mVertexB, mVertexC, mVertexD;
   #endregion
}
#endregion

#region PolyLineBuilder -------------------------------------------------------------------
public class PolyLineBuilder : LineBuilder {

   #region Constructor --------------------------------------------------------------------
   public PolyLineBuilder (DrawingSurface dwgPad) : base (dwgPad) {
   }
   #endregion

   #region Properties -----------------------------------------------------------------------------
   public List<Point> Vertices => mpLPoints;

   #endregion

   protected override Pline PointClicked (Point drawingPt) {
      var dwg = new CADPoint (drawingPt.X, drawingPt.Y);
      X = RoundOff (drawingPt.X); Y = RoundOff (drawingPt.Y);
      Vertices.Add (drawingPt);
      List<CADPoint> pts = new (mpLPoints.Select (x => new CADPoint (x.X, x.Y)));
      mPline = Pline.CreatePolyLine (pts);
      if (pts.Count > 1) {
         DX = pts[^2].X - dwg.X;
         DY = pts[^2].Y - dwg.Y;
         Length = CADPoint.Distance (dwg, pts[^2]);
         Angle = RadianToDegree (dwg.Angle (pts[^2]));

         for (int i = 0; i < pts.Count; i++) {
            //DY = drawingPt.Y - pts[^1].Y;
            // Angle = RadianToDegree (dwg.Angle(pts[^1]));
            var diff = CADPoint.Distance (pts[0], pts[^1]);
            if (diff <= 1.5) {
               pts[^1] = pts[0];
               mPline = Pline.CreatePolyLine (pts);
               Reset ();
            }
         }
      }
      //var pt2 = new CADPoint (mHoverPt.Value.X, mHoverPt.Value.Y);
      //if (l.Points.Count > 1) {
      //   foreach (var pt in pts) {
      //      if (CADPoint.Distance (pt, pt2) <= 1) 
      //         mHoverPt = new Point (pt.X, pt.Y);
      //   }
      //}
      mPline.Name = EEntityType.POLYLINE;
      mPline.Open ();
      //Plines.Add (mPline);
      Pline.AllPlines.Add (mPline);

      //l.AllPlines.Add (l);
      //if(Plines.Count>1)Plines.RemoveAt(Plines.Count - 1);
      //if (IsCompleted)Plines.Add (mPline);
      UndoRedo.PushDrawing ();

      return mPline;
   }

   protected override void DrawingHover (Point drawingPt) {
      base.DrawingHover (drawingPt);
      if (Vertices.Count > 1) {
         var pt1 = new CADPoint (Vertices[0].X, Vertices[0].Y);
         var pt2 = new CADPoint (drawingPt.X, drawingPt.Y);
         if (CADPoint.Distance (pt1, pt2) <= 1)
            mHoverPt = new Point (pt1.X, pt1.Y);
      }
   }

   #region Feedback -------------------------------------------------------------------------------
   public override void Draw (DrawingCommands dwgCmds) {
      base.Draw (dwgCmds);
      if (mpLPoints.Count > 0) dwgCmds.DrawLine (mpLPoints[^1], mHoverPt.Value);
   }

   public void Reset () {
      mIsCompleted = true;
      mpLPoints = new ();
   }
   #endregion

   #region PrivateData -------------------------------------------------------------------------------
   List<Point> mpLPoints = new ();
   bool mIsCompleted = false;
   #endregion

}
#endregion

