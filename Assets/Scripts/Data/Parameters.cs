using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Parameters", fileName = "New Parameters")]
public class Parameters : SerializedScriptableObject
{
    public int swipePixelCount = 50;
    public float tileSize = 1.3f;
    public float cameraSizePerTile = 3f;
    public float tileAnimationDuration = 0.5f;
    public float swapDuration = 0.5f;
    public float slideDuration = 0.5f;
    public float postSlideDelay = 0.1f;
    public Color tileBackgroundColor;
    public Dictionary<TileType, Sprite> SpriteList;
    public AnimationCurve swapZPosition;
    public AnimationCurve swapScale;
    public AnimationCurve failSwapPosition;
}
