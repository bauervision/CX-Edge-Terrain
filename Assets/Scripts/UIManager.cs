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
    public static UIManager instance;

    public bool isDevelopmentTest = false;
    public enum AppState { Init, Editor, Viewer };
    public static AppState myAppState;

    #region PublicUIElements
    public GameObject[] spawn;
    public GameObject LibraryPanel;
    public GameObject WeatherPanel;
    public GameObject MissionPanel;
    public GameObject DeleteMissionButton;
    public GameObject NewMissionPanel;
    public GameObject SaveMissionPanel;
    public GameObject BottomPanel;
    public Text Directions;
    public Dropdown dropDown;
    public Dropdown viewerDropDown;
    public Dropdown editorDropDown;
    public Text currentMission;
    public Text currentElement;
    public Text currentElementForce;
    public Text currentMode;
    public GameObject helpGuide;
    public Text MissionButtonText;

    public static bool isEditing = false;
    public static double loadedMissionLat;
    public static double loadedMissionLon;

    public static bool readOnly = false;

    #endregion

    #region PrivateMembers




    private Canvas editorCanvas;
    private Canvas viewerCanvas;
    private bool isSceneLoaded = false;
    private static SceneActor currentSceneActor;
    private int missionIndexToLoad = 0;
    private List<Dropdown.OptionData> editor_dropDownOptions = new List<Dropdown.OptionData>();
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;

    Missions missionList;
    Mission thisMission;

    public static List<GameObject> spawnedObjects = new List<GameObject>();
    private List<SceneActor> savedObjects = new List<SceneActor>();

    [SerializeField]
    private KeyCode newObjectHotkey = KeyCode.Space;

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

    private string library5 = "Great job!\n\nTo add another model of the same type to the scene, simply re-activate placement mode with the space bar.\n\nTo add a different model, select it from the library panel first, then space bar to turn on placement mode.";

    //private string loadingData = "Loading Mission Data...";

    private string savingData = "Saving Mission Data...";
    private string savingDataSuccess = "Mission Saved Successfully!";
    private string currentElementID;
    private string currentForce; // ui field
    private string currentElementDescription;
    private string currentElementType;// ground, weapon, etc

    private bool showingHelp = true;
    public GameObject missionCamera;
    #endregion

    private void Awake()
    {
        editorCanvas = GameObject.Find("EditorCanvas").GetComponent<Canvas>();
        viewerCanvas = GameObject.Find("ViewerCanvas").GetComponent<Canvas>();
        missionCamera = GameObject.Find("TerrainCamera");

    }

    #region IEnumerators

    IEnumerator DeleteMissionData()
    {
        string url = $"https://us-central1-octo-ar-demo.cloudfunctions.net/deleteMission?name={missionList.missions[missionIndexToLoad].name}";
        var request = new UnityWebRequest(url, "DELETE");
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

                // clear the scene
                NewMission(false);
                //remove the loaded mission
                missionList.missions.RemoveAt(missionIndexToLoad);
                editor_dropDownOptions.RemoveAt(missionIndexToLoad + 1);//account for the "select Mission..." index

                Clear();
                dropDown.value = 0;// reset dropdown to initial
                viewerDropDown.value = 0;
                MissionButtonText.text = "DELETED";
                StartCoroutine(Countdown(MissionButtonText, 3, "Mission Data")); ;

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
    IEnumerator PostSavedData(bool isNewMission)
    {


        // now convert the data
        string data = JsonUtility.ToJson(thisMission);

        print(data);
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
            if (request.responseCode == 200)
            {
                HandleDirectionsText(savingDataSuccess);
                MissionPanel.SetActive(false);

                if (isNewMission)
                {
                    // add this new mission to dropdown options
                    Dropdown.OptionData newOption = new Dropdown.OptionData();
                    newOption.text = thisMission.name;
                    editor_dropDownOptions.Add(newOption);

                    // and to the missionlist
                    missionList.missions.Add(thisMission);
                    MissionButtonText.text = "SAVED!";
                    dropDown.value = editor_dropDownOptions.Count;// reset dropdown to initial
                    viewerDropDown.value = editor_dropDownOptions.Count;
                }
                else
                {
                    MissionButtonText.text = "UPDATED!";
                }

                StartCoroutine(Countdown(MissionButtonText, 2, "Mission Data")); ;
            }
            else
            {
                HandleDirectionsText("There was an error saving the mission");
                MissionButtonText.text = "ERROR!";
                StartCoroutine(Countdown(MissionButtonText, 3, "Mission Data"));
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

                // set the first option as the "Select Mission" option
                Dropdown.OptionData firstOption = new Dropdown.OptionData();
                firstOption.text = "Select a mission...";
                editor_dropDownOptions.Add(firstOption);

                // run through and populate our dropdown with saved missions
                foreach (Mission mission in missionList)
                {
                    Dropdown.OptionData newOption = new Dropdown.OptionData();
                    newOption.text = mission.name;
                    editor_dropDownOptions.Add(newOption);
                }

                dropDown.options = editor_dropDownOptions;
                viewerDropDown.options = editor_dropDownOptions;
                editorDropDown.options = editor_dropDownOptions;
                HandleDirectionsText($"All {missionList.missions.Count} missions successfully loaded!");
                MissionButtonText.text = "LOADED";
                StartCoroutine(Countdown(MissionButtonText, 2, "Mission Data"));
            }
        }
    }

    IEnumerator Countdown(Text textElement, int seconds, string newString)
    {
        int counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }
        textElement.text = newString;
    }

    #endregion

    #region PublicMethods

    public void HideAllPanels()
    {
        if (LibraryPanel.activeInHierarchy)
            LibraryPanel.SetActive(false);

        if (WeatherPanel.activeInHierarchy)
            WeatherPanel.SetActive(false);

        if (MissionPanel.activeInHierarchy)
            MissionPanel.SetActive(false);
    }

    public void ToggleLibraryPanel()
    {
        // if we are showing the library, we want to add new actors, so we arent editing anymore
        isEditing = false;

        LibraryPanel.SetActive(!LibraryPanel.activeInHierarchy);
        if (showingHelp)
            helpGuide.SetActive(true);


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

        if (helpGuide.activeInHierarchy)
            helpGuide.SetActive(false);
    }

    public void ToggleMissionPanel()
    {
        MissionPanel.SetActive(!MissionPanel.activeInHierarchy);

        if (WeatherPanel.activeInHierarchy)
            WeatherPanel.SetActive(false);

        if (helpGuide.activeInHierarchy)
            helpGuide.SetActive(false);
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
        Directions.text = "Deleting current mission...";
        MissionButtonText.text = "DELETING...";
        StartCoroutine(DeleteMissionData());
    }

    public static void DeleteThisActor(int index)
    {
        Destroy(spawnedObjects[index].transform.gameObject);
        spawnedObjects.RemoveAt(index);
    }

    // this is an update to the currently loaded missions
    public void SaveMission()
    {
        //clear out current missionActors so we have 
        thisMission.missionActors = new List<SceneActor>();
        // pull the sceneactor data off of our gameobjects so we have updated data to save
        foreach (GameObject actor in spawnedObjects)
            thisMission.missionActors.Add(actor.GetComponent<SelectModel>().mySceneData);

        thisMission.localMissionWeather = WeatherManager.localWeather;
        thisMission.missionLatitude = WeatherManager.userLat;
        thisMission.missionLongitude = WeatherManager.userLon;
        Directions.text = savingData;
        MissionButtonText.text = "UPDATING...";
        StartCoroutine(PostSavedData(false));
    }

    public void SaveNewMission()
    {
        // pull the sceneactor data off of our gameobjects
        foreach (GameObject actor in spawnedObjects)
            thisMission.missionActors.Add(actor.GetComponent<SelectModel>().mySceneData);


        // grab the terrain camera's transform data 
        thisMission.CameraPosX = (float)System.Math.Round((double)missionCamera.transform.localPosition.x, 2);
        thisMission.CameraPosY = (float)System.Math.Round((double)missionCamera.transform.localPosition.y);
        thisMission.CameraPosZ = (float)System.Math.Round((double)missionCamera.transform.localPosition.z);
        thisMission.CameraRotX = (float)System.Math.Round((double)missionCamera.transform.eulerAngles.x);
        thisMission.CameraRotY = (float)System.Math.Round((double)missionCamera.transform.eulerAngles.y);
        thisMission.CameraRotZ = (float)System.Math.Round((double)missionCamera.transform.eulerAngles.z);

        thisMission.localMissionWeather = WeatherManager.localWeather;
        thisMission.missionLatitude = WeatherManager.userLat;
        thisMission.missionLongitude = WeatherManager.userLon;
        Directions.text = savingData;
        MissionButtonText.text = "SAVING...";
        StartCoroutine(PostSavedData(true));
    }

    // triggered from setting the dropdown
    public void SetMissionToLoad(int missionIndex)
    {
        missionIndexToLoad = missionIndex - 1; //account for the "select mission" option

    }

    public void SetMissionToLoadViewer(int missionIndex)
    {
        missionIndexToLoad = missionIndex - 1; //account for the "select mission" option

        loadedMissionLat = missionList.missions[missionIndexToLoad].missionLatitude;
        loadedMissionLon = missionList.missions[missionIndexToLoad].missionLongitude;

        OnlineMaps.instance.SetPositionAndZoom(loadedMissionLon, loadedMissionLat, 15);
    }

    public static void LoadMission()
    {
        /* when we hit the load mission button, update all scene data with the desired mission */

        Mission loadedMission = instance.missionList.missions[instance.missionIndexToLoad];

        OnlineMaps.instance.SetPositionAndZoom(loadedMissionLon, loadedMissionLat, 15);

        // push the map update
        WeatherManager.SetNewCoords(loadedMission.missionLatitude, loadedMission.missionLongitude);

        // for each actor saved, instantiate the proper mesh and update its transform
        foreach (SceneActor actor in loadedMission.missionActors)
        {
            // OnlineMapsMarker3D marker3D = OnlineMapsMarker3DManager.CreateItem(actor.actorLongitude, actor.actorLatitude, instance.spawn[actor.actorIndex]);

            GameObject newActor = Instantiate(instance.spawn[actor.actorIndex], new Vector3((float)actor.positionX, (float)actor.positionY, (float)actor.positionZ), Quaternion.identity);
            newActor.transform.eulerAngles = new Vector3((float)actor.rotationX, (float)actor.rotationY, (float)actor.rotationZ);
            // make sure we turn on the collider so we can select and translate the actors
            newActor.GetComponent<BoxCollider>().enabled = true;
            spawnedObjects.Add(newActor);

        }

        if (!instance.isDevelopmentTest)
        {
            // send weather data over to the weather manager
            WeatherManager.SetWeatherData(loadedMission.localMissionWeather);
        }

        instance.currentMission.text = loadedMission.name;
        //set thisMission to what we have loaded
        instance.thisMission = loadedMission;
        instance.HandleDirectionsText($"{ loadedMission.name} loaded successfully!");
        // hide the mission and library panel once we load so user can see the whole scene view
        instance.MissionButtonText.text = "LOADED!";
        instance.StartCoroutine(instance.Countdown(instance.MissionButtonText, 3, "Mission Data"));
        instance.MissionPanel.SetActive(false);
        instance.LibraryPanel.SetActive(false);
        // finally set the camera to the saved value
        instance.missionCamera.GetComponent<CameraFly>().loadedVector3 = new Vector3(loadedMission.CameraRotY, -loadedMission.CameraRotX);

        instance.missionCamera.transform.localPosition = new Vector3(loadedMission.CameraPosX, loadedMission.CameraPosY, loadedMission.CameraPosZ);
        instance.missionCamera.transform.eulerAngles = new Vector3(loadedMission.CameraRotY, -loadedMission.CameraRotX);

        instance.isSceneLoaded = true;

    }

    public void LoadMissionEditor()
    {
        Mission loadedMission = instance.missionList.missions[instance.missionIndexToLoad];

        /* when we hit the load mission button, update all scene data with the desired mission */
        OnlineMaps.instance.SetPositionAndZoom(loadedMissionLon, loadedMissionLat, 15);

        // push the map update
        WeatherManager.SetNewCoords(loadedMission.missionLatitude, loadedMission.missionLongitude);

        // for each actor saved, instantiate the proper mesh and update its transform
        foreach (SceneActor actor in loadedMission.missionActors)
        {
            GameObject newActor = Instantiate(instance.spawn[actor.actorIndex], new Vector3((float)actor.positionX, (float)actor.positionY, (float)actor.positionZ), Quaternion.identity);
            newActor.transform.eulerAngles = new Vector3((float)actor.rotationX, (float)actor.rotationY, (float)actor.rotationZ);
            // make sure we turn on the collider so we can select and translate the actors
            newActor.GetComponent<BoxCollider>().enabled = true;
            spawnedObjects.Add(newActor);
        }

        // send weather data over to the weather manager
        if (!instance.isDevelopmentTest)
        {
            WeatherManager.SetWeatherData(loadedMission.localMissionWeather);
        }

        instance.currentMission.text = loadedMission.name;
        //set thisMission to what we have loaded
        instance.thisMission = loadedMission;
        instance.HandleDirectionsText($"{ loadedMission.name} loaded successfully!");
        // hide the mission and library panel once we load so user can see the whole scene view
        instance.MissionButtonText.text = "LOADED!";
        instance.StartCoroutine(instance.Countdown(instance.MissionButtonText, 3, "Mission Data"));
        instance.MissionPanel.SetActive(false);
        instance.LibraryPanel.SetActive(false);
        // // finally set the camera to the saved value
        instance.missionCamera.transform.localPosition = new Vector3(loadedMission.CameraPosX, loadedMission.CameraPosY, loadedMission.CameraPosZ);
        instance.missionCamera.transform.eulerAngles = new Vector3(loadedMission.CameraRotX, loadedMission.CameraRotY, loadedMission.CameraRotZ);

        instance.isSceneLoaded = true;

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
        showingHelp = value;

    }


    public void ReturnToInitState()
    {
        myAppState = AppState.Init;
    }
    #endregion



    #region PrivateMethods

    private void NewMission(bool initialLoad)
    {
        thisMission = new Mission();
        isSceneLoaded = false;
        if (currentMission != null)
            currentMission.text = "Unknown Mission Name";

        if (!initialLoad)
            HandleDirectionsText("Scene has been cleared of all data");
    }

    public void ClearBeforeLoad()
    {
        steps = -1;
        HideAllPanels(-1);
        // clear all spawned objects from the map
        GameObject[] deleteThese = GameObject.FindGameObjectsWithTag("spawnedModel");
        foreach (GameObject spawned in deleteThese)
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
        StartCoroutine(Countdown(Directions, 3, ""));
    }

    private void Start()
    {
        instance = this;
        myAppState = AppState.Init;


        // fetch the available missions
        StartCoroutine(LoadAvailableMissions("https://us-central1-octo-ar-demo.cloudfunctions.net/getAllMissions"));
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

        // if the library panel is open, display the help guide if it is toggled
        helpGuide.SetActive(((LibraryPanel.activeInHierarchy == true) && showingHelp));


    }


    private void HandleInitialState()
    {

        if (editorCanvas.enabled)
            editorCanvas.enabled = false;

        if (viewerCanvas.enabled)
            viewerCanvas.enabled = false;
    }

    private void HandleEditorState()
    {
        if (!editorCanvas.enabled)
            editorCanvas.enabled = true;

        if (viewerCanvas.enabled)
            viewerCanvas.enabled = false;

        HandleNewObjectHotkey();

        if (currentPlaceableObject != null)
        {
            MoveCurrentObjectToMouse();
            RotateFromMouseWheel();
            ReleaseIfClicked();
        }

        HandleUIUpdate();

        DeleteMissionButton.SetActive(isSceneLoaded);
        SaveMissionPanel.SetActive(isSceneLoaded);
        NewMissionPanel.SetActive(!isSceneLoaded);
        // if we are creating a new mission, shows steps for creation
        if (!isSceneLoaded)
        {
            switch (steps)
            {
                case 0: Directions.text = library1; break;
                case 1: Directions.text = library2; break;
                case 2: Directions.text = library3; break;
                case 3: Directions.text = library4; break;
                case 4: Directions.text = library5; break;
                default: break;
            }
        }
        else
        {
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

    public void InitEditor()
    {
        // editor init
        NewMission(true);
        HideAllPanels(-1);
        LibraryPanel.SetActive(false);
        WeatherPanel.SetActive(false);
        MissionPanel.SetActive(false);
        DeleteMissionButton.SetActive(false);
        helpGuide.SetActive(false);


    }
    private void HandleViewerState()
    {
        if (editorCanvas.enabled)
            editorCanvas.enabled = false;

        if (!viewerCanvas.enabled)
            viewerCanvas.enabled = true;
    }
    private void Update()
    {
        switch (myAppState)
        {
            case AppState.Init: { HandleInitialState(); break; }
            case AppState.Editor: { HandleEditorState(); break; }
            case AppState.Viewer: { HandleViewerState(); break; }
        }




    }



    private void HandleNewObjectHotkey()
    {
        if (Input.GetKeyDown(newObjectHotkey))
        {
            steps++;
            if (clickCount < 4)
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

            double lng, lat;
            OnlineMapsControlBase3D.instance.GetCoords(out lng, out lat);

            // we need to turn on the collider once we place the gameobject
            currentPlaceableObject.GetComponent<BoxCollider>().enabled = true;

            int currentID = spawnedObjects.Count - 1;

            // now handle our Sceneactor class
            SceneActor newActor = new SceneActor();
            newActor.forceType = currentForce;
            // set its transforms to currentPlaceableObject
            newActor.SetPosition(currentID, activeSpawnIndex, isBlueForceObject, currentPlaceableObject.transform.localPosition, currentPlaceableObject.transform.localEulerAngles, lat, lng);

            // and add it to the list
            savedObjects.Add(newActor);

            // store this data on the actual gameobject to retrieve later
            SelectModel.SetMySceneData(newActor);
            //clickCount = 0;
            steps = 4;
            currentPlaceableObject = null;
        }
    }

    #endregion
}