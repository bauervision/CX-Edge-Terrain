using UnityEngine;
using UnityEngine.EventSystems;

public class SelectModel : MonoBehaviour
{
    public SceneActor mySceneData;
    public static int myID;
    public Color hoverColor = new Color(255, 255, 255);
    public Color savedColor = new Color(255, 255, 255);
    private Color defaultColor;


    private bool isSelected = false;

    // called from UIManager when this model is finalized
    public void SetMySceneData(SceneActor myData)
    {
        mySceneData = myData;
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

    private void Update()
    {
        if (isSelected)
            MoveToMouse();

        if (Input.GetMouseButtonDown(1))
        {
            UpdateMyPosition();
            isSelected = false;
            GetComponent<BoxCollider>().enabled = true;
        }
    }
}