using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;
using FunBooksAndVideos.Domain.Exceptions;
using Shouldly;

namespace FunBooksAndVideos.UnitTests.Domain;

public class CustomerTests
{
    private static Customer CreateCustomer(params MembershipType[] memberships) =>
        new(1, "Customer", "customer@mail.com", memberships);

    [Theory]
    [InlineData(MembershipType.BookClub)]
    [InlineData(MembershipType.VideoClub)]
    [InlineData(MembershipType.Premium)]
    public void ActivateMembership_AddsMembership(MembershipType membership)
    {
        var customer = CreateCustomer();

        var changed = customer.ActivateMembership(membership);

        changed.ShouldBeTrue();
        customer.Memberships.ShouldBe([membership]);
    }

    [Theory]
    [InlineData(MembershipType.BookClub, MembershipType.BookClub)]
    [InlineData(MembershipType.VideoClub, MembershipType.VideoClub)]
    [InlineData(MembershipType.Premium, MembershipType.BookClub)]
    [InlineData(MembershipType.Premium, MembershipType.VideoClub)]
    [InlineData(MembershipType.Premium, MembershipType.Premium)]
    public void ActivateMembership_AlreadyOwned_ReturnsFalse(MembershipType existingMembership, MembershipType newMembership)
    {
        var customer = CreateCustomer(existingMembership);

        customer.ActivateMembership(newMembership).ShouldBeFalse();
    }

    [Fact]
    public void ActivateMembership_PremiumCoversIndividualClubs()
    {
        var customer = CreateCustomer(MembershipType.Premium);

        customer.ActivateMembership(MembershipType.BookClub).ShouldBeFalse();
        customer.ActivateMembership(MembershipType.VideoClub).ShouldBeFalse();
        customer.Memberships.ShouldBe([MembershipType.Premium]);
    }

    [Fact]
    public void ActivateMembership_BothClubs_UpgradesToPremium()
    {
        var customer = CreateCustomer(MembershipType.BookClub);

        var changed = customer.ActivateMembership(MembershipType.VideoClub);

        changed.ShouldBeTrue();
        customer.Memberships.ShouldBe([MembershipType.Premium]);
    }

    [Fact]
    public void ActivateMembership_PremiumReplacesExistingClub()
    {
        var customer = CreateCustomer(MembershipType.BookClub);

        customer.ActivateMembership(MembershipType.Premium).ShouldBeTrue();
        customer.Memberships.ShouldBe([MembershipType.Premium]);
    }

    [Theory]
    [InlineData("", "customer@mail.com")]
    [InlineData("Customer", "")]
    public void Constructor_InvalidData_Throws(string name, string email)
    {
        Should.Throw<DomainException>(() => new Customer(1, name, email));
    }
}
