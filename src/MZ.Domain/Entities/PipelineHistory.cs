using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Zapis pojedynczego przejścia statusu w pipeline (kto, kiedy, z jakiego do jakiego).</summary>
public class PipelineHistory : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public PipelineStatus? StatusZ { get; set; }
    public PipelineStatus StatusNa { get; set; }
    public DateTimeOffset DataPrzejscia { get; set; }
    public string? Komentarz { get; set; }
}
