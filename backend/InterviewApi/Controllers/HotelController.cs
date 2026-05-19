using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Hotels.Queries.GetHotelById;
using InterviewApi.Application.Hotels.Queries.ListHotels;
using InterviewApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InterviewApi.Api.Controllers;

/// <summary>
/// Hotel catalog — read-only list of properties available for visit registration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HotelController : ControllerBase
{
    private readonly ISender _sender;

    public HotelController(ISender sender) => _sender = sender;

    /// <summary>List every hotel.</summary>
    /// <response code="200">Returns the full hotel catalog.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Hotel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Hotel>>> GetAll(CancellationToken ct)
    {
        var hotels = await _sender.Send(new ListHotelsQuery(), ct);
        return Ok(hotels);
    }

    /// <summary>Get a single hotel by id.</summary>
    /// <param name="id">Hotel identifier.</param>
    /// <response code="200">The hotel was found.</response>
    /// <response code="404">No hotel with the supplied id exists.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Hotel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Hotel>> GetHotel(int id, CancellationToken ct)
    {
        var hotel = await _sender.Send(new GetHotelByIdQuery(id), ct);
        return Ok(hotel);
    }
}
