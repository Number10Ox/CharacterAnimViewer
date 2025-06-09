using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

[System.Serializable]
public class CharacterAnimViewerPopupController : MonoBehaviour
{
    [SerializeField] private AnimationLibrary animationLibrary;
    
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string popupPanelName = "character-anim-viewer";
    [SerializeField] private string viewportName = "character-viewport";
    [SerializeField] private string tabContainerName = "category-tabs";
    [SerializeField] private string scrollListName = "animation-scroll-list";
    [SerializeField] private string selectedCategoryLabelName = "selected-category-title";
    [SerializeField] private string allAnimationsLabelName = "header-title";
    [SerializeField] private string animationCardName = "animation-card";
    [SerializeField] private string animationTitleName = "animation-title";
    [SerializeField] private string animationSubtitleName = "animation-subtitle";
    [SerializeField] private string tabButtonName = "tab-button";
    [SerializeField] private string tabSelectedName = "tab-selected";
    [SerializeField] private string animationCardSelectedName = "animation-card-selected";
    
    [SerializeField] private Character characterPrefab;
    [SerializeField] private Transform characterSpawnPoint;
    
    [SerializeField] private Camera renderCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Vector3 cameraPositionOffset = new Vector3(0f, 1.5f, 3.0f);
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float fieldOfView = 45f;

    private const string AnimationAssetPrefix = "Animations/";
    private const string ModelAssetPrefix = "Models/";
    private const string BaseCharacterModelAssetName = "BaseCharacter";

    private bool assetsLoaded;
    private string selectedCategoryName;
    private VisualElement selectedAnimationCard;

    private VisualElement popupPanel;
    private VisualElement characterViewport;
    private Character spawnedCharacter;
    private VisualElement tabContainer;
    private ScrollView animationList;

    private Dictionary<string, AnimationClip> animationClipsByAssetName = new();
    private Animator characterAnimator;
    private GameObject instantiatedModel;

    private void Awake()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned.");
            enabled = false;
            return;
        }

        var rootElement = uiDocument.rootVisualElement;
        popupPanel = rootElement.Q<VisualElement>(popupPanelName);
        characterViewport = rootElement.Q<VisualElement>(viewportName);
        tabContainer = rootElement.Q<VisualElement>(tabContainerName);
        animationList = rootElement.Q<ScrollView>(scrollListName);

        BuildTabs();

        Hide();
    }
    
    public void Show()
    {
        if (!assetsLoaded)
            StartCoroutine(LoadAssetsAndShow());
        else
            ShowPopup();
    }

    public void Hide()
    {
        popupPanel.style.display = DisplayStyle.None;
        if (renderCamera != null)
            renderCamera.enabled = false;
    }

    private void ShowPopup()
    {
        SelectCategory(animationLibrary.categories[0].categoryName);

        popupPanel.style.display = DisplayStyle.Flex;

        PositionCamera();
        ApplyRenderTextureToUI();
        renderCamera.enabled = true;
    }

    private IEnumerator LoadAssetsAndShow()
    {
        yield return StartCoroutine(LoadModel(ModelAssetPrefix + BaseCharacterModelAssetName));
        yield return StartCoroutine(LoadAnimations());
        SetupCharacterRenderer();
        assetsLoaded = true;

        ShowPopup();
    }

    private IEnumerator LoadAnimations()
    {
        foreach (var category in animationLibrary.categories)
        {
            foreach (var data in category.animationDataList)
            {
                var handle = Addressables.LoadAssetAsync<AnimationClip>(AnimationAssetPrefix + data.assetName);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                    animationClipsByAssetName[data.assetName] = handle.Result;
                else
                    Debug.LogError($"Failed to load animation clip: {data.assetName}");
            }
        }
    }

    private IEnumerator LoadModel(string address)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            spawnedCharacter = Instantiate(characterPrefab, characterSpawnPoint.position, characterSpawnPoint.rotation,
                characterSpawnPoint);

            GameObject model = handle.Result;
            instantiatedModel = Instantiate(model, spawnedCharacter.ModelRoot.transform.position,
                spawnedCharacter.ModelRoot.transform.rotation, spawnedCharacter.ModelRoot);

            characterAnimator = instantiatedModel.GetComponent<Animator>();
            if (characterAnimator == null)
                Debug.LogError("No Animator component found on the character model!");
        }
        else
            Debug.LogError($"Failed to load FBX at address '{address}'");
    }

    private void SetupCharacterRenderer()
    {
        if (renderTexture == null)
        {
            Debug.LogError("No render texture set!");
            return;
        }

        if (renderCamera != null)
        {
            renderCamera.targetTexture = renderTexture;
            renderCamera.backgroundColor = Color.clear;
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
        }
    }

    private void PositionCamera()
    {
        if (renderCamera == null || spawnedCharacter == null)
            return;
        Transform modelRoot = spawnedCharacter.ModelRoot.transform;

        // Debug information
        // Debug.Log($"Character Position: {modelRoot.position}");
        // Debug.Log($"Character Rotation: {modelRoot.eulerAngles}");
        // Debug.Log($"Character Forward: {modelRoot.forward}");

        Vector3 characterPos = modelRoot.position;

        Vector3 cameraPos = characterPos + cameraPositionOffset;
        Vector3 lookAtPos = characterPos + lookAtOffset;
        renderCamera.transform.position = cameraPos;
        renderCamera.transform.LookAt(lookAtPos);
        renderCamera.fieldOfView = 45f;

        // Debug.Log($"Camera Position: {cameraPos}");
        // Debug.Log($"Look At Position: {lookAtPos}");
    }

    private void ApplyRenderTextureToUI()
    {
        if (characterViewport == null || renderTexture == null)
            return;

        characterViewport.style.backgroundImage = Background.FromRenderTexture(renderTexture);
        characterViewport.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Cover));
        characterViewport.style.backgroundRepeat =
            new StyleBackgroundRepeat(new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat));
        characterViewport.style.backgroundPositionX =
            new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
        characterViewport.style.backgroundPositionY =
            new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
    }

    private void BuildTabs()
    {
        tabContainer.Clear();

        foreach (var category in animationLibrary.categories)
        {
            var categoryName = category.categoryName;
            var button = new Button(() => SelectCategory(categoryName))
            {
                text = categoryName
            };

            button.AddToClassList(tabButtonName);

            if (categoryName == selectedCategoryName)
                button.AddToClassList(tabSelectedName);
            else
                button.RemoveFromClassList(tabSelectedName);

            tabContainer.Add(button);
        }
    }

    private void SelectCategory(string categoryName)
    {
        selectedCategoryName = categoryName;
        UpdateSelectedCategoryUI();
    }

    private void UpdateSelectedCategoryUI()
    {
        var root = uiDocument.rootVisualElement;
        var selectedCategoryLabel = root.Q<Label>(selectedCategoryLabelName);
        if (selectedCategoryLabel != null)
            selectedCategoryLabel.text = selectedCategoryName;

        foreach (var tab in tabContainer.Children())
        {
            var isSelected = tab is Button b && b.text == selectedCategoryName;
            if (isSelected)
                tab.AddToClassList("tab-selected");
            else
                tab.RemoveFromClassList("tab-selected");
        }

        // Clear and rebuild animation cards for this category
        animationList.Clear();

        var category = animationLibrary.categories.Find(c => c.categoryName == selectedCategoryName);
        if (category == null)
        {
            Debug.LogWarning($"Category '{selectedCategoryName}' not found.");
            return;
        }

        foreach (var data in category.animationDataList)
        {
            var card = CreateAnimationCard(data);
            animationList.Add(card);
        }
    }

    private VisualElement CreateAnimationCard(AnimationData data)
    {
        var container = new VisualElement();
        container.AddToClassList(animationCardName);

        var title = new Label(data.displayName);
        title.AddToClassList(animationTitleName);

        var clip = animationClipsByAssetName[data.assetName];
        var subtitle = new Label($"Animation • {GetAnimationLengthFormatted(clip)}");
        subtitle.AddToClassList(animationSubtitleName);

        container.Add(title);
        container.Add(subtitle);
        container.style.flexGrow = 0;

        
        container.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log($"Selected animation: {data.displayName}");
            
            if (selectedAnimationCard != null)
                selectedAnimationCard.RemoveFromClassList(animationCardSelectedName);
            
            selectedAnimationCard = container;
            selectedAnimationCard.AddToClassList("animation-card-selected");
            
            PlayAnimation(clip);
        });

        return container;
    }

    private string GetAnimationLengthFormatted(AnimationClip clip)
    {
        float totalSeconds = clip.length;
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);

        return minutes > 0 ? $"{minutes}m {seconds}s" : $"{seconds}s";
    }

    private void PlayAnimation(AnimationClip clip)
    {
        AnimatorOverrideController overrideController;

        if (characterAnimator.runtimeAnimatorController is AnimatorOverrideController existingOverride)
            overrideController = existingOverride;
        else
        {
            var baseController = characterAnimator.runtimeAnimatorController;
            overrideController = new AnimatorOverrideController(baseController);
            characterAnimator.runtimeAnimatorController = overrideController;
        }

        // Replace the first clip (assuming single state)
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        if (overrides.Count == 0)
        {
            Debug.LogError("Base controller has no overridable clips.");
            return;
        }

        overrideController[overrides[0].Key] = clip;

        characterAnimator.Rebind();
        characterAnimator.Update(0);
        characterAnimator.Play(overrideController[overrides[0].Key].name);
        Debug.Log($"Playing animation: {clip.name}");
        
    }
}
