using System.IO;
using GLTFast;
using UnityEngine;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class ModelLoaderButton : MonoBehaviour
{

    private string selectedModelPath = null;
    private bool readyToSpawn = false;

    void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;    
    }

    void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        if (finger.index != 0)
                {
                    Vector2 touchPos = Input.GetTouch(0).position;
                    TrySpawnModelAt(touchPos);
                }
        
    }

    void TrySpawnModelAt(Vector2 screenPosition)
    {
        

            LoadGLBModel(selectedModelPath, screenPosition);
            readyToSpawn = false;
        
    
        
        
    }

    public void SpawnModel()
    {
        LoadGLBModel(selectedModelPath, new Vector3(0,0,0));
    }
    void Start()
    {   
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
            
        }
        #endif
        
    }

    public void AbrirSeletorDeModelo()
    {
        
        SimpleFileBrowser.FileBrowser.SetFilters(true, new string[] { ".glb", ".gltf" });
        SimpleFileBrowser.FileBrowser.SetDefaultFilter(".glb");

        SimpleFileBrowser.FileBrowser.ShowLoadDialog(
            onSuccess: (string[] paths) =>
            {
                if (paths.Length > 0)
                {
                    selectedModelPath = paths[0];
                    readyToSpawn = true;
                    SpawnModel();
                    Debug.Log(selectedModelPath);
                    Debug.Log("Modelo selecionado. Toque na superfície AR para instanciar.");
                }
            },
            onCancel: () => Debug.Log("Seleção cancelada."),
            pickMode: SimpleFileBrowser.FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: Application.persistentDataPath,
            title: "Selecione um modelo",
            loadButtonText: "Carregar"
        );

    }

    
    async void LoadGLBModel(string path, Vector3 spawnPosition)
    {
        var gltf = new GltfImport();
        bool success = await gltf.Load(new System.Uri(path));

        if (success)
        {
            GameObject modelParent = new GameObject("ModeloImportado");
            await gltf.InstantiateMainSceneAsync(modelParent.transform);
            modelParent.AddComponent<DragObject>();
            modelParent.AddComponent<DontDestroy>();
            modelParent.AddComponent<SkinnedMeshRenderer>();

            Bounds bounds = new Bounds();
            bool initialized = false;

            foreach (var renderer in modelParent.GetComponentsInChildren<MeshRenderer>())
            {
                if (!initialized)
                {
                    bounds = renderer.bounds;
                    initialized = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }

                if (!renderer.GetComponent<Collider>())
                    renderer.gameObject.AddComponent<BoxCollider>();


                renderer.enabled = true;

            }

            // Ajusta o modelo para que fique alinhado com o chão detectado
            float bottomY = bounds.min.y;
            spawnPosition.y -= bottomY;
            modelParent.transform.position = spawnPosition;

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = spawnPosition;
            sphere.transform.localScale = Vector3.one * 0.05f;
            sphere.GetComponent<Renderer>().material.color = Color.red;

            Debug.Log("Modelo instanciado em: " + spawnPosition);
        }
        else
        {
            Debug.LogError("Erro ao carregar modelo GLB.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
