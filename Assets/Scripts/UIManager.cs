using UnityEngine;

public class UIManager : MonoBehaviour
{

    #region PublicUIElements
    public GameObject[] spawn;
    public GameObject LibraryPanel;

    #endregion

    #region PrivateMembers
    private int activeSpawnIndex;
    Vector3 MousePosition, TargetPosition;

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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
            {
                GameObject go = Instantiate(spawn[activeSpawnIndex]);
                go.transform.position = hitInfo.point;
            }
        }
    }



}