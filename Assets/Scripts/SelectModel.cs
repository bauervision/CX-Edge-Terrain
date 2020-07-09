using UnityEngine;
using UnityEngine.EventSystems;

public class SelectModel : MonoBehaviour
{

    public Color hoverColor = new Color(255, 255, 255);
    private KeyCode disableDragKey = KeyCode.A;
    public bool isCube = false;
    private Color defaultColor;

    private bool isSelected = false;

    private void Start()
    {
        defaultColor = GetComponent<MeshRenderer>().material.color;
    }
    private void OnMouseOver()
    {
        GetComponent<MeshRenderer>().material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            GetComponent<MeshRenderer>().material.color = defaultColor;
        }
    }



    // mouseDown only works if collider is enabled
    private void OnMouseDown()
    {
        isSelected = !isSelected;
        if (isCube)
            GetComponent<BoxCollider>().enabled = false;
        else
            GetComponent<SphereCollider>().enabled = false;
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
        {
            MoveToMouse();

        }

        if (Input.GetMouseButtonDown(1))
        {
            isSelected = false;
            if (isCube)
            {
                GetComponent<BoxCollider>().enabled = true;
            }
            else
            {
                GetComponent<SphereCollider>().enabled = true;
            }
        }



    }

}