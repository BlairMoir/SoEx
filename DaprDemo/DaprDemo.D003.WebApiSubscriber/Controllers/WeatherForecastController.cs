using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dapr;


namespace DaprDemo.D003.WebApiSubscriber.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [Topic("pubsub", "exampletopic")]
        public WeatherForecast Get([FromBody]WeatherForecast value)
        {
            _logger.LogInformation("Subscription Invoked");
            _logger.LogInformation(value.Summary);   
            return value;
        }
    }
}
