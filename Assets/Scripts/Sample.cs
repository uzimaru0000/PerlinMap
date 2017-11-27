using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class Sample : MonoBehaviour {

	public enum MaskType {
		None,
		Rect,
		Circle
	}

	Terrain _terrain;
	Terrain terrain {
		get {
			if (!_terrain) _terrain = GetComponent<Terrain>();
			return _terrain;
		}
	}

	public float scale;
	public Vector2 offs;
	public int octave;
	public float amplitude;
	public MaskType type;
	public float maskScale;
	public float maskOffs;
	public bool isMaskOnly;
	public bool isDrawTex;
	public AnimationCurve curve;
	public TextureData[] textureDatas;

	Texture2D texture;

	[ContextMenu("CreateTerrain")]
	void CreateTerrain() {
		var w = terrain.terrainData.heightmapWidth;
		var h = terrain.terrainData.heightmapHeight;
		var noise = Perlin.CreateMap(w, h, scale, offs, amplitude, octave);
		var mask = Gradation.CreateGradation(w, h, v => {
			switch(type) {
				case MaskType.Rect:
					return curve.Evaluate(RectMask(v));
				case MaskType.Circle:
					return curve.Evaluate(CircleMask(v));
				default:
					return 0.0f;
			}
		});
		var tex = noise.Zip(mask, (x, y) => new Tuple<float, float>(x, y)).Select(t => t.item1 * t.item2).ToArray();
		terrain.terrainData.SetHeights(0, 0, Perlin.ConvertArray(tex, w, h));
	}

	[ContextMenu("CreateImage")]
	void CreateImage() {
		var data = texture.EncodeToPNG();

		string filePath = EditorUtility.SaveFilePanel("Save Image", "", "noize.png", "png");

		if (filePath.Length > 0) {
			File.WriteAllBytes(filePath, data);
		}
	}

	[ContextMenu("PaintTexture")]
	void PaintTexture() {
		var terrainData = terrain.terrainData;
		var splatData = textureDatas.Select(x => new SplatPrototype {
			texture = x.texture,
			normalMap = x.normalMap,
			tileOffset = x.tileOffs,
			tileSize = x.tileSize
		}).ToArray();
		terrainData.splatPrototypes = splatData;

		// var alphaTexture = terrainData.GetHeights(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight)
		// 	.Cast<float>()
		// 	.Select(x => {
		// 		var arr = new float[splatData.Length];
		// 		var n = 0;
		// 		for (n = 0; n < splatData.Length; n++) {
		// 			if (textureDatas[n].max > x) break;
		// 		}
		// 		arr[n] = 1.0f;
		// 		return arr;
		// 	}).ToArray();

		var alphaTexture = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, textureDatas.Length];
		var data = terrainData.GetHeights(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
		for (var i = 0; i < terrainData.alphamapWidth; i++) {
			for (var j = 0; j < terrainData.alphamapHeight; j++) {
				var n = 0;
				for (n = 0; n < textureDatas.Length; n++) {
					if (textureDatas[n].max > data[i, j]) break;
				}
				alphaTexture[i, j, n] = 1.0f;
			}
		}

		terrainData.SetAlphamaps(0, 0, alphaTexture);
		// terrainData.SetAlphamaps(0, 0, ConvertArray(alphaTexture, terrainData.alphamapWidth, terrainData.alphamapHeight));
		
	}

	void OnDrawGizmos() {
		if (isDrawTex) {
			var w = 128;
			var h = 128;
			var noise = Perlin.CreateMap(w, h, scale, offs, amplitude, octave);
			var mask = Gradation.CreateGradation(w, h, v => {
				switch (type) {
					case MaskType.Rect:
						return RectMask(v);
					case MaskType.Circle:
						return CircleMask(v);
					default:
						return 1.0f;
				}
			});
			var tex = noise.Zip(mask, (x, y) => x * y)
						   .Select(x => curve.Evaluate(x))
						   .ToArray();

			texture = Perlin.CreateTexture(isMaskOnly ? mask : tex, w, h);
			Gizmos.DrawGUITexture(new Rect(0, 0, 128, 128), texture);
		}
	}

	float RectMask(Vector2 v) {
		var x = maskOffs - Mathf.Abs(v.x * 2 - 1);
		var y = maskOffs - Mathf.Abs(v.y * 2 - 1);
		return Mathf.Clamp01(Mathf.Min(x, y) / maskScale);
	}

	float CircleMask(Vector2 v) {
		return Mathf.Clamp01((maskOffs - Mathf.Sqrt(Mathf.Pow(v.x - 0.5f, 2) + Mathf.Pow(v.y - 0.5f, 2))) / maskScale);
	}

	float[,,] ConvertArray(float[][] arr, int w, int h) {
		var d = arr[0].Length;
		var newArr = new float[w, h, d];
		for (var x = 0; x < w; x++) {
			for (var y = 0; y < h; y++) {
				for (var z = 0; z < d; z++) {
					newArr[x, y, z] = arr[x + y * w][z];
				}
			}
		}

		return newArr;
	}
}

[System.Serializable]
public class TextureData {
	public string name;
	[Range(0.0f, 1.0f)]
	public float max;
	public Texture2D texture;
	public Texture2D normalMap;
	public Vector2 tileSize;
	public Vector2 tileOffs;
}