using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AnimationClipLoader : MonoBehaviour
{
    [SerializeField] private AnimationLibrary animationLibrary;

    private Dictionary<string, AnimationClip> animationClips = new();

    void Start()
    {
        StartCoroutine(LoadAllClips());
    }

    private IEnumerator LoadAllClips()
    {
        foreach (var category in animationLibrary.categories)
        {
            foreach (var data in category.animationDataList)
            {
                var handle = Addressables.LoadAssetAsync<AnimationClip>(data.assetName);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    animationClips[data.assetName] = handle.Result;
                    Debug.Log($"Loaded animation: {data.assetName}");
                }
                else
                {
                    Debug.LogError($"Failed to load animation: {data.assetName}");
                }
            }
        }
    }

    public AnimationClip ClipByName(string assetName)
    {
        animationClips.TryGetValue(assetName, out var clip);
        return clip;
    }
}
