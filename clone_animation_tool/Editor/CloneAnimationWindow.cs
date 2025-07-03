using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class CloneAnimationWindow : EditorWindow
{
    private AnimatorController _animatorController;
    private Texture2D _newAnimationSpriteAtlas;
    private string _selectedFile;

    [MenuItem("UGS/Content/Animations")]
    public static void ShowWindow()
    {
        CloneAnimationWindow window = (CloneAnimationWindow)GetWindow(typeof(CloneAnimationWindow));
        window.minSize = new Vector2(250, 125);
        window.maxSize = window.minSize;
        window.Show();
    }

    private void OnGUI()
    {
        // The animator controller.
        EditorGUILayout.LabelField("Original Animator Controller");
        _animatorController =
            EditorGUILayout.ObjectField(_animatorController, typeof(AnimatorController), false) as AnimatorController;

        // Space
        EditorGUILayout.Space();

        // The new Atlas.
        EditorGUILayout.LabelField("New Animation Sprite Atlas");
        _newAnimationSpriteAtlas =
            EditorGUILayout.ObjectField(_newAnimationSpriteAtlas, typeof(Texture2D), false) as Texture2D;

        // Space
        EditorGUILayout.Space();

        // If the original animator and the new atlas are not null, we enable the button.
        GUI.enabled = _animatorController != null && _newAnimationSpriteAtlas != null;
        if (GUILayout.Button("Build"))
        {
            CloneAnimationLogic.BuildAnimator(_animatorController, _newAnimationSpriteAtlas);
        }
        GUI.enabled = true;
    }
}