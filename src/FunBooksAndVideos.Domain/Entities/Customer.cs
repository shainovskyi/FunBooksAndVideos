using FunBooksAndVideos.Domain.Enums;
using FunBooksAndVideos.Domain.Exceptions;

namespace FunBooksAndVideos.Domain.Entities;

public class Customer
{
    private readonly HashSet<MembershipType> _memberships;

    public int Id { get; }
    public string Name { get; }
    public string Email { get; }
    public IReadOnlyCollection<MembershipType> Memberships => _memberships;

    public Customer(int id, string name, string email, IEnumerable<MembershipType>? memberships = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Customer email is required.");

        Id = id;
        Name = name;
        Email = email;
        _memberships = memberships is null ? [] : [.. memberships];
    }

    public bool HasMembership(MembershipType type) =>
        _memberships.Contains(type) || _memberships.Contains(MembershipType.Premium);

    public bool ActivateMembership(MembershipType type)
    {
        if (HasMembership(type))
            return false;

        if (type == MembershipType.Premium)
        {
            UpgradeToPremium();
            return true;
        }

        _memberships.Add(type);

        // Owning both individual clubs upgrades to Premium.
        if (_memberships.Contains(MembershipType.BookClub) && _memberships.Contains(MembershipType.VideoClub))
        {
            UpgradeToPremium();
        }

        return true;
    }

    private void UpgradeToPremium()
    {
        _memberships.Remove(MembershipType.BookClub);
        _memberships.Remove(MembershipType.VideoClub);
        _memberships.Add(MembershipType.Premium);
    }
}
