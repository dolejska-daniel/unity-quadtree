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
    public class RendererItem : GameObjectItem
    {
        /// <summary>
        /// Determines whether the item should be automatically inserted into the tree upon its initialization.
        /// </summary>
        /// 
        /// <seealso cref="RootNode{TItem}.Insert(TItem)"/>
        [SerializeField]
        protected bool InsertOnInitialization = true;

        private Renderer _renderer;

        //==========================================================================dd==
        //  Quadtree ITEM METHODS
        //==========================================================================dd==

        /// <summary>
        /// Finds and locally stores this <c>GameObject</c>'s <c>Renderer</c> component instance.
        /// </summary>
        protected override void Init()
        {
            // load game object renderer component
            _renderer = GetComponent<Renderer>();

            base.Init();
        }

        /// <inheritdoc cref="IBounds.GetBounds"/>
        public override Bounds GetBounds()
        {
            return _renderer.bounds;
        }

        /// <inheritdoc cref="IGameObjectItem.GetGameObject"/>
        public override GameObject GetGameObject() => gameObject;
    }
}