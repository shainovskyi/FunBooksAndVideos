namespace FunBooksAndVideos.Domain.Entities;

public class Video(int id, string name, decimal price) : Product(id, name, price)
{
    public override bool IsPhysical => false;
}
