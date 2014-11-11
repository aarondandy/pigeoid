using DotSpatial.Data;
using Pigeoid;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vertesaur;
using Vertesaur.Transformation;

namespace SimpleRenderSample.Controllers
{
    public class HomeController : Controller
    {

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
    }
}