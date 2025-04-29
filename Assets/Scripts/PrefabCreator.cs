using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PrefabCreator : MonoBehaviour
{
    [SerializeField] GameObject duckPrefab;
    [SerializeField] private Vector3 prefabOffset;

    GameObject duck;
    public ARTrackedImageManager aRTrackedImageManager;

    void OnEnable(){
        
#pragma warning disable CS0618 // Type or member is obsolete
        aRTrackedImageManager.trackedImagesChanged+=OnImageChanged;
#pragma warning restore CS0618 // Type or member is obsolete
    }

    void OnDisable(){
        aRTrackedImageManager.trackedImagesChanged -=OnImageChanged;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    void OnImageChanged(ARTrackedImagesChangedEventArgs obj){
        foreach(ARTrackedImage image in obj.added){
            duck = Instantiate(duckPrefab,image.transform);
            duck.transform.position+=prefabOffset;
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete

}
