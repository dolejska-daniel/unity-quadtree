using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Main class of the Quadtree structure - it represents the root of the tree.
    /// </summary>
    public interface IQuadtreeRoot<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : INode<TItem, TNode>
    {
        /// <summary>
        /// The tree has been initialized and is ready to be used.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Node currently acting as a root of the tree.
        /// </summary>
        TNode CurrentRootNode { get; }

        /// <summary>
        /// Minimum possible size of any of the nodes.
        /// </summary>
        /// <remarks>
        /// Must always be a positive number or zero for no size limit.
        /// </remarks>
        float MinimumPossibleNodeSize { get; }

        /// <summary>
        /// Determines whether or not should number of items in nodes be displayed in gizmos.
        /// </summary>
        bool DisplayNumberOfItemsInGizmos { get; }

        /// <summary>
        /// Inserts item to the tree structure.
        /// </summary>
        /// 
        /// <param name="item">Item to be inserted</param>
        void Insert(TItem item);

        /// <summary>
        /// Expands size of root node.
        /// New root node is created and current root node is assigned as its sub-node.
        /// </summary>
        void Expand();

        /// <summary>
        /// Finds items located within provided boundaries.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries to look for items within</param>
        /// <returns>List of items found within provided boundaries</returns>
        List<TItem> Find(Bounds bounds);

        /// <summary>
        /// Removes provided item from the tree.
        /// </summary>
        /// 
        /// <param name="item">Item to be removed from the tree</param>
        void Remove(TItem item);

        /// <summary>
        /// Clears and resets the whole tree.
        /// </summary>
        void Clear();
    }
}