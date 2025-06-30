using UnityEngine;
using System.IO;
using System.Collections;
using GLTFast;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using NativeFilePickerNamespace;

public class ModelLoaderButton : MonoBehaviour
{
    private string selectedModelPath = null;
    private bool readyToSpawn = false;

    void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += OnTouch;
    }

    void OnDisable()
    {
        EnhancedTouch.Touch.onFingerDown -= OnTouch;
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.TouchSimulation.Disable();
    }

    void OnTouch(EnhancedTouch.Finger finger)
    {
        if (!readyToSpawn || finger.index != 0)
            return;

        Vector2 screenPosition = finger.screenPosition;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 1f));

        StartCoroutine(LoadGLBModel(selectedModelPath, worldPosition));
        readyToSpawn = false;
    }

    public void AbrirSeletorDeModelo() // Chame esta função por código
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageRead);
        }
#endif

        NativeFilePicker.PickFile((path) =>
{
    if (string.IsNullOrEmpty(path))
    {
        Debug.Log("Seleção cancelada.");
        return;
    }

    string ext = Path.GetExtension(path).ToLowerInvariant();
    if (ext != ".glb" && ext != ".gltf")
    {
        Debug.LogWarning("Arquivo inválido: " + ext);
        return;
    }

        StartCoroutine(CopyToPersistentAndPrepare(path));

        }, new string[] { "*/*" }); // permite escolher qualquer coisa
    }

    IEnumerator CopyToPersistentAndPrepare(string originalPath)
    {
        string fileName = Path.GetFileName(originalPath);
        string finalPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.Copy(originalPath, finalPath, true);
        }
        catch (IOException e)
        {
            Debug.LogError("Erro ao copiar arquivo: " + e.Message);
            yield break;
        }

        yield return null;

        selectedModelPath = finalPath;
        readyToSpawn = true;
        Debug.Log("Modelo pronto! Toque na tela para instanciar.");
    }

    IEnumerator LoadGLBModel(string path, Vector3 spawnPosition)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Caminho nulo.");
            yield break;
        }

        var gltf = new GltfImport();
        var uri = new System.Uri(path);
        var success = gltf.Load(uri);

        while (!success.IsCompleted)
            yield return null;

        if (!success.Result)
        {
            Debug.LogError("Erro ao carregar modelo: " + path);
            yield break;
        }

        GameObject model = new GameObject("ModeloImportado");
        
    

        var instTask = gltf.InstantiateMainSceneAsync(model.transform);

        while (!instTask.IsCompleted)
            yield return null;

        
        model.transform.localScale = Vector3.one * 0.05f;
        model.transform.position = spawnPosition;

        int collidersAdicionados = 0;
    foreach (Transform t in model.GetComponentsInChildren<Transform>())
    {
        var meshRenderer = t.GetComponent<MeshRenderer>();
        var skinnedMesh = t.GetComponent<SkinnedMeshRenderer>();

        if (meshRenderer != null || skinnedMesh != null)
        {
            if (t.GetComponent<Collider>() == null)
            {
                t.gameObject.AddComponent<BoxCollider>();
                    t.gameObject.AddComponent<DragObject>();
                collidersAdicionados++;
            }
        }
    }

        Debug.Log("Modelo carregado em: " + spawnPosition);
    }
}
