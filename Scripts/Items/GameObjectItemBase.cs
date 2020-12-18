using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Custom item interface for GameObject quadtree items.
    /// </summary>
    public abstract class GameObjectItemBase<TItem, TNode> : MonoBehaviour, IItem<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : INode<TItem, TNode>
    {
        /// <summary>
        /// Game object's bounds from last update call.
        /// </summary>
        private Bounds _lastBounds;

        /// <summary>
        /// Game object's bounds from last update call.
        /// </summary>
        private Bounds _safeBounds;

        //==========================================================================dd==
        //  MonoBehaviour METHODS
        //==========================================================================dd==

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            Root = null;
            ItemInitialized = false;
            ParentNode.Remove(This());
        }

        private void LateUpdate()
        {
            var currentBounds = GetBounds();
            if (currentBounds != _lastBounds)
            {
                // the object has moved or changed size
                var forceInsertionEvaluation = false;
                if (!currentBounds.Intersects(_safeBounds)
                    || (currentBounds.size - _lastBounds.size).magnitude > 0)
                {
                    // ...far enough to force re-insertion
                    forceInsertionEvaluation = true;
                    _safeBounds = currentBounds;
                }

                // current object bounds are not the same as last update
                // initiate tree update from currently 
                ParentNode?.Update(This(), forceInsertionEvaluation);
                _lastBounds = currentBounds;
            }
        }

        //==========================================================================dd==
        //  CORE TREE ITEM METHODS
        //==========================================================================dd==

        /// <summary>
        /// <c>True</c> if the item has been initialized.
        /// </summary>
        protected internal bool ItemInitialized = false;

        public IQuadtreeRoot<TItem, TNode> Root { get; set; }

        public TNode ParentNode { get; set; }

        public abstract Bounds GetBounds();

        public void QuadTree_Root_Initialized(IQuadtreeRoot<TItem, TNode> root)
        {
            Root = root;

            if (ItemInitialized)
            {
                // the item has been initialized before the tree root
                root.Insert(This());
            }
        }

        /// <summary>
        /// Returns reference to corresponding game object.
        /// </summary>
        /// 
        /// <returns>Game object instance.</returns>
        public abstract GameObject GetGameObject();

        /// <summary>
        /// Initializes the item instance.
        /// </summary>
        /// <remarks>
        /// This may be called either before or after the initialization of the tree root.
        /// </remarks>
        protected virtual void Init()
        {
            // designate item as initialized
            ItemInitialized = true;

            // set initial last bounds
            _lastBounds = GetBounds();
            // set initial safe bounds
            _safeBounds = _lastBounds;

            if (Root == null)
            {
                if (TryGetComponent(out GameObjectQuadtreeRoot quadtreeRoot) && quadtreeRoot.Initialized)
                {
                    Root = (IQuadtreeRoot<TItem, TNode>)quadtreeRoot;
                }
            }

            if (Root != null)
            {
                // the tree root has been initialized before the item
                Root.Insert(This());
            }
        }

        /// <summary>
        /// Overloaded in sub-classes to return correct instance of the item.
        /// </summary>
        /// <remarks>
        /// This method is necessary due to generic typechecking -- <c>this</c> in context of the abstract generic class does not reference TItem itself.
        /// </remarks>
        /// 
        /// <returns>Instance of the item</returns>
        protected abstract TItem This();

        /// <summary>
        /// Returns unique identifier of the item.
        /// </summary>
        /// <remarks>
        /// It is extremely important to override this method because nodes are using HashSets to store items and the items are changing during the updates and so are the hash codes which can result in program not working properly.
        /// </remarks>
        /// 
        /// <returns>Unique identifier</returns>
        public override int GetHashCode()
        {
            return GetInstanceID();
        }
    }
}
