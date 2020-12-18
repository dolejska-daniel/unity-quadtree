using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Main class of the Quadtree structure - it represents the root of the tree.
    /// </summary>
    public abstract class QuadtreeMonoRoot<TItem, TNode> : MonoBehaviour, IQuadtreeRoot<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : INode<TItem, TNode>, new()
    {
        //==========================================================================dd==
        //  MonoBehaviour METHODS
        //==========================================================================dd==

        protected void Start()
        {
            Init();
        }

        protected void OnEnable()
        {
            Init();
        }

        protected void OnDrawGizmos()
        {
            DrawBounds();
        }

        //==========================================================================dd==
        //  CORE ROOT NODE METHODS
        //==========================================================================dd==

        /// <summary>
        /// Root node containing all items and sub-nodes.
        /// </summary>
        protected QuadtreeRoot<TItem, TNode> TreeRoot = null;

        public bool Initialized => TreeRoot != null && TreeRoot.Initialized;

        public TNode CurrentRootNode => TreeRoot.CurrentRootNode;

        [SerializeField]
        protected Vector3 DefaultRootNodeSize = new Vector3(64f, 0f, 64f);

        /// <inheritdoc cref="IQuadtreeRoot{TItem, TNode}.MinimumPossibleNodeSize"/>
        [SerializeField]
        protected float MinimumPossibleNodeSize = 1f;

        float IQuadtreeRoot<TItem, TNode>.MinimumPossibleNodeSize => MinimumPossibleNodeSize;

        /// <inheritdoc cref="IQuadtreeRoot{TItem, TNode}.DisplayNumberOfItemsInGizmos"/>
        [SerializeField]
        private bool DisplayNumberOfItemsInGizmos = false;

        bool IQuadtreeRoot<TItem, TNode>.DisplayNumberOfItemsInGizmos => DisplayNumberOfItemsInGizmos;

        /// <summary>
        /// Initializes Quadtree - creates initial root node and builds the tree (if allowed).
        /// </summary>
        /// 
        /// <seealso cref="IItem{TItem, TNode}.QuadTree_Root_Initialized(IQuadtreeRoot{TItem, TNode})"/>
        protected void Init()
        {
            if (TreeRoot == null)
            {
                TreeRoot = new QuadtreeRoot<TItem, TNode>(transform.position, DefaultRootNodeSize);
            }
            else
            {
                // root node has already been initialized, clear the tree
                TreeRoot.Clear();
            }

            // send a message to children that the tree root has been initialized
            BroadcastMessage("QuadTree_Root_Initialized", this, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Displays Quadtree node boundaries.
        /// </summary>
        /// 
        /// <seealso cref="INode{TItem, TNode}.DrawBounds(bool)"/>
        protected void DrawBounds()
        {
            TreeRoot?.CurrentRootNode.DrawBounds(DisplayNumberOfItemsInGizmos);
        }

        public void Insert(TItem item) => TreeRoot.Insert(item);

        public void Expand() => TreeRoot.Expand();

        public List<TItem> Find(Bounds bounds) => TreeRoot.Find(bounds);

        public void Remove(TItem item) => TreeRoot.Remove(item);

        public void Clear() => TreeRoot.Clear();
    }
}