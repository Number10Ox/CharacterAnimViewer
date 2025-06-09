using System.Collections.Generic;

[System.Serializable]
public class AnimationCategory
{
    public string categoryName;
    public List<AnimationData> animationDataList = new();
}