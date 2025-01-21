using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Novibet.Interfaces;
using Novibet.Models;
using Novibet.Services;
using StackExchange.Redis;
using Xunit;

public class IpInfoServiceTests
{
    private readonly Mock<IIpInfoRepository> _repositoryMock;
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _redisDatabaseMock;
    private readonly IpInfoService _service;

    public IpInfoServiceTests()
    {
        _repositoryMock = new Mock<IIpInfoRepository>();
        _redisMock = new Mock<IConnectionMultiplexer>();
        _redisDatabaseMock = new Mock<IDatabase>();
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDatabaseMock.Object);

        _service = new IpInfoService(_repositoryMock.Object, new HttpClient(), _redisMock.Object);
    }

    [Fact]
    public async Task GetIpInfoAsync_ShouldReturnDataFromCache_WhenAvailable()
    {
        // Arrange
        var ip = "192.168.1.1";
        var cacheKey = $"ipinfo:{ip}";
        var cachedData = new IpInfoResponse { CountryName = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" };

        _redisDatabaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(JsonConvert.SerializeObject(cachedData));

        // Act
        var result = await _service.GetIpInfoAsync(ip);

        // Assert
        result.Should().NotBeNull();
        result.CountryName.Should().Be("United States");
        result.TwoLetterCode.Should().Be("US");
    }

    [Fact]
    public async Task GetIpInfoAsync_ShouldReturnUnknownCountry_WhenNotFoundInCacheOrDatabase()
    {
        // Arrange
        var ip = "10.0.0.1";
        _repositoryMock.Setup(r => r.GetIpInfoFromDbAsync(ip)).ReturnsAsync((IPAddress?)null);
        _repositoryMock.Setup(r => r.GetCountryByCodeAsync("XX"))
            .ReturnsAsync(new Country
            {
                Id = -1,
                Name = "Unknown Country",
                TwoLetterCode = "XX",
                ThreeLetterCode = "XXX",
                CreatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.GetIpInfoAsync(ip);

        // Assert
        result.Should().NotBeNull();
        result.CountryName.Should().Be("Unknown Country");
        result.TwoLetterCode.Should().Be("XX");
        result.ThreeLetterCode.Should().Be("XXX");
    }

}
