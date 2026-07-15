using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule.SaveModule
{
    [Serializable]
    public struct ViewerSettings
    {
        public CanvasSettings canvasSettings;
        public CanvasScalerSettings canvasScalerSettings;
        public GraphicRaycasterSettings graphicRaycasterSettings;
        public CanvasGroupSettings canvasGroupSettings;
        public GridLayoutGroupSettings gridLayoutGroupSettings;
        public RectTransformSettings rectTransformSettings;
    }

    /// <summary>
    /// Saves the graphics rayCaters info.
    /// </summary>
    [System.Serializable]
    public struct GraphicRaycasterSettings
    {
        public bool ignoreReversedGraphics;
        public GraphicRaycaster.BlockingObjects blockingObjects;
        public LayerMask blockingMask;

    }

    [System.Serializable]
    public struct CanvasGroupSettings
    {
        public float alpha;
        public bool interactable;
        public bool blockrayCast;
        public bool ignoreParentGroups;
    }

    [System.Serializable]
    public struct CanvasScalerSettings
    {
        public CanvasScaler.ScaleMode uiScaleMode;
        public float referencePixelsPerUnit;
        public float scaleFactor;
        public Vector2 referenceResolution;
        public CanvasScaler.ScreenMatchMode screenMatchMode;
        public float matchWidthOrHeight;
        public CanvasScaler.Unit physicalUnit;
        public float fallbackScreenDPI;
        public float defaultSpriteDPI;
        public float dynamicPixelsPerUnit;
    }

    [System.Serializable]
    public struct CanvasSettings
    {
        public RenderMode renderMode;
        public int sortingOrder;
        public bool pixelPerfect;
        public bool overrideSorting;
        public Camera worldCamera;
        public float planeDistance;
        public AdditionalCanvasShaderChannels additionalShaderChannels;
    }

    [Serializable]
    public struct GridLayoutGroupSettings
    {
        public RectOffset padding;
        public Vector2 cellSize;
        public Vector2 spacing;
        public GridLayoutGroup.Corner startCorner;
        public GridLayoutGroup.Axis startAxis;
        public TextAnchor childAlignment;
        public GridLayoutGroup.Constraint constraint;
        public int constraintCount;
    }

    [Serializable]
    public struct RectTransformSettings
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 pivot;
        public Vector3 localPosition;
        public Vector3 localScale;
        public Quaternion localRotation;
    }
}
