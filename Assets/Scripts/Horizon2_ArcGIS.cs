using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class Horizon2_ArcGIS : MonoBehaviour
{
    private const int maxDownloads = 1;

    public bool inteceptDownloadElevations = true;

    /// <summary>
    /// Number of tiles horizontally (X axis).
    /// </summary>
    [Tooltip("Number of tiles horizontally (X axis).")]
    public int countX = 3;

    /// <summary>
    /// Number of tiles vertically (Z axis).
    /// </summary>
    [Tooltip("Number of tiles vertically (Z axis).")]
    public int countY = 3;

    /// <summary>
    /// Offset of the horizon mesh along the y-axis relative to the map. Keep it negative.
    /// </summary>
    [Tooltip("Offset of the horizon mesh along the y-axis relative to the map. Keep it negative.")]
    public float positionYOffset = -5;
    public float inMapViewYOffset = -35;

    /// <summary>
    /// Offset zoom of tiles relative to map.zoom. Keep positive.
    /// </summary>
    [Tooltip("Offset zoom of tiles relative to map.zoom. Keep positive.")]
    public int zoomOffset = 3;

    /// <summary>
    /// Shader of the tiles mesh.
    /// </summary>
    [Tooltip("Shader of the tiles mesh.")]
    public Shader shader;

    /// <summary>
    /// Offset of the render queue relative to render queue of the shader.
    /// </summary>
    [Tooltip("Offset of the render queue relative to render queue of the shader.")]
    public int renderQueueOffset = 100;

    /// <summary>
    /// Tile resolution.
    /// </summary>
    [Tooltip("Tile resolution.")]
    public int resolution = 32;

    public int requestResolution = 100;

    public int mapResolution = 32;

    private OnlineMapsTile[] tiles;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] uv;
    private int[] triangles;
    private OnlineMaps map;
    private OnlineMapsTileSetControl control;
    private MeshRenderer meshRenderer;

    private Vector2 ctl;
    private Vector2 cbr;

    private Queue<OnlineMapsTile> requestQueue;
    private List<OnlineMapsWWW> elevationRequests;

    internal static OverviewElevation overviewElevation;
    private static OverviewElevation nextOverviewElevation;
    private static Horizon2_ArcGIS instance;

    private void DownloadElevation(OnlineMapsTile tile)
    {
        CData data = tile["cdata"] as CData;
        if (data != null) return;

        CData cdata = new CData();
        tile["cdata"] = cdata;
        if (!cdata.TryLoadHeights(tile))
        {
            requestQueue.Enqueue(tile);
            StartNextDownload();
        }
    }

    private void DownloadOverview()
    {
        if (nextOverviewElevation != null) return;

        int zoom = map.zoom - zoomOffset;
        if (zoom < 3) zoom = 3;

        double tx, ty;
        map.GetTilePosition(out tx, out ty, zoom);

        int tlx = Mathf.RoundToInt((float)(tx - countX));
        int tly = Mathf.RoundToInt((float)(ty - countY));

        int max = 1 << zoom;

        int brx = tlx + countX * 2;
        int bry = tly + countY * 2;

        if (tlx >= max) tlx -= max;
        if (brx >= max) brx -= max;

        if (overviewElevation != null)
        {
            if (overviewElevation.tlx == tlx && overviewElevation.tly == tly &&
                overviewElevation.brx == brx && overviewElevation.bry == bry) return;
        }

        nextOverviewElevation = new OverviewElevation(tlx, tly, brx, bry, zoom);
        nextOverviewElevation.OnDownloaded += OnOverviewDownloaded;
    }

    private void GetBordersUsingReflection()
    {
        Type type = typeof(OnlineMapsArcGISElevationManager);
        OnlineMapsArcGISElevationManager manager = OnlineMapsArcGISElevationManager.instance;
        IEnumerable<FieldInfo> fields = OnlineMapsReflectionHelper.GetFields(type, BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields)
        {
            string n = field.Name;
            if (n == "elevationX1") ctl.x = (float)field.GetValue(manager);
            else if (n == "elevationY1") ctl.y = (float)field.GetValue(manager);
            else if (n == "elevationW") cbr.x = ctl.x + (float)field.GetValue(manager);
            else if (n == "elevationH") cbr.y = ctl.y + (float)field.GetValue(manager);
        }
    }

    private void InitMesh()
    {
        GameObject go = new GameObject("HorizonMesh");
        go.transform.parent = map.gameObject.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        go.transform.localScale = Vector3.one;

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshRenderer = go.AddComponent<MeshRenderer>();
        int countTiles = countX * countY;

        mesh = new Mesh();
        mesh.name = "HorizonMesh";
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;

        tiles = new OnlineMapsTile[countTiles];

        int countVertices = (countX + 1) * (countY + 1) * resolution * resolution;
        vertices = new Vector3[countVertices];
        normals = new Vector3[countVertices];
        uv = new Vector2[countVertices];
        triangles = new int[6 * resolution * resolution];

        mesh.vertices = vertices;
        mesh.subMeshCount = countTiles;
        float r1 = resolution - 1;

        int index = 0;
        for (int i = 0; i < (countX + 1) * (countY + 1); i++)
        {
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    normals[index] = Vector3.up;
                    uv[index++] = new Vector2(x / r1, 1 - y / r1);
                }
            }
        }

        mesh.uv = uv;
        mesh.normals = normals;

        Material[] materials = new Material[countTiles];

        for (int i = 0; i < countTiles; i++)
        {
            int ti = 0;
            for (int x = 0; x < resolution - 1; x++)
            {
                for (int y = 0; y < resolution - 1; y++)
                {
                    int vi = i * resolution * resolution + x * resolution + y;
                    triangles[ti] = vi;
                    triangles[ti + 1] = vi + resolution + 1;
                    triangles[ti + 2] = vi + 1;
                    triangles[ti + 3] = vi;
                    triangles[ti + 4] = vi + resolution;
                    triangles[ti + 5] = vi + resolution + 1;
                    ti += 6;
                }
            }

            mesh.SetTriangles(triangles, i);

            Material material = new Material(shader);
            material.renderQueue = shader.renderQueue + renderQueueOffset;
            materials[i] = material;
        }

        meshRenderer.sharedMaterials = materials;

        UpdateMesh();

        mesh.RecalculateBounds();
    }

    private void OnDisable()
    {
        //meshRenderer.enabled = false;
    }

    private void OnEnable()
    {
        instance = this;
        if (meshRenderer == null) return;

        meshRenderer.enabled = true;
        OnlineMapsTile.OnTileDownloaded += OnTileDownloaded;
        if (OnlineMapsCache.instance != null)
        {
            OnlineMapsCache.instance.OnLoadedFromCache -= OnTileDownloaded;
            OnlineMapsCache.instance.OnLoadedFromCache += OnTileDownloaded;
        }
        map.OnMapUpdated += UpdateMesh;
        if (inteceptDownloadElevations && OnlineMapsElevationManagerBase.isActive)
        {
            OnlineMapsElevationManagerBase.instance.OnGetElevation -= OnGetElevation;
            OnlineMapsElevationManagerBase.instance.OnGetElevation += OnGetElevation;
        }
    }

    private void OnGetElevation(double leftLng, double topLat, double rightLng, double bottomLat)
    {
        ctl = new Vector2((float)leftLng, (float)topLat);
        cbr = new Vector2((float)rightLng, (float)bottomLat);

        double tlx, tly, brx, bry;

        map.projection.CoordinatesToTile(leftLng, topLat, map.zoom, out tlx, out tly);
        map.projection.CoordinatesToTile(rightLng, bottomLat, map.zoom, out brx, out bry);

        int scale = 1 << zoomOffset;

        int zoom = map.zoom - zoomOffset;

        int res = instance.mapResolution;
        int res2 = res - 1;
        short[,] heights = new short[res, res];
        double rx = (brx - tlx) / res2;
        double ry = (bry - tly) / res2;

        OnlineMapsTile tile = null;
        int max = 1 << zoom;

        for (int x = 0; x < res; x++)
        {
            double tx = (rx * x + tlx) / scale;

            for (int y = 0; y < res; y++)
            {
                double ty = (ry * y + tly) / scale;

                if (tile == null || tile.x != (int)tx || tile.y != (int)ty)
                {
                    tile = map.tileManager.GetTile(zoom, (int)tx, (int)ty);
                }

                if (tile == null)
                {
                    if (overviewElevation != null) heights[x, res2 - y] = (short)Mathf.Round(overviewElevation.GetElevation(tx / max, ty / max));
                    else heights[x, res2 - y] = 0;
                    continue;
                }

                CData data = tile["cdata"] as CData;
                if (data == null || !data.hasData)
                {
                    if (overviewElevation != null) heights[x, res2 - y] = (short)Mathf.Round(overviewElevation.GetElevation(tx / max, ty / max));
                    else heights[x, res2 - y] = 0;
                    continue;
                }
                heights[x, res2 - y] = (short)Mathf.Round(data.GetElevation(tx, ty));
            }
        }

        if (inteceptDownloadElevations && OnlineMapsElevationManagerBase.isActive) OnlineMapsArcGISElevationManager.instance.SetElevationData(heights);
    }

    private void OnOverviewDownloaded()
    {
        if (ctl == Vector2.zero && cbr == Vector2.zero) GetBordersUsingReflection();
        OnGetElevation(ctl.x, ctl.y, cbr.x, cbr.y);
        UpdateMesh();
    }

    private void OnTileDownloaded(OnlineMapsTile tile)
    {
        for (int i = 0; i < countX * countY; i++)
        {
            if (tiles[i] == tile)
            {
                meshRenderer.sharedMaterials[i].mainTexture = tile.texture;
                break;
            }
        }
    }

    private void Start()
    {
        elevationRequests = new List<OnlineMapsWWW>();
        requestQueue = new Queue<OnlineMapsTile>();

        OnlineMapsTileSetControl.instance.elevationResolution = 100;

        if (zoomOffset <= 0) throw new Exception("Zoom offset should be positive.");
        if (shader == null) shader = Shader.Find("Diffuse");

        map = OnlineMaps.instance;
        control = OnlineMapsTileSetControl.instance;
        OnlineMapsTile.OnTileDownloaded += OnTileDownloaded;
        if (OnlineMapsCache.instance != null) OnlineMapsCache.instance.OnLoadedFromCache += OnTileDownloaded;
        map.OnMapUpdated += UpdateMesh;
        if (inteceptDownloadElevations && OnlineMapsElevationManagerBase.isActive) OnlineMapsElevationManagerBase.instance.OnGetElevation += OnGetElevation;

        InitMesh();
    }

    private void StartNextDownload()
    {
        if (requestQueue.Count == 0) return;
        if (elevationRequests.Count == maxDownloads) return;

        OnlineMapsTile tile = requestQueue.Dequeue();
        CData data = tile["cdata"] as CData;

        string url = "https://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationData?f=json&Extent={%22spatialReference%22:{%22wkid%22:4326},%22ymin%22:" +
                     tile.bottomRight.y.ToString(OnlineMapsUtils.numberFormat) + ",%22ymax%22:" +
                     tile.topLeft.y.ToString(OnlineMapsUtils.numberFormat) + ",%22xmin%22:" +
                     tile.topLeft.x.ToString(OnlineMapsUtils.numberFormat) + ",%22xmax%22:" +
                     tile.bottomRight.x.ToString(OnlineMapsUtils.numberFormat) + "}&Rows=" + instance.requestResolution + "&Columns=" + instance.requestResolution;
        OnlineMapsWWW request = new OnlineMapsWWW(url);
        request.OnComplete += www =>
        {
            elevationRequests.Remove(request);

            if (data != null)
            {
                short[,] elevations = ParseResponse(www);
                if (elevations != null)
                {
                    data.SetHeight(elevations);
                    data.SaveHeights(tile);
                    if (ctl == Vector2.zero && cbr == Vector2.zero) GetBordersUsingReflection();
                    OnGetElevation(ctl.x, ctl.y, cbr.x, cbr.y);
                    UpdateMesh();
                }
            }

            StartNextDownload();
        };
        elevationRequests.Add(request);
    }

    private static short[,] ParseResponse(OnlineMapsWWW www)
    {
        if (www.hasError)
        {
            Debug.Log(www.error);
            return null;
        }

        string response = www.text;
        short[,] data = null;

        try
        {
            int dataIndex = response.IndexOf("\"data\":[");
            if (dataIndex == -1) return null;

            int res = instance.requestResolution;
            int res2 = res - 1;

            data = new short[res, res];
            dataIndex += 8;

            int index = 0;
            int v = 0;
            bool isNegative = false;

            for (int i = dataIndex; i < response.Length; i++)
            {
                char c = response[i];
                if (c == ',')
                {
                    int x = index % res;
                    int y = res2 - index / res;
                    if (isNegative) v = -v;
                    data[x, y] = (short)v;
                    v = 0;
                    isNegative = false;
                    index++;
                }
                else if (c == '-') isNegative = true;
                else if (c > 47 && c < 58) v = v * 10 + (c - 48);
                else break;
            }

            if (isNegative) v = -v;
            data[res2, 0] = (short)v;
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }

        return data;
    }

    private void UpdateMesh()
    {
        int zoom = map.zoom - zoomOffset;
        if (zoom < 3) zoom = 3;

        for (int i = 0; i < countX * countY; i++) if (tiles[i] != null) tiles[i].Unblock(this);

        double tx, ty;
        map.GetTilePosition(out tx, out ty, zoom);

        int itx = Mathf.RoundToInt((float)(tx - countX / 2f));
        int ity = Mathf.RoundToInt((float)(ty - countY / 2f));

        Vector3 offset = new Vector3(0, positionYOffset, 0) - transform.position;

        int max = 1 << zoom;
        Material[] materials = meshRenderer.sharedMaterials;

        float r1 = resolution - 1;
        int vi = 0;

        double tlx, tly, brx, bry;
        map.GetCorners(out tlx, out tly, out brx, out bry);
        float elevationScale = OnlineMapsElevationManagerBase.GetBestElevationYScale(tlx, tly, brx, bry);

        int ox = countY * resolution * resolution - (resolution - 1) * resolution;
        int oz = resolution * (resolution - 1) + 1;
        Vector2 sizeInScene = OnlineMapsTileSetControl.instance.sizeInScene;

        float elevationOffset = 0;
        if (OnlineMapsElevationManagerBase.isActive)
        {
            if (OnlineMapsElevationManagerBase.instance.bottomMode == OnlineMapsElevationBottomMode.minValue)
            {
                elevationOffset = OnlineMapsElevationManagerBase.instance.GetMinElevation(1 / OnlineMapsElevationManagerBase.instance.scale);
            }
            elevationScale *= OnlineMapsElevationManagerBase.instance.scale;
        }

        DownloadOverview();

        Matrix4x4 matrix = transform.worldToLocalMatrix;

        for (int x = 0; x < countX; x++)
        {
            int tileX = itx + x;
            int nextTileX = tileX + 1;
            if (tileX >= max) tileX -= max;
            if (nextTileX >= max) nextTileX -= max;

            for (int y = 0; y < countY; y++)
            {
                int tileY = ity + y;
                int nextTileY = tileY + 1;

                if (tileY >= max) tileY -= max;
                if (nextTileY >= max) nextTileY -= max;

                OnlineMapsTile tile = map.tileManager.GetTile(zoom, tileX, tileY);
                if (tile == null)
                {
                    OnlineMapsTile parentTile = map.tileManager.GetTile(zoom - 1, tileX / 2, tileY / 2);
                    tile = new OnlineMapsRasterTile(tileX, tileY, zoom, map);
                    tile.parent = parentTile;
                }
                int tileIndex = x * countY + y;
                tiles[tileIndex] = tile;
                tile.Block(this);

                CData data = tile["cdata"] as CData;
                if (data == null)
                {
                    DownloadElevation(tile);
                    data = tile["cdata"] as CData;
                }

                double px, py;

                map.projection.TileToCoordinates(tileX, tileY, zoom, out px, out py);
                Vector3 v1 = matrix.MultiplyPoint(control.GetWorldPosition(px, py) + offset);

                map.projection.TileToCoordinates(nextTileX, nextTileY, zoom, out px, out py);
                Vector3 v2 = matrix.MultiplyPoint(control.GetWorldPosition(px, py) + offset);
                Vector3 ov = (v2 - v1) / r1;

                for (int vx = 0; vx < resolution; vx++)
                {
                    for (int vz = 0; vz < resolution; vz++)
                    {
                        Vector3 v = new Vector3(ov.x * vx + v1.x, 0, ov.z * vz + v1.z);
                        if (vz == 0 && y > 0) v.y = vertices[vi - oz].y;
                        else if (vx == 0 && x > 0) v.y = vertices[vi - ox].y;
                        else
                        {
                            double evx = vx / r1;
                            double evz = vz / r1;
                            if (evx >= 1) evx = 0.999;
                            if (evz >= 1) evz = 0.999;
                            if (OnlineMapsElevationManagerBase.isActive)
                            {
                                float elevation = 0;
                                if (data.hasData) elevation = data.GetElevation(evx, evz);
                                else if (overviewElevation != null) elevation = overviewElevation.GetElevation((tileX + vx / r1) / max, (tileY + vz / r1) / max);
                                v.y = (elevation - elevationOffset) * elevationScale + offset.y;
                            }
                            else v.y = positionYOffset;

                            if (v.x <= 0 && v.x >= -sizeInScene.x &&
                                v.z >= 0 && v.z <= sizeInScene.y)
                            {
                                float rx = Mathf.Abs(sizeInScene.x / 2 + v.x) / sizeInScene.x * 2;
                                float ry = Mathf.Abs(sizeInScene.y / 2 - v.z) / sizeInScene.y * 2;
                                v.y += (1 - Mathf.Max(rx, ry)) * inMapViewYOffset;
                            }
                        }
                        vertices[vi++] = v;
                    }
                }

                materials[tileIndex].mainTexture = tile.texture;
                materials[tileIndex].color = new Color(1, 1, 1, tile.texture != null ? 1 : 0);
            }
        }

        mesh.vertices = vertices;
    }

    internal class OverviewElevation
    {
        public Action OnDownloaded;

        public int tlx;
        public int tly;
        public int brx;
        public int bry;

        public double mx1;
        public double my1;
        public double mx2;
        public double my2;
        public double mrx;
        public double mry;

        public short[,] heights;

        public OverviewElevation(int tlx, int tly, int brx, int bry, int zoom)
        {
            this.tlx = tlx;
            this.tly = tly;
            this.brx = brx;
            this.bry = bry;

            double z = 1 << zoom;
            mx1 = tlx / z;
            my1 = tly / z;
            mx2 = brx / z;
            my2 = bry / z;
            mrx = mx2 - mx1;
            mry = my2 - my1;

            double left, right, bottom, top;
            OnlineMaps.instance.projection.TileToCoordinates(tlx, tly, zoom, out left, out top);
            OnlineMaps.instance.projection.TileToCoordinates(brx, bry, zoom, out right, out bottom);

            string url = "http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/Elevation/ESRI_Elevation_World/MapServer/exts/ElevationsSOE/ElevationLayers/1/GetElevationData?f=json&Extent={%22spatialReference%22:{%22wkid%22:4326},%22ymin%22:" +
                         bottom.ToString(OnlineMapsUtils.numberFormat) + ",%22ymax%22:" +
                         top.ToString(OnlineMapsUtils.numberFormat) + ",%22xmin%22:" +
                         left.ToString(OnlineMapsUtils.numberFormat) + ",%22xmax%22:" +
                         right.ToString(OnlineMapsUtils.numberFormat) + "}&Rows=" + instance.requestResolution + "&Columns=" + instance.requestResolution;
            OnlineMapsWWW request = new OnlineMapsWWW(url);
            request.OnComplete += OnRequestComplete;
        }

        public void Dispose()
        {
            heights = null;
        }

        public float GetElevation(double mx, double my)
        {
            if (heights == null) return 0;

            int res = instance.requestResolution;
            int res2 = res - 1;

            double x1 = (mx - mx1) / mrx * res2;
            double y1 = (my - my1) / mry * res2;

            int ix1 = (int)x1;
            int iy1 = (int)y1;

            int ix2 = ix1 + 1;
            int iy2 = iy1 + 1;

            if (ix1 < 0) ix1 = 0;
            else if (ix1 > res2) ix1 = res2;

            if (iy1 < 0) iy1 = 0;
            else if (iy1 > res2) iy1 = res2;

            if (ix2 < 0)
            {
                x1 = 0;
                ix2 = 0;
            }
            else if (ix2 > res2)
            {
                x1 = res2;
                ix2 = res2;
            }

            if (iy2 < 0)
            {
                y1 = 0;
                iy2 = 0;
            }
            else if (iy2 > res2)
            {
                y1 = res2;
                iy2 = res2;
            }

            double rx = x1 - ix1;
            double ry = y1 - iy1;

            short h11 = heights[ix1, res2 - iy1];
            short h12 = heights[ix1, res2 - iy2];
            short h21 = heights[ix2, res2 - iy1];
            short h22 = heights[ix2, res2 - iy2];

            double h1 = (h21 - h11) * rx + h11;
            double h2 = (h22 - h12) * rx + h12;
            double h = (h2 - h1) * ry + h1;

            return (float)h;
        }

        private void OnRequestComplete(OnlineMapsWWW www)
        {
            heights = ParseResponse(www);

            if (overviewElevation != null) overviewElevation.Dispose();
            overviewElevation = this;
            nextOverviewElevation = null;

            if (OnDownloaded == null) OnDownloaded();
        }
    }

    internal class CData
    {
        private short[,] heights;

        public bool hasData
        {
            get { return heights != null; }
        }

        public CData()
        {

        }

        public float GetElevation(double tx, double ty)
        {
            if (heights == null) return 0;

            int res = instance.requestResolution;
            int res2 = res - 1;

            double x1 = (tx - Math.Floor(tx)) * res2;
            double y1 = (ty - Math.Floor(ty)) * res2;

            int ix1 = (int)x1;
            int iy1 = (int)y1;

            int ix2 = ix1 + 1;
            int iy2 = iy1 + 1;

            if (ix2 > res2) x1 = res2;
            if (iy2 > res2) y1 = res2;

            double rx = x1 - ix1;
            double ry = y1 - iy1;

            short h11 = heights[ix1, res2 - iy1];
            short h12 = heights[ix1, res2 - iy2];
            short h21 = heights[ix2, res2 - iy1];
            short h22 = heights[ix2, res2 - iy2];

            double h1 = (h21 - h11) * rx + h11;
            double h2 = (h22 - h12) * rx + h12;
            double h = (h2 - h1) * ry + h1;

            return (float)h;
        }

        private string GetTilePath(OnlineMapsTile tile)
        {
            return Application.persistentDataPath + "/ArcGIS Elevations/" + tile.zoom + "/" + tile.x + "/" + tile.y + ".elv";
        }

        public void SaveHeights(OnlineMapsTile tile)
        {
#if !UNITY_WEBGL
            string path = GetTilePath(tile);
            FileInfo info = new FileInfo(path);
            if (!info.Directory.Exists) info.Directory.Create();

            if (File.Exists(path)) File.Delete(path);

            FileStream stream = File.OpenWrite(path);
            BinaryWriter writer = new BinaryWriter(stream);

            for (int i = 0; i < heights.GetLength(0); i++)
            {
                for (int j = 0; j < heights.GetLength(1); j++)
                {
                    writer.Write(heights[i, j]);
                }
            }

            writer.Close();

            Debug.Log("Saved to Cache " + tile);
#endif
        }

        public void SetHeight(short[,] elevations)
        {
            heights = elevations;
        }

        public bool TryLoadHeights(OnlineMapsTile tile)
        {
#if !UNITY_WEBGL
            string path = GetTilePath(tile);
            if (!File.Exists(path)) return false;

            FileInfo info = new FileInfo(path);
            if (info.Length != instance.requestResolution * instance.requestResolution * 2) return false;

            try
            {
                short[,] hs = new short[instance.requestResolution, instance.requestResolution];
                FileStream stream = File.OpenRead(path);
                BinaryReader reader = new BinaryReader(stream);

                for (int i = 0; i < instance.requestResolution; i++)
                {
                    for (int j = 0; j < instance.requestResolution; j++)
                    {
                        hs[i, j] = reader.ReadInt16();
                    }
                }

                reader.Close();

                heights = hs;

                Debug.Log("Loaded From Cache " + tile);

                return true;
            }
            catch
            {
                return false;
            }
            
#else
            return false;
#endif
        }
    }
}