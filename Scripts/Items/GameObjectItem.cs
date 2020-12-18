
namespace Quadtree.Items
{
    public abstract class GameObjectItem : GameObjectItemBase<GameObjectItem, Node<GameObjectItem>>
    {
        protected override GameObjectItem This()
        {
            return this;
        }
    }
}
