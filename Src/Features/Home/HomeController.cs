﻿using System.Threading.Tasks;
using Bolt.RequestBus;
using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Web.Features.Home
{
    public class HomeController : Controller
    {
        private readonly IRequestBus bus;

        public HomeController(IRequestBus bus)
        {
            this.bus = bus;
        }

        public async Task<IActionResult> Index()
        {
            await bus.PublishAsync(new HomePageRequestedEvent());

            return View();
        } 
    }
}
