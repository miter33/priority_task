namespace InterviewApi.Application.Common.Dtos;

public class VisitationView
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public bool IsLoyal { get; set; }
}
