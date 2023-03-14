using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.LocalSubs;

/// <summary>
/// Single template string model.
/// </summary>
public class TemplateModel
{
    /// <summary>
    /// Gets or sets a template string.
    /// </summary>
    [Required]
    public string Template { get; set; } = string.Empty;
}

/// <summary>
/// Multiple template strings model.
/// </summary>
public class TemplatesModel
{
    /// <summary>
    /// Gets or sets a template string.
    /// </summary>
    public IEnumerable<string> Templates { get; set; } = new List<string>();
}

/// <summary>
/// Controller for configuration page.
/// </summary>
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[Authorize(Policy = "DefaultAuthorization")]
public class LocalSubsController : ControllerBase
{
    /// <summary>
    /// Add template string.
    /// </summary>
    /// <response code="200">Template string valid.</response>
    /// <response code="400">Template string is missing or invalid.</response>
    /// <param name="body">The request body.</param>
    /// <returns>
    /// An <see cref="NoContentResult"/> if the login info is valid, a <see cref="BadRequestResult"/> if the request body missing is data
    /// or <see cref="UnauthorizedResult"/> if the login info is not valid.
    /// </returns>
    [HttpPost("Jellyfin.Plugin.LocalSubs/AddTemplate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AddTemplate([FromBody] TemplateModel body)
    {
        if (string.IsNullOrEmpty(body.Template))
        {
            return BadRequest();
        }
        else
        {
            LocalSubsPlugin.Instance!.Configuration.AddTemplate(body.Template);
            return Ok();
        }
    }

    /// <summary>
    /// Remove template strings.
    /// </summary>
    /// <response code="200">Template string valid.</response>
    /// <response code="400">Template string is missing or invalid.</response>
    /// <param name="body">The request body.</param>
    /// <returns>
    /// An <see cref="NoContentResult"/> if the login info is valid, a <see cref="BadRequestResult"/> if the request body missing is data
    /// or <see cref="UnauthorizedResult"/> if the login info is not valid.
    /// </returns>
    [HttpPost("Jellyfin.Plugin.LocalSubs/DeleteTemplates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteTemplates([FromBody] TemplatesModel body)
    {
        if (body.Templates == null)
        {
            return BadRequest();
        }
        else
        {
            foreach (string template in body.Templates)
            {
                if (!string.IsNullOrEmpty(template))
                {
                    LocalSubsPlugin.Instance!.Configuration.RemoveTemplate(template);
                }
            }

            LocalSubsPlugin.Instance!.UpdateConfiguration(LocalSubsPlugin.Instance!.Configuration);
            return Ok();
        }
    }

    /// <summary>
    /// Reset template strings to defaults.
    /// </summary>
    /// <response code="200">Reset successful.</response>
    /// <returns>
    /// An <see cref="NoContentResult"/> if the login info is valid, a <see cref="BadRequestResult"/> if the request body missing is data
    /// or <see cref="UnauthorizedResult"/> if the login info is not valid.
    /// </returns>
    [HttpGet("Jellyfin.Plugin.LocalSubs/GetTemplates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult GetTemplates()
    {
        return Ok(new TemplatesModel
        {
            Templates = new List<string>(LocalSubsPlugin.Instance!.Configuration.Templates)
        });
    }

    /// <summary>
    /// Reset template strings to defaults.
    /// </summary>
    /// <response code="200">Reset successful.</response>
    /// <returns>
    /// An <see cref="NoContentResult"/> if the login info is valid, a <see cref="BadRequestResult"/> if the request body missing is data
    /// or <see cref="UnauthorizedResult"/> if the login info is not valid.
    /// </returns>
    [HttpPost("Jellyfin.Plugin.LocalSubs/ResetTemplates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult ResetTemplates()
    {
        LocalSubsPlugin.Instance!.Configuration.ResetTemplates();
        LocalSubsPlugin.Instance!.UpdateConfiguration(LocalSubsPlugin.Instance!.Configuration);
        return Ok();
    }
}
