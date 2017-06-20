using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour {

	public float scale;
	public Vector2 offs;
	public int octave;

	Terrain terrain;

	// Use this for initialization
	void Start () {
		terrain = GetComponent<Terrain>();
		var data = terrain.terrainData;
		var w = data.heightmapWidth;
		var h = data.heightmapHeight;
		data.SetHeights(0, 0, Perlin.ConvertArray(Perlin.CreateMap(w, h, offs, 1, scale, octave), w, h));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmos() {
		var tex = Perlin.CreateTexture(Perlin.CreateMap(128, 128, offs, 1, scale, octave), 128, 128);
		Gizmos.DrawGUITexture(new Rect(0, 0, 128, 128), tex);
	}
}
