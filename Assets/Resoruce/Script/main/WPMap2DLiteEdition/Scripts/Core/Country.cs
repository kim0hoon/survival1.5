using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {
	public class Country: IAdminEntity {

        /// <summary>
        /// Country name.
        /// </summary>
		public string name { get; set; }
       
	
       

	


        /// <summary>
        /// List of all regions for the admin entity.
        /// </summary>
        public List<Region> regions { get; set; }
		
		/// <summary>
		/// Center of the admin entity in the plane
		/// </summary>
		public Vector2 center { get; set; }

		/// <summary>
		/// Index of the biggest region
		/// </summary>
		public int mainRegionIndex { get; set; }

		/// <summary>
		/// Continent name.
		/// </summary>
		public string continent;

		/// <summary>
		/// Optional custom label. It set, it will be displayed instead of the country name.
		/// </summary>
		public string customLabel; 

		/// <summary>
		/// Set it to true to specify a custom color for the label.
		/// </summary>
		public bool labelColorOverride;

		/// <summary>
		/// The color of the label.
		/// </summary>
		public Color labelColor = Color.white;
		Font _labelFont;
		Material _labelShadowFontMaterial;

		/// <summary>
		/// Internal method used to obtain the shadow material associated to a custom Font provided.
		/// </summary>
		/// <value>The label shadow font material.</value>
		public Material labelFontShadowMaterial { get { return _labelShadowFontMaterial; } }

		/// <summary>
		/// Optional font for this label. Note that the font material will be instanced so it can change color without affecting other labels.
		/// </summary>
		public Font labelFontOverride { 
			get {
				return _labelFont;
			}
			set {
				if (value != _labelFont) {
					_labelFont = value;
					if (_labelFont != null) {
						Material fontMaterial = GameObject.Instantiate (_labelFont.material);
						fontMaterial.hideFlags = HideFlags.DontSave;
						_labelFont.material = fontMaterial;
						_labelShadowFontMaterial = GameObject.Instantiate (fontMaterial);
						_labelShadowFontMaterial.hideFlags = HideFlags.DontSave;
						_labelShadowFontMaterial.renderQueue--;
					}
				}
			}
		}

		#region internal fields
		// Used internally. Don't change fields below.
		public GameObject labelGameObject;
		public float labelMeshWidth, labelMeshHeight;
		public Vector2 labelMeshCenter;
		#endregion

		public Country (string name, string continent) {
			this.name = name;
			this.continent = continent;
			
			this.regions = new List<Region> ();
		}

		public Country Clone() {
			Country c = new Country(name, continent);
			c.center = center;
			c.regions = regions;
			c.customLabel = customLabel;
			c.labelColor = labelColor;
			c.labelColorOverride = labelColorOverride;
			c.labelFontOverride = labelFontOverride;
			return c;
		}

	}

}