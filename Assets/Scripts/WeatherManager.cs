using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MissionWeather
{

    /*  =============== JSON related =====================  */
    #region JSONrelated
    [System.Serializable]
    public class WeatherData
    {
        public float lat;
        public float lon;
        public string timezone;
        public WeatherDetails current;

        public WeatherData()
        {
            this.lat = 0.0f;
            this.lon = 0.0f;
            this.timezone = "Unknown";
            this.current = new WeatherDetails();
        }

    }

    [System.Serializable]
    public class WeatherDetails
    {
        public string temp;
        public string humidity;
        public float clouds;
        public string visibility;
        public float wind_speed;
        public float wind_deg;
        public List<Forecast> weather;

        public WeatherDetails()
        {
            this.temp = "NA";
            this.humidity = "NA";
            this.clouds = 0.0f;
            this.visibility = "NA";
            this.wind_deg = 0.0f;
            this.wind_speed = 0.0f;
            this.weather = new List<Forecast>();
        }
    }

    [System.Serializable]
    public class Forecast
    {
        public string id;
        public string main;
        public Forecast()
        {
            this.id = "NA";
            this.main = "NA";
        }
    }

    public class WeatherManager : MonoBehaviour
    {
        #region PublicMembers
        public Text forecastText;
        public Text tempText;
        public Text humidityText;
        public Text cloudsText;
        public Text windSpeedText;
        public Text windDirText;
        public Text timezoneText;
        public Text latText;
        public Text lonText;
        public Text timeText;
        public GameObject NewLocationPanel;

        public static bool hasUpdated = false;

        #endregion

        #region PrivateMembers
        private float userLat = 43.148107f;
        private float userLon = -109.702438f;

        #endregion


        public static WeatherData localWeather;
        #endregion


        // Launch the fetch to the API and grab the data
        void Start()
        {
            GetLocationWeatherData();
            NewLocationPanel.SetActive(false);
        }

        IEnumerator GetRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = url.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    var data = webRequest.downloadHandler.text;
                    localWeather = JsonUtility.FromJson<WeatherData>(data);

                    SetWeatherData();
                    SetSky();
                }
            }
        }

        public void SetTime(float value)
        {
            GetComponent<TOD_Sky>().Cycle.Hour = value;
            timeText.text = $"Hour:{value}";
        }

        public void ToggleNewLocationPanel()
        {
            NewLocationPanel.SetActive(!NewLocationPanel.activeInHierarchy);
        }
        public void SetNewLat(string newLat)
        {
            userLat = float.Parse(newLat);
            GameObject.Find("SetTextLat").GetComponent<Text>().text = userLat.ToString();
        }

        public void SetNewLon(string newLon)
        {
            userLon = float.Parse(newLon);
            GameObject.Find("SetTextLon").GetComponent<Text>().text = userLon.ToString();
        }

        public void GetLocationWeatherData()
        {
            NewLocationPanel.SetActive(false);
            StartCoroutine(GetRequest($"https://api.openweathermap.org/data/2.5/onecall?lat={userLat}&lon={userLon}&exclude=minutely,hourly,%20daily&units=imperial&appid=0ce2abbf4237a937a882f6497cb0cc92"));
        }


        private void SetWeatherData()
        {
            timezoneText.text = localWeather.timezone;
            latText.text = $"{localWeather.lat} LAT";
            lonText.text = $"{localWeather.lon} LON";
            cloudsText.text = $"Clouds: {localWeather.current.clouds}%";
            forecastText.text = $"{localWeather.current.weather[0].main}";
            humidityText.text = $"Humidity: {localWeather.current.humidity}";
            tempText.text = $"Temp: {localWeather.current.temp.Trim('0')}F";
            windDirText.text = $"Wind Dir: {localWeather.current.wind_deg}";
            windSpeedText.text = $"Wind Speed: {localWeather.current.wind_speed}";
        }

        public static void SetWeatherData(WeatherData loadedWeather)
        {

            localWeather = loadedWeather;
            hasUpdated = true;
        }

        private void SetSky()
        {
            GetComponent<TOD_Sky>().World.Latitude = localWeather.lat;
            GetComponent<TOD_Sky>().World.Longitude = localWeather.lon;
            GetComponent<TOD_Sky>().Clouds.Coverage = localWeather.current.clouds / 100;
            GetComponent<TOD_Sky>().Atmosphere.Fogginess = localWeather.current.clouds / 100;
            GetComponent<TOD_Animation>().WindDegrees = localWeather.current.wind_deg;
            GetComponent<TOD_Animation>().WindSpeed = localWeather.current.wind_speed;
        }

        private void SetNewCoords()
        {
            GameObject.Find("SetTextLat").GetComponent<Text>().text = userLat.ToString();
            GameObject.Find("SetTextLon").GetComponent<Text>().text = userLon.ToString();
        }
        public void SetAlaska()
        {
            userLat = 60.256f;
            userLon = -154.288f;
            SetNewCoords();
        }

        public void SetDC()
        {
            userLat = 38.89f;
            userLon = -77.035f;
            SetNewCoords();
        }

        public void SetUK()
        {
            userLat = 53.19f;
            userLon = -2.89f;
            SetNewCoords();
        }

        public void SetAfghan()
        {
            userLat = 33.049f;
            userLon = 65.086f;
            SetNewCoords();
        }

        private void Update()
        {
            if (hasUpdated)
            {
                userLat = localWeather.lat;
                userLon = localWeather.lon;
                GetLocationWeatherData();
                hasUpdated = false;
            }
        }
    }
}