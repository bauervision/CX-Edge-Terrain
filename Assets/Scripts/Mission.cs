using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MissionWeather;

[System.Serializable]
public class Mission
{

    //set all dynamic actors that could be placed for this mission
    public List<GameObject> missionActors;
    //store the weather for the mission
    public WeatherData localMissionWeather;

    public Mission()
    {

    }


}