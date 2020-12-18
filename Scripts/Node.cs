using Quadtree.Items;

namespace Quadtree
{
    /// <summary>
    /// Single quad tree node.
    /// </summary>
    public class Node<TItem> : NodeBase<TItem, Node<TItem>>
        where TItem : IItem<TItem, Node<TItem>>
    {
    }
}