
namespace BendRectifierLib.Geometry;
public readonly struct Vector2D {
   public float X { get; }
   public float Y { get; }
   public float Length { get => MathF.Sqrt (X * X + Y * Y); }
   public float Angle { get => AngleTo (XAxis); }
   public Vector2D Normal { get => new Vector2D (X, Y) / Length; }
   public readonly Vector2D Perpendicular { get => new (-Y, X); }

   public static Vector2D Zero { get => new (0, 0); }
   public static Vector2D One { get => new (1, 1); }
   public static Vector2D XAxis { get => new (1, 0); }
   public static Vector2D YAxis { get => new (0, 1); }

   public Vector2D (float x, float y) {
      X = x;
      Y = y;
   }

   //public Vector2D Transform (Matrix2D transformation) {
   //   float x = transformation.M11 * X + transformation.M12 * Y;
   //   float y = transformation.M21 * X + transformation.M22 * Y;
   //   return new Vector2D (x, y);
   //}

   public float DotProduct (Vector2D v) => X * v.X + Y * v.Y;

   public float CrossProduct (Vector2D v) => X * v.Y - Y * v.X;

   public float AngleTo (Vector2D v) => ClampAngle (SignedAngleTo (v));

   public bool IsBetween (Vector2D a, Vector2D b) {
      float ang = ClampAngle (b.SignedAngleTo (a), true, false);
      float ang1 = ClampAngle (this.SignedAngleTo (a), true, false);
      float ang2 = ClampAngle (b.SignedAngleTo (this), true, false);
      return Math.Abs (ang2 + ang1 - ang) < 0.0001f;
   }

   float SignedAngleTo (Vector2D v) {
      float dot = this.DotProduct (v);
      float det = X * v.Y - v.X * Y;
      float ang = -MathF.Atan2 (det, dot);
      return ang;
   }

   static float ClampAngle (float ang) => ClampAngle (ang, true, true);

   private static float ClampAngle (float ang, bool low, bool high) {
      if (low) { while (ang < 0) ang += 2 * MathF.PI; }
      if (high) { while (ang > 2 * MathF.PI) ang -= 2 * MathF.PI; }
      return ang;
   }

   public static Vector2D FromAngle (float angle) => new (MathF.Cos (angle), MathF.Sin (angle));

   public static Vector2D operator + (Vector2D a, Vector2D b) => new (a.X + b.X, a.Y + b.Y);

   public static Vector2D operator - (Vector2D a, Vector2D b) => new (a.X - b.X, a.Y - b.Y);

   public static Vector2D operator * (Vector2D p, float f) => new (p.X * f, p.Y * f);

   public static Vector2D operator * (float f, Vector2D p) => new (p.X * f, p.Y * f);

   public static Vector2D operator / (Vector2D p, float f) => new (p.X / f, p.Y / f);

   public static explicit operator System.Drawing.SizeF (Vector2D a) => new (a.X, a.Y);

   public CADPoint AsPoint2D () => new (X, Y);

   public string ToString (IFormatProvider provider) => ToString ("{0:F}, {1:F}", provider);

   public string ToString (string format = "{0:F}, {1:F}", IFormatProvider provider = null) => (provider == null) ?
          string.Format (format, X, Y) :
          string.Format (provider, format, X, Y);

   //public static bool TryParse (string s, out Vector2D result) {
   //   Vector2DConverter conv = new Vector2DConverter ();
   //   if (conv.IsValid (s)) {
   //      result = (Vector2D)conv.ConvertFrom (s);
   //      return true;
   //   } else {
   //      result = Vector2D.Zero;
   //      return false;
   //   }
   //}
}
