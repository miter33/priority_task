using InterviewApi.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InterviewApi.Api.Controllers;

/// <summary>
/// Returns the original interview brief — kept for backwards compatibility with the Welcome page.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssignmentController : ControllerBase
{
    /// <summary>Get the interview assignment text.</summary>
    /// <response code="200">Static metadata about the assignment.</response>
    [HttpGet]
    [ProducesResponseType(typeof(InterviewAssignment), StatusCodes.Status200OK)]
    public ActionResult<InterviewAssignment> GetAssignment()
    {
        var assignment = new InterviewAssignment
        {
            Title = "Hotel Visitation Management System",
            Description = @"# Hotel Visitation Management System - Technical Assignment

## Overview
You are tasked with developing a comprehensive Hotel Visitation Management System that allows customers to register visits to various hotels and enables administrators to track customer loyalty patterns.

## Business Context
A hotel chain wants to implement a system to manage customer visits across their properties. The system should help identify loyal customers who regularly visit specific hotels and provide insights into customer behavior patterns.

## System Requirements

### Frontend Application (React)
Develop a single-page application with the following features:

**1. Customer Profile Management**
Create a dynamic customer profile page that adapts based on the URL:
- When accessed with a customer ID (/profile/123), display the customer's information and provide a ""Register Visit"" button
- When accessed without an ID (/profile), show a customer registration form with a ""Create Profile"" button
- The page should handle both viewing existing customer data and creating new customer profiles

**2. Visitation Analytics Dashboard**
Build a comprehensive dashboard for viewing customer visitations:
- Implement a search panel with:
  - Multi-select dropdown for hotel selection
  - Month/Year picker in MM/yyyy format (e.g., 01/2025, 02/2025)
  - ""Only Loyal Customers"" checkbox filter
  - Search submission button
- Create a data grid displaying:
  - Sequential index
  - Customer name (clickable for navigation)
  - Visit date
  - Hotel name
- Enable navigation to customer profiles by clicking on customer names
- Populate grid data through backend API calls

**3. Visit Registration Modal**
Design a modal dialog for registering new visits:
- Hotel selection dropdown
- Date picker for visit scheduling
- Submit functionality that communicates with the backend

### Backend API (.NET Core 8)
Implement a RESTful API with the following endpoints:

**1. Customer Management**
- Create new customer profiles
- Retrieve customer information by ID
- Utilize the provided customers.json file as the data source

**2. Visit Registration System**
- Maintain a list of visitations in application memory
- Load initial data from the provided visitations.json file
- Accept new visit registrations with fields: hotelId, customerId, visitDate
- Add new visit records to the in-memory collection

**3. Loyalty Analytics**
- Implement an API endpoint that returns customer loyalty data
- Define loyal customers as those who visit the same hotel on the same day of the week for an entire month
- Example: A customer visiting ""Grand Hotel"" every Sunday throughout October
- Support filtering to show only loyal customers when requested

## Data Resources
The following JSON files are provided as your data foundation:
- customers.json: Contains 2 sample customer records
- hotels.json: Contains 5 hotel properties with details
- visitations.json: Contains 12 sample visitation records demonstrating various patterns

## Technical Expectations
- Follow industry-standard coding practices and design patterns
- Implement comprehensive error handling and input validation
- Maintain clean, readable, and well-structured code
- Ensure proper separation of concerns between frontend and backend
- Create a public Git repository to showcase your work

## Success Criteria
Your solution should demonstrate:
- Proficiency in full-stack development
- Understanding of RESTful API design
- Ability to work with JSON data sources
- Implementation of complex business logic (loyalty calculations)
- Creation of an intuitive user interface
- Proper handling of data relationships and navigation

## Time Allocation
You have 1.5 hours to complete this assignment. Focus on delivering a working solution that demonstrates your technical capabilities and problem-solving approach.

Good luck with your implementation!",
            Duration = "1.5 h",
            Contact = "interview@priority-software.com"
        };

        return Ok(assignment);
    }
}
