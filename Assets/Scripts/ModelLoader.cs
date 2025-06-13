using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GLTFast;
using System;

public class ModelLoader : MonoBehaviour
{
    public Button loadModelButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadModelButton.onClick.AddListener(OpenFileBrowser);

        #if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
        }
        #endif
    }

    void OpenFileBrowser()
    {
        // Filtros permitidos
        SimpleFileBrowser.FileBrowser.SetFilters(true, new string[] { ".glb", ".gltf" });
        SimpleFileBrowser.FileBrowser.SetDefaultFilter(".glb");

        // Correção: parâmetros corretos para ShowLoadDialog
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(
            onSuccess: (string[] paths) =>
            {
                if (paths.Length > 0)
                {
                    Debug.Log("Arquivo selecionado: " + paths[0]);
                    LoadGLBModel(paths[0]);
                }
            },
            onCancel: () => {
                Debug.Log("Seleção cancelada");
            },
            pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
            allowMultipleSelection: false,
            initialPath: "/storage/emulated/0/",
            title: "Selecione um modelo",
            loadButtonText: "Carregar"
        );
    }

    void OnFileSelected(string path)
    {
        Debug.Log("Caminho selecionado: " + path);
        LoadGLBModel(path);
    }

    async void LoadGLBModel(string path)
{
    var gltf = new GltfImport();
    bool success = await gltf.Load(new Uri(path));

        if (success)
        {
            GameObject modelParent = new GameObject("ModeloImportado");

            // Instancia o modelo de forma assíncrona
            await gltf.InstantiateMainSceneAsync(modelParent.transform);

            // Calcula os limites do modelo (bounds) somando todos os MeshRenderers
            Bounds combinedBounds = new Bounds();
            bool initialized = false;

            foreach (var renderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                if (!initialized)
                {
                    combinedBounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }

            // Eleva o modelo para que sua base fique exatamente no Y = 0
            float bottomY = combinedBounds.min.y;
            modelParent.transform.position = new Vector3(0, -bottomY, 0);

            Debug.Log("Modelo carregado e reposicionado acima da superfície!");



            foreach (var renderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.gameObject.GetComponent<Collider>() == null)
                    renderer.gameObject.AddComponent<BoxCollider>();
            }
                
                modelParent.AddComponent<DragObject>();
        }
        else
        {
            Debug.LogError("Erro ao carregar modelo GLB.");
        }
}
    }

