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
  


   

    public enum EARTH_STYLE {
		Natural = 0,
		SolidColor = 1
	}
    
    [Serializable]
    public partial class WorldMap2D : MonoBehaviour {
        



        #region Public properties

        //world 관련 변수 및 초기화
        /*
        public float worldTemperature;
        public float worldCarbonPPM;
        public long worldCarbonEmission;
        public long worldCarbonAbsorbed;
        public long worldForest;
        public long worldPopulation;
        public long worldDead;
        public long worldRefugees;
        public long worldFood;
        */
      
        /*
        WorldMap2D()
        {
            
            worldTemperature = 0.0f;
            worldCarbonPPM = 0.0f;
            worldCarbonEmission = 0;
            worldCarbonAbsorbed = 0;
            worldForest = 0;
            worldPopulation = 0;
            worldDead = 0;
            worldRefugees = 0;
            worldFood = 0;
        }
        */
        //
        


        /// <summary>
        /// Complete array of countries and the continent name they belong to.
        /// </summary>
        [NonSerialized]
        public Country[]
            countries;

        /// <summary>
        /// Complete list of cities with their names and country names.
        /// </summary>
        [NonSerialized]
        public List<City>
            cities;

        /// <summary>
        /// Returns City under mouse position or null if none.
        /// </summary>
        [NonSerialized]
        public City
            cityHighlighted;
        Country _countryHighlighted;
        /// <summary>
        /// Returns Country under mouse position or null if none.
        /// </summary>
        public Country countryHighlighted { get { return _countryHighlighted; } }

        int _countryHighlightedIndex = -1;
        /// <summary>
        /// Returns currently highlighted country index in the countries list.
        /// </summary>
        public int countryHighlightedIndex { get { return _countryHighlightedIndex; } }

        Region _countryRegionHighlighted;
        /// <summary>
        /// Returns currently highlightd country's region.
        /// </summary>
        /// <value>The country region highlighted.</value>
        public Region countryRegionHighlighted { get { return _countryRegionHighlighted; } }

        int _countryRegionHighlightedIndex = -1;
        /// <summary>
        /// Returns currently highlighted region of the country.
        /// </summary>
        public int countryRegionHighlightedIndex { get { return _countryRegionHighlightedIndex; } }

        int _countryLastClicked = -1;
        /// <summary>
        /// Returns the last clicked country.
        /// </summary>
        public int countryLastClicked { get { return _countryLastClicked; } }


        /// <summary>
        /// Returns true is mouse has entered the Earth's collider.
        /// </summary>
        [NonSerialized]
        public bool
            mouseIsOver;

        /// <summary>
        /// Enable/disable country highlight when mouse is over.
        /// </summary>
        [SerializeField]
        bool
            _enableCountryHighlight = true;

        public bool enableCountryHighlight {
            get {
                return _enableCountryHighlight;
            }
            set {
                if (_enableCountryHighlight != value) {
                    _enableCountryHighlight = value;
                    isDirty = true;
                }
            }
        }


        /// <summary>
        /// The navigation time in seconds.
        /// </summary>
        [SerializeField]
        [Range(1.0f, 16.0f)]
        float
            _navigationTime = 4.0f;

        public float navigationTime {
            get {
                return _navigationTime;
            }
            set {
                if (_navigationTime != value) {
                    _navigationTime = value;
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Returns whether a navigation is taking place at this moment.
        /// </summary>
        public bool isFlying { get { return flyToActive; } }

        [SerializeField]
        bool
            _showFrontiers = true;

        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showFrontiers {
            get {
                return _showFrontiers;
            }
            set {
                if (value != _showFrontiers) {
                    _showFrontiers = value;
                    isDirty = true;

                    if (frontiersLayer != null) {
                        frontiersLayer.SetActive(_showFrontiers);
                    } else if (_showFrontiers) {
                        DrawFrontiers();
                    }
                }
            }
        }

        [SerializeField]
        bool
            _showCities = true;

        /// <summary>
        /// Toggle cities visibility.
        /// </summary>
        public bool showCities {
            get {
                return _showCities;
            }
            set {
                if (_showCities != value) {
                    _showCities = value;
                    isDirty = true;
                    if (citiesLayer != null) {
                        citiesLayer.SetActive(_showCities);
                    } else if (_showCities) {
                        DrawCities();
                    }
                }
            }
        }

        [SerializeField]
        bool
            _fitWindowWidth = true;
        /// <summary>
        /// Ensure the map is always visible and occupy the entire Window.
        /// </summary>
        public bool fitWindowWidth {
            get {
                return _fitWindowWidth;
            }
            set {
                if (value != _fitWindowWidth) {
                    _fitWindowWidth = value;
                    isDirty = true;
                    CenterMap();
                }
            }
        }

        [SerializeField]
        bool
            _fitWindowHeight = true;
        /// <summary>
        /// Ensure the map is always visible and occupy the entire Window.
        /// </summary>
        public bool fitWindowHeight {
            get {
                return _fitWindowHeight;
            }
            set {
                if (value != _fitWindowHeight) {
                    _fitWindowHeight = value;
                    isDirty = true;
                    CenterMap();
                }
            }
        }

        [SerializeField]
        bool
            _showWorld = true;
        /// <summary>
        /// Toggle Earth visibility.
        /// </summary>
        public bool showEarth {
            get {
                return _showWorld;
            }
            set {
                if (value != _showWorld) {
                    _showWorld = value;
                    isDirty = true;
                    gameObject.GetComponent<MeshRenderer>().enabled = _showWorld;
                }
            }
        }

        /// <summary>
        /// Fill color to use when the mouse hovers a country's region.
        /// </summary>
        [SerializeField]
        Color
            _fillColor = new Color(1, 0, 0, 0.7f);

        public Color fillColor {
            get {
                if (hudMatCountry != null) {
                    return hudMatCountry.color;
                } else {
                    return _fillColor;
                }
            }
            set {
                Color proposedNewColor = new Color(value.r, value.g, value.b, 0.7f);
                if (proposedNewColor != _fillColor) {
                    _fillColor = proposedNewColor;
                    isDirty = true;
                    if (hudMatCountry != null && _fillColor != hudMatCountry.color) {
                        hudMatCountry.color = _fillColor;
                    }
                }
            }
        }


        /// <summary>
        /// Global color for frontiers.
        /// </summary>
        [SerializeField]
        Color
            _frontiersColor = Color.green;

        public Color frontiersColor {
            get {
                if (frontiersMat != null) {
                    return frontiersMat.color;
                } else {
                    return _frontiersColor;
                }
            }
            set {
                if (value != _frontiersColor) {
                    _frontiersColor = value;
                    isDirty = true;

                    if (frontiersMat != null && _frontiersColor != frontiersMat.color) {
                        frontiersMat.color = _frontiersColor;
                    }
                }
            }
        }



        /// <summary>
        /// Global color for cities.
        /// </summary>
        [SerializeField]
        Color
            _citiesColor = Color.white;

        public Color citiesColor {
            get {
                if (citiesMat != null) {
                    return citiesMat.color;
                } else {
                    return _citiesColor;
                }
            }
            set {
                if (value != _citiesColor) {
                    _citiesColor = value;
                    isDirty = true;

                    if (citiesMat != null && _citiesColor != citiesMat.color) {
                        citiesMat.color = _citiesColor;
                    }
                }
            }
        }


        /// <summary>
        /// The size of the cities icon (dot).
        /// </summary>
        public float cityIconSize = 1.0f;
        [SerializeField]
        bool
            _showOutline = true;

        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showOutline {
            get {
                return _showOutline;
            }
            set {
                if (value != _showOutline) {
                    _showOutline = value;
                    Redraw(); // recreate surfaces layer
                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Color for country frontiers outline.
        /// </summary>
        [SerializeField]
        Color
            _outlineColor = Color.black;

        public Color outlineColor {
            get {
                if (outlineMat != null) {
                    return outlineMat.color;
                } else {
                    return _outlineColor;
                }
            }
            set {
                if (value != _outlineColor) {
                    _outlineColor = value;
                    isDirty = true;

                    if (outlineMat != null && _outlineColor != outlineMat.color) {
                        outlineMat.color = _outlineColor;
                    }
                }
            }
        }

        static WorldMap2D _instance;

        /// <summary>
        /// Instance of the world map. Use this property to access World Map functionality.
        /// </summary>
        public static WorldMap2D instance {
            get {
                if (_instance == null) {
                    GameObject obj = GameObject.Find("WorldMap2D");
                    if (obj == null) {
                        Debug.LogWarning("'WorldMap2D' GameObject could not be found in the scene. Make sure it's created with this name before using any map functionality.");
                    } else {
                        _instance = obj.GetComponent<WorldMap2D>();
                    }
                }
                return _instance;
            }
        }

        [Range(0, 17000)]
        [SerializeField]
        int
            _minPopulation = 0;

        public int minPopulation {
            get {
                return _minPopulation;
            }
            set {
                if (value != _minPopulation) {
                    _minPopulation = value;
                    isDirty = true;
                    DrawCities();
                }
            }
        }

        [SerializeField]
        EARTH_STYLE
            _earthStyle = EARTH_STYLE.Natural;

        public EARTH_STYLE earthStyle {
            get {
                return _earthStyle;
            }
            set {
                if (value != _earthStyle) {
                    _earthStyle = value;
                    isDirty = true;
                    RestyleEarth();
                }
            }
        }


        /// <summary>
        /// Color for Earth (for SolidColor style)
        /// </summary>
        [SerializeField]
        Color
            _earthColor = Color.black;

        public Color earthColor {
            get {
                return _earthColor;
            }
            set {
                if (value != _earthColor) {
                    _earthColor = value;
                    isDirty = true;

                    if (_earthStyle == EARTH_STYLE.SolidColor) {
                        Material mat = GetComponent<Renderer>().sharedMaterial;
                        mat.color = _earthColor;
                    }
                }
            }
        }

        [NonSerialized]
        public int
            numCitiesDrawn = 0;
        [NonSerialized]
        public bool
            isDirty; // internal variable used to confirm changes in custom inspector - don't change its value

        [SerializeField]
        bool
            _allowUserDrag = true;

        public bool allowUserDrag {
            get { return _allowUserDrag; }
            set {
                _allowUserDrag = value;
                isDirty = true;
            }
        }


        [SerializeField]
        bool
            _respectOtherUI = true;

        /// <summary>
        /// When enabled, will prevent globe interaction if pointer is over an UI element
        /// </summary>
        public bool respectOtherUI {
            get { return _respectOtherUI; }
            set {
                if (value != _respectOtherUI) {
                    _respectOtherUI = value;
                    isDirty = true;
                }
            }
        }


        [SerializeField]
        bool
            _centerOnRightClick = true;

        public bool centerOnRightClick {
            get { return _centerOnRightClick; }
            set {
                _centerOnRightClick = value;
                isDirty = true;
            }
        }

        [SerializeField]
        bool
            _allowUserZoom = true;

        public bool allowUserZoom {
            get { return _allowUserZoom; }
            set {
                _allowUserZoom = value;
                isDirty = true;
            }
        }

        [SerializeField]
        [Range(0.1f, 3)]
        float
            _mouseWheelSensitivity = 0.2f;

        public float mouseWheelSensitivity {
            get { return _mouseWheelSensitivity; }
            set {
                _mouseWheelSensitivity = value;
                isDirty = true;
            }
        }

        [SerializeField]
        [Range(0.1f, 3)]
        float
            _mouseDragSensitivity = 0.2f;

        public float mouseDragSensitivity {
            get { return _mouseDragSensitivity; }
            set {
                _mouseDragSensitivity = value;
                isDirty = true;
            }
        }

        [SerializeField]
        bool
            _showCursor = true;

        /// <summary>
        /// Toggle cursor lines visibility.
        /// </summary>
        public bool showCursor {
            get {
                return _showCursor;
            }
            set {
                if (value != _showCursor) {
                    _showCursor = value;
                    isDirty = true;

                    if (cursorLayerVLine != null) {
                        cursorLayerVLine.SetActive(_showCursor);
                    }
                    if (cursorLayerHLine != null) {
                        cursorLayerHLine.SetActive(_showCursor);
                    }
                }
            }
        }

        /// <summary>
        /// Cursor lines color.
        /// </summary>
        [SerializeField]
        Color
            _cursorColor = new Color(0.56f, 0.47f, 0.68f);

        public Color cursorColor {
            get {
                if (cursorMat != null) {
                    return cursorMat.color;
                } else {
                    return _cursorColor;
                }
            }
            set {
                if (value != _cursorColor) {
                    _cursorColor = value;
                    isDirty = true;

                    if (cursorMat != null && _cursorColor != cursorMat.color) {
                        cursorMat.color = _cursorColor;
                    }
                }
            }
        }

        [SerializeField]
        bool
            _cursorFollowMouse = true;

        /// <summary>
        /// Makes the cursor follow the mouse when it's over the World.
        /// </summary>
        public bool cursorFollowMouse {
            get {
                return _cursorFollowMouse;
            }
            set {
                if (value != _cursorFollowMouse) {
                    _cursorFollowMouse = value;
                    isDirty = true;
                }
            }
        }

        Vector3
            _cursorLocation;

        public Vector3
            cursorLocation {
            get {
                return _cursorLocation;
            }
            set {
                if (_cursorLocation != value) {
                    _cursorLocation = value;
                    if (cursorLayerVLine != null) {
                        cursorLayerVLine.transform.localPosition = transform.right * _cursorLocation.x;
                    }
                    if (cursorLayerHLine != null) {
                        cursorLayerHLine.transform.localPosition = transform.up * _cursorLocation.y;
                    }
                }
            }
        }

        [SerializeField]
        bool
            _showLatitudeLines = true;

        /// <summary>
        /// Toggle latitude lines visibility.
        /// </summary>
        public bool showLatitudeLines {
            get {
                return _showLatitudeLines;
            }
            set {
                if (value != _showLatitudeLines) {
                    _showLatitudeLines = value;
                    isDirty = true;

                    if (latitudeLayer != null) {
                        latitudeLayer.SetActive(_showLatitudeLines);
                    }
                }
            }
        }

        [SerializeField]
        [Range(5.0f, 45.0f)]
        int
            _latitudeStepping = 15;
        /// <summary>
        /// Specify latitude lines separation.
        /// </summary>
        public int latitudeStepping {
            get {
                return _latitudeStepping;
            }
            set {
                if (value != _latitudeStepping) {
                    _latitudeStepping = value;
                    isDirty = true;

                    if (gameObject.activeInHierarchy)
                        DrawLatitudeLines();
                }
            }
        }

        [SerializeField]
        bool
            _showLongitudeLines = true;

        /// <summary>
        /// Toggle longitude lines visibility.
        /// </summary>
        public bool showLongitudeLines {
            get {
                return _showLongitudeLines;
            }
            set {
                if (value != _showLongitudeLines) {
                    _showLongitudeLines = value;
                    isDirty = true;

                    if (longitudeLayer != null) {
                        longitudeLayer.SetActive(_showLongitudeLines);
                    }
                }
            }
        }

        [SerializeField]
        [Range(5.0f, 45.0f)]
        int
            _longitudeStepping = 15;
        /// <summary>
        /// Specify longitude lines separation.
        /// </summary>
        public int longitudeStepping {
            get {
                return _longitudeStepping;
            }
            set {
                if (value != _longitudeStepping) {
                    _longitudeStepping = value;
                    isDirty = true;

                    if (gameObject.activeInHierarchy)
                        DrawLongitudeLines();
                }
            }
        }

        /// <summary>
        /// Color for imaginary lines (longitude and latitude).
        /// </summary>
        [SerializeField]
        Color
            _gridColor = new Color(0.16f, 0.33f, 0.498f);

        public Color gridLinesColor {
            get {
                if (gridMat != null) {
                    return gridMat.color;
                } else {
                    return _gridColor;
                }
            }
            set {
                if (value != _gridColor) {
                    _gridColor = value;
                    isDirty = true;

                    if (gridMat != null && _gridColor != gridMat.color) {
                        gridMat.color = _gridColor;
                    }
                }
            }
        }

        [SerializeField]
        bool
            _showCountryNames = false;

        public bool showCountryNames {
            get {
                return _showCountryNames;
            }
            set {
                if (value != _showCountryNames) {
#if TRACE_CTL
					Debug.Log ("CTL " + DateTime.Now + ": showcountrynames!");
#endif
                    _showCountryNames = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        if (!showCountryNames) {
                            DestroyMapLabels();
                        } else {
                            DrawMapLabels();
                        }
                    }
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsAbsoluteMinimumSize = 0.5f;

        public float countryLabelsAbsoluteMinimumSize {
            get {
                return _countryLabelsAbsoluteMinimumSize;
            }
            set {
                if (value != _countryLabelsAbsoluteMinimumSize) {
                    _countryLabelsAbsoluteMinimumSize = value;
                    isDirty = true;
                    if (_showCountryNames)
                        DrawMapLabels();
                }
            }
        }

        [SerializeField]
        float
            _countryLabelsSize = 0.25f;

        public float countryLabelsSize {
            get {
                return _countryLabelsSize;
            }
            set {
                if (value != _countryLabelsSize) {
                    _countryLabelsSize = value;
                    isDirty = true;
                    if (_showCountryNames)
                        DrawMapLabels();
                }
            }
        }

        [SerializeField]
        bool
            _showLabelsShadow = true;

        /// <summary>
        /// Draws a shadow under map labels. Specify the color using labelsShadowColor.
        /// </summary>
        /// <value><c>true</c> if show labels shadow; otherwise, <c>false</c>.</value>
        public bool showLabelsShadow {
            get {
                return _showLabelsShadow;
            }
            set {
                if (value != _showLabelsShadow) {
                    _showLabelsShadow = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        RedrawMapLabels();
                    }
                }
            }
        }

        [SerializeField]
        Color
            _countryLabelsColor = Color.white;

        /// <summary>
        /// Color for map labels.
        /// </summary>
        public Color countryLabelsColor {
            get {
                return _countryLabelsColor;
            }
            set {
                if (value != _countryLabelsColor) {
                    _countryLabelsColor = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        labelsFont.material.color = _countryLabelsColor;
                    }
                }
            }
        }

        [SerializeField]
        Color
            _countryLabelsShadowColor = Color.black;

        /// <summary>
        /// Color for map labels.
        /// </summary>
        public Color countryLabelsShadowColor {
            get {
                return _countryLabelsShadowColor;
            }
            set {
                if (value != _countryLabelsShadowColor) {
                    _countryLabelsShadowColor = value;
                    isDirty = true;
                    if (gameObject.activeInHierarchy) {
                        labelsShadowMaterial.color = _countryLabelsShadowColor;
                    }
                }
            }
        }

        [SerializeField]
        Font _countryLabelsFont;

        /// <summary>
        /// Gets or sets the default font for country labels
        /// </summary>
        public Font countryLabelsFont {
            get {
                return _countryLabelsFont;
            }
            set {
                if (value != _countryLabelsFont) {
                    _countryLabelsFont = value;
                    isDirty = true;
                    ReloadFont();
                    RedrawMapLabels();
                }
            }

        }

        #endregion


        
       
	#region Public API area

	
		/// <summary>
		/// Moves the map in front of the camera so it fits the viewport.
		/// </summary>
		public void CenterMap () {
			lastMapPosition = transform.position;
			lastCamPosition = cameraMain.transform.position;
			transform.localScale = new Vector3 (200, 100, 1);
			float fv = cameraMain.fieldOfView;
			float aspect = cameraMain.aspect; 
			float radAngle = fv * Mathf.Deg2Rad;
			float distance, frustumDistanceW, frustumDistanceH;
			frustumDistanceH = mapHeight * 0.5f / Mathf.Tan (radAngle * 0.5f);
			frustumDistanceW = (mapWidth / aspect) * 0.5f / Mathf.Tan (radAngle * 0.5f);
			if (_fitWindowHeight) {
				distance = Mathf.Min (frustumDistanceH, frustumDistanceW);
				maxFrustumDistanceSqr = distance * distance;
			} else if (_fitWindowWidth) {
				distance = Mathf.Max (frustumDistanceH, frustumDistanceW);
				maxFrustumDistanceSqr = distance * distance;
			} else {
				distance = Vector3.Distance (transform.position, cameraMain.transform.position);
				maxFrustumDistanceSqr = float.MaxValue;
			}
			transform.rotation = cameraMain.transform.rotation;
			transform.position = cameraMain.transform.position + cameraMain.transform.forward * distance;
		}

		/// <summary>
		/// Returns the index of a country in the countries array by its name.
		/// </summary>
		public int GetCountryIndex (string countryName) {
			if (countryLookup.ContainsKey (countryName)) 
				return countryLookup [countryName];
			else
				return -1;
		}

		/// <summary>
		/// Returns the index of a country in the countries collection by its reference.
		/// </summary>
		public int GetCountryIndex (Country country) {
			if (countryLookup.ContainsKey (country.name)) 
				return countryLookup [country.name];
			else
				return -1;
		}

		
		/// <summary>
		/// Renames the country. Name must be unique, different from current and one letter minimum.
		/// </summary>
		/// <returns><c>true</c> if country was renamed, <c>false</c> otherwise.</returns>
		public bool CountryRename (string oldName, string newName) {
			if (newName == null || newName.Length == 0)
				return false;
			int countryIndex = GetCountryIndex (oldName);
			int newCountryIndex = GetCountryIndex (newName);
			if (countryIndex < 0 || newCountryIndex >= 0)
				return false;
			countries [countryIndex].name = newName;
			lastCountryLookupCount = -1;
			return true;
				
		}


		/// <summary>
		/// Deletes the country. Optionally also delete its provinces.
		/// </summary>
		/// <returns><c>true</c> if country was deleted, <c>false</c> otherwise.</returns>
		public bool CountryDelete (int countryIndex) {
			if (countryIndex <0 || countryIndex>=countries.Length)
				return false;

			// Excludes country from new array
			List<Country>newCountries = new List<Country>(countries.Length);
			for (int k=0;k<countries.Length;k++) {
				if (k!=countryIndex) newCountries.Add (countries[k]);
			}
			countries = newCountries.ToArray();

			// Update lookup dictionaries
			lastCountryLookupCount = -1;

			return true;
		}



		/// <summary>
		/// Adds a new country which has been properly initialized. Used by the Map Editor. Name must be unique.
		/// </summary>
		/// <returns><c>true</c> if country was added, <c>false</c> otherwise.</returns>
		public bool CountryAdd (Country country) {
			int countryIndex = GetCountryIndex (country.name);
			if (countryIndex >= 0)
				return false;
			Country[] newCountries = new Country[countries.Length + 1];
			for (int k=0; k<countries.Length; k++) {
				newCountries [k] = countries [k];
			}
			newCountries [newCountries.Length - 1] = country;
			countries = newCountries;
			lastCountryLookupCount = -1;
			return true;
		}


		/// <summary>
		/// Returns the country index by screen position.
		/// </summary>
		public bool GetCountryIndex (Ray ray, out int countryIndex, out int regionIndex) {
			RaycastHit[] hits = Physics.RaycastAll (ray, 500, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector2 localHit = transform.InverseTransformPoint (hits [k].point);
						for (int c=0; c<countries.Length; c++) {
							for (int cr=0; cr<countries[c].regions.Count; cr++) {
								// project country polygon to screen space
								Region region = countries [c].regions [cr];
								if (ContainsPoint (localHit, region.points, region.points.Length - 1)) {
									countryIndex = c;
									regionIndex = cr;
									return true;
								}	
							}
						}
					}
				}
			}
			countryIndex = -1;
			regionIndex = -1;
			return false;
		}

	
		/// <summary>
		/// Returns the index of a city in the global countries collection. Note that country index needs to be supplied due to repeated city names.
		/// </summary>
		public int GetCityIndex (int countryIndex, string cityName) {
			if (countryIndex >= 0 && countryIndex < countries.Length) {
				string countryName = countries [countryIndex].name;
				for (int k=0; k<cities.Count; k++) {
					if (cities [k].name.Equals (cityName) && cities [k].country.Equals (countryName))
						return k;
				}
			} else {
				// Try to select city by its name alone
				for (int k=0; k<cities.Count; k++) {
					if (cities [k].name.Equals (cityName))
						return k;
				}
			}
			return -1;
		}

		/// <summary>
		/// Starts navigation to target country. Returns false if country is not found.
		/// </summary>
		public bool FlyToCountry (string name) {
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				FlyToCountry (countryIndex);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Starts navigation to target country. with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// Returns false if country is not found. 
		/// </summary>
		public bool FlyToCountry (string name, float duration) {
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				FlyToCountry (countryIndex, duration);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Starts navigation to target country by index in the countries collection. Returns false if country is not found.
		/// </summary>
		public void FlyToCountry (int countryIndex) {
			FlyToCountry (countryIndex, _navigationTime);
		}
		
		/// <summary>
		/// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// </summary>
		public void FlyToCountry (int countryIndex, float duration) {
			SetDestination (countries [countryIndex].center, duration);
		}

		/// <summary>
		/// Flashes specified city by index in the global city collection.
		/// </summary>
		public void BlinkCity (int cityIndex, Color color1, Color color2, float duration, float blinkingSpeed) {
			if (citiesLayer == null)
				return;
			Transform t = citiesLayer.transform.Find ("City" + cityIndex);
			if (t == null)
				return;
			CityBlinker sb = t.gameObject.AddComponent<CityBlinker> ();
			sb.blinkMaterial = citiesMat;
			sb.color1 = color1;
			sb.color2 = color2;
			sb.duration = duration;
			sb.speed = blinkingSpeed;
		}


		/// <summary>
		/// Starts navigation to target city. Returns false if not found.
		/// </summary>
		public bool FlyToCity (string name) {
			return FlyToCity (name, _navigationTime);
		}

		/// <summary>
		/// Starts navigation to target city with duration provided. Returns false if not found.
		/// </summary>
		public bool FlyToCity (string name, float duration) {
			for (int k=0; k<cities.Count; k++) {
				if (name.Equals (cities [k].name)) {
					FlyToCity (k);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Starts navigation to target city by index in the cities collection.
		/// </summary>
		public void FlyToCity (int cityIndex) {
			FlyToCity (cityIndex, _navigationTime);
		}

		
		/// <summary>
		/// Starts navigation to target city by index with duration providedn
		/// </summary>
		public void FlyToCity (int cityIndex, float duration) {
			SetDestination (cities [cityIndex].unity2DLocation, duration);
		}


		/// <summary>
		/// Returns the city index by screen position.
		/// </summary>
		public bool GetCityIndex (Ray ray, out int cityIndex) {
			RaycastHit[] hits = Physics.RaycastAll (ray, 500, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector3 localHit = transform.InverseTransformPoint (hits [k].point);
						int c = GetCityNearPoint (localHit);
						if (c >= 0) {
							cityIndex = c;
							return true;
						}
					}
				}
			}
			cityIndex = -1;
			return false;
		}


		/// <summary>
		/// Clears any city highlighted (color changed) and resets them to default city color
		/// </summary>
		public void HideCityHighlights () {
			DrawCities ();
		}


		/// <summary>
		/// Toggles the city highlight.
		/// </summary>
		/// <param name="cityIndex">City index.</param>
		/// <param name="color">Color.</param>
		/// <param name="highlighted">If set to <c>true</c> the color of the city will be changed. If set to <c>false</c> the color of the city will be reseted to default color</param>
		public void ToggleCityHighlight (int cityIndex, Color color, bool highlighted) {
			if (citiesLayer == null)
				return;
			Transform t = citiesLayer.transform.Find ("City" + cityIndex);
			if (t == null)
				return;
			Renderer rr = t.gameObject.GetComponent<Renderer> ();
			if (rr == null)
				return;
			if (highlighted) {
				Material mat = Instantiate (rr.sharedMaterial);
				mat.hideFlags = HideFlags.DontSave;
				mat.color = color;
				rr.sharedMaterial = mat;
			} else {
				rr.sharedMaterial = citiesMat;
			}
		}

		
		/// <summary>
		/// Starts navigation to target location in local spherical coordinates.
		/// </summary>
		public void FlyToLocation (float x, float y) {
			SetDestination (new Vector2 (x, y), _navigationTime);
		}
		
	
		/// <summary>
		/// Colorize all regions of specified country by name. Returns false if not found.
		/// </summary>
		public bool ToggleCountrySurface (string name, bool visible, Color color) {
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				ToggleCountrySurface (countryIndex, visible, color);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Iterates for the countries list and colorizes those belonging to specified continent name.
		/// </summary>
		public void ToggleContinentSurface (string continentName, bool visible, Color color) {
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					ToggleCountrySurface (countries [colorizeIndex].name, visible, color);
				}
			}
		}

		/// <summary>
		/// Uncolorize/hide specified countries beloning to a continent.
		/// </summary>
		public void HideContinentSurface (string continentName) {
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					HideCountrySurface (colorizeIndex);
				}
			}
		}

		/// <summary>
		/// Colorize all regions of specified country by index in the countries collection.
		/// </summary>
		public void ToggleCountrySurface (int countryIndex, bool visible, Color color) {
			ToggleCountrySurface (countryIndex, visible, color, null);
		}

		/// <summary>
		/// Colorize all regions of specified country and assings a texture.
		/// </summary>
		public void ToggleCountrySurface (int countryIndex, bool visible, Color color, Texture2D texture) {
			if (!visible) {
				HideCountrySurface (countryIndex);
				return;
			}
			for (int r=0; r<countries[countryIndex].regions.Count; r++) {
				ToggleCountryRegionSurface (countryIndex, r, visible, color, texture, Vector2.one, Vector2.zero, 0);
			}
		}

		/// <summary>
		/// Uncolorize/hide specified country by index in the countries collection.
		/// </summary>
		public void HideCountrySurface (int countryIndex) {
			for (int r=0; r<countries[countryIndex].regions.Count; r++) {
				HideCountryRegionSurface (countryIndex, r);
			}
		}

		/// <summary>
		/// Highlights the country region specified.
		/// Internally used by the Editor component, but you can use it as well to temporarily mark a country region.
		/// </summary>
		/// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
		public GameObject ToggleCountryRegionSurfaceHighlight (int countryIndex, int regionIndex, Color color, bool drawOutline) {
			GameObject surf;
			Material mat = Instantiate (hudMatCountry);
			mat.hideFlags = HideFlags.DontSave;
			mat.color = color;
			mat.renderQueue--;
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (existsInCache) {
				surf = surfaces [cacheIndex];
				if (surf == null) {
					surfaces.Remove (cacheIndex);
				} else {
					surf.SetActive (true);
					surf.GetComponent<Renderer> ().sharedMaterial = mat;
				}
			} else {
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, mat, Vector2.one, Vector2.zero, 0, drawOutline);
			}
			return surf;
		}
		
		/// <summary>
		/// Colorize main region of a country by index in the countries collection.
		/// </summary>
		/// <param name="texture">Optional texture or null to colorize with single color</param>
		public void ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
			ToggleCountryRegionSurface (countryIndex, countries [countryIndex].mainRegionIndex, visible, color, texture, textureScale, textureOffset, textureRotation);
		}

		public void ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color) {
			ToggleCountryRegionSurface (countryIndex, regionIndex, visible, color, null, Vector2.one, Vector2.zero, 0);
		}

		/// <summary>
		/// Colorize specified region of a country by indexes.
		/// </summary>
		public void ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation) {
			if (!visible) {
				HideCountryRegionSurface (countryIndex, regionIndex);
				return;
			}
			GameObject surf = null;
			Region region = countries [countryIndex].regions [regionIndex];
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			// Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
			if (surfaces.ContainsKey (cacheIndex) && surfaces [cacheIndex] != null) 
				surf = surfaces [cacheIndex];

			// Should the surface be recreated?
			Material surfMaterial;
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset || 
					textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals (texturizedMat.name))) {
					surfaces.Remove (cacheIndex);
					DestroyImmediate (surf);
					surf = null;
				}
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = countryHighlightedIndex == countryIndex && countryRegionHighlightedIndex == regionIndex;
			if (surf != null) {
				if (!surf.activeSelf)
					surf.SetActive (true);
				// Check if material is ok
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if ((texture == null && !surfMaterial.name.Equals (coloredMat.name)) || (texture != null && !surfMaterial.name.Equals (texturizedMat.name)) 
					|| (surfMaterial.color != color && !isHighlighted) || (texture != null && region.customMaterial.mainTexture != texture)) {
					Material goodMaterial = GetColoredTexturedMaterial (color, texture);
					region.customMaterial = goodMaterial;
					ApplyMaterialToSurface (surf, goodMaterial);
				}
			} else {
				surfMaterial = GetColoredTexturedMaterial (color, texture);
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, surfMaterial, textureScale, textureOffset, textureRotation, _showOutline);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = textureOffset;
				region.customTextureRotation = textureRotation;
				region.customTextureScale = textureScale;
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial != null && isHighlighted && region.customMaterial.color != hudMatCountry.color) {
				Material clonedMat = Instantiate (region.customMaterial);
				clonedMat.hideFlags = HideFlags.DontSave;
				clonedMat.name = region.customMaterial.name;
				clonedMat.color = hudMatCountry.color;
				surf.GetComponent<Renderer> ().sharedMaterial = clonedMat;
				countryRegionHighlightedObj = surf;
			}
		}

		
		/// <summary>
		/// Uncolorize/hide specified country by index in the countries collection.
		/// </summary>
		public void HideCountryRegionSurface (int countryIndex, int regionIndex) {
			if (_countryHighlightedIndex != countryIndex || _countryRegionHighlightedIndex != regionIndex) {
				int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
				if (surfaces.ContainsKey (cacheIndex)) {
					surfaces [cacheIndex].SetActive (false);
				}
			}
			countries [countryIndex].regions [regionIndex].customMaterial = null;
		}

		/// <summary>
		/// Hides all colorized regions of all countries.
		/// </summary>
		public void HideCountrySurfaces () {
			for (int c=0; c<countries.Length; c++) {
				HideCountrySurface (c);
			}
		}

	
		/// <summary>
		/// Flashes specified country by index in the countries collection.
		/// </summary>
		public void BlinkCountry (int countryIndex, Color color1, Color color2, float duration, float blinkingSpeed) {
			int mainRegionIndex = countries [countryIndex].mainRegionIndex;
			BlinkCountry (countryIndex, mainRegionIndex, color1, color2, duration, blinkingSpeed);
		}

		/// <summary>
		/// Flashes specified country's region.
		/// </summary>
		public void BlinkCountry (int countryIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed) {
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			GameObject surf;
			bool disableAtEnd;
			if (surfaces.ContainsKey (cacheIndex)) {
				surf = surfaces [cacheIndex];
				disableAtEnd = !surf.activeSelf;
			} else {
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, hudMatCountry, _showOutline);
				disableAtEnd = true;
			}
			SurfaceBlinker sb = surf.AddComponent<SurfaceBlinker> ();
			sb.blinkMaterial = hudMatCountry;
			sb.color1 = color1;
			sb.color2 = color2;
			sb.duration = duration;
			sb.speed = blinkingSpeed;
			sb.disableAtEnd = disableAtEnd;
			sb.customizableSurface = countries [countryIndex].regions [regionIndex];
			surf.SetActive (true);
		}

		/// <summary>
		/// Returns an array of country names. The returning list can be grouped by continent.
		/// </summary>
		public string[] GetCountryNames (bool groupByContinent) {
			List<string> c = new List<string> ();
			if (countries == null)
				return c.ToArray ();
			Dictionary<string, bool> continentsAdded = new Dictionary<string, bool> ();
			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				if (groupByContinent) {
					if (!continentsAdded.ContainsKey (country.continent)) {
						continentsAdded.Add (country.continent, true);
						c.Add (country.continent);
					}
					c.Add (country.continent + "|" + country.name + " (" + k + ")");
				} else {
					c.Add (country.name + " (" + k + ")");
				}
			}
			c.Sort ();

			if (groupByContinent) {
				int k = -1;
				while (++k<c.Count) {
					int i = c [k].IndexOf ('|');
					if (i > 0) {
						c [k] = "  " + c [k].Substring (i + 1);
					}
				}
			}
			return c.ToArray ();
		}


		/// <summary>
		/// Returns an array with the city names.
		/// </summary>
		public string[] GetCityNames () {
			List<string> c = new List<string> (cities.Count);
			for (int k=0; k<cities.Count; k++) {
				c.Add (cities [k].name + " (" + k + ")");
			}
			c.Sort ();
			return c.ToArray ();
		}

		#endregion
    
	}

}