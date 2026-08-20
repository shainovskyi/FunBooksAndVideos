namespace FunBooksAndVideos.Domain.Entities;

public class Book(int id, string name, decimal price) : Product(id, name, price)
{
    public override bool IsPhysical => true;
}
