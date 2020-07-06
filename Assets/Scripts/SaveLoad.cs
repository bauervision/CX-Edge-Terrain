using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MissionWeather;

namespace DataSaving
{
    public static class SaveLoad
    {
        public static List<Mission> savedMissions = new List<Mission>();


        public static void SetMissionData(List<GameObject> spawnedObjects)
        {

            Mission thisMission = new Mission();
            thisMission.missionActors = spawnedObjects;
            thisMission.localMissionWeather = WeatherManager.localWeather;

            Debug.Log(thisMission.missionActors.Count);
            Debug.Log(thisMission.localMissionWeather.timezone);

        }

        public static void Save()
        {
            //savedMissions.Add(thisMission);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "_savedMissions.gd");
            bf.Serialize(file, SaveLoad.savedMissions);
            file.Close();
        }

        public static void Load()
        {
            if (File.Exists(Application.persistentDataPath + "_savedMissions.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "_savedMissions.gd", FileMode.Open);
                SaveLoad.savedMissions = (List<Mission>)bf.Deserialize(file);
                file.Close();
            }
        }


    }
}