using AventasApi.Filters;
using DBData.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
//using AventasApi.GestorData;

//using DBData.Database;
using AventasApi.Models.ViewModels;

public class AsesorController : ApiController
{
    AVentasEntities context = new AVentasEntities();


    [HttpGet]
    public async Task<IHttpActionResult> GetAsesores()
    {
        return Ok(context.Asesores.Select(ase => new AsesorViewModel { CodigoAsesor = ase.CodigoAsesor, Nombre = ase.Nombre }).ToList());
    }
}
public class AsesorViewModel
{
    public string CodigoAsesor { get; set; }
    public string Nombre { get; set; }
}

