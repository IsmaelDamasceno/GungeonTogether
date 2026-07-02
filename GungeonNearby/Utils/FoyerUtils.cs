using System.Collections;
using UnityEngine;

namespace GungeonNearby.Utils
{
    public static class FoyerUtils
    {
        public static IEnumerator WaitForFoyer()
        {
            if (IsInFoyer())
            {
                yield break;
            }
            yield return new WaitUntil(IsInFoyer);
        }

        public static bool IsInFoyer() =>
            GameManager.Instance != null && GameManager.Instance.IsFoyer;
    }
}
