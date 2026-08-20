using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;
using FunBooksAndVideos.Domain.Exceptions;
using Shouldly;

namespace FunBooksAndVideos.UnitTests.Domain;

public class PurchaseOrderTests
{
    [Fact]
    public void Constructor_ComputesTotalFromLineItems()
    {
        var order = new PurchaseOrder(1, 1,
        [
            LineItem.ForProduct(0, new Book(1, "Book 1", 27.13m)),
            LineItem.ForProduct(0, new Video(3, "Video 1", 63.02m)),
            LineItem.ForMembership(0, MembershipType.BookClub, 70.00m)
        ]);

        order.TotalPrice.ShouldBe(27.13m + 63.02m + 70.00m);
    }

    [Fact]
    public void Constructor_EmptyLineItems_Throws()
    {
        Should.Throw<DomainException>(() => new PurchaseOrder(1, 1, []));
    }

    [Fact]
    public void ContainsMemberships_TrueOnlyWithMembershipLines()
    {
        var productOnly = new PurchaseOrder(1, 1, [LineItem.ForProduct(0, new Book(1, "Book 1", 4.00m))]);
        var withMembership = new PurchaseOrder(2, 1, [LineItem.ForMembership(0, MembershipType.VideoClub, 6.00m)]);

        productOnly.ContainsMemberships.ShouldBeFalse();
        withMembership.ContainsMemberships.ShouldBeTrue();
    }

    [Fact]
    public void GetMemberships_ReturnsDistinctMembershipTypes()
    {
        var order = new PurchaseOrder(1, 1,
        [
            LineItem.ForMembership(0, MembershipType.BookClub, 90.37m),
            LineItem.ForMembership(0, MembershipType.BookClub, 90.37m),
            LineItem.ForProduct(0, new Book(1, "Book 1", 50.00m))
        ]);

        order.GetMemberships().ShouldBe([MembershipType.BookClub]);
        order.GetProductLines().Count.ShouldBe(1);
    }
}
