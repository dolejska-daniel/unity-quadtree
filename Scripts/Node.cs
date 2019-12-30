using Quadtree.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Quadtree
{
    /// <summary>
    /// Single quad tree node.
    /// </summary>
    public class Node<TItem> where TItem : class, IItem
    {
        /// <summary>
        /// Minimum possible value for node size.
        /// </summary>
        public const float MinSize = 1f;

        /// <summary>
        /// Bounds of this quad tree node.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Underlying quad tree nodes.
        /// </summary>
        private readonly List<Node<TItem>> _subNodes;

        /// <summary>
        /// List of inserted items.
        /// </summary>
        private readonly List<TItem> _items;

        /// <summary>
        /// Creates new quad tree node at provided point with provided sizes and sub-nodes.
        /// </summary>
        /// <remarks>
        /// No boundary validation is performed for provided sub-nodes.
        /// </remarks>
        /// 
        /// <param name="center">Center point of this node</param>
        /// <param name="size">Edge lengths of this node</param>
        /// <param name="subNodes">Custom defined sub-nodes</param>
        public Node(Vector3 center, Vector3 size, List<Node<TItem>> subNodes)
        {
            Bounds = new Bounds(center, size);
            _subNodes = subNodes;
            _items = new List<TItem>();
        }

        /// <summary>
        /// Creates new quad tree node at provided point with provided sizes.
        /// </summary>
        /// 
        /// <param name="center">Center point of this node</param>
        /// <param name="size">Edge lengths of this node</param>
        public Node(Vector3 center, Vector3 size) : this(center, size, new List<Node<TItem>>(4))
        {
        }

        /// <summary>
        /// Verifies whether provided boundaries are fully contained within boundaries of this node.
        /// </summary>
        /// 
        /// <param name="objectBounds">Boundaries of an object</param>
        /// <returns>Object is/is not fully contained</returns>
        public bool Contains(Bounds objectBounds) =>
            objectBounds.min.x >= Bounds.min.x
            && objectBounds.min.z >= Bounds.min.z
            && objectBounds.max.x <= Bounds.max.x
            && objectBounds.max.z <= Bounds.max.z;

        /// <summary>
        /// Inserts item into smallest node possible in hierrarchy.
        /// </summary>
        /// <remarks>
        /// Node's insert method expects item boundaries to be fully contained within node's boundaries.
        /// </remarks>
        /// 
        /// <param name="item">Item to be inserted</param>
        public void Insert(TItem item)
        {
            // create new sub-nodes
            if (_subNodes.Count == 0)
                CreateSubNodes();

            // sub-nodes can not be created anymore
            if (_subNodes.Count == 0)
            {
                _items.Add(item);
                return;
            }

            var itemBounds = item.GetBounds();
            if (itemBounds.min.z >= Bounds.center.z)
            {
                // item is located in top sub-nodes
                if (itemBounds.max.x < Bounds.center.x)
                {
                    // item is located in top left sub-node
                    _subNodes[0].Insert(item);
                }
                else if (itemBounds.min.x >= Bounds.center.x)
                {
                    // item is located in top right sub-node
                    _subNodes[1].Insert(item);
                }
                else
                {
                    // item does not fit to either one
                    // (max.x is right, min.x is left)
                    _items.Add(item);
                }
            }
            else if (itemBounds.max.z < Bounds.center.z)
            {
                // item is located in bottom sub-nodes
                if (itemBounds.max.x < Bounds.center.x)
                {
                    // item is located in bottom left sub-node
                    _subNodes[3].Insert(item);
                }
                else if (itemBounds.min.x >= Bounds.center.x)
                {
                    // item is located in bottom right sub-node
                    _subNodes[2].Insert(item);
                }
                else
                {
                    // item does not fit to either one
                    // (max.x is right, min.x is left)
                    _items.Add(item);
                }
            }
            else
            {
                // item does not fit to any sub-node
                // (max.z is top, min.z is bottom)
                _items.Add(item);
            }
        }

        /// <summary>
        /// Splits current node to appropriate sub-nodes.
        /// </summary>
        private void CreateSubNodes()
        {
            var subBoundsSize = Bounds.size * .5f;
            if (subBoundsSize.x < MinSize || subBoundsSize.z < MinSize)
            {
                // new sub-node bounds are too small
                return;
            }

            var centerOffset = subBoundsSize * .5f;

            // top left node [-x +z]
            centerOffset.x *= -1f;
            _subNodes.Insert(0, new Node<TItem>(Bounds.center + centerOffset, subBoundsSize));

            // top right node [+x +z]
            centerOffset.x *= -1f;
            _subNodes.Insert(1, new Node<TItem>(Bounds.center + centerOffset, subBoundsSize));

            // bottom right node [+x -z]
            centerOffset.z *= -1f;
            _subNodes.Insert(2, new Node<TItem>(Bounds.center + centerOffset, subBoundsSize));

            // bottom left node [-x -z]
            centerOffset.x *= -1f;
            _subNodes.Insert(3, new Node<TItem>(Bounds.center + centerOffset, subBoundsSize));
        }

        /// <summary>
        /// Finds items located within provided boundaries.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries to look for items within</param>
        /// <param name="items">Output list for found items</param>
        public void FindAndAddItems(Bounds bounds, ref List<TItem> items)
        {
            if (_subNodes.Count == 0)
            {
                // no sub-nodes exist
                AddOwnItems(ref items);
                return;
            }

            if (bounds.min.z >= Bounds.center.z)
            {
                // items are located in top sub-nodes
                if (bounds.max.x < Bounds.center.x)
                {
                    // items are located in top left sub-node
                    _subNodes[0].FindAndAddItems(bounds, ref items);
                }
                else if (bounds.min.x >= Bounds.center.x)
                {
                    // items are located in top right sub-node
                    _subNodes[1].FindAndAddItems(bounds, ref items);
                }
                else
                {
                    // item does not fit to either one, but is top
                    // (max.x is right, min.x is left)
                    // top left
                    _subNodes[0].AddItems(ref items, bounds);
                    // top right
                    _subNodes[1].AddItems(ref items, bounds);
                }
            }
            else if (bounds.max.z < Bounds.center.z)
            {
                // items are located in bottom sub-nodes
                if (bounds.max.x < Bounds.center.x)
                {
                    // items are located in bottom left sub-node
                    _subNodes[3].FindAndAddItems(bounds, ref items);
                }
                else if (bounds.min.x >= Bounds.center.x)
                {
                    // items are located in bottom right sub-node
                    _subNodes[2].FindAndAddItems(bounds, ref items);
                }
                else
                {
                    // item does not fit to either one, but is bottom
                    // (max.x is right, min.x is left)
                    // bottom left
                    _subNodes[3].AddItems(ref items, bounds);
                    // bottom right
                    _subNodes[2].AddItems(ref items, bounds);
                }
            }
            else
            {
                // item does not fit to any sub-node
                // (max.z is top, min.z is bottom)
                if (bounds.min.x >= Bounds.center.x)
                {
                    // bounds span over
                    // top right
                    _subNodes[1].AddItems(ref items, bounds);
                    // and bottom right sub-nodes
                    _subNodes[2].AddItems(ref items, bounds);
                }
                else if (bounds.max.x < Bounds.center.x)
                {
                    // bounds span over
                    // top left
                    _subNodes[0].AddItems(ref items, bounds);
                    // and bottom left sub-nodes
                    _subNodes[3].AddItems(ref items, bounds);
                }
                else
                {
                    // bounds span over all sub-nodes
                    AddItems(ref items);
                }
            }
        }

        /// <summary>
        /// Adds all items of this node and its sub-nodes to provided list of items.
        /// If additional boundaries are provided only items truly intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        public void AddItems(ref List<TItem> items, Bounds? bounds = null)
        {
            AddOwnItems(ref items, bounds);
            AddSubNodeItems(ref items, bounds);
        }

        /// <summary>
        /// Adds all items belonging to this node (ignoring sub-nodes).
        /// If additional boundaries are provided only items truly intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        private void AddOwnItems(ref List<TItem> items, Bounds? bounds = null)
        {
            items.AddRange(
                bounds != null
                    ? _items.Where(item => item.GetBounds().Intersects((Bounds) bounds))
                    : _items
            );
        }

        /// <summary>
        /// Adds all items belonging to sub-nodes (ignoring own items).
        /// If additional boundaries are provided only items truly intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        private void AddSubNodeItems(ref List<TItem> items, Bounds? bounds = null)
        {
            foreach (var subNode in _subNodes)
                subNode.AddItems(ref items, bounds);
        }

        /// <summary>
        /// Removes any existing items or sub-nodes.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _subNodes.Clear();
        }

        /// <summary>
        /// Displays boundaries of this node and all its sub-nodes.
        /// </summary>
        public void DrawBounds()
        {
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
            foreach (var subNode in _subNodes)
                subNode.DrawBounds();
        }
    }
}