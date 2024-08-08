using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess;

public class YouTubeCleanupToolDbContextFactory(
    [NotNull] YouTubeServiceCreatorOptions youTubeServiceCreatorOptions,
    [NotNull] IMapper mapper)
    : IYouTubeCleanupToolDbContextFactory
{
    // Due to errors like "A second operation was started on this context before a previous operation completed"
    // We can only have a cache of this DB Context per thread, not one static instance per application
    [ThreadStatic]
    private static IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;

    public IYouTubeCleanupToolDbContext Create()
    {
            if (_youTubeCleanupToolDbContext != null)
                return _youTubeCleanupToolDbContext;

            var dbContextBuilder = new DbContextOptionsBuilder<YouTubeCleanupToolDbContext>();
            dbContextBuilder.UseSqlite(@$"Data Source={youTubeServiceCreatorOptions.DatabasePath}");
            _youTubeCleanupToolDbContext = new YouTubeCleanupToolDbContext(dbContextBuilder.Options, mapper);

            return _youTubeCleanupToolDbContext;
        }
}