using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Rules;
using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;
using Moq;
using Shouldly;

namespace FunBooksAndVideos.UnitTests.Rules;

public class MembershipActivationRuleTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly MembershipActivationRule _rule;

    public MembershipActivationRuleTests()
    {
        _rule = new MembershipActivationRule(_customerRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_OrderWithoutMemberships_DoesNothing()
    {
        var order = new PurchaseOrder(2, 1, [LineItem.ForProduct(0, new Book(1, "Book 1", 5.00m))]);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        _customerRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _customerRepository.Verify(r => r.UpdateMembershipsAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ActivatesMembershipAndPersists()
    {
        var customer = new Customer(1, "Customer", "customer@mail.com");
        _customerRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var order = new PurchaseOrder(1, 1, [LineItem.ForMembership(0, MembershipType.BookClub, 8.50m)]);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        customer.Memberships.ShouldBe([MembershipType.BookClub]);
        _customerRepository.Verify(r => r.UpdateMembershipsAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(MembershipType.BookClub, MembershipType.VideoClub)]
    [InlineData(MembershipType.VideoClub, MembershipType.BookClub)]
    public async Task HandleAsync_UpgradeMembershipToPremium(MembershipType existingMembership, MembershipType newMembership)
    {
        var customer = new Customer(1, "Customer", "customer@mail.com", [existingMembership]);
        _customerRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var order = new PurchaseOrder(1, 1, [LineItem.ForMembership(0, newMembership, 18.99m)]);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        customer.Memberships.ShouldBe([MembershipType.Premium]);
        _customerRepository.Verify(r => r.UpdateMembershipsAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MembershipAlreadyOwned_DoesNotPersist()
    {
        var customer = new Customer(1, "Customer", "customer@mail.com", [MembershipType.Premium]);
        _customerRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var order = new PurchaseOrder(1, 1, [LineItem.ForMembership(0, MembershipType.BookClub, 259.01m)]);

        await _rule.HandleAsync(new PurchaseOrderCreated(order));

        _customerRepository.Verify(r => r.UpdateMembershipsAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NotExistingCustomer_ShouldThrowNotFoundException()
    {
        _customerRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);
        var order = new PurchaseOrder(1, 1, [LineItem.ForMembership(0, MembershipType.BookClub, 75.75m)]);

        await Should.ThrowAsync<NotFoundException>(async () => await _rule.HandleAsync(new PurchaseOrderCreated(order)));
    }
}
