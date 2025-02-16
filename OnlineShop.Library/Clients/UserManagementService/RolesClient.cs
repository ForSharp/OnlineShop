﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OnlineShop.Library.Constants;
using OnlineShop.Library.Options;
using OnlineShop.Library.UserManagementService.Responses;

namespace OnlineShop.Library.Clients.UserManagementService;

public class RolesClient : UserManagementBaseClient, IRolesClient
{
    public RolesClient(HttpClient httpClient, IOptions<ServiceAddressOptions> options) : base(httpClient, options) { }

    public async Task<IdentityResult> Add(IdentityRole role) 
        => await SendPostRequest(role, $"/{RolesControllerRoutes.ControllerName}/{RepoActions.Add}");

    public async Task<UserManagementServiceResponse<IdentityRole>> Get(string name) 
        => await SendGetRequest<IdentityRole>($"{RolesControllerRoutes.ControllerName}?name={name}");

    public async Task<UserManagementServiceResponse<IEnumerable<IdentityRole>>> GetAll() 
        => await SendGetRequest<IEnumerable<IdentityRole>>($"/{RolesControllerRoutes.ControllerName}/{RepoActions.GetAll}");

    public async Task<IdentityResult> Remove(IdentityRole role) 
        => await SendPostRequest(role, $"/{RolesControllerRoutes.ControllerName}/{RepoActions.Remove}");

    public async Task<IdentityResult> Update(IdentityRole role) 
        => await SendPostRequest(role, $"/{RolesControllerRoutes.ControllerName}/{RepoActions.Update}");
}