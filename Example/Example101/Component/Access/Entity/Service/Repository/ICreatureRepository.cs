namespace Example101.Access.Entity.Service.Repository
{
    public interface ICreatureRepository
    {
        IEnumerable<T> Load<T>()  where T : new();
    }
}