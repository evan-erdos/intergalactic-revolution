
using UnityEditor;

public class EditorImporter : AssetPostprocessor {
    public void OnPreprocessModel() {
        var importer = base.assetImporter as ModelImporter;
        importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
    }
}
