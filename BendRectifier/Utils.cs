namespace BendRectifier;
public class Utils {
   public const double EPSILON = 1E-05;
   public const double PI = 3.14159265358979323846;

   public static double Clamp (double value, double min, double max) => Math.Max (Math.Min (value, max), min);

   public static double DegreeToRadian (double angle) => angle * PI / 180.0;

   public static bool IsEqual (double x, double y, double epsilon = EPSILON) => IsEqualZero (x - y, epsilon);

   public static bool IsEqualZero (double x, double epsilon = EPSILON) => Math.Abs (x) < epsilon;

   public static double RadianToDegree (double angle) => angle * 180.0 / PI;

   public static double RoundOff (double val) => Math.Round (val, 4);

   //public static double Distance (Point2D startPoint, Point2D endPoint) => (new Line (startPoint, endPoint)).length;
}
