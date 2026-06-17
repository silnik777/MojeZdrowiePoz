namespace MZ.Domain.Common;

/// <summary>
/// Wspólna baza encji: identyfikator oraz znaczniki audytowe wypełniane automatycznie
/// przez interceptor EF Core (patrz MZ.Infrastructure).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset UtworzonoUtc { get; set; }
    public string? UtworzonoPrzez { get; set; }
    public DateTimeOffset? ZmodyfikowanoUtc { get; set; }
    public string? ZmodyfikowanoPrzez { get; set; }
}
