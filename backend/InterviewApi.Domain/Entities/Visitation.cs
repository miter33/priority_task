namespace InterviewApi.Domain.Entities;

public class Visitation
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int HotelId { get; set; }
    public DateTime VisitDate { get; set; }
}
