using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AnimationLibrary", menuName = "Custom/Animation Library")]
public class AnimationLibrary : ScriptableObject
{
    public List<AnimationCategory> categories = new();
}