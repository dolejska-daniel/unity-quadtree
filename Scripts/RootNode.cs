using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Main class of the Quadtree structure - it represents the root of the tree.
    /// </summary>
    public abstract class RootNode<TItem> : MonoBehaviour where TItem : class, IItem
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
            if (CurrentRootNode == null)
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
        /// Determines whether the structure should be automatically built upon its initialization.
        /// </summary>
        /// 
        /// <seealso cref="Rebuild"/>
        [SerializeField] protected bool BuildAutomatically = true;

        /// <summary>
        /// Initializes Quadtree - creates initial root node and builds the tree (if allowed).
        /// </summary>
        /// 
        /// <seealso cref="Rebuild"/>
        protected void Init()
        {
            CurrentRootNode = new Node<TItem>(transform.position, new Vector3(256f, 0f, 256f));
            if (BuildAutomatically)
                Rebuild();
        }

        /// <summary>
        /// Resets tree to its initial state.
        /// Inserts all children having any <c>Quadtree.Items.IItem</c> component attached into the tree structure.
        /// </summary>
        protected void Rebuild()
        {
            CurrentRootNode.Clear();
            var items = GetComponentsInChildren<TItem>();
            foreach (var item in items)
                Insert(item);
        }

        /// <summary>
        /// Inserts item to the tree structure.
        /// </summary>
        /// 
        /// <param name="item">Item to be inserted</param>
        public void Insert(TItem item)
        {
            var itemBounds = item.GetBounds();
            // validate boundaries
            Debug.AssertFormat(
                !float.IsInfinity(itemBounds.max.x)
                && !float.IsInfinity(itemBounds.max.z)
                && !float.IsInfinity(itemBounds.min.x)
                && !float.IsInfinity(itemBounds.min.z),
                "Item boundaries are too large - it is not possible to store such item in Quadtree.",
                item
            );

            if (!CurrentRootNode.Contains(itemBounds))
            {
                print(CurrentRootNode.Bounds);
                print(itemBounds);
                Debug.LogError("Item does not fit root node!");
                return;
            }

            // expand root node
            while (!CurrentRootNode.Contains(itemBounds))
                Expand();

            // insert item into the tree
            CurrentRootNode.Insert(item);
        }

        /// <summary>
        /// Expands size of root node.
        /// New root node is created and current root node is assigned as its sub-node.
        /// </summary>
        protected void Expand()
        {
            var subNodes = new List<Node<TItem>>(4);

            var subBoundsSize = CurrentRootNode.Bounds.size;
            var centerOffset = subBoundsSize * .5f;

            // center if expanding to left
            var center = CurrentRootNode.Bounds.min;
            if (ExpansionRight)
                // center if expanding to right
                center = CurrentRootNode.Bounds.max;

            // top left node [-x +y]
            centerOffset.x *= -1f;
            subNodes.Insert(0, new Node<TItem>(center + centerOffset, subBoundsSize));

            // top right node [+x +y]
            centerOffset.x *= -1f;
            subNodes.Insert(
                1, !ExpansionRight
                    ? CurrentRootNode
                    : new Node<TItem>(center + centerOffset, subBoundsSize)
            );

            // bottom right node [+x -y]
            centerOffset.z *= -1f;
            subNodes.Insert(2, new Node<TItem>(center + centerOffset, subBoundsSize));

            // bottom left node [-x -y]
            centerOffset.x *= -1f;
            subNodes.Insert(
                3, ExpansionRight
                    ? CurrentRootNode
                    : new Node<TItem>(center + centerOffset, subBoundsSize)
            );

            // assign new root node
            CurrentRootNode = new Node<TItem>(center, subBoundsSize * 2f, subNodes);
            // toggle side
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
        /// Displays Quadtree node boundaries.
        /// </summary>
        protected void DrawBounds()
        {
            CurrentRootNode?.DrawBounds();
        }
    }
}