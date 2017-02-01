namespace Typify.Test
{
    using Typify.NET;

    [Typify(typeof(Entity))]
    public class EntityConsumer
    {
        public Entity Get()
        {
            return new Entity();
        }
    }
}
