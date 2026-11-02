using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Scans, validates and builds the form models. Throws if anything is invalid.</summary>
internal static class FormModelFactory
{
    /// <summary>Builds the registry from the form attributes on the API's entity types.</summary>
    public static FormModelRegistry Create(OptionProviderRegistry options) =>
        Create(LoadModel(), options, typeof(AppDbContext).Assembly.GetTypes());

    public static FormModelRegistry Create(
        IModel model,
        OptionProviderRegistry options,
        IEnumerable<Type> types
    )
    {
        var (entities, problems) = FormModelScanner.Scan(model, types);
        List<string> allProblems = [.. problems, .. FormModelValidator.Validate(entities, options)];
        if (allProblems.Count > 0)
        {
            throw new FormModelException(allProblems);
        }

        return new FormModelRegistry(FormModelBuilder.Build(entities, options));
    }

    /// <summary>
    /// The EF model, built without a database connection so the form model never depends on
    /// connection settings. The design-time model keeps the annotations (such as column defaults)
    /// that the startup checks read.
    /// </summary>
    public static IModel LoadModel()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql()
            .UseSnakeCaseNamingConvention()
            .Options;
        using AppDbContext dbContext = new(options);
        return dbContext.GetService<IDesignTimeModel>().Model;
    }
}
