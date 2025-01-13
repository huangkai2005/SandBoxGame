using System;
using System.IO;
using Cysharp.Threading.Tasks;
using MoonFramework.Tool;
using Serilog;
using UnityEditor;
using UnityEngine;

namespace MoonFramework.Test
{
    [Serializable]
    public class TestAss : MonoBehaviour
    {
        public Texture2D forestTexture; // ɭ������
        public Texture2D[] marshTextures; // ������������
        public string savePath = "Assets/Resources/Map"; // ����·��

        public async void Start()
        {
            LoggerManager.RegisterLog("TestLog");
            Log.Debug("Start");
            UniTaskCompletionSource token = new();
            test(token).Forget();
            await token.Task;
            Log.Debug("over");
        }

        public async UniTaskVoid test(UniTaskCompletionSource token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5f));
            token.TrySetResult();
            await UniTask.Delay(TimeSpan.FromSeconds(5f));
            Log.Debug("���");
        }

        [ContextMenu("Generate Cell Textures")]
        public void GenerateCellTextures()
        {
            var textureCellSize = forestTexture.width;

            for (var textureIndex = -1; textureIndex < marshTextures.Length; textureIndex++)
            {
                var cellTexture = new Texture2D(textureCellSize, textureCellSize, TextureFormat.RGBA32, false);

                for (var y = 0; y < textureCellSize; y++)
                for (var x = 0; x < textureCellSize; x++)
                {
                    Color pixelColor;
                    if (textureIndex < 0)
                    {
                        pixelColor = forestTexture.GetPixel(x, y); // ��ɭ��
                    }
                    else
                    {
                        var marshColor = marshTextures[textureIndex].GetPixel(x, y);
                        pixelColor = marshColor.a < 1f
                            ? forestTexture.GetPixel(x, y) // ��͸������ʹ��ɭ��
                            : marshColor;
                    }

                    cellTexture.SetPixel(x, y, pixelColor);
                }

                cellTexture.filterMode = FilterMode.Point;
                cellTexture.wrapMode = TextureWrapMode.Clamp;
                cellTexture.Apply();

                var pngData = cellTexture.EncodeToPNG();
                var fileName = $"{savePath}/CellTexture_{textureIndex + 1}.png";
                File.WriteAllBytes(fileName, pngData);
                Log.Debug($"Saved texture: {fileName}");
            }

            AssetDatabase.Refresh();
        }
    }
}