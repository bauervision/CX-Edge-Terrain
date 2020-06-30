using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class UIManager : MonoBehaviour
{

    #region PublicUIElements
    public GameObject[] spawn;
    public GameObject LibraryPanel;

    #endregion

    #region PrivateMembers
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    [SerializeField]
    private KeyCode newObjectHotkey = KeyCode.A;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;

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
            if (currentPlaceableObject != null)
            {
                Destroy(currentPlaceableObject);
            }
            else
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
        mouseWheelRotation += Input.mouseScrollDelta.y;
        if (Input.GetButtonDown("Fire2"))
        {
            print(mouseWheelRotation);
            currentPlaceableObject.transform.localScale += Vector3.one * (mouseWheelRotation * 0.1f);
            //currentPlaceableObject.transform.Scale(Vector3.up, mouseWheelRotation * 10f);
        }
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }

    private void ReleaseIfClicked()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentPlaceableObject = null;
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