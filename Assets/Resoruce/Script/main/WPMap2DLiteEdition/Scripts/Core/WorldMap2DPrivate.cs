// World Political Map - Globe Edition for Unity - Main Script
// Copyright 2015 Kronnect Games
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

//#define TRACE_CTL

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPMF {

    [ExecuteInEditMode]
    public partial class WorldMap2D : MonoBehaviour {

        public const int MAP_UNITY_LAYER = 5;
        // will use the UI Layer for the culling of overlay layers
        public const float MAP_PRECISION = 5000000f;

        const string OVERLAY_BASE = "OverlayLayer";
        const string OVERLAY_TEXT_ROOT = "TextRoot";
        const int MAX_POINTS_TO_TRIANGULATE = 6000;

        #region Internal variables

        // resources
        Material coloredMat, coloredAlphaMat, texturizedMat;
        Material outlineMat, cursorMat, gridMat;
        Camera _camera;

        //highlight된 object list
        Stack<GameObject> highlightStack = new Stack<GameObject>();

        // gameObjects
        GameObject _surfacesLayer;

        GameObject surfacesLayer {
            get {
                if (_surfacesLayer == null)
                    CreateSurfacesLayer();
                return _surfacesLayer;
            }
        }

        GameObject cursorLayerHLine, cursorLayerVLine, latitudeLayer, longitudeLayer;

        // caché and gameObject lifetime control
        Dictionary<int, GameObject> surfaces;
        Dictionary<Color, Material> coloredMatCache;
        Dictionary<double, Region> frontiersCacheHit;
        List<Vector3> frontiersPoints;

        // FlyTo functionality
        Quaternion flyToStartQuaternion, flyToEndQuaternion;
        Vector3 flyToStartLocation, flyToEndLocation;
        //		Vector3 lastDestinationPoint;
        bool flyToActive;
        float flyToStartTime, flyToDuration;

        // UI interaction variables
        Vector3 mouseDragStart, dragDirection;
        int dragDamping;
        float wheelAccel, dragSpeed, maxFrustumDistanceSqr, lastDistanceFromCamera;
        bool dragging;
        bool mouseIsOverUIElement;
        Vector3 lastMapPosition, lastCamPosition;

        // Overlay (Labels, tickers, ...)
        GameObject overlayLayer;
        Font labelsFont;

        public static float mapWidth { get { return WorldMap2D.instance != null ? WorldMap2D.instance.transform.localScale.x : 200.0f; } }

        public static float mapHeight { get { return WorldMap2D.instance != null ? WorldMap2D.instance.transform.localScale.y : 100.0f; } }

        Material labelsShadowMaterial;
        RenderTexture overlayRT;

        int layerMask { get { return 1 << MAP_UNITY_LAYER; } }

        #endregion



        #region Gameloop events

        void OnEnable() {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": enable wpm");
#endif

            if (countries == null) {
                Init();
            }

            // Check material
            Renderer renderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null) {
                RestyleEarth();
            }

            if (hudMatCountry != null && hudMatCountry.color != _fillColor) {
                hudMatCountry.color = _fillColor;
            }
            if (frontiersMat != null && frontiersMat.color != _frontiersColor) {
                frontiersMat.color = _frontiersColor;
            }
            if (citiesLayer != null && citiesMat.color != _citiesColor) {
                citiesMat.color = _citiesColor;
            }
            if (outlineMat.color != _outlineColor) {
                outlineMat.color = _outlineColor;
            }
            if (cursorMat.color != _cursorColor) {
                cursorMat.color = _cursorColor;
            }
            if (gridMat.color != _gridColor) {
                gridMat.color = _gridColor;
            }
        }

        void OnDestroy() {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": destroy wpm");
#endif
            if (_surfacesLayer != null)
                GameObject.DestroyImmediate(_surfacesLayer);
            overlayLayer = null;
        }

        void Reset() {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": reset");
#endif
            Redraw();
        }

        void Update() {

            // Check if navigateTo... has been called and in this case rotate the globe until the country is centered
            if (flyToActive) {
                MoveMapToDestination();
            }

            // Check whether the points is on an UI element, then cancels
            if (_respectOtherUI) {
                if (UnityEngine.EventSystems.EventSystem.current != null) {
                    if ((Input.touchSupported && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) || // mobile
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1)) { // non-mobile                        
                        if (!mouseIsOverUIElement) {                     
                            mouseIsOverUIElement = true;
                        }
                        return;
                    }
                }
            }
            mouseIsOverUIElement = false;

            // Verify if mouse enter a country boundary - we only check if mouse is inside the sphere of world
            if (mouseIsOver) {
                if (!Input.touchSupported || Input.GetMouseButtonDown(0)) {
                   //CheckMousePos();
                }
                // Remember the last country clicked
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))

                 CheckMousePos();
                _countryLastClicked = _countryHighlightedIndex;
               // Debug.Log(_countryHighlightedIndex);

            }

            // User interaction
            bool buttonLeftPressed = Input.GetMouseButton(0) && (!Input.touchSupported || Input.touchCount == 1);
            // if mouse/finger is over map, implement drag and rotation of the world
            if (mouseIsOver) {
                // Use left mouse button and drag to rotate the world
                if (_allowUserDrag && !flyToActive) {
                    if (Input.GetMouseButtonDown(0)) {
                        mouseDragStart = Input.mousePosition;
                        dragSpeed = Mathf.Sqrt(lastDistanceFromCamera) * _mouseDragSensitivity * 0.35f;
                        dragging = true;
                    }

                    // Use right mouse button and drag to spin the world around z-axis
                    if (Input.GetMouseButtonDown(1)) {
                        if (_countryHighlightedIndex >= 0 && Input.GetMouseButtonDown(1) && _centerOnRightClick) {
                            FlyToCountry(_countryHighlightedIndex, 0.8f);
                        }
                    }
                }
            }

            if (dragging) {
                if (buttonLeftPressed) {
                    dragDirection = (mouseDragStart - Input.mousePosition) * 0.001f * dragSpeed;
                    cameraMain.transform.Translate(dragDirection);
                    dragDamping = 1;
                } else
                    dragging = false;
            }

            if (dragDamping > 0 && !buttonLeftPressed) {
                if (++dragDamping < 20) {
                    dragging = true;
                    cameraMain.transform.Translate(dragDirection / dragDamping, Space.Self);
                } else {
                    dragDamping = 0;
                }
            }

            // Use mouse wheel to zoom in and out
            if (allowUserZoom && (mouseIsOver || wheelAccel != 0)) {
                float wheel = Input.GetAxis("Mouse ScrollWheel");
                wheelAccel += wheel;

                // Support for pinch on mobile
                if (Input.touchSupported && Input.touchCount == 2) {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    // Pass the delta to the wheel accel
                    wheelAccel += deltaMagnitudeDiff;
                }

                if (wheelAccel != 0) {
                    dragDamping = 0;
                    wheelAccel = Mathf.Clamp(wheelAccel, -0.1f, 0.1f);
                    if (wheelAccel >= 0.01f || wheelAccel <= -0.01f) {
                        Vector3 dest;
                        GetLocalHitFromMousePos(out dest);
                        if (dest != Vector3.zero)
                            dest = transform.TransformPoint(dest);
                        else
                            dest = transform.position;
                        Vector3 camPos = cameraMain.transform.position - (dest - cameraMain.transform.position) * wheelAccel * _mouseWheelSensitivity;
                        cameraMain.transform.position = camPos;
                        float minDistance = 0.01f * transform.localScale.y;
                        float camDistance = (dest - cameraMain.transform.position).sqrMagnitude;
                        // Last distance
                        lastDistanceFromCamera = camDistance;
                        if (camDistance < minDistance) {
                            cameraMain.transform.position = dest - (dest - cameraMain.transform.position).normalized * minDistance;
                        } else if (camDistance > maxFrustumDistanceSqr) {
                            cameraMain.transform.position = dest - (dest - cameraMain.transform.position).normalized * Mathf.Sqrt(maxFrustumDistanceSqr);
                        }
                        wheelAccel /= 1.15f;
                    } else {
                        wheelAccel = 0;
                    }
                }
            }

            // Check boundaries
            if (transform.position != lastMapPosition || cameraMain.transform.position != lastCamPosition) {
                float limitLeft, limitRight;
                if (_fitWindowWidth) {
                    limitLeft = 0;
                    limitRight = 1.0f;
                } else {
                    limitLeft = 0.9f;
                    limitRight = 0.1f;
                }

                // Reduce floating-point errors
                Vector3 apos = transform.position;
                Vector3 posEdge = transform.TransformPoint(0.5f, 0, 0);
                Vector3 pos = cameraMain.WorldToViewportPoint(posEdge);
                if (pos.x < limitRight) {
                    pos.x = limitRight;
                    pos = cameraMain.ViewportToWorldPoint(pos);
                    cameraMain.transform.position += (posEdge - pos);
                    dragDamping = 0;
                } else {
                    posEdge = transform.TransformPoint(-0.5f, 0, 0);
                    pos = cameraMain.WorldToViewportPoint(posEdge);
                    if (pos.x > limitLeft) {
                        pos.x = limitLeft;
                        pos = cameraMain.ViewportToWorldPoint(pos);
                        cameraMain.transform.position += (posEdge - pos);
                    }
                }

                float limitTop, limitBottom;
                if (_fitWindowHeight) {
                    limitTop = 1.0f;
                    limitBottom = 0;
                } else {
                    limitTop = 0.1f;
                    limitBottom = 0.9f;
                }

                posEdge = transform.TransformPoint(0, 0.5f, 0);
                pos = cameraMain.WorldToViewportPoint(posEdge);
                if (pos.y < limitTop) {
                    pos.y = limitTop;
                    pos = cameraMain.ViewportToWorldPoint(pos);
                    cameraMain.transform.position += (posEdge - pos);
                } else {
                    posEdge = transform.TransformPoint(0, -0.5f, 0);
                    pos = cameraMain.WorldToViewportPoint(posEdge);
                    if (pos.y > limitBottom) {
                        pos.y = limitBottom;
                        pos = cameraMain.ViewportToWorldPoint(pos);
                        cameraMain.transform.position += (posEdge - pos);
                    }
                }
            }
        }

        public void OnMouseEnter() {
            mouseIsOver = true;
        }

        public void OnMouseExit() {
            mouseIsOver = false;
            HideCountryRegionHighlight();
        }

        #endregion

        #region System initialization

        public void Init() {
            // Load materials
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": init");
#endif
            gameObject.layer = MAP_UNITY_LAYER;

            // Labels materials
            ReloadFont();

            // Map materials
            frontiersMat = Instantiate(Resources.Load<Material>("Materials/Frontiers"));
            frontiersMat.hideFlags = HideFlags.DontSave;
            hudMatCountry = Instantiate(Resources.Load<Material>("Materials/HudCountry"));
            hudMatCountry.hideFlags = HideFlags.DontSave;
            citySpot = Resources.Load<GameObject>("Prefabs/CitySpot");
            citiesMat = Instantiate(Resources.Load<Material>("Materials/Cities"));
            citiesMat.hideFlags = HideFlags.DontSave;
            outlineMat = Instantiate(Resources.Load<Material>("Materials/Outline"));
            outlineMat.hideFlags = HideFlags.DontSave;
            coloredMat = Instantiate(Resources.Load<Material>("Materials/ColorizedRegion"));
            coloredMat.hideFlags = HideFlags.DontSave;
            coloredAlphaMat = Instantiate(Resources.Load<Material>("Materials/ColorizedTranspRegion"));
            coloredAlphaMat.hideFlags = HideFlags.DontSave;
            texturizedMat = Instantiate(Resources.Load<Material>("Materials/TexturizedRegion"));
            texturizedMat.hideFlags = HideFlags.DontSave;
            cursorMat = Instantiate(Resources.Load<Material>("Materials/Cursor"));
            cursorMat.hideFlags = HideFlags.DontSave;
            gridMat = Instantiate(Resources.Load<Material>("Materials/Grid"));
            gridMat.hideFlags = HideFlags.DontSave;

            coloredMatCache = new Dictionary<Color, Material>();

            ReloadData();
        }



        Camera cameraMain {
            get {
                if (_camera == null) {
                    _camera = GetCamera();
                }
                return _camera;
            }
        }


        public static Camera GetCamera() {
            Camera cam = Camera.main;
            if (cam == null) {
                Camera[] cameras = FindObjectsOfType<Camera>();
                for (int k = 0; k < cameras.Length; k++) {
                    if (cameras[k].enabled) {
                        return cameras[k];
                    }
                }
            }
            return cam;
        }

        void ReloadFont() {
            if (_countryLabelsFont == null) {
                labelsFont = Instantiate(Resources.Load<Font>("Font/Lato"));
            } else {
                labelsFont = Instantiate(_countryLabelsFont);
            }
            labelsFont.hideFlags = HideFlags.DontSave;

            Material fontMaterial = Instantiate(Resources.Load<Material>("Materials/Font")); // this material is linked to a shader that has into account zbuffer
            if (labelsFont.material != null) {
                fontMaterial.mainTexture = labelsFont.material.mainTexture;
            }
            fontMaterial.hideFlags = HideFlags.DontSave;
            labelsFont.material = fontMaterial;
            labelsShadowMaterial = GameObject.Instantiate(fontMaterial);
            labelsShadowMaterial.hideFlags = HideFlags.DontSave;
            labelsShadowMaterial.renderQueue--;
        }


        /// <summary>
        /// Reloads the data of frontiers and cities from datafiles and redraws the map.
        /// </summary>
        public void ReloadData() {
            // Destroy surfaces layer
            DestroySurfaces();
            // read baked data
            ReadCountriesPackedString();
            ReadCitiesPackedString();

            // Redraw frontiers and cities -- destroy layers if they already exists
            Redraw();
        }


        void DestroySurfaces() {
            HideCountryRegionHighlights(true);
            if (frontiersCacheHit != null)
                frontiersCacheHit.Clear();
            if (surfaces != null)
                surfaces.Clear();
            if (_surfacesLayer != null)
                DestroyImmediate(_surfacesLayer);
        }


        void ReadCitiesPackedString() {
            string cityCatalogFileName = "Geodata/cities110";
            TextAsset ta = Resources.Load<TextAsset>(cityCatalogFileName);
            string s = ta.text;

            string[] cityList = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            int cityCount = cityList.Length;
            cities = new List<City>(cityCount);
            for (int k = 0; k < cityCount; k++) {
                string[] cityInfo = cityList[k].Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                string name = cityInfo[0];
                string country = cityInfo[1];
                int population = int.Parse(cityInfo[2]);
                float x = float.Parse(cityInfo[3]);
                float y = float.Parse(cityInfo[4]);
                City city = new City(name, country, population, new Vector3(x, y));
                cities.Add(city);
            }
        }

        #endregion

        #region Drawing stuff

        /// <summary>
        /// Used internally and by other components to redraw the layers in specific moments. You shouldn't call this method directly.
        /// </summary>
        public void Redraw() {
            if (!gameObject.activeInHierarchy)
                return;
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": Redraw");
#endif
            // Initialize surface cache
            if (surfaces != null) {
                List<GameObject> cached = new List<GameObject>(surfaces.Values);
                for (int k = 0; k < cached.Count; k++)
                    if (cached[k] != null)
                        DestroyImmediate(cached[k]);
            }
            surfaces = new Dictionary<int, GameObject>();

            RestyleEarth(); // Apply texture to Earth

            DrawFrontiers();    // Redraw frontiers -- the next method is also called from width property when this is changed

            DrawCities();       // Redraw cities layer

            DrawCursor();       // Draw cursor lines

            DrawGrid();     // Draw longitude & latitude lines

            DestroyMapLabels(); // Clear and optionally draw the map labels
            if (_showCountryNames)
                DrawMapLabels();

            if (lastDistanceFromCamera == 0)
                lastDistanceFromCamera = (transform.position - cameraMain.transform.position).sqrMagnitude;

        }

        void CreateSurfacesLayer() {
            Transform t = transform.Find("Surfaces");
            if (t != null) {
                DestroyImmediate(t.gameObject);
                for (int k = 0; k < countries.Length; k++)
                    for (int r = 0; r < countries[k].regions.Count; r++)
                        countries[k].regions[r].customMaterial = null;
            }
            _surfacesLayer = new GameObject("Surfaces");
            _surfacesLayer.transform.SetParent(transform, false);
            _surfacesLayer.transform.localPosition = Vector3.back * 0.01f;
            _surfacesLayer.layer = gameObject.layer;
        }

        void RestyleEarth() {
            if (gameObject == null)
                return;

            string materialName;
            switch (_earthStyle) {
                case EARTH_STYLE.SolidColor:
                    materialName = "EarthSolidColor";
                    break;
                default:
                    materialName = "Earth";
                    break;
            }
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null || !renderer.sharedMaterial.name.Equals(materialName)) {
                Material earthMaterial = Instantiate(Resources.Load<Material>("Materials/" + materialName));
                earthMaterial.hideFlags = HideFlags.DontSave;
                if (_earthStyle == EARTH_STYLE.SolidColor) {
                    earthMaterial.color = _earthColor;
                }
                earthMaterial.name = materialName;
                renderer.material = earthMaterial;
            }
        }

        #endregion



        #region Highlighting

        bool GetLocalHitFromMousePos(out Vector3 localPoint) {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = cameraMain.ScreenPointToRay(mousePos);
            RaycastHit[] hits = Physics.RaycastAll(cameraMain.transform.position, ray.direction, 500, layerMask);
            if (hits.Length > 0) {
                for (int k = 0; k < hits.Length; k++) {
                    if (hits[k].collider.gameObject == gameObject) {
                        localPoint = gameObject.transform.InverseTransformPoint(hits[k].point);
                        return true;
                    }
                }
            }
            localPoint = Vector3.zero;
            return false;
        }


        //스택에 있는 모든 highlight 
        void HideHighlightAll()
        {
            while (highlightStack.Count > 0)
            {
                highlightStack.Pop().SetActive(false);
            }
            _countryHighlighted = null;
            _countryHighlightedIndex = -1;
            _countryRegionHighlighted = null;
            _countryRegionHighlightedIndex = -1;
        }
        void CheckMousePos() {
            Vector3 localPoint;
            bool goodHit = GetLocalHitFromMousePos(out localPoint);
            if (goodHit) {
                // Cursor follow
                if (_cursorFollowMouse && _showCursor) {
                    cursorLocation = localPoint;
                }
                // Verify if a city is hit
                if (_showCities) {
                    int c = GetCityNearPoint(localPoint);
                    if (c >= 0) {
                        cityHighlighted = cities[c];
                    } else {
                        cityHighlighted = null;
                    }
                }
                // verify if hitPos is inside any country polygon
                for (int c = 0; c < countries.Length; c++) {
                    for (int cr = 0; cr < countries[c].regions.Count; cr++) {
                        if (ContainsPoint(localPoint, countries[c].regions[cr].points, countries[c].regions[cr].points.Length)) { // regionPolygon, regionPolygonPointCount)) {
                            if (countries[c] != _countryHighlighted) {

                                //  HideCountryRegionHighlight();
                                HideHighlightAll();
                                _countryHighlighted = countries[c];     // HighlightCountryRegion also set these fields
                                _countryHighlightedIndex = c;           // but we do it here because we want to know this indexes
                                _countryRegionHighlighted = countries[c].regions[cr];   // even if showProvinces is true (ie. country will not get highlighted)
                                _countryRegionHighlightedIndex = cr;        // but this way we know both, the province highlighted indexes and the country's
                            }
                            //국가 내 지역 모두 highlight
                            for (int cr2 = 0; cr2 < countries[c].regions.Count; cr2++) {
                                highlightStack.Push(HighlightCountryRegion(c, cr2, false, _showOutline));
                            }
                            
                            return;
                        }
                    }
                }
            }
            // HideCountryRegionHighlight();
            HideHighlightAll();
        }

        #endregion


        #region Geometric functions

        bool ContainsPoint(Vector2 p, Vector3[] poly, int numPoints) {
            int j = numPoints - 1;
            bool inside = false;
            for (int i = 0; i < numPoints; j = i++) {
                if (((poly[i].y <= p.y && p.y < poly[j].y) || (poly[j].y <= p.y && p.y < poly[i].y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                    inside = !inside;
            }
            return inside;
        }


        #endregion

        #region Internal API area

        /// <summary>
        /// Returns the overlay base layer (parent gameObject), useful to overlay stuff on the map (like labels). It will be created if it doesn't exist.
        /// </summary>
        public GameObject GetOverlayLayer(bool createIfNotExists) {
            if (overlayLayer != null) {
                return overlayLayer;
            } else if (createIfNotExists) {
                return CreateOverlay();
            } else {
                return null;
            }
        }

        void SetDestination(Vector3 point, float duration) {
            flyToStartQuaternion = cameraMain.transform.rotation;
            flyToStartLocation = cameraMain.transform.position;
            flyToEndQuaternion = transform.rotation;
            Plane plane = new Plane(-transform.forward, transform.position);
            float distance = plane.GetDistanceToPoint(cameraMain.transform.position);
            flyToEndLocation = transform.TransformPoint(point) - transform.forward * distance;
            flyToDuration = duration;
            flyToActive = true;
            flyToStartTime = Time.time;
            if (flyToDuration == 0)
                MoveMapToDestination();
        }

        /// <summary>
        /// Used internally to rotate the globe during FlyTo operations. Use FlyTo method.
        /// </summary>
        void MoveMapToDestination() {
            float delta;
            Quaternion rotation;
            Vector3 destination;
            if (flyToDuration == 0) {
                delta = flyToDuration;
                rotation = flyToEndQuaternion;
                destination = flyToEndLocation;
            } else {
                delta = Time.time - flyToStartTime;
                float t = Mathf.SmoothStep(0, 1, delta / flyToDuration);
                rotation = Quaternion.Lerp(flyToStartQuaternion, flyToEndQuaternion, t);
                destination = Vector3.Lerp(flyToStartLocation, flyToEndLocation, t);
            }
            cameraMain.transform.rotation = rotation;
            cameraMain.transform.position = destination;
            if (delta >= flyToDuration) {
                flyToActive = false;
            }
        }

        Material GetColoredTexturedMaterial(Color color, Texture2D texture) {
            if (texture == null && coloredMatCache.ContainsKey(color)) {
                return coloredMatCache[color];
            } else {
                Material customMat;
                if (texture != null) {
                    customMat = Instantiate(texturizedMat);
                    customMat.name = texturizedMat.name;
                    customMat.mainTexture = texture;
                } else {
                    if (color.a < 1.0f) {
                        customMat = Instantiate(coloredAlphaMat);
                    } else {
                        customMat = Instantiate(coloredMat);
                    }
                    customMat.name = coloredMat.name;
                    coloredMatCache[color] = customMat;
                }
                customMat.color = color;
                customMat.hideFlags = HideFlags.DontSave;
                return customMat;
            }
        }

        void ApplyMaterialToSurface(GameObject obj, Material sharedMaterial) {
            if (obj != null) {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null)
                    r.sharedMaterial = sharedMaterial;
            }
        }

        Vector3[] ReducedPoly(Vector3[] points) {
            int numPoints = points.Length;
            if (points.Length > MAX_POINTS_TO_TRIANGULATE) {
                List<Vector3> reducedPoints = new List<Vector3>(MAX_POINTS_TO_TRIANGULATE);
                Vector3 prev = Vector3.zero;
                for (int k = 0; k < MAX_POINTS_TO_TRIANGULATE; k++) {
                    int j = Mathf.FloorToInt(numPoints * (float)k / MAX_POINTS_TO_TRIANGULATE);
                    Vector3 p = points[j];
                    if (p != prev) {
                        reducedPoints.Add(p);
                        prev = p;
                    }
                }
                return reducedPoints.ToArray();
            } else
                return points;
        }

        #endregion

        #region World Gizmos

        void DrawCursor() {
            // Compute cursor dash lines
            float r = 0.5f;

            // Generate line V **********************
            int numPointsV = 100;
            Vector3[] pointsV = new Vector3[numPointsV];
            int[] indicesV = new int[numPointsV];
            for (int k = 0; k < numPointsV; k++) {
                indicesV[k] = k;
            }
            for (int k = 0; k < numPointsV; k++) {
                pointsV[k] = Vector3.up * (2 * r * k / numPointsV - r);
            }

            Transform t = transform.Find("CursorV");
            if (t != null)
                DestroyImmediate(t.gameObject);
            cursorLayerVLine = new GameObject("CursorV");
            cursorLayerVLine.hideFlags = HideFlags.DontSave;
            cursorLayerVLine.transform.SetParent(transform, false);
            cursorLayerVLine.transform.localPosition = Vector3.zero;
            cursorLayerVLine.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cursorLayerVLine.layer = gameObject.layer;
            cursorLayerVLine.SetActive(_showCursor);

            Mesh mesh = new Mesh();
            mesh.vertices = pointsV;
            mesh.SetIndices(indicesV, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = cursorLayerVLine.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = cursorLayerVLine.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //			mr.useLightProbes = false;
            mr.sharedMaterial = cursorMat;


            // Generate line H **********************
            int numPointsH = 200;
            Vector3[] pointsH = new Vector3[numPointsH];
            int[] indicesH = new int[numPointsH];
            for (int k = 0; k < numPointsH; k++) {
                indicesH[k] = k;
            }
            for (int k = 0; k < numPointsH; k++) {
                pointsH[k] = Vector3.right * (2 * r * k / numPointsH - r);
            }

            t = transform.Find("CursorH");
            if (t != null)
                DestroyImmediate(t.gameObject);
            cursorLayerHLine = new GameObject("CursorH");
            cursorLayerHLine.hideFlags = HideFlags.DontSave;
            cursorLayerHLine.transform.SetParent(transform, false);
            cursorLayerHLine.transform.localPosition = Vector3.zero;
            cursorLayerHLine.transform.localRotation = Quaternion.Euler(Vector3.zero);
            cursorLayerHLine.layer = gameObject.layer;
            cursorLayerHLine.SetActive(_showCursor);

            mesh = new Mesh();
            mesh.vertices = pointsH;
            mesh.SetIndices(indicesH, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            mf = cursorLayerHLine.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            mr = cursorLayerHLine.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //			mr.useLightProbes = false;
            mr.sharedMaterial = cursorMat;


        }

        void DrawGrid() {
            DrawLatitudeLines();
            DrawLongitudeLines();
        }

        void DrawLatitudeLines() {
            // Generate latitude lines
            List<Vector3> points = new List<Vector3>();
            List<int> indices = new List<int>();
            float r = 0.5f;
            int idx = -1;

            for (float a = 0; a < 90; a += _latitudeStepping) {
                for (int h = 1; h >= -1; h--) {
                    if (h == 0)
                        continue;
                    float y = h * a / 90.0f * r;
                    points.Add(new Vector3(-r, y, 0));
                    points.Add(new Vector3(r, y, 0));
                    indices.Add(++idx);
                    indices.Add(++idx);
                    if (a == 0)
                        break;
                }
            }

            Transform t = transform.Find("LatitudeLines");
            if (t != null)
                DestroyImmediate(t.gameObject);
            latitudeLayer = new GameObject("LatitudeLines");
            latitudeLayer.hideFlags = HideFlags.DontSave;
            latitudeLayer.transform.SetParent(transform, false);
            latitudeLayer.transform.localPosition = Vector3.zero;
            latitudeLayer.transform.localRotation = Quaternion.Euler(Vector3.zero);
            latitudeLayer.layer = gameObject.layer;
            latitudeLayer.SetActive(_showLatitudeLines);

            Mesh mesh = new Mesh();
            mesh.vertices = points.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = latitudeLayer.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = latitudeLayer.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //			mr.useLightProbes = false;
            mr.sharedMaterial = gridMat;

        }

        void DrawLongitudeLines() {
            // Generate longitude lines
            List<Vector3> points = new List<Vector3>();
            List<int> indices = new List<int>();
            float r = 0.5f;
            int idx = -1;
            int step = 180 / _longitudeStepping;

            for (float a = 0; a < 90; a += step) {
                for (int h = 1; h >= -1; h--) {
                    if (h == 0)
                        continue;
                    float x = h * a / 90.0f * r;
                    points.Add(new Vector3(x, -r, 0));
                    points.Add(new Vector3(x, r, 0));
                    indices.Add(++idx);
                    indices.Add(++idx);
                    if (a == 0)
                        break;
                }
            }


            Transform t = transform.Find("LongitudeLines");
            if (t != null)
                DestroyImmediate(t.gameObject);
            longitudeLayer = new GameObject("LongitudeLines");
            longitudeLayer.hideFlags = HideFlags.DontSave;
            longitudeLayer.transform.SetParent(transform, false);
            longitudeLayer.transform.localPosition = Vector3.zero;
            longitudeLayer.transform.localRotation = Quaternion.Euler(Vector3.zero);
            longitudeLayer.layer = gameObject.layer;
            longitudeLayer.SetActive(_showLongitudeLines);

            Mesh mesh = new Mesh();
            mesh.vertices = points.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = longitudeLayer.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = longitudeLayer.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            //			mr.useLightProbes = false;
            mr.sharedMaterial = gridMat;

        }

        #endregion

        #region Map Labels

        /// <summary>
        /// Forces redraw of all labels.
        /// </summary>
        public void RedrawMapLabels() {
            DestroyMapLabels();
            DrawMapLabels();
        }

        /// <summary>
        /// Draws the map labels. Note that it will update cached textmesh objects if labels are already drawn.
        /// </summary>
        void DrawMapLabels() {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": Draw map labels");
#endif
            //			DateTime start = DateTime.Now;

            // Set colors
            labelsFont.material.color = _countryLabelsColor;
            labelsShadowMaterial.color = _countryLabelsShadowColor;

            // Create texts
            GameObject overlay = GetOverlayLayer(true);
            GameObject textRoot;
            Transform t = overlay.transform.Find(OVERLAY_TEXT_ROOT);
            if (t == null) {
                textRoot = new GameObject(OVERLAY_TEXT_ROOT);
                textRoot.hideFlags = HideFlags.DontSave;
                textRoot.layer = overlay.layer;
            } else {
                textRoot = t.gameObject;
            }

            List<MeshRect> meshRects = new List<MeshRect>();
            for (int countryIndex = 0; countryIndex < countries.Length; countryIndex++) {
                Country country = countries[countryIndex];
                Vector2 center = new Vector2(country.center.x * mapWidth, country.center.y * mapHeight);
                Region region = country.regions[country.mainRegionIndex];

                // Special countries adjustements
                switch (countryIndex) {
                    case 6: // Antartica
                        //center.y += 3f;
                        break;
                    case 65: // Greenland
                        center.y -= 3f;
                        break;
                    case 22: // Brazil
                        center.y += 4f;
                        center.x += 1.0f;
                        break;
                    case 73: // India
                        center.x -= 2f;
                        break;
                    case 168: // USA
                        center.x -= 1f;
                        break;
                    case 27: // Canada
                        center.x -= 3f;
                        break;
                    case 30: // China
                        center.x -= 1f;
                        center.y -= 1f;
                        break;
                }

                // Adjusts country name length
                string countryName = country.customLabel != null ? country.customLabel : country.name.ToUpper();
                bool introducedCarriageReturn = false;
                if (countryName.Length > 15) {
                    int spaceIndex = countryName.IndexOf(' ', countryName.Length / 2);
                    if (spaceIndex >= 0) {
                        countryName = countryName.Substring(0, spaceIndex) + "\n" + countryName.Substring(spaceIndex + 1);
                        introducedCarriageReturn = true;
                    }
                }

                // add caption
                GameObject textObj;
                if (country.labelGameObject == null) {
                    Color labelColor = country.labelColorOverride ? country.labelColor : _countryLabelsColor;
                    Font customFont = country.labelFontOverride ?? labelsFont;
                    Material customLabelShadowMaterial = country.labelFontShadowMaterial ?? labelsShadowMaterial;
                    textObj = Drawing.CreateText(countryName, null, center, customFont, labelColor, _showLabelsShadow, customLabelShadowMaterial, _countryLabelsShadowColor);
                    country.labelGameObject = textObj;
                    Bounds bounds = textObj.GetComponent<Renderer>().bounds;
                    country.labelMeshWidth = bounds.size.x;
                    country.labelMeshHeight = bounds.size.y;
                    country.labelMeshCenter = center;
                    textObj.transform.SetParent(textRoot.transform, false);
                    textObj.transform.localPosition = center;
                    textObj.layer = textRoot.gameObject.layer;
                } else {
                    textObj = country.labelGameObject;
                    textObj.transform.localPosition = center;
                }

                float meshWidth = country.labelMeshWidth;
                float meshHeight = country.labelMeshHeight;

                // adjusts caption
                Rect rect = new Rect(region.rect2D.xMin * mapWidth, region.rect2D.yMin * mapHeight, region.rect2D.width * mapWidth, region.rect2D.height * mapHeight);
                float absoluteHeight;
                if (rect.height > rect.width * 1.45f) {
                    float angle;
                    if (rect.height > rect.width * 1.5f) {
                        angle = 90;
                    } else {
                        angle = Mathf.Atan2(rect.height, rect.width) * Mathf.Rad2Deg;
                    }
                    textObj.transform.localRotation = Quaternion.Euler(0, 0, angle);
                    absoluteHeight = Mathf.Min(rect.width * _countryLabelsSize, rect.height);
                } else {
                    absoluteHeight = Mathf.Min(rect.height * _countryLabelsSize, rect.width);
                }

                // adjusts scale to fit width in rect
                float adjustedMeshHeight = introducedCarriageReturn ? meshHeight * 0.5f : meshHeight;
                float scale = absoluteHeight / adjustedMeshHeight;
                float desiredWidth = meshWidth * scale;
                if (desiredWidth > rect.width) {
                    scale = rect.width / meshWidth;
                }
                if (adjustedMeshHeight * scale < _countryLabelsAbsoluteMinimumSize) {
                    scale = _countryLabelsAbsoluteMinimumSize / adjustedMeshHeight;
                }

                // stretchs out the caption
                float displayedMeshWidth = meshWidth * scale;
                float displayedMeshHeight = meshHeight * scale;
                string wideName;
                int times = Mathf.FloorToInt(rect.width * 0.45f / (meshWidth * scale));
                if (times > 10)
                    times = 10;
                if (times > 0) {
                    StringBuilder sb = new StringBuilder();
                    string spaces = new string(' ', times * 2);
                    for (int c = 0; c < countryName.Length; c++) {
                        sb.Append(countryName[c]);
                        if (c < countryName.Length - 1) {
                            sb.Append(spaces);
                        }
                    }
                    wideName = sb.ToString();
                } else {
                    wideName = countryName;
                }

                TextMesh tm = textObj.GetComponent<TextMesh>();
                if (tm.text.Length != wideName.Length) {
                    tm.text = wideName;
                    displayedMeshWidth = textObj.GetComponent<Renderer>().bounds.size.x * scale;
                    displayedMeshHeight = textObj.GetComponent<Renderer>().bounds.size.y * scale;
                    if (_showLabelsShadow) {
                        textObj.transform.Find("shadow").GetComponent<TextMesh>().text = wideName;
                    }
                }

                // apply scale
                textObj.transform.localScale = new Vector3(scale, scale, 1);

                // Save mesh rect for overlapping checking
                MeshRect mr = new MeshRect(countryIndex, new Rect(center.x - displayedMeshWidth * 0.5f, center.y - displayedMeshHeight * 0.5f, displayedMeshWidth, displayedMeshHeight));
                meshRects.Add(mr);
            }

            // Simple-fast overlapping checking
            int cont = 0;
            bool needsResort = true;

            while (needsResort && ++cont < 10) {
                meshRects.Sort(overlapComparer);

                for (int c = 1; c < meshRects.Count; c++) {
                    Rect thisMeshRect = meshRects[c].rect;
                    for (int prevc = c - 1; prevc >= 0; prevc--) {
                        Rect otherMeshRect = meshRects[prevc].rect;
                        if (thisMeshRect.Overlaps(otherMeshRect)) {
                            needsResort = true;
                            int thisCountryIndex = meshRects[c].countryIndex;
                            Country country = countries[thisCountryIndex];
                            GameObject thisLabel = country.labelGameObject;

                            //							if (country.name.Equals("Brazil")) Debug.Log (country.labelMeshCenter.x + " " + thisLabel.transform.localPosition);

                            // displaces this label
                            float offsety = (thisMeshRect.yMax - otherMeshRect.yMin);
                            offsety = Mathf.Min(country.regions[country.mainRegionIndex].rect2D.height * mapHeight * 0.35f, offsety);
                            thisLabel.transform.localPosition = new Vector3(country.labelMeshCenter.x, country.labelMeshCenter.y - offsety, thisLabel.transform.localPosition.z);
                            thisMeshRect = new Rect(thisLabel.transform.localPosition.x - thisMeshRect.width * 0.5f,
                                thisLabel.transform.localPosition.y - thisMeshRect.height * 0.5f,
                                thisMeshRect.width, thisMeshRect.height);
                            meshRects[c].rect = thisMeshRect;
                        }
                    }
                }
            }

            // Adjusts parent
            textRoot.transform.SetParent(overlay.transform, false);
            textRoot.transform.localPosition = new Vector3(0, 0, -0.001f);
            textRoot.transform.localRotation = Quaternion.Euler(Vector3.zero);
            textRoot.transform.localScale = new Vector3(1.0f / mapWidth, 1.0f / mapHeight, 1);


            //			Debug.Log ("DRAW LABELS: " + (DateTime.Now - start).TotalMilliseconds);
        }

        int overlapComparer(MeshRect r1, MeshRect r2) {
            return (r2.rect.center.y).CompareTo(r1.rect.center.y);
        }

        class MeshRect {
            public int countryIndex;
            public Rect rect;

            public MeshRect(int countryIndex, Rect rect) {
                this.countryIndex = countryIndex;
                this.rect = rect;
            }
        }

        void DestroyMapLabels() {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": destroy labels");
#endif
            if (countries != null) {
                for (int k = 0; k < countries.Length; k++) {
                    if (countries[k].labelGameObject != null) {
                        DestroyImmediate(countries[k].labelGameObject);
                        countries[k].labelGameObject = null;
                    }
                }
            }
            // Security check: if there're still gameObjects under TextRoot, also delete it
            if (overlayLayer != null) {
                Transform t = overlayLayer.transform.Find(OVERLAY_TEXT_ROOT);
                if (t != null && t.childCount > 0) {
                    DestroyImmediate(t.gameObject);
                }
            }
        }

        #endregion

        #region Overlay & Render viewport

        public GameObject CreateOverlay() {
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": CreateOverlay");
#endif

            // 2D labels layer
            Transform t = transform.Find(OVERLAY_BASE);
            if (t == null) {
                overlayLayer = new GameObject(OVERLAY_BASE);
                overlayLayer.hideFlags = HideFlags.DontSave;
                overlayLayer.transform.SetParent(transform, false);
                overlayLayer.transform.localPosition = Vector3.back * 0.02f;
                overlayLayer.transform.localScale = Vector3.one;
                overlayLayer.layer = gameObject.layer;
            } else {
                overlayLayer = t.gameObject;
                overlayLayer.SetActive(true);
            }
            return overlayLayer;
        }


        #endregion


    }

}