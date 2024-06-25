using BendRectifierLib.Edit;
using BendRectifierLib.Geometry;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BendRectifier; 
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
   #region Constructor -------------------------------------------------------------------------
   public MainWindow () {
      InitializeComponent ();
      WindowState = WindowState.Maximized;
      mLine.Click += OnShapeClick;
      mRectangle.Click += OnShapeClick;
      mPolyLine.Click += OnShapeClick;
      KeyDown += OnKeyDown;
      mSave.Click += OnSave;
      mOpen.Click += OnOpen;
      mUndo.Click += OnUndo;
      mRedo.Click += OnRedo;
      mNew.Click += OnNew;
      //mCircle.Click += OnCircle;
      //mSaveButton.Click += OnSave;
      //mOpenButton.Click += OnOpen;
      //mUndoButton.Click += OnUndo;
      //mRedoButton.Click += OnRedo;
      //mNewButton.Click += OnNew;
      //mSnap.Click += OnSnap;
      //mLineMenu.Click += OnShapeClick;
      mPanWidget = new (mPad, OnPan);
      Loaded += delegate {
         var bound = new Bounding (new CADPoint (-10, -10), new CADPoint (100, 100));
         mPad.mProjXfm = Transforms.ComputeZoomExtentsProjXfm (mPad.ActualWidth, mPad.ActualHeight, bound);
         mPad.mInvProjXfm = mPad.mProjXfm; mPad.mInvProjXfm.Invert ();
         mPad.Xfm = mPad.mProjXfm;
      };

      mPad.MouseMove += OnDwgSurfaceMouseMove;
   }

   void OnNew (object sender, RoutedEventArgs e) {
      Reset ();
      mPad.InvalidateVisual ();
   }

   void OnRedo (object sender, RoutedEventArgs e) {
      UndoRedo.Redo ();
      mPad.InvalidateVisual ();
   }

   void OnUndo (object sender, RoutedEventArgs e) {
      UndoRedo.Undo ();
      mPad.InvalidateVisual ();
   }

   void OnOpen (object sender, RoutedEventArgs e) {
      Reset();
      mFileHandler.OpenFile ();
      mPad.InvalidateVisual ();
   }

   void OnSave (object sender, RoutedEventArgs e) {
     // mFileHandler.SaveFile ();
   }

   void OnKeyDown (object sender, KeyEventArgs e) {
      if (e.Key == Key.Escape) {
         ((PolyLineBuilder)mWidget).Reset ();
         //mWidget = new PolyLineBuilder (mPad);
      }
   }

   static void Reset () {
      UndoRedo.Reset ();
      Pline.Reset ();
   }
   void OnDwgSurfaceMouseMove (object sender, MouseEventArgs e) {
      Point position = e.GetPosition (mPad);
      var wcs = mPad.mInvProjXfm.Transform (position);
      UpdateStatusCoords (wcs.X, wcs.Y);
   }

   void UpdateStatusCoords (double x, double y) => statusCoords.Text = $"{x:0.##}, {y:0.##}";
   #endregion

   #region ClickEvents -------------------------------------------------------------------------
   // void OnSnap (object sender, RoutedEventArgs e) => mPad.OnSnap (sender, e);

   //void OnNew (object sender, RoutedEventArgs e) => mPad (sender, e);

   //void OnRedo (object sender, RoutedEventArgs e) => mPad.OnRedoButtonClick (sender, e);

   //void OnUndo (object sender, RoutedEventArgs e) => mPad.OnUndoButtonClick (sender, e);

   //void OnOpen (object sender, RoutedEventArgs e) => mPad.OnOpenButtonClick (sender, e);
   //void OnSave (object sender, RoutedEventArgs e) => mPad.OnSaveButtonClick (sender, e);
   void OnPan (Vector panDisp) {
      Matrix m = Matrix.Identity; m.Translate (panDisp.X, panDisp.Y);
      mPad.mProjXfm.Append (m);
      mPad.mInvProjXfm = mPad.mProjXfm; mPad.mInvProjXfm.Invert ();
      mPad.Xfm = mPad.mProjXfm;
      mPad.InvalidateVisual ();
   }

   //void OnCircle (object sender, RoutedEventArgs e) {
   //   if (mWidget is not CircleWidget) mWidget?.Detach ();
   //   mWidget = new CircleWidget (mCAD);
   //   Controller.SetDrawingInfo (mInfoPanel,new Circle().Parameter);
   //}
   void OnShapeClick (object sender, RoutedEventArgs e) {
      if (sender is FrameworkElement element && element.Tag is string tag) {
         mWidget?.Detach ();
         mWidget = tag switch {
            "Line" => new LineBuilder (mPad),
            "Rectangle" => new RectBuilder (mPad) {
            },
            "PolyLine" => new PolyLineBuilder (mPad),
            _ => throw new NotImplementedException ()
         };
         //mDrawing = new ();
         mPad.EntityBuilder = mWidget;
         mWidget.Attach ();
         UpdatePrompt (mWidget.Prompt);
         AddParameters (mWidget.Parameter);
      }
   }
   void AddParameters (Dictionary<string, double> w) {
      mInfoPanel.Children.Clear ();
      var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
      foreach (var item in w) {
         var label = new Label { Content = item.Key, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
         var textBox = new TextBox {
            Name = $"{item.Key}",
            Text = item.Value.ToString (),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness (5, 0, 0, 0),
            Height = 20,
            Width = 100,
         };
         var b = new Binding (Name = item.Key) {
            Source = mWidget,
            Mode = BindingMode.TwoWay
         };
         textBox.SetBinding (TextBox.TextProperty, b);
         stackPanel.Children.Add (label);
         stackPanel.Children.Add (textBox);
      }
      mInfoPanel.Children.Add (stackPanel);
   }

   void UpdatePrompt (string prompt) {
      mPromptingPanel.Children.Clear ();
      var label = new Label { Content = prompt };
      //var bind = new Binding (Name = prompt) {
      //   Source = mWidget,
      //};
      //label.SetBinding(Label.ContentProperty, bind);
      mPromptingPanel.Children.Add (label);
   }
   //void OnRect (object sender, RoutedEventArgs e) {
   //   //   if (mWidget is not RectangleWidget) mWidget?.Detach ();
   //   //   mWidget = new RectangleWidget (mCAD);
   //   //   Controller.SetDrawingInfo (mInfoPanel, new Rectangle().Parameter);
   //   mPad.RectBuilder = new RectBuilder (mPad);

   //   mPad.RectBuilder.Attach ();
   //}

   //void OnLine (object sender, RoutedEventArgs e) {
   //   mPad.LineBuilder = new LineBuilder (mPad);

   //   mPad.LineBuilder.Attach ();
   //   //mWidget = new LineWidget (mCAD);
   //   //mWidget = new Widget1 (mCAD);
   //   //Controller.SetDrawingInfo(mSteps, Line.Prompt);
   //   // Controller.SetDrawingInfo (mInfoPanel, (LineWidget)mWidget.m Parameter);

   //}

   #endregion

   #region Private Data ------------------------------------------------------------------------
   readonly PanWidget mPanWidget;
   Widget mWidget;
   Drawing mDrawing;
   Dictionary<string, double> p;
   FileHandler mFileHandler = new ();
   UndoRedo mUndoRedo = new ();
   #endregion
}