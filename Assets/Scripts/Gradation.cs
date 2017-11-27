using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gradation {

	public static float[] CreateGradation(int w, int h, System.Func<Vector2, float> func) {
		return Enumerable.Range(0, h)
			.Select(x => x / (float)h)
			.Select(y => Enumerable.Range(0, w).Select(x => x / (float)w).Select(x => new Vector2(x, y)))
			.SelectMany(x => x)
			.Select(func).ToArray();
	}

}
