using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreCodeCamp.Data;
using AutoMapper;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator )
        {
            _repository = repository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetCamps(bool includeTalks)
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);

                if (results == null) return NotFound();

                CampModel[] models = _mapper.Map<CampModel[]>(results);
                return models;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");            
            } 
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var result =  await _repository.GetCampAsync(moniker);

                if (result == null) return NotFound();

                CampModel model = _mapper.Map<CampModel>(result);
                return model;
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

       
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                //Get Uri of current model to return after success
                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use curent moniker");
                }

                var camp =  _mapper.Map<Camp>(model);
                _repository.Add(camp);

                if(await _repository.SaveChangesAsync())
                {
                    //update campModel in case new model contains additional fields 
                    return Created($"/api/camps/{camp.Moniker}", _mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }
    }
}
