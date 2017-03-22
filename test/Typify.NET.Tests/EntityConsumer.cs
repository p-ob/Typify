namespace Typify.NET.Tests
{
    [Typify(typeof(Entity))]
    public class EntityConsumer
    {
        public Entity Get()
        {
            return new Entity();
        }
    }
}
