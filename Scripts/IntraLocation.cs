
namespace Quadtree
{
    /// <summary>
    /// Describes relative local position in respect to the current node.
    /// </summary>
    /// <remarks>
    /// Integer values of <c>UPPER_LEFT</c>, <c>UPPER_RIGHT</c>, <c>LOWER_RIGHT</c>, <c>LOWER_LEFT</c> do correspond with the indices of the sub-nodes.
    /// </remarks>
    public enum IntraLocation
    {
        UPPER_LEFT,
        UPPER_RIGHT,
        LOWER_RIGHT,
        LOWER_LEFT,
        SPANNING_LEFT,
        SPANNING_RIGHT,
        SPANNING_UPPER,
        SPANNING_LOWER,
        SPANNING
    };
}
