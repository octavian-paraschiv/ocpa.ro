namespace ocpa.ro.domain.Abstractions.Services;

public interface IHostingEnvironmentService
{
    string ContentRootPath { get; }

    string ContentPath { get; }
}