using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MissionWeather;

[System.Serializable]
public class Mission
{

    //set all dynamic actors that could be placed for this mission
    public List<SceneActor> missionActors;
    //store the weather for the mission
    public WeatherData localMissionWeather;
    // public Time missionTime;

    public Mission()
    {
        this.missionActors = new List<SceneActor>();
        this.localMissionWeather = new WeatherData();
    }


}