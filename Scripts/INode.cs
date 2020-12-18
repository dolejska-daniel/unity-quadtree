using Quadtree.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Mandatory interface of any single quadtree node.
    /// </summary>
    public interface INode<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : INode<TItem, TNode>
    {
        /// <summary>
        /// Bounds of this tree node.
        /// </summary>
        Bounds Bounds { get; set; }

        /// <summary>
        /// Root of the whole tree.
        /// </summary>
        IQuadtreeRoot<TItem, TNode> TreeRoot { get; set; }

        /// <summary>
        /// Reference to parent tree node.
        /// </summary>
        /// <remarks>
        /// Is <c>null</c> for root node of the tree.
        /// </remarks>
        TNode ParentNode { get; set; }

        /// <summary>
        /// Child nodes of this node.
        /// </summary>
        IList<TNode> SubNodes { get; set; }

        /// <summary>
        /// Verifies whether provided boundaries (<paramref name="bounds"/>) are fully contained within the boundaries of the node.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries of an object</param>
        /// <returns><c>True</c> if object is fully contained within the node, <c>False</c> otherwise</returns>
        bool Contains(Bounds bounds);

        /// <summary>
        /// Calculates relative internal position of the provided bounds (<paramref name="bounds"/>) within the node.
        /// </summary>
        /// <remarks>
        /// The method expects the boundaries to be fully contained within the node.
        /// </remarks>
        /// 
        /// <param name="bounds">Boundaries contained within the node</param>
        /// <returns>Relative internal position</returns>
        IntraLocation Location(Bounds bounds);

        /// <summary>
        /// Inserts item (<paramref name="item"/>) into the smallest node possible in the subtree.
        /// </summary>
        /// <remarks>
        /// The method expects item boundaries to be fully contained within the node.
        /// </remarks>
        /// 
        /// <param name="item">Item to be inserted</param>
        void Insert(TItem item);

        /// <summary>
        /// Removes the provided item (<paramref name="item"/>) from the node and its subtree.
        /// </summary>
        /// 
        /// <param name="item">Item to be removed from the tree</param>
        void Remove(TItem item);

        /// <summary>
        /// Checks whether the node and recursively all its subnodes are empty.
        /// </summary>
        /// 
        /// <returns><c>True</c> if node and all its subnodes are empty, <c>False</c> otherwise</returns>
        bool IsEmpty();

        /// <summary>
        /// Updates provided item's (<paramref name="item"/>) location within the tree.
        /// </summary>
        /// 
        /// <param name="item">Item which's location is to be updated</param>
        /// <param name="forceInsertionEvaluation"><c>True</c> forces tree to re-insert the item</param>
        /// <param name="hasOriginallyContainedItem"><c>True</c> only for the first called node</param>
        void Update(TItem item, bool forceInsertionEvaluation = true, bool hasOriginallyContainedItem = true);

        /// <summary>
        /// Finds items (<paramref name="items"/>) located within provided boundaries (<paramref name="bounds"/>).
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries to look for items within</param>
        /// <param name="items">Output list for found items</param>
        void FindAndAddItems(Bounds bounds, ref IList<TItem> items);

        /// <summary>
        /// Adds all items of this node and its sub-nodes to the provided list of items (<paramref name="items"/>).
        /// If boundaries (<paramref name="bounds"/>) are provided then only items intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        void AddItems(ref IList<TItem> items, Bounds? bounds = null);

        /// <summary>
        /// Removes any existing items from the node and removes all of its sub-nodes.
        /// </summary>
        void Clear();

        /// <summary>
        /// Displays boundaries of this node and all its sub-nodes and optinally a current number of contained items if <paramref name="displayNumberOfItems"/> is <c>True</c>.
        /// </summary>
        /// 
        /// <param name="displayNumberOfItems"><c>True</c> if number of node's items should be displayed</param>
        void DrawBounds(bool displayNumberOfItems = false);
    }
}