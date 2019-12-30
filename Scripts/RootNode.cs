using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Main class of the Quadtree structure - it represents the root of the tree.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Spatial partitioning/Quadtree/Root node")]
    public class RootNode : MonoBehaviour
    {
        //==========================================================================dd==
        //  MonoBehaviour METHODS
        //==========================================================================dd==

        private void Start()
        {
            Init();
        }

        private void OnDrawGizmos()
        {
            DrawBounds();
        }

        //==========================================================================dd==
        //  CORE Quadtree METHODS
        //==========================================================================dd==

        /// <summary>
        /// Root node containing all items and sub-nodes.
        /// </summary>
        private Node _rootNode;

        /// <summary>
        /// Determines side of root node expansion if necessary.
        /// </summary>
        private bool _expansionRight;

        /// <summary>
        /// Determines whether the structure should be automatically built upon its initialization.
        /// </summary>
        /// <seealso cref="Rebuild"/>
        [SerializeField] private bool _buildAutomatically = true;

        /// <summary>
        /// Initializes Quadtree.
        /// Creates initial root node and builds the tree.
        /// </summary>
        /// <seealso cref="Rebuild"/>
        private void Init()
        {
            _rootNode = new Node(transform.position, new Vector3(256f, 0f, 256f));
            if (_buildAutomatically)
                Rebuild();
        }

        /// <summary>
        /// Resets tree to its initial state.
        /// Inserts all children having any <c>Quadtree.Items.IItem</c> component attached into the tree structure.
        /// </summary>
        [ContextMenu("Rebuild Quadtree")]
        private void Rebuild()
        {
            _rootNode.Clear();
            var items = GetComponentsInChildren<IItem>();
            foreach (var item in items)
                Insert(item);
        }

        /// <summary>
        /// Inserts item to the tree structure.
        /// </summary>
        /// 
        /// <param name="item">Item to be inserted</param>
        public void Insert(IItem item)
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

            if (!_rootNode.Contains(itemBounds))
            {
                print(_rootNode.Bounds);
                print(itemBounds);
                Debug.LogError("Item does not fit root node!");
                return;
            }

            // expand root node
            while (!_rootNode.Contains(itemBounds))
                Expand();

            // insert item into the tree
            _rootNode.Insert(item);
        }

        /// <summary>
        /// Expands size of root node.
        /// New root node is created and current root node is assigned as its sub-node.
        /// </summary>
        private void Expand()
        {
            var subNodes = new List<Node>(4);

            var subBoundsSize = _rootNode.Bounds.size;
            var centerOffset = subBoundsSize * .5f;

            // center if expanding to left
            var center = _rootNode.Bounds.min;
            if (_expansionRight)
                // center if expanding to right
                center = _rootNode.Bounds.max;

            // top left node [-x +y]
            centerOffset.x *= -1f;
            subNodes.Insert(0, new Node(center + centerOffset, subBoundsSize));

            // top right node [+x +y]
            centerOffset.x *= -1f;
            subNodes.Insert(
                1, !_expansionRight
                    ? _rootNode
                    : new Node(center + centerOffset, subBoundsSize)
            );

            // bottom right node [+x -y]
            centerOffset.z *= -1f;
            subNodes.Insert(2, new Node(center + centerOffset, subBoundsSize));

            // bottom left node [-x -y]
            centerOffset.x *= -1f;
            subNodes.Insert(
                3, !_expansionRight
                    ? _rootNode
                    : new Node(center + centerOffset, subBoundsSize)
            );

            // assign new root node
            _rootNode = new Node(center, subBoundsSize * 2f, subNodes);
            // toggle side
            _expansionRight = !_expansionRight;
        }

        /// <summary>
        /// Finds items located within provided boundaries.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries to look for items within</param>
        /// <returns>List of items found within provided boundaries</returns>
        public List<IItem> Find(Bounds bounds)
        {
            var itemList = new List<IItem>();
            _rootNode.FindAndAddItems(bounds, ref itemList);

            return itemList;
        }

        /// <summary>
        /// Displays Quadtree node boundaries.
        /// </summary>
        private void DrawBounds()
        {
            _rootNode?.DrawBounds();
        }
    }
}