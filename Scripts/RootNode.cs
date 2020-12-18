using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Main class of the Quadtree structure - it represents the root of the tree.
    /// </summary>
    public abstract class RootNode<TItem> : MonoBehaviour where TItem : IItem<TItem>
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
        //  CORE Quadtree METHODS
        //==========================================================================dd==

        /// <summary>
        /// Root node containing all items and sub-nodes.
        /// </summary>
        protected Node<TItem> CurrentRootNode;

        /// <summary>
        /// Determines side of root node expansion if necessary.
        /// </summary>
        protected bool ExpansionRight;

        /// <summary>
        /// Determines size of the initial root node of the tree.
        /// </summary>
        /// <remarks>
        /// Only X and Z coordinates should be used.
        /// </remarks>
        [SerializeField]
        protected Vector3 DefaultRootNodeSize = new Vector3(64f, 0f, 64f);

        /// <summary>
        /// Determines whether or not should number of items in nodes be displayed in gizmos.
        /// </summary>
        [SerializeField]
        protected internal bool DisplayNumberOfItemsInNodeGizmos = false;

        /// <summary>
        /// Initializes Quadtree - creates initial root node and builds the tree (if allowed).
        /// </summary>
        /// 
        /// <seealso cref="Rebuild"/>
        protected void Init()
        {
            if (CurrentRootNode == null)
            {
                // root node has not been initialized yet, create initial node
                CurrentRootNode = new Node<TItem>(this, null, transform.position, DefaultRootNodeSize);
            }
            else
            {
                // root node has already been initialized, clear the tree
                CurrentRootNode.Clear();
            }

            BroadcastMessage("QuadTree_Root_Initialized", this);
        }

        /// <summary>
        /// Inserts item to the tree structure.
        /// </summary>
        /// 
        /// <param name="item">Item to be inserted</param>
        public void Insert(TItem item)
        {
            // get item bounds
            var itemBounds = item.GetBounds();

            // expand root node if necessary
            while (!CurrentRootNode.Contains(itemBounds))
                Expand();

            // insert item into the tree
            CurrentRootNode.Insert(item);
        }

        /// <summary>
        /// Expands size of root node.
        /// New root node is created and current root node is assigned as its sub-node.
        /// </summary>
        protected internal void Expand()
        {
            var subBoundsSize = CurrentRootNode.Bounds.size;
            var centerOffset = subBoundsSize * .5f;

            // center if expanding to left
            var center = CurrentRootNode.Bounds.min;
            if (ExpansionRight)
                // center if expanding to right
                center = CurrentRootNode.Bounds.max;

            var subNodes = new List<Node<TItem>>(4);
            var newRootNode = new Node<TItem>(this, null, center, subBoundsSize * 2f, subNodes);
            CurrentRootNode.ParentNode = newRootNode;

            // top left node [-x +y]
            centerOffset.x *= -1f;
            subNodes.Insert(0, new Node<TItem>(this, newRootNode, center + centerOffset, subBoundsSize));

            // top right node [+x +y]
            centerOffset.x *= -1f;
            subNodes.Insert(
                1, !ExpansionRight
                    ? CurrentRootNode
                    : new Node<TItem>(this, newRootNode, center + centerOffset, subBoundsSize)
            );

            // bottom right node [+x -y]
            centerOffset.z *= -1f;
            subNodes.Insert(2, new Node<TItem>(this, newRootNode, center + centerOffset, subBoundsSize));

            // bottom left node [-x -y]
            centerOffset.x *= -1f;
            subNodes.Insert(
                3, ExpansionRight
                    ? CurrentRootNode
                    : new Node<TItem>(this, newRootNode, center + centerOffset, subBoundsSize)
            );

            // assign new root node
            CurrentRootNode = newRootNode;
            // toggle expansion side
            ExpansionRight = !ExpansionRight;
        }

        /// <summary>
        /// Finds items located within provided boundaries.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries to look for items within</param>
        /// <returns>List of items found within provided boundaries</returns>
        public List<TItem> Find(Bounds bounds)
        {
            var itemList = new List<TItem>();
            CurrentRootNode.FindAndAddItems(bounds, ref itemList);

            return itemList;
        }

        /// <summary>
        /// Removes provided item from the tree.
        /// </summary>
        /// 
        /// <param name="item">Item to be removed from the tree</param>
        public void Remove(TItem item)
        {
            CurrentRootNode.Remove(item);
        }

        /// <summary>
        /// Displays Quadtree node boundaries.
        /// </summary>
        protected void DrawBounds()
        {
            CurrentRootNode?.DrawBounds();
        }
    }
}