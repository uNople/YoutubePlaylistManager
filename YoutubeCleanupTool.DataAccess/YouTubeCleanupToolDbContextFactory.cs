using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess
{
    public class YouTubeCleanupToolDbContextFactory : IYouTubeCleanupToolDbContextFactory
    {
        private readonly YouTubeServiceCreatorOptions _youTubeServiceCreatorOptions;
        private readonly IMapper _mapper;
        // Due to errors like "A second operation was started on this context before a previous operation completed"
        // We can only have a cache of this DB Context per thread, not one static instance per application
        [ThreadStatic]
        private static IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;

        public YouTubeCleanupToolDbContextFactory([NotNull] YouTubeServiceCreatorOptions youTubeServiceCreatorOptions,
            [NotNull] IMapper mapper)
        {
            _youTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
            _mapper = mapper;
        }

        public IYouTubeCleanupToolDbContext Create()
        {
            if (_youTubeCleanupToolDbContext != null)
                return _youTubeCleanupToolDbContext;

            var dbContextBuilder = new DbContextOptionsBuilder<YouTubeCleanupToolDbContext>();
            dbContextBuilder.UseSqlite(@$"Data Source={_youTubeServiceCreatorOptions.DatabasePath}");
            _youTubeCleanupToolDbContext = new YouTubeCleanupToolDbContext(dbContextBuilder.Options, _mapper);

            return _youTubeCleanupToolDbContext;
        }
    }
}
