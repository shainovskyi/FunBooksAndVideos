using FunBooksAndVideos.Application.Abstractions;
using FunBooksAndVideos.Application.Dtos;
using FunBooksAndVideos.Application.Events;
using FunBooksAndVideos.Application.Exceptions;
using FunBooksAndVideos.Application.Services;
using FunBooksAndVideos.Domain.Entities;
using FunBooksAndVideos.Domain.Enums;
using Moq;
using Shouldly;

namespace FunBooksAndVideos.UnitTests.Services;

public class PurchaseOrderProcessorTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepository = new();
    private readonly Mock<IMembershipRepository> _membershipRepository = new();
    private readonly Mock<IDomainEventDispatcher> _eventDispatcher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public PurchaseOrderProcessorTests()
    {
        _membershipRepository
            .Setup(r => r.GetPricesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<MembershipType, decimal>
            {
                [MembershipType.BookClub] = 12.99m,
                [MembershipType.VideoClub] = 8.01m,
                [MembershipType.Premium] = 15.50m
            });
    }

    private PurchaseOrderProcessor CreateProcessor() =>
        new(_customerRepository.Object, _productRepository.Object, _purchaseOrderRepository.Object,
            _membershipRepository.Object, _eventDispatcher.Object, _unitOfWork.Object);

    private void SetupCustomer(int id = 1) =>
        _customerRepository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Customer(id, "Customer", "customer@mail.com"));

    private void SetupProducts(params Product[] products) =>
        _productRepository
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

    [Fact]
    public async Task ProcessAsync_ValidOrder_PersistsAndPublishesEventInTransaction()
    {
        SetupCustomer();
        SetupProducts(new Book(1, "Book 1", 19.99m));
        _purchaseOrderRepository
            .Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder o, CancellationToken _) => new PurchaseOrder(42, o.CustomerId, o.LineItems, o.CreatedAtUtc));

        var request = new CreatePurchaseOrderRequest(1,
            [new CreateLineItemRequest(1, null), new CreateLineItemRequest(null, MembershipType.BookClub)]);

        var response = await CreateProcessor().ProcessAsync(request);

        response.Id.ShouldBe(42);
        response.TotalPrice.ShouldBe(19.99m + 12.99m);
        _unitOfWork.Verify(u => u.BeginAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcher.Verify(d => d.PublishAsync(
            It.Is<PurchaseOrderCreated>(e => e.Order.Id == 42), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_EventHandlerFails_RollsBackTransaction()
    {
        SetupCustomer();
        SetupProducts(new Book(1, "Book 1", 5.00m));
        _purchaseOrderRepository
            .Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder o, CancellationToken _) => new PurchaseOrder(1, o.CustomerId, o.LineItems, o.CreatedAtUtc));
        _eventDispatcher.Setup(d => d.PublishAsync(It.IsAny<PurchaseOrderCreated>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("rule failed"));

        await Should.ThrowAsync<InvalidOperationException>(() =>
            CreateProcessor().ProcessAsync(new CreatePurchaseOrderRequest(1, [new CreateLineItemRequest(1, null)])));

        _unitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_UnknownCustomer_ThrowsNotFound()
    {
        _customerRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        await Should.ThrowAsync<NotFoundException>(() =>
            CreateProcessor().ProcessAsync(new CreatePurchaseOrderRequest(99, [new CreateLineItemRequest(1, null)])));
    }

    [Fact]
    public async Task ProcessAsync_UnknownProduct_ThrowsNotFound()
    {
        SetupCustomer();
        SetupProducts(); // no products

        await Should.ThrowAsync<NotFoundException>(() =>
            CreateProcessor().ProcessAsync(new CreatePurchaseOrderRequest(1, [new CreateLineItemRequest(123, null)])));
    }

    [Fact]
    public async Task ProcessAsync_EmptyOrder_ThrowsValidation()
    {
        await Should.ThrowAsync<ValidationException>(() =>
            CreateProcessor().ProcessAsync(new CreatePurchaseOrderRequest(1, [])));
    }

    [Fact]
    public async Task ProcessAsync_LineWithBothProductAndMembership_ThrowsValidation()
    {
        SetupCustomer();
        SetupProducts(new Book(1, "Book 1", 5.00m));

        await Should.ThrowAsync<ValidationException>(() =>
            CreateProcessor().ProcessAsync(
                new CreatePurchaseOrderRequest(1, [new CreateLineItemRequest(1, MembershipType.BookClub)])));
    }

    [Fact]
    public async Task ProcessAsync_EmptyLine_ThrowsValidation()
    {
        SetupCustomer();
        SetupProducts();

        await Should.ThrowAsync<ValidationException>(() =>
            CreateProcessor().ProcessAsync(new CreatePurchaseOrderRequest(1, [new CreateLineItemRequest(null, null)])));
    }
}
