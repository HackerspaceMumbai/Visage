using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var registrations = new List<Registrant>();

app.MapPost("/register", async ([FromBody] Registrant registrant) =>
{
    registrations.Add(registrant);
    return Results.Created($"/register/{registrant.Email}", registrant);
})
.WithName("Register")
.WithOpenApi();

app.Run();

public class Registrant
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string GovtId { get; set; } = string.Empty;
    public string GovtIdLast4Digits { get; set; } = string.Empty;
    public string OccupationStatus { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string LinkedInProfile { get; set; } = string.Empty;
    public string GitHubProfile { get; set; } = string.Empty;
    public string EducationalInstituteName { get; set; } = string.Empty;
    public string GenderIdentity { get; set; } = string.Empty;
    public string SelfDescribeGender { get; set; } = string.Empty;
    public string AgeRange { get; set; } = string.Empty;
    public string Ethnicity { get; set; } = string.Empty;
    public string SelfDescribeEthnicity { get; set; } = string.Empty;
    public string LanguageProficiency { get; set; } = string.Empty;
    public string SelfDescribeLanguage { get; set; } = string.Empty;
    public string EducationalBackground { get; set; } = string.Empty;
    public string SelfDescribeEducation { get; set; } = string.Empty;
    public string Disability { get; set; } = string.Empty;
    public string DisabilityDetails { get; set; } = string.Empty;
    public string DietaryRequirements { get; set; } = string.Empty;
    public string SelfDescribeDietary { get; set; } = string.Empty;
    public string LgbtqIdentity { get; set; } = string.Empty;
    public string ParentalStatus { get; set; } = string.Empty;
    public string FirstTimeAttendee { get; set; } = string.Empty;
    public string HowDidYouHear { get; set; } = string.Empty;
    public string SelfDescribeHowDidYouHear { get; set; } = string.Empty;
    public string AreasOfInterest { get; set; } = string.Empty;
    public string SelfDescribeAreasOfInterest { get; set; } = string.Empty;
    public string VolunteerOpportunities { get; set; } = string.Empty;
    public string AdditionalSupport { get; set; } = string.Empty;
}
