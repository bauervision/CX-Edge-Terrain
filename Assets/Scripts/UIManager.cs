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
    public GameObject DeleteMissionButton;
    public GameObject NewMissionPanel;
    public GameObject SaveMissionPanel;
    public Text Directions;
    public Dropdown dropDown;
    public Text currentMission;
    public Text currentElement;
    public Text currentElementForce;
    public Text currentMode;
    public GameObject helpGuide;
    public Text MissionButtonText;

    public static bool isEditing = false;

    #endregion

    #region PrivateMembers

    private bool isSceneLoaded = false;
    private static SceneActor currentSceneActor;
    private int missionIndexToLoad = 0;
    private List<Dropdown.OptionData> m_dropDownOptions = new List<Dropdown.OptionData>();
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;

    Missions missionList;
    Mission thisMission;

    public static List<GameObject> spawnedObjects = new List<GameObject>();
    private List<SceneActor> savedObjects = new List<SceneActor>();

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

    private string loadingData = "Loading Mission Data...";

    private string savingData = "Saving Mission Data...";
    private string savingDataSuccess = "Mission Saved Successfully!";
    private string currentElementID;
    private string currentForce; // ui field
    private string currentElementDescription;
    private string currentElementType;// ground, weapon, etc


    #endregion


    #region IEnumerators

    IEnumerator DeleteMissionData()
    {
        string data = JsonUtility.ToJson(thisMission);

        string url = $"https://us-central1-octo-ar-demo.cloudfunctions.net/deleteMission?name{thisMission.name}";
        var request = new UnityWebRequest(url, "DELETE");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Access-Control-Allow-Methods", "DELETE");
        request.SetRequestHeader("Access-Control-Allow-Headers", "*");
        request.SetRequestHeader("Access-Control-Allow-Origin", "https://cx-edge-terrain.web.app");

        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("Status Code: " + request.responseCode);
            if (request.responseCode == 200)
            {
                HandleDirectionsText("Mission deleted successfully!");
                MissionPanel.SetActive(false);
                MissionButtonText.text = "Mission Data";
            }
            else
            {
                HandleDirectionsText("There was an error deleting the mission!");
                MissionPanel.SetActive(false);
                MissionButtonText.text = "ERROR DELETING";
            }

        }

    }


    // PostSavedData is used to both create new missions and will also update them
    IEnumerator PostSavedData()
    {
        string data = JsonUtility.ToJson(thisMission);

        string url = $"https://us-central1-octo-ar-demo.cloudfunctions.net/addSavedMission";
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Access-Control-Allow-Methods", "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Access-Control-Allow-Headers", "*");
        request.SetRequestHeader("Access-Control-Allow-Origin", "https://cx-edge-terrain.web.app");

        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            Debug.Log("Error: " + request.error);
        }
        else
        {
            Debug.Log("Status Code: " + request.responseCode);
            HandleDirectionsText(savingDataSuccess);
            MissionPanel.SetActive(false);

            // add this new mission to dropdown options
            Dropdown.OptionData newOption = new Dropdown.OptionData();
            newOption.text = thisMission.name;
            m_dropDownOptions.Add(newOption);
            // and to the missionlist
            missionList.missions.Add(thisMission);
            MissionButtonText.text = "Mission Data";
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

                // set the first option as the "Select Mission" option
                Dropdown.OptionData firstOption = new Dropdown.OptionData();
                firstOption.text = "Select a mission...";
                m_dropDownOptions.Add(firstOption);

                // run through and populate our dropdown with saved missions
                foreach (Mission mission in missionList)
                {
                    Dropdown.OptionData newOption = new Dropdown.OptionData();
                    newOption.text = mission.name;
                    m_dropDownOptions.Add(newOption);
                }

                dropDown.options = m_dropDownOptions;
                HandleDirectionsText($"All {missionList.missions.Count} missions successfully loaded!");
                MissionButtonText.text = "Mission Data";
            }
        }
    }

    IEnumerator Countdown(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }
        Directions.text = "";
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

    public void Load_Blu_Troops()
    {
        steps = 1;
        HideAllPanels(0);
        isBlueForceObject = true;
        currentForce = "Troops";
    }

    public void Load_Blu_Armor()
    {
        steps = 1;
        HideAllPanels(1);
        isBlueForceObject = true;
        currentForce = "Armored";
    }

    public void Load_RallyPoint()
    {
        steps = 1;
        HideAllPanels(2);
        isBlueForceObject = true;
        currentForce = "Rally Point";
    }

    public void Load_Blu_Arrow()
    {
        steps = 1;
        HideAllPanels(3);
        isBlueForceObject = true;
        currentForce = "Direction";
    }

    public void Load_Objective()
    {
        steps = 1;
        HideAllPanels(4);
        isBlueForceObject = false;
        currentForce = "Objective";
    }
    public void Load_Red_Troops()
    {
        steps = 1;
        HideAllPanels(5);
        isBlueForceObject = false;
        currentForce = "Troops";
    }

    public void Load_Red_Armor()
    {
        steps = 1;
        HideAllPanels(6);
        isBlueForceObject = false;
        currentForce = "Armored";
    }

    public void Load_Red_Arrow()
    {
        steps = 1;
        HideAllPanels(7);
        isBlueForceObject = false;
        currentForce = "Direction";
    }


    // called from the UI
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
        // savedObjects.Clear();
        // create a new mission
        NewMission(false);
        currentElementID = $"id:";
        currentElementForce.text = "Element Type:";
    }

    public void DeleteMission()
    {
        // clear out the current mission and create a new empty one
        NewMission(false);
        thisMission.missionIndex = missionIndexToLoad;// set which index to update in DB
        Directions.text = "Deleting current mission...";
        MissionButtonText.text = "DELETING...";
        StartCoroutine(DeleteMissionData());
    }

    public static void DeleteThisActor(int index)
    {
        print("Delete index" + index + " of spawnedObjects");
        print("Before Delete " + spawnedObjects.Count);
        Destroy(spawnedObjects[index].transform.gameObject);
        spawnedObjects.RemoveAt(index);
        print("After Delete " + spawnedObjects.Count);
    }

    public void SaveMission()
    {
        // pull the sceneactor data off of our gameobjects
        foreach (GameObject actor in spawnedObjects)
            thisMission.missionActors.Add(actor.GetComponent<SelectModel>().mySceneData);

        print(thisMission.missionActors.Count);
        thisMission.localMissionWeather = WeatherManager.localWeather;
        thisMission.missionIndex = missionIndexToLoad;//how we know which index to update in DB
        Directions.text = savingData;
        MissionButtonText.text = "UPDATING...";
        StartCoroutine(PostSavedData());
    }

    public void SaveNewMission()
    {
        // pull the sceneactor data off of our gameobjects
        foreach (GameObject actor in spawnedObjects)
            thisMission.missionActors.Add(actor.GetComponent<SelectModel>().mySceneData);

        thisMission.localMissionWeather = WeatherManager.localWeather;
        thisMission.missionIndex = missionIndexToLoad;
        Directions.text = savingData;
        MissionButtonText.text = "SAVING...";
        StartCoroutine(PostSavedData());
    }

    // triggered from setting the dropdown
    public void SetMissionToLoad(int missionIndex)
    {
        missionIndexToLoad = missionIndex - 1; //account for the "select mission" option
    }

    public void LoadMission()
    {
        /* WebGL doesn't use binary file saving --legacy for this demo, but worth saving for the exe version
        thisMission = SaveLoad.Load();*/

        /* when we hit the load mission button, update all scene data with the desired mission */

        // first clear out any current scene data
        if (spawnedObjects.Count > 0)
        {
            ClearBeforeLoad();
        }


        // for each actor saved, instantiate the proper mesh and update its transform
        foreach (SceneActor actor in missionList.missions[missionIndexToLoad].missionActors)
        {
            GameObject newActor = Instantiate(spawn[actor.actorIndex]);
            newActor.transform.position = new Vector3(actor.positionX, actor.positionY, actor.positionZ);
            newActor.transform.eulerAngles = new Vector3(actor.rotationX, actor.rotationY, actor.rotationZ);
            // make sure we turn on the collider so we can select and translate the actors
            newActor.GetComponent<BoxCollider>().enabled = true;
            // push the saved data onto the game object
            SelectModel.SetMySceneData(actor);

            spawnedObjects.Add(newActor);

        }

        // send weather data over to the weather manager
        WeatherManager.SetWeatherData(missionList.missions[missionIndexToLoad].localMissionWeather);
        currentMission.text = missionList.missions[missionIndexToLoad].name;
        HandleDirectionsText($"{missionList.missions[missionIndexToLoad].name} loaded successfully!");
        // hide the mission and library panel once we load so user can see the whole scene view
        MissionPanel.SetActive(false);
        LibraryPanel.SetActive(false);
        isSceneLoaded = true;
    }

    public void SetMissionName(string newName)
    {
        thisMission.name = newName;
        currentMission.text = newName;
        HandleDirectionsText($"Mission name saved as {newName}!");
    }

    public static void SetSelected(SceneActor selectedActor)
    {
        currentSceneActor = selectedActor;
    }


    public void SetDescription(string description)
    {
        currentElementDescription = description;
    }

    public void ShowHelp(bool value)
    {
        helpGuide.SetActive(value);
    }

    #endregion



    #region PrivateMethods

    private void NewMission(bool initialLoad)
    {
        thisMission = new Mission();
        isSceneLoaded = false;
        currentMission.text = "Unknown Mission Name";
        if (!initialLoad)
            HandleDirectionsText("Scene has been cleared of all data");
    }

    private void ClearBeforeLoad()
    {
        steps = -1;
        HideAllPanels(-1);
        // clear all spawned objects from the map
        foreach (GameObject spawned in spawnedObjects)
        {
            Destroy(spawned);
        }
        // clear the lists as well
        spawnedObjects.Clear();
        savedObjects.Clear();
        // create a new mission
        NewMission(false);


    }
    private void HandleDirectionsText(string newText)
    {
        Directions.text = newText;
        StartCoroutine(Countdown(3));
    }

    private void Start()
    {
        // we need to fetch the available missions
        Directions.text = loadingData;
        MissionButtonText.text = "LOADING...";
        StartCoroutine(LoadAvailableMissions("https://us-central1-octo-ar-demo.cloudfunctions.net/getAllMissions"));

        NewMission(true);

        HideAllPanels(-1);
        LibraryPanel.SetActive(false);
        WeatherPanel.SetActive(false);
        MissionPanel.SetActive(false);
        DeleteMissionButton.SetActive(false);
        helpGuide.SetActive(false);
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

    private void HandleUIUpdate()
    {
        string isBlue;


        if (currentSceneActor != null)
        {
            isBlue = (isBlueForceObject || (currentSceneActor != null && currentSceneActor.isBlueForce)) ? "Blue Force" : "Red Force";

            currentElement.text = $"id:{currentSceneActor.id}\n{currentSceneActor.isBlueForce}";
            currentElementForce.text = $"Element Type:\n{currentSceneActor.forceType}";

        }
        else if (currentPlaceableObject != null)
        {
            isBlue = currentPlaceableObject.GetComponent<SelectModel>().mySceneData.isBlueForce ? "Blue Force" : "Red Force";
            currentElement.text = $"id:{spawnedObjects.Count - 1}\n{isBlue}";
            currentElementForce.text = $"Element Type:\n{currentPlaceableObject.GetComponent<SelectModel>().mySceneData.forceType}";

        }
        else
        {
            currentElement.text = "id:\n";
            currentElementForce.text = "Element Type:\n";
        }

        // if we have a mission loaded, we're in edit mode
        currentMode.text = isSceneLoaded ? "Edit\nMission" : "Create New\nMission";


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

        HandleUIUpdate();

        // if we are creating a new mission, shows steps for creation
        if (!isSceneLoaded)
        {
            DeleteMissionButton.SetActive(false);
            SaveMissionPanel.SetActive(false);
            NewMissionPanel.SetActive(true);
            switch (steps)
            {
                case 0: Directions.text = library1; break;
                case 1: Directions.text = library2; break;
                case 2: Directions.text = library3; break;
                case 3: Directions.text = library4; break;
                default: break;
            }
        }
        else
        {
            DeleteMissionButton.SetActive(true);
            SaveMissionPanel.SetActive(true);
            NewMissionPanel.SetActive(false);

            // we are editing a mission
            if (!isEditing)
            {
                Directions.text = "Mouse over a placed model to view it's details.\n Left click it to select and re-position it.";
            }
            else
            {
                Directions.text = "Right click to end re-positioning and set its new position on the map.";
            }
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


            }
            else if (currentPlaceableObject == null)
            {
                currentPlaceableObject = Instantiate(spawn[activeSpawnIndex]);
                spawnedObjects.Add(currentPlaceableObject);
            }

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
        mouseWheelRotation += Input.mouseScrollDelta.y;
        steps = 3;
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }

    /*
    ReleaseIfClicked: finalizes the placement of the model on the terrain
    */
    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // we need to turn on the collider once we place the gameobject
            currentPlaceableObject.GetComponent<BoxCollider>().enabled = true;

            int currentID = spawnedObjects.Count - 1;
            // Now set some data to retrieve from the model when hovering

            // now handle our Sceneactor class
            SceneActor newActor = new SceneActor();
            newActor.forceType = currentForce;
            // set its transforms to currentPlaceableObject
            newActor.SetPosition(currentID, activeSpawnIndex, isBlueForceObject, currentPlaceableObject.transform.position, currentPlaceableObject.transform.eulerAngles);
            // and add it to the list
            savedObjects.Add(newActor);

            // store this data on the actual gameobject to retrieve later
            SelectModel.SetMySceneData(newActor);
            currentPlaceableObject = null;
            clickCount = 0;
            steps = -1;
        }
    }

    #endregion
}