using InterviewApi.Application.Common.Cqrs;
using InterviewApi.Application.Common.Dtos;
using InterviewApi.Application.Customers.Commands.CreateCustomer;
using InterviewApi.Application.Customers.Queries.GetCustomerById;
using InterviewApi.Application.Customers.Queries.ListCustomers;
using InterviewApi.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InterviewApi.Api.Controllers;

/// <summary>
/// Customer management — create profiles, fetch by id, list all.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomerController : ControllerBase
{
    private readonly ISender _sender;

    public CustomerController(ISender sender) => _sender = sender;

    /// <summary>List every customer.</summary>
    /// <response code="200">Returns the full collection of customers.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Customer>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Customer>>> GetAll(CancellationToken ct)
    {
        var customers = await _sender.Send(new ListCustomersQuery(), ct);
        return Ok(customers);
    }

    /// <summary>Get a single customer by id.</summary>
    /// <param name="id">Customer identifier.</param>
    /// <response code="200">The customer was found.</response>
    /// <response code="404">No customer with the supplied id exists.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Customer>> GetCustomer(int id, CancellationToken ct)
    {
        var customer = await _sender.Send(new GetCustomerByIdQuery(id), ct);
        return Ok(customer);
    }

    /// <summary>Create a new customer profile.</summary>
    /// <param name="request">Customer name + email.</param>
    /// <response code="201">Customer was created. The response body contains the persisted entity.</response>
    /// <response code="400">Validation failed (missing fields or invalid email).</response>
    [HttpPost]
    [ProducesResponseType(typeof(Customer), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Customer>> AddCustomer(
        [FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest(new { message = "Request body is required" });

        var command = new CreateCustomerCommand(request.Name, request.Email);
        var customer = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }
}
