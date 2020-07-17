using UnityEngine;
using UnityEngine.UI;


public class Initializer : MonoBehaviour
{
    public GameObject InitialPanel;
    public GameObject EditorPanel;
    public GameObject ViewerPanel;
    public GameObject BackButton;
    public GameObject ValidateButton;
    public GameObject GenerateButton;
    public TextAlignment RegionText;



    public GameObject TerrainLoadPanel;
    public GameObject TerrainGenerationPanel;

    private string lat = "";
    private string lon = "";

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
    }

    public void BackToInitial()
    {
        EditorPanel.SetActive(false);
        ViewerPanel.SetActive(false);
        TerrainLoadPanel.SetActive(false);
        BackButton.SetActive(false);
        InitialPanel.SetActive(true);
    }

    public void SetLat(string newLat)
    {
        lat = newLat;
    }

    public void SetLon(string newLon)
    {
        lon = newLon;
    }

    public void ValidateCoords()
    {
        print("lat: " + lat + "  lon: " + lon);
        if (lat != "" && lon != "")
            GenerateButton.GetComponent<Button>().enabled = true;
    }
    public void GenerateTerrain()
    {
        TerrainLoadPanel.SetActive(false);
    }

    public void LoadTerrain()
    {
        TerrainGenerationPanel.SetActive(false);
        BackButton.SetActive(false);
    }
    private void Start()
    {
        //EditorPanel.SetActive(false);
        ViewerPanel.SetActive(false);
        TerrainLoadPanel.SetActive(false);
        BackButton.SetActive(false);
        GenerateButton.GetComponent<Button>().enabled = false;

    }

}