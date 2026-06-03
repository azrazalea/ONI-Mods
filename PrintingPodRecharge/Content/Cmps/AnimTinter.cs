using UnityEngine;

namespace PrintingPodRecharge.Content.Cmps
{
    // Applies a flat tint to the entity's animation at spawn. Setting TintColour on the prefab does
    // not stick because the batch instance data only exists once the object is actually spawned, so
    // we (re)apply it here.
    public class AnimTinter : KMonoBehaviour
    {
        public Color tint = Color.white;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            var kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                kbac.TintColour = tint;
            }
        }
    }
}
