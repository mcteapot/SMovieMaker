using UnityEngine;
using UnityEditor;

public class Force2dAudio : AssetPostprocessor {
	void OnPreprocessAudio () {
		AudioImporter importer = (AudioImporter) assetImporter;
		importer.threeD = false;
	}
}