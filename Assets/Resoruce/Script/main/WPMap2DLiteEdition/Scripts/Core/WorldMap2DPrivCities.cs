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

	public partial class WorldMap2D : MonoBehaviour {

		const float CITY_HIT_PRECISION = 0.00075f;

		// resources
		Material citiesMat;
		GameObject citySpot;

		// gameObjects
		GameObject citiesLayer;


	#region Drawing stuff

		/// <summary>
		/// Redraws the cities. This is automatically called by Redraw(). Used internally by the Map Editor. You should not need to call this method directly.
		/// </summary>
		public void DrawCities () {
			// Create cities layer
			Transform t = transform.Find ("Cities");
			if (t != null)
				DestroyImmediate (t.gameObject);
			citiesLayer = new GameObject ("Cities");
			citiesLayer.hideFlags = HideFlags.DontSave;
			citiesLayer.transform.SetParent (transform, false);
			citiesLayer.transform.localPosition = Vector3.back * 0.001f;
			citiesLayer.layer = gameObject.layer;

			// Draw city marks
			numCitiesDrawn = 0;
			int minPopulation = _minPopulation * 1000;
			for (int k=0; k<cities.Count; k++) {
				City city = cities [k];
				if (city.population >= minPopulation) {
					GameObject cityObj = Instantiate (citySpot);
					cityObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
					cityObj.layer = citiesLayer.layer;
					cityObj.GetComponent<Renderer> ().sharedMaterial = citiesMat;
					cityObj.name = "City" + k;
					cityObj.transform.SetParent (citiesLayer.transform, false);
					cityObj.transform.localPosition = city.unity2DLocation;
					numCitiesDrawn++;
				}
			}

			// Toggle cities layer visibility according to settings
			citiesLayer.SetActive (_showCities);
			CityScaler cityScaler = citiesLayer.GetComponent<CityScaler>() ?? citiesLayer.AddComponent<CityScaler>();
			cityScaler.map = this;
			if (_showCities) {
				cityScaler.ScaleCities();
			}
		}

	#endregion

		#region Internal API

		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		int GetCityNearPoint(Vector3 localPoint) {
			float rl = localPoint.x - CITY_HIT_PRECISION;
			float rr = localPoint.x + CITY_HIT_PRECISION;
			float rt = localPoint.y + CITY_HIT_PRECISION;
			float rb = localPoint.y - CITY_HIT_PRECISION;
			for (int c=0;c<cities.Count;c++) {
				Vector2 cityLoc = cities[c].unity2DLocation;
				if (cityLoc.x>rl && cityLoc.x<rr && cityLoc.y>rb && cityLoc.y<rt) {
					return c;
				}
			}
			return -1;
		}

		/// <summary>
		/// Returns the file name corresponding to the current city data file (cities50, cities110)
		/// </summary>
		public string GetCityFileName() {
			return "cities110.txt";
		}


		#endregion
		
		
	}
	
}