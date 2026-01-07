using Microsoft.AspNetCore.Mvc;
using sinergyit.ServiceA.API.ApiHelper;

namespace sinergyit.ServiceA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly RabbitMQLogger _rabbitLogger;

        public TestController(ILogger<TestController> logger, RabbitMQLogger rabbitLogger)
        {
            _logger = logger;
            _rabbitLogger = rabbitLogger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var message = "ServiceA test endpoint çağrıldı";

            _logger.LogInformation(message);
            _rabbitLogger.LogInformation(message);

            _logger.LogWarning("Bu bir test warning logu");
            _rabbitLogger.LogWarning("Bu bir test warning logu");

            return Ok(new { message = "service a", timestamp = DateTime.Now });
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] string data)
        {
            var message = $"ServiceA Veri oluştu: {data}";

            _logger.LogInformation(message);
            _rabbitLogger.LogInformation(message);

            return Ok(new { success = true, data = data });
        }
    }
}