using FunBooksAndVideos.Domain.Entities;

namespace FunBooksAndVideos.Application.Events;

public sealed record PurchaseOrderCreated(PurchaseOrder Order);
