using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Boundaries of this quadtree item are determined by present <c>UnityEngine.Renderer</c> component.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Spatial partitioning/Quadtree/Items/Renderer-based Item")]
    public class RendererItem : MonoBehaviour, IGameObjectItem
    {
        //==========================================================================dd==
        //  MonoBehaviour METHODS
        //==========================================================================dd==

        private Renderer _renderer;

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            if (_renderer == null)
                Init();
        }

        //==========================================================================dd==
        //  Quadtree ITEM METHODS
        //==========================================================================dd==

        /// <summary>
        /// Finds and locally stores this <c>GameObject</c>'s <c>Renderer</c> component instance.
        /// </summary>
        private void Init()
        {
            _renderer = GetComponent<Renderer>();
        }

        /// <inheritdoc cref="IGameObjectItem.GetBounds"/>
        public Bounds GetBounds()
        {
            return _renderer.bounds;
        }

        /// <inheritdoc cref="IGameObjectItem.GetGameObject"/>
        public GameObject GetGameObject()
        {
            return gameObject;
        }
    }
}