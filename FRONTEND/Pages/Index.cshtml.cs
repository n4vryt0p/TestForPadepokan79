﻿using System.Text.Json;
using DevExtreme.AspNet.Data;
using FRONTEND;
using FRONTEND.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontEnd.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly IPadepokanManage _manageEngineApi;
    private readonly IJsonOptions _jOpt;

    public IndexModel(IPadepokanManage manageEngineApi, IJsonOptions jOpt)
    {
        _manageEngineApi = manageEngineApi;
        _jOpt = jOpt;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetReadAsync(DataSourceLoadOptionsBase set)
    {
        var response = await _manageEngineApi.Ready().AccountListAsync();

        return new JsonResult(DataSourceLoader.Load(response, set));
    }

    public async Task OnPostCreateAsync(IFormCollection collection)
    {
        string? grup = collection["values"].FirstOrDefault();
        if (!string.IsNullOrEmpty(grup))
        {
            var rdto = JsonSerializer.Deserialize<UserDto>(grup, _jOpt.JOpts());

            await _manageEngineApi.Ready().AddAccountAsync(rdto);
        }
    }

    public async Task<IActionResult?> OnPutEditAsync(IFormCollection collection)
    {
        string? grupId = collection["key"].FirstOrDefault();
        string? grup = collection["values"].FirstOrDefault();
        if (grup == null || string.IsNullOrEmpty(grupId))
        {
            return BadRequest("Data Error");
        }

        var rdto = JsonSerializer.Deserialize<UserDto>(grup, _jOpt.JOpts());
        rdto!.Id = Convert.ToInt32(grupId);

        await _manageEngineApi.Ready().EditAccountAsync(rdto);
        return null;
    }

    public async Task<IActionResult?> OnDeleteDeleteAsync(IFormCollection collection)
    {
        try
        {
            string? grupId = collection["key"].FirstOrDefault();
            if (grupId == null)
            {
                return BadRequest("Data Error");
            }

            await _manageEngineApi.Ready().DeleteAccountAsync(Convert.ToInt32(grupId));
            return null;
        }
        catch
        {
            return BadRequest("Server Error");
        }
    }
}