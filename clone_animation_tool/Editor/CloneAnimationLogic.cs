using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CloneAnimationLogic
{
    #region Variables

    // Constants.
    private const string BaseSpriteSheetsPath = "Assets/Art/Sprites";
    private const string BaseAnimationsPath = "Assets/Art/Animations";

    #endregion

    #region Methods

    public static void BuildAnimator(AnimatorController animatorController, Texture2D spriteSheet)
    {
        string animatorName = spriteSheet.name;
        Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();

        // Clone the original animations, and store them in a dictionary.
        foreach (var animation in animatorController.animationClips)
        {
            var resultClip = CloneAnimation(spriteSheet, animation, animatorController.name);
            if (resultClip != null)
            {
                animations.Add(resultClip.name, resultClip);
            }
        }

        // If the destination folder doesn't exist, we create it.
        string resultClipsPath =
            GetAndCreateRelativePath(BaseSpriteSheetsPath, AssetDatabase.GetAssetPath(spriteSheet), animatorName);

        // We are using animation override controllers for the new assets.
        string destinationPath = $"{resultClipsPath}/{animatorName}.overrideController";
        CreateAnimationOverrideController(destinationPath, animatorName, animatorController, animations);
    }

    private static void CreateAnimationOverrideController(string destinationPath,
        string animatorName,
        AnimatorController originalAnimatorController,
        Dictionary<string, AnimationClip> animations)
    {
        // Create a new Override Controller.
        AnimatorOverrideController destinationOverrideController =
            AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(destinationPath);
        if (destinationOverrideController == null)
        {
            destinationOverrideController = new AnimatorOverrideController(originalAnimatorController);
            AssetDatabase.CreateAsset(destinationOverrideController, destinationPath);
        }
        else
        {
            destinationOverrideController.runtimeAnimatorController = originalAnimatorController;
        }

        // Start assigning animations.
        foreach (var animation in originalAnimatorController.animationClips)
        {
            string animationName = animation.name.Replace(originalAnimatorController.name, animatorName);
            if (animations.TryGetValue(animationName, out var animationClip))
            {
                destinationOverrideController[animation.name] = animationClip;
            }
        }

        // We mark as Dirty
        EditorUtility.SetDirty(destinationOverrideController);

        // Save and refresh the hierarchy
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static AnimationClip CloneAnimation(Texture2D spriteSheet, AnimationClip originalClip,
        string originalAnimatorName)
    {
        if (originalClip == null || spriteSheet == null)
        {
            Debug.LogError("One of the elements is null");
            return null;
        }

        // If the destination folder doesn't exist, we create it.
        // We are creating folders trying to keep the structure we had with the SpriteSheet.
        string animatorName = spriteSheet.name;
        string destinationClipsPath =
            GetAndCreateRelativePath(BaseSpriteSheetsPath, AssetDatabase.GetAssetPath(spriteSheet), animatorName);

        // If the clip doesn't already exist, we create a new one.
        string clipName = originalClip.name.Replace(originalAnimatorName, animatorName);
        string resultClipPath = $"{destinationClipsPath}/{clipName}.anim";
        var resultClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(resultClipPath);
        if (resultClip == null)
        {
            // We need to make sure to keep the frame rate and the settings (Loop, etc.) consistent with the original clip.
            resultClip = new AnimationClip
            {
                frameRate = originalClip.frameRate
            };
            var settings = AnimationUtility.GetAnimationClipSettings(originalClip);
            AnimationUtility.SetAnimationClipSettings(resultClip, settings);
            AssetDatabase.CreateAsset(resultClip, resultClipPath);
        }

        // We start building the Keyframes for the new animation.
        var curvesBindings = AnimationUtility.GetObjectReferenceCurveBindings(originalClip);
        foreach (EditorCurveBinding binding in curvesBindings)
        {
            // Right now, this tool is only for swapping sprites.
            if (binding.type != typeof(SpriteRenderer))
            {
                continue;
            }

            bool success = BuildKeyframesForBinding(spriteSheet, binding, originalClip, resultClip);
            if (!success)
            {
                // If we couldn't build the keyframe, we just return null and throw an error. 
                // (There's probably a nicer way of handling it, but... For now this works)
                Debug.LogWarning($"We couldn't build the animation for {resultClipPath}");
                AssetDatabase.DeleteAsset(resultClipPath);
                return null;
            }
        }

        // Copy the events.
        AnimationUtility.SetAnimationEvents(resultClip, AnimationUtility.GetAnimationEvents(originalClip));

        // Save the asset
        AssetDatabase.SaveAssetIfDirty(resultClip);

        // We return the clip
        return resultClip;
    }

    private static bool BuildKeyframesForBinding(Texture2D spriteSheet, EditorCurveBinding binding,
        AnimationClip animationAsset, AnimationClip resultClip)
    {
        ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(animationAsset, binding);
        List<ObjectReferenceKeyframe> keyFrameList = new List<ObjectReferenceKeyframe>();
        // Load the Sprites in a dictionary (To avoid searching).
        var sprites = LoadSpriteByName(spriteSheet);
        if (sprites == null)
        {
            return false;
        }

        // Here we start creating the Keyframes swapping the textures.
        foreach (var keyframe in keyframes)
        {
            if (!sprites.TryGetValue(keyframe.value.name, out Sprite sprite))
            {
                Debug.LogWarning($"We can't find the sprite for {keyframe.value}");
                return false;
            }

            keyFrameList.Add(new ObjectReferenceKeyframe
            {
                time = keyframe.time,
                value = sprite
            });
        }

        // Build the clip if valid
        if (keyFrameList.Count > 0)
        {
            // Set the keyframes to the animation
            AnimationUtility.SetObjectReferenceCurve(resultClip, binding, keyFrameList.ToArray());
        }

        return true;
    }

    private static string GetAndCreateRelativePath(string basePath, string assetPath, string assetName)
    {
        string relativePath = Path.GetRelativePath(basePath, Path.GetDirectoryName(assetPath));
        // We don't want '.' as a path.
        string resultClipsPath = relativePath == "." ? Path.Join(BaseAnimationsPath, assetName): Path.Join(BaseAnimationsPath, relativePath, assetName);
        if (!Directory.Exists(resultClipsPath))
        {
            Directory.CreateDirectory(resultClipsPath);
        }

        return resultClipsPath;
    }

    private static Dictionary<string, Sprite> LoadSpriteByName(Texture2D texture)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (sprites == null)
        {
            return null;
        }

        var spriteDictionary = new Dictionary<string, Sprite>();
        foreach (var sprite in sprites)
        {
            spriteDictionary[sprite.name] = sprite as Sprite;
        }

        return spriteDictionary;
    }

    #endregion
}