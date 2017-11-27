using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Perlin {
	public static float[] CreatePerlin(int w, int h, float scale, Vector2 offs) {
		var noise = Enumerable.Range(0, h)
			.Select(y => Enumerable.Range(0, w).Select(x => new Vector2(x, y)))
			.SelectMany(p => p)
			.Select(p => new Vector2(p.x / w, p.y / h))
			.Select(p => p * scale + offs)
			.Select(p => Mathf.PerlinNoise(p.x, p.y))
			.Select(p => p * 2 - 1);
		return noise.ToArray();
	}

	// Combert array to texture
	public static Texture2D CreateTexture(float[] array, int w, int h) {
		Texture2D tex = new Texture2D(w, h);
		Color[] col = array.Select(n => new Color(n, n, n)).ToArray();
		tex.SetPixels(col);
		tex.Apply();
		return tex;
	} 

	public static float[] CreateMap(int w, int h, float scale, Vector2 offs, float amplitude, int octave = 1) {
		var tex = AddArray(w, h, scale, offs, amplitude, CreatePerlin(w, h, scale, offs), octave);
		var max = tex.Max();
		var min = tex.Min();
		return tex.Select(x => Mathf.InverseLerp(min, max, x)).ToArray();
	}

	static float[] AddArray(int w, int h, float scale, Vector2 offs, float amplitude, float[] baseTex, int octave) {
		if (octave <= 0) return new float[w * h];

		var tex = Enumerable.Range(1, octave)
			.Select(x => CreatePerlin(w, h, scale * Mathf.Pow(2, x), offs).Select(y => y * Mathf.Pow(amplitude, x)))
			.Aggregate((x, elem) => x.Zip(elem, (y, z) => y + z))
			.ToArray();
		return tex;
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
