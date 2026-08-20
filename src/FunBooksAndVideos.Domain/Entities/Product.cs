using FunBooksAndVideos.Domain.Exceptions;

namespace FunBooksAndVideos.Domain.Entities;

public abstract class Product
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public abstract bool IsPhysical { get; }

    protected Product(int id, string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (price < 0)
            throw new DomainException("Product price cannot be negative.");

        Id = id;
        Name = name;
        Price = price;
    }
}
