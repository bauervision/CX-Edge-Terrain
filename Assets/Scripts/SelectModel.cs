using UnityEngine;
using UnityEngine.EventSystems;

public class SelectModel : MonoBehaviour
{
    private static SelectModel instance;
    public SceneActor mySceneData;
    public static int myID;
    public Color hoverColor = new Color(255, 255, 255);
    public Color savedColor = new Color(255, 255, 255);
    private Color defaultColor;
    private float mouseWheelRotation;


    private bool isSelected = false;

    private void Awake()
    {
        instance = this;
    }
    // called from UIManager when this model is finalized
    public static void SetMySceneData(SceneActor myData)
    {
        instance.mySceneData = myData;
    }

    public void SetMyDescription(string value)
    {
        mySceneData.description = value;
    }

    private void UpdateMyPosition()
    {
        if (isSelected)
        {
            mySceneData.positionX = (float)System.Math.Round(transform.position.x, 2);
            mySceneData.positionY = (float)System.Math.Round(transform.position.y, 2);
            mySceneData.positionZ = (float)System.Math.Round(transform.position.z, 2);
            mySceneData.rotationX = (float)System.Math.Round(transform.rotation.x, 2);
            mySceneData.rotationY = (float)System.Math.Round(transform.rotation.y, 2);
            mySceneData.rotationZ = (float)System.Math.Round(transform.rotation.z, 2);
        }
    }

    private void Start()
    {
        defaultColor = GetComponent<MeshRenderer>().material.color;
    }
    private void OnMouseOver()
    {
        string isBlue = mySceneData.isBlueForce ? "Blue Force" : "Red Force";

        GetComponent<MeshRenderer>().material.color = hoverColor;
        //Debug.Log(JsonUtility.ToJson(mySceneData));
        UIManager.SetSelected(mySceneData);
        Tooltip.ShowToolTip($"id: {mySceneData.id}\n{isBlue}\n{mySceneData.forceType}");
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            GetComponent<MeshRenderer>().material.color = defaultColor;
        }
        UIManager.SetSelected(null);
        Tooltip.HideToolTip();
    }



    // mouseDown only works if collider is enabled
    private void OnMouseDown()
    {
        isSelected = !isSelected;
        // if we have clicked on this model, set edit mode 
        if (isSelected)
            UIManager.isEditing = true;

        UIManager.SetSelected(mySceneData);
        GetComponent<BoxCollider>().enabled = false;

    }

    private void MoveToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            transform.position = hitInfo.point;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
    }

    private GameObject DeleteMe()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            UIManager.DeleteThisActor(mySceneData.id);
            //Destroy(transform.gameObject);
            return null;
        }
        return transform.gameObject;
    }

    private void Update()
    {

        if (isSelected)
        {
            DeleteMe();

            MoveToMouse();
            mouseWheelRotation += Input.mouseScrollDelta.y;
            transform.Rotate(Vector3.up, mySceneData.rotationY + (mouseWheelRotation * 10f));
        }

        if (Input.GetMouseButtonDown(1))
        {
            UpdateMyPosition();
            isSelected = false;
            //UIManager.isEditing = false;
            GetComponent<BoxCollider>().enabled = true;
        }
    }
}