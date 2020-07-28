/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsExamples
{
    /// <summary>
    /// Example of how to handle change of the position and zoom the map.
    /// </summary>

    public class ZoomChanged : MonoBehaviour
    {

        public Text ZoomLevelText;
        public GameObject OptimumText;
        private void OnChangeZoom()
        {
            // When the zoom changes you will see in the console new zoom.
            ZoomLevelText.text = OnlineMaps.instance.zoom.ToString();
        }

        private void Start()
        {
            // Subscribe to change zoom event.
            OnlineMaps.instance.OnChangeZoom += OnChangeZoom;

        }

        private void Update()
        {
            if (OnlineMaps.instance.zoom > 11)
            {
                OptimumText.SetActive(true);
            }
            else
            {
                OptimumText.SetActive(false);
            }
        }
    }
}