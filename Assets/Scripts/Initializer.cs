using UnityEngine;
using UnityEngine.UI;


public class Initializer : MonoBehaviour
{
    public GameObject InitialPanel;
    public GameObject EditorPanel;
    public GameObject ViewerPanel;
    public GameObject BackButton;
    public GameObject terrain;
    public GameObject sky;
    public GameObject TerrainLoadPanel;
    public GameObject TerrainGenerationPanel;
    public GameObject mapLight;
    public GameObject map;
    public GameObject horizon;
    public Camera startCamera;
    public Camera terrainCamera;

    public void LoadViewer()
    {
        InitialPanel.SetActive(false);
        ViewerPanel.SetActive(true);
        BackButton.SetActive(true);
    }

    public void LoadEditor()
    {
        InitialPanel.SetActive(false);
        TerrainLoadPanel.SetActive(true);
        BackButton.SetActive(true);
        map.SetActive(true);
    }

    public void BackToInitial()
    {
        EditorPanel.SetActive(false);
        ViewerPanel.SetActive(false);
        TerrainLoadPanel.SetActive(false);
        BackButton.SetActive(false);
        InitialPanel.SetActive(true);
    }


    public void GenerateTerrain()
    {
        TerrainLoadPanel.SetActive(false);
        sky.SetActive(true);
        mapLight.SetActive(false);
        horizon.SetActive(true);

        startCamera.enabled = false;
        terrainCamera.enabled = true;
    }

    public void LoadTerrain()
    {
        TerrainGenerationPanel.SetActive(false);
        BackButton.SetActive(false);
    }

    public void SetMapType(int choice)
    {
        string[] choices = new string[] { "arcgis.worldimagery", "arcgis.worldtopomap", "arcgis.worldstreetmap" };

        OnlineMaps.instance.mapType = choices[choice];
    }
    private void Start()
    {
        startCamera.enabled = true;
        terrainCamera.enabled = false;

        //EditorPanel.SetActive(false);
        ViewerPanel.SetActive(false);
        TerrainLoadPanel.SetActive(false);
        TerrainGenerationPanel.SetActive(false);
        BackButton.SetActive(false);

        terrain.SetActive(false);
        sky.SetActive(false);
        map.SetActive(false);
        horizon.SetActive(false);

    }


}