using BendRectifier;
namespace BendRectifierLib.Edit;
public class UndoRedo {

   #region Methods --------------------------------------------------------------------------------
   public static void Undo () {
      PushDrawing ();
      if (Pline.AllPlines.Count > 0 && sUndoStack.Count > 0) {
         var item = sUndoStack.Pop ();
         sRedoStack.Push (item);
         Pline.AllPlines.RemoveAt (Pline.AllPlines.Count - 1);
      }
   }

   public static void Redo () {
      if (sRedoStack.Count > 0) {
         var prevDrawing = sRedoStack.Pop ();
         sUndoStack.Push (prevDrawing);
         Pline.AllPlines.Add (prevDrawing);
      }
   }

   public static void PushDrawing () {
      if (Pline.AllPlines.Count > 0) sUndoStack.Push (Pline.AllPlines[^1]);
      sRedoStack.Clear ();
   }

   public static void Reset () {
      sUndoStack.Clear ();
      sRedoStack.Clear ();
   }
   #endregion

   #region PrivateFields --------------------------------------------------------------------------
   static Stack<Pline> sUndoStack = new ();
   static Stack<Pline> sRedoStack = new ();
   #endregion
}