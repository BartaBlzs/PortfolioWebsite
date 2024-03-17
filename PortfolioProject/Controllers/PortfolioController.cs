using Course.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Diagnostics;

namespace Course.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(ILogger<PortfolioController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Roadmap()
        {
            return View();
        }

		public IActionResult Projects(string id)
		{
            if (id != null) return View("project", GetContent(id));
            else return View();
		}

        private string GetContent(string id)
        {
            MySqlConnection connection = new("Server=MYSQL6008.site4now.net;Database=db_a9f44b_barta;Uid=a9f44b_barta;Pwd=4700216001Sh");
            connection.Open();
            MySqlCommand cmd = new("SELECT content FROM projects WHERE id = @sid", connection);
            cmd.Parameters.AddWithValue("sid", id);
            string s = cmd.ExecuteScalar().ToString();
            connection.Close();
            return s;
        }

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
