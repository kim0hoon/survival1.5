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

		/// <summary>
		/// Country look up dictionary. Used internally for fast searching of country names.
		/// </summary>
		Dictionary<string, int>_countryLookup;
		int lastCountryLookupCount = -1;

		Dictionary<string, int>countryLookup {
			get {
				if (_countryLookup != null && countries.Length == lastCountryLookupCount)
					return _countryLookup;
				if (_countryLookup == null) {
					_countryLookup = new Dictionary<string,int> ();
				} else {
					_countryLookup.Clear ();
				}
				for (int k=0; k<countries.Length; k++)
					_countryLookup.Add (countries [k].name, k);
				lastCountryLookupCount = _countryLookup.Count;
				return _countryLookup;
			}
		}

		// resources
		Material frontiersMat, hudMatCountry;

		// gameObjects
		GameObject countryRegionHighlightedObj;
		GameObject frontiersLayer;
		// caché and gameObject lifetime control
		Vector3[][] frontiers;
		int[][] frontiersIndices;

		void ReadCountriesPackedString () {
			lastCountryLookupCount = -1;
			string frontiersFileName = "Geodata/countries110";
			TextAsset ta = Resources.Load<TextAsset> (frontiersFileName);
			string s = ta.text;
			string[] countryList = s.Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			int countryCount = countryList.Length;
			countries = new Country[countryCount];
			for (int k=0; k<countryCount; k++) {
				string[] countryInfo = countryList [k].Split (new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
				string name = countryInfo [0];
				string continent = countryInfo [1];
				Country country = new Country (name, continent);
				string[] regions = countryInfo [2].Split (new char[] {'*'}, StringSplitOptions.RemoveEmptyEntries);
				int regionCount = regions.Length;
				country.regions = new List<Region> ();
				float maxVol = 0;
				for (int r=0; r<regionCount; r++) {
					string[] coordinates = regions [r].Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					int coorCount = coordinates.Length;
					Vector3 min = Vector3.one * 10;
					Vector3 max = -min;
					Region countryRegion = new Region (country, country.regions.Count);
					countryRegion.points = new Vector3[coorCount];
					for (int c=0; c<coorCount; c++) {
						string[] coordinate = coordinates [c].Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						float x = float.Parse (coordinate [0]) / MAP_PRECISION;
						float y = float.Parse (coordinate [1]) / MAP_PRECISION;
						if (x < min.x)
							min.x = x;
						if (x > max.x)
							max.x = x;
						if (y < min.y)
							min.y = y;
						if (y > max.y)
							max.y = y;
						Vector2 point = new Vector2 (x, y);
						countryRegion.points [c] = point;
					}
					Vector3 normRegionCenter = (min + max) * 0.5f;
					countryRegion.center = normRegionCenter; 

					// Calculate bounding rect
					float left, top, bottom, right;
					left = bottom = 1000;
					top = right = -1000;
					
					int step = 1 + coorCount / 8;
					for (int c=0; c<coorCount; c+=step) {
						Vector3 p = countryRegion.points [c];
						if (p.x < left)
							left = p.x;
						if (p.x > right)
							right = p.x;
						if (p.y < bottom)
							bottom = p.y;
						if (p.y > top)
							top = p.y;
					}
					countryRegion.rect2D = new Rect (left, top,  Math.Abs (right - left), Mathf.Abs (top - bottom));

                    
					float vol = (max - min).sqrMagnitude;
					if (vol > maxVol) {
						maxVol = vol;
						country.mainRegionIndex = country.regions.Count;
						country.center = countryRegion.center;
					}
                    
					country.regions.Add (countryRegion);
				}
				countries[k] = country;
			}

			OptimizeFrontiers ();
		}


		/// <summary>
		/// Used internally by the Map Editor. It will recalculate de boundaries and optimize frontiers based on new data of countries array
		/// </summary>
		public void RefreshCountryDefinition (int countryIndex, List<Region>filterRegions)
		{
			lastCountryLookupCount = -1;
			if (countryIndex >= 0 && countryIndex < countries.Length) {
				float maxVol = 0;
				Country country = countries [countryIndex];
				int regionCount = country.regions.Count;
				for (int r=0; r<regionCount; r++) {
					Region countryRegion = country.regions [r];
					countryRegion.entity = country;	// just in case one country has been deleted
					countryRegion.regionIndex = r;				// just in case a region has been deleted
					int coorCount = countryRegion.points.Length;
					Vector3 min = Vector3.one * 10;
					Vector3 max = -min;
					for (int c=0; c<coorCount; c++) {
						float x = countryRegion.points [c].x;
						float y = countryRegion.points [c].y;
						if (x < min.x)
							min.x = x;
						if (x > max.x)
							max.x = x;
						if (y < min.y)
							min.y = y;
						if (y > max.y)
							max.y = y;
					}
					Vector3 normRegionCenter = (min + max) * 0.5f;
					countryRegion.center = normRegionCenter; 
					
					// Calculate bounding rect
					float left, top, bottom, right;
					left = bottom = 1000;
					top = right = -1000;
					
					int step = 1 + coorCount / 8;
					for (int c=0; c<coorCount; c+=step) {
						Vector3 p = countryRegion.points [c];
						if (p.x < left)
							left = p.x;
						if (p.x > right)
							right = p.x;
						if (p.y < bottom)
							bottom = p.y;
						if (p.y > top)
							top = p.y;
					}
					countryRegion.rect2D = new Rect (left, top, Math.Abs (right - left), Mathf.Abs (top - bottom));
					float vol = (max - min).sqrMagnitude;
					if (vol > maxVol) {
						maxVol = vol;
						country.mainRegionIndex = r;
						country.center = countryRegion.center;
					}
				}
			}
			OptimizeFrontiers (filterRegions);
			DrawFrontiers ();
		}

		void OptimizeFrontiers () {
			OptimizeFrontiers(null);
		}

		void OptimizeFrontiers (List<Region>filterRegions) {
			if (frontiersPoints==null) {
				frontiersPoints = new List<Vector3>(1000000); // needed for high-def resolution map
			} else {
				frontiersPoints.Clear();
			}
			if (frontiersCacheHit==null) {
				frontiersCacheHit = new Dictionary<double, Region>(500000); // needed for high-resolution map
			} else {
				frontiersCacheHit.Clear();
			}

			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				for (int r=0; r<country.regions.Count; r++) {
					Region region = country.regions [r];
					if (filterRegions==null || filterRegions.Contains(region)) {
						region.entity = country;
						region.regionIndex = r;
						region.neighbours.Clear();
					}
				}
			}

			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				for (int r=0; r<country.regions.Count; r++) {
					Region region = country.regions [r];
					if (filterRegions==null || filterRegions.Contains(region)) {
					int numPoints = region.points.Length - 1;
					for (int i = 0; i<numPoints; i++) {
//						Vector3 p0 = region.points [i];
//						Vector3 p1 = region.points [i + 1];
							double v = (region.points [i].x + region.points [i + 1].x) + MAP_PRECISION * (region.points [i].y + region.points [i + 1].y);
						if (frontiersCacheHit.ContainsKey(v)) { // add neighbour references
							Region neighbour = frontiersCacheHit[v];
							if (neighbour!=region) {
								if (!region.neighbours.Contains(neighbour)) {
									region.neighbours.Add(neighbour);
									neighbour.neighbours.Add (region);
								}
							}
						} else {
							frontiersCacheHit.Add(v,region);
								frontiersPoints.Add (region.points [i]);
								frontiersPoints.Add (region.points [i + 1]);
						}
					}
					// Close the polygon
					frontiersPoints.Add (region.points [numPoints]);
					frontiersPoints.Add (region.points [0]);
				}
				}
			}

			int meshGroups = (frontiersPoints.Count / 65000) + 1;
			int meshIndex = -1;
			frontiersIndices = new int[meshGroups][];
			frontiers = new Vector3[meshGroups][];
			for (int k=0; k<frontiersPoints.Count; k+=65000) {
				int max = Mathf.Min (frontiersPoints.Count - k, 65000); 
				frontiers [++meshIndex] = new Vector3[max];
				frontiersIndices [meshIndex] = new int[max];
				for (int j=k; j<k+max; j++) {
					frontiers [meshIndex] [j - k] = frontiersPoints [j];
					frontiersIndices [meshIndex] [j - k] = j - k;
				}
			}
		}

	#region Drawing stuff

		int GetCacheIndexForCountryRegion (int countryIndex, int regionIndex) {
			return countryIndex * 1000 + regionIndex;
		}


		void DrawFrontiers () {
			if (!gameObject.activeInHierarchy)
				return;
			
			// Create frontiers layer
			Transform t = transform.Find ("Frontiers");
			if (t != null)
				DestroyImmediate (t.gameObject);
			frontiersLayer = new GameObject ("Frontiers");
			frontiersLayer.hideFlags = HideFlags.DontSave;
			frontiersLayer.transform.SetParent (transform, false);
			frontiersLayer.transform.localPosition = Vector3.zero;
			frontiersLayer.transform.localRotation = Quaternion.Euler (Vector3.zero);
			frontiersLayer.layer = gameObject.layer;

			for (int k=0; k<frontiers.Length; k++) {
				GameObject flayer = new GameObject ("flayer");
				flayer.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				flayer.transform.SetParent (frontiersLayer.transform, false);
				flayer.transform.localPosition = Vector3.zero;
				flayer.transform.localRotation = Quaternion.Euler (Vector3.zero);
				flayer.layer = frontiersLayer.layer;

				Mesh mesh = new Mesh ();
				mesh.vertices = frontiers [k];
				mesh.SetIndices (frontiersIndices [k], MeshTopology.Lines, 0);
				mesh.RecalculateBounds ();
				mesh.hideFlags = HideFlags.DontSave;

				MeshFilter mf = flayer.AddComponent<MeshFilter> ();
				mf.sharedMesh = mesh;

				MeshRenderer mr = flayer.AddComponent<MeshRenderer> ();
				mr.receiveShadows = false;
				mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//				mr.useLightProbes = false;
				mr.sharedMaterial = frontiersMat;
			}

			// Toggle frontiers visibility layer according to settings
			frontiersLayer.SetActive (_showFrontiers);
		}

		#endregion



		#region Country highlighting

		void HideCountryRegionHighlight () {
			if (_countryRegionHighlighted==null)
				return;
			if (countryRegionHighlightedObj != null) {
				if (_countryRegionHighlighted!=null && _countryRegionHighlighted.customMaterial!=null) {
				
					ApplyMaterialToSurface (countryRegionHighlightedObj, _countryRegionHighlighted.customMaterial);
				} else {
					countryRegionHighlightedObj.SetActive (false);
				}
				countryRegionHighlightedObj = null;
			}
			_countryHighlighted = null;
			_countryHighlightedIndex = -1;
			_countryRegionHighlighted = null;
			_countryRegionHighlightedIndex = -1;
		}

		


		/// <summary>
		/// Disables all country regions highlights. This doesn't remove custom materials.
		/// </summary>
		public void HideCountryRegionHighlights (bool destroyCachedSurfaces) {
			HideCountryRegionHighlight();
			if (countries==null) return;
			for (int c=0;c<countries.Length;c++) {
				Country country = countries[c];
				for (int cr=0;cr<country.regions.Count;cr++) {
					Region region = country.regions[cr];
					int cacheIndex = GetCacheIndexForCountryRegion(c, cr);
					if (surfaces.ContainsKey(cacheIndex)) {
						GameObject surf = surfaces[cacheIndex];
						if (surf==null) {
							surfaces.Remove(cacheIndex);
						} else {
							if (destroyCachedSurfaces) {
								surfaces.Remove(cacheIndex);
								DestroyImmediate(surf);
							} else {
								if (region.customMaterial==null) {
									surf.SetActive(false);
								} else {
									ApplyMaterialToSurface (surf, region.customMaterial);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Highlights the country region specified. Returns the generated highlight surface gameObject.
		/// Internally used by the Map UI and the Editor component, but you can use it as well to temporarily mark a country region.
		/// </summary>
		/// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
		public GameObject HighlightCountryRegion (int countryIndex, int regionIndex, bool refreshGeometry, bool drawOutline) {
			//if (countryRegionHighlightedObj!=null) HideCountryRegionHighlight();
			if (countryIndex<0 || countryIndex>=countries.Length || regionIndex<0 || regionIndex>=countries[countryIndex].regions.Count) return null;
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (refreshGeometry && existsInCache) {
				GameObject obj = surfaces [cacheIndex];
				surfaces.Remove(cacheIndex);
				DestroyImmediate(obj);
				existsInCache = false;
			}
			if (_enableCountryHighlight) {
			if (existsInCache) {
				countryRegionHighlightedObj = surfaces [cacheIndex];
				if (countryRegionHighlightedObj==null) {
					surfaces.Remove(cacheIndex);
				} else {
					if (!countryRegionHighlightedObj.activeSelf)
						countryRegionHighlightedObj.SetActive (true);
					Renderer rr = countryRegionHighlightedObj.GetComponent<Renderer> ();
					if (rr.sharedMaterial!=hudMatCountry)
						rr.sharedMaterial = hudMatCountry;
				}
			} else {
				countryRegionHighlightedObj = GenerateCountryRegionSurface (countryIndex, regionIndex, hudMatCountry, Vector2.one, Vector2.zero, 0, drawOutline);
			}
			}
			_countryHighlightedIndex = countryIndex;
			_countryRegionHighlighted = countries[countryIndex].regions[regionIndex];
			_countryRegionHighlightedIndex = regionIndex;
			_countryHighlighted = countries[countryIndex];
			return countryRegionHighlightedObj;
		}

		GameObject GenerateCountryRegionSurface (int countryIndex, int regionIndex, Material material, bool drawOutline) {
			return GenerateCountryRegionSurface(countryIndex, regionIndex, material, Vector2.one, Vector2.zero, 0, drawOutline);
		}

		GameObject GenerateCountryRegionSurface (int countryIndex, int regionIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool drawOutline) {
			if (countryIndex<0 || countryIndex>=countries.Length) return null;
			Country country = countries[countryIndex];
            Region region = country.regions [regionIndex];
         

			// Triangulate to get the polygon vertex indices
			int[] surfIndices = Triangulator.GetPoints (region.points);

			// Prepare surface cache entry and deletes older surface if exists
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			string cacheIndexSTR = cacheIndex.ToString();
			Transform t = surfacesLayer.transform.Find(cacheIndexSTR);
			if (t!=null) DestroyImmediate(t.gameObject); // Deletes potential residual surface

			// Creates surface mesh
			GameObject surf = Drawing.CreateSurface (cacheIndexSTR, region.points, surfIndices, material, region.rect2D, textureScale, textureOffset, textureRotation);									
			surf.transform.SetParent (surfacesLayer.transform, false);
			surf.transform.localPosition = Vector3.zero;
			surf.transform.localRotation = Quaternion.Euler (Vector3.zero);
			surf.layer = gameObject.layer;
			if (surfaces.ContainsKey(cacheIndex)) surfaces.Remove(cacheIndex);
			surfaces.Add (cacheIndex, surf);

			// Draw polygon outline
			if (drawOutline) {
				int[] indices = new int[region.points.Length];
				for (int k=0; k<indices.Length; k++) {
					indices [k] = k;
				}

				GameObject boldFrontiers = new GameObject ("boldFrontiers");
				boldFrontiers.hideFlags = HideFlags.DontSave;
				boldFrontiers.transform.SetParent (surf.transform, false);
				boldFrontiers.transform.localPosition = Vector3.zero;
				boldFrontiers.transform.localRotation = Quaternion.Euler (Vector3.zero);
				boldFrontiers.layer = surf.layer;

				Mesh mesh = new Mesh ();
				mesh.vertices = region.points; //region.points;
				mesh.SetIndices (indices, MeshTopology.LineStrip, 0);
				mesh.RecalculateBounds ();
				mesh.hideFlags = HideFlags.DontSave;
				
				MeshFilter mf = boldFrontiers.AddComponent<MeshFilter> ();
				mf.sharedMesh = mesh;
				
				MeshRenderer mr = boldFrontiers.AddComponent<MeshRenderer> ();
				mr.receiveShadows = false;
				mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//				mr.useLightProbes = false;
				mr.sharedMaterial = outlineMat;
			}
			return surf;
		}

	#endregion

	}

}