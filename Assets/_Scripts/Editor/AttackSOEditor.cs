using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackSO))]
public sealed class AttackSOEditor : OdinEditor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        if (target is not AttackSO attackSo || attackSo.AbilityIcon == null)
        {
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        var preview = TryRenderFromSprite(attackSo.AbilityIcon, width, height);
        if (preview != null)
        {
            return preview;
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    private static Texture2D TryRenderFromSprite(Sprite sprite, int width, int height)
    {
        if (sprite == null || width <= 0 || height <= 0)
        {
            return null;
        }

        var texture = sprite.texture;
        if (texture == null || texture.width <= 0 || texture.height <= 0)
        {
            return null;
        }

        var spriteRect = sprite.textureRect;
        if (spriteRect.width <= 0f || spriteRect.height <= 0f)
        {
            return null;
        }

        var normalizedSourceRect = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height);

        var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        var previousActive = RenderTexture.active;

        try
        {
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, width, height, 0f);
            var targetRect = BuildAspectFitRect(width, height, spriteRect.width, spriteRect.height);
            Graphics.DrawTexture(targetRect, texture, normalizedSourceRect, 0, 0, 0, 0);
            GL.PopMatrix();

            var output = new Texture2D(width, height, TextureFormat.ARGB32, false);
            output.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            output.Apply();
            return output;
        }
        finally
        {
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    private static Rect BuildAspectFitRect(float containerWidth, float containerHeight, float sourceWidth, float sourceHeight)
    {
        if (sourceWidth <= 0f || sourceHeight <= 0f)
        {
            return new Rect(0f, 0f, containerWidth, containerHeight);
        }

        var sourceAspect = sourceWidth / sourceHeight;
        var containerAspect = containerWidth / Mathf.Max(0.0001f, containerHeight);

        if (sourceAspect > containerAspect)
        {
            var fittedHeight = containerWidth / sourceAspect;
            return new Rect(0f, (containerHeight - fittedHeight) * 0.5f, containerWidth, fittedHeight);
        }

        var fittedWidth = containerHeight * sourceAspect;
        return new Rect((containerWidth - fittedWidth) * 0.5f, 0f, fittedWidth, containerHeight);
    }
}
