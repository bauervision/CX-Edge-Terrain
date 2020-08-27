using UnityEngine;
using System.Collections;

[System.Serializable]
public class SceneActor
{
    public bool isBlueForce;
    public string forceType;
    public int id;
    public int actorIndex;
    public double positionX, positionY, positionZ, rotationX, rotationY, rotationZ;

    public double actorLatitude;
    public double actorLongitude;
    public string description;


    //called when the user finalizes the model
    public void SetPosition(int id, int index, bool isBlue, Vector3 pos, Vector3 rot, double lat, double lng)
    {
        this.isBlueForce = isBlue;
        this.actorIndex = index;// which index is the mesh we chose?
        this.id = id; // specific id for this object index of when it was spawned in the scene.

        this.actorLatitude = lat;
        this.actorLongitude = lng;

        this.positionX = pos.x;
        this.positionY = pos.y;
        this.positionZ = pos.z;

        this.rotationX = rot.x;
        this.rotationY = rot.y;
        this.rotationZ = rot.z;

        this.description = "";
    }



}