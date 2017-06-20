using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Perlin {
	// Create a perlin noise
	public static float[] CreatePerlin(int w, int h, float scale = 1.0f, Vector2 offs = new Vector2()) {
		float[] noise = new float[w * h];
		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				var dx = offs.x + (float)x / w * scale;
				var dy = offs.y + (float)y / h * scale;
				noise[x + y * w] = Mathf.PerlinNoise(dx, dy);
			}
		}
		return noise;
	}

	// Combert array to texture
	public static Texture2D CreateTexture(float[] array, int w, int h) {
		Texture2D tex = new Texture2D(w, h);
		Color[] col = new Color[array.Length];
		for (int i = 0; i < array.Length; i++) {
			col[i] = new Color(array[i], array[i], array[i]);
		}
		tex.SetPixels(col);
		tex.Apply();
		return tex;
	} 

	public static float[] CreateMap(int w, int h, Vector2 offs, float amplitude, float scale = 1, int octave = 1) {
		var baseTex = CreatePerlin(w, h, scale, offs);
		System.Action<int> func = null;
		func = (n) => {
			if (n <= 0) {
				return;
			} else {
				var tex = CreatePerlin(w, h, Mathf.Pow(2, n), offs);
				for (var i = 0; i < baseTex.Length; i++) {
					baseTex[i] *= tex[i] * amplitude / n;
				}
			}
		};
		func(octave);
		var max = baseTex.Max();
		return baseTex.Select(x => x / max).ToArray();
	}

	public static float[,] ConvertArray(float[] array, int w, int h) {
		var tex = new float[w, h];
		for (var x = 0; x < w; x++) {
			for (var y = 0; y < h; y++) {
				tex[x, y] = array[x + y * w];
			}
		}
		return tex;
	}
}
