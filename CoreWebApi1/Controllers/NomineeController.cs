using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CoreWebApi1.Controllers
{
    [Route("api/[controller]")]
    public class NomineeController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get(string id)
        {
            return NoContent();
        }
    }
}