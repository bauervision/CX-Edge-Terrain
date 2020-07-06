using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MissionWeather;


public static class SaveLoad
{
    public static Mission savedMissions = new Mission();


    // called from thw UIManager script when the user decides to save the mission
    public static void Save(Mission missionToSave)
    {
        // create our new Mission object
        Mission thisMission = new Mission();
        // set its data
        thisMission = missionToSave;
        // format and write to file
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "_savedMissions.gd");
        bf.Serialize(file, thisMission);
        file.Close();
    }

    public static Mission Load()
    {
        if (File.Exists(Application.persistentDataPath + "_savedMissions.gd"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "_savedMissions.gd", FileMode.Open);
            Mission thisMission = new Mission();
            thisMission = (Mission)bf.Deserialize(file);
            file.Close();
            return thisMission;
        }
        return null;
    }


}
