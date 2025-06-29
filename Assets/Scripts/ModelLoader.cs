using UnityEngine;
using GLTFast;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.Rendering;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class ModelLoader : MonoBehaviour
{
    public ARRaycastManager arRaycastManager= null; // Referência ao ARRaycastManager da cena
    public Material overrideMaterial= null;
    private string selectedModelPath = null;
    private bool readyToSpawn = false;
    

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        
    }

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

        if(arRaycastManager.Raycast(finger.currentTouch.screenPosition,hits,
        TrackableType.PlaneWithinPolygon)){
            if (readyToSpawn)
            {
                if (finger.index != 0)
                {
                    Vector2 touchPos = Input.GetTouch(0).position;
                    TrySpawnModelAt(touchPos);
                }
            }
        }
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

    void Update()
    {
        if (readyToSpawn)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began||Input.GetMouseButtonDown(0))
            {
                if (Input.touchCount > 0)
                {
                    Vector2 touchPos = Input.GetTouch(0).position;
                    TrySpawnModelAt(touchPos);
                }
                else
                {
                    
                    TrySpawnModelAt(Input.mousePosition);
                }
                
            }
            
        }
    }

    // CHAMADO PELO BOTÃO
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

    void TrySpawnModelAt(Vector2 screenPosition)
    {
        if (arRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            LoadGLBModel(selectedModelPath, hitPose.position);
            readyToSpawn = false;
        }
        else
        {
            Debug.LogWarning("Toque não acertou nenhum plano AR.");
        }
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

                if (overrideMaterial != null)
                renderer.material = overrideMaterial;
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
}
