using UnityEngine;
using System.Collections;

[System.Serializable]

public class SceneActor : MonoBehaviour
{
    public bool isBlueForce;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;


    private void Start()
    {

    }

    public void SetPosition()
    {
        this.position = this.transform.position;
        this.rotation = this.transform.rotation;
        this.scale = this.transform.localScale;

        print(this.isBlueForce + "isBlue?");
        print(this.position + "position!");
        print(this.rotation + "rotation!");
        print(this.scale + "scale!");
    }


}