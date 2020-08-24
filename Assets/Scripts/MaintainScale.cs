using UnityEngine;

public class MaintainScale : MonoBehaviour
{
    private float offset = 70.0f;
    private void Update()
    {
        float size = (Camera.main.transform.position - transform.position).magnitude;
        transform.localScale = new Vector3(size, size, size) / offset;
    }
}