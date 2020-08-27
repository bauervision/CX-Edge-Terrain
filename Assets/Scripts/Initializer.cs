using UnityEngine;
using UnityEngine.UI;

using MissionWeather;
public class Initializer : MonoBehaviour
{
    public GameObject InitialPanel;
    public GameObject EditorPanel;
    public GameObject ViewerPanel;
    public GameObject BackButton;
    public GameObject sky;
    public GameObject TerrainLoadPanel;


    public GameObject mapLight;
    public GameObject map;
    public GameObject horizon;
    private GameObject horizonMesh;

    public GameObject startCameraGO;
    private Camera startCamera;
    public Camera terrainCamera;

    public GameObject generateButton;

    public static bool MarkerSet = false;
    public static bool MinimumZoom = false;

    private GameObject CameraActiveCanvas;
    private Vector3 initialCameraPosition = new Vector3(-512, 1024, 512);
    private Vector3 initialCameraRotation = new Vector3(90, 180, 0);


    private void Awake()
    {
        startCamera = startCameraGO.GetComponent<Camera>();
        CameraActiveCanvas = GameObject.Find("CameraActiveCanvas");
        CameraActiveCanvas.SetActive(false);
    }


    public void LoadViewer()
    {
        map.SetActive(true);
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
        horizon.SetActive(false);


    }

    public void BackToInitial()
    {
        CameraActiveCanvas.SetActive(false);
        EditorPanel.SetActive(false);
        ViewerPanel.SetActive(false);
        TerrainLoadPanel.SetActive(false);
        BackButton.SetActive(false);
        InitialPanel.SetActive(true);
        UIManager.myAppState = UIManager.AppState.Init;
        startCamera.enabled = true;

        terrainCamera.transform.SetParent(null);

        terrainCamera.enabled = false;
        startCamera.gameObject.tag = "MainCamera";
        terrainCamera.gameObject.tag = "Untagged";

        OnlineMaps.instance.SetPositionAndZoom(0, 0, 0);
        OnlineMapsTileSetControl.instance.allowUserControl = true;
        InfinityCode.OnlineMapsExamples.DrawMarkerRange.RestoreClickHandler();

        UIManager.instance.ClearBeforeLoad();

        sky.SetActive(false);
        map.SetActive(false);
        horizon.SetActive(false);
        mapLight.SetActive(true);



        MinimumZoom = false;
        MarkerSet = false;


        // remove the 2d markers
        if (OnlineMapsMarkerManager.CountItems > 0)
        {
            OnlineMapsMarkerManager.RemoveAllItems();
        }

        // and remove the octogon shapes
        if (OnlineMapsDrawingElementManager.CountItems > 0)
        {
            OnlineMapsDrawingElementManager.RemoveAllItems();
        }

        // remove the 3d markers
        if (OnlineMapsMarker3DManager.CountItems > 0)
        {
            OnlineMapsMarker3DManager.RemoveAllItems();
        }

        if (horizonMesh != null)
        {
            print("Found HorizonMesh");
            Destroy(horizonMesh);
        }

        map.GetComponent<OnlineMapsCameraOrbit>().rotation = Vector2.zero;

        generateButton.GetComponent<Button>().enabled = false;

    }


    public void ReturnToMissionSelect()
    {
        CameraActiveCanvas.SetActive(false);
        UIManager.myAppState = UIManager.AppState.Init;
        horizon.SetActive(false);
        //UIManager.instance.viewerDropDown.value = 0;
        ViewerPanel.SetActive(true);
        startCamera.enabled = true;
        terrainCamera.enabled = false;
        if (horizonMesh != null)
        {
            print("Found HorizonMesh");
            Destroy(horizonMesh);
        }
    }

    public void GenerateEditorTerrain()
    {
        UIManager.instance.InitEditor();
        UIManager.myAppState = UIManager.AppState.Editor;
        TerrainLoadPanel.SetActive(false);
        EditorPanel.SetActive(true);
        sky.SetActive(true);
        mapLight.SetActive(false);
        horizon.SetActive(true);
        CameraActiveCanvas.SetActive(true);
        horizonMesh = GameObject.Find("HorizonMesh");

        OnlineMaps.instance.SetPositionAndZoom(WeatherManager.userLon, WeatherManager.userLat, 15);

        // stop monitoring clicks on the terrain
        map.GetComponent<GetCoords>().acceptClicks = false;
        // stop drawing the range markers
        InfinityCode.OnlineMapsExamples.DrawMarkerRange.RemoveClickHandler();
        OnlineMapsTileSetControl.instance.allowUserControl = false;
        CameraFly.isActive = true;

        startCamera.enabled = false;
        startCamera.gameObject.tag = "Untagged";
        terrainCamera.enabled = true;
        terrainCamera.gameObject.tag = "MainCamera";

        GameObject placedmarker = GameObject.Find("Markers");
        terrainCamera.transform.SetParent(placedmarker.transform);

        // Vector3 newCameraPosition = new Vector3(-516, 930, 236);
        // terrainCamera.transform.localEulerAngles = newCameraPosition;
        WeatherManager.GetLocationWeatherData();
    }

    public void LoadMissionTerrain()
    {
        UIManager.myAppState = UIManager.AppState.Viewer;
        ViewerPanel.SetActive(false);

        sky.SetActive(true);
        mapLight.SetActive(false);
        horizon.SetActive(true);
        CameraActiveCanvas.SetActive(true);
        // stop monitoring clicks on the terrain
        InfinityCode.OnlineMapsExamples.DrawMarkerRange.RemoveClickHandler();
        OnlineMapsTileSetControl.instance.allowUserControl = false;
        CameraFly.isActive = true;

        startCamera.enabled = false;
        terrainCamera.enabled = true;
        terrainCamera.gameObject.tag = "MainCamera";

        horizonMesh = GameObject.Find("HorizonMesh");
        if (horizonMesh != null)
            print("horizonMesh.activeInHierarchy " + horizonMesh.activeInHierarchy);


        WeatherManager.GetLocationWeatherData();
        UIManager.LoadMission();


    }



    public void SetMapType(int choice)
    {
        string[] choices = new string[] { "arcgis.worldimagery", "arcgis.worldtopomap", "arcgis.worldstreetmap" };

        OnlineMaps.instance.mapType = choices[choice];
    }
    private void Start()
    {
        terrainCamera.enabled = false;

        //EditorPanel.SetActive(false);
        ViewerPanel.SetActive(false);
        TerrainLoadPanel.SetActive(false);
        BackButton.SetActive(false);

        sky.SetActive(false);
        map.SetActive(false);
        horizon.SetActive(false);

        generateButton.GetComponent<Button>().enabled = false;

    }

    private void Update()
    {
        if (MinimumZoom && MarkerSet)
        {
            generateButton.GetComponent<Button>().enabled = true;
        }
    }


}