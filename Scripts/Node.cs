using Quadtree.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Quadtree
{
    /// <summary>
    /// Single quad tree node.
    /// </summary>
    public class Node<TItem> where TItem : IItem<TItem>
    {
        /// <summary>
        /// Describes relative local position in respect to the current node.
        /// </summary>
        /// <remarks>
        /// Integer values of <c>UPPER_LEFT</c>, <c>UPPER_RIGHT</c>, <c>LOWER_RIGHT</c>, <c>LOWER_LEFT</c> do correspond with the indices of the sub-nodes.
        /// </remarks>
        public enum IntraLocation {
            UPPER_LEFT, UPPER_RIGHT, LOWER_RIGHT, LOWER_LEFT,
            SPANNING_LEFT, SPANNING_RIGHT, SPANNING_UPPER, SPANNING_LOWER, SPANNING
        };

        /// <summary>
        /// Bounds of this tree node.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// List of inserted items.
        /// </summary>
        private readonly HashSet<TItem> _items;

        /// <inheritdoc cref="_parentNode"/>
        public Node<TItem> ParentNode { get => _parentNode; internal set => _parentNode = value; }

        /// <summary>
        /// Reference to parent tree node.
        /// </summary>
        /// <remarks>
        /// Is <c>null</c> for root node of the tree.
        /// </remarks>
        private Node<TItem> _parentNode = null;

        /// <summary>
        /// Root of the whole tree.
        /// </summary>
        private RootNode<TItem> _root;

        /// <summary>
        /// List of child tree nodes.
        /// </summary>
        private readonly List<Node<TItem>> _subNodes;

        /// <summary>
        /// Creates new tree node at provided point (<paramref name="center"/>) with provided sizes (<paramref name="size"/>).
        /// List of predefined sub-nodes (<paramref name="subNodes"/>) and/or items (<paramref name="items"/>) may be provided.
        /// </summary>
        /// <remarks>
        /// No boundary validation is performed neither for provided sub-nodes (<paramref name="subNodes"/>) nor items (<paramref name="items"/>).
        /// </remarks>
        /// 
        /// <param name="root">Root node of the tree</param>
        /// <param name="parent">Parent node of this node</param>
        /// <param name="center">Center point of this node</param>
        /// <param name="size">Edge lengths of this node</param>
        /// <param name="subNodes">Predefined sub-nodes of this node</param>
        /// <param name="items">Predefined items contained by this node</param>
        public Node(RootNode<TItem> root, Node<TItem> parent, Vector3 center, Vector3 size,
            List<Node<TItem>> subNodes = null, HashSet<TItem> items = null)
        {
            Bounds = new Bounds(center, size);
            ParentNode = parent;
            _root = root;
            _subNodes = subNodes ?? new List<Node<TItem>>(4);
            _items = items ?? new HashSet<TItem>();
        }

        /// <summary>
        /// Verifies whether provided boundaries (<paramref name="bounds"/>) are fully contained within the boundaries of the node.
        /// </summary>
        /// 
        /// <param name="bounds">Boundaries of an object</param>
        /// <returns><c>True</c> if object is fully contained within the node, <c>False</c> otherwise</returns>
        public bool Contains(Bounds bounds) =>
            bounds.min.x >= Bounds.min.x
            && bounds.min.z >= Bounds.min.z
            && bounds.max.x < Bounds.max.x
            && bounds.max.z < Bounds.max.z;

        /// <summary>
        /// Calculates relative internal position of the provided bounds (<paramref name="bounds"/>) within the node.
        /// </summary>
        /// <remarks>
        /// The method expects the boundaries to be fully contained within the node.
        /// </remarks>
        /// 
        /// <param name="bounds">Boundaries contained within the node</param>
        /// <returns>Relative internal position</returns>
        public IntraLocation Location(Bounds bounds)
        {
            if (bounds.min.z >= Bounds.center.z)
            {
                // items are located in top sub-nodes
                if (bounds.max.x < Bounds.center.x)
                {
                    // items are located in top left sub-node
                    return IntraLocation.UPPER_LEFT;
                }
                else if (bounds.min.x >= Bounds.center.x)
                {
                    // items are located in top right sub-node
                    return IntraLocation.UPPER_RIGHT;
                }
                else
                {
                    // item does not fit to either one, but is top
                    // (max.x is right, min.x is left)
                    return IntraLocation.SPANNING_UPPER;
                }
            }
            else if (bounds.max.z < Bounds.center.z)
            {
                // items are located in bottom sub-nodes
                if (bounds.max.x < Bounds.center.x)
                {
                    // items are located in bottom left sub-node
                    return IntraLocation.LOWER_LEFT;
                }
                else if (bounds.min.x >= Bounds.center.x)
                {
                    // items are located in bottom right sub-node
                    return IntraLocation.LOWER_RIGHT;
                }
                else
                {
                    // item does not fit to either one, but is bottom
                    // (max.x is right, min.x is left)
                    return IntraLocation.SPANNING_LOWER;
                }
            }
            else
            {
                // item does not fit to any sub-node
                // (max.z is top, min.z is bottom)
                if (bounds.min.x >= Bounds.center.x)
                {
                    // bounds span over top right and bottom right nodes
                    return IntraLocation.SPANNING_RIGHT;
                }
                else if (bounds.max.x < Bounds.center.x)
                {
                    // bounds span over top left and bottom left nodes
                    return IntraLocation.SPANNING_LEFT;
                }
                else
                {
                    // bounds span over all sub-nodes
                    return IntraLocation.SPANNING;
                }
            }
        }

        /// <summary>
        /// Inserts item (<paramref name="item"/>) into the smallest node possible in the subtree.
        /// </summary>
        /// <remarks>
        /// The method expects item boundaries to be fully contained within the node.
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
                // insert item into this node
                _items.Add(item);
                // and designate this node its parent
                item.ParentNode = this;

                return;
            }

            var itemBounds = item.GetBounds();
            var itemBoundsLocation = Location(itemBounds);
            switch (itemBoundsLocation)
            {
                // boundaries are contained within one of the subnodes
                case IntraLocation.UPPER_LEFT:
                case IntraLocation.UPPER_RIGHT:
                case IntraLocation.LOWER_RIGHT:
                case IntraLocation.LOWER_LEFT:
                    _subNodes[(int)itemBoundsLocation].Insert(item);
                    break;

                // boundaries are spanning over 2 or more subnodes
                default:
                    _items.Add(item);
                    item.ParentNode = this;
                    break;
            }
        }

        /// <summary>
        /// Removes the provided item (<paramref name="item"/>) from the node and its subtree.
        /// </summary>
        /// 
        /// <param name="item">Item to be removed from the tree</param>
        public void Remove(TItem item)
        {
            var itemBounds = item.GetBounds();
            var itemBoundsLocation = Location(itemBounds);
            switch (itemBoundsLocation)
            {
                // boundaries are contained within one of the subnodes
                case IntraLocation.UPPER_LEFT:
                case IntraLocation.UPPER_RIGHT:
                case IntraLocation.LOWER_RIGHT:
                case IntraLocation.LOWER_LEFT:
                    _subNodes[(int)itemBoundsLocation].Remove(item);
                    break;

                // boundaries are spanning over 2 or more subnodes
                default:
                    RemoveOwnItem(item);
                    break;
            }
        }

        /// <summary>
        /// Removes provided item (<paramref name="item"/>) from the node.
        /// </summary>
        /// 
        /// <param name="item">Item to be removed from the node</param>
        protected internal void RemoveOwnItem(TItem item)
        {
            // remove the item from the node
            _items.Remove(item);
            // update its parent node
            item.ParentNode = null;

            if (IsEmpty())
            {
                // remove subnodes if subtree of this node is empty
                _subNodes.Clear();
            }
        }

        /// <summary>
        /// Checks whether the node and recursively all its subnodes are empty.
        /// </summary>
        /// 
        /// <returns><c>True</c> if node and all its subnodes are empty, <c>False</c> otherwise</returns>
        public bool IsEmpty()
        {
            if (_items.Count > 0)
                return false;

            return _subNodes.TrueForAll(node => node.IsEmpty());
        }

        /// <summary>
        /// Updates provided item's (<paramref name="item"/>) location within the tree.
        /// </summary>
        /// 
        /// <param name="item">Item which's location is to be updated</param>
        /// <param name="forceInsertionEvaluation"><c>True</c> forces tree to re-insert the item</param>
        /// <param name="hasOriginallyContainedItem"><c>True</c> only for the first called node</param>
        protected internal void Update(TItem item, bool forceInsertionEvaluation = true, bool hasOriginallyContainedItem = true)
        {
            if (Contains(item.GetBounds()))
            {
                // item is contained by this node
                if (hasOriginallyContainedItem)
                {
                    // ...and this node has originally contained the item
                    if (forceInsertionEvaluation)
                    {
                        // ...and insertion evaluation is forced
                        // this checks whether the item hasn't moved into any of the subnodes
                        RemoveOwnItem(item);
                        Insert(item);
                    }

                    // item is still contained by its original node, no action necessary
                    return;
                }

                // ...but this node is not its original container
                // insert item either to this node or any of its children
                Insert(item);

                // update has been successful
                return;
            }

            // the item is not contained by this node
            if (_parentNode == null)
            {
                // ...and this node does not have any parent - the tree must be expanded
                _root.Expand();
                if (_parentNode == null)
                {
                    // the expansion has failed for some reason
                    Debug.LogError("Tree root expansion failed for item " + item.ToString());
                    return;
                }
            }

            // the item is not contained by this node
            if (hasOriginallyContainedItem)
            {
                // ...and this node has originally contained the item - it must be removed
                RemoveOwnItem(item);
            }

            // parent is (now) available
            _parentNode.Update(item, forceInsertionEvaluation, false);
            // the item is now contained by another node, update has been successful
        }

        /// <summary>
        /// Creates sub-nodes for the node.
        /// </summary>
        protected internal void CreateSubNodes()
        {
            var subBoundsSize = Bounds.size * .5f;
            if (subBoundsSize.x < _root.NodeMinimumPossibleSize
                || subBoundsSize.z < _root.NodeMinimumPossibleSize)
            {
                // new sub-node bounds are too small
                return;
            }

            var centerOffset = subBoundsSize * .5f;

            // top left node [-x +z]
            centerOffset.x *= -1f;
            _subNodes.Insert(
                (int)IntraLocation.UPPER_LEFT,
                new Node<TItem>(_root, this, Bounds.center + centerOffset, subBoundsSize)
            );

            // top right node [+x +z]
            centerOffset.x *= -1f;
            _subNodes.Insert(
                (int)IntraLocation.UPPER_RIGHT,
                new Node<TItem>(_root, this, Bounds.center + centerOffset, subBoundsSize)
            );

            // bottom right node [+x -z]
            centerOffset.z *= -1f;
            _subNodes.Insert(
                (int)IntraLocation.LOWER_RIGHT,
                new Node<TItem>(_root, this, Bounds.center + centerOffset, subBoundsSize)
            );

            // bottom left node [-x -z]
            centerOffset.x *= -1f;
            _subNodes.Insert(
                (int)IntraLocation.LOWER_LEFT,
                new Node<TItem>(_root, this, Bounds.center + centerOffset, subBoundsSize)
            );
        }

        /// <summary>
        /// Finds items (<paramref name="items"/>) located within provided boundaries (<paramref name="bounds"/>).
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

            // always add any items in this node intersecting with the boundaries
            AddOwnItems(ref items, bounds);

            var boundsLocation = Location(bounds);
            switch (boundsLocation)
            {
                // boundaries are contained within one of the subnodes
                case IntraLocation.UPPER_LEFT:
                case IntraLocation.UPPER_RIGHT:
                case IntraLocation.LOWER_RIGHT:
                case IntraLocation.LOWER_LEFT:
                    _subNodes[(int)boundsLocation].FindAndAddItems(bounds, ref items);
                    break;

                // boundaries are spanning over left subnodes
                case IntraLocation.SPANNING_LEFT:
                    _subNodes[(int)IntraLocation.UPPER_LEFT].AddItems(ref items, bounds);
                    _subNodes[(int)IntraLocation.LOWER_LEFT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over right subnodes
                case IntraLocation.SPANNING_RIGHT:
                    _subNodes[(int)IntraLocation.UPPER_RIGHT].AddItems(ref items, bounds);
                    _subNodes[(int)IntraLocation.LOWER_RIGHT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over upper subnodes
                case IntraLocation.SPANNING_UPPER:
                    _subNodes[(int)IntraLocation.UPPER_LEFT].AddItems(ref items, bounds);
                    _subNodes[(int)IntraLocation.UPPER_RIGHT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over lower subnodes
                case IntraLocation.SPANNING_LOWER:
                    _subNodes[(int)IntraLocation.LOWER_LEFT].AddItems(ref items, bounds);
                    _subNodes[(int)IntraLocation.LOWER_RIGHT].AddItems(ref items, bounds);
                    break;

                // boundaries are spanning over all subnodes
                case IntraLocation.SPANNING:
                default:
                    AddSubNodeItems(ref items, bounds);
                    break;
            }
        }

        /// <summary>
        /// Adds all items of this node and its sub-nodes to the provided list of items (<paramref name="items"/>).
        /// If boundaries (<paramref name="bounds"/>) are provided then only items intersecting with them will be added.
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
        /// Adds all items belonging to this node (ignoring sub-nodes) to the provided list of items (<paramref name="items"/>).
        /// If boundaries (<paramref name="bounds"/>) are provided then only items intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        protected internal void AddOwnItems(ref List<TItem> items, Bounds? bounds = null)
        {
            items.AddRange(
                bounds != null
                    ? _items.Where(item => item.GetBounds().Intersects((Bounds) bounds))
                    : _items
            );
        }

        /// <summary>
        /// Adds all items belonging to sub-nodes (ignoring own items) to the provided list of items (<paramref name="items"/>).
        /// If boundaries (<paramref name="bounds"/>) are provided then only items intersecting with them will be added.
        /// </summary>
        /// 
        /// <param name="items">Output list for found items</param>
        /// <param name="bounds">Boundaries to look for items within</param>
        protected internal void AddSubNodeItems(ref List<TItem> items, Bounds? bounds = null)
        {
            foreach (var subNode in _subNodes)
                subNode.AddItems(ref items, bounds);
        }

        /// <summary>
        /// Removes any existing items from the node and removes all of its sub-nodes.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _subNodes.Clear();
        }

        /// <summary>
        /// Displays boundaries of this node and all its sub-nodes and a current number of contained items if allowed.
        /// </summary>
        public void DrawBounds()
        {
            if (_root.DisplayNumberOfItemsInNodeGizmos)
                Handles.Label(Bounds.center, _items.Count.ToString());

            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
            foreach (var subNode in _subNodes)
                subNode.DrawBounds();
        }
    }
}