namespace CameraTools
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class CameraToolsMenu : EditorWindow
    {
        public static Camera TargetCamera;
        public static int PictureResolutionWidth = 4096;
        public static int PictureResolutionHeight = 2048;

        [MenuItem("CameraTools/Open Menu")]
        static void Init()
        {
            CameraToolsMenu window = (CameraToolsMenu)EditorWindow.GetWindow(typeof(CameraToolsMenu), false, "Camera Tools");
            window.minSize = new Vector2(288f, 250f);
            window.Show();
        }

        private RenderTexture PictureRT;
        private bool Transparent;
        private string OutputPath = String.Empty;

        public void OnGUI()
        {
            EditorGUILayout.HelpBox("Move your camera from scene hierarchy into this box below", MessageType.Info);
            TargetCamera = (Camera)EditorGUILayout.ObjectField("Target Camera", TargetCamera, typeof(Camera), true);
            GUILayout.Space(25f);
            MakeBox("Picture");
            PictureResolutionWidth = EditorGUILayout.IntField("Width", PictureResolutionWidth);
            PictureResolutionHeight = EditorGUILayout.IntField("Height", PictureResolutionHeight);
            GUI.enabled = TargetCamera != null;
            Transparent = GUILayout.Toggle(Transparent, "Transparent");
            if (TargetCamera != null)
            {
                if (TargetCamera.clearFlags == CameraClearFlags.Depth && !Transparent)
                    TargetCamera.clearFlags = CameraClearFlags.Skybox;
                else if (TargetCamera.clearFlags != CameraClearFlags.Depth && Transparent)
                    TargetCamera.clearFlags = CameraClearFlags.Depth;
            }
            GUI.enabled = true;
            GUILayout.Space(15f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Output path");
            if (GUILayout.Button("Open"))
            {
                OutputPath = EditorUtility.OpenFolderPanel("Output path", OutputPath, "Pics");
            }
            GUILayout.EndHorizontal();
            GUI.enabled = false;
            GUILayout.TextField(OutputPath);
            GUI.enabled = true;
            GUI.enabled = TargetCamera != null;
            if (GUILayout.Button("Take picture"))
            {
                CapturePicture(TargetCamera, PictureResolutionWidth, PictureResolutionHeight);
            }
            GUI.enabled = true;
        }

        void MakeBox(string name)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUILayout.Label(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public void CapturePicture(Camera targetCamera, int width, int height)
        {
            PictureRT = new RenderTexture(new RenderTextureDescriptor(width, width));
            PictureRT.depth = 24;

            PictureRT.dimension = TextureDimension.Tex2D;

            if (PictureRT.width != width)
                PictureRT.width = width;
            if (PictureRT.height != height)
                PictureRT.height = height;

            targetCamera.targetTexture = PictureRT;
            targetCamera.Render();

            Save("Picture", PictureRT);

            targetCamera.targetTexture = null;
            DestroyImmediate(PictureRT);
        }

        public void Save(string type, RenderTexture rt)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, Transparent ? TextureFormat.ARGB32 : TextureFormat.RGB565, false);

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();

            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            string path = Path.Combine(OutputPath, $"{type}_{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.png");

            File.WriteAllBytes(path, bytes);
            Debug.Log($"{type} saved at location {path}");
        }
    }
}
