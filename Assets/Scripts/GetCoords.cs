
using UnityEngine;
using MissionWeather;

[AddComponentMenu("Get Coords")]
public class GetCoords : MonoBehaviour
{

    public bool acceptClicks = true;
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

        WeatherManager.userLat = lat;
        WeatherManager.userLon = lng;
        WeatherManager.SetCoordText();

        //print("Lat: " + lat + " lng: " + lng);

        Initializer.MarkerSet = true;

    }

    private void Update()
    {
        if (!acceptClicks)
        {
            OnlineMapsControlBase.instance.OnMapClick -= OnMapClick;
        }
    }
}
