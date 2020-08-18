using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MissionWeather;

[System.Serializable]
public class Missions : IEnumerable<Mission>
{
    public List<Mission> missions;

    public Missions()
    {
        List<Mission> missions = new List<Mission>();
    }

    public IEnumerator<Mission> GetEnumerator()
    {
        return missions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return missions.GetEnumerator();
    }
}


[System.Serializable]
public class Mission
{
    public string name;
    //set all dynamic actors that could be placed for this mission
    public List<SceneActor> missionActors;
    //store the weather for the mission
    public WeatherData localMissionWeather;

    public double missionLatitude;
    public double missionLongitude;
    // public Time missionTime;

    public Mission()
    {
        this.name = "MyFirstMission";
        this.missionActors = new List<SceneActor>();
        this.localMissionWeather = new WeatherData();
    }


}