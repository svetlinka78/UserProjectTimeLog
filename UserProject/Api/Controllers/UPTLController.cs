using System;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using static UserProject.UserProjectTimeLog;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UPTLController : ControllerBase
    {
        private IBusinessLayer _bl;

        public UPTLController(IBusinessLayer bl)
        {
            _bl = bl;
        }

  
        [HttpPost]
        public ActionResult Post([FromBody] Page page)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            try
            {

                var uptl = _bl.GetData(page.PageNumber, page.PageRows, page.CheckCount);
                    return Ok(uptl);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }

        }

    }
}
