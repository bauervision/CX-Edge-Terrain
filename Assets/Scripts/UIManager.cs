using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    #region PublicUIElements
    public GameObject[] spawn;
    public GameObject LibraryPanel;
    public Text ScalingText;

    #endregion

    #region PrivateMembers
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool setRotation = false;

    [SerializeField]
    private KeyCode newObjectHotkey = KeyCode.A;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;
    private int clickCount = 0;

    #endregion

    #region PublicMethods

    public void ToggleLibraryPanel()
    {
        LibraryPanel.SetActive(!LibraryPanel.activeInHierarchy);
    }

    #endregion

    #region PrivateMethods

    #endregion


    private void Start()
    {
        HideAllPanels(-1);
        LibraryPanel.SetActive(false);
    }

    public void Load_B_Cube()
    {
        HideAllPanels(0);
    }

    public void Load_B_Sphere()
    {
        HideAllPanels(1);
    }

    public void Load_R_Cube()
    {
        HideAllPanels(2);
    }
    public void Load_R_Sphere()
    {
        HideAllPanels(3);
    }


    public void Clear()
    {
        HideAllPanels(-1);
        // clear all spawned objects from the map
        foreach (GameObject spawned in spawnedObjects)
        {
            Destroy(spawned);
        }

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


    public void ToggleSetRotation()
    {
        setRotation = !setRotation;
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
    }

    private void HandleNewObjectHotkey()
    {
        if (Input.GetKeyDown(newObjectHotkey))
        {
            if (clickCount < 3)
            {
                clickCount++;
            }
            print(clickCount);
            if (currentPlaceableObject != null && clickCount > 2)
            {
                Destroy(currentPlaceableObject);
                clickCount = 0;
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
            ScalingText.text = $"Rotation: {mouseWheelRotation}";
            currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
        }
        else
        {
            ScalingText.text = $"Scale: {mouseWheelRotation}";
            currentPlaceableObject.transform.localScale += Vector3.one * (mouseWheelRotation * 0.05f);
        }

    }

    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentPlaceableObject = null;
            clickCount = 0;
        }
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         // make sure we don't spawn if over UI
    //         if (EventSystem.current.IsPointerOverGameObject())
    //             return;


    //         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //         RaycastHit hitInfo;

    //         if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
    //         {
    //             GameObject go = Instantiate(spawn[activeSpawnIndex]);
    //             go.transform.position = hitInfo.point;
    //             spawnedObjects.Add(go);
    //         }
    //     }
    // }



}