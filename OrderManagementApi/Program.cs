using FluentValidation;
using FluentValidation.AspNetCore;
using OrderManagementApi.Middleware;
using OrderManagementApi.PipelineExtensions;
using OrderManagementApi.Validations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Adicionar Swagger com configuração JWT
builder.Services.AddSwaggerWithJwt();

// Configurar autenticação JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar serviços de autenticação
builder.Services.AddAuthServices();

// Register validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AddProductToOrderRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom exception handling middleware
app.UseExceptionHandling();

app.UseHttpsRedirection();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
