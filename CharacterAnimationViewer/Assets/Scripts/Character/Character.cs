using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;

    public Transform ModelRoot => modelRoot;
    
    private void Awake()
    {
        if (modelRoot == null)
        {
            Debug.LogError($"[CharacterRoot] ModelRoot is not assigned on {gameObject.name}");
        }
    }
}
