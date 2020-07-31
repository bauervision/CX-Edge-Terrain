
using UnityEngine;
using MissionWeather;

[AddComponentMenu("Get Coords")]
public class GetCoords : MonoBehaviour
{

    private void Start()
    {
        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;

    }

    private void OnMapClick()
    {
        // Get the coordinates under the cursor.
        double lng, lat;
        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);


        //print("Clicked on lat: " + lat + " and lon: " + lng);
        WeatherManager.userLat = (float)lat;
        WeatherManager.userLon = (float)lng;
        WeatherManager.SetCoordText();

        Initializer.MarkerSet = true;

    }
}
