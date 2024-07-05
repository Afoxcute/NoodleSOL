using UnityEngine;

public static class Collider2DExtensions
{
    // A generic extension method to find a component in sibling GameObjects.
    public static T GetComponentInSiblings<T>(this Collider2D collider) where T : Component
    {
        if (collider.transform.parent == null)
        {
            // No parent, so no siblings.
            return null;
        }

        Transform parent = collider.transform.parent;
        int siblingCount = parent.childCount;

        for (int i = 0; i < siblingCount; i++)
        {
            Transform sibling = parent.GetChild(i);

            if (sibling != collider.transform) // Skip the current GameObject.
            {
                T component = sibling.GetComponent<T>();

                if (component != null)
                {
                    return component;
                }
            }
        }

        return null; // Component not found in siblings.
    }
}
