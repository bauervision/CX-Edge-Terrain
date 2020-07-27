
using UnityEngine;
using MissionWeather;

[AddComponentMenu("CreateMarkerOnClick")]
public class GetCoords : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;
        // create the first marker
        string label = "Mission Marker";
        OnlineMapsMarkerManager.CreateItem(-76.198450, 36.695620, label);
    }

    private void OnMapClick()
    {
        // Get the coordinates under the cursor.
        double lng, lat;
        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        // Create a label for the marker.
        string label = "Mission Marker";

        // if we have more than 1 marker, remove the previous one
        if (OnlineMapsMarkerManager.CountItems > 0)
        {
            OnlineMapsMarkerManager.RemoveAllItems();
        }
        // Create a new marker.
        OnlineMapsMarkerManager.CreateItem(lng, lat, label);
        //print("Clicked on lat: " + lat + " and lon: " + lng);
        WeatherManager.userLat = (float)lat;
        WeatherManager.userLon = (float)lng;
        WeatherManager.GetLocationWeatherData();
    }
}
