using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogBank.Api.Controllers;
[Authorize]
[ApiController]
[Route("api/test")]
public class TestController: ControllerBase
{
    
}