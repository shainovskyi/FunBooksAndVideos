namespace FunBooksAndVideos.Application.Dtos;

public record CustomerDto(int Id, string Name, string Email, IReadOnlyList<string> Memberships);
