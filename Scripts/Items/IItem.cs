using UnityEngine;

namespace Quadtree.Items
{
    /// <summary>
    /// Mandatory interface of any quadtree item.
    /// </summary>
    public interface IItem<TItem, TNode>
        where TItem : IItem<TItem, TNode>
        where TNode : INode<TItem, TNode>
    {
        /// <summary>
        /// Returns object bounds.
        /// </summary>
        /// 
        /// <returns>Object box bounds.</returns>
        Bounds GetBounds();

        /// <summary>
        /// Node which currently contains the item.
        /// </summary>
        TNode ParentNode { get; set; }

        /// <summary>
        /// Receiver method for broadcasted tree initialization message.
        /// </summary>
        void QuadTree_Root_Initialized(IQuadtreeRoot<TItem, TNode> root);
    }
}