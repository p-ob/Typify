namespace Typify.NET.Tests.Library
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
