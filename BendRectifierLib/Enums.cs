namespace BendRectifierLib;
enum EGeoState {
   INITIAL,
   POINTS,
   LINES,
   BENDPARAMETER,
   BENDLINE
}

enum EDxfState {
   INITIAL,
   POLYLINE,
   VERTEX,
   BENDLINE,
   BENDPARAMETER,
   END
}
