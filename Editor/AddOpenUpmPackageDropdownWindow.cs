using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace PackageWizard.Editor
{
    internal class AddOpenUpmPackageDropdownWindow : EditorWindow
    {
        private string _packageName;

        private GUIContent _openUpmIcon;
        
        private static EditorWindow _packageManagerWindow;
        private static readonly Vector2 _defaultWindowSize = new Vector2(320f, 50f);
        
        private static GUIStyle _dropdownStyle;
        
        public static void ShowDropdown()
        {
            // Find the PackageManagerWindow and disable its contents (this is what the other options do in the add menu) 
            var packageManagerWindowType = typeof(IPackageManagerExtension).Assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");
            _packageManagerWindow = GetWindow(packageManagerWindowType);
            _packageManagerWindow.rootVisualElement.SetEnabled(false);
            
            // Create a PackageWizardDropdownWindow and position it under the Package Manager window's header
            var window = CreateInstance<AddOpenUpmPackageDropdownWindow>();
            window.UpdateWindowSize();
            window.ShowPopup();
        }

        private void OnEnable()
        {
            _openUpmIcon = new GUIContent(Load<Texture2D>("Icons/openupm-icon-32.png"));
            
            // Approximately the margin that the other package manager dropdown options have
            _dropdownStyle = new GUIStyle(GUIStyle.none)
            {
                margin = new RectOffset(6, 6, 3, 6)
            };
        }

        private void OnGUI()
        {
            // todo: find a non-hardcoded way to get Editor background color
            EditorGUI.DrawRect(new Rect(0, 0, position.x, position.y),
                EditorGUIUtility.isProSkin ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.76f, 0.76f, 0.76f));
            
            using (new EditorGUILayout.VerticalScope(_dropdownStyle))
            {
                // Draw header
                LabelWithIcon(new GUIContent("Add package from OpenUPM"), _openUpmIcon, EditorStyles.boldLabel);

                var rect = EditorGUILayout.GetControlRect();
                var buttonRect = new Rect(rect)
                {
                    x = rect.x + rect.width - 37f,
                    width = 37,
                };
                var fieldRect = new Rect(rect)
                {
                    width = rect.width - buttonRect.width - 6,
                };
                _packageName = PlaceholderTextField(fieldRect, _packageName, "Name");
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_packageName)))
                {
                    if (GUI.Button(buttonRect, "Add"))
                    {
                        var process = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = "openupm",
                                Arguments = $"add {_packageName}",
                                WorkingDirectory = Directory.GetParent(Application.dataPath).ToString(),
                                WindowStyle = ProcessWindowStyle.Hidden,
                            }
                        };
                        
                        EditorUtility.DisplayProgressBar("OpenUPM", $"Adding {_packageName}", 1f);
                        
                        process.Start();
                        process.WaitForExit();
                        
                        EditorUtility.ClearProgressBar();
                        
                        if (process.ExitCode != 0)
                        {
                            Debug.LogError($"Encountered error while trying to resolve package '{_packageName}' from OpenUPM.");
                        }
                        else
                        {
                            Client.Resolve();
                            CloseWindow();
                        }
                    }
                }
            }
        }
        
        // https://forum.unity.com/threads/gizmos-in-package.724841/#post-8932158
        private static T Load<T>(string path) where T : Object
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            var packagePath = $"{packageInfo.assetPath}/{path}".Replace('\\', '/');
            var result = AssetDatabase.LoadAssetAtPath<T>(packagePath);
            if (result)
            {
                return result;
            }
            Debug.LogError($"Failed to load [{typeof(T)}] from [{path}]");
            return default;
        }

        private void OnLostFocus()
        {
            CloseWindow();
        }

        private void UpdateWindowSize()
        {
            position = new Rect()
            {
                x = _packageManagerWindow.position.x,
                y = _packageManagerWindow.position.y + EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing * 2f + 1f,
                width = _defaultWindowSize.x,
                height = _defaultWindowSize.y
            };
        }

        private void CloseWindow()
        {
            // Re-enable the Package Manager window's contents
            _packageManagerWindow.rootVisualElement.SetEnabled(true);
            
            // Close this dropdown
            Close();
        }
        
        private static void LabelWithIcon(GUIContent label, GUIContent icon, GUIStyle labelStyle = null)
        {
            var rect = EditorGUILayout.GetControlRect();
            var labelRect = IconPrefix(rect, icon);
            
            if (labelStyle != null)
            {
                GUI.Label(labelRect, label, labelStyle);
            }
            else
            {
                GUI.Label(labelRect, label);
            }
        }
        
        private static Rect IconPrefix(Rect rect, GUIContent icon, float padding = 0f, float iconHeight = -1f)
        {
            var iconRect = new Rect(rect);
            if (iconHeight < 0)
            {
                iconRect.width = iconRect.height;
            }
            else
            {
                iconRect.height = iconHeight;
                iconRect.width = iconHeight;
            }
            
            var contentRect = new Rect(rect);
            contentRect.width -= iconRect.width + padding;
            contentRect.x += iconRect.width + padding;
            
            GUI.Label(iconRect, icon);

            return contentRect;
        }
        
        private static string PlaceholderTextField(Rect rect, string value, string placeholder)
        {
            var output = EditorGUI.TextField(rect, value);
            if (string.IsNullOrEmpty(value))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    var placeholderRect = new Rect(rect);
                    placeholderRect.x += 2f;
                    placeholderRect.width -= 2f;
                    EditorGUI.LabelField(placeholderRect, placeholder, EditorStyles.miniLabel);
                }
            }

            return output;
        }
    }
}