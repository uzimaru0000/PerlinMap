using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Perlin {
	// Create a perlin noise
	public static float[,] CreatePerlin(int w, int h, float scale = 1.0f, Vector2 offs = new Vector2()) {
		float[,] noise = new float[w, h];
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				var dx = offs.x + (float)x / w * scale;
				var dy = offs.y + (float)y / h * scale;
				noise[x, y] = Mathf.PerlinNoise(dx, dy);
			}
		}
		return noise;
	}

	// Combert array to texture
	public static Texture2D CreateTexture(float[,] array) {
		int w = array.GetLength(0),
			h = array.GetLength(1);
		Texture2D tex = new Texture2D(w, h);
		Color[] col = new Color[w * h];
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				col[x + y * w] = new Color(array[x, y], array[x, y], array[x, y]);
			}
		}
		tex.SetPixels(col);
		tex.Apply();
		return tex;
	} 

	public static float[,] CreateMap(int w, int h, Vector2 offs, float amplitude, int octave = 1) {
		var baseTex = Array2List<float>(CreatePerlin(w, h, 1, offs));
		System.Func<List<List<float>>, int, List<List<float>>> func = null;
		func = (bt, n) => {
			if (n == 0) {
				return bt;
			} else {
				var tex = Array2List<float>(CreatePerlin(w, h, Mathf.Pow(2, n), offs));
				return func(
					baseTex.Select(x => x.Join());
				);
			}
		};

		return func(baseTex, octave);
	}

	static List<List<T>> Array2List<T>(T[,] arr) {
		var list = new List<List<T>>();
		for (int x = 0; x < arr.GetLength(0); x++) {
			list.Add(new List<T>());
			for (int y = 0; y < arr.GetLength(1); y++) {
				list[x].Add(arr[x, y]);
			}
		}
		return list;
	}

	static T[,] List2Array<T>(List<List<T>> list) {
		var arr = list.ToArray();
		var result = new T[list.Count, list[0].Count];
		for (int i = 0; i < arr.GetLength(0); i++) {
			var a = arr[i].ToArray();
			for (int j = 0; j < a.Length; j++) {
				result[i, j] = a[j];
			}
		}
		return result;
	}
}
