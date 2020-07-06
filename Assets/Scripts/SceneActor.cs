using UnityEngine;
using System.Collections;

[System.Serializable]
public class SceneActor
{
    public bool isBlueForce;
    public float positionX, positionY, positionZ, rotationX, rotationY, rotationZ, scaleX, scaleY, scaleZ;


    //called when the user finalizes the model
    public void SetPosition(bool isBlue, Vector3 pos, Vector3 rot, Vector3 scaling)
    {
        this.isBlueForce = isBlue;
        this.positionX = pos.x;
        this.positionY = pos.y;
        this.positionZ = pos.z;
        this.rotationX = rot.x;
        this.rotationY = rot.y;
        this.rotationZ = rot.z;
        this.scaleX = scaling.x;
        this.scaleY = scaling.y;
        this.scaleZ = scaling.z;
    }


}