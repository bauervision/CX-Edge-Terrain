using UnityEngine;
using System.Collections;

[System.Serializable]
public class SceneActor
{
    public bool isBlueForce;
    public int actorIndex;
    public float positionX, positionY, positionZ, rotationX, rotationY, rotationZ, scaleX, scaleY, scaleZ;


    //called when the user finalizes the model
    public void SetPosition(bool isBlue, int index, Vector3 pos, Vector3 rot, Vector3 scaling)
    {
        this.isBlueForce = isBlue;// good or bad guy?
        this.actorIndex = index;// which index is the mesh we chose?
        this.positionX = (float)System.Math.Round(pos.x, 2);
        this.positionY = (float)System.Math.Round(pos.y, 2);
        this.positionZ = (float)System.Math.Round(pos.z, 2);
        this.rotationX = (float)System.Math.Round(rot.x, 2);
        this.rotationY = (float)System.Math.Round(rot.y, 2);
        this.rotationZ = (float)System.Math.Round(rot.z, 2);
        this.scaleX = (float)System.Math.Round(scaling.x, 2);
        this.scaleY = (float)System.Math.Round(scaling.y, 2);
        this.scaleZ = (float)System.Math.Round(scaling.z, 2);
    }


}