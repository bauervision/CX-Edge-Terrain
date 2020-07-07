using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
/* Used if not webGL 
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;*/
using MissionWeather;

public class UIManager : MonoBehaviour
{

    #region PublicUIElements
    public GameObject[] spawn;
    public GameObject LibraryPanel;
    public GameObject WeatherPanel;
    public GameObject MissionPanel;
    public Text ScalingText;
    public Text Directions;
    public Dropdown dropDown;
    public Text currentMission;

    #endregion

    #region PrivateMembers
    private int missionIndexToLoad;
    private List<Dropdown.OptionData> m_dropDownOptions = new List<Dropdown.OptionData>();
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;

    Missions missionList;
    Mission thisMission;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<SceneActor> savedObjects = new List<SceneActor>();
    private bool setRotation = false;

    [SerializeField]
    private KeyCode newObjectHotkey = KeyCode.A;

    // a gameobject will allow us to grab transform values
    private GameObject currentPlaceableObject;

    private bool isBlueForceObject;
    private float mouseWheelRotation;
    private int clickCount = 0;
    private int steps = -1;

    private string library1 = "Select an Actor to place on the terrain";
    private string library2 = "Hit Space bar to activate placement mode";
    private string library3 = "Use the mouse wheel to set +/- negative scaling. Then hit space bar again to enter rotation mode";
    private string library4 = "Continue to use the mouse wheel to now set the rotation angle of the actor and then finalize placement with the Left mouse button";
    #endregion


    #region IEnumerators

    IEnumerator PostSavedData()
    {
        string data = JsonUtility.ToJson(thisMission);
        Debug.Log(data);
        string url = $"https://us-central1-octo-ar-demo.cloudfunctions.net/addSavedMission";
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();

        Debug.Log("Status Code: " + request.responseCode);
        // add this new mission to dropdown options
        Dropdown.OptionData newOption = new Dropdown.OptionData();
        newOption.text = thisMission.name;
        m_dropDownOptions.Add(newOption);
        // and to the missionlist
        missionList.missions.Add(thisMission);
    }

    IEnumerator GetSavedData(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                var data = webRequest.downloadHandler.text;

                missionList = JsonUtility.FromJson<Missions>(data);

                // run through and populate our dropdown with saved missions
                foreach (Mission mission in missionList)
                {
                    Dropdown.OptionData newOption = new Dropdown.OptionData();
                    newOption.text = mission.name;
                    m_dropDownOptions.Add(newOption);
                }

                dropDown.options = m_dropDownOptions;



                // // for each actor saved, instantiate the proper mesh and update its transform
                foreach (SceneActor actor in missionList.missions[0].missionActors)
                {
                    GameObject newActor = Instantiate(spawn[actor.actorIndex]);
                    newActor.transform.position = new Vector3(actor.positionX, actor.positionY, actor.positionZ);
                    newActor.transform.eulerAngles = new Vector3(actor.rotationX, actor.rotationY, actor.rotationZ);
                    newActor.transform.localScale = new Vector3(actor.scaleX, actor.scaleY, actor.scaleZ);
                    spawnedObjects.Add(newActor);

                }

                // send weather data over to the weather manager
                WeatherManager.SetWeatherData(missionList.missions[0].localMissionWeather);

            }
        }
    }

    IEnumerator LoadAvailableMissions(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                var data = webRequest.downloadHandler.text;

                missionList = JsonUtility.FromJson<Missions>(data);

                // run through and populate our dropdown with saved missions
                foreach (Mission mission in missionList)
                {
                    Dropdown.OptionData newOption = new Dropdown.OptionData();
                    newOption.text = mission.name;
                    m_dropDownOptions.Add(newOption);
                }

                dropDown.options = m_dropDownOptions;

            }
        }
    }
    #endregion

    #region PublicMethods
    public void ToggleLibraryPanel()
    {
        LibraryPanel.SetActive(!LibraryPanel.activeInHierarchy);
        if (LibraryPanel.activeInHierarchy == true)
        {
            steps = 0;
        }
    }

    public void ToggleWeatherPanel()
    {
        WeatherPanel.SetActive(!WeatherPanel.activeInHierarchy);
        if (MissionPanel.activeInHierarchy)
            MissionPanel.SetActive(false);
    }

    public void ToggleMissionPanel()
    {
        MissionPanel.SetActive(!MissionPanel.activeInHierarchy);

        if (WeatherPanel.activeInHierarchy)
            WeatherPanel.SetActive(false);
    }

    public void Load_B_Cube()
    {
        steps = 1;
        HideAllPanels(0);
        isBlueForceObject = true;
    }

    public void Load_B_Sphere()
    {
        steps = 1;
        HideAllPanels(1);
        isBlueForceObject = true;
    }

    public void Load_R_Cube()
    {
        steps = 1;
        HideAllPanels(2);
        isBlueForceObject = false;
    }
    public void Load_R_Sphere()
    {
        steps = 1;
        HideAllPanels(3);
        isBlueForceObject = false;
    }


    public void Clear()
    {
        steps = 0;
        HideAllPanels(-1);
        // clear all spawned objects from the map
        foreach (GameObject spawned in spawnedObjects)
        {
            Destroy(spawned);
        }
        // clear the lists as well
        spawnedObjects.Clear();
        savedObjects.Clear();

    }

    public void SaveMission()
    {
        thisMission.missionActors = savedObjects;
        thisMission.localMissionWeather = WeatherManager.localWeather;
        // SaveLoad.Save(thisMission);
        StartCoroutine(PostSavedData());
    }

    // triggered from setting the dropdown
    public void SetMissionToLoad(int missionIndex)
    {
        missionIndexToLoad = missionIndex;
    }

    public void LoadMission()
    {
        /* WebGL doesn't use binary file saving --legacy for this demo, but worth saving for the exe version
        thisMission = SaveLoad.Load();*/

        /* when we hit the load mission button, update all scene data with the desired mission */

        // first clear out any current scene data
        Clear();
        // for each actor saved, instantiate the proper mesh and update its transform
        foreach (SceneActor actor in missionList.missions[missionIndexToLoad].missionActors)
        {
            GameObject newActor = Instantiate(spawn[actor.actorIndex]);
            newActor.transform.position = new Vector3(actor.positionX, actor.positionY, actor.positionZ);
            newActor.transform.eulerAngles = new Vector3(actor.rotationX, actor.rotationY, actor.rotationZ);
            newActor.transform.localScale = new Vector3(actor.scaleX, actor.scaleY, actor.scaleZ);
            spawnedObjects.Add(newActor);

        }

        // send weather data over to the weather manager
        WeatherManager.SetWeatherData(missionList.missions[missionIndexToLoad].localMissionWeather);
        currentMission.text = missionList.missions[missionIndexToLoad].name;


    }

    public void SetMissionName(string newName)
    {
        thisMission.name = newName;
    }
    #endregion



    #region PrivateMethods

    private void Start()
    {
        // we need to fetch the available missions
        StartCoroutine(LoadAvailableMissions("https://us-central1-octo-ar-demo.cloudfunctions.net/getSavedMissions"));

        thisMission = new Mission();

        HideAllPanels(-1);
        LibraryPanel.SetActive(false);
        WeatherPanel.SetActive(false);
        MissionPanel.SetActive(false);
        currentMission.text = "Unknown Mission Name";

    }


    private void HideAllPanels(int index)
    {
        if (index != -1)
        {
            activeSpawnIndex = index;
        }

        GameObject[] activePanels = GameObject.FindGameObjectsWithTag("activePanel");
        // turn off all panels
        foreach (GameObject panel in activePanels)
        {
            panel.SetActive(false);
        }
    }


    private void Update()
    {
        HandleNewObjectHotkey();

        if (currentPlaceableObject != null)
        {
            MoveCurrentObjectToMouse();
            RotateFromMouseWheel();
            ReleaseIfClicked();
        }

        if (LibraryPanel.activeInHierarchy == true)
        {
            switch (steps)
            {
                case 0: Directions.text = library1; break;
                case 1: Directions.text = library2; break;
                case 2: Directions.text = library3; break;
                case 3: Directions.text = library4; break;
                default: Directions.text = ""; break;
            }
        }
        else
        {
            Directions.text = "";
        }

    }

    private void HandleNewObjectHotkey()
    {
        if (Input.GetKeyDown(newObjectHotkey))
        {
            steps++;
            if (clickCount < 3)
            {
                clickCount++;
            }



            if (currentPlaceableObject != null && clickCount > 2)
            {
                Destroy(currentPlaceableObject);
                clickCount = 0;
                steps = -1;
                ScalingText.text = $"Rotation: {0}";

            }
            else if (currentPlaceableObject == null)
            {

                currentPlaceableObject = Instantiate(spawn[activeSpawnIndex]);
                spawnedObjects.Add(currentPlaceableObject);

            }
            mouseWheelRotation = 0;
        }
    }

    private void MoveCurrentObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            currentPlaceableObject.transform.position = hitInfo.point;
            currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
    }

    private void RotateFromMouseWheel()
    {
        // if we're scaling, clamp the values, otherwise leave it alone
        mouseWheelRotation += (setRotation) ? Input.mouseScrollDelta.y : Mathf.Clamp(Input.mouseScrollDelta.y, -2, 2);

        if (clickCount == 2)
        {
            steps = 3;
            ScalingText.text = $"Rotation: {mouseWheelRotation}";
            currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
        }
        else
        {
            steps = 2;
            ScalingText.text = $"Scale: {mouseWheelRotation}";
            currentPlaceableObject.transform.localScale += Vector3.one * (mouseWheelRotation * 0.05f);
        }

    }

    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // now handle our Sceneactor class
            SceneActor newActor = new SceneActor();
            // set its transforms to currentPlaceableObject
            newActor.SetPosition(isBlueForceObject, activeSpawnIndex, currentPlaceableObject.transform.position, currentPlaceableObject.transform.eulerAngles, currentPlaceableObject.transform.localScale);
            // and add it to the list
            savedObjects.Add(newActor);

            currentPlaceableObject = null;
            clickCount = 0;
            steps = -1;
        }
    }

    #endregion
}