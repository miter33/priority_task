namespace InterviewApi.Application.Common.Dtos;

public class CreateVisitationRequest
{
    public int CustomerId { get; set; }
    public int HotelId { get; set; }
    public DateTime VisitDate { get; set; }
}
