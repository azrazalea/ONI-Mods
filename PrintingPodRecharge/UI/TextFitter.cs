using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrintingPodRecharge.UI
{
    public class TextFitter : KMonoBehaviour, ILayoutSelfController
    {
        [SerializeField]
        public TextMeshProUGUI targetText;

        // Assigned by Klei's component injection (OnPrefabInit); initialized to satisfy the compiler.
        [MyCmpReq]
        private LayoutElement layoutElement = null;

        [SerializeField]
        public float padding = 10f;

        public void SetLayoutHorizontal()
        {
        }

        public void SetLayoutVertical()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(FitDelayed());
            }
        }

        private IEnumerator FitDelayed()
        {
            yield return new WaitForEndOfFrame();
            layoutElement.minHeight = targetText.renderedHeight + padding;
        }
    }
}
