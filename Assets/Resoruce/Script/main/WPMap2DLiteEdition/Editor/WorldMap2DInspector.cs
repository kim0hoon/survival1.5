using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {
	[CustomEditor(typeof(WorldMap2D))]
	public class WorldMap2DInspector : Editor {
		WorldMap2D _map;
		Texture2D _headerTexture, _blackTexture;
		string[] earthStyleOptions;
		GUIStyle blackStyle;

		void OnEnable () {
			Color backColor = EditorGUIUtility.isProSkin ? new Color (0.18f, 0.18f, 0.18f) : new Color (0.7f, 0.7f, 0.7f);
			_blackTexture = MakeTex (4, 4, backColor);
			_blackTexture.hideFlags = HideFlags.DontSave;
			_map = (WorldMap2D)target;
			_headerTexture = Resources.Load<Texture2D> ("EditorHeader");
			if (_map.countries == null) {
				_map.Init ();
			}
			earthStyleOptions = new string[] {
				"Natural", "Solid Color"
			};
			blackStyle = new GUIStyle ();
			blackStyle.normal.background = _blackTexture;
		}

		public override void OnInspectorGUI () {
			_map.isDirty = false;

			EditorGUILayout.Separator ();
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
			GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;  
			EditorGUILayout.Separator ();

			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Fit Window Width", GUILayout.Width (120));
			_map.fitWindowWidth = EditorGUILayout.Toggle (_map.fitWindowWidth);
			GUILayout.Label ("Fit Window Height");
			_map.fitWindowHeight = EditorGUILayout.Toggle (_map.fitWindowHeight);
			if (GUILayout.Button ("Center Map")) {
				_map.CenterMap ();
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Earth", GUILayout.Width (120));
			_map.showEarth = EditorGUILayout.Toggle (_map.showEarth);
			EditorGUILayout.EndHorizontal ();

			if (_map.showEarth) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Earth Style", GUILayout.Width (120));
				_map.earthStyle = (EARTH_STYLE)EditorGUILayout.Popup ((int)_map.earthStyle, earthStyleOptions);

				if (_map.earthStyle == EARTH_STYLE.SolidColor) {
					GUILayout.Label ("Color");
					_map.earthColor = EditorGUILayout.ColorField (_map.earthColor, GUILayout.Width (50));
				}
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Latitude Lines", GUILayout.Width (120));
			_map.showLatitudeLines = EditorGUILayout.Toggle (_map.showLatitudeLines, GUILayout.Width(40));
			GUILayout.Label ("Stepping");
			_map.latitudeStepping = EditorGUILayout.IntSlider (_map.latitudeStepping, 5, 45);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Longitude Lines", GUILayout.Width (120));
			_map.showLongitudeLines = EditorGUILayout.Toggle (_map.showLongitudeLines, GUILayout.Width(40));
			GUILayout.Label ("Stepping");
			_map.longitudeStepping = EditorGUILayout.IntSlider (_map.longitudeStepping, 5, 45);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Grid Color", GUILayout.Width (120));
			_map.gridLinesColor = EditorGUILayout.ColorField (_map.gridLinesColor, GUILayout.Width (50));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Cities", GUILayout.Width (120));
			_map.showCities = EditorGUILayout.Toggle (_map.showCities);
			EditorGUILayout.EndHorizontal ();

			if (_map.showCities) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Cities Color", GUILayout.Width (120));
				_map.citiesColor = EditorGUILayout.ColorField (_map.citiesColor, GUILayout.Width (50));

				GUILayout.Label ("Icon Size");
				_map.cityIconSize = EditorGUILayout.Slider (_map.cityIconSize, 0.1f, 5.0f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Min Population (K)", GUILayout.Width (120));
				_map.minPopulation = EditorGUILayout.IntSlider (_map.minPopulation, 0, 17000);
				GUILayout.Label (_map.numCitiesDrawn + "/" + _map.cities.Count);
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Countries", GUILayout.Width (120));
			_map.showFrontiers = EditorGUILayout.Toggle (_map.showFrontiers, GUILayout.Width(40));
			EditorGUILayout.EndHorizontal ();

			if (_map.showFrontiers) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Frontiers Color", GUILayout.Width (120));
				_map.frontiersColor = EditorGUILayout.ColorField (_map.frontiersColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Country Highlight", GUILayout.Width (120));
			_map.enableCountryHighlight = EditorGUILayout.Toggle (_map.enableCountryHighlight, GUILayout.Width(40));

			if (_map.enableCountryHighlight) {

				GUILayout.Label ("Highlight Color", GUILayout.Width (120));
				_map.fillColor = EditorGUILayout.ColorField (_map.fillColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Draw Outline", GUILayout.Width (120));
				_map.showOutline = EditorGUILayout.Toggle (_map.showOutline, GUILayout.Width(40));
				if (_map.showOutline) {
					GUILayout.Label ("Outline Color", GUILayout.Width (120));
					_map.outlineColor = EditorGUILayout.ColorField (_map.outlineColor, GUILayout.Width (50));
				}
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Country Names", GUILayout.Width (120));
			_map.showCountryNames = EditorGUILayout.Toggle (_map.showCountryNames);
			EditorGUILayout.EndHorizontal ();

			if (_map.showCountryNames) {
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Relative Size", GUILayout.Width (120));
				_map.countryLabelsSize = EditorGUILayout.Slider (_map.countryLabelsSize, 0.01f, 0.9f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Minimum Size", GUILayout.Width (120));
				_map.countryLabelsAbsoluteMinimumSize = EditorGUILayout.Slider (_map.countryLabelsAbsoluteMinimumSize, 0.01f, 2.5f);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Font", GUILayout.Width (120));
				_map.countryLabelsFont = (Font)EditorGUILayout.ObjectField (_map.countryLabelsFont, typeof(Font), false);
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Labels Color", GUILayout.Width (120));
				_map.countryLabelsColor = EditorGUILayout.ColorField (_map.countryLabelsColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("  Draw Shadow", GUILayout.Width (120));
				_map.showLabelsShadow = EditorGUILayout.Toggle (_map.showLabelsShadow, GUILayout.Width(40));
				if (_map.showLabelsShadow) {
					GUILayout.Label ("Shadow Color", GUILayout.Width (120));
					_map.countryLabelsShadowColor = EditorGUILayout.ColorField (_map.countryLabelsShadowColor, GUILayout.Width (50));
				}
				EditorGUILayout.EndHorizontal ();

			}

			EditorGUILayout.EndVertical (); 
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical (blackStyle);

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Show Cursor", GUILayout.Width (120));
			_map.showCursor = EditorGUILayout.Toggle (_map.showCursor, GUILayout.Width(40));

			if (_map.showCursor) {
				GUILayout.Label ("Cursor Color", GUILayout.Width (120));
				_map.cursorColor = EditorGUILayout.ColorField (_map.cursorColor, GUILayout.Width (50));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Follow Mouse", GUILayout.Width (120));
				_map.cursorFollowMouse = EditorGUILayout.Toggle (_map.cursorFollowMouse, GUILayout.Width(40));
			}
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Respect Other UI", GUILayout.Width (120));
			_map.respectOtherUI = EditorGUILayout.Toggle (_map.respectOtherUI, GUILayout.Width(40));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Allow User Drag", GUILayout.Width (120));
			_map.allowUserDrag = EditorGUILayout.Toggle (_map.allowUserDrag, GUILayout.Width(40));
			if (_map.allowUserDrag) {
				GUILayout.Label ("Speed");
				_map.mouseDragSensitivity = EditorGUILayout.Slider (_map.mouseDragSensitivity, 0.1f, 3);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("   Right Click Centers", GUILayout.Width (120));
				_map.centerOnRightClick = EditorGUILayout.Toggle (_map.centerOnRightClick, GUILayout.Width(40));
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Allow User Zoom", GUILayout.Width (120));
			_map.allowUserZoom = EditorGUILayout.Toggle (_map.allowUserZoom, GUILayout.Width(40), GUILayout.Width(40));
			if (_map.allowUserZoom) {
				GUILayout.Label ("Speed");
				_map.mouseWheelSensitivity = EditorGUILayout.Slider (_map.mouseWheelSensitivity, 0.1f, 3);
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Navigation Time", GUILayout.Width (120));
			_map.navigationTime = EditorGUILayout.Slider (_map.navigationTime, 0, 10);
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical ();

			if (_map.isDirty) {
				EditorUtility.SetDirty (target);
			}
		}

		Texture2D MakeTex (int width, int height, Color col) {
			Color[] pix = new Color[width * height];
			
			for (int i = 0; i < pix.Length; i++)
				pix [i] = col;
			
			Texture2D result = new Texture2D (width, height);
			result.SetPixels (pix);
			result.Apply ();
			
			return result;
		}



	}

}