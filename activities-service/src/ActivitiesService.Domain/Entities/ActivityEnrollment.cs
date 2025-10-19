namespace ActivitiesService.Domain.Entities;

public enum EnrollmentStatus { Enrolled = 0, Cancelled = 1 }

public class ActivityEnrollment
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int StudentId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime EnrolledUtc { get; set; }
}