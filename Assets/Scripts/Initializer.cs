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


    public void GenerateTerrain()
    {
        TerrainLoadPanel.SetActive(false);
        sky.SetActive(true);
        mapLight.SetActive(false);
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
        TerrainGenerationPanel.SetActive(false);
        BackButton.SetActive(false);

        terrain.SetActive(false);
        sky.SetActive(false);

    }


}