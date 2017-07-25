
using UnityEditor;

public class ModelImporterEditor : AssetPostprocessor {
    public void OnPreprocessModel() {
        var importer = base.assetImporter as ModelImporter;
        importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
        importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
    }
}
