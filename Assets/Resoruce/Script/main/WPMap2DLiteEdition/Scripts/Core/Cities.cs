using UnityEngine;
using System.Collections;

namespace WPMF {
	public class City {
		public string name;
		public string country;
		public Vector2 unity2DLocation;
		public int population;

		public City (string name, string country, int population, Vector2 location) {
			this.name = name;
			this.country = country;
			this.population = population;
			this.unity2DLocation = location;
		}

		public City Clone() {
			City c = new City(name, country, population, unity2DLocation);
			return c;
		}
	}
}