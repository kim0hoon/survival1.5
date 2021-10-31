using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPMF
{
	public class Region
	{
		public Vector3[] points { get; set; }
		
		public Vector2 center;
		
		/// <summary>
		/// 2D rect in the billboard
		/// </summary>
		public Rect rect2D;
		
		public Material customMaterial { get; set; }
		
		public Vector2 customTextureScale, customTextureOffset;
		public float customTextureRotation;
		
		public List<Region>neighbours { get; set; }
		public IAdminEntity entity { get; set; }	// country or province index
		public int regionIndex { get; set; }
		
		public Region(IAdminEntity entity, int regionIndex) {
			this.entity = entity;
			this.regionIndex = regionIndex;
			neighbours = new List<Region>();
		}
		
		public Region Clone() {
			Region c = new Region(entity, regionIndex);
			c.center = this.center;
			c.rect2D = this.rect2D;
			c.customMaterial = this.customMaterial;
			c.customTextureScale = this.customTextureScale;
			c.customTextureOffset = this.customTextureOffset;
			c.customTextureRotation = this.customTextureRotation;
			c.points = new Vector3[points.Length];
			Array.Copy(points, c.points, points.Length);
			return c;
		}
		
	}


}