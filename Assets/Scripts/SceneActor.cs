using UnityEngine;
using System.Collections;

[System.Serializable]
public class SceneActor
{
    public bool isBlueForce;
    public string forceType;
    public int id;
    public int actorIndex;
    public float positionX, positionY, positionZ, rotationX, rotationY, rotationZ;

    public string description;


    //called when the user finalizes the model
    public void SetPosition(int id, int index, Vector3 pos, Vector3 rot)
    {
        this.actorIndex = index;// which index is the mesh we chose?
        this.id = id; // specific id for this object index of when it was spawned in the scene.
        this.positionX = (float)System.Math.Round(pos.x, 2);
        this.positionY = (float)System.Math.Round(pos.y, 2);
        this.positionZ = (float)System.Math.Round(pos.z, 2);
        this.rotationX = (float)System.Math.Round(rot.x, 2);
        this.rotationY = (float)System.Math.Round(rot.y, 2);
        this.rotationZ = (float)System.Math.Round(rot.z, 2);
        this.description = "";
    }


}