using Microsoft.EntityFrameworkCore;
using Monolith.Web.Data;
using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class WorkspaceStore(IDbContextFactory<MonolithDbContext> dbFactory, TemplateCatalog catalog)
{
    public IReadOnlyList<ProductionJob> GetJobs()
    {
        using var db = dbFactory.CreateDbContext();
        return db.Jobs.AsNoTracking()
            .OrderByDescending(job => job.UpdatedAtUtc)
            .Select(job => new ProductionJob(job.Id, job.ProjectId, job.TemplateSlug, job.TemplateName,
                job.Status, job.Progress, job.CreatedAtUtc, job.UpdatedAtUtc))
            .ToArray();
    }

    public IReadOnlyList<Project> GetProjects()
    {
        using var db = dbFactory.CreateDbContext();
        return db.Projects.AsNoTracking().OrderByDescending(project => project.CreatedAtUtc)
            .Select(project => new Project(project.Id, project.Name, project.Slug, project.TemplateSlug,
                project.Description, project.CreatedAtUtc)).ToArray();
    }

    public Project? GetProject(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        return db.Projects.AsNoTracking().Where(project => project.Id == id)
            .Select(project => new Project(project.Id, project.Name, project.Slug, project.TemplateSlug,
                project.Description, project.CreatedAtUtc)).FirstOrDefault();
    }

    public IReadOnlyList<ProductionJob> GetProjectJobs(Guid projectId)
    {
        using var db = dbFactory.CreateDbContext();
        return db.Jobs.AsNoTracking().Where(job => job.ProjectId == projectId)
            .OrderByDescending(job => job.UpdatedAtUtc)
            .Select(job => new ProductionJob(job.Id, job.ProjectId, job.TemplateSlug, job.TemplateName,
                job.Status, job.Progress, job.CreatedAtUtc, job.UpdatedAtUtc)).ToArray();
    }

    public Project? CreateProject(string name, string templateSlug, string description)
    {
        var template = catalog.GetBySlug(templateSlug);
        if (template is null || string.IsNullOrWhiteSpace(name))
            return null;

        using var db = dbFactory.CreateDbContext();
        var project = new ProjectEntity
        {
            Id = Guid.NewGuid(), Name = name.Trim(), Slug = $"{name.Trim().ToLowerInvariant().Replace(' ', '-')}-{Guid.NewGuid():N}"[..Math.Min(48, name.Trim().Length + 33)],
            TemplateSlug = template.Slug, Description = description?.Trim() ?? string.Empty, CreatedAtUtc = DateTime.UtcNow
        };
        db.Projects.Add(project);
        db.SaveChanges();
        return new Project(project.Id, project.Name, project.Slug, project.TemplateSlug, project.Description, project.CreatedAtUtc);
    }

    public IReadOnlyList<ProjectLog> GetLogs(Guid jobId)
    {
        using var db = dbFactory.CreateDbContext();
        return db.JobLogs.AsNoTracking().Where(log => log.JobId == jobId).OrderBy(log => log.CreatedAtUtc)
            .Select(log => new ProjectLog(log.Id, log.JobId, log.Message, log.CreatedAtUtc)).ToArray();
    }

    public ProductionJob? GetJob(Guid id) => GetJobs().FirstOrDefault(job => job.Id == id);

    public ProductionJob? CreateJob(string templateSlug, Guid? projectId = null)
    {
        var template = catalog.GetBySlug(templateSlug);
        if (template is null)
            return null;

        using var db = dbFactory.CreateDbContext();
        var now = DateTime.UtcNow;
        var job = new JobEntity
        {
            Id = Guid.NewGuid(), ProjectId = projectId, TemplateSlug = template.Slug, TemplateName = template.Name,
            Status = "Queued", Progress = 0, CreatedAtUtc = now, UpdatedAtUtc = now
        };
        db.Jobs.Add(job);
        db.JobLogs.Add(new JobLogEntity { Id = Guid.NewGuid(), JobId = job.Id, Message = "Job created and queued", CreatedAtUtc = now });
        db.SaveChanges();
        return ToModel(job);
    }

    public ProductionJob? AdvanceJob(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        var job = db.Jobs.Find(id);
        if (job is null)
            return null;

        job.Progress = Math.Min(job.Progress + 20, 100);
        job.Status = job.Progress == 100 ? "Ready for review" : "Running pipeline";
        job.UpdatedAtUtc = DateTime.UtcNow;
        db.JobLogs.Add(new JobLogEntity { Id = Guid.NewGuid(), JobId = job.Id, Message = $"Pipeline advanced to {job.Progress}%", CreatedAtUtc = job.UpdatedAtUtc });
        db.SaveChanges();
        return ToModel(job);
    }

    public ProductionJob? ApproveJob(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        var job = db.Jobs.Find(id);
        if (job is null)
            return null;

        job.Status = "Approved for publishing";
        job.Progress = 100;
        job.UpdatedAtUtc = DateTime.UtcNow;
        db.JobLogs.Add(new JobLogEntity { Id = Guid.NewGuid(), JobId = job.Id, Message = "Publishing approval recorded", CreatedAtUtc = job.UpdatedAtUtc });
        db.SaveChanges();
        return ToModel(job);
    }

    public ProductionJob? CancelJob(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        var job = db.Jobs.Find(id);
        if (job is null)
            return null;
        job.Status = "Cancelled";
        job.UpdatedAtUtc = DateTime.UtcNow;
        db.JobLogs.Add(new JobLogEntity { Id = Guid.NewGuid(), JobId = job.Id, Message = "Job cancelled", CreatedAtUtc = job.UpdatedAtUtc });
        db.SaveChanges();
        return ToModel(job);
    }

    public ProductionJob? RetryJob(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        var job = db.Jobs.Find(id);
        if (job is null)
            return null;
        job.Status = "Queued";
        job.Progress = 0;
        job.UpdatedAtUtc = DateTime.UtcNow;
        db.JobLogs.Add(new JobLogEntity { Id = Guid.NewGuid(), JobId = job.Id, Message = "Job queued for retry", CreatedAtUtc = job.UpdatedAtUtc });
        db.SaveChanges();
        return ToModel(job);
    }

    private static ProductionJob ToModel(JobEntity job) =>
        new(job.Id, job.ProjectId, job.TemplateSlug, job.TemplateName, job.Status, job.Progress,
            job.CreatedAtUtc, job.UpdatedAtUtc);
}
