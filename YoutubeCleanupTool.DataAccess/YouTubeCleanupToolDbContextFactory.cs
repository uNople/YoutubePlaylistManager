using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess
{
    public class YouTubeCleanupToolDbContextFactory : IYouTubeCleanupToolDbContextFactory
    {
        private readonly YouTubeServiceCreatorOptions _youTubeServiceCreatorOptions;
        private readonly IMapper _mapper;
        private IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;

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

            var dbContextBuilder = new DbContextOptionsBuilder<YoutubeCleanupToolDbContext>();
            dbContextBuilder.UseSqlite(@$"Data Source={_youTubeServiceCreatorOptions.DatabasePath}");
            _youTubeCleanupToolDbContext = new YoutubeCleanupToolDbContext(dbContextBuilder.Options, _mapper);

            return _youTubeCleanupToolDbContext;
        }
    }
}
