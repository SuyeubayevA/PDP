namespace School.Contracts;

public record ActivityCreated(string Name, string Schedule, int Capacity);

public record StudentEnrolledInActivity(int ActivityId, int StudentId);