using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Dtos;
using InterviewApi.Application.Visitations.Commands.CreateVisitation;
using InterviewApi.Application.Visitations.Queries.SearchVisitations;
using InterviewApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InterviewApi.Api.Controllers;

/// <summary>
/// Visitations — search the activity grid and register new visits.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VisitationController : ControllerBase
{
    private readonly ISender _sender;

    public VisitationController(ISender sender) => _sender = sender;

    /// <summary>
    /// Search visitations by hotel, month/year, and loyalty.
    /// </summary>
    /// <remarks>
    /// A customer is considered <em>loyal</em> when they visit the same hotel on every occurrence
    /// of a particular weekday within the month (e.g. every Sunday at Grand Hotel in January).
    /// <para>Pass <c>hotelIds</c> multiple times to filter by several hotels: <c>?hotelIds=1&amp;hotelIds=2</c>.</para>
    /// </remarks>
    /// <param name="month">Month (1-12). Optional, but if supplied with <c>year</c> the loyalty
    /// calculation is scoped to that bucket.</param>
    /// <param name="year">Year (1900-2999).</param>
    /// <param name="hotelIds">Optional list of hotel ids to include.</param>
    /// <param name="onlyLoyal">When true, only loyal-pattern rows are returned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Visitation rows matching the filters.</response>
    /// <response code="400">A query parameter is out of range.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VisitationView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<VisitationView>>> Search(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] List<int>? hotelIds,
        [FromQuery] bool onlyLoyal,
        CancellationToken ct)
    {
        if (month is < 1 or > 12) return BadRequest(new { message = "month must be 1-12" });
        if (year is < 1900 or > 2999) return BadRequest(new { message = "year must be a 4-digit year" });

        var query = new SearchVisitationsQuery(month, year, hotelIds, onlyLoyal);
        var rows = await _sender.Send(query, ct);
        return Ok(rows);
    }

    /// <summary>Register a new visit.</summary>
    /// <param name="request">Customer, hotel, and visit date.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Visit was recorded. Body contains the persisted entity (with assigned id).</response>
    /// <response code="400">Validation failed (missing customer/hotel, invalid date, or unknown id).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Visitation), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Visitation>> Register(
        [FromBody] CreateVisitationRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest(new { message = "Request body is required" });

        var command = new CreateVisitationCommand(request.CustomerId, request.HotelId, request.VisitDate);
        var visit = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(Search), new { }, visit);
    }
}
