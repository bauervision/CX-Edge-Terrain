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
        public double lat;
        public double lon;
        public string timezone;
        public float time;
        public WeatherDetails current;

        public WeatherData()
        {
            this.lat = 0.0f;
            this.lon = 0.0f;
            this.time = 8.0f;// default to 8am
            this.timezone = "Unknown";
            this.current = new WeatherDetails();
        }

    }

    [System.Serializable]
    public class WeatherDetails
    {
        public int dt;
        public string temp;
        public string humidity;
        public float clouds;
        public float visibility;
        public float wind_speed;
        public float wind_deg;
        public List<Forecast> weather;

        public WeatherDetails()
        {
            this.dt = System.DateTime.UtcNow.Hour;
            this.temp = "NA";
            this.humidity = "NA";
            this.clouds = 0.0f;
            this.visibility = 0.0f;
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
        private static WeatherManager instance;
        #region PublicMembers
        public Text forecastText;
        public Text tempText;
        public Text humidityText;
        public Text cloudsText;
        public Text windSpeedText;
        public Text windDirText;
        public Text timezoneText;

        public Text latText;
        public Text latTextPlaceholder;
        public Text lonText;
        public Text lonTextPlaceholder;
        public Text timeText;
        public GameObject NewLocationPanel;
        public Slider timeSlider;

        public static bool hasUpdated = false;
        public static int loadedTimeStamp = 0;

        #endregion

        #region PrivateMembers
        public static double userLat = 36.695620;// my house
        public static double userLon = -76.198450;

        #endregion

        public static WeatherData localWeather;
        #endregion


        private System.DateTime UnixTimeStampToDateTime(int unixTimeStamp)
        {
            System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        private float ConvertTimeToFloat(System.DateTime timestamp)
        {
            return (float)UnixTimeStampToDateTime(localWeather.current.dt).Hour + ((float)UnixTimeStampToDateTime(localWeather.current.dt).Minute * 0.01f);

        }

        private void ConvertFloatToTimestamp(double roundedValue)
        {
            // Now to set a new timestamp based on this passed in value,
            // first convert the float to a string and split it apart
            string floatString = roundedValue.ToString();
            string[] split = floatString.Split('.');
            // parse those strings into integers
            int hour = int.Parse(split[0]);
            int min = (int)Mathf.Clamp(float.Parse(split[1]), 0, 59);

            // now convert to date time to unix timestamp
            var dateTime = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, hour, min, 0, System.DateTimeKind.Unspecified);
            var dateTimeOffset = new System.DateTimeOffset(dateTime);
            var unixDateTime = (int)dateTimeOffset.ToUnixTimeSeconds();

            //int convertedTime = (int)dateTime.Subtract(new System.DateTime(1970, 1, 1)).TotalMilliseconds;
            // now update localweather.current.dt with converted unix timestamp
            localWeather.current.dt = unixDateTime;
        }

        void Start()
        {
            instance = this;
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

                    // handle the initial time load
                    if (loadedTimeStamp != 0)
                    {
                        localWeather.current.dt = loadedTimeStamp;
                    }

                    SetWeatherData();
                    SetSky();
                }
            }
        }

        public void SetTime(float value)
        {
            // drop all but 2 decimals and set the sky hour
            double roundedValue = System.Math.Round(value, 2);
            GetComponent<TOD_Sky>().Cycle.Hour = (float)roundedValue;
            timeText.text = $"Hour:{roundedValue}";
            ConvertFloatToTimestamp(roundedValue);

        }

        public void ToggleNewLocationPanel()
        {
            NewLocationPanel.SetActive(!NewLocationPanel.activeInHierarchy);
        }
        public void SetNewLat(string newLat)
        {
            userLat = double.Parse(newLat);
            var latText = GameObject.Find("SetTextLat");
            if (latText != null)
                latText.GetComponent<Text>().text = userLat.ToString();


        }

        public void SetNewLon(string newLon)
        {
            userLon = double.Parse(newLon);
            var lonText = GameObject.Find("SetTextLon");
            if (lonText != null)
                lonText.GetComponent<Text>().text = userLon.ToString();


        }



        public static void GetLocationWeatherData()
        {
            instance.NewLocationPanel.SetActive(false);
            instance.StartCoroutine(instance.GetRequest($"https://api.openweathermap.org/data/2.5/onecall?lat={userLat}&lon={userLon}&exclude=minutely,hourly,%20daily&units=imperial&appid=0ce2abbf4237a937a882f6497cb0cc92"));
        }

        public static void SetCoordText()
        {
            if (instance.latTextPlaceholder != null)
                instance.latTextPlaceholder.text = userLat.ToString();

            if (instance.lonTextPlaceholder != null)
                instance.lonTextPlaceholder.text = userLon.ToString();
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
            // since this is triggered from the UI mission load, we need to load the time that the user saved
            loadedTimeStamp = loadedWeather.current.dt;
        }

        private void SetSky()
        {
            float visibilityToMiles = localWeather.current.visibility / 1609;

            GetComponent<TOD_Sky>().World.Latitude = (float)localWeather.lat;
            GetComponent<TOD_Sky>().World.Longitude = (float)localWeather.lon;
            GetComponent<TOD_Sky>().Clouds.Coverage = localWeather.current.clouds / 100;
            GetComponent<TOD_Sky>().Atmosphere.Fogginess = visibilityToMiles / 100;// convert to percentage
            GetComponent<TOD_Sky>().Cycle.Year = System.DateTime.Now.Year;
            GetComponent<TOD_Sky>().Cycle.Month = System.DateTime.Now.Month;
            GetComponent<TOD_Sky>().Cycle.Day = System.DateTime.Now.Day;
            GetComponent<TOD_Animation>().WindDegrees = localWeather.current.wind_deg;
            GetComponent<TOD_Animation>().WindSpeed = localWeather.current.wind_speed;

            //now set the sky based on current time, or user specified time from the mission loaded
            float timeFloat = ConvertTimeToFloat(UnixTimeStampToDateTime(localWeather.current.dt));

            SetTime(timeFloat);
            timeSlider.value = timeFloat;
        }

        public static void SetNewCoords(double lat, double lon)
        {
            userLat = lat;
            userLon = lon;
            OnlineMaps.instance.SetPositionAndZoom(lon, lat, 15);
        }

        public void SetNewCoords()
        {
            GameObject setLatText = GameObject.Find("SetTextLat");
            if (setLatText != null)
            {
                GameObject.Find("SetTextLat").GetComponent<Text>().text = userLat.ToString();
                GameObject.Find("SetTextLon").GetComponent<Text>().text = userLon.ToString();
            }
            OnlineMaps.instance.SetPositionAndZoom(userLon, userLat, 15);
        }
        public void SetAlaska()
        {
            userLat = 60.256;
            userLon = -154.288;
            SetNewCoords();
        }

        public void SetDC()
        {
            userLat = 38.89;
            userLon = -77.035;
            SetNewCoords();
        }

        public void SetUK()
        {
            userLat = 53.19;
            userLon = -2.89;
            SetNewCoords();
        }

        public void SetAfghan()
        {
            userLat = 33.049;
            userLon = 65.086;
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