using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator LocalMove(Transform transform, Vector3 initialPosition, Vector3 targetPosition, float duration)
    {
        transform.localPosition = initialPosition;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / duration;
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, progress);
        }

        transform.localPosition = targetPosition;
    }
    
    public static IEnumerator GlobalMove(Transform transform, Vector3 initialPosition, Vector3 targetPosition, float duration)
    {
        transform.position = initialPosition;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / duration;
            transform.position = Vector3.Lerp(initialPosition, targetPosition, progress);
        }

        transform.position = targetPosition;
    }
    
    public static IEnumerator AnimateScale(Transform transform, Vector3 initialScale, Vector3 targetScale, float duration)
    {
        transform.localScale = initialScale;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            var progress = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);
        }

        transform.localScale = targetScale;
    }

    public static bool IsInsideBounds(Coordinate coord)
    {
        var dimensions = EventManager.GetDimensions();
        return coord.xCoord < dimensions.xCoord && coord.xCoord >= 0 &&
               coord.yCoord < dimensions.yCoord && coord.yCoord >= 0;
    }

     public static IEnumerator DelayedAction(float duration, Action action)
    {
        yield return new WaitForSeconds(duration);
        action.Invoke();
    }
}
