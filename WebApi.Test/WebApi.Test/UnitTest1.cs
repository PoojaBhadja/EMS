using Application.Contracts;
using Application.Services;
using AutoMapper;
using Castle.Core.Logging;
using Domain;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace WebApi.Test
{
    public class UnitTest1
    {
        [Fact]
        public void PassIRepositoryAsNull_ThrowArgumentException()
        {
            IRepository? repository = null;
            IMapper? mapper = Substitute.For<IMapper>();
            ILogger<CardHolderService>? logger = Substitute.For<ILogger<CardHolderService>>();
            ICacheService? _cacheService = Substitute.For<ICacheService>();

            Assert.Throws<ArgumentException>(() =>
            {
                CardHolderService accountDetailService = new(repository, mapper, logger, _cacheService);
            });
        }

        [Fact]
        public void PassIMapperAsNull_ThrowArgumentException()
        {
            IRepository? repository = Substitute.For<IRepository>();
            IMapper? mapper = null;
            ILogger<CardHolderService>? logger = Substitute.For<ILogger<CardHolderService>>();
            ICacheService? _cacheService = Substitute.For<ICacheService>();

            Assert.Throws<ArgumentException>(() =>
            {
                CardHolderService accountDetailService = new(repository, mapper, logger, _cacheService);
            });
        }

        [Fact]
        public void PassILoggerAsNull_ThrowArgumentException()
        {
            IRepository? repository = Substitute.For<IRepository>();
            IMapper? mapper = Substitute.For<IMapper>();
            ILogger<CardHolderService>? logger = null;
            ICacheService? _cacheService = Substitute.For<ICacheService>();

            Assert.Throws<ArgumentException>(() =>
            {
                CardHolderService accountDetailService = new(repository, mapper, logger, _cacheService);
            });
        }
    }
}