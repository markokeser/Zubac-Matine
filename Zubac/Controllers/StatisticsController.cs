using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Zubac.Data;
using Zubac.Interfaces;
using Zubac.Models;

public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStatisticService _service;

    public StatisticsController(ApplicationDbContext context, IStatisticService service)
    {
        _context = context;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _service.GetStatistic();

        return View(response);
    }
}