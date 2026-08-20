using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Application.Rules;
using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;
using Moq;
using Shouldly;

namespace FunBooksAndVideos.UnitTests.Rules;

public class ShippingSlipRuleTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IShippingSlipRepository> _shippingSlipRepository = new();
    private readonly ShippingSlipRule _rule;

    public ShippingSlipRuleTests()
    {
        _rule = new ShippingSlipRule(_productRepository.Object, _shippingSlipRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_MembershipOnlyOrder_DoesNothing()
    {
        var order = new PurchaseOrder(2, 1, [LineItem.ForMembership(0, MembershipType.BookClub, 9.99m)]);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        _productRepository.Verify(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        _shippingSlipRepository.Verify(r => r.AddAsync(It.IsAny<ShippingSlip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PhysicalProduct_GeneratesSlipWithPhysicalItemsOnly()
    {
        var book = new Book(1, "Book 1", 19.23m);
        var video = new Video(3, "Video 1", 14.00m);
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([book, video]);

        var order = new PurchaseOrder(10, 1,
            [LineItem.ForProduct(0, book), LineItem.ForProduct(0, video)]);

        ShippingSlip? slip = null;
        _shippingSlipRepository
            .Setup(r => r.AddAsync(It.IsAny<ShippingSlip>(), It.IsAny<CancellationToken>()))
            .Callback<ShippingSlip, CancellationToken>((s, _) => slip = s)
            .ReturnsAsync(1);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        slip.ShouldNotBeNull();
        slip.PurchaseOrderId.ShouldBe(10);
        slip.CustomerId.ShouldBe(1);
        slip.ItemsToShip.ShouldBe(["Book 1"]);
    }

    [Fact]
    public async Task HandleAsync_VideoOnlyOrder_DoesNotGenerateSlip()
    {
        var video = new Video(3, "Video 1", 4.01m);
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([video]);

        var order = new PurchaseOrder(10, 1, [LineItem.ForProduct(0, video)]);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        _shippingSlipRepository.Verify(r => r.AddAsync(It.IsAny<ShippingSlip>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
