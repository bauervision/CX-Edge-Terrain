using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MissionWeather;



public class UIManager : MonoBehaviour
{

    #region PublicUIElements
    public GameObject[] spawn;
    public GameObject LibraryPanel;
    public GameObject WeatherPanel;
    public Text ScalingText;
    public Text Directions;

    #endregion

    #region PrivateMembers
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;
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
    }
    #endregion

    #region PrivateMethods

    #endregion


    private void Start()
    {
        thisMission = new Mission();

        HideAllPanels(-1);
        LibraryPanel.SetActive(false);
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
            newActor.SetPosition(isBlueForceObject, currentPlaceableObject.transform.position, currentPlaceableObject.transform.eulerAngles, currentPlaceableObject.transform.localScale);
            // and add it to the list
            savedObjects.Add(newActor);

            currentPlaceableObject = null;
            clickCount = 0;
            steps = -1;
        }
    }


    public void SaveMission()
    {
        thisMission.missionActors = savedObjects;
        thisMission.localMissionWeather = WeatherManager.localWeather;
        SaveLoad.Save(thisMission);
    }

    public void LoadMission()
    {
        thisMission = SaveLoad.Load();
        Debug.Log("actors " + thisMission.missionActors.Count);
        Debug.Log("timezone " + thisMission.localMissionWeather.timezone);
    }



}