using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using TTForABP.Models;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;


namespace TTForABP.Controllers
{


    [ApiController]
    [Produces("application/json")]
    [Route("api/experiments")]
    public class ExperimentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly DbManager _dbManager;
        private readonly AppDbContext _dbContext;

        public ExperimentController(IConfiguration configuration, DbManager dbManager, AppDbContext dbContext)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _dbManager = dbManager;
            _dbContext = dbContext;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateExperiment([FromBody] ExperimentDto experimentDto)
        {
            try
            {
                var experiment = new Experiment
                {
                    DeviceToken = experimentDto.Key,
                    Key = experimentDto.Key,
                    Options = experimentDto.Options
                };

                _dbContext.Experiments.Add(experiment);
                await _dbContext.SaveChangesAsync();

                return Ok("Experiment created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("experiment/{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetExperimentValue([FromQuery] string deviceToken, string key)
        {
            try
            {
                var experiment = _dbContext.Experiments
                    .OrderBy(e => Guid.NewGuid()) 
                    .FirstOrDefault(e => e.Key == key);

                switch (key)
                {
                    case ("button-color"):
                        async Task<IActionResult> GetButtonColorExperiment(string deviceToken)
                        {
                            try
                            {
                                var experiment = await _dbContext.Experiments
                                    .Where(e => e.DeviceToken == deviceToken && e.Key == "button_color")
                                    .FirstOrDefaultAsync();

                                if (experiment == null)
                                {
                                    var random = new Random();
                                    var colors = new[] { "#FF0000", "#00FF00", "#0000FF" };
                                    var randomColor = colors[random.Next(colors.Length)];

                                    experiment = new Experiment
                                    {
                                        DeviceToken = deviceToken,
                                        Key = "button_color",
                                        Options = randomColor
                                    };

                                    _dbContext.Experiments.Add(experiment);
                                    await _dbContext.SaveChangesAsync();
                                }

                                return Ok(new { key = experiment.Key, value = experiment.Options });
                            }
                            catch (Exception ex)
                            {
                                return BadRequest($"Error: {ex.Message}");
                            }
                        }

                        break;

                    case ("price"):
                         async Task<IActionResult> GetPriceExperiment(string deviceToken)
                        {
                            try
                            {
                                // Отримайте з бази даних значення для заданого deviceToken
                                var experiment = await _dbContext.Experiments
                                    .Where(e => e.DeviceToken == deviceToken && e.Key == "price")
                                    .FirstOrDefaultAsync();

                                // Якщо експеримент не існує, створіть новий з випадковим значенням
                                if (experiment == null)
                                {
                                    var random = new Random();
                                    var prices = new[] { 10, 20, 50, 5 };
                                    var randomPrice = prices[random.Next(prices.Length)];

                                    experiment = new Experiment
                                    {
                                        DeviceToken = deviceToken,
                                        Key = "price",
                                        Options = randomPrice.ToString()
                                    };

                                    _dbContext.Experiments.Add(experiment);
                                    await _dbContext.SaveChangesAsync();
                                }

                                // Поверніть значення експерименту
                                return Ok(new { key = experiment.Key, value = experiment.Options });
                            }
                            catch (Exception ex)
                            {
                                return BadRequest($"Error: {ex.Message}");
                            }
                        }
                        break;
                }


                if (experiment != null)
                {
                    return Ok(new { Key = key, Value = experiment.Options });
                }
                else
                {
                    return NotFound("Experiment not found for the given Key.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetStatistics()
        {
            try
            {
                var statistics = _dbContext.Experiments
                    .GroupBy(e => e.Key)
                    .Select(group => new StatisticsDto
                    {
                        Key = group.Key,
                        TotalDevices = _dbContext.Experiments.Count(de => de.Experiment.Key == group.Key),
                        OptionsDistribution = group.ToDictionary(e => e.Options, e => e.Distribution)
                    })
                    .ToList();

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        //public async Task<IActionResult> GetAllExperiments()
        //{
        //    try
        //    {
        //        var experiments = await _dbContext.Experiments.ToListAsync();
        //        return Ok(experiments);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}
        [HttpGet("statistics/{key}")]
        public async Task<IActionResult> GetExperimentStatistics(string key)
        {
            try
            {
                var statistics = await _dbContext.Experiments
                    .Where(e => e.Key == key)
                    .GroupBy(e => e.Options)
                    .Select(g => new
                    {
                        Option = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var totalDevices = await _dbContext.Experiments
                    .Select(e => e.DeviceToken)
                    .Distinct()
                    .CountAsync();

                return Ok(new
                {
                    Experiments = statistics,
                    TotalDevices = totalDevices
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("all-statistics")]
        public async Task<IActionResult> GetAllExperimentStatistics()
        {
            try
            {
                var allStatistics = await _dbContext.Experiments
                    .GroupBy(e => new { e.Key, e.Options })
                    .Select(g => new
                    {
                        Key = g.Key.Key,
                        Option = g.Key.Options,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var totalDevices = await _dbContext.Experiments
                    .Select(e => e.DeviceToken)
                    .Distinct()
                    .CountAsync();

                return Ok(new
                {
                    AllExperiments = allStatistics,
                    TotalDevices = totalDevices
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }

    public class ExperimentDto
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string Options { get; set; }
        public string Distribution { get; set; }
    }
}
