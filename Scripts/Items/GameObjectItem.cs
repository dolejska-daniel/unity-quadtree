using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Custom item interface for GameObject quadtree items.
    /// </summary>
    public abstract class GameObjectItem : MonoBehaviour, IItem<GameObjectItem>
    {
        /// <summary>
        /// Game object's bounds from last update call.
        /// </summary>
        private Bounds _lastBounds;

        /// <summary>
        /// Game object's bounds from last update call.
        /// </summary>
        private Bounds _safeBounds;

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
            _root = null;
            ItemInitialized = false;
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
                _parentNode?.Update(this, forceInsertionEvaluation);
                _lastBounds = currentBounds;
            }
        }

        /// <inheritdoc cref="IItem{TItem}.ParentNode"/>
        private Node<GameObjectItem> _parentNode = null;

        public Node<GameObjectItem> ParentNode {
            get => _parentNode;
            set => _parentNode = value;
        }

        public abstract Bounds GetBounds();

        /// <inheritdoc cref="_root"/>
        public RootNode<GameObjectItem> Root { get => _root; }

        /// <summary>
        /// Reference to the root of the tree.
        /// </summary>
        private RootNode<GameObjectItem> _root = null;

        /// <summary>
        /// <c>True</c> if the item has been initialized.
        /// </summary>
        protected bool ItemInitialized = false;

        public void QuadTree_Root_Initialized(RootNode<GameObjectItem> root)
        {
            _root = root;

            if (ItemInitialized)
            {
                // the item has been initialized before the tree root
                root.Insert(this);
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

            if (_root != null)
            {
                // the tree root has been initialized before the item
                _root.Insert(this);
            }
        }

        public override int GetHashCode()
        {
            // it is extremely important to override this method
            // because nodes are using HashSets to store items
            // and items are changing during the updates and so are the hash codes
            return GetInstanceID();
        }
    }
}
