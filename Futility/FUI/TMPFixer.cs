using TMPro;
using UnityEngine;

namespace FUtility.FUI
{
    // There is an issue with how TMP imports itself and alighnment has to be reapplied
    class TMPFixer : KMonoBehaviour
    {
        [SerializeField]
        public TextAlignmentOptions alignment;

        // Assigned by Klei's component injection (OnSpawn); initialized to satisfy the compiler.
        [MyCmpReq]
        private LocText text = null;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            text.alignment = alignment;
            Destroy(this);
        }
    }
}
