using Quadtree.Items;

namespace Quadtree
{
    /// <summary>
    /// Single quadtree node.
    /// </summary>
    public class Node<TItem> : NodeBase<TItem, Node<TItem>>
        where TItem : IItem<TItem, Node<TItem>>
    {
    }
}