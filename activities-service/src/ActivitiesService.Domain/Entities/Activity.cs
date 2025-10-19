namespace ActivitiesService.Domain.Entities;

public class Activity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Schedule { get; set; } = default!;
    public int Capacity { get; set; } = 20;
    public bool IsActive { get; set; } = true;
}