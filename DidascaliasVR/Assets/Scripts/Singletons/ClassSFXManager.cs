using AYellowpaper.SerializedCollections;
using UnityEngine;

public class ClassSFXManager : Singleton<ClassSFXManager>
{
    [SerializeField]
    SerializedDictionary<string, AudioSource> _audioKeyToSound = new SerializedDictionary<string, AudioSource>();

    protected override void Awake()
    {
        _isSceneSingleton = true;

        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
