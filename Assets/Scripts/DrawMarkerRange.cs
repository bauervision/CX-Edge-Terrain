/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example how to draw a circle around a marker
    /// </summary>

    public class DrawMarkerRange : MonoBehaviour
    {
        public static DrawMarkerRange instance;
        /// <summary>
        /// Radius of the circle
        /// </summary>
        public float radiusKM = 0.1f;

        /// <summary>
        /// Number of segments
        /// </summary>
        public int segments = 32;
        public Color circleColor;

        public GameObject markerText;

        /// <summary>
        /// This method is called when a user clicks on a map
        /// </summary>
        private void OnMapClick()
        {
            // Get the coordinates under cursor
            double lng, lat;

            OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

            // Create a label for the marker.
            string label = "Mission Marker";

            // if we have more than 1 marker, remove the previous one
            if (OnlineMapsMarkerManager.CountItems > 0)
            {
                OnlineMapsMarkerManager.RemoveAllItems();
            }

            markerText.SetActive(true);
            // Create a new marker.
            OnlineMapsMarkerManager.CreateItem(lng, lat, label);

            OnlineMaps map = OnlineMaps.instance;

            // Get the coordinate at the desired distance
            double nlng, nlat;
            OnlineMapsUtils.GetCoordinateInDistance(lng, lat, radiusKM, 90, out nlng, out nlat);

            double tx1, ty1, tx2, ty2;

            // Convert the coordinate under cursor to tile position
            map.projection.CoordinatesToTile(lng, lat, 20, out tx1, out ty1);

            // Convert remote coordinate to tile position
            map.projection.CoordinatesToTile(nlng, nlat, 20, out tx2, out ty2);

            // Calculate radius in tiles
            double r = tx2 - tx1;

            // Create a new array for points
            OnlineMapsVector2d[] points = new OnlineMapsVector2d[segments];

            // Calculate a step
            double step = 360d / segments;

            // Calculate each point of circle
            for (int i = 0; i < segments; i++)
            {
                double px = tx1 + Math.Cos(step * i * OnlineMapsUtils.Deg2Rad) * r;
                double py = ty1 + Math.Sin(step * i * OnlineMapsUtils.Deg2Rad) * r;
                map.projection.TileToCoordinates(px, py, 20, out lng, out lat);
                points[i] = new OnlineMapsVector2d(lng, lat);
            }

            // Create a new polygon to draw a circle
            if (OnlineMapsDrawingElementManager.CountItems > 0)
            {
                OnlineMapsDrawingElementManager.RemoveAllItems();
            }
            OnlineMapsDrawingElementManager.AddItem(new OnlineMapsDrawingPoly(points, circleColor, 3));
        }

        /// <summary>
        /// This method is called when the script starts
        /// </summary>
        private void Start()
        {
            instance = this;
            markerText.SetActive(false);
            // Subscribe to click on map event
            OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
        }

        public static void RemoveClickHandler()
        {
            OnlineMapsControlBase.instance.OnMapClick -= instance.OnMapClick;
        }
    }
}