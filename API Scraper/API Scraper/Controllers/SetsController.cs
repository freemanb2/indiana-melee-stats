using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API_Scraper.Models;
using System.Collections.Generic;

namespace API_Scraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetsController : ControllerBase
    {
        private readonly SetConsumer _consumer;
        public SetsController(SetConsumer consumer)
        {
            _consumer = consumer;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var results = await _consumer.GetRecentIndianaTournamentResults();

            List<Tournament> tournamentList = new List<Tournament>();
            
            foreach(var result in results)
            {
                tournamentList.Add(new Tournament(result));
            }

            return Ok(tournamentList);
        }
}
}
