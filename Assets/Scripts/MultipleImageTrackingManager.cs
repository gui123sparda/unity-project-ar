using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImageTrackingManager : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabsToSpawn = new List<GameObject>();

    private ARTrackedImageManager _trackeImageManager;
    private Dictionary<string, GameObject> _arObjects;

    private bool _trackingEnabled = true;

    void Start()
    {
        _trackeImageManager = GetComponent<ARTrackedImageManager>();
        if (_trackeImageManager == null) return;

        _trackeImageManager.trackablesChanged.AddListener(OnImagesTrackedChanged);
        _arObjects = new Dictionary<string, GameObject>();
        SetupSceneElements();
    }

    private void OnDestroy()
    {
        _trackeImageManager.trackablesChanged.RemoveListener(OnImagesTrackedChanged);
    }

    void OnImagesTrackedChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (!_trackingEnabled) return;

        foreach (var trackedImage in eventArgs.added)
            UpdateTrackedImages(trackedImage);

        foreach (var trackedImage in eventArgs.updated)
            UpdateTrackedImages(trackedImage);

        foreach (var trackedImage in eventArgs.removed)
            UpdateTrackedImages(trackedImage.Value);
    }

    void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        if (trackedImage == null || !_trackingEnabled) return;

        var name = trackedImage.referenceImage.name;

        if (!_arObjects.ContainsKey(name)) return;

        GameObject obj = _arObjects[name];

        if (trackedImage.trackingState is TrackingState.Limited or TrackingState.None)
        {
            obj.SetActive(false);
            return;
        }

        obj.SetActive(true);
        obj.transform.position = trackedImage.transform.position;
        obj.transform.rotation = trackedImage.transform.rotation;
    }

    private void SetupSceneElements()
    {
        foreach (var prefab in prefabsToSpawn)
        {
            var arObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            arObject.name = prefab.name;
            arObject.SetActive(false);
            _arObjects.Add(arObject.name, arObject);
        }
    }

    /// <summary>
    /// Para o rastreamento e mantém os objetos fixos na última posição detectada.
    /// </summary>
    public void StopTracking()
    {
        _trackingEnabled = false;
        _trackeImageManager.enabled = false;
    }

    /// <summary>
    /// Retoma o rastreamento das imagens, reativando o sistema.
    /// </summary>
    public void ResumeTracking()
    {
        _trackeImageManager.enabled = true;
        _trackingEnabled = true;
    }

    void Update() { }
}
