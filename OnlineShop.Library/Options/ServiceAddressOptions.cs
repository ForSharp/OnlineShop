﻿namespace OnlineShop.Library.Options;

public class ServiceAddressOptions
{
    public const string SectionName = nameof(ServiceAddressOptions);
    public string IdentityServer { get; set; }
    public string UserManagementService { get; set; }
    public string OrdersService { get; set; }
    public string ArticlesService { get; set; }
    public string ApiService { get; set; }
}