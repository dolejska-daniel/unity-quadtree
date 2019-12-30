using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Boundaries of this item are determined by present <c>UnityEngine.Renderer</c> component.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Spatial partitioning/Quadtree/Items/Renderer-based Item")]
    public class RendererItem : MonoBehaviour, IItem
    {
        //==========================================================================dd==
        //  MonoBehaviour METHODS
        //==========================================================================dd==

        private Renderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
        }
    
        //==========================================================================dd==
        //  Quadtree ITEM METHODS
        //==========================================================================dd==

        /// <inheritdoc cref="IItem"/>
        public Bounds GetBounds()
        {
            return _renderer.bounds;
        }

        /// <inheritdoc cref="IItem"/>
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}